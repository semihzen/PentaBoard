import { useEffect, useState } from 'react';
import { createPortal } from 'react-dom';
import { API_BASE, getToken } from '../utils/auth';
import './SetRoleModal.css';

export default function SetRoleModal({
  open,
  projectId,
  userId,
  name,
  current,
  onClose,
  onSaved,
}) {
  const [value, setValue] = useState(current || 'member');
  const [saving, setSaving] = useState(false);

  // Modal açılınca inputu güncelle
  useEffect(() => { setValue(current || 'member'); }, [current, open]);

  // Arka plan scroll kilidi + Escape ile kapama
  useEffect(() => {
    if (!open) return;
    const prevOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';

    const onKey = (e) => {
      if (e.key === 'Escape') onClose?.();
    };
    window.addEventListener('keydown', onKey);

    return () => {
      document.body.style.overflow = prevOverflow;
      window.removeEventListener('keydown', onKey);
    };
  }, [open, onClose]);

  if (!open) return null;

  const save = async () => {
    try {
      setSaving(true);
      const clean = (value ?? '').trim() || 'member';
      await fetch(`${API_BASE}/api/projects/${projectId}/members/${userId}/role`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${getToken()}`,
        },
        body: JSON.stringify({ SubRole: clean }),
      });
      onSaved?.();
      onClose?.();
    } catch (e) {
      console.error(e);
      alert('Failed to set role');
    } finally {
      setSaving(false);
    }
  };

  const modal = (
    <div
      className="pb-modal-backdrop"
      role="dialog"
      aria-modal="true"
      onClick={(e) => { if (e.target === e.currentTarget) onClose?.(); }}
    >
      <div className="pb-modal">
        <div className="pb-modal-head">
          <h3 className="pb-h3">Set role</h3>
        </div>

        <div className="pb-modal-body">
          <div className="pb-modal-row">
            <div className="pb-modal-label">Member</div>
            <div className="pb-modal-value">{name}</div>
          </div>

          <div className="pb-modal-row">
            <div className="pb-modal-label">Sub role</div>
            <div>
              <input
                className="pb-input"
                value={value}
                onChange={(e) => setValue(e.target.value)}
                onKeyDown={(e) => { if (e.key === 'Enter') save(); }}
                placeholder="ör: frontend, backend, designer…"
                maxLength={32}
                autoFocus
              />
              <small className="pb-help">Boş kalırsa “member” olarak kaydedilir.</small>
            </div>
          </div>
        </div>

        <div className="pb-modal-actions">
          <button className="pb-btn" onClick={onClose} disabled={saving}>Cancel</button>
          <button className="pb-btn pb-btn-primary" onClick={save} disabled={saving}>
            {saving ? 'Saving…' : 'Save'}
          </button>
        </div>
      </div>
    </div>
  );

  return createPortal(modal, document.body);
}
