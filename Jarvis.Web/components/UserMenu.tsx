"use client";

import { useState } from "react";
import { authApi } from "@/lib/authApi";
import type { AuthStatus, ViewKey } from "@/lib/types";

type UserMenuProps = {
  authStatus: AuthStatus | null;
  onChangeView: (view: ViewKey) => void;
  onAuthChanged: () => Promise<void>;
  onToast: (message: string) => void;
};

export function UserMenu({ authStatus, onChangeView, onAuthChanged, onToast }: UserMenuProps) {
  const [isOpen, setIsOpen] = useState(false);
  const isSignedIn = authStatus?.isAuthenticated ?? false;
  const label = isSignedIn
    ? authStatus?.user?.name || authStatus?.user?.email || "User"
    : "Sign in";
  const initials = getInitials(label);

  async function signOut() {
    try {
      const result = await authApi.signOut();
      await onAuthChanged();
      onToast(result.message);
      setIsOpen(false);
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Sign out failed");
    }
  }

  function openView(view: ViewKey) {
    onChangeView(view);
    setIsOpen(false);
  }

  return (
    <div className="user-menu">
      <button className="user-menu-trigger" type="button" onClick={() => setIsOpen((current) => !current)}>
        <span className="user-avatar">{isSignedIn ? initials : "?"}</span>
        <span className="user-menu-text">
          <strong>{label}</strong>
          <small>{isSignedIn ? authStatus?.user?.email : "Local placeholder auth"}</small>
        </span>
      </button>

      {isOpen && (
        <div className="user-menu-popover">
          {!isSignedIn ? (
            <>
              <button type="button" onClick={() => openView("auth")}>Sign in</button>
              <button type="button" onClick={() => openView("auth")}>Sign up</button>
            </>
          ) : (
            <>
              <button type="button" onClick={() => openView("auth")}>Profile</button>
              <button type="button" onClick={() => openView("connectedApps")}>Connected Apps</button>
              <button type="button" onClick={() => openView("settings")}>Settings</button>
              <button type="button" onClick={() => void signOut()}>Sign out</button>
            </>
          )}
        </div>
      )}
    </div>
  );
}

function getInitials(value: string) {
  const parts = value.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "U";
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
}
