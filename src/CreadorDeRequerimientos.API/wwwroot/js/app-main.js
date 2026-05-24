import { $ } from "./core/dom.js";
import { state } from "./core/state.js";
import { api } from "./core/api.js";
import { registerAppHandlers } from "./core/app-context.js";
import { createProject, deleteCurrentProject, openProject, renderProjectList, saveCurrentProject } from "./modules/project.js";
import { createDraftRequirement, deleteRequirement, renderRequirements, selectRequirement, saveRequirement } from "./modules/requirements.js";
import { createSurvey, renderSurveyTemplateSelect, renderSurveys, setupSpeech } from "./modules/surveys.js?v=20260524-android-cache";
import {
    appendInterviewSection,
    appendMinuteSection,
    deleteProjectTemplate,
    deleteSystemTemplate,
    exportProjectTemplate,
    renderProjectTemplateEditor,
    renderProjectTemplatesTab,
    renderSystemTemplateEditor,
    renderSystemTemplatesPanel,
    saveProjectTemplate,
    saveSystemTemplate
} from "./modules/templates.js";

const THEME_STORAGE_KEY = "cr-theme";

document.addEventListener("DOMContentLoaded", async () => {
    applyStoredTheme();
    registerAppHandlers({
        loadWorkspace,
        refreshCurrentProject,
        renderApp,
        setActiveView,
        selectTab
    });

    api.setUnauthorizedHandler(handleUnauthorized);
    api.setErrorHandler(showApiError);
    wireEvents();
    setupSpeech();
    try {
        await bootstrapApp();
    } catch (error) {
        showApiError(error, "inicio");
    }
    renderApp();
});

function wireEvents() {
    $("#newProjectButton").addEventListener("click", createProject);
    $("#systemTemplatesButton").addEventListener("click", () => setActiveView("systemTemplates"));
    $("#themeToggleButton").addEventListener("click", toggleTheme);
    $("#saveProjectButton").addEventListener("click", saveCurrentProject);
    $("#deleteProjectButton").addEventListener("click", deleteCurrentProject);
    $("#newSurveyButton").addEventListener("click", createSurvey);
    $("#newRequirementButton").addEventListener("click", () => selectRequirement(null));
    $("#draftRequirementButton").addEventListener("click", createDraftRequirement);
    $("#deleteRequirementButton").addEventListener("click", deleteRequirement);
    $("#requirementForm").addEventListener("submit", saveRequirement);
    $("#loginForm").addEventListener("submit", handleLoginSubmit);
    $("#logoutButton").addEventListener("click", handleLogout);

    $("#newSystemTemplateButton").addEventListener("click", () => {
        state.selectedSystemTemplateId = null;
        renderSystemTemplateEditor();
    });
    $("#systemTemplateForm").addEventListener("submit", saveSystemTemplate);
    $("#deleteSystemTemplateButton").addEventListener("click", deleteSystemTemplate);
    $("#resetSystemTemplateButton").addEventListener("click", () => {
        state.selectedSystemTemplateId = null;
        renderSystemTemplateEditor();
    });
    $("#addSystemInterviewSectionButton").addEventListener("click", () => {
        appendInterviewSection($("#systemInterviewSections"));
    });
    $("#addSystemMinuteSectionButton").addEventListener("click", () => {
        appendMinuteSection($("#systemMinuteSections"));
    });

    $("#newProjectTemplateButton").addEventListener("click", () => {
        state.selectedProjectTemplateId = null;
        renderProjectTemplateEditor();
    });
    $("#projectTemplateForm").addEventListener("submit", saveProjectTemplate);
    $("#deleteProjectTemplateButton").addEventListener("click", deleteProjectTemplate);
    $("#resetProjectTemplateButton").addEventListener("click", () => {
        state.selectedProjectTemplateId = null;
        renderProjectTemplateEditor();
    });
    $("#exportProjectTemplateButton").addEventListener("click", exportProjectTemplate);
    $("#addProjectInterviewSectionButton").addEventListener("click", () => {
        appendInterviewSection($("#projectInterviewSections"));
    });
    $("#addProjectMinuteSectionButton").addEventListener("click", () => {
        appendMinuteSection($("#projectMinuteSections"));
    });

    document.querySelectorAll(".tab").forEach(tab => {
        tab.addEventListener("click", () => selectTab(tab.dataset.tab));
    });

    wireEnterToSave();
}

async function bootstrapApp() {
    await refreshAuthState();
    if (!state.auth.enabled || state.auth.isAuthenticated) {
        await loadWorkspace();
    }
}

function applyStoredTheme() {
    const storedTheme = localStorage.getItem(THEME_STORAGE_KEY);
    const resolvedTheme = storedTheme || (window.matchMedia?.("(prefers-color-scheme: dark)").matches ? "dark" : "light");
    document.documentElement.dataset.theme = resolvedTheme;
    updateThemeToggleLabel(resolvedTheme);
}

function toggleTheme() {
    const currentTheme = document.documentElement.dataset.theme === "dark" ? "dark" : "light";
    const nextTheme = currentTheme === "dark" ? "light" : "dark";
    document.documentElement.dataset.theme = nextTheme;
    localStorage.setItem(THEME_STORAGE_KEY, nextTheme);
    updateThemeToggleLabel(nextTheme);
}

function updateThemeToggleLabel(theme) {
    const button = $("#themeToggleButton");
    if (!button) {
        return;
    }

    button.textContent = theme === "dark" ? "Modo claro" : "Modo oscuro";
}

async function loadWorkspace() {
    clearApiError();
    if (state.auth.enabled && !state.auth.isAuthenticated) {
        state.projects = [];
        state.systemTemplates = [];
        state.currentProject = null;
        return;
    }

    const [projects, systemTemplates] = await Promise.all([
        api.getProjects(),
        api.getSystemTemplates()
    ]);

    state.projects = projects;
    state.systemTemplates = systemTemplates;

    if (state.currentProject) {
        try {
            state.currentProject = await api.getProject(state.currentProject.id);
        } catch {
            state.currentProject = null;
        }
    }

    if (!state.currentProject && state.activeView === "project") {
        state.activeView = "home";
    }
}

async function refreshCurrentProject(projectId = state.currentProject?.id) {
    if (!projectId) {
        state.currentProject = null;
        return;
    }

    state.currentProject = await api.getProject(projectId);
}

function setActiveView(view) {
    state.activeView = view;
    renderApp();
}

function renderApp() {
    const requiresLogin = state.auth.enabled && !state.auth.isAuthenticated;
    document.body.classList.toggle("auth-locked", requiresLogin);
    $("#authGate").classList.toggle("hidden", !requiresLogin);
    $("#logoutButton").classList.toggle("hidden", !state.auth.enabled || !state.auth.isAuthenticated);
    renderAuthStatusBanner();

    if (requiresLogin) {
        $("#homePanel").classList.add("hidden");
        $("#systemTemplatesPanel").classList.add("hidden");
        $("#projectPanel").classList.add("hidden");
        renderProjectList();
        return;
    }

    renderProjectList();
    renderSurveyTemplateSelect();
    $("#homePanel").classList.toggle("hidden", state.activeView !== "home");
    $("#systemTemplatesPanel").classList.toggle("hidden", state.activeView !== "systemTemplates");
    $("#projectPanel").classList.toggle("hidden", state.activeView !== "project" || !state.currentProject);

    if (state.activeView === "systemTemplates") {
        renderSystemTemplatesPanel();
    }

    if (state.activeView === "project" && state.currentProject) {
        renderProject();
    }
}

function renderProject() {
    $("#projectName").value = state.currentProject.name;
    $("#featureName").value = state.currentProject.featureName;
    $("#projectNotes").value = state.currentProject.notes;
    wireEnterToSave();
    renderSurveys();
    renderRequirements();
    renderProjectTemplatesTab();
}

function renderAuthStatusBanner() {
    const banner = $("#authStatusBanner");
    if (!banner) {
        return;
    }

    if (!state.auth.enabled) {
        banner.classList.add("hidden");
        banner.textContent = "";
        return;
    }

    banner.classList.remove("hidden");
    banner.textContent = state.auth.isAuthenticated
        ? `Sesion abierta como ${state.auth.username || "admin"}`
        : "La app requiere login para abrir datos y editar contenido.";
}

function showApiError(error, path) {
    const banner = $("#appErrorBanner");
    if (!banner) {
        return;
    }

    const detail = error?.status ? `Error HTTP ${error.status}` : "No pude conectar con el servidor";
    banner.textContent = `${detail}. Revisa tu sesion o intenta recargar la pagina. Ruta: ${path}`;
    banner.classList.remove("hidden");
}

function clearApiError() {
    const banner = $("#appErrorBanner");
    if (!banner) {
        return;
    }

    banner.textContent = "";
    banner.classList.add("hidden");
}

async function refreshAuthState() {
    const auth = await api.getAuthStatus();
    state.auth.enabled = Boolean(auth.enabled);
    state.auth.isAuthenticated = Boolean(auth.isAuthenticated);
    state.auth.username = auth.username || null;
}

async function handleLoginSubmit(event) {
    event.preventDefault();
    const loginError = $("#loginError");
    loginError.classList.add("hidden");
    loginError.textContent = "";

    try {
        await api.login({
            username: $("#loginUsername").value,
            password: $("#loginPassword").value
        });
        $("#loginPassword").value = "";
        await refreshAuthState();
        await loadWorkspace();
        renderApp();
    } catch (error) {
        loginError.textContent = error?.status === 401
            ? "Usuario o password incorrectos."
            : "No pude iniciar sesion. Intenta otra vez.";
        loginError.classList.remove("hidden");
    }
}

async function handleLogout() {
    await api.logout();
    state.auth.isAuthenticated = false;
    state.auth.username = null;
    state.projects = [];
    state.systemTemplates = [];
    state.currentProject = null;
    state.currentRequirementId = null;
    renderApp();
}

function handleUnauthorized() {
    if (!state.auth.enabled) {
        return;
    }

    state.auth.isAuthenticated = false;
    state.auth.username = null;
    state.projects = [];
    state.systemTemplates = [];
    state.currentProject = null;
    state.currentRequirementId = null;
    renderApp();
}

function wireEnterToSave() {
    bindSubmitOnEnter($("#projectName"), () => saveCurrentProject());
    bindSubmitOnEnter($("#featureName"), () => saveCurrentProject());
    bindSubmitOnCtrlEnter($("#projectNotes"), () => saveCurrentProject());
}

function bindSubmitOnEnter(element, handler) {
    if (!element || element.dataset.enterBound === "true") {
        return;
    }

    element.dataset.enterBound = "true";
    element.addEventListener("keydown", async event => {
        if (event.key !== "Enter" || event.shiftKey) {
            return;
        }

        event.preventDefault();
        await handler();
    });
}

function bindSubmitOnCtrlEnter(element, handler) {
    if (!element || element.dataset.ctrlEnterBound === "true") {
        return;
    }

    element.dataset.ctrlEnterBound = "true";
    element.addEventListener("keydown", async event => {
        if (event.key !== "Enter" || !event.ctrlKey) {
            return;
        }

        event.preventDefault();
        await handler();
    });
}

function selectTab(name) {
    document.querySelectorAll(".tab").forEach(tab => tab.classList.toggle("active", tab.dataset.tab === name));
    $("#surveysTab").classList.toggle("hidden", name !== "surveys");
    $("#requirementsTab").classList.toggle("hidden", name !== "requirements");
    $("#templatesTab").classList.toggle("hidden", name !== "templates");
}

export { openProject };
