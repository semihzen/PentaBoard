import React, { useEffect, useMemo, useState } from 'react';
import './SummaryBody.css';
import { API_BASE, getToken } from '../utils/auth';
import ChangeDescription from './ChangeDescription';
import InviteMemberModal from './InviteMemberModal';

export default function SummaryBody({ project, me: meProp, onInvite }) {
  const [p, setP] = useState(project);
  useEffect(() => { setP(project); }, [project]);

  // me
  const [me, setMe] = useState(meProp || null);
  useEffect(() => { setMe(meProp || null); }, [meProp]);
  useEffect(() => {
    if (meProp) return;
    (async () => {
      try {
        const res = await fetch(`${API_BASE}/api/users/me`, {
          headers: { Authorization: `Bearer ${getToken()}` }
        });
        if (res.ok) setMe(await res.json());
      } catch {}
    })();
  }, [meProp]);

  // modals
  const [descOpen, setDescOpen] = useState(false);
  const [inviteOpen, setInviteOpen] = useState(false);

  // ===== Members state =====
  const [members, setMembers] = useState([]);      // { userId, name, email, subRole, isOwner }
  const [loadingMembers, setLoadingMembers] = useState(false);
  const [removing, setRemoving] = useState(null);

  // helpers
  const formatDate = (d) => {
    if (!d) return null;
    const dt = new Date(d);
    return isNaN(dt) ? null : dt.toLocaleDateString();
  };

  // members load
  const loadMembers = async () => {
    if (!p?.id) return;
    try {
      setLoadingMembers(true);
      const res = await fetch(`${API_BASE}/api/projects/${p.id}/members`, {
        headers: { Authorization: `Bearer ${getToken()}` }
      });
      if (!res.ok) throw new Error(await res.text().catch(() => 'Failed'));
      const data = await res.json();
      const items = (data?.items ?? []).map(m => {
        const full = `${m.firstName ?? ''} ${m.lastName ?? ''}`.trim();
        return {
          userId: m.userId,
          name: full || m.email || 'Member',
          email: m.email ?? '',
          subRole: m.subRole ?? '',
          isOwner: (m.subRole ?? '').toLowerCase() === 'owner',
        };
      });
      const sorted = [
        ...items.filter(x => x.isOwner),
        ...items.filter(x => !x.isOwner).sort((a,b)=>a.name.localeCompare(b.name)),
      ];
      setMembers(sorted);
    } catch (e) {
      console.error(e);
      setMembers([]);
    } finally {
      setLoadingMembers(false);
    }
  };
  useEffect(() => { loadMembers(); }, [p?.id]);

  // izinler: admin || proje sahibi (system admin + owner)
  const meCanManage = useMemo(() => {
    if (!me || !p) return false;
    const role = (me.role || '').toLowerCase();
    const isAdmin = role === 'admin';
    const isSystemAdmin = role === 'system admin';
    const isOwner =
      isSystemAdmin &&
      me.id && p.projectAdminId &&
      String(me.id).toLowerCase() === String(p.projectAdminId).toLowerCase();
    return isAdmin || isOwner;
  }, [me, p]);

  const canInvite = meCanManage;

  // UI helpers
  const hasDesc = !!(p?.description && String(p.description).trim().length);
  const hasMeta = !!p?.startDate || !!p?.endDate || (p?.tags && p.tags.length > 0);

  return (
    <div className="pb-summary-wrap">
      {/* Sol kolon */}
      <div className="pb-col-left">
        {/* About */}
        <section className="pb-card pb-about">
          <div className="pb-card-head">
            <h2 className="pb-h2">About this project</h2>
            {meCanManage && (
              <button
                type="button"
                className="pb-btn pb-btn-primary"
                onClick={() => setDescOpen(true)}
              >
                Change description
              </button>
            )}
          </div>

          <div className="pb-about__text">
            {hasDesc ? (
              <p className="pb-paragraph">{p.description}</p>
            ) : (
              <>
                <p className="pb-paragraph">
                  Help others to get on board!
                  <br />
                  Describe your project and make it easier for other people to understand it.
                </p>
                {meCanManage && (
                  <button
                    type="button"
                    className="pb-btn pb-btn-ghost"
                    onClick={() => setDescOpen(true)}
                  >
                    <span className="pb-btn-plus" aria-hidden>+</span>
                    Add Project Description
                  </button>
                )}
              </>
            )}

            {hasMeta && (
              <div className="pb-meta">
                {formatDate(p?.startDate) && (
                  <div className="pb-meta-item"><span>Start:</span> {formatDate(p.startDate)}</div>
                )}
                {formatDate(p?.endDate) && (
                  <div className="pb-meta-item"><span>End:</span> {formatDate(p.endDate)}</div>
                )}
                {p?.tags?.length > 0 && (
                  <div className="pb-tags">
                    {p.tags.map((t, i) => <span key={i} className="pb-tag">{t}</span>)}
                  </div>
                )}
              </div>
            )}
          </div>

          <div className="pb-about__illu" aria-hidden>
            <div className="pb-figure" />
            <div className="pb-cloud pb-cloud-1" />
            <div className="pb-cloud pb-cloud-2" />
          </div>
        </section>

        {/* Members – GENİŞ KART (About’ın ALTINDA) */}
        <section className="pb-card pb-members-wide">
          <div className="pb-side-head">
            <h3 className="pb-h3">Members</h3>
            {canInvite && (
              <button
                type="button"
                className="pb-btn pb-btn-primary"
                onClick={() => setInviteOpen(true)}
              >
                Add member
              </button>
            )}
          </div>

          {loadingMembers ? (
            <div className="pb-member pb-muted">Loading members…</div>
          ) : members.length === 0 ? (
            <div className="pb-member pb-muted">No members yet</div>
          ) : (
            <div className="pb-members-scroll">
              <div className="pb-members-list">
                {members.map((m) => (
                  <div key={m.userId} className="pb-member-row">
                    <div className="pb-member">
                      <div className={`pb-member-circle ${m.isOwner ? 'owner' : ''}`}>
                        {(m.name?.[0] || m.email?.[0] || 'U').toUpperCase()}
                      </div>
                      <div>
                        <div className="pb-member-name">
                          {m.name}
                          {m.isOwner && <span className="pb-chip">owner</span>}
                        </div>
                        <div className="pb-member-sub">
                          <span className="pb-member-email">{m.email}</span>
                          {!!m.subRole && !m.isOwner && (
                            <span className="pb-chip light">{m.subRole}</span>
                          )}
                        </div>
                      </div>
                    </div>

                    <button
                      className="pb-btn danger sm"
                      disabled={!meCanManage || m.isOwner || removing === m.userId}
                      title={m.isOwner ? 'Owner cannot be removed' : 'Remove from project'}
                      onClick={() => console.log('remove to-be-implemented', m.userId)}
                    >
                      {removing === m.userId ? 'Removing…' : 'Remove'}
                    </button>
                  </div>
                ))}
              </div>
            </div>
          )}
        </section>
      </div>

      {/* Sağ kolon */}
      <aside className="pb-col-right">
        <section className="pb-card pb-side-card">
          <div className="pb-side-head">
            <h3 className="pb-h3">Project stats</h3>
            <select className="pb-select" defaultValue="7">
              <option value="7">Last 7 days</option>
              <option value="14">Last 14 days</option>
              <option value="30">Last 30 days</option>
            </select>
          </div>

          <div className="pb-side-stats">
            <div className="pb-side-stat">
              <div className="pb-side-value">1</div>
              <div className="pb-side-sub">Work items created</div>
            </div>
            <div className="pb-side-stat">
              <div className="pb-side-value">0</div>
              <div className="pb-side-sub">Work items completed</div>
            </div>
          </div>
        </section>
      </aside>

      {/* Modals */}
      {meCanManage && (
        <ChangeDescription
          open={descOpen}
          project={p}
          onClose={() => setDescOpen(false)}
          onSaved={(updated) => setP((prev) => ({ ...(prev || {}), ...updated }))}
        />
      )}

      <InviteMemberModal
        open={inviteOpen}
        project={p}
        onClose={() => setInviteOpen(false)}
        onAdded={() => loadMembers()}
      />
    </div>
  );
}
