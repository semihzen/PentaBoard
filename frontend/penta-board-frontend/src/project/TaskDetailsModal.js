import React, { useEffect } from "react";
import "./TaskDetailModal.css";

export default function TaskDetailsModal({
  open,
  onClose,
  task,           // { id, title, type, priority, assigneeId, description? }
  assigneeLabel,  // string
}) {
  // ESC ile kapat
  useEffect(() => {
    if (!open) return;
    const onKey = (e) => e.key === "Escape" && onClose?.();
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [open, onClose]);

  if (!open || !task) return null;

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
          <div className="pb-modal-title">{task.title}</div>
          <button className="icon-btn" title="Close" onClick={onClose} aria-label="Close">✕</button>
        </div>

        <div className="pb-modal-body">
          <div className="pb-row">
            <span className="pb-label">Type</span>
            <span className="pill">{task.type || "Task"}</span>
          </div>

          <div className="pb-row">
            <span className="pb-label">Priority</span>
            {task.priority ? (
              <span className={`prio p-${task.priority}`}>P{task.priority}</span>
            ) : (
              <span className="pb-muted">Not set</span>
            )}
          </div>

          <div className="pb-row">
            <span className="pb-label">Assignee</span>
            <span>{assigneeLabel || "Unassigned"}</span>
          </div>

          <div className="pb-row col">
            <span className="pb-label">Description</span>
            <div className="pb-desc">
              {task.description?.trim()
                ? task.description
                : <span className="pb-muted">No description</span>}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
