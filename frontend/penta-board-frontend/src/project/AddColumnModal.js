import { useEffect, useState } from "react";
import "./AddColumnModal.css";
import { API_BASE, getToken } from "../utils/auth";

export default function AddColumnModal({ open, projectId, afterId, onClose, onAdded }) {
  const [name, setName] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (open) {
      setName("");
      setSaving(false);
    }
  }, [open]);

  if (!open) return null;

  const save = async () => {
    const trimmed = (name || "").trim();
    if (!trimmed || !projectId) return;

    try {
      setSaving(true);
      const res = await fetch(`${API_BASE}/api/projects/${projectId}/board/columns`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${getToken()}`
        },
        body: JSON.stringify({
          name: trimmed,
          color: null,
          wipLimit: null,
          isDoneLike: false,
          insertAfterColumnId: afterId || null
        })
      });
      if (!res.ok) {
        const txt = await res.text().catch(() => "");
        throw new Error(txt || "Failed to add column");
      }
      const dto = await res.json();
      onAdded?.(dto);
      onClose?.();
    } catch (e) {
      console.error(e);
      alert(e.message || "Failed to add column");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div
      className="pb-modal-backdrop"
      onClick={(e) => { if (e.target === e.currentTarget) onClose?.(); }}
    >
      <div className="pb-modal pb-modal-sm">
        <div className="pb-modal-head">
          <h3 className="pb-h3">New column</h3>
        </div>

        <div className="pb-modal-body">
          <label className="pb-modal-label">Name</label>
          <input
            className="input wfull"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="e.g. Review"
            autoFocus
            onKeyDown={(e) => { if (e.key === "Enter" && name.trim()) save(); }}
          />
        </div>

        <div className="pb-modal-actions">
          <button className="btn" onClick={onClose} disabled={saving}>Cancel</button>
          <button className="btn btn-primary" onClick={save} disabled={saving || !name.trim()}>
            {saving ? "Saving…" : "Create"}
          </button>
        </div>
      </div>
    </div>
  );
}
