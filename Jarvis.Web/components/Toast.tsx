type ToastProps = {
  message: string;
};

export function Toast({ message }: ToastProps) {
  return <div className={message ? "toast visible" : "toast"}>{message}</div>;
}
