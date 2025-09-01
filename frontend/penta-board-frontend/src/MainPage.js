// src/MainPage.js
import { useEffect, useMemo, useState, useRef } from 'react';
import {
  Search, Settings, User as UserIcon, Plus, MoreVertical,
  Home, GitBranch, LogOut, Shield, UserPlus, Folder
} from 'lucide-react';
import { useLocation, useNavigate } from 'react-router-dom';
import { clearToken, getToken, API_BASE } from './utils/auth';
import UsersPage from './AdminUsersPage';
import InviteUserPopUp from './InviteUserPopUp';
import './InviteUserPopUp.css';
import './MainPage.css';
import NewProjectPopUp from './NewProjectPopUp';
import DeleteConfirm from './DeleteConfirm';

// Body’yi projeye göre yöneten layout
import ProjectLayout from './project/ProjectLayout';

// Route key üretimi için utils
import slugify from './utils/slugify';

export default function MainPage({ slug, initialSection }) {
  const navigate = useNavigate();
  const location = useLocation();

  const [section, setSection] = useState(initialSection || 'Overview');
  const [activeTab, setActiveTab] = useState('Projects');

  const [me, setMe] = useState(location.state?.me || null);
  const [loadingMe, setLoadingMe] = useState(!location.state?.me);
  const [error, setError] = useState('');

  const [projects, setProjects] = useState([]);
  const [loadingProjects, setLoadingProjects] = useState(true);

  // --- menu & delete confirm state ---
  const [openMenuId, setOpenMenuId] = useState(null);
  const [deletingId, setDeletingId] = useState(null);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [toDelete, setToDelete] = useState(null);
  const menuRefs = useRef({}); // her kart için menü ref

  // /PentaBoard/:slug/projects/:projectKey(/:sub?)
  const parseProjectRoute = (pathname) => {
    const m = pathname.match(/\/pentaboard\/[^/]+\/projects\/([^/]+)(?:\/([^/]+))?/i);
    return m ? { projectKey: m[1], sub: (m[2] || 'summary').toLowerCase() } : null;
  };

  // dışarı tıklayınca menüyü kapat
  useEffect(() => {
    const closeOnOutside = (e) => {
      if (!openMenuId) return;
      const ref = menuRefs.current[openMenuId];
      if (ref && !ref.contains(e.target)) setOpenMenuId(null);
    };
    document.addEventListener('mousedown', closeOnOutside);
    return () => document.removeEventListener('mousedown', closeOnOutside);
  }, [openMenuId]);

  const initials = useMemo(() => {
    if (!me) return slug?.slice(0, 2)?.toUpperCase() || 'PB';
    const a = (me.firstName?.[0] || '').toUpperCase();
    const b = (me.lastName?.[0] || '').toUpperCase();
    return (a + b) || (me.handle?.slice(0, 2)?.toUpperCase()) || 'PB';
  }, [me, slug]);

  const displayName = useMemo(() => {
    if (!me) return slug || 'user';
    const full = `${me.firstName ?? ''} ${me.lastName ?? ''}`.trim();
    return full || me.handle || 'user';
  }, [me, slug]);

  const subTitle = useMemo(() => {
    if (!me) return '';
    const bits = [me.email, me.role].filter(Boolean);
    return bits.join(' • ');
  }, [me]);

  const roleText = (me?.role || '').trim().toLowerCase();
  const isAdminExact = roleText === 'admin';
  const isSystemAdmin = roleText === 'system admin';
  const canCreateProject = isAdminExact || isSystemAdmin;

  // Genel section (Users / Overview)
  useEffect(() => {
    const path = location.pathname.toLowerCase();
    setSection(path.endsWith('/users') ? 'Users' : 'Overview');
  }, [location.pathname]);

  // === Proje bağlamı (URL’e göre) — SADE: sadece eldeki projelerden eşleştir, by-key çağrısı yok ===
  const [projectCtx, setProjectCtx] = useState(null); // { project, sub }
  useEffect(() => {
    const info = parseProjectRoute(location.pathname);
    if (!info) { setProjectCtx(null); return; }

    const keyLower = info.projectKey.toLowerCase();

    // Liste yüklenmeden deneme yapma
    if (loadingProjects) return;

    // id / key / slug(name) üzerinden eşleştir
    const p = projects.find(pr =>
      (pr.key && pr.key.toLowerCase() === keyLower) ||
      slugify(pr.name) === keyLower ||
      String(pr.id).toLowerCase() === keyLower
    );

    setProjectCtx({ project: p || null, sub: info.sub });
  }, [location.pathname, projects, loadingProjects]);

  // === Sidebar: base grup + (varsa) proje grubu ===
  const sidebarBase = useMemo(() => ([
    {
      key: 'Overview',
      icon: Home,
      label: 'Projects Overview',
      onClick: () => navigate(`/PentaBoard/${slug}`)
    },
    ...(isAdminExact ? [{
      key: 'Users',
      icon: UserIcon,
      label: 'Users',
      onClick: () => navigate(`/PentaBoard/${slug}/users`)
    }] : [])
  ]), [isAdminExact, navigate, slug]);

  const sidebarProject = useMemo(() => {
    if (!projectCtx?.project) return [];
    const pn = projectCtx.project.name || 'Project';
    const pk = projectCtx.project.key || slugify(pn);
    return [
      {
        key: 'ProjectSummary',
        icon: Home,
        label: 'Summary',
        onClick: () => navigate(`/PentaBoard/${slug}/projects/${pk}`) // base → summary
      },
      {
        key: 'ProjectDashboard',
        icon: GitBranch,
        label: 'Dashboard',
        onClick: () => navigate(`/PentaBoard/${slug}/projects/${pk}/dashboard`)
      },
      {
        key: 'ProjectFiles',
        icon: Folder,
        label: 'Files',
        onClick: () => navigate(`/PentaBoard/${slug}/projects/${pk}/files`)
      },
    ];
  }, [projectCtx, navigate, slug]);

  // “Users” sayfasına yetkisiz giriş
  useEffect(() => {
    if (!isAdminExact && location.pathname.toLowerCase().endsWith('/users')) {
      navigate(`/PentaBoard/${slug}`, { replace: true });
    }
  }, [isAdminExact, location.pathname, navigate, slug]);

  const handleLogout = () => {
    clearToken();
    navigate('/PentaLogin');
  };

  // me
  useEffect(() => {
    if (me) return;
    const token = getToken();
    if (!token) {
      navigate('/PentaLogin', { replace: true });
      return;
    }
    (async () => {
      try {
        setLoadingMe(true);
        const res = await fetch(`${API_BASE}/api/users/me`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        if (!res.ok) throw new Error('Unauthorized');
        const data = await res.json();
        setMe(data);

        if (slug && slug !== data.handle) {
          const toUsers = location.pathname.toLowerCase().endsWith('/users');
          const newPath = toUsers ? `/PentaBoard/${data.handle}/users` : `/PentaBoard/${data.handle}`;
          navigate(newPath, { replace: true, state: { me: data } });
        }
      } catch {
        setError('Oturum süren dolmuş olabilir. Lütfen tekrar giriş yap.');
        clearToken();
        navigate('/PentaLogin', { replace: true });
      } finally {
        setLoadingMe(false);
      }
    })();
  }, [navigate, slug, me, location.pathname]);

  // projects
  useEffect(() => {
    const token = getToken();
    if (!token) return;
    (async () => {
      try {
        setLoadingProjectsTrue();
      } catch {}
    })();
  }, []);
  const setLoadingProjectsTrue = async () => {
    try {
      setLoadingProjects(true);
      const res = await fetch(`${API_BASE}/api/projects`, {
        headers: { Authorization: `Bearer ${getToken()}` }
      });
      if (!res.ok) throw new Error('Failed to load projects');
      const data = await res.json();
      const mapped = data.map(p => ({
        id: p.id,
        name: p.name,
        description: p.description,
        startDate: p.startDate,
        endDate: p.endDate,
        tags: p.tags ?? [],
        key: p.key,
        color: p.color || 'teal',
        initial: (p.name?.[0] || 'P').toUpperCase(),
        projectAdminId: p.projectAdminId,
      }));
      setProjects(mapped);
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingProjects(false);
    }
  };

  const [inviteOpen, setInviteOpen] = useState(false);
  const [projectOpen, setProjectOpen] = useState(false);

  // izin: bu projeyi silebilir miyim?
  const canDeleteProject = (p) => {
    if (isAdminExact) return true;
    if (isSystemAdmin && me?.id && p.projectAdminId &&
        p.projectAdminId.toLowerCase?.() === me.id.toLowerCase?.()) {
      return true;
    }
    return false;
  };

  // menüden delete'e basınca onay modalını aç
  const openDeleteConfirm = (p) => {
    setToDelete(p);
    setConfirmOpen(true);
    setOpenMenuId(null);
  };

  // sil
  const doDelete = async () => {
    if (!toDelete) return;
    try {
      setDeletingId(toDelete.id);
      const res = await fetch(`${API_BASE}/api/projects/${toDelete.id}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${getToken()}` }
      });
      if (!res.ok) throw new Error(await res.text());
      setProjects(prev => prev.filter(x => x.id !== toDelete.id));
    } catch (e) {
      console.error(e);
      alert('Silme başarısız: ' + (e?.message || 'Unknown error'));
    } finally {
      setDeletingId(null);
      setConfirmOpen(false);
      setToDelete(null);
    }
  };

  const inProject = !!projectCtx?.project;

  return (
    <div className="main-container">
      <header className="header">
        <div className="header-left">
          {/* PentaBoard başlığı: link gibi, altı çizgisiz */}
          <a
            href="/PentaBoard"
            onClick={(e) => { e.preventDefault(); navigate('/PentaBoard'); }}
            className="title linklike"
            title="Go to PentaBoard home"
          >
            PentaBoard
          </a>
          {me?.role && (
            <span className="role-chip">
              <Shield size={14} /> {me.role}
            </span>
          )}
        </div>

        <div className="search-box">
          <Search className="search-icon" />
          <input type="text" placeholder="Search" />
        </div>

        <div className="header-right">
          <button className="icon-btn" title="Settings"><Settings /></button>
          <button className="icon-btn" title="Profile"><UserIcon /></button>
          <div className="avatar" title={displayName}>{initials}</div>
          <button className="icon-btn" title="Logout" onClick={handleLogout}><LogOut /></button>
        </div>
      </header>

      <div className="content">
        <aside className="sidebar">
          <nav className="nav-list">
            {/* 1) Base grup: Projects Overview + (varsa) Users */}
            {sidebarBase.map((item) => {
              const Icon = item.icon;
              const isActive =
                (!inProject && item.key === 'Overview' && section === 'Overview') ||
                (item.key === 'Users' && section === 'Users');
              return (
                <button
                  key={item.key}
                  className={`sidebar-item ${isActive ? 'active' : ''}`}
                  onClick={item.onClick}
                >
                  <Icon className="sidebar-icon" />
                  <span>{item.label}</span>
                </button>
              );
            })}

            {/* 2) Çizgi: sadece proje görünümündeyken */}
            {inProject && (
              <div
                className="nav-divider"
                style={{ borderTop: '1px solid #e5e7eb', margin: '8px 0' }}
              />
            )}

            {/* 3) Proje alt menüsü: Summary → Dashboard → Files */}
            {sidebarProject.map((item) => {
              const Icon = item.icon;
              const isActive =
                inProject &&
                (
                  (item.key === 'ProjectSummary' && projectCtx.sub === 'summary') ||
                  (item.key === 'ProjectDashboard' && projectCtx.sub === 'dashboard') ||
                  (item.key === 'ProjectFiles' && projectCtx.sub === 'files')
                );
              return (
                <button
                  key={item.key}
                  className={`sidebar-item ${isActive ? 'active' : ''}`}
                  onClick={item.onClick}
                >
                  <Icon className="sidebar-icon" />
                  <span>{item.label}</span>
                </button>
              );
            })}
          </nav>
        </aside>

        <main className="body">
          {/* Üst başlık */}
          <div className="page-head">
            <div className="page-title">
              {inProject ? (
                <>
                  <h1 className="h1-trim">{projectCtx.project?.name || 'Project'}</h1>
                  <div className="h1-sub">
                    Project • {projectCtx.sub.charAt(0).toUpperCase()+projectCtx.sub.slice(1)}
                  </div>
                </>
              ) : (
                <>
                  <h1 className="h1-trim">
                    {loadingMe ? 'Loading…' : (me ? displayName : (slug || 'user'))}
                  </h1>
                  {!!subTitle && <div className="h1-sub">{subTitle}</div>}
                </>
              )}
            </div>

            <div>
              {!inProject && section === 'Users' && isAdminExact && (
                <button className="new-project-btn" onClick={() => setInviteOpen(true)}>
                  <UserPlus /> User Invite
                </button>
              )}
              {!inProject && section !== 'Users' && canCreateProject && (
                <button className="new-project-btn" onClick={() => setProjectOpen(true)}>
                  <Plus /> New project
                </button>
              )}
            </div>
          </div>

          {/* BODY */}
          {inProject ? (
            <ProjectLayout
              slug={slug}
              me={me}
              project={projectCtx.project}
              sub={projectCtx.sub}
              navigate={navigate}
            />
          ) : section === 'Users' && isAdminExact ? (
            <UsersPage />
          ) : (
            <>
              <div className="tabs">
                {['Projects', 'My work items', 'My pull requests'].map((tab) => (
                  <button
                    key={tab}
                    className={`tab ${activeTab === tab ? 'active' : ''}`}
                    onClick={() => setActiveTab(tab)}
                  >
                    {tab}
                  </button>
                ))}
              </div>

              {activeTab === 'Projects' ? (
                <div className="projects-grid">
                  {loadingProjects ? (
                    <p>Loading projects...</p>
                  ) : projects.length > 0 ? (
                    projects.map((p) => (
                      <div
                        key={p.id}
                        className="project-card"
                        onClick={() => {
                          const key = p.key || slugify(p.name) || p.id;
                          navigate(`/PentaBoard/${slug}/projects/${key}`); // base path → Summary default
                        }}
                        title="Open project"
                      >
                        <div className={`project-avatar ${p.color}`}>{p.initial}</div>
                        <h3 className="project-name">{p.name}</h3>
                        {!isAdminExact && !isSystemAdmin && (
                          <div className="project-chip">Project member</div>
                        )}

                        {(isAdminExact || isSystemAdmin) && (
                          <div
                            className="menu-wrap"
                            ref={(el) => (menuRefs.current[p.id] = el)}
                            onClick={(e) => e.stopPropagation()}
                          >
                            <button
                              className="more-btn"
                              onClick={() =>
                                setOpenMenuId((i) => (i === p.id ? null : p.id))
                              }
                              aria-haspopup="menu"
                              aria-expanded={openMenuId === p.id}
                              title="Actions"
                            >
                              <MoreVertical />
                            </button>

                            {openMenuId === p.id && (
                              <div className="card-menu" role="menu">
                                {canDeleteProject(p) ? (
                                  <button
                                    className="menu-item danger"
                                    onClick={() => openDeleteConfirm(p)}
                                    disabled={deletingId === p.id}
                                  >
                                    {deletingId === p.id ? 'Deleting…' : 'Delete project'}
                                  </button>
                                ) : (
                                  <div
                                    className="menu-item disabled"
                                    title="You don't have permission"
                                  >
                                    Delete project
                                  </div>
                                )}
                              </div>
                            )}
                          </div>
                        )}
                      </div>
                    ))
                  ) : (
                    <p>No projects yet.</p>
                  )}
                </div>
              ) : (
                <div className="empty-state">
                  <div className="empty-icon"><GitBranch /></div>
                  <h3>No {activeTab.toLowerCase()} found</h3>
                  <p>You don't have any {activeTab.toLowerCase()} yet.</p>
                </div>
              )}
            </>
          )}

          {section === 'Users' && isAdminExact && (
            <InviteUserPopUp
              open={inviteOpen}
              onClose={() => setInviteOpen(false)}
              onInvited={() => window.dispatchEvent(new CustomEvent('users:refresh'))}
            />
          )}

          {canCreateProject && (
            <NewProjectPopUp
              open={projectOpen}
              onClose={() => setProjectOpen(false)}
              onCreated={(proj) => setProjects((prev) => [...prev, proj])}
            />
          )}
        </main>
      </div>

      <footer className="footer">
        <div className="footer-left">
          <span>© 2025 PentaBoard</span> • <button>Privacy & Cookies</button> •{' '}
          <button>Terms of Use</button>
        </div>
        <div className="footer-right">Version 2025.1</div>
      </footer>

      {/* Delete confirm modal */}
      <DeleteConfirm
        open={confirmOpen}
        title="Delete project"
        message={`"${toDelete?.name ?? 'This project'}" will be permanently deleted.`}
        loading={!!toDelete && deletingId === toDelete.id}
        onCancel={() => {
          setConfirmOpen(false);
          setToDelete(null);
        }}
        onConfirm={doDelete}
      />
    </div>
  );
}
