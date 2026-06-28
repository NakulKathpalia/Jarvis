import type { ReactNode } from "react";

type SettingsCardProps = {
  title: string;
  children: ReactNode;
};

export function SettingsCard({ title, children }: SettingsCardProps) {
  return (
    <section className="settings-glass-card">
      <h3>{title}</h3>
      <div>{children}</div>
    </section>
  );
}