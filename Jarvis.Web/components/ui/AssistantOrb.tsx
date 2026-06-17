type AssistantOrbProps = {
  size?: "sm" | "md" | "lg";
  active?: boolean;
  label?: string;
};

const sizes = {
  sm: "h-12 w-12",
  md: "h-20 w-20",
  lg: "h-32 w-32"
};

export function AssistantOrb({ size = "md", active = false, label = "J" }: AssistantOrbProps) {
  return (
    <div className={`relative grid place-items-center ${sizes[size]} ${active ? "animate-orb-pulse" : ""}`}>
      <span className="absolute inset-0 rounded-full bg-jarvis-green/20 blur-xl" />
      <span className="absolute inset-2 rounded-full border border-jarvis-green/30 bg-jarvis-green/10" />
      <span className="relative grid h-[72%] w-[72%] place-items-center rounded-full bg-gradient-to-br from-jarvis-green2 via-jarvis-green to-emerald-700 text-lg font-black text-jarvis-bg shadow-glow">
        {label}
      </span>
    </div>
  );
}
