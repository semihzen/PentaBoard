import './DeleteConfirm.css';

export default function DeleteConfirm({
  open,
  title = 'Delete project',
  message = 'Are you sure you want to delete this project? This action cannot be undone.',
  confirmText = 'Delete',
  cancelText = 'Cancel',
  loading = false,
  onConfirm,
  onCancel,
}) {
  if (!open) return null;

  return (
    <div className="dc-backdrop" onMouseDown={onCancel}>
      <div className="dc-modal" role="dialog" aria-modal="true" onMouseDown={e => e.stopPropagation()}>
        <h3 className="dc-title">{title}</h3>
        <p className="dc-msg">{message}</p>
        <div className="dc-actions">
          <button className="dc-btn" onClick={onCancel} disabled={loading}>{cancelText}</button>
          <button className="dc-btn danger" onClick={onConfirm} disabled={loading}>
            {loading ? 'Deleting…' : confirmText}
          </button>
        </div>
      </div>
    </div>
  );
}
