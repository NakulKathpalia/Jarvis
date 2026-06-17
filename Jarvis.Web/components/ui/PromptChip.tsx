type PromptChipProps = {
  label: string;
  onClick: () => void;
};

export function PromptChip({ label, onClick }: PromptChipProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="rounded-full border border-jarvis-border bg-white/[0.04] px-4 py-2 text-sm font-semibold text-jarvis-text transition hover:-translate-y-0.5 hover:border-jarvis-green/50 hover:bg-jarvis-green/10"
    >
      {label}
    </button>
  );
}
