"use client";

import { ChangeEvent, useEffect, useMemo, useState } from "react";
import { jarvisApi } from "@/lib/api";
import type { IngestionJob, IngestionMemoryCandidate, KnowledgeCategory, MemoryItem } from "@/lib/types";

type MemoryIngestionPanelProps = {
  onMemoryChanged: () => Promise<void>;
};

type CandidateDraft = {
  content: string;
  category: string;
  memoryType: MemoryItem["memoryType"];
  importance: number;
  confidence: number;
};

const categories = [
  "Astrology",
  "Tarot",
  "Occult",
  "Vastu",
  "Projects",
  "Preferences",
  "Identity",
  "Education",
  "Work",
  "Goals",
  "Health",
  "General"
];

export function MemoryIngestionPanel({ onMemoryChanged }: MemoryIngestionPanelProps) {
  const [jobs, setJobs] = useState<IngestionJob[]>([]);
  const [selectedJobId, setSelectedJobId] = useState("");
  const [drafts, setDrafts] = useState<Record<string, CandidateDraft>>({});
  const [selectedCandidates, setSelectedCandidates] = useState<string[]>([]);
  const [bulkCategory, setBulkCategory] = useState("General");
  const [bulkImportance, setBulkImportance] = useState(3);
  const [bulkConfidence, setBulkConfidence] = useState(7);
  const [knowledgeCategory, setKnowledgeCategory] = useState<KnowledgeCategory>("Documents");
  const [statusMessage, setStatusMessage] = useState("");
  const [isBusy, setIsBusy] = useState(false);

  const selectedJob = useMemo(
    () => jobs.find((job) => job.id === selectedJobId) ?? jobs[0] ?? null,
    [jobs, selectedJobId]
  );

  useEffect(() => {
    refreshJobs().catch((error: Error) => setStatusMessage(error.message));
  }, []);

  async function refreshJobs() {
    const nextJobs = await jarvisApi.ingestionJobs();
    setJobs(nextJobs);
    if (!selectedJobId && nextJobs.length > 0) {
      setSelectedJobId(nextJobs[0].id);
    }
  }

  async function handleFiles(event: ChangeEvent<HTMLInputElement>) {
    const files = Array.from(event.target.files ?? []);
    if (files.length === 0) {
      return;
    }

    setIsBusy(true);
    setStatusMessage("Uploading...");
    try {
      let latest: IngestionJob | null = null;
      for (const file of files) {
        latest = await jarvisApi.uploadIngestionFile(file);
      }

      await refreshJobs();
      if (latest) {
        setSelectedJobId(latest.id);
      }
      setStatusMessage("Upload complete.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Upload failed.");
    } finally {
      event.target.value = "";
      setIsBusy(false);
    }
  }

  async function runJobAction(action: () => Promise<IngestionJob>, message: string) {
    setIsBusy(true);
    setStatusMessage(message);
    try {
      const job = await action();
      upsertJob(job);
      setSelectedJobId(job.id);
      setStatusMessage(job.errorMessage || `${message.replace("...", "")} done.`);
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Action failed.");
    } finally {
      setIsBusy(false);
    }
  }

  async function approveCandidate(candidate: IngestionMemoryCandidate) {
    const draft = drafts[candidate.id] ?? toDraft(candidate);
    setIsBusy(true);
    setStatusMessage("Saving memory...");
    try {
      const result = await jarvisApi.approveIngestionCandidate(candidate.id, {
        content: draft.content,
        category: draft.category,
        memoryType: draft.memoryType,
        importance: draft.importance,
        confidence: draft.confidence
      });
      upsertJob(result.job);
      await onMemoryChanged();
      setStatusMessage("Memory approved.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Approve failed.");
    } finally {
      setIsBusy(false);
    }
  }

  async function bulkApprove() {
    const ids = selectedCandidates.filter(Boolean);
    if (ids.length === 0) {
      setStatusMessage("Select candidates first.");
      return;
    }

    setIsBusy(true);
    setStatusMessage("Bulk approving...");
    try {
      const result = await jarvisApi.bulkApproveIngestionCandidates(ids, {
        category: bulkCategory,
        importance: bulkImportance,
        confidence: bulkConfidence
      });
      setJobs(result.jobs);
      setSelectedCandidates([]);
      await onMemoryChanged();
      setStatusMessage(`${result.approved} memories approved.`);
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Bulk approve failed.");
    } finally {
      setIsBusy(false);
    }
  }

  async function bulkReject() {
    const ids = selectedCandidates.filter(Boolean);
    if (ids.length === 0) {
      setStatusMessage("Select candidates first.");
      return;
    }

    setIsBusy(true);
    setStatusMessage("Bulk rejecting...");
    try {
      const result = await jarvisApi.bulkRejectIngestionCandidates(ids);
      setJobs(result.jobs);
      setSelectedCandidates([]);
      setStatusMessage(`${result.rejected} candidates rejected.`);
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Bulk reject failed.");
    } finally {
      setIsBusy(false);
    }
  }

  async function saveAsKnowledge() {
    if (!selectedJob?.extractedText.trim()) {
      setStatusMessage("Extract text before saving as knowledge.");
      return;
    }

    setIsBusy(true);
    setStatusMessage("Saving knowledge...");
    try {
      await jarvisApi.saveIngestionAsKnowledge(selectedJob.id, {
        title: selectedJob.fileName,
        category: knowledgeCategory
      });
      setStatusMessage("Saved as knowledge.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Save as knowledge failed.");
    } finally {
      setIsBusy(false);
    }
  }

  async function saveAsMemory() {
    if (!selectedJob?.extractedText.trim()) {
      setStatusMessage("Extract text before saving as memory.");
      return;
    }

    setIsBusy(true);
    setStatusMessage("Saving memory...");
    try {
      await jarvisApi.saveIngestionAsMemory(selectedJob.id, {
        category: bulkCategory,
        memoryType: "PermanentMemory",
        importance: bulkImportance,
        confidence: bulkConfidence
      });
      await onMemoryChanged();
      setStatusMessage("Saved as memory.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Save as memory failed.");
    } finally {
      setIsBusy(false);
    }
  }

  async function rejectCandidate(candidate: IngestionMemoryCandidate) {
    setIsBusy(true);
    setStatusMessage("Rejecting candidate...");
    try {
      const result = await jarvisApi.rejectIngestionCandidate(candidate.id);
      upsertJob(result.job);
      setStatusMessage("Candidate rejected.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Reject failed.");
    } finally {
      setIsBusy(false);
    }
  }

  async function deleteSelectedJob() {
    if (!selectedJob) {
      return;
    }

    setIsBusy(true);
    setStatusMessage("Deleting import...");
    try {
      const nextJobs = await jarvisApi.deleteIngestionJob(selectedJob.id);
      setJobs(nextJobs);
      setSelectedJobId(nextJobs[0]?.id ?? "");
      setStatusMessage("Import deleted.");
    } catch (error) {
      setStatusMessage(error instanceof Error ? error.message : "Delete failed.");
    } finally {
      setIsBusy(false);
    }
  }

  function upsertJob(job: IngestionJob) {
    setJobs((current) => [job, ...current.filter((item) => item.id !== job.id)]);
  }

  function draftFor(candidate: IngestionMemoryCandidate) {
    return drafts[candidate.id] ?? toDraft(candidate);
  }

  function updateDraft(candidate: IngestionMemoryCandidate, next: CandidateDraft) {
    setDrafts((current) => ({ ...current, [candidate.id]: next }));
  }

  function toggleCandidate(id: string) {
    setSelectedCandidates((current) =>
      current.includes(id) ? current.filter((item) => item !== id) : [...current, id]
    );
  }

  function toggleAllPending() {
    const pendingIds = selectedJob?.candidates
      .filter((candidate) => candidate.reviewStatus === "Pending")
      .map((candidate) => candidate.id) ?? [];
    setSelectedCandidates((current) => current.length === pendingIds.length ? [] : pendingIds);
  }

  return (
    <section className="memory-ingestion-panel">
      <div className="memory-section-heading">
        <div>
          <h3>Memory Import</h3>
          <p>Upload PDFs or photos, preview extracted text, then approve only the memories you want Jarvis to keep.</p>
        </div>
        <strong>{jobs.length}</strong>
      </div>

      <div className="simple-card ingestion-upload-card">
        <label className="ingestion-upload">
          <span>Upload PDF or image</span>
          <input
            type="file"
            accept=".pdf,.png,.jpg,.jpeg,.webp"
            multiple
            disabled={isBusy}
            onChange={handleFiles}
          />
        </label>
        <p>Supported: PDF, PNG, JPG, JPEG, WEBP. Max size: 25 MB. Files stay local.</p>
        {statusMessage && <div className="control-message">{statusMessage}</div>}
      </div>

      {jobs.length > 0 && (
        <div className="simple-card">
          <label className="ingestion-select">
            <span>Import job</span>
            <select value={selectedJob?.id ?? ""} onChange={(event) => setSelectedJobId(event.target.value)}>
              {jobs.map((job) => (
                <option key={job.id} value={job.id}>
                  {job.fileName} - {job.status}
                </option>
              ))}
            </select>
          </label>
        </div>
      )}

      {selectedJob && (
        <article className="list-card ingestion-job-card">
          <div className="memory-card-head">
            <strong>{selectedJob.fileName}</strong>
            <div className="memory-badges">
              <span className="memory-badge">{selectedJob.fileType}</span>
              <span className="memory-badge">{selectedJob.sourceType}</span>
              <span className="memory-badge">{selectedJob.status}</span>
            </div>
          </div>

          <div className="memory-review-summary">
            <span>OCR Source: <strong>{selectedJob.extractionSource || selectedJob.sourceType}</strong></span>
            <span>Language: <strong>{selectedJob.extractionLanguage || "embedded"}</strong></span>
            <span>Confidence: <strong>{selectedJob.extractionConfidence || "n/a"}</strong></span>
            <span>Characters: <strong>{selectedJob.extractedCharacterCount}</strong></span>
            <span>Words: <strong>{selectedJob.extractedWordCount}</strong></span>
          </div>

          {selectedJob.errorMessage && <div className="control-message">{selectedJob.errorMessage}</div>}

          <div className="memory-actions">
            <button
              className="soft-button"
              type="button"
              disabled={isBusy}
              onClick={() => runJobAction(() => jarvisApi.extractIngestionJob(selectedJob.id), "Extracting...")}
            >
              Extract Text
            </button>
            <button
              className="soft-button"
              type="button"
              disabled={isBusy || !selectedJob.extractedText.trim()}
              onClick={() => runJobAction(() => jarvisApi.generateIngestionCandidates(selectedJob.id), "Creating suggestions...")}
            >
              Suggest Memories
            </button>
            <button
              className="soft-button"
              type="button"
              disabled={isBusy || !selectedJob.extractedText.trim()}
              onClick={saveAsMemory}
            >
              Save as Memory
            </button>
            <button
              className="soft-button"
              type="button"
              disabled={isBusy || !selectedJob.extractedText.trim()}
              onClick={saveAsKnowledge}
            >
              Save as Knowledge
            </button>
            <button className="danger-button" type="button" disabled={isBusy} onClick={deleteSelectedJob}>
              Delete Import
            </button>
          </div>

          <div className="ingestion-compare">
            <div className="ingestion-preview">
              <strong>Original source</strong>
              {selectedJob.sourceType === "Image" ? (
                <img src={jarvisApi.ingestionFileUrl(selectedJob.id)} alt={selectedJob.fileName} />
              ) : (
                <a className="soft-button" href={jarvisApi.ingestionFileUrl(selectedJob.id)} target="_blank" rel="noreferrer">
                  Open PDF preview
                </a>
              )}
            </div>
            <div className="ingestion-preview">
              <strong>Extracted text preview</strong>
              <pre>{selectedJob.extractedText || "No text extracted yet."}</pre>
            </div>
          </div>

          <div className="memory-form compact">
            <label>
              <span>Bulk Category</span>
              <select value={bulkCategory} onChange={(event) => setBulkCategory(event.target.value)}>
                {categories.map((category) => (
                  <option key={category} value={category}>{category}</option>
                ))}
              </select>
            </label>
            <label>
              <span>Knowledge Category</span>
              <select value={knowledgeCategory} onChange={(event) => setKnowledgeCategory(event.target.value as KnowledgeCategory)}>
                {["Astrology", "Tarot", "Occult", "Vastu", "Research", "Books", "Documents", "General"].map((category) => (
                  <option key={category} value={category}>{category}</option>
                ))}
              </select>
            </label>
            <label>
              <span>Bulk Importance</span>
              <input type="number" min={1} max={10} value={bulkImportance} onChange={(event) => setBulkImportance(Number(event.target.value) || 3)} />
            </label>
            <label>
              <span>Bulk Confidence</span>
              <input type="number" min={1} max={10} value={bulkConfidence} onChange={(event) => setBulkConfidence(Number(event.target.value) || 7)} />
            </label>
          </div>

          <div className="memory-actions">
            <button className="soft-button" type="button" disabled={isBusy || selectedJob.candidates.length === 0} onClick={toggleAllPending}>
              Select Pending
            </button>
            <button className="soft-button" type="button" disabled={isBusy || selectedCandidates.length === 0} onClick={bulkApprove}>
              Approve Selected
            </button>
            <button className="danger-button" type="button" disabled={isBusy || selectedCandidates.length === 0} onClick={bulkReject}>
              Reject Selected
            </button>
            <span className="memory-quality">{selectedCandidates.length} selected</span>
          </div>

          <div className="memory-section-list">
            <section className="memory-review-section">
              <div className="memory-section-heading compact">
                <div>
                  <h3>Suggested Memories</h3>
                  <p>Review each candidate before saving it as a Jarvis memory.</p>
                </div>
                <strong>{selectedJob.candidates.length}</strong>
              </div>

              {selectedJob.candidates.length === 0 ? (
                <div className="list-empty compact">No candidates yet.</div>
              ) : (
                <div className="item-list">
                  {selectedJob.candidates.map((candidate) => (
                    <CandidateCard
                      key={candidate.id}
                      candidate={candidate}
                      draft={draftFor(candidate)}
                      disabled={isBusy}
                      selected={selectedCandidates.includes(candidate.id)}
                      onChange={(next) => updateDraft(candidate, next)}
                      onToggleSelected={() => toggleCandidate(candidate.id)}
                      onApprove={() => approveCandidate(candidate)}
                      onReject={() => rejectCandidate(candidate)}
                    />
                  ))}
                </div>
              )}
            </section>
          </div>
        </article>
      )}
    </section>
  );
}

function CandidateCard({
  candidate,
  draft,
  disabled,
  onChange,
  selected,
  onToggleSelected,
  onApprove,
  onReject
}: {
  candidate: IngestionMemoryCandidate;
  draft: CandidateDraft;
  disabled: boolean;
  onChange: (draft: CandidateDraft) => void;
  selected: boolean;
  onToggleSelected: () => void;
  onApprove: () => void;
  onReject: () => void;
}) {
  return (
    <article className="list-card memory-card">
      <div className="memory-card-head">
        <label className="candidate-select">
          <input type="checkbox" checked={selected} disabled={disabled || candidate.reviewStatus !== "Pending"} onChange={onToggleSelected} />
          <strong>{candidate.reviewStatus}</strong>
        </label>
        <div className="memory-badges">
          <span className="memory-badge">{candidate.sourceFile}</span>
          <span className="memory-badge">P{draft.importance}</span>
          <span className="memory-badge">C{draft.confidence}</span>
        </div>
      </div>

      <label>
        <span>Candidate text</span>
        <textarea rows={3} value={draft.content} onChange={(event) => onChange({ ...draft, content: event.target.value })} />
      </label>
      <div className="memory-form compact">
        <label>
          <span>Category</span>
          <select value={draft.category} onChange={(event) => onChange({ ...draft, category: event.target.value })}>
            {categories.map((category) => (
              <option key={category} value={category}>
                {category}
              </option>
            ))}
          </select>
        </label>
        <label>
          <span>Memory Type</span>
          <select
            value={draft.memoryType}
            onChange={(event) => onChange({ ...draft, memoryType: event.target.value as MemoryItem["memoryType"] })}
          >
            <option value="SuggestedMemory">Suggested</option>
            <option value="PermanentMemory">Permanent</option>
            <option value="TemporaryContext">Temporary</option>
          </select>
        </label>
        <label>
          <span>Importance</span>
          <input type="number" min={1} max={10} value={draft.importance} onChange={(event) => onChange({ ...draft, importance: Number(event.target.value) || 3 })} />
        </label>
        <label>
          <span>Confidence</span>
          <input type="number" min={1} max={10} value={draft.confidence} onChange={(event) => onChange({ ...draft, confidence: Number(event.target.value) || 5 })} />
        </label>
      </div>

      <div className="memory-actions">
        {candidate.reviewStatus === "Pending" && (
          <>
            <button className="soft-button" type="button" disabled={disabled || !draft.content.trim()} onClick={onApprove}>
              Approve
            </button>
            <button className="danger-button" type="button" disabled={disabled} onClick={onReject}>
              Reject
            </button>
          </>
        )}
        {candidate.reviewStatus === "Approved" && <span className="memory-quality good">Saved</span>}
        {candidate.reviewStatus === "Rejected" && <span className="memory-quality danger">Rejected</span>}
      </div>
    </article>
  );
}

function toDraft(candidate: IngestionMemoryCandidate): CandidateDraft {
  return {
    content: candidate.content,
    category: candidate.suggestedCategory || "General",
    memoryType: candidate.suggestedMemoryType || "SuggestedMemory",
    importance: candidate.suggestedImportance || 3,
    confidence: candidate.suggestedConfidence || 5
  };
}
