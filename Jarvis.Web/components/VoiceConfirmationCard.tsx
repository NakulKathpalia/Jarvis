import type { VoiceCommandResult } from "@/lib/types";

export type PendingVoiceCommand = Pick<
  VoiceCommandResult,
  "command" | "message" | "confirmationValue"
> & {
  transcript: string;
};

type VoiceConfirmationCardProps = {
  pendingCommand: PendingVoiceCommand | null;
  disabled: boolean;
  onConfirm: () => Promise<void>;
  onCancel: () => void;
};

export function VoiceConfirmationCard({
  pendingCommand,
  disabled,
  onConfirm,
  onCancel
}: VoiceConfirmationCardProps) {
  if (!pendingCommand) {
    return null;
  }

  return (
    <section className="voice-confirmation" aria-label="Voice command confirmation">
      <div className="voice-confirmation-main">
        <strong>Confirm voice command</strong>
        <p>{pendingCommand.message}</p>
        <span>{pendingCommand.confirmationValue}</span>
      </div>
      <div className="voice-confirmation-actions">
        <button className="soft-button" type="button" onClick={onCancel} disabled={disabled}>
          Cancel
        </button>
        <button className="primary-button" type="button" onClick={() => void onConfirm()} disabled={disabled}>
          Confirm
        </button>
      </div>
    </section>
  );
}
