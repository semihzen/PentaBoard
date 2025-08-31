import { useEffect, useMemo, useRef, useState } from "react";
import { API_BASE, getToken } from "../utils/auth";
import "./FilesBody.css";

export default function FilesBody({ project }) {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");
  const [uploading, setUploading] = useState(false);
  const [q, setQ] = useState("");
  const [sort, setSort] = useState({ key: "fileName", dir: "asc" });
  const fileInputRef = useRef(null);

  // preview state
  const [preview, setPreview] = useState({
    open: false,
    src: "",        // blob URL
    name: "",
    id: null,       // for closing when the same file is deleted
    loading: false,
  });

  // blob URL cleanup ref
  const blobUrlRef = useRef("");

  const authHeaders = () => ({ Authorization: `Bearer ${getToken()}` });

  useEffect(() => {
    if (!project?.id) return;
    (async () => {
      try {
        setLoading(true);
        setErr("");
        const res = await fetch(`${API_BASE}/api/projects/${project.id}/files`, { headers: authHeaders() });
        if (!res.ok) throw new Error("Failed to fetch files.");
        const data = await res.json();
        setRows(Array.isArray(data) ? data : []);
      } catch (e) {
        setErr(e.message || "Something went wrong.");
      } finally {
        setLoading(false);
      }
    })();

    // ESC closes preview
    const onEsc = (ev) => { if (ev.key === "Escape") closePreview(); };
    window.addEventListener("keydown", onEsc);
    return () => window.removeEventListener("keydown", onEsc);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [project?.id]);

  // cleanup helper
  function revokeBlobUrl() {
    if (blobUrlRef.current) {
      URL.revokeObjectURL(blobUrlRef.current);
      blobUrlRef.current = "";
    }
  }

  const prettySize = (b) => {
    if (b < 1024) return `${b} B`;
    if (b < 1024 * 1024) return `${(b / 1024).toFixed(1)} KB`;
    return `${(b / 1024 / 1024).toFixed(1)} MB`;
  };

  function toggleSort(key) {
    setSort((s) => (s.key === key ? { key, dir: s.dir === "asc" ? "desc" : "asc" } : { key, dir: "asc" }));
  }

  const filteredSorted = useMemo(() => {
    const term = q.trim().toLowerCase();
    let data = rows.filter((r) => !term || r.fileName.toLowerCase().includes(term));
    data = [...data].sort((a, b) => {
      const { key, dir } = sort;
      let av = a[key], bv = b[key];
      if (key === "createdAt") { av = new Date(av).getTime(); bv = new Date(bv).getTime(); }
      if (key === "sizeBytes") { av = Number(av); bv = Number(bv); }
      if (typeof av === "string") { av = av.toLowerCase(); bv = String(bv).toLowerCase(); }
      if (av < bv) return dir === "asc" ? -1 : 1;
      if (av > bv) return dir === "asc" ? 1 : -1;
      return 0;
    });
    return data;
  }, [rows, q, sort]);

  async function uploadPdf(file) {
    if (!file) return;
    const isPdf = file.type === "application/pdf" || file.name.toLowerCase().endsWith(".pdf");
    if (!isPdf) { setErr("Only PDF files are allowed."); return; }

    const form = new FormData();
    form.append("file", file);

    try {
      setUploading(true);
      setErr("");
      const res = await fetch(`${API_BASE}/api/projects/${project.id}/files`, {
        method: "POST",
        headers: authHeaders(), // content-type auto for FormData
        body: form,
      });
      if (!res.ok) throw new Error((await res.text()) || "Upload failed.");

      // refresh list
      const list = await fetch(`${API_BASE}/api/projects/${project.id}/files`, { headers: authHeaders() }).then(r => r.json());
      setRows(Array.isArray(list) ? list : []);
    } catch (e) {
      setErr(e.message || "Upload failed.");
    } finally {
      setUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  }

  async function remove(id) {
    if (!window.confirm("Delete this PDF?")) return;
    try {
      const res = await fetch(`${API_BASE}/api/projects/${project.id}/files/${id}`, {
        method: "DELETE",
        headers: authHeaders(),
      });
      if (!res.ok) throw new Error("Delete failed.");
      setRows((s) => s.filter((r) => r.id !== id));

      // close preview if the same file
      setPreview((p) => (p.open && p.id === id ? { open: false, src: "", name: "", id: null, loading: false } : p));
      if (preview.open && preview.id === id) revokeBlobUrl();
    } catch (e) {
      setErr(e.message || "Delete failed.");
    }
  }

  // Preview: auth fetch to preview endpoint -> blob -> iframe
  async function openPreview(row) {
    try {
      setErr("");
      revokeBlobUrl();
      setPreview({ open: true, src: "", name: row.fileName, id: row.id, loading: true });

      const url = `${API_BASE}/api/projects/${project.id}/files/${row.id}/preview`;
      const res = await fetch(url, { headers: authHeaders() });

      if (!res.ok) {
        const msg = await res.text().catch(() => "");
        throw new Error(msg || `Preview failed (HTTP ${res.status}).`);
      }

      const blob = await res.blob();
      const blobUrl = URL.createObjectURL(blob);
      blobUrlRef.current = blobUrl;

      setPreview({ open: true, src: blobUrl, name: row.fileName, id: row.id, loading: false });
    } catch (e) {
      setPreview({ open: false, src: "", name: "", id: null, loading: false });
      setErr(e.message || "Preview failed.");
    }
  }

  function closePreview() {
    revokeBlobUrl();
    setPreview({ open: false, src: "", name: "", id: null, loading: false });
  }

  return (
    <div className="files-explorer">
      <div className="fx-toolbar">
        <div className="fx-left">
          <h3 className="fx-title">Files</h3>
          <div className="fx-sub">Project: <strong>{project?.name || "—"}</strong></div>
        </div>
        <div className="fx-right">
          <input
            className="fx-search"
            placeholder="Search by name…"
            value={q}
            onChange={(e) => setQ(e.target.value)}
          />
          <input
            ref={fileInputRef}
            type="file"
            accept="application/pdf,.pdf"
            hidden
            onChange={(e) => uploadPdf(e.target.files?.[0])}
          />
          <button
            className={`fx-btn ${uploading ? "disabled" : ""}`}
            onClick={() => fileInputRef.current?.click()}
          >
            + Add File
          </button>
        </div>
      </div>

      {err && <div className="fx-alert">{err}</div>}

      <div className="fx-table-wrap">
        <table className="fx-table" aria-label="Explorer style file list">
          <thead>
            <tr>
              <th className="fx-col-name" onClick={() => toggleSort("fileName")}>
                Name <SortMark active={sort.key === "fileName"} dir={sort.dir} />
              </th>
              <th className="fx-col-type">Type</th>
              <th className="fx-col-size" onClick={() => toggleSort("sizeBytes")}>
                Size <SortMark active={sort.key === "sizeBytes"} dir={sort.dir} />
              </th>
              <th className="fx-col-date" onClick={() => toggleSort("createdAt")}>
                Modified <SortMark active={sort.key === "createdAt"} dir={sort.dir} />
              </th>
              <th className="fx-col-actions"></th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td className="fx-muted" colSpan={5}>Loading…</td></tr>
            ) : filteredSorted.length === 0 ? (
              <tr><td className="fx-muted" colSpan={5}>No PDFs to show.</td></tr>
            ) : (
              filteredSorted.map((r) => (
                <tr key={r.id}>
                  <td className="fx-namecell">
                    <button className="fx-linklike" title="Preview" onClick={() => openPreview(r)}>
                      <span className="fx-icon" aria-hidden>📄</span>
                      <span className="fx-filename">{r.fileName}</span>
                    </button>
                  </td>
                  <td className="fx-type">PDF Document</td>
                  <td className="fx-size">{prettySize(r.sizeBytes)}</td>
                  <td className="fx-date">{new Date(r.createdAt).toLocaleString()}</td>
                  <td className="fx-actions">
                    <a
                      className="fx-link"
                      href={`${API_BASE}/api/projects/${project.id}/files/${r.id}/download`}
                    >
                      Download
                    </a>
                    <button className="fx-link" onClick={() => openPreview(r)}>Preview</button>
                    <button className="fx-link danger" onClick={() => remove(r.id)}>Delete</button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Centered Preview Modal */}
      {preview.open && (
        <>
          <div className="fx-backdrop" onClick={closePreview} />
          <div className="fx-preview fx-centered">
            <div className="fx-preview-head">
              <div className="fx-preview-title" title={preview.name}>
                {preview.loading ? "Loading…" : preview.name}
              </div>
              <button className="fx-btn" onClick={closePreview}>Close</button>
            </div>
            <div className="fx-preview-body">
              {preview.loading ? (
                <div className="fx-muted" style={{ padding: 12 }}>Preparing preview…</div>
              ) : (
                <iframe
                  src={preview.src}
                  title={preview.name}
                  className="fx-preview-frame"
                  allow="fullscreen"
                />
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}

function SortMark({ active, dir }) {
  return <span className={`fx-sort ${active ? "on" : ""}`}>{active ? (dir === "asc" ? "▲" : "▼") : ""}</span>;
}
