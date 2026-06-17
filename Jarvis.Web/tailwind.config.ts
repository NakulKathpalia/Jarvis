import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./app/**/*.{js,ts,jsx,tsx,mdx}",
    "./components/**/*.{js,ts,jsx,tsx,mdx}",
    "./lib/**/*.{js,ts,jsx,tsx,mdx}"
  ],
  theme: {
    extend: {
      colors: {
        jarvis: {
          bg: "#06110d",
          panel: "#0b1712",
          panel2: "#10231b",
          card: "#132820",
          card2: "#18362a",
          border: "#254438",
          text: "#ecfff7",
          muted: "#8fb4a4",
          faint: "#5f8172",
          green: "#20e382",
          green2: "#63ffb6",
          amber: "#f6c453",
          danger: "#ff6b6b"
        }
      },
      boxShadow: {
        glow: "0 0 40px rgba(32, 227, 130, 0.26)",
        card: "0 18px 50px rgba(0, 0, 0, 0.28)"
      },
      animation: {
        "orb-pulse": "orbPulse 2.8s ease-in-out infinite",
        "soft-float": "softFloat 5s ease-in-out infinite",
        "thinking-dot": "thinkingDot 1.2s ease-in-out infinite"
      },
      keyframes: {
        orbPulse: {
          "0%, 100%": { transform: "scale(1)", boxShadow: "0 0 36px rgba(32, 227, 130, 0.26)" },
          "50%": { transform: "scale(1.04)", boxShadow: "0 0 70px rgba(99, 255, 182, 0.42)" }
        },
        softFloat: {
          "0%, 100%": { transform: "translateY(0)" },
          "50%": { transform: "translateY(-8px)" }
        },
        thinkingDot: {
          "0%, 100%": { opacity: "0.35", transform: "translateY(0)" },
          "50%": { opacity: "1", transform: "translateY(-3px)" }
        }
      }
    }
  },
  plugins: []
};

export default config;
