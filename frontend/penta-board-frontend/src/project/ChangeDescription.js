import React, { useEffect, useRef, useState } from "react";
import "./ChangeDescription.css";
import { API_BASE, getToken } from "../utils/auth";

/**
 * Props:
 *  - open: boolean
 *  - project: { id, description }
 *  - onClose(): void
 *  - onSaved(updatedProject): void   // başarılı güncellemede çağrılır
 */
export default function ChangeDescription({ open, project, onClose, onSaved }) {
  const [value, setValue] = useState(project?.description ?? "");
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState("");
  const dialogRef = useRef(null);
  const textareaRef = useRef(null);

  useEffect(() => {
    if (open) {
      setValue(project?.description ?? "");
      setErr("");
      setTimeout(() => textareaRef.current?.focus(), 10);
    }
  }, [open, project]);

  useEffect(() => {
    const onEsc = (e) => (e.key === "Escape" ? onClose?.() : null);
    if (open) document.addEventListener("keydown", onEsc);
    return () => document.removeEventListener("keydown", onEsc);
  }, [open, onClose]);

  if (!open) return null;

  const charCount = value?.length ?? 0;
  const disabled = loading;

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!project?.id) return;
    try {
      setLoading(true);
      setErr("");
      const res = await fetch(`${API_BASE}/api/projects/${project.id}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${getToken()}`,
        },
        body: JSON.stringify({ description: value }),
      });
      if (!res.ok) throw new Error((await res.text().catch(() => "")) || "Update failed");
      const updated = await res.json();
      onSaved?.(updated);
      onClose?.();
    } catch (e2) {
      setErr(e2?.message || "Update failed");
    } finally {
      setLoading(false);
    }
  };

  const stop = (e) => e.stopPropagation();

  return (
    <div className="cds-overlay" onClick={onClose} role="presentation">
      <div className="cds-modal" role="dialog" aria-modal="true" ref={dialogRef} onClick={stop}>
        <div className="cds-head">
          <h3 className="cds-title">Change description</h3>
          <button type="button" className="cds-x" onClick={onClose} aria-label="Close">×</button>
        </div>

        <form onSubmit={handleSubmit}>
          <label className="cds-label" htmlFor="cds-textarea">Description</label>
          <textarea
            id="cds-textarea"
            ref={textareaRef}
            className="cds-textarea"
            rows={6}
            maxLength={2000}
            value={value}
            onChange={(e) => setValue(e.target.value)}
            placeholder="Describe your project so others can get on board…"
          />
          <div className="cds-meta">
            <span className="cds-count">{charCount}/2000</span>
            {err && <span className="cds-error">{err}</span>}
          </div>

          <div className="cds-actions">
            <button type="button" className="cds-btn" onClick={onClose} disabled={disabled}>
              Cancel
            </button>
            <button type="submit" className="cds-btn cds-primary" disabled={disabled}>
              {loading ? "Saving…" : "Save"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
