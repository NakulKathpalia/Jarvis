type TopBarProps = {
  title: string;
  subtitle: string;
  action?: React.ReactNode;
};

export function TopBar({ title, subtitle, action }: TopBarProps) {
  return (
    <header className="top-bar">
      <div>
        <h2>{title}</h2>
        <p>{subtitle}</p>
      </div>
      {action}
    </header>
  );
}
