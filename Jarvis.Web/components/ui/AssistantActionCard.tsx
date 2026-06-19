import { AssistantOrb } from "./AssistantOrb";
import { StatusBadge } from "./StatusBadge";

type AssistantActionCardProps = {
  title: string;
  message: string;
  state: "confirm" | "executing" | "result" | "error";
  command?: string;
  target?: string;
  disabled?: boolean;
  onConfirm?: () => void;
  onCancel?: () => void;
};

export function AssistantActionCard({
  title,
  message,
  state,
  command,
  target,
  disabled = false,
  onConfirm,
  onCancel
}: AssistantActionCardProps) {
  const tone = state === "error" ? "red" : state === "confirm" ? "amber" : "green";

  return (
    <article className="mx-auto flex w-full max-w-4xl justify-start px-4">
      <div className="grid w-full max-w-xl gap-4 rounded-[1.6rem] border border-jarvis-border bg-jarvis-card/95 p-5 shadow-card">
        <div className="flex items-start gap-4">
          <AssistantOrb size="sm" active={state === "executing"} label={state === "confirm" ? "?" : "J"} />
          <div className="min-w-0 flex-1">
            <div className="mb-2 flex flex-wrap items-center gap-2">
              <h3 className="text-lg font-black text-jarvis-text">{title}</h3>
              <StatusBadge tone={tone}>
                {state === "confirm" ? "Needs confirmation" : state === "executing" ? "Executing" : state === "error" ? "Error" : "Done"}
              </StatusBadge>
            </div>
            <p className="text-sm leading-7 text-jarvis-muted">{message}</p>
            {(command || target) && (
              <div className="mt-3 grid gap-2 rounded-2xl border border-jarvis-border bg-black/20 p-3 text-sm">
                {command && (
                  <div className="flex justify-between gap-3">
                    <span className="text-jarvis-faint">Command</span>
                    <strong className="text-right text-jarvis-text">{command}</strong>
                  </div>
                )}
                {target && (
                  <div className="flex justify-between gap-3">
                    <span className="text-jarvis-faint">Target</span>
                    <strong className="break-all text-right text-jarvis-text">{target}</strong>
                  </div>
                )}
              </div>
            )}
          </div>
        </div>

        {state === "confirm" && (
          <div className="flex flex-wrap justify-end gap-2">
            <button className="soft-button" type="button" disabled={disabled} onClick={onCancel}>
              Cancel
            </button>
            <button className="primary-button" type="button" disabled={disabled} onClick={onConfirm}>
              Confirm
            </button>
          </div>
        )}
      </div>
    </article>
  );
}
