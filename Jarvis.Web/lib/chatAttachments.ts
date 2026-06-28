export const ingestibleAttachmentExtensions = [
  ".pdf",
  ".png",
  ".jpg",
  ".jpeg",
  ".webp"
];

export const acceptedAttachmentExtensions = [
  ".pdf",
  ".png",
  ".jpg",
  ".jpeg",
  ".webp",
  ".txt",
  ".md",
  ".csv",
  ".json",
  ".xml",
  ".html",
  ".css",
  ".js",
  ".ts",
  ".tsx",
  ".jsx",
  ".cs",
  ".py",
  ".java",
  ".cpp",
  ".c",
  ".h",
  ".sql",
  ".docx",
  ".xlsx",
  ".pptx"
];

export const maxAttachmentSizeBytes = 25 * 1024 * 1024;

export function getFileExtension(fileName: string) {
  const index = fileName.lastIndexOf(".");
  return index >= 0 ? fileName.slice(index).toLowerCase() : "";
}

export function isIngestibleAttachment(file: File) {
  return ingestibleAttachmentExtensions.includes(getFileExtension(file.name));
}

export function validateChatAttachment(file: File): string | null {
  const extension = getFileExtension(file.name);

  if (!acceptedAttachmentExtensions.includes(extension)) {
    return "This file type can be attached, but Jarvis does not support it yet.";
  }

  if (file.size > maxAttachmentSizeBytes) {
    return "File is too large. Maximum size is 25 MB.";
  }

  return null;
}

export function getAttachmentKind(file: File): "image" | "pdf" | "code" | "document" | "spreadsheet" | "presentation" | "text" | "file" {
  const extension = getFileExtension(file.name);

  if ([".png", ".jpg", ".jpeg", ".webp"].includes(extension)) return "image";
  if (extension === ".pdf") return "pdf";
  if ([".js", ".ts", ".tsx", ".jsx", ".cs", ".py", ".java", ".cpp", ".c", ".h", ".sql"].includes(extension)) return "code";
  if ([".txt", ".md", ".json", ".xml", ".html", ".css"].includes(extension)) return "text";
  if ([".docx"].includes(extension)) return "document";
  if ([".xlsx", ".csv"].includes(extension)) return "spreadsheet";
  if ([".pptx"].includes(extension)) return "presentation";

  return "file";
}

export function formatAttachmentSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}