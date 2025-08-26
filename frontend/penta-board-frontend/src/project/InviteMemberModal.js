import React, { useEffect, useMemo, useState } from "react";
import "./InviteMemberModal.css";
import { API_BASE, getToken } from "../utils/auth";

export default function InviteMemberModal({ open, project, onClose, onAdded }) {
  const [loading, setLoading] = useState(false);
  const [users, setUsers] = useState([]);
  const [query, setQuery] = useState("");
  const [selectedUserId, setSelectedUserId] = useState(null);
  const [subRole, setSubRole] = useState("");

  useEffect(() => {
    if (open) {
      setQuery("");
      setSelectedUserId(null);
      setSubRole("");
      fetchUsers();
    }
  }, [open]);

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const res = await fetch(`${API_BASE}/api/users`, {
        headers: { Authorization: `Bearer ${getToken()}` },
      });
      if (!res.ok) throw new Error(await res.text());
      const data = await res.json();
      setUsers(Array.isArray(data) ? data : (data.items ?? []));
    } catch (e) {
      console.error(e);
      alert("Users could not be loaded.");
    } finally {
      setLoading(false);
    }
  };

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();

    // arama yoksa hiçbir şey göstermeyelim
    if (!q) return [];

    const isAdmin = (r) => (r || "").toLowerCase() === "admin";
    const ownerId = project?.projectAdminId?.toLowerCase?.();

    return users
      // admin ve proje sahibini gizle
      .filter((u) => !isAdmin(u.role) && u.id?.toLowerCase?.() !== ownerId)
      // e-posta öncelikli, ad/soyad destekli
      .filter((u) => {
        const full = `${u.firstName ?? ""} ${u.lastName ?? ""}`.trim().toLowerCase();
        const mail = (u.email || "").toLowerCase();
        return mail.includes(q) || full.includes(q);
      });
  }, [users, query, project]);

  const canSubmit = !!selectedUserId && subRole.trim().length > 0;

  const handleAdd = async () => {
    if (!canSubmit) return;
    try {
      setLoading(true);
      const res = await fetch(`${API_BASE}/api/projects/${project.id}/members`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${getToken()}`,
        },
        body: JSON.stringify({
          userId: selectedUserId,
          subRole: subRole.trim(),
        }),
      });
      if (!res.ok) throw new Error(await res.text().catch(() => "Add member failed"));
      onAdded?.();
      onClose?.();
    } catch (e) {
      console.error(e);
      alert("Member could not be added.");
    } finally {
      setLoading(false);
    }
  };

  if (!open) return null;

  return (
    <div className="imodal-backdrop" onMouseDown={onClose}>
      <div className="imodal" onMouseDown={(e) => e.stopPropagation()}>
        <div className="imodal-head">
          <h3 className="imodal-title">Add member to “{project?.name ?? "Project"}”</h3>
          <button className="imodal-x" onClick={onClose} aria-label="Close">×</button>
        </div>

        <div className="imodal-body">
          <label className="imodal-label">Search user</label>
          <input
            className="imodal-input"
            placeholder="Type email (e.g. user@company.com)…"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
          />

          <div className="imodal-list">
            {loading ? (
              <div className="imodal-empty">Loading…</div>
            ) : query.trim().length === 0 ? (
              <div className="imodal-empty">Start typing to search users by email</div>
            ) : filtered.length === 0 ? (
              <div className="imodal-empty">No matching users</div>
            ) : (
              filtered.map((u) => {
                const full = `${u.firstName ?? ""} ${u.lastName ?? ""}`.trim() || u.email;
                return (
                  <label key={u.id} className="imodal-row">
                    <input
                      type="radio"
                      name="userPick"
                      value={u.id}
                      checked={selectedUserId === u.id}
                      onChange={() => setSelectedUserId(u.id)}
                    />
                    <div className="imodal-row-main">
                      <div className="imodal-row-name">{full}</div>
                      <div className="imodal-row-sub">{u.email}</div>
                    </div>
                  </label>
                );
              })
            )}
          </div>

          <div className="imodal-field">
            <label className="imodal-label">Sub role</label>
            <input
              className="imodal-input"
              placeholder="e.g. backend, frontend, qa…"
              value={subRole}
              onChange={(e) => setSubRole(e.target.value)}
            />
          </div>
        </div>

        <div className="imodal-foot">
          <button className="imodal-btn ghost" onClick={onClose}>Cancel</button>
          <button className="imodal-btn primary" disabled={!canSubmit || loading} onClick={handleAdd}>
            {loading ? "Adding…" : "Add member"}
          </button>
        </div>
      </div>
    </div>
  );
}
