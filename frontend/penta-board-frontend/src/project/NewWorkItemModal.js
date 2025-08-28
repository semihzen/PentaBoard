import { useEffect, useMemo, useState } from "react";
import { API_BASE, getToken } from "../utils/auth";
import "./NewWorkItemModal.css";

export default function NewWorkItemModal({
  open,
  onClose,
  projectId,
  boardId,
  onCreated,
}) {
  const [title, setTitle] = useState("");
  const [desc, setDesc] = useState("");
  const [type, setType] = useState("Task");  // serbest yazılacak
  const [prio, setPrio] = useState("");
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState("");

  // ---- Assignee search ----
  const [members, setMembers] = useState([]);
  const [query, setQuery] = useState("");
  const [assignee, setAssignee] = useState(null);
  const [dropOpen, setDropOpen] = useState(false);

  const filtered = useMemo(() => {
    const q = (query || "").toLowerCase();
    if (!q) return members.slice(0, 8);
    return members
      .filter(m =>
        (m.name || "").toLowerCase().includes(q) ||
        (m.email || "").toLowerCase().includes(q)
      )
      .slice(0, 8);
  }, [members, query]);

  useEffect(() => {
    if (!open || !projectId) return;
    (async () => {
      try {
        const res = await fetch(`${API_BASE}/api/projects/${projectId}/members`, {
          headers: { Authorization: `Bearer ${getToken()}` },
        });
        if (!res.ok) throw new Error("Failed to load members");
        const data = await res.json();

        // backend: { items: [...] }
        const raw = data?.items ?? data?.members ?? data ?? [];
        const arr = Array.isArray(raw) ? raw : [];
        const list = arr.map(x => {
          const first = x.firstName ?? "";
          const last = x.lastName ?? "";
          const name = ((x.name ?? x.fullName ?? `${first} ${last}`) || "").trim();
          return {
            userId: x.userId ?? x.id ?? x.user?.id,
            name: name || x.email || "Unknown",
            email: x.email ?? x.user?.email ?? "",
            avatarUrl: x.avatarUrl ?? null,
          };
        }).filter(x => x.userId);

        setMembers(list);
      } catch {
        setMembers([]);
      }
    })();
  }, [open, projectId]);

  useEffect(() => {
    if (!open) {
      setTitle(""); setDesc(""); setType("Task"); setPrio("");
      setAssignee(null); setQuery(""); setDropOpen(false); setErr("");
    }
  }, [open]);

  if (!open) return null;

  const submit = async () => {
    const t = (title || "").trim();
    if (!t) return setErr("Title required");
    try {
      setBusy(true); setErr("");
      const body = {
        Title: t,
        Description: desc || null,
        Type: (type || "Task").trim(),
        Priority: prio ? Number(prio) : null,
        AssigneeId: assignee?.userId || null,
      };
      const res = await fetch(`${API_BASE}/api/projects/${projectId}/boards/${boardId}/workitems`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${getToken()}` },
        body: JSON.stringify(body),
      });
      if (!res.ok) throw new Error(await res.text().catch(() => "Create failed"));
      const dto = await res.json();
      onCreated?.({ dto, assignee });
      onClose();
    } catch (e) {
      setErr(e.message || "Create failed");
    } finally { setBusy(false); }
  };

  return (
    <div className="nim-backdrop" onClick={(e)=>{ if (e.target.classList.contains("nim-backdrop")) onClose(); }}>
      <div className="nim-card">
        <div className="nim-head">
          <div>New Task</div>
          <button className="nim-x" onClick={onClose} aria-label="Close">×</button>
        </div>

        <div className="nim-body">
          <label>Title</label>
          <input className="input" value={title} onChange={e=>setTitle(e.target.value)} autoFocus />

          <label>Description</label>
          <textarea className="input" rows={3} value={desc} onChange={e=>setDesc(e.target.value)} />

          <div className="nim-row">
            <div className="nim-col">
              <label>Type</label>
              <input
                className="input"
                placeholder="Task | Bug | Story | Issue …"
                value={type}
                onChange={(e)=>setType(e.target.value)}
              />
            </div>

            <div className="nim-col">
              <label>Priority</label>
              <div className="nim-input-with-hint">
                <input
                  className="input nim-input-prio"
                  placeholder="1-5"
                  value={prio}
                  onChange={(e)=>setPrio(e.target.value)}
                />
                <span className="nim-hint">(1 = highest priority)</span>
              </div>
            </div>
          </div>

          {/* Assignee */}
          <div className="nim-row">
            <div className="nim-col">
              <label>Assignee</label>
              <div className="nim-assignee">
                <input
                  className="input"
                  placeholder="Search members…"
                  value={assignee ? assignee.name : query}
                  onChange={(e)=>{ setAssignee(null); setQuery(e.target.value); setDropOpen(true); }}
                  onFocus={()=>setDropOpen(true)}
                />
                {assignee && (
                  <button className="nim-chip" onClick={()=>{ setAssignee(null); setQuery(""); setDropOpen(true); }}>
                    {assignee.name} ✕
                  </button>
                )}
                {dropOpen && (
                  <div className="nim-drop">
                    {filtered.length === 0 && <div className="nim-empty">No members</div>}
                    {filtered.map(m => (
                      <div
                        key={m.userId}
                        className="nim-opt"
                        onMouseDown={()=>{ setAssignee(m); setQuery(""); setDropOpen(false); }}
                        title={m.email}
                      >
                        <div className="nim-avatar">{(m.name||"?").slice(0,1).toUpperCase()}</div>
                        <div className="nim-opt-main">
                          <div className="nim-opt-name">{m.name}</div>
                          <div className="nim-opt-sub">{m.email}</div>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>

          {err && <div className="nim-err">{err}</div>}
        </div>

        <div className="nim-actions">
          <button className="btn" onClick={onClose} disabled={busy}>Cancel</button>
          <button className="btn btn-primary" onClick={submit} disabled={busy}>Create</button>
        </div>
      </div>
    </div>
  );
}
