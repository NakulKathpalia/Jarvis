"use client";

import { useEffect, useState } from "react";
import { connectedAppsApi } from "@/lib/connectedAppsApi";
import type { ConnectedAppInfo } from "@/lib/types";

type ConnectedAppsPanelProps = {
  onToast: (message: string) => void;
};

export function ConnectedAppsPanel({ onToast }: ConnectedAppsPanelProps) {
  const [apps, setApps] = useState<ConnectedAppInfo[]>([]);
  const [isBusyProvider, setIsBusyProvider] = useState<string | null>(null);

  async function refreshApps() {
    setApps(await connectedAppsApi.list());
  }

  useEffect(() => {
    refreshApps().catch((error: Error) => onToast(error.message));
  }, [onToast]);

  async function connect(provider: string) {
    setIsBusyProvider(provider);
    try {
      const result = await connectedAppsApi.connect(provider);
      onToast(result.message);
      await refreshApps();
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Connect failed");
    } finally {
      setIsBusyProvider(null);
    }
  }

  async function disconnect(provider: string) {
    setIsBusyProvider(provider);
    try {
      const result = await connectedAppsApi.disconnect(provider);
      onToast(result.message);
      await refreshApps();
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Disconnect failed");
    } finally {
      setIsBusyProvider(null);
    }
  }

  return (
    <section className="tool-panel">
      <div className="page-header">
        <h2>Connected Apps</h2>
        <p>Future app integrations live here. Current connections are placeholders and never create OAuth tokens.</p>
      </div>

      <div className="connected-app-grid">
        {apps.map((app) => (
          <article className="simple-card connected-app-card" key={app.id}>
            <div className="card-heading-row">
              <h3>{app.name}</h3>
              <span className={app.configured ? "badge online" : "badge"}>{app.configured ? "Configured" : "Not configured"}</span>
            </div>
            <p>{app.description}</p>
            <div className="status-stack">
              <strong>Status: {app.status}</strong>
              <span>Provider: {app.provider}</span>
            </div>
            <ul className="capability-list">
              {app.capabilities.map((capability) => (
                <li key={capability}>{capability}</li>
              ))}
            </ul>
            <div className="card-actions">
              <button
                className="primary-action"
                disabled={isBusyProvider === app.id}
                type="button"
                onClick={() => void connect(app.id)}
              >
                Connect
              </button>
              <button
                className="secondary-action"
                disabled={isBusyProvider === app.id}
                type="button"
                onClick={() => void disconnect(app.id)}
              >
                Disconnect
              </button>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}
