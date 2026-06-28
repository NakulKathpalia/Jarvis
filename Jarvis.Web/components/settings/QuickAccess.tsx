export function QuickAccess() {
  return (
    <section className="quick-access">
      <h3>Quick Access</h3>
      <div className="quick-grid">
        <QuickCard icon="🧠" title="Memory" text="Memory retrieval and saved context" />
        <QuickCard icon="▤" title="Knowledge Library" text="Files and indexed knowledge" />
        <QuickCard icon="⇧" title="OCR" text="Image and document reading" />
        <QuickCard icon="≋" title="Voice" text="Speech and wake word" />
        <QuickCard icon="▭" title="Appearance" text="Theme and interface" />
      </div>
    </section>
  );
}

function QuickCard({ icon, title, text }: { icon: string; title: string; text: string }) {
  return (
    <button className="quick-card" type="button">
      <span>{icon}</span>
      <div>
        <strong>{title}</strong>
        <p>{text}</p>
      </div>
      <b>›</b>
    </button>
  );
}