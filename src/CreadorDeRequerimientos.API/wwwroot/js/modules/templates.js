import { api } from "../core/api.js";
import { $ , escapeHtml } from "../core/dom.js";
import { state } from "../core/state.js";
import { getAppHandlers } from "../core/app-context.js";

export function renderSystemTemplatesPanel() {
    if (!state.selectedSystemTemplateId && state.systemTemplates.length > 0) {
        state.selectedSystemTemplateId = state.systemTemplates[0].id;
    }

    renderTemplateList($("#systemTemplateList"), state.systemTemplates, state.selectedSystemTemplateId, templateId => {
        state.selectedSystemTemplateId = templateId;
        renderSystemTemplateEditor();
    });
    renderSystemTemplateEditor();
}

export function renderProjectTemplatesTab() {
    if (!state.selectedProjectTemplateId && state.currentProject.projectTemplates.length > 0) {
        state.selectedProjectTemplateId = state.currentProject.projectTemplates[0].id;
    }

    renderTemplateList($("#projectTemplateList"), state.currentProject.projectTemplates, state.selectedProjectTemplateId, templateId => {
        state.selectedProjectTemplateId = templateId;
        renderProjectTemplateEditor();
    });
    renderTemplatePreviewList($("#projectSystemTemplateList"), state.systemTemplates);
    renderProjectTemplateEditor();
}

function renderTemplateList(container, templates, selectedId, onSelect) {
    const template = $("#templateListItemTemplate");
    container.innerHTML = "";

    templates.forEach(item => {
        const node = template.content.firstElementChild.cloneNode(true);
        node.classList.toggle("active", item.id === selectedId);
        node.querySelector(".template-item-name").textContent = item.name;
        node.querySelector(".template-item-meta").textContent = `${item.interviewSections.length} secciones - ${item.minuteSections.length} bloques de minuta`;
        node.addEventListener("click", () => onSelect(item.id));
        container.appendChild(node);
    });
}

function renderTemplatePreviewList(container, templates) {
    container.innerHTML = "";

    templates.forEach(template => {
        const card = document.createElement("div");
        card.className = "template-preview-card";
        card.innerHTML = `
            <h4>${escapeHtml(template.name)}</h4>
            <p>${escapeHtml(template.description || "Sin descripcion")}</p>
        `;
        container.appendChild(card);
    });
}

export function renderSystemTemplateEditor() {
    const template = state.systemTemplates.find(item => item.id === state.selectedSystemTemplateId);
    fillTemplateEditor({
        nameInput: $("#systemTemplateName"),
        descriptionInput: $("#systemTemplateDescription"),
        interviewContainer: $("#systemInterviewSections"),
        minuteContainer: $("#systemMinuteSections"),
        template
    });
}

export function renderProjectTemplateEditor() {
    const template = state.currentProject?.projectTemplates.find(item => item.id === state.selectedProjectTemplateId);
    fillTemplateEditor({
        nameInput: $("#projectTemplateName"),
        descriptionInput: $("#projectTemplateDescription"),
        interviewContainer: $("#projectInterviewSections"),
        minuteContainer: $("#projectMinuteSections"),
        template
    });
}

function fillTemplateEditor({ nameInput, descriptionInput, interviewContainer, minuteContainer, template }) {
    nameInput.value = template?.name ?? "";
    descriptionInput.value = template?.description ?? "";
    interviewContainer.innerHTML = "";
    minuteContainer.innerHTML = "";

    const interviewSections = template?.interviewSections ?? [];
    const minuteSections = template?.minuteSections ?? [];

    if (interviewSections.length === 0) {
        appendInterviewSection(interviewContainer);
    } else {
        interviewSections.forEach(section => appendInterviewSection(interviewContainer, section));
    }

    if (minuteSections.length === 0) {
        appendMinuteSection(minuteContainer);
    } else {
        minuteSections.forEach(section => appendMinuteSection(minuteContainer, section));
    }
}

export function appendInterviewSection(container, section = null) {
    const node = $("#interviewSectionTemplate").content.firstElementChild.cloneNode(true);
    node.querySelector(".section-title-input").value = section?.title ?? "";
    node.querySelector(".section-prompt-input").value = section?.prompt ?? "";
    node.querySelector(".remove-section-button").addEventListener("click", () => node.remove());
    node.querySelector(".add-question-button").addEventListener("click", () => appendQuestion(node.querySelector(".questions-list")));

    const questionList = node.querySelector(".questions-list");
    if (section?.questions?.length) {
        section.questions.forEach(question => appendQuestion(questionList, question));
    } else {
        appendQuestion(questionList);
    }

    container.appendChild(node);
}

function appendQuestion(container, value = "") {
    const node = $("#questionTemplate").content.firstElementChild.cloneNode(true);
    node.querySelector(".question-input").value = value;
    node.querySelector(".remove-question-button").addEventListener("click", () => node.remove());
    container.appendChild(node);
}

export function appendMinuteSection(container, section = null) {
    const node = $("#minuteSectionTemplate").content.firstElementChild.cloneNode(true);
    node.querySelector(".minute-title-input").value = section?.title ?? "";
    node.querySelector(".minute-prompt-input").value = section?.prompt ?? "";
    node.querySelector(".remove-minute-section-button").addEventListener("click", () => node.remove());
    container.appendChild(node);
}

function readTemplateEditor(interviewContainer, minuteContainer, nameInput, descriptionInput) {
    return {
        name: nameInput.value,
        description: descriptionInput.value,
        interviewSections: [...interviewContainer.children].map(section => ({
            title: section.querySelector(".section-title-input").value,
            prompt: section.querySelector(".section-prompt-input").value,
            questions: [...section.querySelectorAll(".question-input")]
                .map(question => question.value)
                .filter(question => question.trim().length > 0)
        })),
        minuteSections: [...minuteContainer.children].map(section => ({
            title: section.querySelector(".minute-title-input").value,
            prompt: section.querySelector(".minute-prompt-input").value
        }))
    };
}

export async function saveSystemTemplate(event) {
    event.preventDefault();
    const body = readTemplateEditor($("#systemInterviewSections"), $("#systemMinuteSections"), $("#systemTemplateName"), $("#systemTemplateDescription"));
    if (state.selectedSystemTemplateId) {
        await api.updateSystemTemplate(state.selectedSystemTemplateId, body);
    } else {
        const template = await api.createSystemTemplate(body);
        state.selectedSystemTemplateId = template.id;
    }

    await getAppHandlers().loadWorkspace();
    getAppHandlers().renderApp();
}

export async function deleteSystemTemplate() {
    if (!state.selectedSystemTemplateId || !confirm("Eliminar esta plantilla del sistema?")) {
        return;
    }

    await api.deleteSystemTemplate(state.selectedSystemTemplateId);
    state.selectedSystemTemplateId = null;
    await getAppHandlers().loadWorkspace();
    getAppHandlers().renderApp();
}

export async function saveProjectTemplate(event) {
    event.preventDefault();
    if (!state.currentProject) {
        return;
    }

    const body = readTemplateEditor($("#projectInterviewSections"), $("#projectMinuteSections"), $("#projectTemplateName"), $("#projectTemplateDescription"));
    if (state.selectedProjectTemplateId) {
        state.currentProject = await api.updateProjectTemplate(state.currentProject.id, state.selectedProjectTemplateId, body);
    } else {
        state.currentProject = await api.createProjectTemplate(state.currentProject.id, body);
        state.selectedProjectTemplateId =
            state.currentProject.projectTemplates.find(template => template.name === body.name && template.description === body.description)?.id
            ?? state.currentProject.projectTemplates[0]?.id
            ?? null;
    }

    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    getAppHandlers().renderApp();
}

export async function deleteProjectTemplate() {
    if (!state.currentProject || !state.selectedProjectTemplateId || !confirm("Eliminar esta plantilla del proyecto?")) {
        return;
    }

    state.currentProject = await api.deleteProjectTemplate(state.currentProject.id, state.selectedProjectTemplateId);
    state.selectedProjectTemplateId = null;
    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    getAppHandlers().renderApp();
}

export async function exportProjectTemplate() {
    if (!state.currentProject || !state.selectedProjectTemplateId) {
        return;
    }

    await api.exportProjectTemplate(state.currentProject.id, state.selectedProjectTemplateId);
    await getAppHandlers().loadWorkspace();
    getAppHandlers().renderApp();
}
