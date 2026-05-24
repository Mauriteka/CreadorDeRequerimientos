import { api } from "../core/api.js";
import { $, applyThemeVariables, escapeHtml, formatTime, getParticipantTheme } from "../core/dom.js";
import { state } from "../core/state.js";
import { getAppHandlers } from "../core/app-context.js";

export function renderSurveyTemplateSelect() {
    const select = $("#surveyTemplateSelect");
    if (!select) {
        return;
    }
    const currentValue = select.value;
    select.innerHTML = "";

    const emptyOption = document.createElement("option");
    emptyOption.value = "";
    emptyOption.textContent = "Sin plantilla";
    select.appendChild(emptyOption);

    state.systemTemplates.forEach(template => {
        const option = document.createElement("option");
        option.value = `system:${template.id}`;
        option.textContent = `Sistema - ${template.name}`;
        select.appendChild(option);
    });

    if (state.currentProject) {
        state.currentProject.projectTemplates.forEach(template => {
            const option = document.createElement("option");
            option.value = `project:${template.id}`;
            option.textContent = `Proyecto - ${template.name}`;
            select.appendChild(option);
        });
    }

    if ([...select.options].some(option => option.value === currentValue)) {
        select.value = currentValue;
    }
}

function setSpeechStatus(message) {
    const node = $("#speechStatus");
    if (!node) {
        return;
    }

    node.textContent = message;
}

export async function createSurvey() {
    if (!state.currentProject) {
        return;
    }

    const number = state.currentProject.surveys.length + 1;
    state.currentProject = await api.createSurvey(state.currentProject.id, {
        title: `Encuesta ${number}`,
        interviewee: "",
        objective: "",
        templateId: null,
        templateScope: null,
        ownerEmail: "",
        intervieweeEmail: "",
        extraEmails: "",
        minuteDraft: "",
        isFinalized: false
    });
    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    getAppHandlers().renderApp();
}

async function updateSurvey(survey, card) {
    state.currentProject = await api.updateSurvey(state.currentProject.id, survey.id, buildSurveyUpdateRequest(survey, card));
    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    getAppHandlers().renderApp();
}

async function deleteSurvey(survey) {
    if (!confirm("Eliminar esta encuesta?")) {
        return;
    }

    state.currentProject = await api.deleteSurvey(state.currentProject.id, survey.id);
    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    getAppHandlers().renderApp();
}

export function renderSurveys() {
    const list = $("#surveyList");
    const template = $("#surveyTemplate");
    list.innerHTML = "";

    const surveys = state.currentProject?.surveys ?? [];
    if (surveys.length === 0) {
        const empty = document.createElement("div");
        empty.className = "applied-template-card";
        empty.innerHTML = "<p>Aun no hay encuestas en este proyecto.</p>";
        list.appendChild(empty);
        state.reviewFocusSurveyId = null;
        return;
    }

    surveys.forEach(survey => {
        const card = template.content.firstElementChild.cloneNode(true);
        card.dataset.surveyId = survey.id;
        card.classList.toggle("finalized", Boolean(survey.isFinalized));
        card.querySelector(".survey-title").textContent = survey.title;
        card.querySelector(".survey-meta").textContent = `${getSurveyParticipants(survey).length} participantes${survey.isFinalized ? " - finalizada" : ""}`;
        card.querySelector(".survey-title-input").value = survey.title;
        card.querySelector(".survey-objective-input").value = survey.objective;
        const deleteButton = card.querySelector(".delete-survey-summary");
        if (deleteButton) {
            deleteButton.addEventListener("click", event => {
                event.preventDefault();
                event.stopPropagation();
                deleteSurvey(survey);
            });
        }
        card.querySelector(".add-guest-button").addEventListener("click", () => addGuestParticipant(survey));
        card.querySelector(".copy-conversation-button").addEventListener("click", async event => {
            event.preventDefault();
            event.stopPropagation();
            await copyTextToClipboard(survey.conversationCopy);
        });
        bindSaveOnEnter(card.querySelector(".survey-title-input"), () => updateSurvey(survey, card));
        bindSaveOnEnter(card.querySelector(".survey-objective-input"), () => updateSurvey(survey, card));
        bindAutoSaveOnBlur(card.querySelector(".survey-title-input"), () => updateSurvey(survey, card));
        bindAutoSaveOnBlur(card.querySelector(".survey-objective-input"), () => updateSurvey(survey, card));

        list.appendChild(card);
        renderSurveySectionSafely(survey, card, "Usuarios", () => renderParticipants(survey, card), ".participants");
        renderSurveySectionSafely(survey, card, "Plantilla aplicada", () => renderAppliedTemplate(survey, card), ".applied-template");
        renderSurveySectionSafely(survey, card, "Guia de entrevista", () => renderInterviewGuide(survey, card), ".interview-guide");
        renderSurveySectionSafely(survey, card, "Transcript", () => renderTranscriptTurns(survey, card), ".transcript-list");
        renderSurveySectionSafely(survey, card, "Validacion y minuta", () => renderSurveyReview(survey, card), ".survey-review");

        if (state.reviewFocusSurveyId === survey.id) {
            requestAnimationFrame(() => {
                card.querySelector(".review-minute-text")?.focus();
                card.querySelector(".review-box")?.scrollIntoView({ block: "nearest", behavior: "smooth" });
            });
        }
    });

    state.reviewFocusSurveyId = null;
}

function renderSurveySectionSafely(survey, card, sectionName, renderAction, targetSelector) {
    try {
        renderAction();
    } catch (error) {
        console.error(`No se pudo renderizar ${sectionName}`, survey.id, error);
        const node = card.querySelector(targetSelector);
        if (!node) {
            return;
        }

        node.innerHTML = `
            <div class="applied-template-card">
                <p>No se pudo abrir ${escapeHtml(sectionName)}.</p>
                <small>Encuesta: ${escapeHtml(survey.title || "Sin titulo")}</small>
                <small class="render-error-detail">${escapeHtml(error?.message || "Error desconocido")}</small>
            </div>
        `;
    }
}

function renderAppliedTemplate(survey, card) {
    const container = card.querySelector(".applied-template");
    container.innerHTML = "";
    const appliedTemplate = survey.appliedTemplate ?? null;
    const interviewSections = getInterviewSections(survey);

    if (!appliedTemplate) {
        const templateCard = document.createElement("div");
        templateCard.className = "applied-template-card";
        templateCard.innerHTML = "<p>Encuesta sin plantilla.</p>";

        if (canAssignTemplateToSurvey(survey)) {
            const controls = document.createElement("div");
            controls.className = "inline-template-picker";

            const select = document.createElement("select");
            populateSurveyTemplateOptions(select);

            const applyButton = document.createElement("button");
            applyButton.type = "button";
            applyButton.className = "primary";
            applyButton.textContent = "Aplicar plantilla";
            applyButton.addEventListener("click", async () => {
                await applyTemplateToSurvey(survey, card, select.value);
            });

            controls.appendChild(select);
            controls.appendChild(applyButton);
            templateCard.appendChild(controls);
        }

        container.appendChild(templateCard);
        return;
    }

    const templateCard = document.createElement("div");
    templateCard.className = "applied-template-card";
    templateCard.innerHTML = `
        <h4>${escapeHtml(appliedTemplate.name || "Plantilla sin nombre")}</h4>
        <p>${escapeHtml(appliedTemplate.description || "Sin descripcion")}</p>
    `;

    if (interviewSections.length > 0) {
        const list = document.createElement("ul");
        interviewSections.forEach(section => {
            const item = document.createElement("li");
            const questions = Array.isArray(section.questions) ? section.questions : [];
            item.textContent = `${section.title || "Seccion"} - ${questions.length} preguntas`;
            list.appendChild(item);
        });
        templateCard.appendChild(list);
    }

    container.appendChild(templateCard);
}

function renderInterviewGuide(survey, card) {
    const container = card.querySelector(".interview-guide");
    container.innerHTML = "";
    const interviewSections = getInterviewSections(survey);

    if (!survey.appliedTemplate || interviewSections.length === 0) {
        container.innerHTML = '<div class="applied-template-card"><p>Selecciona una plantilla con preguntas para usar la guia durante la entrevista.</p></div>';
        return;
    }

    const guideState = ensureGuideState(survey);
    const captureState = getCaptureState(survey, card);
    const currentSection = interviewSections[guideState.sectionIndex] ?? interviewSections[0];
    const currentQuestions = Array.isArray(currentSection?.questions) ? currentSection.questions : [];
    const currentQuestion = currentQuestions[guideState.questionIndex] ?? "Sin preguntas en esta seccion.";

    const guideCard = document.createElement("div");
    guideCard.className = "guide-card";
    applyThemeVariables(guideCard, getParticipantTheme(survey, captureState.speakerId));

    const progress = document.createElement("div");
    progress.className = "guide-progress";
    progress.innerHTML = `
        <div>
            <strong>${escapeHtml(currentSection.title)}</strong>
            <small>Pregunta ${guideState.questionIndex + 1} de ${currentQuestions.length} en esta seccion</small>
        </div>
        <small>${escapeHtml(buildGuideProgressText(survey, guideState))}</small>
    `;

    const questionCard = document.createElement("div");
    questionCard.className = "guide-question-card";
    questionCard.innerHTML = `
        <h4>Pregunta actual</h4>
        <p>${escapeHtml(currentQuestion)}</p>
    `;

    if (currentSection.prompt) {
        const prompt = document.createElement("small");
        prompt.textContent = currentSection.prompt;
        questionCard.appendChild(prompt);
    }

    const nav = document.createElement("div");
    nav.className = "guide-nav";

    const previousButton = document.createElement("button");
    previousButton.type = "button";
    previousButton.textContent = "Pregunta anterior";
    previousButton.disabled = !hasPreviousGuideQuestion(survey, guideState);
    previousButton.addEventListener("click", () => moveGuideQuestion(survey, -1));

    const nextButton = document.createElement("button");
    nextButton.type = "button";
    nextButton.className = "primary";
    const nextAction = getNextQuestionAction(survey, guideState);
    if (nextAction === "request-response") {
        nextButton.textContent = "Responder";
        nextButton.className = "attention";
        nextButton.addEventListener("click", () => requestIntervieweeResponse(survey));
    } else if (hasNextGuideQuestion(survey, guideState)) {
        nextButton.textContent = "Siguiente pregunta";
        nextButton.addEventListener("click", () => moveGuideQuestion(survey, 1));
    } else {
        nextButton.textContent = "Finaliza";
        nextButton.addEventListener("click", () => finalizeSurveyInterview(survey, card));
    }

    nav.appendChild(previousButton);
    nav.appendChild(nextButton);

    const sections = document.createElement("div");
    sections.className = "guide-sections";

    interviewSections.forEach((section, sectionIndex) => {
        const sectionNode = document.createElement("div");
        sectionNode.className = `guide-section-card${sectionIndex === guideState.sectionIndex ? " active" : ""}`;

        const title = document.createElement("h4");
        title.textContent = section.title;
        sectionNode.appendChild(title);

        if (section.prompt) {
            const prompt = document.createElement("p");
            prompt.textContent = section.prompt;
            sectionNode.appendChild(prompt);
        }

        const questionList = document.createElement("div");
        questionList.className = "question-list";

        const questions = Array.isArray(section.questions) ? section.questions : [];
        questions.forEach((question, questionIndex) => {
            const questionKey = `${sectionIndex}:${questionIndex}`;
            const progressState = getQuestionProgressState(survey, questionKey);
            const button = document.createElement("button");
            button.type = "button";
            button.className = [
                "question-list-item",
                sectionIndex === guideState.sectionIndex && questionIndex === guideState.questionIndex ? "active" : "",
                progressState.hasAsked ? "asked" : "",
                progressState.hasResponse ? "answered" : ""
            ].filter(Boolean).join(" ");
            button.innerHTML = `
                <strong>Pregunta ${questionIndex + 1}</strong>
                <small>${escapeHtml(question)}</small>
                <span class="question-status-summary">${escapeHtml(buildQuestionStatusLabel(progressState))}</span>
            `;
            button.addEventListener("click", () => setGuideQuestion(survey.id, sectionIndex, questionIndex));
            questionList.appendChild(button);
        });

        sectionNode.appendChild(questionList);
        sections.appendChild(sectionNode);
    });

    guideCard.appendChild(progress);
    guideCard.appendChild(questionCard);
    guideCard.appendChild(nav);
    guideCard.appendChild(renderCapturePanel(survey, card, captureState));
    guideCard.appendChild(sections);
    container.appendChild(guideCard);
}

function renderCapturePanel(survey, card, captureState) {
    const panel = document.createElement("div");
    panel.className = "guide-capture-card";
    applyThemeVariables(panel, getParticipantTheme(survey, captureState.speakerId));
    const participants = getSurveyParticipants(survey);

    const statusRow = document.createElement("div");
    statusRow.className = "capture-status-row";
    statusRow.innerHTML = `
        <div>
            <strong>Captura del turno actual</strong>
            <small>${escapeHtml(resolveCaptureHint(captureState))}</small>
        </div>
    `;

    const status = document.createElement("span");
    status.className = `capture-status ${captureState.processing ? "processing" : captureState.status}`;
    status.textContent = resolveCaptureStatusLabel(captureState);
    statusRow.appendChild(status);

    if (captureState.processing) {
        const activity = document.createElement("span");
        activity.className = "capture-activity";
        activity.innerHTML = "<span></span><span></span><span></span>";
        statusRow.appendChild(activity);
    }

    const controls = document.createElement("div");
    controls.className = "capture-controls capture-controls-inline";
    const participantsNode = document.createElement("div");
    participantsNode.className = "capture-participants";
    participants
        .slice()
        .sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0))
        .forEach(participant => {
            participantsNode.appendChild(renderParticipantChip(survey, participant, captureState));
        });
    controls.appendChild(participantsNode);

    const startStopButton = createCaptureButton(
        captureState.status === "idle" ? "Iniciar" : "Detener",
        "primary",
        async () => {
            if (captureState.status === "idle") {
                await startCapture(survey, card, captureState.speakerId);
                return;
            }

            stopCapture("stop");
        });
    controls.appendChild(startStopButton);

    const memory = document.createElement("div");
    memory.className = "capture-question-memory";
    memory.innerHTML = `
        <div class="question-memory-title">
            <strong>Bloque de trabajo</strong>
            <small>${escapeHtml(captureState.questionLabel || "Sin pregunta activa")}</small>
        </div>
        <small>Todo lo que digas aqui se acumula para esta pregunta y este participante.</small>
    `;

    const liveText = document.createElement("textarea");
    liveText.className = `capture-live-text${captureState.processing ? " processing" : ""}`;
    liveText.placeholder = "Aqui veras y podras corregir la transcripcion del turno actual.";
    liveText.value = captureState.text;
    liveText.addEventListener("input", async event => {
        await updateActiveCaptureDraft(survey.id, card, { text: event.target.value });
    });

    panel.appendChild(statusRow);
    panel.appendChild(controls);
    panel.appendChild(memory);
    panel.appendChild(liveText);
    return panel;
}

function renderParticipantChip(survey, participant, captureState) {
    const wrapper = document.createElement("div");
    wrapper.className = `participant-chip${participant.id === captureState.speakerId ? " active" : ""}`;
    const activateButton = document.createElement("button");
    activateButton.type = "button";
    activateButton.className = participant.id === captureState.speakerId ? "primary" : "";
    activateButton.textContent = getDisplayParticipantName(participant);
    applyThemeVariables(wrapper, getParticipantTheme(survey, participant.id));
    activateButton.addEventListener("click", async () => {
        await selectCaptureSpeaker(survey, participant.id);
    });
    wrapper.appendChild(activateButton);
    return wrapper;
}

function createCaptureButton(text, className, onClick) {
    const button = document.createElement("button");
    button.type = "button";
    button.textContent = text;
    button.className = className;
    button.addEventListener("click", onClick);
    return button;
}

function renderParticipants(survey, card) {
    const container = card.querySelector(".participants");
    container.innerHTML = "";
    const participants = getSurveyParticipants(survey);

    participants
        .slice()
        .sort((a, b) => a.sortOrder - b.sortOrder)
        .forEach(participant => {
            const row = document.createElement("div");
            row.className = "participant-row";
            const actions = document.createElement("div");
            actions.className = "participant-actions";

            if (participant.roleType === "Self") {
                const name = document.createElement("div");
                name.className = "participant-name-static";
                name.textContent = "Entrevistador";
                const email = document.createElement("input");
                email.className = "participant-email";
                email.value = participant.email || survey.ownerEmail || "";
                email.placeholder = "Tu correo";
                const saveButton = document.createElement("button");
                saveButton.type = "button";
                saveButton.textContent = "Guardar";
                const saveSelf = async () => {
                    await renameParticipant(survey, participant, "Yo", email.value);
                    await saveSurveyUsers(survey, card);
                };
                saveButton.addEventListener("click", saveSelf);
                bindSaveOnEnter(email, saveSelf);
                bindAutoSaveOnBlur(email, saveSelf);
                actions.appendChild(saveButton);
                row.appendChild(name);
                row.appendChild(email);
                row.appendChild(actions);
                container.appendChild(row);
                return;
            }

            const input = document.createElement("input");
            input.value = participant.displayName;
            input.placeholder = "Nombre del participante";
            const email = document.createElement("input");
            email.className = "participant-email";
            email.value = participant.email || "";
            email.placeholder = "Correo del participante";

            const saveButton = document.createElement("button");
            saveButton.type = "button";
            saveButton.textContent = "Guardar";
            const saveParticipant = () => renameParticipant(survey, participant, input.value, email.value);
            saveButton.addEventListener("click", saveParticipant);
            bindSaveOnEnter(input, saveParticipant);
            bindSaveOnEnter(email, saveParticipant);
            bindAutoSaveOnBlur(input, saveParticipant);
            bindAutoSaveOnBlur(email, saveParticipant);

            const deleteButton = document.createElement("button");
            deleteButton.type = "button";
            deleteButton.className = "danger";
            deleteButton.textContent = "Eliminar";
            deleteButton.disabled = getSurveyTurns(survey).some(turn => turn.speakerId === participant.id);
            deleteButton.addEventListener("click", () => deleteParticipant(survey, participant));

            actions.appendChild(saveButton);
            actions.appendChild(deleteButton);
            row.appendChild(input);
            row.appendChild(email);
            row.appendChild(actions);
            container.appendChild(row);
        });

    const extraRow = document.createElement("div");
    extraRow.className = "participant-row participant-row-extra";
    const extraLabel = document.createElement("div");
    extraLabel.className = "participant-name-static";
    extraLabel.textContent = "Correos extra";
    const extraEmails = document.createElement("input");
    extraEmails.className = "survey-extra-emails";
    extraEmails.value = survey.extraEmails || "";
    extraEmails.placeholder = "Correos adicionales separados por coma";
    const extraActions = document.createElement("div");
    extraActions.className = "participant-actions";
    const extraSaveButton = document.createElement("button");
    extraSaveButton.type = "button";
    extraSaveButton.textContent = "Guardar";
    const saveExtraEmails = async () => {
        await saveSurveyUsers(survey, card);
    };
    extraSaveButton.addEventListener("click", saveExtraEmails);
    bindSaveOnEnter(extraEmails, saveExtraEmails);
    bindAutoSaveOnBlur(extraEmails, saveExtraEmails);
    extraActions.appendChild(extraSaveButton);
    extraRow.appendChild(extraLabel);
    extraRow.appendChild(extraEmails);
    extraRow.appendChild(extraActions);
    container.appendChild(extraRow);
}

function renderTranscriptTurns(survey, card) {
    const list = card.querySelector(".transcript-list");
    list.innerHTML = "";

    const groups = groupTurnsByQuestion(survey);
    groups.forEach(group => {
        const section = document.createElement("section");
        section.className = "transcript-question-group";
        const header = document.createElement("div");
        header.className = "transcript-question-header";
        header.innerHTML = `<h4>${escapeHtml(resolveQuestionLabel(survey, group.questionKey, group.questionLabel))}</h4><small>${group.turns.length} intervenciones</small>`;
        section.appendChild(header);

        group.turns.forEach(turn => {
            const row = document.createElement("article");
            row.className = "turn-row";
            applyThemeVariables(row, getParticipantTheme(survey, turn.speakerId));

            const sourceBadge = document.createElement("span");
            sourceBadge.className = "source-badge";
            sourceBadge.textContent = `${formatTime(turn.createdAt)} - ${normalizeSpeakerName(turn.speakerName)} - ${turn.sourceType}`;

            const meta = document.createElement("div");
            meta.className = "turn-meta";
            const speakerName = document.createElement("strong");
            speakerName.textContent = normalizeSpeakerName(turn.speakerName);
            meta.appendChild(speakerName);
            meta.appendChild(sourceBadge);
            if (turn.important) {
                const importantBadge = document.createElement("span");
                importantBadge.className = "participant-badge";
                importantBadge.textContent = "Importante";
                meta.appendChild(importantBadge);
            }

            const text = document.createElement("div");
            text.className = "turn-readonly-text";
            text.textContent = turn.text;

            row.appendChild(meta);
            row.appendChild(text);
            section.appendChild(row);
        });

        list.appendChild(section);
    });
}

async function addGuestParticipant(survey) {
    state.currentProject = await api.createParticipant(state.currentProject.id, survey.id, {
        displayName: null,
        roleType: "GuestPlaceholder"
    });
    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    getAppHandlers().renderApp();
}

async function renameParticipant(survey, participant, displayName, email = "") {
    state.currentProject = await api.renameParticipant(state.currentProject.id, survey.id, participant.id, {
        displayName,
        email
    });
    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    getAppHandlers().renderApp();
}

async function deleteParticipant(survey, participant) {
    const turnCount = getSurveyTurns(survey).filter(turn => turn.speakerId === participant.id).length;
    if (turnCount > 0) {
        alert(`No puedes eliminar a ${participant.displayName} porque ya tiene transcript asociado.`);
        return;
    }

    if (!confirm(`Eliminar a ${participant.displayName}?`)) {
        return;
    }

    state.currentProject = await api.deleteParticipant(state.currentProject.id, survey.id, participant.id);
    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    getAppHandlers().renderApp();
}

function getSurveyById(surveyId) {
    return state.currentProject?.surveys.find(item => item.id === surveyId) ?? null;
}

function getSurveyCard(surveyId) {
    return [...document.querySelectorAll(".survey-card")].find(node => node.dataset.surveyId === surveyId) ?? null;
}

function getCaptureState(survey, card) {
    const activeCapture = state.activeCapture?.surveyId === survey.id ? state.activeCapture : null;
    const questionContext = getGuideQuestionContext(survey);
    const speakerId = activeCapture?.speakerId ?? resolveDefaultSpeakerId(survey);
    return {
        surveyId: survey.id,
        speakerId,
        status: activeCapture?.status ?? "idle",
        text: activeCapture?.text ?? "",
        important: activeCapture?.important ?? false,
        turnId: activeCapture?.turnId ?? null,
        card: activeCapture?.card ?? card,
        questionKey: activeCapture?.questionKey ?? questionContext.questionKey,
        questionLabel: activeCapture?.questionLabel ?? questionContext.questionLabel,
        processing: activeCapture?.processing ?? false
    };
}

function getSurveyParticipants(survey) {
    return Array.isArray(survey?.participants) ? survey.participants.filter(Boolean) : [];
}

function getSurveyTurns(survey) {
    return Array.isArray(survey?.transcriptTurns) ? survey.transcriptTurns.filter(Boolean) : [];
}

function resolveCaptureHint(captureState) {
    if (captureState.processing) return "El navegador sigue convirtiendo voz a texto.";
    if (captureState.status === "recording") return "Transcribiendo en vivo mientras hablas.";
    if (captureState.status === "paused") return "Turno pausado; puedes corregir el texto y continuar.";
    return "Selecciona quien habla y comienza un turno nuevo.";
}

function resolveCaptureStatusLabel(captureState) {
    if (captureState.processing) return "Transcribiendo";
    if (captureState.status === "recording") return "Grabando";
    if (captureState.status === "paused") return "Pausado";
    return "Listo";
}

async function selectCaptureSpeaker(survey, speakerId) {
    await switchCaptureContext(survey, {
        speakerId,
        shouldResumeRecording: state.activeCapture?.surveyId === survey.id && state.activeCapture.status === "recording"
    });
}

async function startCapture(survey, card, speakerId) {
    if (!state.recognition || !speakerId) {
        return;
    }

    if (state.activeCapture?.status === "recording" && state.activeCapture.surveyId === survey.id) {
        return;
    }

    const existing = state.activeCapture?.surveyId === survey.id ? state.activeCapture : null;
    const questionContext = getGuideQuestionContext(survey);
    const shouldResumePausedTurn =
        existing?.status === "paused" &&
        existing.speakerId === speakerId &&
        existing.questionKey === questionContext.questionKey;
    state.activeCapture = shouldResumePausedTurn
        ? {
            ...existing,
            status: "recording",
            card,
            processing: false,
            processingTimer: null,
            stopReason: null,
            pendingContext: null
        }
        : createDraftTurnCapture(survey, card, speakerId, questionContext, {
            status: "recording",
            sourceType: "Voice"
        });
    updateBufferFromCapture(state.activeCapture);

    renderSurveyCapture(survey.id);
    setSpeechStatus("Escuchando...");
    playCaptureTone("start");
    state.recognition.start();
}

function pauseCapture() {
    if (!state.activeCapture || state.activeCapture.status !== "recording") return;
    state.activeCapture.stopReason = "pause";
    playCaptureTone("pause");
    state.recognition?.stop();
}

async function continueCapture(survey, card) {
    if (!state.activeCapture || state.activeCapture.surveyId !== survey.id || state.activeCapture.status !== "paused") return;
    state.activeCapture.status = "recording";
    state.activeCapture.card = card;
    state.activeCapture.baseText = state.activeCapture.text ?? "";
    renderSurveyCapture(survey.id);
    setSpeechStatus("Escuchando...");
    playCaptureTone("continue");
    state.recognition?.start();
}

function stopCapture(reason) {
    if (!state.activeCapture || state.activeCapture.status === "idle") return;
    if (state.activeCapture.syncTimer) {
        clearTimeout(state.activeCapture.syncTimer);
        state.activeCapture.syncTimer = null;
    }
    if (state.activeCapture.restartTimer) {
        clearTimeout(state.activeCapture.restartTimer);
        state.activeCapture.restartTimer = null;
    }
    state.activeCapture.stopReason = reason;
    if (reason === "stop" || reason === "finalize") playCaptureTone("stop");
    if (reason === "switch" && typeof state.recognition?.abort === "function") {
        state.recognition.abort();
        return;
    }

    state.recognition?.stop();
}

async function updateActiveCaptureDraft(surveyId, card, patch) {
    const survey = getSurveyById(surveyId);
    if (!survey) return;
    const questionContext = getGuideQuestionContext(survey);
    const speakerId = state.activeCapture?.speakerId ?? resolveDefaultSpeakerId(survey);
    if (!state.activeCapture || state.activeCapture.surveyId !== surveyId) {
        state.activeCapture = createDraftTurnCapture(survey, card, speakerId, questionContext, {
            status: "idle",
            sourceType: "Voice"
        });
    }

    Object.assign(state.activeCapture, patch);
    updateBufferFromCapture(state.activeCapture);
    state.activeCapture.card = card;
    updateCaptureLiveView(surveyId);
    queueCaptureSync(surveyId, 450, getCaptureBufferKey(state.activeCapture));
}

function queueCaptureSync(surveyId, delayMs = 450, bufferKey = null) {
    if (!state.activeCapture || state.activeCapture.surveyId !== surveyId) return;
    const resolvedBufferKey = bufferKey ?? getCaptureBufferKey(state.activeCapture);
    const buffer = state.captureBuffers[resolvedBufferKey];
    if (!buffer) return;
    if (buffer.syncTimer) clearTimeout(buffer.syncTimer);
    buffer.syncTimer = setTimeout(() => {
        const currentBuffer = state.captureBuffers[resolvedBufferKey];
        if (!currentBuffer) return;
        currentBuffer.syncTimer = null;
        syncCaptureBuffer(resolvedBufferKey);
    }, delayMs);
}

async function syncActiveCapture(surveyId, overridePatch = null) {
    if (!state.activeCapture || state.activeCapture.surveyId !== surveyId) return;
    if (overridePatch) Object.assign(state.activeCapture, overridePatch);
    updateBufferFromCapture(state.activeCapture);
    await syncCaptureBuffer(getCaptureBufferKey(state.activeCapture));
}

async function syncCaptureBuffer(bufferKey) {
    const capture = state.captureBuffers[bufferKey];
    if (!capture) {
        return;
    }

    const survey = getSurveyById(capture.surveyId);
    if (!survey || !capture.text.trim()) {
        if (state.activeCapture && getCaptureBufferKey(state.activeCapture) === bufferKey) {
            updateCaptureLiveView(capture.surveyId);
        }
        return;
    }

    capture.syncPromise = (capture.syncPromise ?? Promise.resolve()).then(async () => {
        const request = {
            speakerId: capture.speakerId,
            text: capture.text,
            tag: buildQuestionTag(capture.questionKey, capture.questionLabel),
            important: capture.important,
            sourceType: capture.sourceType ?? "Voice"
        };

        let project;
        if (!capture.turnId) {
            project = await api.addTurn(state.currentProject.id, capture.surveyId, request);
        } else {
            project = await api.updateTurn(state.currentProject.id, capture.surveyId, capture.turnId, request);
        }

        state.currentProject = project;
        const freshSurvey = getSurveyById(capture.surveyId);
        if (!capture.turnId && freshSurvey) {
            capture.turnId = resolveLatestTurnId(freshSurvey, request);
        }

        capture.lastSyncedText = request.text;
        state.captureBuffers[bufferKey] = capture;
        if (state.activeCapture && getCaptureBufferKey(state.activeCapture) === bufferKey) {
            state.activeCapture.turnId = capture.turnId;
            state.activeCapture.lastSyncedText = capture.lastSyncedText;
            updateCaptureLiveView(capture.surveyId);
        }
        if (freshSurvey) {
            const nextCard = getSurveyCard(capture.surveyId) ?? capture.card;
            if (nextCard) renderTranscriptTurns(freshSurvey, nextCard);
        }
    }).catch(() => {
        setSpeechStatus("No se pudo sincronizar la captura");
    });

    await capture.syncPromise;
}

function resolveLatestTurnId(survey, request) {
    return getSurveyTurns(survey)
        .filter(turn =>
            turn.speakerId === request.speakerId &&
            turn.text === request.text &&
            turn.tag === request.tag &&
            turn.important === request.important)
        .sort((left, right) => new Date(right.updatedAt) - new Date(left.updatedAt))[0]?.id ?? null;
}

function getGuideQuestionContext(survey) {
    const guideState = ensureGuideState(survey);
    const sections = getInterviewSections(survey);
    const section = sections[guideState.sectionIndex];
    const questions = Array.isArray(section?.questions) ? section.questions : [];
    const question = questions[guideState.questionIndex] ?? "";
    const sectionTitle = section?.title ?? "Sin seccion";
    const questionLabel = question ? `${sectionTitle} - ${question}` : sectionTitle;
    return {
        sectionIndex: guideState.sectionIndex,
        questionIndex: guideState.questionIndex,
        sectionTitle,
        questionText: question,
        questionKey: `${guideState.sectionIndex}:${guideState.questionIndex}`,
        questionLabel
    };
}

function buildQuestionTag(questionKey, questionLabel) {
    return `question:${questionKey}|${questionLabel}`;
}

function parseQuestionTag(tag) {
    if (!tag || !tag.startsWith("question:")) return null;
    const separatorIndex = tag.indexOf("|");
    if (separatorIndex < 0) return null;
    return {
        questionKey: tag.slice("question:".length, separatorIndex),
        questionLabel: tag.slice(separatorIndex + 1) || "Pregunta sin titulo"
    };
}

function groupTurnsByQuestion(survey) {
    const groups = new Map();
    getSurveyTurns(survey)
        .slice()
        .sort((left, right) => new Date(left.createdAt) - new Date(right.createdAt))
        .forEach(turn => {
            const parsed = parseQuestionTag(turn.tag);
            const questionKey = parsed?.questionKey ?? "opening";
            const questionLabel = resolveQuestionLabel(survey, questionKey, parsed?.questionLabel);
            if (!groups.has(questionKey)) {
                groups.set(questionKey, { questionKey, questionLabel, turns: [] });
            }
            groups.get(questionKey).turns.push(turn);
        });
    return [...groups.values()];
}

function buildSurveyUpdateRequest(survey, card, overrides = {}) {
    const participantEmailInputs = [...card.querySelectorAll(".participant-email")];
    const ownerEmail = participantEmailInputs[0]?.value ?? survey.ownerEmail ?? "";
    const intervieweeEmail = participantEmailInputs.slice(1).map(input => input.value.trim()).find(Boolean) ?? survey.intervieweeEmail ?? "";
    return {
        title: card.querySelector(".survey-title-input").value,
        interviewee: survey.interviewee ?? "",
        objective: card.querySelector(".survey-objective-input").value,
        templateId: overrides.templateId ?? null,
        templateScope: overrides.templateScope ?? null,
        ownerEmail,
        intervieweeEmail,
        extraEmails: card.querySelector(".survey-extra-emails")?.value ?? survey.extraEmails ?? "",
        minuteDraft: card.querySelector(".review-minute-text")?.value ?? survey.minuteDraft ?? "",
        isFinalized: typeof overrides.isFinalized === "boolean" ? overrides.isFinalized : Boolean(survey.isFinalized),
        ...overrides
    };
}

function canAssignTemplateToSurvey(survey) {
    return !survey.appliedTemplate && getSurveyTurns(survey).length === 0;
}

function populateSurveyTemplateOptions(select) {
    select.innerHTML = "";

    state.systemTemplates.forEach(template => {
        const option = document.createElement("option");
        option.value = `system:${template.id}`;
        option.textContent = `Sistema - ${template.name}`;
        select.appendChild(option);
    });

    state.currentProject?.projectTemplates?.forEach(template => {
        const option = document.createElement("option");
        option.value = `project:${template.id}`;
        option.textContent = `Proyecto - ${template.name}`;
        select.appendChild(option);
    });
}

async function applyTemplateToSurvey(survey, card, selection) {
    if (!selection || !canAssignTemplateToSurvey(survey)) {
        return;
    }

    const [templateScope, templateId] = selection.split(":");
    state.currentProject = await api.updateSurvey(
        state.currentProject.id,
        survey.id,
        buildSurveyUpdateRequest(survey, card, {
            templateId: templateId || null,
            templateScope: templateScope || null
        }));
    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    getAppHandlers().renderApp();
}

function getCaptureBufferKey(capture) {
    return `${capture.surveyId}|${capture.speakerId}|${capture.questionKey}|${capture.turnToken}`;
}

function getCaptureBuffer(surveyId, speakerId, questionKey) {
    state.captureBuffers ??= {};
    return Object.values(state.captureBuffers).find(item =>
        item.surveyId === surveyId &&
        item.speakerId === speakerId &&
        item.questionKey === questionKey &&
        item.isActive) ?? null;
}

function getOrCreateCaptureBuffer(capture) {
    state.captureBuffers ??= {};
    const key = getCaptureBufferKey(capture);
    if (!state.captureBuffers[key]) {
        state.captureBuffers[key] = {
            surveyId: capture.surveyId,
            speakerId: capture.speakerId,
            questionKey: capture.questionKey,
            turnToken: capture.turnToken,
            text: "",
            important: false,
            turnId: null,
            lastSyncedText: "",
            syncTimer: null,
            syncPromise: Promise.resolve(),
            isActive: true,
            sourceType: capture.sourceType ?? "Voice"
        };
    }

    return state.captureBuffers[key];
}

function updateBufferFromCapture(capture) {
    const key = getCaptureBufferKey(capture);
    const buffer = getOrCreateCaptureBuffer(capture);
    buffer.text = capture.text ?? "";
    buffer.important = capture.important ?? false;
    buffer.turnId = capture.turnId ?? buffer.turnId;
    buffer.lastSyncedText = capture.lastSyncedText ?? buffer.lastSyncedText;
    buffer.syncPromise = capture.syncPromise ?? buffer.syncPromise;
    buffer.syncTimer = capture.syncTimer ?? buffer.syncTimer;
    buffer.isActive = capture.status !== "completed";
    buffer.sourceType = capture.sourceType ?? buffer.sourceType;
    state.captureBuffers[key] = buffer;
}

function renderSurveyReview(survey, card) {
    const container = card.querySelector(".survey-review");
    container.innerHTML = "";
    const preferredDraft = survey.minuteDraft?.trim()
        ? survey.minuteDraft
        : (survey.suggestedMinute || survey.conversationCopy || "");
    const draft = shouldRegenerateMinuteText(preferredDraft, survey)
        ? buildMinuteTextFromSurvey(survey)
        : normalizeMinuteText(preferredDraft, survey);

    const form = document.createElement("div");
    form.className = "survey-review-form";
    const minuteText = document.createElement("textarea");
    minuteText.className = "review-minute-text";
    minuteText.placeholder = "Aqui puedes validar y ajustar la minuta.";
    minuteText.value = draft;
    form.appendChild(minuteText);

    const actions = document.createElement("div");
    actions.className = "form-actions";

    const saveButton = document.createElement("button");
    saveButton.type = "button";
    saveButton.className = "primary";
    saveButton.textContent = survey.isFinalized ? "Guardar validacion" : "Guardar minuta";
    saveButton.addEventListener("click", async () => {
        await syncPendingSurveyCapture(survey.id);
        const freshSurvey = getSurveyById(survey.id) ?? survey;
        const minuteDraft = buildReviewMinuteDraft(freshSurvey, card);
        state.currentProject = await api.updateSurvey(state.currentProject.id, survey.id, buildSurveyUpdateRequest(freshSurvey, card, { minuteDraft }));
        await getAppHandlers().loadWorkspace();
        await getAppHandlers().refreshCurrentProject();
        getAppHandlers().renderApp();
    });

    const reloadButton = document.createElement("button");
    reloadButton.type = "button";
    reloadButton.textContent = "Recargar minuta";
    reloadButton.addEventListener("click", async () => {
        await syncPendingSurveyCapture(survey.id);
        const freshSurvey = getSurveyById(survey.id) ?? survey;
        card.querySelector(".review-minute-text").value = buildMinuteTextFromSurvey(freshSurvey);
        setSpeechStatus("Minuta regenerada desde el transcript");
    });

    const copyButton = document.createElement("button");
    copyButton.type = "button";
    copyButton.textContent = "Copiar minuta";
    copyButton.addEventListener("click", async () => {
        await copyTextToClipboard(card.querySelector(".review-minute-text").value);
    });

    const mailButton = document.createElement("button");
    mailButton.type = "button";
    mailButton.textContent = "Abrir borrador de correo";
    mailButton.addEventListener("click", async () => {
        await openMailDraftForSurvey(survey.id);
    });

    actions.appendChild(saveButton);
    actions.appendChild(reloadButton);
    actions.appendChild(copyButton);
    actions.appendChild(mailButton);
    container.appendChild(form);
    container.appendChild(actions);
}

async function finalizeSurveyInterview(survey, card) {
    if (state.activeCapture?.surveyId === survey.id && state.activeCapture.status !== "idle") {
        state.activeCapture.pendingFinalize = true;
        stopCapture("finalize");
        return;
    }

    await completeSurveyFinalization(survey.id);
}

async function completeSurveyFinalization(surveyId) {
    const survey = getSurveyById(surveyId);
    const card = getSurveyCard(surveyId);
    if (!survey || !card) return;

    await syncPendingSurveyCapture(surveyId);
    const freshSurvey = getSurveyById(surveyId) ?? survey;
    const minuteDraft = buildReviewMinuteDraft(freshSurvey, card);
    state.currentProject = await api.updateSurvey(state.currentProject.id, surveyId, buildSurveyUpdateRequest(freshSurvey, card, {
        minuteDraft,
        isFinalized: true
    }));
    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    state.reviewFocusSurveyId = surveyId;
    getAppHandlers().renderApp();

    const finalizedSurvey = getSurveyById(surveyId);
    if (finalizedSurvey && confirm("Encuesta finalizada. Deseas abrir un borrador de correo para validar la minuta?")) {
        await openMailDraftForSurvey(surveyId);
    }
}

async function openMailDraftForSurvey(surveyId) {
    const survey = getSurveyById(surveyId);
    const card = getSurveyCard(surveyId);
    if (!survey || !card) return;

    const request = buildSurveyUpdateRequest(survey, card);
    const recipients = [request.ownerEmail, request.intervieweeEmail, ...(request.extraEmails || "").split(",")]
        .map(item => item.trim())
        .filter(Boolean);
    const subject = `Validacion de minuta - ${request.title}`;
    const preferredDraft = request.minuteDraft || survey.suggestedMinute || survey.conversationCopy || "";
    const body = shouldRegenerateMinuteText(preferredDraft, survey)
        ? buildMinuteTextFromSurvey(survey)
        : normalizeMinuteText(preferredDraft, survey);
    const mailto = `mailto:${recipients.map(item => encodeURIComponent(item)).join(",")}?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(body)}`;
    window.location.href = mailto;
}

async function syncPendingSurveyCapture(surveyId) {
    const pendingBuffers = Object.entries(state.captureBuffers ?? {})
        .filter(([, buffer]) => buffer?.surveyId === surveyId && buffer.text?.trim());

    for (const [bufferKey] of pendingBuffers) {
        await syncCaptureBuffer(bufferKey);
    }
}

function buildReviewMinuteDraft(survey, card) {
    const currentDraft = card.querySelector(".review-minute-text")?.value
        || survey.minuteDraft
        || survey.suggestedMinute
        || survey.conversationCopy
        || "";

    return shouldRegenerateMinuteText(currentDraft, survey)
        ? buildMinuteTextFromSurvey(survey)
        : normalizeMinuteText(currentDraft, survey);
}

async function copyTextToClipboard(text) {
    await navigator.clipboard.writeText(text || "");
    setSpeechStatus("Texto copiado");
}

async function saveSurveyUsers(survey, card) {
    state.currentProject = await api.updateSurvey(state.currentProject.id, survey.id, buildSurveyUpdateRequest(survey, card));
    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    getAppHandlers().renderApp();
}

async function switchCaptureContext(survey, options = {}) {
    const speakerId = options.speakerId ?? resolveDefaultSpeakerId(survey);
    const card = getSurveyCard(survey.id) ?? state.activeCapture?.card ?? null;
    const questionContext = getGuideQuestionContext(survey);
    const shouldResumeRecording = Boolean(options.shouldResumeRecording);
    const nextCapture = createDraftTurnCapture(survey, card, speakerId, questionContext, {
        status: shouldResumeRecording ? "recording" : "idle",
        sourceType: "Voice"
    });

    if (!state.activeCapture || state.activeCapture.surveyId !== survey.id || state.activeCapture.status === "idle") {
        if (state.activeCapture?.surveyId === survey.id && hasUnsyncedCaptureDraft(state.activeCapture)) {
            await syncActiveCapture(survey.id);
        }
        state.activeCapture = nextCapture;
        updateBufferFromCapture(state.activeCapture);
        renderSurveyCapture(survey.id);
        if (shouldResumeRecording) {
            setSpeechStatus("Escuchando...");
            playCaptureTone("continue");
            try {
                state.recognition?.start();
            } catch {
                state.activeCapture.status = "paused";
            }
            renderSurveyCapture(survey.id);
        }
        return;
    }

    if (state.activeCapture.status !== "recording") {
        if (hasUnsyncedCaptureDraft(state.activeCapture)) {
            await syncActiveCapture(survey.id);
        }
        state.activeCapture = nextCapture;
        updateBufferFromCapture(state.activeCapture);
        renderSurveyCapture(survey.id);
        return;
    }

    if (state.activeCapture.stopReason === "switch") {
        state.activeCapture.pendingContext = nextCapture;
        state.activeCapture.card = card;
        setSpeechStatus("Cambiando pregunta, espera un momento...");
        renderSurveyCapture(survey.id);
        return;
    }

    if (state.activeCapture.speakerId === nextCapture.speakerId && state.activeCapture.questionKey === nextCapture.questionKey) {
        state.activeCapture.card = card;
        renderSurveyCapture(survey.id);
        return;
    }

    state.activeCapture.pendingContext = nextCapture;
    setSpeechStatus("Cambiando pregunta, espera un momento...");
    stopCapture("switch");
}

function renderSurveyCapture(surveyId) {
    const survey = getSurveyById(surveyId);
    const card = getSurveyCard(surveyId);
    if (!survey || !card) return;
    renderInterviewGuide(survey, card);
}

function updateCaptureLiveView(surveyId) {
    const survey = getSurveyById(surveyId);
    const card = getSurveyCard(surveyId);
    if (!survey || !card) return;

    const captureState = getCaptureState(survey, card);
    const textarea = card.querySelector(".capture-live-text");
    const status = card.querySelector(".capture-status");
    const hint = card.querySelector(".capture-status-row small");
    const questionLabel = card.querySelector(".question-memory-title small");

    if (textarea) {
        const wasFocused = document.activeElement === textarea;
        const selectionStart = textarea.selectionStart ?? textarea.value.length;
        const selectionEnd = textarea.selectionEnd ?? textarea.value.length;
        if (textarea.value !== captureState.text) {
            textarea.value = captureState.text;
            if (wasFocused) {
                const nextStart = Math.min(selectionStart, textarea.value.length);
                const nextEnd = Math.min(selectionEnd, textarea.value.length);
                textarea.setSelectionRange(nextStart, nextEnd);
            }
        }
    }

    if (status) {
        status.className = `capture-status ${captureState.processing ? "processing" : captureState.status}`;
        status.textContent = resolveCaptureStatusLabel(captureState);
    }

    if (hint) {
        hint.textContent = resolveCaptureHint(captureState);
    }

    if (questionLabel) {
        questionLabel.textContent = captureState.questionLabel || "Sin pregunta activa";
    }

    refreshGuideActionButton(survey, card);
    refreshQuestionProgressBadges(survey, card);
}

function createDraftTurnCapture(survey, card, speakerId, questionContext, overrides = {}) {
    return {
        surveyId: survey.id,
        speakerId,
        status: "idle",
        text: "",
        important: false,
        turnId: null,
        sourceType: "Voice",
        card,
        questionKey: questionContext.questionKey,
        questionLabel: questionContext.questionLabel,
        baseText: "",
        processing: false,
        processingTimer: null,
        lastSyncedText: "",
        syncTimer: null,
        restartTimer: null,
        syncPromise: Promise.resolve(),
        stopReason: null,
        pendingContext: null,
        pendingFinalize: false,
        turnToken: createTurnToken(),
        ...overrides
    };
}

function createTurnToken() {
    return `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
}

function hasUnsyncedCaptureDraft(capture) {
    return Boolean(capture?.text?.trim()) && capture.text !== (capture.lastSyncedText ?? "");
}

function ensureGuideState(survey) {
    state.surveyGuideState ??= {};
    const sections = getInterviewSections(survey);
    const safeSectionIndex = Math.max(0, Math.min(state.surveyGuideState[survey.id]?.sectionIndex ?? 0, Math.max(sections.length - 1, 0)));
    const safeQuestions = Array.isArray(sections[safeSectionIndex]?.questions) ? sections[safeSectionIndex].questions : [];
    const safeQuestionIndex = Math.max(0, Math.min(state.surveyGuideState[survey.id]?.questionIndex ?? 0, Math.max(safeQuestions.length - 1, 0)));
    state.surveyGuideState[survey.id] = { sectionIndex: safeSectionIndex, questionIndex: safeQuestionIndex };
    return state.surveyGuideState[survey.id];
}

function buildGuideProgressText(survey, guideState) {
    const sections = getInterviewSections(survey);
    const counts = sections.map(section => Array.isArray(section.questions) ? section.questions.length : 0);
    const totalQuestions = counts.reduce((sum, count) => sum + count, 0);
    const completedInPreviousSections = counts.slice(0, guideState.sectionIndex).reduce((sum, count) => sum + count, 0);
    const currentNumber = completedInPreviousSections + guideState.questionIndex + 1;
    return `Pregunta ${currentNumber} de ${totalQuestions}`;
}

function hasPreviousGuideQuestion(survey, guideState) {
    return !(guideState.sectionIndex === 0 && guideState.questionIndex === 0);
}

function hasNextGuideQuestion(survey, guideState) {
    const sections = getInterviewSections(survey);
    const lastSectionIndex = sections.length - 1;
    const lastQuestionIndex = (Array.isArray(sections[lastSectionIndex]?.questions) ? sections[lastSectionIndex].questions.length : 1) - 1;
    return !(guideState.sectionIndex === lastSectionIndex && guideState.questionIndex === lastQuestionIndex);
}

function setGuideQuestion(surveyId, sectionIndex, questionIndex) {
    state.surveyGuideState[surveyId] = { sectionIndex, questionIndex };
    getAppHandlers().selectTab("surveys");
    const survey = getSurveyById(surveyId);
    if (!survey) return;

    if (getSurveyCard(surveyId)) {
        renderSurveyCapture(surveyId);
    } else {
        getAppHandlers().renderApp();
    }

    switchCaptureContext(survey, {
        speakerId: resolveDefaultSpeakerId(survey),
        shouldResumeRecording: state.activeCapture?.surveyId === surveyId && state.activeCapture.status === "recording"
    });
    playCaptureTone("focus");
}

function moveGuideQuestion(survey, direction) {
    if (direction > 0 && !canAdvanceToNextQuestion(survey)) {
        return;
    }

    const guideState = ensureGuideState(survey);
    const sections = getInterviewSections(survey);
    let sectionIndex = guideState.sectionIndex;
    let questionIndex = guideState.questionIndex + direction;

    if (questionIndex < 0) {
        sectionIndex -= 1;
        if (sectionIndex < 0) return;
        questionIndex = (Array.isArray(sections[sectionIndex]?.questions) ? sections[sectionIndex].questions.length : 1) - 1;
    }

    if (questionIndex >= (Array.isArray(sections[sectionIndex]?.questions) ? sections[sectionIndex].questions.length : 0)) {
        sectionIndex += 1;
        if (sectionIndex >= sections.length) return;
        questionIndex = 0;
    }

    setGuideQuestion(survey.id, sectionIndex, questionIndex);
}

function getNextQuestionAction(survey) {
    return canAdvanceToNextQuestion(survey, true) ? "next" : "request-response";
}

function requestIntervieweeResponse(survey) {
    const participants = getSurveyParticipants(survey);
    const firstGuest = participants.find(item => item.roleType !== "Self");
    if (!firstGuest) {
        return;
    }

    switchCaptureContext(survey, {
        speakerId: firstGuest.id,
        shouldResumeRecording: state.activeCapture?.surveyId === survey.id && state.activeCapture.status === "recording"
    });
    setSpeechStatus("Captura lista para la respuesta del entrevistado");
}

function canAdvanceToNextQuestion(survey, previewOnly = false) {
    const guideState = ensureGuideState(survey);
    const questionKey = `${guideState.sectionIndex}:${guideState.questionIndex}`;
    const participants = getSurveyParticipants(survey);
    const hasGuest = participants.some(item => item.roleType !== "Self");
    if (!hasGuest) {
        return true;
    }

    const hasGuestReply = getSurveyTurns(survey).some(turn => {
        if (turn.speakerId === resolveDefaultSpeakerId(survey)) {
            return false;
        }

        return parseQuestionTag(turn.tag)?.questionKey === questionKey;
    }) || hasActiveGuestReply(survey, questionKey);

    if (hasGuestReply) {
        return true;
    }

    if (previewOnly) {
        return false;
    }

    const firstGuest = participants.find(item => item.roleType !== "Self");
    if (firstGuest && state.activeCapture?.speakerId !== firstGuest.id) {
        switchCaptureContext(survey, {
            speakerId: firstGuest.id,
            shouldResumeRecording: state.activeCapture?.surveyId === survey.id && state.activeCapture.status === "recording"
        });
        setSpeechStatus("Falta respuesta del entrevistado en esta pregunta");
        return false;
    }

    return true;
}

function bindSaveOnEnter(element, handler) {
    if (!element || element.dataset.enterSaveBound === "true") {
        return;
    }

    element.dataset.enterSaveBound = "true";
    element.addEventListener("keydown", async event => {
        if (event.key !== "Enter" || event.shiftKey) {
            return;
        }

        event.preventDefault();
        await handler();
    });
}

function bindAutoSaveOnBlur(element, handler) {
    if (!element || element.dataset.blurSaveBound === "true") {
        return;
    }

    element.dataset.blurSaveBound = "true";
    element.addEventListener("blur", async () => {
        await handler();
    });
}

function getLatestContextTurn(survey, speakerId, questionKey) {
    return getSurveyTurns(survey)
        .filter(turn => turn.speakerId === speakerId && parseQuestionTag(turn.tag)?.questionKey === questionKey)
        .sort((left, right) => new Date(right.updatedAt) - new Date(left.updatedAt))[0] ?? null;
}

function getQuestionProgressState(survey, questionKey) {
    const selfSpeakerId = resolveDefaultSpeakerId(survey);
    const turns = getSurveyTurns(survey);
    const hasAsked = turns.some(turn =>
        turn.speakerId === selfSpeakerId &&
        parseQuestionTag(turn.tag)?.questionKey === questionKey) ||
        hasActiveSpeakerDraft(survey, selfSpeakerId, questionKey);
    const hasResponse = turns.some(turn =>
        turn.speakerId !== selfSpeakerId &&
        parseQuestionTag(turn.tag)?.questionKey === questionKey) ||
        hasActiveGuestReply(survey, questionKey);
    return { hasAsked, hasResponse };
}

function hasActiveGuestReply(survey, questionKey) {
    const selfSpeakerId = resolveDefaultSpeakerId(survey);
    return Boolean(
        state.activeCapture &&
        state.activeCapture.surveyId === survey.id &&
        state.activeCapture.speakerId !== selfSpeakerId &&
        state.activeCapture.questionKey === questionKey &&
        state.activeCapture.text?.trim()
    );
}

function hasActiveSpeakerDraft(survey, speakerId, questionKey) {
    return Boolean(
        state.activeCapture &&
        state.activeCapture.surveyId === survey.id &&
        state.activeCapture.speakerId === speakerId &&
        state.activeCapture.questionKey === questionKey &&
        state.activeCapture.text?.trim()
    );
}

function refreshGuideActionButton(survey, card) {
    const navButtons = card.querySelectorAll(".guide-nav button");
    const currentButton = navButtons[1];
    if (!currentButton) {
        return;
    }

    const replacement = currentButton.cloneNode(false);
    const nextAction = getNextQuestionAction(survey);
    if (nextAction === "request-response") {
        replacement.textContent = "Responder";
        replacement.className = "attention";
        replacement.addEventListener("click", () => requestIntervieweeResponse(survey));
    } else if (hasNextGuideQuestion(survey, ensureGuideState(survey))) {
        replacement.textContent = "Siguiente pregunta";
        replacement.className = "primary";
        replacement.addEventListener("click", () => moveGuideQuestion(survey, 1));
    } else {
        replacement.textContent = "Finaliza";
        replacement.className = "primary";
        replacement.addEventListener("click", () => finalizeSurveyInterview(survey, card));
    }

    currentButton.replaceWith(replacement);
}

function refreshQuestionProgressBadges(survey, card) {
    const sectionCards = [...card.querySelectorAll(".guide-section-card")];
    sectionCards.forEach((sectionCard, sectionIndex) => {
        const buttons = [...sectionCard.querySelectorAll(".question-list-item")];
        buttons.forEach((button, questionIndex) => {
            const questionKey = `${sectionIndex}:${questionIndex}`;
            const progressState = getQuestionProgressState(survey, questionKey);
            button.classList.toggle("asked", progressState.hasAsked);
            button.classList.toggle("answered", progressState.hasResponse);
            const summary = button.querySelector(".question-status-summary");
            if (summary) {
                summary.textContent = buildQuestionStatusLabel(progressState);
            }
        });
    });
}

function buildQuestionStatusLabel(progressState) {
    if (progressState.hasResponse) {
        return "Respondida";
    }

    if (progressState.hasAsked) {
        return "Preguntada";
    }

    return "Pendiente";
}

function normalizeSpeakerName(value) {
    if (!value || !value.trim()) {
        return "Persona";
    }

    return value.trim().toLowerCase() === "yo" ? "Entrevistador" : value.trim();
}

function getDisplayParticipantName(participant) {
    if (!participant) {
        return "Persona";
    }

    return participant.roleType === "Self" ? "Entrevistador" : (participant.displayName || "Persona");
}

function resolveSelfParticipant(survey) {
    const participants = getSurveyParticipants(survey);
    return participants.find(participant => participant.roleType === "Self") ?? participants[0] ?? null;
}

function resolveDefaultSpeakerId(survey) {
    return resolveSelfParticipant(survey)?.id ?? getSurveyParticipants(survey)[0]?.id ?? "";
}

function getInterviewSections(survey) {
    return Array.isArray(survey?.appliedTemplate?.interviewSections)
        ? survey.appliedTemplate.interviewSections.filter(Boolean)
        : [];
}

function resolveQuestionLabel(survey, questionKey, fallbackLabel) {
    if (fallbackLabel && fallbackLabel.trim() && fallbackLabel.trim().toLowerCase() !== "undefined") {
        return fallbackLabel.trim();
    }

    if (!questionKey || questionKey === "opening") {
        return "Apertura de entrevista";
    }

    const [sectionIndexText, questionIndexText] = questionKey.split(":");
    const sectionIndex = Number.parseInt(sectionIndexText, 10);
    const questionIndex = Number.parseInt(questionIndexText, 10);
    if (Number.isNaN(sectionIndex) || Number.isNaN(questionIndex)) {
        return "Pregunta sin titulo";
    }

    const section = getInterviewSections(survey)[sectionIndex];
    const question = Array.isArray(section?.questions) ? section.questions[questionIndex] : "";
    if (!section) {
        return "Pregunta sin titulo";
    }

    return question ? `${section.title} - ${question}` : (section.title || "Pregunta sin titulo");
}

function normalizeMinuteText(text, survey) {
    const source = text || "";
    return source
        .replaceAll("] Yo:", "] Entrevistador:")
        .replaceAll("### undefined", "### Pregunta sin titulo");
}

function restartRecognitionForPendingCapture(pending) {
    if (shouldUseManualMobileSpeech()) {
        pending.status = "paused";
        setSpeechStatus("Dictado en pausa; toca iniciar para seguir grabando");
        renderSurveyCapture(pending.surveyId);
        return;
    }

    state.captureRestartAttempts = 0;

    const tryStart = () => {
        if (!state.activeCapture || state.activeCapture !== pending || pending.status !== "recording") {
            return;
        }

        try {
            state.recognition.start();
            state.captureRestartAttempts = 0;
            setSpeechStatus("Escuchando...");
        } catch {
            state.captureRestartAttempts += 1;
            if (state.captureRestartAttempts >= 12) {
                state.activeCapture.status = "paused";
                setSpeechStatus("No pude reactivar el microfono tras varios intentos; toca iniciar para seguir grabando");
                renderSurveyCapture(pending.surveyId);
                return;
            }

            setSpeechStatus(`Procesando cambio de pregunta... intento ${state.captureRestartAttempts + 1} de 12`);
            setTimeout(tryStart, state.captureRestartAttempts < 4 ? 140 : 220);
        }
    };

    setSpeechStatus("Procesando cambio de pregunta...");
    setTimeout(tryStart, 100);
}

function shouldRegenerateMinuteText(text, survey = null) {
    const normalized = (text || "").trim().toLowerCase();
    if (!normalized) {
        return true;
    }

    if (normalized.includes("### undefined") || normalized.includes("### pregunta sin titulo")) {
        return true;
    }

    const hasSavedTurns = survey ? getSurveyTurns(survey).some(turn => turn.text?.trim()) : false;
    const hasMinuteStructure = normalized.includes("###") || normalized.includes("notas sugeridas:");
    return hasSavedTurns && !hasMinuteStructure;
}

function buildMinuteTextFromSurvey(survey) {
    const lines = [`Minuta de ${survey.title}`];
    if (survey.interviewee?.trim()) {
        lines.push(`Entrevistado: ${survey.interviewee.trim()}`);
    }

    for (const group of groupTurnsByQuestion(survey)) {
        lines.push("");
        lines.push(`### ${resolveQuestionLabel(survey, group.questionKey, group.questionLabel)}`);
        for (const turn of group.turns) {
            const speaker = normalizeSpeakerName(turn.speakerName || getParticipantById(survey, turn.speakerId)?.displayName || "Persona");
            lines.push(`- [${formatTime(turn.createdAt)}] ${speaker}: ${turn.text}`);
        }
    }

    const minuteSections = Array.isArray(survey.appliedTemplate?.minuteSections)
        ? survey.appliedTemplate.minuteSections.filter(Boolean)
        : [];

    if (minuteSections.length > 0) {
        lines.push("");
        lines.push("Notas sugeridas:");
        for (const section of minuteSections) {
            lines.push(`- ${section.title}: ${section.prompt}`);
        }
    }

    return lines.join("\n").trim();
}

function shouldUseManualMobileSpeech() {
    return /Android|iPhone|iPad|iPod/i.test(navigator.userAgent || "");
}

export function setupSpeech() {
    const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (!SpeechRecognition) {
        setSpeechStatus("Dictado no disponible en este navegador");
        return;
    }

    state.recognition = new SpeechRecognition();
    state.recognition.lang = "es-MX";
    state.recognition.interimResults = true;
    state.recognition.continuous = !shouldUseManualMobileSpeech();
    state.recognition.maxAlternatives = 1;
    state.recognition.onstart = () => {
        if (!state.activeCapture) return;
        if (state.activeCapture.restartTimer) {
            clearTimeout(state.activeCapture.restartTimer);
            state.activeCapture.restartTimer = null;
        }
        state.activeCapture.status = "recording";
        state.activeCapture.processing = false;
        setSpeechStatus("Escuchando...");
        updateCaptureLiveView(state.activeCapture.surveyId);
    };
    state.recognition.onresult = async event => {
        let text = "";
        for (let index = 0; index < event.results.length; index++) {
            text += `${event.results[index][0].transcript} `;
        }

        if (state.activeCapture?.card) {
            const baseText = state.activeCapture.baseText?.trim() ?? "";
            const transcriptText = text.trim();
            state.activeCapture.text = [baseText, transcriptText].filter(Boolean).join(baseText && transcriptText ? "\n" : "");
            state.activeCapture.processing = true;
            if (state.activeCapture.processingTimer) clearTimeout(state.activeCapture.processingTimer);
            state.activeCapture.processingTimer = setTimeout(() => {
                if (!state.activeCapture) return;
                state.activeCapture.processing = false;
                updateCaptureLiveView(state.activeCapture.surveyId);
            }, 1200);
            updateCaptureLiveView(state.activeCapture.surveyId);
            queueCaptureSync(state.activeCapture.surveyId, 300);
        }
    };
    state.recognition.onerror = event => {
        if (!state.activeCapture) return;
        if (event.error === "aborted") return;
        setSpeechStatus(`Dictado: ${event.error}`);
    };
    state.recognition.onend = async () => {
        if (!state.activeCapture) {
            setSpeechStatus("Dictado listo");
            return;
        }

        const capture = state.activeCapture;
        if (capture.stopReason === "pause") {
            if (capture.text.trim()) await syncActiveCapture(capture.surveyId);
            capture.status = "paused";
            capture.processing = false;
            capture.stopReason = null;
            setSpeechStatus("Dictado en pausa");
            renderSurveyCapture(capture.surveyId);
            return;
        }

        if (capture.stopReason === "stop") {
            if (capture.text.trim()) await syncActiveCapture(capture.surveyId);
            const freshSurvey = getSurveyById(capture.surveyId);
            const freshCard = getSurveyCard(capture.surveyId) ?? capture.card;
            const questionContext = freshSurvey ? getGuideQuestionContext(freshSurvey) : { questionKey: capture.questionKey, questionLabel: capture.questionLabel };
            state.activeCapture = freshSurvey
                ? createDraftTurnCapture(freshSurvey, freshCard, capture.speakerId, questionContext, {
                    status: "idle",
                    sourceType: capture.sourceType ?? "Voice"
                })
                : null;
            setSpeechStatus("Dictado listo");
            renderSurveyCapture(capture.surveyId);
            return;
        }

        if (capture.stopReason === "switch") {
            if (capture.text.trim()) await syncActiveCapture(capture.surveyId);
            const pending = capture.pendingContext;
            if (!pending) {
                const freshSurvey = getSurveyById(capture.surveyId);
                const freshCard = getSurveyCard(capture.surveyId) ?? capture.card;
                const questionContext = freshSurvey ? getGuideQuestionContext(freshSurvey) : { questionKey: capture.questionKey, questionLabel: capture.questionLabel };
                state.activeCapture = freshSurvey
                    ? createDraftTurnCapture(freshSurvey, freshCard, capture.speakerId, questionContext, {
                        status: "idle",
                        sourceType: capture.sourceType ?? "Voice"
                    })
                    : null;
                setSpeechStatus("Dictado listo");
                renderSurveyCapture(capture.surveyId);
                return;
            }

            state.activeCapture = pending;
            state.activeCapture.baseText = pending.text ?? "";
            state.activeCapture.processing = false;
            state.activeCapture.stopReason = null;
            state.activeCapture.pendingContext = null;
            renderSurveyCapture(pending.surveyId);

            if (pending.status === "recording") {
                restartRecognitionForPendingCapture(pending);
            }
            return;
        }

        if (capture.stopReason === "finalize") {
            if (capture.text.trim()) await syncActiveCapture(capture.surveyId);
            capture.status = "completed";
            updateBufferFromCapture(capture);
            state.activeCapture = null;
            setSpeechStatus("Encuesta lista para validacion");
            renderSurveyCapture(capture.surveyId);
            if (capture.pendingFinalize) {
                capture.pendingFinalize = false;
                await completeSurveyFinalization(capture.surveyId);
            }
            return;
        }

        if (capture.status === "recording") {
            if (shouldUseManualMobileSpeech()) {
                if (capture.text.trim()) await syncActiveCapture(capture.surveyId);
                capture.status = "paused";
                capture.processing = false;
                capture.stopReason = null;
                setSpeechStatus("Dictado en pausa; toca iniciar para seguir grabando");
                renderSurveyCapture(capture.surveyId);
                return;
            }

            setSpeechStatus("Reconectando microfono...");
            capture.restartTimer = setTimeout(() => {
                if (!state.activeCapture || state.activeCapture.surveyId !== capture.surveyId || state.activeCapture.status !== "recording") return;
                try {
                    state.recognition.start();
                } catch {
                    capture.restartTimer = setTimeout(() => {
                        if (!state.activeCapture || state.activeCapture.surveyId !== capture.surveyId || state.activeCapture.status !== "recording") return;
                        try {
                            state.recognition.start();
                        } catch {
                            setSpeechStatus("Toca continuar para seguir grabando");
                            state.activeCapture.status = "paused";
                            state.activeCapture.processing = false;
                            renderSurveyCapture(capture.surveyId);
                        }
                    }, 900);
                }
            }, 250);
            renderSurveyCapture(capture.surveyId);
            return;
        }

        state.activeCapture.status = "idle";
        state.activeCapture.processing = false;
        state.activeCapture.stopReason = null;
        setSpeechStatus("Dictado listo");
        renderSurveyCapture(state.activeCapture.surveyId);
    };
}

function getAudioContext() {
    if (state.audioContext) return state.audioContext;
    const AudioCtor = window.AudioContext || window.webkitAudioContext;
    if (!AudioCtor) return null;
    state.audioContext = new AudioCtor();
    return state.audioContext;
}

function playCaptureTone(kind) {
    const context = getAudioContext();
    if (!context) return;

    const patterns = {
        start: [620, 780],
        pause: [460],
        continue: [520, 680],
        stop: [380, 260],
        focus: [700]
    };
    const frequencies = patterns[kind] ?? [520];
    let offset = 0;
    frequencies.forEach((frequency, index) => {
        const oscillator = context.createOscillator();
        const gain = context.createGain();
        oscillator.type = index % 2 === 0 ? "sine" : "triangle";
        oscillator.frequency.value = frequency;
        gain.gain.setValueAtTime(0.0001, context.currentTime + offset);
        gain.gain.exponentialRampToValueAtTime(0.06, context.currentTime + offset + 0.01);
        gain.gain.exponentialRampToValueAtTime(0.0001, context.currentTime + offset + 0.12);
        oscillator.connect(gain);
        gain.connect(context.destination);
        oscillator.start(context.currentTime + offset);
        oscillator.stop(context.currentTime + offset + 0.14);
        offset += 0.09;
    });
}
