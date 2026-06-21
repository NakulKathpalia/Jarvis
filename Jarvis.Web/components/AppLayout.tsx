import type React from "react";

type AppLayoutProps = {
  sidebar: React.ReactNode;
  topbar?: React.ReactNode;
  rightPanel?: React.ReactNode;
  children: React.ReactNode;
  mobileSidebarOpen?: boolean;
  onCloseMobileSidebar?: () => void;
};

export function AppLayout({
  sidebar,
  topbar,
  rightPanel,
  children,
  mobileSidebarOpen = false,
  onCloseMobileSidebar
}: AppLayoutProps) {
  return (
    <div className="app-shell">
      <div className="app-frame">
        <div className={mobileSidebarOpen ? "sidebar-shell open" : "sidebar-shell"}>
          {sidebar}
        </div>
        <button
          aria-label="Close sidebar"
          className={mobileSidebarOpen ? "mobile-sidebar-backdrop open" : "mobile-sidebar-backdrop"}
          type="button"
          onClick={onCloseMobileSidebar}
        />
        <main className="app-main">
          {topbar}
          <div className="app-content">{children}</div>
        </main>
        {rightPanel}
      </div>
    </div>
  );
}
