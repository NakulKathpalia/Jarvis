export function ToggleRow({
  title,
  text,
  checked,
  onChange,
}: {
  title: string;
  text: string;
  checked: boolean;
  onChange: (value: boolean) => void;
}) {
  return (
    <div className="setting-row">
      <div>
        <strong>{title}</strong>
        <p>{text}</p>
      </div>
      <label className="jarvis-switch">
        <input checked={checked} type="checkbox" onChange={(event) => onChange(event.target.checked)} />
        <span />
      </label>
    </div>
  );
}

export function InputRow({
  title,
  text,
  value,
  onChange,
  type = "text",
}: {
  title: string;
  text: string;
  value: string | number;
  onChange: (value: string) => void;
  type?: string;
}) {
  return (
    <label className="setting-row">
      <div>
        <strong>{title}</strong>
        {text && <p>{text}</p>}
      </div>
      <input type={type} value={value ?? ""} onChange={(event) => onChange(event.target.value)} />
    </label>
  );
}

export function SelectRow({
  title,
  text,
  value,
  onChange,
  options,
}: {
  title: string;
  text: string;
  value: string;
  onChange: (value: string) => void;
  options: string[];
}) {
  return (
    <label className="setting-row">
      <div>
        <strong>{title}</strong>
        <p>{text}</p>
      </div>
      <select value={value} onChange={(event) => onChange(event.target.value)}>
        {options.map((option) => (
          <option key={option} value={option}>
            {option}
          </option>
        ))}
      </select>
    </label>
  );
}

export function StatusLine({ title, value }: { title: string; value: string }) {
  return (
    <div className="setting-row">
      <div>
        <strong>{title}</strong>
        <p>{value}</p>
      </div>
    </div>
  );
}