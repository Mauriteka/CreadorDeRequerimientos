export const $ = selector => document.querySelector(selector);

export function formatTime(value) {
    return new Date(value).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", second: "2-digit" });
}

export function escapeAttribute(value) {
    return String(value).replace(/[&<>"']/g, character => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        "\"": "&quot;",
        "'": "&#039;"
    }[character]));
}

export function escapeHtml(value) {
    return String(value).replace(/[&<>"']/g, character => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        "\"": "&quot;",
        "'": "&#039;"
    }[character]));
}

export function readCheckedValues(selector) {
    return [...document.querySelectorAll(`${selector} input[type="checkbox"]:checked`)].map(input => input.value);
}

export function getParticipantTheme(survey, speakerId) {
    const isDark = document.documentElement.dataset.theme === "dark";
    const participants = Array.isArray(survey?.participants) ? survey.participants.filter(Boolean) : [];
    const palette = isDark
        ? [
            { accent: "#27b3a9", soft: "rgba(39, 179, 169, 0.14)", dark: "#91e6df" },
            { accent: "#4f8cff", soft: "rgba(79, 140, 255, 0.14)", dark: "#bfd4ff" },
            { accent: "#d18a3d", soft: "rgba(209, 138, 61, 0.14)", dark: "#f0c896" },
            { accent: "#a78bfa", soft: "rgba(167, 139, 250, 0.14)", dark: "#ddd2ff" },
            { accent: "#ef5da8", soft: "rgba(239, 93, 168, 0.14)", dark: "#f9b5d6" }
        ]
        : [
            { accent: "#0f766e", soft: "#dff5f1", dark: "#0b5b56" },
            { accent: "#2563eb", soft: "#dbeafe", dark: "#1d4ed8" },
            { accent: "#b45309", soft: "#fef3c7", dark: "#92400e" },
            { accent: "#7c3aed", soft: "#ede9fe", dark: "#5b21b6" },
            { accent: "#be185d", soft: "#fce7f3", dark: "#9d174d" }
        ];
    const participant = participants.find(item => item.id === speakerId) ?? participants[0];
    const index = Math.max(0, participants.findIndex(item => item.id === participant?.id));
    return participant?.roleType === "Self" ? palette[0] : palette[(index % (palette.length - 1)) + 1];
}

export function applyThemeVariables(element, theme) {
    if (!element || !theme) {
        return;
    }

    element.style.setProperty("--participant-accent", theme.accent);
    element.style.setProperty("--participant-soft", theme.soft);
    element.style.setProperty("--participant-dark", theme.dark);
}
