import { API_BASE_URL } from "./api";
import type { AuthProviderInfo, AuthResponse, AuthStatus } from "./types";

function authUrl(path: string) {
  return `${API_BASE_URL}${path}`;
}

async function authRequest<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(authUrl(path), {
    headers: {
      "Content-Type": "application/json",
      ...(options?.headers ?? {})
    },
    ...options
  });

  if (!response.ok) {
    throw new Error(`Auth request failed (${response.status})`);
  }

  return response.json() as Promise<T>;
}

export const authApi = {
  status: () => authRequest<AuthStatus>("/api/auth/status"),
  providers: () => authRequest<AuthProviderInfo[]>("/api/auth/providers"),
  signIn: (email: string, password: string) =>
    authRequest<AuthResponse>("/api/auth/signin", {
      method: "POST",
      body: JSON.stringify({ email, password })
    }),
  signUp: (email: string, password: string, name?: string) =>
    authRequest<AuthResponse>("/api/auth/signup", {
      method: "POST",
      body: JSON.stringify({ email, password, name })
    }),
  signOut: () =>
    authRequest<AuthResponse>("/api/auth/signout", {
      method: "POST"
    })
};
