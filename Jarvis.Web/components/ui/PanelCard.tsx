import type React from "react";

type PanelCardProps = {
  children: React.ReactNode;
  className?: string;
};

export function PanelCard({ children, className = "" }: PanelCardProps) {
  return (
    <section className={`rounded-2xl border border-jarvis-border bg-jarvis-card/70 p-5 shadow-card ${className}`}>
      {children}
    </section>
  );
}
