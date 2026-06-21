"use client";

import { UserMenu } from "./UserMenu";
import type { AuthStatus, ViewKey } from "@/lib/types";

type TopNavbarProps = {
  title: string;
  subtitle: string;
  authStatus: AuthStatus | null;
  onChangeView: (view: ViewKey) => void;
  onAuthChanged: () => Promise<void>;
  onToast: (message: string) => void;
  onToggleSidebar: () => void;
};

export function TopNavbar({
  title,
  subtitle,
  authStatus,
  onChangeView,
  onAuthChanged,
  onToast,
  onToggleSidebar
}: TopNavbarProps) {
  return (
    <header className="top-navbar">
      <div className="top-navbar-left">
        <button className="mobile-menu-button" type="button" onClick={onToggleSidebar} aria-label="Open sidebar">
          Menu
        </button>
        <div className="top-navbar-title">
          <h2>{title}</h2>
          <p>{subtitle}</p>
        </div>
      </div>

      <div className="top-navbar-actions">
        <button className="secondary-action" type="button" onClick={() => onChangeView("tools")}>
          Tools
        </button>
        <UserMenu
          authStatus={authStatus}
          onChangeView={onChangeView}
          onAuthChanged={onAuthChanged}
          onToast={onToast}
        />
      </div>
    </header>
  );
}
