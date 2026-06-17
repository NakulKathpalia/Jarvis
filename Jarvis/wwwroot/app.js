const state = {
  settings: null,
  history: [],
  memory: [],
};

const els = {
  statusText: document.querySelector("#statusText"),
  modelName: document.querySelector("#modelName"),
  memoryCount: document.querySelector("#memoryCount"),
  chatLog: document.querySelector("#chatLog"),
  chatForm: document.querySelector("#chatForm"),
  messageInput: document.querySelector("#messageInput"),
  sendButton: document.querySelector("#sendButton"),
  refreshButton: document.querySelector("#refreshButton"),
  memoryForm: document.querySelector("#memoryForm"),
  memoryInput: document.querySelector("#memoryInput"),
  memoryList: document.querySelector("#memoryList"),
  clearMemoryButton: document.querySelector("#clearMemoryButton"),
  indexButton: document.querySelector("#indexButton"),
  fileSearchForm: document.querySelector("#fileSearchForm"),
  fileQueryInput: document.querySelector("#fileQueryInput"),
  fileResults: document.querySelector("#fileResults"),
  saveSettingsButton: document.querySelector("#saveSettingsButton"),
  ollamaUrlInput: document.querySelector("#ollamaUrlInput"),
  modelInput: document.querySelector("#modelInput"),
  historyInput: document.querySelector("#historyInput"),
  fileRootInput: document.querySelector("#fileRootInput"),
  systemPromptInput: document.querySelector("#systemPromptInput"),
  toast: document.querySelector("#toast"),
};

document.querySelectorAll(".nav-item").forEach((button) => {
  button.addEventListener("click", () => {
    document.querySelectorAll(".nav-item").forEach((item) => item.classList.remove("active"));
    document.querySelectorAll(".view").forEach((view) => view.classList.remove("active"));
    button.classList.add("active");
    document.querySelector(`#${button.dataset.view}View`).classList.add("active");
  });
});

els.chatForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  const message = els.messageInput.value.trim();
  if (!message) return;

  appendMessage("user", message);
  els.messageInput.value = "";
  els.sendButton.disabled = true;
  appendMessage("system", "Thinking...");

  try {
    const result = await api("/api/chat", {
      method: "POST",
      body: JSON.stringify({ message }),
    });
    removeThinking();
    appendMessage("assistant", result.response || "(empty response)");
    await refreshAll(false);
  } catch (error) {
    removeThinking();
    toast(error.message);
  } finally {
    els.sendButton.disabled = false;
    els.messageInput.focus();
  }
});

els.messageInput.addEventListener("keydown", (event) => {
  if (event.key === "Enter" && !event.shiftKey) {
    event.preventDefault();
    els.chatForm.requestSubmit();
  }
});

els.refreshButton.addEventListener("click", () => refreshAll());

els.memoryForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  const text = els.memoryInput.value.trim();
  if (!text) return;

  await api("/api/memory", {
    method: "POST",
    body: JSON.stringify({ text, category: "General" }),
  });
  els.memoryInput.value = "";
  await refreshMemory();
  await refreshStatus();
  toast("Memory saved");
});

els.clearMemoryButton.addEventListener("click", async () => {
  await api("/api/memory", { method: "DELETE" });
  await refreshMemory();
  await refreshStatus();
  toast("Memory cleared");
});

els.indexButton.addEventListener("click", async () => {
  els.indexButton.disabled = true;
  try {
    const result = await api("/api/files/index", { method: "POST" });
    toast(`Indexed ${result.count} files`);
  } finally {
    els.indexButton.disabled = false;
  }
});

els.fileSearchForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  const query = els.fileQueryInput.value.trim();
  if (!query) return;

  const results = await api(`/api/files/search?q=${encodeURIComponent(query)}`);
  renderFiles(results);
});

els.saveSettingsButton.addEventListener("click", async () => {
  const settings = {
    ollamaBaseUrl: els.ollamaUrlInput.value.trim(),
    model: els.modelInput.value.trim(),
    systemPrompt: els.systemPromptInput.value,
    maxHistoryMessages: Number.parseInt(els.historyInput.value, 10) || 20,
    fileIndexRoot: els.fileRootInput.value.trim() || ".",
  };

  state.settings = await api("/api/settings", {
    method: "POST",
    body: JSON.stringify(settings),
  });
  renderSettings();
  await refreshStatus();
  toast("Settings saved");
});

async function refreshAll(showToast = true) {
  await Promise.all([refreshStatus(), refreshHistory(), refreshMemory(), refreshSettings()]);
  if (showToast) toast("Refreshed");
}

async function refreshStatus() {
  const status = await api("/api/status");
  els.statusText.textContent = status.online ? "Ollama online" : "Ollama offline";
  els.statusText.style.color = status.online ? "var(--accent)" : "var(--warn)";
  els.modelName.textContent = status.model || "-";
  els.memoryCount.textContent = status.memoryCount ?? 0;
}

async function refreshHistory() {
  state.history = await api("/api/history");
  renderHistory();
}

async function refreshMemory() {
  state.memory = await api("/api/memory");
  renderMemory();
}

async function refreshSettings() {
  state.settings = await api("/api/settings");
  renderSettings();
}

function renderHistory() {
  els.chatLog.innerHTML = "";
  if (!state.history.length) {
    appendMessage("system", "Ready.");
    return;
  }

  state.history
    .filter((message) => message.role !== "system")
    .forEach((message) => appendMessage(message.role, message.content));
}

function appendMessage(role, content) {
  const item = document.createElement("div");
  item.className = `message ${role}`;
  item.textContent = content;
  els.chatLog.appendChild(item);
  els.chatLog.scrollTop = els.chatLog.scrollHeight;
}

function removeThinking() {
  const messages = [...els.chatLog.querySelectorAll(".message.system")];
  const last = messages.at(-1);
  if (last && last.textContent === "Thinking...") {
    last.remove();
  }
}

function renderMemory() {
  els.memoryList.innerHTML = "";
  if (!state.memory.length) {
    els.memoryList.append(emptyItem("No memory saved yet."));
    return;
  }

  state.memory.forEach((item) => {
    const row = document.createElement("div");
    row.className = "list-item";
    row.innerHTML = `<strong>${escapeHtml(item.text)}</strong><br><small>${escapeHtml(item.category)} · ${formatDate(item.createdAtUtc)}</small>`;
    els.memoryList.append(row);
  });
}

function renderFiles(files) {
  els.fileResults.innerHTML = "";
  if (!files.length) {
    els.fileResults.append(emptyItem("No files found."));
    return;
  }

  files.forEach((file) => {
    const row = document.createElement("div");
    row.className = "list-item";
    row.textContent = file;
    els.fileResults.append(row);
  });
}

function renderSettings() {
  if (!state.settings) return;
  els.ollamaUrlInput.value = state.settings.ollamaBaseUrl || "";
  els.modelInput.value = state.settings.model || "";
  els.historyInput.value = state.settings.maxHistoryMessages || 20;
  els.fileRootInput.value = state.settings.fileIndexRoot || ".";
  els.systemPromptInput.value = state.settings.systemPrompt || "";
}

function emptyItem(text) {
  const row = document.createElement("div");
  row.className = "list-item";
  row.textContent = text;
  return row;
}

async function api(path, options = {}) {
  const response = await fetch(path, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });

  if (!response.ok) {
    let message = `Request failed (${response.status})`;
    try {
      const body = await response.json();
      message = body.error || message;
    } catch {
      // Keep default message.
    }
    throw new Error(message);
  }

  return response.json();
}

function toast(message) {
  els.toast.textContent = message;
  els.toast.classList.add("visible");
  window.clearTimeout(toast.timer);
  toast.timer = window.setTimeout(() => els.toast.classList.remove("visible"), 2200);
}

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;");
}

function formatDate(value) {
  if (!value) return "";
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

refreshAll(false).catch((error) => toast(error.message));
