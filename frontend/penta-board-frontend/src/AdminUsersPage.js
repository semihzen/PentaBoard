import { useEffect, useMemo, useState } from "react";
import { Search, RefreshCcw } from "lucide-react";
import { API_BASE, getToken } from "./utils/auth";
import "./UsersPage.css";

export default function AdminUsersPage() {
  const [users, setUsers] = useState([]);
  const [q, setQ] = useState("");
  const [role, setRole] = useState("all");
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  const load = async () => {
    try {
      setLoading(true);
      setErr("");
      const res = await fetch(`${API_BASE}/api/users`, {
        headers: { Authorization: `Bearer ${getToken()}` },
      });
      if (!res.ok) {
        if (res.status === 401) throw new Error("Unauthorized (401)");
        if (res.status === 403) throw new Error("Forbidden (403): You don't have access");
        throw new Error((await res.text()) || `HTTP ${res.status}`);
      }
      const data = await res.json();
      setUsers(Array.isArray(data) ? data : data.items ?? []);
    } catch (e) {
      setErr(e.message || "Failed to load users");
    } finally {
      setLoading(false);
    }
  };

  // 🔧 İLK AÇILIŞTA YÜKLE (eksik olan buydu)
  useEffect(() => { load(); }, []);

  // Davet sonrası otomatik yenile
  useEffect(() => {
    const h = () => load();
    window.addEventListener("users:refresh", h);
    return () => window.removeEventListener("users:refresh", h);
  }, []);

  const roles = useMemo(() => {
    const set = new Set(users.map(u => (u.role || "").trim()).filter(Boolean));
    return ["all", ...Array.from(set)];
  }, [users]);

  const filtered = useMemo(() => {
    const text = q.trim().toLowerCase();
    return users.filter(u => {
      const roleOk = role === "all" || (u.role || "") === role;
      if (!text) return roleOk;
      const full = `${u.firstName ?? ""} ${u.lastName ?? ""} ${u.email ?? ""}`.toLowerCase();
      return roleOk && full.includes(text);
    });
  }, [users, q, role]);

  return (
    <div className="users-wrap">
      <div className="users-head">
        <div className="users-title">
          <h2>Users</h2>
          <p className="muted">Manage people, roles and access.</p>
        </div>

        <div className="users-tools">
          <div className="users-search">
            <Search className="search-ic" />
            <input
              placeholder="Search by name or email"
              value={q}
              onChange={(e) => setQ(e.target.value)}
            />
          </div>

          <select
            className="users-select"
            value={role}
            onChange={(e) => setRole(e.target.value)}
          >
            {roles.map(r => (
              <option key={r} value={r}>
                {r === "all" ? "All roles" : r}
              </option>
            ))}
          </select>

          <button className="btn ghost" onClick={load} title="Refresh">
            <RefreshCcw size={16} /> Refresh
          </button>
        </div>
      </div>

      {err && <div className="banner error">{err}</div>}

      <div className="table">
        <div className="t-head">
          <div>Name</div>
          <div>Email</div>
          <div>Role</div>
        </div>

        {loading ? (
          <div className="t-loading">Loading users…</div>
        ) : filtered.length === 0 ? (
          <div className="t-empty">No users found.</div>
        ) : (
          filtered.map((u, i) => (
            <div key={u.id ?? i} className="t-row">
              <div className="cell name">
                <div className="avatar-sm">
                  {(u.firstName?.[0] || u.email?.[0] || "?").toUpperCase()}
                </div>
                <div>
                  <div className="strong">
                    {`${u.firstName ?? ""} ${u.lastName ?? ""}`.trim() || "—"}
                  </div>
                  <div className="muted small">{u.email?.split("@")[0] || "—"}</div>
                </div>
              </div>
              <div className="cell">{u.email || "—"}</div>
              <div className="cell">
                <span className="role-pill">{u.role || "User"}</span>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
