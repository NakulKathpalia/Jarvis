"use client";

import { FormEvent, useEffect, useState } from "react";
import { authApi } from "@/lib/authApi";
import type { AuthProviderInfo, AuthStatus } from "@/lib/types";

type AuthPanelProps = {
  onToast: (message: string) => void;
};

type AuthTab = "signin" | "signup";

export function AuthPanel({ onToast }: AuthPanelProps) {
  const [activeTab, setActiveTab] = useState<AuthTab>("signin");
  const [status, setStatus] = useState<AuthStatus | null>(null);
  const [providers, setProviders] = useState<AuthProviderInfo[]>([]);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [name, setName] = useState("");
  const [message, setMessage] = useState("");
  const [isBusy, setIsBusy] = useState(false);

  async function refreshAuth() {
    const [nextStatus, nextProviders] = await Promise.all([
      authApi.status(),
      authApi.providers()
    ]);
    setStatus(nextStatus);
    setProviders(nextProviders);
  }

  useEffect(() => {
    refreshAuth().catch((error: Error) => onToast(error.message));
  }, [onToast]);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsBusy(true);
    try {
      const result = activeTab === "signin"
        ? await authApi.signIn(email, password)
        : await authApi.signUp(email, password, name);
      setStatus(result.status);
      setMessage(result.message);
      onToast(result.message);
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Auth request failed");
    } finally {
      setIsBusy(false);
    }
  }

  async function signOut() {
    setIsBusy(true);
    try {
      const result = await authApi.signOut();
      setStatus(result.status);
      setMessage(result.message);
      onToast(result.message);
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Sign out failed");
    } finally {
      setIsBusy(false);
    }
  }

  const oauthProviders = providers.filter((provider) => provider.type !== "Local");

  return (
    <section className="tool-panel">
      <div className="page-header">
        <h2>Auth</h2>
        <p>Placeholder local authentication for UI testing. OAuth providers are intentionally disabled until configured.</p>
      </div>

      <div className="auth-layout">
        <article className="simple-card auth-card">
          <div className="auth-tabs">
            <button className={activeTab === "signin" ? "active" : ""} type="button" onClick={() => setActiveTab("signin")}>
              Sign in
            </button>
            <button className={activeTab === "signup" ? "active" : ""} type="button" onClick={() => setActiveTab("signup")}>
              Sign up
            </button>
          </div>

          <form className="auth-form" onSubmit={submit}>
            {activeTab === "signup" && (
              <label>
                <span>Name</span>
                <input autoComplete="name" value={name} onChange={(event) => setName(event.target.value)} />
              </label>
            )}
            <label>
              <span>Email</span>
              <input autoComplete="email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
            </label>
            <label>
              <span>Password</span>
              <input autoComplete={activeTab === "signin" ? "current-password" : "new-password"} type="password" value={password} onChange={(event) => setPassword(event.target.value)} />
            </label>
            <button className="primary-action" disabled={isBusy} type="submit">
              {activeTab === "signin" ? "Sign in" : "Sign up"}
            </button>
          </form>

          <div className="oauth-grid" aria-label="OAuth providers">
            {oauthProviders.map((provider) => (
              <button
                disabled
                key={provider.id}
                title={provider.message}
                type="button"
                onClick={() => onToast("OAuth is not configured yet.")}
              >
                Continue with {provider.name}
                <span>Not configured yet</span>
              </button>
            ))}
          </div>
        </article>

        <article className="simple-card auth-status-card">
          <h3>Auth Status</h3>
          <p>{status?.message ?? "Loading auth status..."}</p>
          <div className="status-stack">
            <strong>{status?.isAuthenticated ? "Signed in" : "Signed out"}</strong>
            {status?.user && <span>{status.user.name} ({status.user.email})</span>}
          </div>
          {status?.isAuthenticated && (
            <button className="secondary-action" disabled={isBusy} type="button" onClick={() => void signOut()}>
              Sign out
            </button>
          )}
          {message && <p className="muted-note">{message}</p>}
        </article>
      </div>
    </section>
  );
}
