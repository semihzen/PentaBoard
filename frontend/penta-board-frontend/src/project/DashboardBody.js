import { useEffect, useMemo, useRef, useState } from "react";
import "./DashboardBody.css";
import { API_BASE, getToken } from "../utils/auth";
import AddColumnModal from "./AddColumnModal";

export default function DashboardBody({ project }) {
  // ---------- Board & Columns (server) ----------
  const [board, setBoard] = useState(null);             // { id, projectId, name }
  const [columns, setColumns] = useState([]);           // { id, name, orderKey, locked, isDefault, isDoneLike }
  const [loadingCols, setLoadingCols] = useState(true);
  const [errCols, setErrCols] = useState("");

  // Work items (şimdilik local only)
  const storageKey = useMemo(
    () => (board?.id ? `pb.dashboard.items.${board.id}` : null),
    [board?.id]
  );
  const [items, setItems] = useState([]);               // { id, title, colId }
  const [newTitle, setNewTitle] = useState("");
  const inputRef = useRef(null);

  // Column rename UI
  const [editingColId, setEditingColId] = useState(null);
  const [editingName, setEditingName] = useState("");

  // DnD
  const dragItemId = useRef(null);

  // Modal: Add column
  const [addOpen, setAddOpen] = useState(false);

  // ========== Load board & columns from API ==========
  const loadBoard = async () => {
    if (!project?.id) return;
    try {
      setLoadingCols(true);
      setErrCols("");
      const res = await fetch(`${API_BASE}/api/projects/${project.id}/board`, {
        headers: { Authorization: `Bearer ${getToken()}` },
      });
      if (!res.ok) throw new Error(await res.text().catch(() => "Failed to load board"));
      const data = await res.json(); // { id, projectId, name, columns: [...] }

      setBoard({ id: data.id, projectId: data.projectId, name: data.name });

      // Normalize columns for UI
      const cols = (data.columns ?? [])
        .slice()
        .sort((a, b) => a.orderKey - b.orderKey)
        .map(c => ({
          id: c.id,
          name: c.name,
          orderKey: c.orderKey,
          locked: !!(c.isDefault || c.isDoneLike || ["to do", "doing", "done"].includes((c.name || "").toLowerCase())),
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
  };

  useEffect(() => { loadBoard(); }, [project?.id]);

  // ========== Persist items (board scoped, local only) ==========
  useEffect(() => {
    if (!storageKey) return;
    try {
      const raw = localStorage.getItem(storageKey);
      if (raw) {
        const parsed = JSON.parse(raw);
        if (Array.isArray(parsed)) setItems(parsed);
      }
    } catch {}
  }, [storageKey]);

  useEffect(() => {
    if (!storageKey) return;
    try { localStorage.setItem(storageKey, JSON.stringify(items)); } catch {}
  }, [items, storageKey]);

  // ========== Actions (items – local only) ==========
  const addItem = () => {
    const title = (newTitle || "").trim();
    if (!title || !columns.length) return;
    const id = genId();
    const toDo = columns.find(c => c.isDefault) || columns[0];
    setItems(prev => [...prev, { id, title, colId: toDo.id }]);
    setNewTitle("");
    inputRef.current?.focus();
  };

  const deleteItem = (id) => setItems(prev => prev.filter(i => i.id !== id));

  const onDragStart = (e, itemId) => {
    dragItemId.current = itemId;
    e.dataTransfer.effectAllowed = "move";
  };
  const onDragOverCol = (e) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = "move";
  };
  const onDropToCol = (e, colId) => {
    e.preventDefault();
    const itemId = dragItemId.current;
    if (!itemId) return;
    setItems(prev => prev.map(it => it.id === itemId ? { ...it, colId } : it));
    dragItemId.current = null;
  };

  // ========== Rename column (PUT /name) ==========
  const renameColumn = async (columnId, newName) => {
    if (!project?.id) return;
    const res = await fetch(
      `${API_BASE}/api/projects/${project.id}/board/columns/${columnId}/name`,
      {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${getToken()}`
        },
        body: JSON.stringify({ name: newName })
      }
    );
    if (!res.ok) throw new Error(await res.text().catch(()=> "Failed to rename"));
    const dto = await res.json();
    setColumns(prev =>
      prev
        .map(c => c.id === columnId
          ? { ...c, name: dto.name, orderKey: dto.orderKey, isDefault: !!dto.isDefault, isDoneLike: !!dto.isDoneLike }
          : c)
        .sort((a,b)=>a.orderKey-b.orderKey)
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
    if (!name || !editingColId) { cancelRename(); return; }
    try {
      await renameColumn(editingColId, name);
    } catch (e) {
      alert(e.message || "Rename failed");
    } finally {
      cancelRename();
    }
  };

  // ========== Delete column (DELETE) ==========
  const deleteColumn = async (columnId) => {
    if (!project?.id) return;
    const res = await fetch(
      `${API_BASE}/api/projects/${project.id}/board/columns/${columnId}`,
      {
        method: "DELETE",
        headers: { Authorization: `Bearer ${getToken()}` }
      }
    );
    if (!res.ok) throw new Error(await res.text().catch(()=> "Failed to delete"));
    const data = await res.json(); // { columns: [...] }
    const list = data.columns ?? [];
    const cols = list
      .slice()
      .sort((a,b)=>a.orderKey-b.orderKey)
      .map(c => ({
        id: c.id,
        name: c.name,
        orderKey: c.orderKey,
        locked: !!(c.isDefault || c.isDoneLike || ["to do","doing","done"].includes((c.name||"").toLowerCase())),
        isDefault: !!c.isDefault,
        isDoneLike: !!c.isDoneLike
      }));
    setColumns(cols);

    // local item'ları fallback kolona taşı
    if (cols.length) {
      const fallback = cols.find(c => c.isDefault) || cols[0];
      setItems(prev => prev.map(i => {
        if (!cols.some(c => c.id === i.colId)) return { ...i, colId: fallback.id };
        return i;
      }));
    } else {
      // kolon kalmadıysa item'ları temizle
      setItems([]);
    }
  };

  // ========== Move column (PUT /move) ==========
  const moveColumn = async (columnId, targetIndex) => {
    if (!project?.id) return;
    try {
      const res = await fetch(
        `${API_BASE}/api/projects/${project.id}/board/columns/${columnId}/move`,
        {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${getToken()}`
          },
          body: JSON.stringify({ targetIndex })
        }
      );
      if (!res.ok) {
        const txt = await res.text().catch(() => "");
        throw new Error(txt || "Failed to move column");
      }
      const data = await res.json(); // { columns: [...] }  veya  [...]
      const list = Array.isArray(data) ? data : (data.columns ?? []);
      const cols = list
        .slice()
        .sort((a,b)=>a.orderKey-b.orderKey)
        .map(c => ({
          id: c.id,
          name: c.name,
          orderKey: c.orderKey,
          locked: !!(c.isDefault || c.isDoneLike || ["to do","doing","done"].includes((c.name||"").toLowerCase())),
          isDefault: !!c.isDefault,
          isDoneLike: !!c.isDoneLike
        }));
      setColumns(cols);
    } catch (e) {
      console.error(e);
      alert(e.message || "Failed to move column");
    }
  };

  // ========== Derived ==========
  const grouped = useMemo(() => {
    const map = new Map(columns.map(c => [c.id, []]));
    items.forEach(it => {
      if (!map.has(it.colId)) map.set(it.colId, []);
      map.get(it.colId).push(it);
    });
    return map;
  }, [columns, items]);

  // Modal’dan başarılı dönüşte listeye ekle
  const handleColumnAdded = (dto) => {
    const newCol = {
      id: dto.id,
      name: dto.name,
      orderKey: dto.orderKey,
      locked: !!(dto.isDefault || dto.isDoneLike),
      isDefault: !!dto.isDefault,
      isDoneLike: !!dto.isDoneLike,
    };
    setColumns(prev => [...prev, newCol].sort((a, b) => a.orderKey - b.orderKey));
  };

  return (
    <div className="dash-wrap">
      {/* Top bar */}
      <div className="dash-top">
        <div className="dash-new">
          <button className="btn btn-primary" onClick={addItem} title="Add work item" disabled={!columns.length}>+ New item</button>
          <input
            ref={inputRef}
            className="input"
            placeholder="Work item title…"
            value={newTitle}
            onChange={(e)=>setNewTitle(e.target.value)}
            onKeyDown={(e)=>{ if (e.key === "Enter") addItem(); }}
          />
        </div>

        <div className="dash-tools">
          <button className="btn" onClick={() => setAddOpen(true)} title="Add column">+ Add column</button>
          <button className="btn" onClick={loadBoard} title="Refresh columns">↻ Refresh</button>
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
              onDrop={(e)=>onDropToCol(e, col.id)}
            >
              <div className="col-head">
                {editingColId === col.id ? (
                  <div className="col-edit">
                    <input
                      className="input col-rename"
                      value={editingName}
                      onChange={e=>setEditingName(e.target.value)}
                      onKeyDown={e => {
                        if (e.key === "Enter") saveRename();
                        if (e.key === "Escape") cancelRename();
                      }}
                      autoFocus
                    />
                    <button className="icon-btn success" title="Save name" onClick={saveRename}>✓</button>
                    <button className="icon-btn" title="Cancel" onClick={cancelRename}>✕</button>
                  </div>
                ) : (
                  <>
                    <div
                      className="col-title"
                      title="Click to rename"
                      onClick={()=>startRename(col)}
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
                  >←</button>

                  <button
                    className="icon-btn"
                    title="Move right"
                    disabled={idx === columns.length - 1}
                    onClick={() => moveColumn(col.id, idx + 1)}
                  >→</button>

                  {/* Silme: tüm kolonlarda 'x' ve confirm */}
                  <button
                    className="icon-btn danger"
                    title="Delete column"
                    onClick={async () => {
                      if (!window.confirm(`Delete column "${col.name}"?`)) return;
                      try { await deleteColumn(col.id); }
                      catch (e) { alert(e.message || "Delete failed"); }
                    }}
                  >x</button>
                </div>
              </div>

              <div className="col-body">
                {(grouped.get(col.id) || []).map(it => (
                  <div
                    key={it.id}
                    className="card"
                    draggable
                    onDragStart={(e)=>onDragStart(e, it.id)}
                    title="Drag to move"
                  >
                    <div className="card-title">{it.title}</div>
                    <button className="icon-btn" title="Delete" onClick={()=>deleteItem(it.id)}>✕</button>
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
    </div>
  );
}

// ------- helpers -------
function genId() {
  return Math.random().toString(36).slice(2, 8) + Date.now().toString(36).slice(-4);
}
