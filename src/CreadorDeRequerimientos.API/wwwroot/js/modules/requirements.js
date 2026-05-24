import { api } from "../core/api.js";
import { $, escapeHtml, readCheckedValues } from "../core/dom.js";
import { state } from "../core/state.js";
import { getAppHandlers } from "../core/app-context.js";

export function renderRequirements() {
    const list = $("#requirementList");
    list.innerHTML = "";
    const template = $("#requirementListItemTemplate");

    state.currentProject.requirements.forEach(requirement => {
        const node = template.content.firstElementChild.cloneNode(true);
        node.dataset.requirementId = requirement.id;
        node.classList.toggle("active", requirement.id === state.currentRequirementId);
        node.querySelector(".requirement-item-name").textContent = requirement.title;
        node.querySelector(".requirement-item-meta").textContent = `${requirement.stage} - ${requirement.surveyIds.length} encuestas`;
        node.querySelector(".requirement-item-extra").textContent = `${requirement.relatedRequirementIds.length} requerimientos relacionados`;
        node.addEventListener("click", () => selectRequirement(requirement.id));
        list.appendChild(node);
    });

    selectRequirement(state.currentRequirementId);
}

export function selectRequirement(id) {
    state.currentRequirementId = id;
    document.querySelectorAll(".requirement-list-item").forEach(node => {
        node.classList.toggle("active", node.dataset.requirementId === id);
    });
    const requirement = state.currentProject?.requirements.find(item => item.id === id);
    $("#requirementStage").value = requirement?.stage ?? "toma";
    $("#requirementTitle").value = requirement?.title ?? "";
    $("#requirementSummary").value = requirement?.summary ?? "";
    $("#requirementContent").value = requirement?.content ?? "";
    renderRequirementSurveyLinks(requirement);
    renderRelatedRequirementLinks(requirement);
}

function renderRequirementSurveyLinks(requirement) {
    const container = $("#requirementSurveyLinks");
    container.innerHTML = "";

    state.currentProject.surveys.forEach(survey => {
        const item = document.createElement("label");
        item.className = "check-item";
        const input = document.createElement("input");
        input.type = "checkbox";
        input.value = survey.id;
        input.checked = Boolean(requirement?.surveyIds?.includes(survey.id));
        const text = document.createElement("div");
        text.innerHTML = `<strong>${escapeHtml(survey.title)}</strong><small>${escapeHtml(survey.interviewee || "Sin entrevistado")}</small>`;
        item.appendChild(input);
        item.appendChild(text);
        container.appendChild(item);
    });
}

function renderRelatedRequirementLinks(requirement) {
    const container = $("#relatedRequirementLinks");
    container.innerHTML = "";

    state.currentProject.requirements
        .filter(item => item.id !== requirement?.id)
        .forEach(item => {
            const node = document.createElement("label");
            node.className = "check-item";
            const input = document.createElement("input");
            input.type = "checkbox";
            input.value = item.id;
            input.checked = Boolean(requirement?.relatedRequirementIds?.includes(item.id));
            const text = document.createElement("div");
            text.innerHTML = `<strong>${escapeHtml(item.title)}</strong><small>${escapeHtml(item.stage)}</small>`;
            node.appendChild(input);
            node.appendChild(text);
            container.appendChild(node);
        });
}

export async function saveRequirement(event) {
    event.preventDefault();
    if (!state.currentProject) {
        return;
    }

    state.currentProject = await api.saveRequirement(state.currentProject.id, {
        id: state.currentRequirementId,
        stage: $("#requirementStage").value,
        title: $("#requirementTitle").value,
        summary: $("#requirementSummary").value,
        content: $("#requirementContent").value,
        surveyIds: readCheckedValues("#requirementSurveyLinks"),
        relatedRequirementIds: readCheckedValues("#relatedRequirementLinks")
    });
    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    if (!state.currentRequirementId) {
        state.currentRequirementId = state.currentProject.requirements[0]?.id ?? null;
    }
    getAppHandlers().renderApp();
    getAppHandlers().selectTab("requirements");
}

export async function createDraftRequirement() {
    if (!state.currentProject) {
        return;
    }

    state.currentProject = await api.createDraft(state.currentProject.id, {
        stage: "toma",
        title: `Borrador - ${state.currentProject.featureName || state.currentProject.name}`,
        requirementId: state.currentRequirementId
    });
    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    state.currentRequirementId = state.currentRequirementId ?? state.currentProject.requirements[0]?.id ?? null;
    getAppHandlers().renderApp();
    getAppHandlers().selectTab("requirements");
}

export async function deleteRequirement() {
    if (!state.currentProject || !state.currentRequirementId || !confirm("Eliminar este requerimiento?")) {
        return;
    }

    await api.deleteRequirement(state.currentProject.id, state.currentRequirementId);
    await getAppHandlers().loadWorkspace();
    await getAppHandlers().refreshCurrentProject();
    state.currentRequirementId = state.currentProject?.requirements[0]?.id ?? null;
    getAppHandlers().renderApp();
}
