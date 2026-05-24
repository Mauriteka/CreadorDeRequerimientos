export const state = {
    projects: [],
    systemTemplates: [],
    currentProject: null,
    auth: {
        enabled: false,
        isAuthenticated: false,
        username: null
    },
    currentRequirementId: null,
    recognition: null,
    audioContext: null,
    activeCapture: null,
    captureBuffers: {},
    captureRestartAttempts: 0,
    activeView: "home",
    surveyGuideState: {},
    selectedSystemTemplateId: null,
    selectedProjectTemplateId: null,
    reviewFocusSurveyId: null
};
