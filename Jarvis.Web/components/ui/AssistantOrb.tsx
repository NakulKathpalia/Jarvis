type AssistantOrbProps = {
  size?: "sm" | "md" | "lg";
  active?: boolean;
  state?: "idle" | "listening" | "thinking" | "executing" | "speaking" | "error";
  label?: string;
};

const sizes = {
  sm: "h-12 w-12",
  md: "h-20 w-20",
  lg: "h-32 w-32"
};

export function AssistantOrb({ size = "md", active = false, state = "idle", label = "J" }: AssistantOrbProps) {
  const isActive = active || state === "listening" || state === "thinking" || state === "executing" || state === "speaking";
  const tone = state === "error"
    ? "from-rose-200 via-jarvis-danger to-rose-900"
    : state === "speaking"
      ? "from-cyan-200 via-jarvis-green2 to-emerald-700"
      : state === "executing"
        ? "from-jarvis-amber via-jarvis-green to-emerald-800"
        : "from-jarvis-green2 via-jarvis-green to-emerald-700";

  return (
    <div className={`relative grid place-items-center ${sizes[size]} ${isActive ? "animate-orb-pulse" : ""}`}>
      <span className={`absolute inset-0 rounded-full blur-xl ${state === "error" ? "bg-jarvis-danger/25" : "bg-jarvis-green/20"}`} />
      <span className="absolute inset-2 rounded-full border border-jarvis-green/30 bg-jarvis-green/10" />
      <span className={`relative grid h-[72%] w-[72%] place-items-center rounded-full bg-gradient-to-br ${tone} text-lg font-black text-jarvis-bg shadow-glow`}>
        {label}
      </span>
    </div>
  );
}
