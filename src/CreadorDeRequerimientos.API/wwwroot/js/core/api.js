export const api = {
    unauthorizedHandler: null,
    errorHandler: null,
    setUnauthorizedHandler(handler) {
        api.unauthorizedHandler = handler;
    },
    setErrorHandler(handler) {
        api.errorHandler = handler;
    },
    async request(path, options = {}) {
        const response = await fetch(path, {
            credentials: "same-origin",
            headers: { "Content-Type": "application/json" },
            ...options
        });

        if (!response.ok) {
            const error = new Error(`Error HTTP ${response.status}`);
            error.status = response.status;
            if (response.status === 401 && typeof api.unauthorizedHandler === "function") {
                api.unauthorizedHandler();
            }
            if (typeof api.errorHandler === "function") {
                api.errorHandler(error, path);
            }
            throw error;
        }

        return response.status === 204 ? null : response.json();
    },
    getAuthStatus: () => api.request("/api/auth/status"),
    login: body => api.request("/api/auth/login", { method: "POST", body: JSON.stringify(body) }),
    logout: () => api.request("/api/auth/logout", { method: "POST" }),
    getProjects: () => api.request("/api/projects"),
    getProject: id => api.request(`/api/projects/${id}`),
    createProject: body => api.request("/api/projects", { method: "POST", body: JSON.stringify(body) }),
    updateProject: (id, body) => api.request(`/api/projects/${id}`, { method: "PUT", body: JSON.stringify(body) }),
    deleteProject: id => api.request(`/api/projects/${id}`, { method: "DELETE" }),
    getSystemTemplates: () => api.request("/api/templates/system"),
    createSystemTemplate: body => api.request("/api/templates/system", { method: "POST", body: JSON.stringify(body) }),
    updateSystemTemplate: (id, body) => api.request(`/api/templates/system/${id}`, { method: "PUT", body: JSON.stringify(body) }),
    deleteSystemTemplate: id => api.request(`/api/templates/system/${id}`, { method: "DELETE" }),
    createProjectTemplate: (projectId, body) => api.request(`/api/projects/${projectId}/templates`, { method: "POST", body: JSON.stringify(body) }),
    updateProjectTemplate: (projectId, templateId, body) => api.request(`/api/projects/${projectId}/templates/${templateId}`, { method: "PUT", body: JSON.stringify(body) }),
    deleteProjectTemplate: (projectId, templateId) => api.request(`/api/projects/${projectId}/templates/${templateId}`, { method: "DELETE" }),
    exportProjectTemplate: (projectId, templateId) => api.request(`/api/projects/${projectId}/templates/${templateId}/export`, { method: "POST" }),
    createSurvey: (projectId, body) => api.request(`/api/projects/${projectId}/surveys`, { method: "POST", body: JSON.stringify(body) }),
    updateSurvey: (projectId, surveyId, body) => api.request(`/api/projects/${projectId}/surveys/${surveyId}`, { method: "PUT", body: JSON.stringify(body) }),
    deleteSurvey: (projectId, surveyId) => api.request(`/api/projects/${projectId}/surveys/${surveyId}`, { method: "DELETE" }),
    createParticipant: (projectId, surveyId, body) => api.request(`/api/projects/${projectId}/surveys/${surveyId}/participants`, { method: "POST", body: JSON.stringify(body) }),
    renameParticipant: (projectId, surveyId, participantId, body) => api.request(`/api/projects/${projectId}/surveys/${surveyId}/participants/${participantId}`, { method: "PUT", body: JSON.stringify(body) }),
    deleteParticipant: (projectId, surveyId, participantId) => api.request(`/api/projects/${projectId}/surveys/${surveyId}/participants/${participantId}`, { method: "DELETE" }),
    addTurn: (projectId, surveyId, body) => api.request(`/api/projects/${projectId}/surveys/${surveyId}/turns`, { method: "POST", body: JSON.stringify(body) }),
    updateTurn: (projectId, surveyId, turnId, body) => api.request(`/api/projects/${projectId}/surveys/${surveyId}/turns/${turnId}`, { method: "PUT", body: JSON.stringify(body) }),
    deleteTurn: (projectId, surveyId, turnId) => api.request(`/api/projects/${projectId}/surveys/${surveyId}/turns/${turnId}`, { method: "DELETE" }),
    saveRequirement: (projectId, body) => api.request(`/api/projects/${projectId}/requirements`, { method: "POST", body: JSON.stringify(body) }),
    deleteRequirement: (projectId, requirementId) => api.request(`/api/projects/${projectId}/requirements/${requirementId}`, { method: "DELETE" }),
    createDraft: (projectId, body) => api.request(`/api/projects/${projectId}/requirements/draft`, { method: "POST", body: JSON.stringify(body) })
};
