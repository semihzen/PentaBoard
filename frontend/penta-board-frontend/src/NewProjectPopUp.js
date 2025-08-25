import { useEffect, useRef, useState } from 'react';
import { X, Info, Calendar, Tag } from 'lucide-react';
import slugify from './utils/slugify';
import { getToken, API_BASE } from './utils/auth';
import './NewProjectPopUp.css';

export default function NewProjectPopUp({ open, onClose, onCreated }) {
  const firstRef = useRef(null);

  // form
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [color, setColor] = useState('teal');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [tags, setTags] = useState('');
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);

  // role / me
  const [me, setMe] = useState(null);
  const roleText = (me?.role || '').trim().toLowerCase();
  const isAdmin = roleText === 'admin';
  const isSystemAdmin = roleText === 'system admin';

  // system admin picker (only for Admins)
  const [saQuery, setSaQuery] = useState('');
  const [saResults, setSaResults] = useState([]);
  const [saLoading, setSaLoading] = useState(false);
  const [selectedSA, setSelectedSA] = useState(null); // { id, email, firstName, lastName }
  const blurTimer = useRef(null);

  // modal aç/kapa reset
  useEffect(() => {
    if (open) {
      setTimeout(() => firstRef.current?.focus(), 0);
    } else {
      setName(''); setDescription('');
      setColor('teal'); setStartDate(''); setEndDate('');
      setTags(''); setErrors({}); setLoading(false);
      setSaQuery(''); setSaResults([]); setSelectedSA(null); setSaLoading(false);
    }
  }, [open]);

  // rol/me çek
  useEffect(() => {
    if (!open) return;
    const token = getToken();
    if (!token) return;
    (async () => {
      try {
        const res = await fetch(`${API_BASE}/api/users/me`, {
          headers: { Authorization: `Bearer ${token}` }
        });
        if (res.ok) {
          const data = await res.json();
          setMe(data);
        }
      } catch { /* noop */ }
    })();
  }, [open]);

  // System Admin arama (Admin ise)
  useEffect(() => {
    if (!open || !isAdmin) return;
    const token = getToken();
    if (!token) return;

    const t = setTimeout(async () => {
      const q = saQuery.trim();
      try {
        setSaLoading(true);

        // 1) Rol parametreli endpoint varsa deneyelim
        let url = `${API_BASE}/api/users?role=${encodeURIComponent('System Admin')}`;
        if (q) url += `&query=${encodeURIComponent(q)}`;
        let res = await fetch(url, { headers: { Authorization: `Bearer ${token}` } });

        // 2) Yoksa /api/users fallback -> client-side filter
        if (!res.ok) {
          res = await fetch(`${API_BASE}/api/users`, {
            headers: { Authorization: `Bearer ${token}` }
          });
        }

        if (!res.ok) throw new Error('users fetch failed');
        const list = await res.json();

        const normalized = (Array.isArray(list) ? list : (list.items || list.data || []))
          .filter(u =>
            (u.role || '').toLowerCase() === 'system admin' &&
            (!q || (u.email || '').toLowerCase().includes(q.toLowerCase()) ||
              (`${u.firstName || ''} ${u.lastName || ''}`.toLowerCase().includes(q.toLowerCase())))
          )
          .slice(0, 10);

        setSaResults(normalized);
      } catch {
        setSaResults([]);
      } finally {
        setSaLoading(false);
      }
    }, 250);

    return () => clearTimeout(t);
  }, [open, isAdmin, saQuery]);

  const validate = () => {
    const e = {};
    if (!name.trim()) e.name = 'Project name is required';
    if (startDate && endDate && new Date(endDate) < new Date(startDate)) {
      e.endDate = 'End date cannot be before start date';
    }
    if (isAdmin && !selectedSA) {
      e.projectAdminId = 'Please select a System Admin for this project';
    }
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;

    const body = {
      name: name.trim(),
      key: slugify(name.trim()),
      description: description.trim() || null,
      color,
      startDate: startDate || null,
      endDate: endDate || null,
      tags: tags.split(',').map(t => t.trim()).filter(Boolean)
    };

    if (isAdmin && selectedSA?.id) {
      body.projectAdminId = selectedSA.id;
    }

    try {
      setLoading(true);
      const res = await fetch(`${API_BASE}/api/projects`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${getToken()}`
        },
        body: JSON.stringify(body)
      });

      if (!res.ok) {
        const errText = await res.text();
        throw new Error(errText || 'Project create failed');
      }

      const data = await res.json();
      const project = {
        id: data.id,
        name: data.name,
        key: data.key,
        description: body.description || undefined,
        color: body.color,
        startDate: body.startDate,
        endDate: body.endDate,
        tags: body.tags,
        initial: (body.name[0] || 'P').toUpperCase(),
        createdAt: new Date().toISOString()
      };

      onCreated?.(project);
      onClose?.();
    } catch (err) {
      console.error(err);
      setErrors({ api: err.message });
    } finally {
      setLoading(false);
    }
  };

  if (!open) return null;

  return (
    <div className="np-backdrop" onMouseDown={onClose}>
      <div className="np-modal" role="dialog" aria-modal="true" onMouseDown={(e)=>e.stopPropagation()}>
        <div className="np-header">
          <h2>Create new project</h2>
          <button className="np-icon" onClick={onClose} aria-label="Close"><X /></button>
        </div>

        <form className="np-form" onSubmit={handleSubmit}>
          {/* Admin ise System Admin ata */}
          {isAdmin && (
            <label className="np-field np-rel">
              <span>Assign to System Admin</span>
              <div className="np-input">
                <input
                  type="search"
                  placeholder="Search by email or name"
                  value={saQuery}
                  onChange={e => { setSelectedSA(null); setSaQuery(e.target.value); }}
                  onFocus={() => { /* açık kalsın */ }}
                  onBlur={() => { blurTimer.current = setTimeout(() => setSaQuery(q => q), 120); }}
                  disabled={loading}
                />
              </div>

              {(saQuery && !selectedSA) && (
                <div
                  className="np-suggest"
                  onMouseDown={(e)=> e.preventDefault()} /* input blur'da kapanmasın */
                >
                  {saLoading && <div className="np-suggest-item muted">Searching…</div>}
                  {!saLoading && saResults.length === 0 && (
                    <div className="np-suggest-item muted">No matches</div>
                  )}
                  {!saLoading && saResults.map(u => (
                    <button
                      key={u.id}
                      type="button"
                      className="np-suggest-item"
                      onClick={() => { clearTimeout(blurTimer.current); setSelectedSA(u); setSaQuery(''); }}
                    >
                      <div className="np-suggest-main">
                        <strong>{u.email}</strong>
                        <span>{[u.firstName, u.lastName].filter(Boolean).join(' ')}</span>
                      </div>
                      <span className="np-chip">System Admin</span>
                    </button>
                  ))}
                </div>
              )}

              {selectedSA && (
                <div className="np-selected">
                  Assigned to: <strong>{selectedSA.email}</strong>
                  <button type="button" className="np-clear" onClick={() => setSelectedSA(null)}>change</button>
                </div>
              )}
              {errors.projectAdminId && <div className="np-err">{errors.projectAdminId}</div>}
            </label>
          )}

          <label className="np-field">
            <span>Project name</span>
            <div className="np-input">
              <Info className="np-leading" />
              <input
                ref={firstRef}
                type="text"
                placeholder="e.g. Payments Platform"
                value={name}
                onChange={e=>setName(e.target.value)}
                disabled={loading}
              />
            </div>
            {errors.name && <div className="np-err">{errors.name}</div>}
          </label>

          <label className="np-field">
            <span>Description (optional)</span>
            <textarea
              rows={3}
              placeholder="What is this project about?"
              value={description}
              onChange={e=>setDescription(e.target.value)}
              disabled={loading}
            />
          </label>

          <div className="np-grid">
            <label className="np-field">
              <span>Color</span>
              <select value={color} onChange={e=>setColor(e.target.value)} disabled={loading}>
                <option value="teal">Teal</option>
                <option value="blue">Blue</option>
                <option value="amber">Amber</option>
                <option value="rose">Rose</option>
                <option value="violet">Violet</option>
                <option value="slate">Slate</option>
              </select>
            </label>

            <label className="np-field">
              <span>Start date</span>
              <div className="np-input">
                <Calendar className="np-leading" />
                <input type="date" value={startDate} onChange={e=>setStartDate(e.target.value)} disabled={loading}/>
              </div>
            </label>

            <label className="np-field">
              <span>End date</span>
              <div className="np-input">
                <Calendar className="np-leading" />
                <input type="date" value={endDate} onChange={e=>setEndDate(e.target.value)} disabled={loading}/>
              </div>
              {errors.endDate && <div className="np-err">{errors.endDate}</div>}
            </label>
          </div>

          <label className="np-field">
            <span>Tags (comma separated)</span>
            <div className="np-input">
              <Tag className="np-leading" />
              <input
                type="text"
                placeholder="backend, api, auth"
                value={tags}
                onChange={e=>setTags(e.target.value)}
                disabled={loading}
              />
            </div>
          </label>

          {errors.api && <div className="np-err">{errors.api}</div>}

          <div className="np-actions">
            <button type="button" className="np-btn ghost" onClick={onClose} disabled={loading}>Cancel</button>
            <button type="submit" className="np-btn primary" disabled={loading || (isAdmin && !selectedSA)}>
              {loading ? 'Creating...' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
