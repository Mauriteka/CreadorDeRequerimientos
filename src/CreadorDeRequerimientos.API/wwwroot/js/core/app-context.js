const handlers = {
    loadWorkspace: async () => {},
    refreshCurrentProject: async () => {},
    renderApp: () => {},
    setActiveView: () => {},
    selectTab: () => {}
};

export function registerAppHandlers(nextHandlers) {
    Object.assign(handlers, nextHandlers);
}

export function getAppHandlers() {
    return handlers;
}
