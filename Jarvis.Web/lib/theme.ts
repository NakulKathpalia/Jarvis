export enum ThemeMode {
  System = "system",
  Light = "light",
  Dark = "dark"
}

const storageKey = "jarvis-theme-mode";

export const themeOptions = [
  { value: ThemeMode.System, label: "Use system setting" },
  { value: ThemeMode.Light, label: "Light" },
  { value: ThemeMode.Dark, label: "Dark" }
] as const;

export class ThemeService {
  static getInitialMode(): ThemeMode {
    if (typeof window === "undefined") {
      return ThemeMode.System;
    }

    const stored = window.localStorage.getItem(storageKey);
    return Object.values(ThemeMode).includes(stored as ThemeMode)
      ? (stored as ThemeMode)
      : ThemeMode.System;
  }

  static apply(mode: ThemeMode) {
    if (typeof window === "undefined") {
      return;
    }

    window.localStorage.setItem(storageKey, mode);
    const resolved = mode === ThemeMode.System && window.matchMedia("(prefers-color-scheme: light)").matches
      ? ThemeMode.Light
      : mode === ThemeMode.System
        ? ThemeMode.Dark
        : mode;

    document.documentElement.dataset.theme = resolved;
    document.documentElement.dataset.themeMode = mode;
  }
}
