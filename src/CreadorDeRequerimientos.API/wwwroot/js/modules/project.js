import { api } from "../core/api.js";
import { $ } from "../core/dom.js";
import { state } from "../core/state.js";
import { getAppHandlers } from "../core/app-context.js";

export function renderProjectList() {
    const list = $("#projectList");
    const template = $("#projectItemTemplate");
    list.innerHTML = "";

    state.projects.forEach(project => {
        const node = template.content.firstElementChild.cloneNode(true);
        node.classList.toggle("active", project.id === state.currentProject?.id && state.activeView === "project");
        node.querySelector(".project-name").textContent = project.name;
        node.querySelector("small").textContent = `${project.surveyCount} encuestas - ${project.projectTemplateCount} plantillas`;
        node.addEventListener("click", async () => {
            await openProject(project.id);
        });
        list.appendChild(node);
    });
}

export async function openProject(projectId) {
    state.currentProject = await api.getProject(projectId);
    state.currentRequirementId = state.currentProject.requirements[0]?.id ?? null;
    if (!state.selectedProjectTemplateId && state.currentProject.projectTemplates.length > 0) {
        state.selectedProjectTemplateId = state.currentProject.projectTemplates[0].id;
    }
    state.activeView = "project";
    getAppHandlers().renderApp();
}

export async function createProject() {
    const project = await api.createProject({
        name: "Nuevo proyecto",
        featureName: "Funcionalidad por definir",
        notes: ""
    });
    await getAppHandlers().loadWorkspace();
    await openProject(project.id);
}

export async function saveCurrentProject() {
    if (!state.currentProject) {
        return;
    }

    state.currentProject = await api.updateProject(state.currentProject.id, {
        name: $("#projectName").value,
        featureName: $("#featureName").value,
        notes: $("#projectNotes").value
    });
    await getAppHandlers().loadWorkspace();
    getAppHandlers().renderApp();
}

export async function deleteCurrentProject() {
    if (!state.currentProject || !confirm("Eliminar este proyecto y sus encuestas?")) {
        return;
    }

    await api.deleteProject(state.currentProject.id);
    state.currentProject = null;
    state.currentRequirementId = null;
    state.selectedProjectTemplateId = null;
    await getAppHandlers().loadWorkspace();
    getAppHandlers().setActiveView("home");
}
