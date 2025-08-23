import { useEffect, useRef, useState } from "react";
import { X, Mail, Shield, StickyNote } from "lucide-react";
import { API_BASE, getToken } from "./utils/auth";

export default function InviteUserModal({ open, onClose, onInvited }) {
  const [email, setEmail] = useState("");
  const [role, setRole] = useState("User");
  const [note, setNote] = useState("");
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState("");
  const [ok, setOk] = useState(false);
  const boxRef = useRef(null);

  useEffect(() => {
    if (!open) return;
    const onKey = (e) => e.key === "Escape" && onClose?.();
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [open, onClose]);

  useEffect(() => {
    if (!open) { setEmail(""); setRole("User"); setNote(""); setErr(""); setOk(false); }
  }, [open]);

  if (!open) return null;

  const validEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  const canSubmit = validEmail && !busy;

  const submit = async (e) => {
    e.preventDefault();
    if (!canSubmit) return;
    setBusy(true); setErr(""); setOk(false);
    try {
      const res = await fetch(`${API_BASE}/api/users/invite`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${getToken()}`,
        },
        body: JSON.stringify({ email, role, note: note.trim() || null }),
      });
      if (!res.ok) {
        const msg = await res.text();
        throw new Error(msg || `HTTP ${res.status}`);
      }
      setOk(true);
      onInvited?.(); // Users listesi yenilensin
      setTimeout(() => onClose?.(), 900);
    } catch (e) {
      setErr(e.message || "Failed to send invite");
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="modal-backdrop" onClick={(e) => e.target === e.currentTarget && onClose?.()}>
      <div className="modal" ref={boxRef} role="dialog" aria-modal="true" aria-label="Invite user">
        <div className="modal-head">
          <h3>User Invite</h3>
          <button className="icon-btn" onClick={onClose} aria-label="Close"><X /></button>
        </div>

        <form className="modal-body" onSubmit={submit}>
          <label className="field">
            <span>Email</span>
            <div className="input-ic">
              <Mail size={16} />
              <input
                type="email"
                placeholder="user@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                autoFocus
              />
            </div>
          </label>

          <label className="field">
            <span>Role</span>
            <div className="input-ic">
              <Shield size={16} />
              <select value={role} onChange={(e) => setRole(e.target.value)}>
                <option>User</option>
                <option>Admin</option>
                <option>System Admin</option>
              </select>
            </div>
          </label>

          <label className="field">
            <span>Note (optional)</span>
            <div className="input-ic textarea">
              <StickyNote size={16} />
              <textarea
                rows={3}
                placeholder="e.g. Project A board access"
                value={note}
                onChange={(e) => setNote(e.target.value)}
              />
            </div>
          </label>

          {err && <div className="banner error">{err}</div>}
          {ok  && <div className="banner success">Invite sent.</div>}

          <div className="modal-foot">
            <button type="button" className="btn ghost" onClick={onClose}>Cancel</button>
            <button type="submit" className="btn primary" disabled={!canSubmit}>
              {busy ? "Sending…" : "Send Invite"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
