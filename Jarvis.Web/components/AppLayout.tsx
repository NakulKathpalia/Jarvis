import type React from "react";

type AppLayoutProps = {
  sidebar: React.ReactNode;
  children: React.ReactNode;
};

export function AppLayout({ sidebar, children }: AppLayoutProps) {
  return (
    <div className="min-h-screen bg-[radial-gradient(circle_at_top_left,rgba(32,227,130,0.18),transparent_32%),linear-gradient(135deg,#06110d_0%,#08140f_45%,#020504_100%)] text-jarvis-text">
      <div className="flex min-h-screen flex-col lg:flex-row">
        {sidebar}
        <main className="min-w-0 flex-1">{children}</main>
      </div>
    </div>
  );
}
