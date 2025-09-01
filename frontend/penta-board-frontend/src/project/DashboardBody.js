import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import "./DashboardBody.css";
import { API_BASE, getToken } from "../utils/auth";
import AddColumnModal from "./AddColumnModal";
import NewWorkItemModal from "./NewWorkItemModal";
import TaskDetailsModal from "./TaskDetailsModal";

export default function DashboardBody({ project }) {
  // ---------- Board & Columns ----------
  const [board, setBoard] = useState(null);
  const [columns, setColumns] = useState([]);
  const [loadingCols, setLoadingCols] = useState(true);
  const [errCols, setErrCols] = useState("");

  // ---------- Current user ----------
  const [me, setMe] = useState(null); // /api/users/me -> { id, role, ... }

  // ---------- Members (labels & roles) ----------
  const [memberMap, setMemberMap] = useState(new Map());         // userId -> label
  const [memberRoleMap, setMemberRoleMap] = useState(new Map()); // userId -> projectRole

  // ---------- Details modal ----------
  const [detailOpen, setDetailOpen] = useState(false);
  const [detailItem, setDetailItem] = useState(null);

  // ---------- Items (local UI state) ----------
  const storageKey = useMemo(
    () => (board?.id ? `pb.dashboard.items.${board.id}` : null),
    [board?.id]
  );
  // item: { id, title, colId, type, priority, assigneeId, description?, projectId?, boardId? }
  const [items, setItems] = useState([]);

  // Column rename UI
  const [editingColId, setEditingColId] = useState(null);
  const [editingName, setEditingName] = useState("");

  // DnD
  const dragItemId = useRef(null);

  // Modals
  const [addOpen, setAddOpen] = useState(false);
  const [createOpen, setCreateOpen] = useState(false);

  // ===== Load current user =====
  useEffect(() => {
    (async () => {
      try {
        const res = await fetch(`${API_BASE}/api/users/me`, {
          headers: { Authorization: `Bearer ${getToken()}` },
        });
        if (!res.ok) throw new Error();
        const data = await res.json();
        setMe(data || null);
      } catch {
        setMe(null);
      }
    })();
  }, []);

  // ===== Load board & columns =====
  const loadBoard = useCallback(async () => {
    if (!project?.id) return;
    try {
      setLoadingCols(true);
      setErrCols("");
      const res = await fetch(`${API_BASE}/api/projects/${project.id}/board`, {
        headers: { Authorization: `Bearer ${getToken()}` },
      });
      if (!res.ok) throw new Error(await res.text().catch(() => "Failed to load board"));
      const data = await res.json();

      setBoard({ id: data.id, projectId: data.projectId, name: data.name });

      const cols = (data.columns ?? [])
        .slice()
        .sort((a, b) => a.orderKey - b.orderKey)
        .map((c) => ({
          id: c.id,
          name: c.name,
          orderKey: c.orderKey,
          locked: !!(
            c.isDefault ||
            c.isDoneLike ||
            ["to do", "doing", "done"].includes((c.name || "").toLowerCase())
          ),
          isDefault: !!c.isDefault,
          isDoneLike: !!c.isDoneLike,
        }));
      setColumns(cols);
    } catch (e) {
      console.error(e);
      setErrCols(e.message || "Failed to load board");
      setBoard(null);
      setColumns([]);
    } finally {
      setLoadingCols(false);
    }
  }, [project?.id]);

  useEffect(() => {
    loadBoard();
  }, [loadBoard]); // ✅ eslint uyarısı çözüldü

  // ===== Load members (labels + project roles) =====
  useEffect(() => {
    if (!project?.id) return;
    (async () => {
      try {
        const res = await fetch(`${API_BASE}/api/projects/${project.id}/members`, {
          headers: { Authorization: `Bearer ${getToken()}` },
        });
        if (!res.ok) throw new Error();
        const data = await res.json();
        const raw = data?.items ?? data?.members ?? data ?? [];

        const map = new Map();
        const roles = new Map();
        raw.forEach((m) => {
          const id = m.userId ?? m.id;
          const first = m.firstName ?? "";
          const last = m.lastName ?? "";
          const name = `${first} ${last}`.trim();
          const label = name || m.email || "User";
          if (id) {
            map.set(id, label);
            const role = m.role ?? m.memberRole ?? m.projectRole ?? null;
            if (role) roles.set(id, role);
          }
        });

        setMemberMap(map);
        setMemberRoleMap(roles);
      } catch {
        setMemberMap(new Map());
        setMemberRoleMap(new Map());
      }
    })();
  }, [project?.id]);

  // ===== Persist items (local only) =====
  useEffect(() => {
    if (!storageKey) return;
    try {
      const raw = localStorage.getItem(storageKey);
      if (raw) {
        const parsed = JSON.parse(raw);
        if (Array.isArray(parsed)) {
          setItems(
            parsed.map((it) => ({
              ...it,
              projectId: it.projectId ?? project?.id,
              boardId: it.boardId ?? board?.id,
            }))
          );
        }
      }
    } catch {}
  }, [storageKey, project?.id, board?.id]);

  useEffect(() => {
    if (!storageKey) return;
    try {
      localStorage.setItem(storageKey, JSON.stringify(items));
    } catch {}
  }, [items, storageKey]);

  const deleteItem = async (id) => {
    if (!project?.id || !board?.id) return;
    try {
      const res = await fetch(
        `${API_BASE}/api/projects/${project.id}/boards/${board.id}/workitems/${id}`,
        { method: "DELETE", headers: { Authorization: `Bearer ${getToken()}` } }
      );
      if (!res.ok) {
        const txt = await res.text().catch(() => "");
        throw new Error(txt || "Delete failed");
      }
      setItems((prev) => prev.filter((i) => i.id !== id));
    } catch (e) {
      alert(e.message || "Delete failed");
    }
  };

  const onDragStart = (e, itemId) => {
    dragItemId.current = itemId;
    e.dataTransfer.effectAllowed = "move";
  };
  const onDragOverCol = (e) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = "move";
  };

  // >>> Sunucuya MOVE atan yardımcı
  const persistMove = async (workItemId, targetColId) => {
    const url = `${API_BASE}/api/projects/${project.id}/boards/${board.id}/workitems/${workItemId}/move`;
    const res = await fetch(url, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${getToken()}`,
      },
      body: JSON.stringify({
        projectId: project.id,
        boardId: board.id,
        workItemId: workItemId,
        targetColumnId: targetColId,
      }),
    });
    if (!res.ok) {
      const txt = await res.text().catch(() => "");
      throw new Error(txt || "Move failed");
    }
    return res.json().catch(() => ({}));
  };

  const onDropToCol = async (e, colId) => {
    e.preventDefault();
    const itemId = dragItemId.current;
    dragItemId.current = null;
    if (!itemId) return;

    const item = items.find((i) => i.id === itemId);
    if (!item || item.colId === colId) return;

    const prevItems = items;
    setItems((prev) => prev.map((it) => (it.id === itemId ? { ...it, colId } : it)));

    try {
      await persistMove(itemId, colId);
    } catch (err) {
      console.error(err);
      alert(err.message || "Move failed");
      setItems(prevItems); // rollback
    }
  };

  // ===== Rename column =====
  const renameColumn = async (columnId, newName) => {
    if (!project?.id) return;
    const res = await fetch(
      `${API_BASE}/api/projects/${project.id}/board/columns/${columnId}/name`,
      {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${getToken()}`,
        },
        body: JSON.stringify({ name: newName }),
      }
    );
    if (!res.ok) throw new Error(await res.text().catch(() => "Failed to rename"));
    const dto = await res.json();
    setColumns((prev) =>
      prev
        .map((c) =>
          c.id === columnId
            ? {
                ...c,
                name: dto.name,
                orderKey: dto.orderKey,
                isDefault: !!dto.isDefault,
                isDoneLike: !!dto.isDoneLike,
              }
            : c
        )
        .sort((a, b) => a.orderKey - b.orderKey)
    );
  };

  const startRename = (col) => {
    setEditingColId(col.id);
    setEditingName(col.name);
  };
  const cancelRename = () => {
    setEditingColId(null);
    setEditingName("");
  };
  const saveRename = async () => {
    const name = (editingName || "").trim();
    if (!name || !editingColId) {
      cancelRename();
      return;
    }
    try {
      await renameColumn(editingColId, name);
    } catch (e) {
      alert(e.message || "Rename failed");
    } finally {
      cancelRename();
    }
  };

  // ===== Delete column =====
  const deleteColumn = async (columnId) => {
    if (!project?.id) return;
    const res = await fetch(
      `${API_BASE}/api/projects/${project.id}/board/columns/${columnId}`,
      { method: "DELETE", headers: { Authorization: `Bearer ${getToken()}` } }
    );
    if (!res.ok) throw new Error(await res.text().catch(() => "Failed to delete"));
    const data = await res.json();
    const list = data.columns ?? [];
    const cols = list
      .slice()
      .sort((a, b) => a.orderKey - b.orderKey)
      .map((c) => ({
        id: c.id,
        name: c.name,
        orderKey: c.orderKey,
        locked: !!(
          c.isDefault ||
          c.isDoneLike ||
          ["to do", "doing", "done"].includes((c.name || "").toLowerCase())
        ),
        isDefault: !!c.isDefault,
        isDoneLike: !!c.isDoneLike,
      }));
    setColumns(cols);

    if (cols.length) {
      const fallback = cols.find((c) => c.isDefault) || cols[0];
      setItems((prev) =>
        prev.map((i) => (!cols.some((c) => c.id === i.colId) ? { ...i, colId: fallback.id } : i))
      );
    } else {
      setItems([]);
    }
  };

  // ===== Move column =====
  const moveColumn = async (columnId, targetIndex) => {
    if (!project?.id) return;
    try {
      const res = await fetch(
        `${API_BASE}/api/projects/${project.id}/board/columns/${columnId}/move`,
        {
          method: "PUT",
          headers: { "Content-Type": "application/json", Authorization: `Bearer ${getToken()}` },
          body: JSON.stringify({ targetIndex }),
        }
      );
      if (!res.ok) throw new Error(await res.text().catch(() => "Failed to move column"));
      const data = await res.json();
      const list = Array.isArray(data) ? data : data.columns ?? [];
      const cols = list
        .slice()
        .sort((a, b) => a.orderKey - b.orderKey)
        .map((c) => ({
          id: c.id,
          name: c.name,
          orderKey: c.orderKey,
          locked: !!(
            c.isDefault ||
            c.isDoneLike ||
            ["to do", "doing", "done"].includes((c.name || "").toLowerCase())
          ),
          isDefault: !!c.isDefault,
          isDoneLike: !!c.isDoneLike,
        }));
      setColumns(cols);
    } catch (e) {
      console.error(e);
      alert(e.message || "Failed to move column");
    }
  };

  // ===== Permissions =====
  const isGlobalAdmin = (role) => {
    if (!role) return false;
    const r = String(role).toLowerCase();
    // "System Admin" veya "Admin" (normal admin)
    return (
      (r.includes("system") && r.includes("admin")) ||
      r === "admin" ||
      r.endsWith(" admin") ||
      r.includes(" admin")
    );
  };
  const isProjectAdmin = (userId) => {
    const role = memberRoleMap.get(userId);
    if (!role) return false;
    const r = String(role).toLowerCase();
    return r.includes("admin");
  };
  const canManageColumns = !!me && (isGlobalAdmin(me.role) || isProjectAdmin(me.id));

  // ===== Derived =====
  const grouped = useMemo(() => {
    const map = new Map(columns.map((c) => [c.id, []]));
    items.forEach((it) => {
      if (!map.has(it.colId)) map.set(it.colId, []);
      map.get(it.colId).push(it);
    });
    return map;
  }, [columns, items]);

  const handleColumnAdded = (dto) => {
    const newCol = {
      id: dto.id,
      name: dto.name,
      orderKey: dto.orderKey,
      locked: !!(dto.isDefault || dto.isDoneLike),
      isDefault: !!dto.isDefault,
      isDoneLike: !!dto.isDoneLike,
    };
    setColumns((prev) => [...prev, newCol].sort((a, b) => a.orderKey - b.orderKey));
  };

  const assigneeName = (userId) =>
    userId && memberMap.get(userId) ? memberMap.get(userId) : "Unassigned";
  const prioClass = (p) => (p >= 1 && p <= 5 ? `prio p-${p}` : "");

  return (
    <div className="dash-wrap">
      {/* Top bar */}
      <div className="dash-top">
        <div className="dash-new">
          <button
            className="btn btn-primary"
            onClick={() => setCreateOpen(true)}
            title="Add work item"
            disabled={!columns.length || !board}
          >
            + New Task
          </button>
        </div>

        <div className="dash-tools">
          <button
            className="btn"
            onClick={() => {
              if (canManageColumns) setAddOpen(true);
            }}
            title={canManageColumns ? "Add column" : "Only admins can add columns"}
            disabled={!canManageColumns}
          >
            + Add column
          </button>
          <button className="btn" onClick={loadBoard} title="Refresh columns">
            ↻ Refresh
          </button>
        </div>
      </div>

      {/* Board */}
      {loadingCols ? (
        <div className="pb-muted">Loading board…</div>
      ) : errCols ? (
        <div className="pb-muted">Failed to load board: {errCols}</div>
      ) : (
        <div className="board">
          {columns.map((col, idx) => (
            <div
              key={col.id}
              className="col"
              onDragOver={onDragOverCol}
              onDrop={(e) => onDropToCol(e, col.id)}
            >
              <div className="col-head">
                {editingColId === col.id ? (
                  <div className="col-edit">
                    <input
                      className="input col-rename"
                      value={editingName}
                      onChange={(e) => setEditingName(e.target.value)}
                      onKeyDown={(e) => {
                        if (e.key === "Enter") saveRename();
                        if (e.key === "Escape") cancelRename();
                      }}
                      autoFocus
                    />
                    <button className="icon-btn success" title="Save name" onClick={saveRename}>
                      ✓
                    </button>
                    <button className="icon-btn" title="Cancel" onClick={cancelRename}>
                      ✕
                    </button>
                  </div>
                ) : (
                  <>
                    <div
                      className="col-title"
                      title="Click to rename"
                      onClick={() => startRename(col)}
                    >
                      {col.name}
                    </div>
                    <div className="col-meta">{(grouped.get(col.id) || []).length}</div>
                  </>
                )}

                <div className="col-actions">
                  <button
                    className="icon-btn"
                    title="Move left"
                    disabled={idx === 0}
                    onClick={() => moveColumn(col.id, idx - 1)}
                  >
                    ←
                  </button>
                  <button
                    className="icon-btn"
                    title="Move right"
                    disabled={idx === columns.length - 1}
                    onClick={() => moveColumn(col.id, idx + 1)}
                  >
                    →
                  </button>
                  <button
                    className="icon-btn danger"
                    title="Delete column"
                    onClick={async () => {
                      if (!window.confirm(`Delete column "${col.name}"?`)) return;
                      try {
                        await deleteColumn(col.id);
                      } catch (e) {
                        alert(e.message || "Delete failed");
                      }
                    }}
                  >
                    x
                  </button>
                </div>
              </div>

              <div className="col-body">
                {(grouped.get(col.id) || []).map((it) => (
                  <div
                    key={it.id}
                    className={`card ${it.priority ? `prio-${it.priority}` : ""}`}
                    draggable
                    onDragStart={(e) => onDragStart(e, it.id)}
                    title="Drag to move"
                  >
                    {/* MAIN içerik – karta tıklayınca detay modalı */}
                    <div
                      className="card-main"
                      onClick={() => {
                        setDetailItem({ ...it, projectId: project?.id, boardId: board?.id });
                        setDetailOpen(true);
                      }}
                    >
                      <div className="card-title">{it.title}</div>
                      <div className="card-meta">
                        <span className="pill">{it.type || "Task"}</span>
                        <span className="meta-sep">•</span>
                        <span className="muted">{assigneeName(it.assigneeId)}</span>
                        {it.priority ? (
                          <>
                            <span className="meta-sep">•</span>
                            <span className={prioClass(it.priority)}>P{it.priority}</span>
                          </>
                        ) : null}
                      </div>
                    </div>
                    <button className="icon-btn" title="Delete" onClick={() => deleteItem(it.id)}>
                      ✕
                    </button>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Add Column Modal */}
      <AddColumnModal
        open={addOpen}
        projectId={project?.id}
        afterId={columns.length ? columns[columns.length - 1].id : null}
        onClose={() => setAddOpen(false)}
        onAdded={handleColumnAdded}
      />

      {/* New Work Item Modal */}
      <NewWorkItemModal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        projectId={project?.id}
        boardId={board?.id}
        onCreated={({ dto, assignee }) => {
          setItems((prev) => [
            ...prev,
            {
              id: dto.id,
              title: dto.title,
              colId: dto.boardColumnId,
              type: dto.type || "Task",
              priority: dto.priority ?? null,
              assigneeId: assignee?.userId ?? dto.assigneeId ?? null,
              projectId: project?.id,
              boardId: board?.id,
            },
          ]);
        }}
      />

      {/* Task Details Modal */}
      <TaskDetailsModal
        open={detailOpen}
        onClose={() => setDetailOpen(false)}
        task={detailItem}
        assigneeLabel={assigneeName(detailItem?.assigneeId)}
      />
    </div>
  );
}
