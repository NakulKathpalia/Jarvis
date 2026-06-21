"use client";

import type { ViewKey } from "@/lib/types";

type ToolsPanelProps = {
  onChangeView: (view: ViewKey) => void;
};

const sections: Array<{
  title: string;
  description: string;
  items: Array<{ label: string; description: string; view: ViewKey }>;
}> = [
  {
    title: "General",
    description: "Appearance, model behavior, and local memory preferences.",
    items: [
      { label: "Appearance / Theme", description: "Theme and visual preferences.", view: "settings" },
      { label: "Model Settings", description: "Ollama URL, model, history, and prompt.", view: "settings" },
      { label: "Memory Settings", description: "Review and manage local memories.", view: "memory" }
    ]
  },
  {
    title: "Tools",
    description: "Local assistant capabilities that run through the backend.",
    items: [
      { label: "Voice", description: "Wake word, voice pipeline, and recent transcripts.", view: "voice" },
      { label: "Memory", description: "Search, add, edit, and delete saved local facts.", view: "memory" },
      { label: "PC Control", description: "Run safe commands through Jarvis Security.", view: "control" },
      { label: "Files", description: "Index and search local files.", view: "files" },
      { label: "Connected Apps", description: "Future Google, Microsoft, GitHub, and Discord connectors.", view: "connectedApps" }
    ]
  },
  {
    title: "Security",
    description: "Command safety, audit trail, and runtime health.",
    items: [
      { label: "Security Overview", description: "Security pipeline, confirmation, and risk state.", view: "security" },
      { label: "Audit / Activity Logs", description: "Recent chat, voice, command, and error events.", view: "activity" },
      { label: "Diagnostics", description: "Local runtime paths and service health.", view: "diagnostics" }
    ]
  },
  {
    title: "Account",
    description: "Placeholder auth and account setup.",
    items: [
      { label: "Auth", description: "Sign in or sign up with the local placeholder flow.", view: "auth" },
      { label: "Profile Placeholder", description: "Future account profile area.", view: "auth" }
    ]
  }
];

export function ToolsPanel({ onChangeView }: ToolsPanelProps) {
  return (
    <section className="tool-panel">
      <div className="page-header">
        <h2>Tools & Settings</h2>
        <p>Feature pages are grouped here so the sidebar can stay focused on conversations.</p>
      </div>

      <div className="tools-hub">
        {sections.map((section) => (
          <article className="simple-card tools-section-card" key={section.title}>
            <h3>{section.title}</h3>
            <p>{section.description}</p>
            <div className="tools-link-list">
              {section.items.map((item) => (
                <button type="button" key={item.label} onClick={() => onChangeView(item.view)}>
                  <strong>{item.label}</strong>
                  <span>{item.description}</span>
                </button>
              ))}
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}
