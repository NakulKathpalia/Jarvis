import type React from "react";

type AppLayoutProps = {
  sidebar: React.ReactNode;
  rightPanel?: React.ReactNode;
  children: React.ReactNode;
};

export function AppLayout({ sidebar, rightPanel, children }: AppLayoutProps) {
  return (
    <div className="app-shell">
      <div className="app-frame">
        {sidebar}
        <main className="app-main">{children}</main>
        {rightPanel}
      </div>
    </div>
  );
}
