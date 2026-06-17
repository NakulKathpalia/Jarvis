"use client";

import { useEffect, useState } from "react";
import { jarvisApi } from "@/lib/api";
import type { VoiceCommandCatalogItem } from "@/lib/types";

type VoiceCommandHelpPanelProps = {
  onToast: (message: string) => void;
};

export function VoiceCommandHelpPanel({
  onToast
}: VoiceCommandHelpPanelProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [commands, setCommands] = useState<VoiceCommandCatalogItem[]>([]);

  useEffect(() => {
    jarvisApi
      .voiceCommands()
      .then(setCommands)
      .catch((error: Error) => onToast(error.message));
  }, [onToast]);

  return (
    <section className={isOpen ? "voice-command-help open" : "voice-command-help"}>
      <button
        className="voice-command-help-toggle"
        type="button"
        onClick={() => setIsOpen((current) => !current)}
      >
        <span>Voice Commands</span>
        <strong>{isOpen ? "Hide" : "Show"}</strong>
      </button>

      {isOpen && (
        <div className="voice-command-help-list">
          {commands.map((command) => (
            <article className="voice-command-help-item" key={command.category}>
              <div>
                <strong>{command.category}</strong>
                <p>{command.description}</p>
              </div>
              <div className="voice-command-examples">
                {command.examples.map((example) => (
                  <span className="voice-command-example" key={example}>
                    {example}
                  </span>
                ))}
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
