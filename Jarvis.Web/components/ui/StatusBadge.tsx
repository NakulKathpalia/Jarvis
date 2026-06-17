import type React from "react";

type StatusBadgeProps = {
  tone?: "green" | "amber" | "red" | "neutral";
  children: React.ReactNode;
};

const tones = {
  green: "border-jarvis-green/30 bg-jarvis-green/10 text-jarvis-green2",
  amber: "border-jarvis-amber/30 bg-jarvis-amber/10 text-jarvis-amber",
  red: "border-jarvis-danger/30 bg-jarvis-danger/10 text-rose-200",
  neutral: "border-jarvis-border bg-white/5 text-jarvis-muted"
};

export function StatusBadge({ tone = "neutral", children }: StatusBadgeProps) {
  return (
    <span className={`inline-flex min-h-7 items-center rounded-full border px-3 text-xs font-semibold ${tones[tone]}`}>
      {children}
    </span>
  );
}
