import type { MemoryFormValues, MemoryItem } from "@/lib/types";

export type MemoryDraft = MemoryFormValues & { id?: string };

export const emptyMemoryDraft: MemoryFormValues = {
  text: "",
  category: "General",
  tags: [],
  importance: 3
};

export function memoryToDraft(item: MemoryItem): MemoryDraft {
  return {
    id: item.id,
    text: item.text,
    category: item.category,
    tags: item.tags ?? [],
    importance: item.importance ?? 3
  };
}

export function parseMemoryTags(value: string) {
  return value
    .split(",")
    .map((tag) => tag.trim())
    .filter(Boolean);
}

export function cleanMemoryDraft(draft: MemoryFormValues): MemoryFormValues {
  return {
    text: draft.text.trim(),
    category: draft.category.trim() || "General",
    tags: draft.tags,
    importance: draft.importance
  };
}
