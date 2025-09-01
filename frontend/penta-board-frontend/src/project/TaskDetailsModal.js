import React, { useEffect, useState } from "react";
import "./TaskDetailModal.css";
import { API_BASE, getToken } from "../utils/auth"; // ← API kökü + token

export default function TaskDetailsModal({ open, onClose, task, assigneeLabel }) {
  const [fullTask, setFullTask] = useState(task);
  const [loading, setLoading] = useState(false);

  // ESC ile kapat
  useEffect(() => {
    if (!open) return;
    const onKey = (e) => e.key === "Escape" && onClose?.();
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [open, onClose]);

  // 🔗 Doğrudan GUID’lerle detay çek: /api/projects/{projectId}/boards/{boardId}/workitems/{id}
  useEffect(() => {
    if (!open) return;

    // Gerekli id’ler yoksa dışarıdan gelen özet objeyi göster
    if (!task?.id || !task?.projectId || !task?.boardId) {
      setFullTask(task);
      return;
    }

    const controller = new AbortController();
    (async () => {
      try {
        setLoading(true);

        const url = `${API_BASE}/api/projects/${task.projectId}/boards/${task.boardId}/workitems/${task.id}`;
        const res = await fetch(url, {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${getToken()}`,
          },
          signal: controller.signal,
        });

        if (!res.ok) {
          const txt = await res.text().catch(() => "");
          console.error("WorkItem GET failed:", res.status, res.statusText, txt?.slice(0, 200));
          setFullTask(task);
          return;
        }

        const ct = res.headers.get("content-type") || "";
        if (!ct.includes("application/json")) {
          const txt = await res.text().catch(() => "");
          console.error("WorkItem GET non-JSON response:", ct, txt?.slice(0, 200));
          setFullTask(task);
          return;
        }

        const data = await res.json();
        setFullTask(data);
      } catch (err) {
        console.error("WorkItem GET error:", err);
        setFullTask(task);
      } finally {
        setLoading(false);
      }
    })();

    return () => controller.abort();
  }, [open, task]);

  if (!open || !fullTask) return null;

  return (
    <div
      className="pb-modal-backdrop"
      onClick={onClose}
      role="dialog"
      aria-modal="true"
      aria-label="Task details"
    >
      <div className="pb-modal" onClick={(e) => e.stopPropagation()}>
        <div className="pb-modal-head">
          <div className="pb-modal-title">{fullTask.title || "Work Item"}</div>
          <button className="icon-btn" onClick={onClose} aria-label="Close">
            ✕
          </button>
        </div>

        <div className="pb-modal-body">
          <div className="pb-row">
            <span className="pb-label">Type</span>
            <span className="pill">{fullTask.type || "Task"}</span>
          </div>

          <div className="pb-row">
            <span className="pb-label">Priority</span>
            {fullTask.priority ? (
              <span className={`prio p-${fullTask.priority}`}>P{fullTask.priority}</span>
            ) : (
              <span className="pb-muted">Not set</span>
            )}
          </div>

          {fullTask.severity !== undefined && (
            <div className="pb-row">
              <span className="pb-label">Severity</span>
              {fullTask.severity ? (
                <span className={`sev s-${fullTask.severity}`}>S{fullTask.severity}</span>
              ) : (
                <span className="pb-muted">Not set</span>
              )}
            </div>
          )}

          <div className="pb-row">
            <span className="pb-label">Assignee</span>
            <span>{assigneeLabel || "Unassigned"}</span>
          </div>

          <div className="pb-row col">
            <span className="pb-label">Description</span>
            <div className="pb-desc">
              {loading ? (
                <span className="pb-muted">Loading…</span>
              ) : fullTask.description?.trim() ? (
                fullTask.description
              ) : (
                <span className="pb-muted">No description</span>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
