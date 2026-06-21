import { API_BASE_URL } from "./api";
import type { ConnectedAppConnectionResult, ConnectedAppInfo } from "./types";

function connectedAppsUrl(path: string) {
  return `${API_BASE_URL}${path}`;
}

async function connectedAppsRequest<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(connectedAppsUrl(path), {
    headers: {
      "Content-Type": "application/json",
      ...(options?.headers ?? {})
    },
    ...options
  });

  if (!response.ok) {
    throw new Error(`Connected app request failed (${response.status})`);
  }

  return response.json() as Promise<T>;
}

export const connectedAppsApi = {
  list: () => connectedAppsRequest<ConnectedAppInfo[]>("/api/connected-apps"),
  connect: (provider: string) =>
    connectedAppsRequest<ConnectedAppConnectionResult>(`/api/connected-apps/${encodeURIComponent(provider)}/connect`, {
      method: "POST"
    }),
  disconnect: (provider: string) =>
    connectedAppsRequest<ConnectedAppConnectionResult>(`/api/connected-apps/${encodeURIComponent(provider)}/disconnect`, {
      method: "POST"
    })
};
