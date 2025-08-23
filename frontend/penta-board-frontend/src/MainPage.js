import { useEffect, useMemo, useState } from 'react';
import {
  Search, Settings, User as UserIcon, Plus, MoreVertical,
  Home, GitBranch, LogOut, Shield, UserPlus,
} from 'lucide-react';
import { useLocation, useNavigate } from 'react-router-dom';
import { clearToken, getToken, API_BASE } from './utils/auth';
import UsersPage from './AdminUsersPage';
import InviteUserPopUp from './InviteUserPopUp';
import './InviteUserPopUp.css';
import './MainPage.css';

export default function MainPage({ slug, initialSection }) {
  const navigate = useNavigate();
  const location = useLocation();

  // sadece body değişir
  const [section, setSection] = useState(initialSection || 'Overview');
  const [activeTab, setActiveTab] = useState('Projects');

  const [me, setMe] = useState(location.state?.me || null);
  const [loadingMe, setLoadingMe] = useState(!location.state?.me);
  const [error, setError] = useState('');

  const projects = [{ id: 1, name: 'Test', color: 'teal', initial: 'T' }];

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

  // Sadece TAM "Admin" rolü (System Admin değil)
  const roleText = (me?.role || '').trim().toLowerCase();
  const isAdminExact = roleText === 'admin';

  // URL -> section senkronizasyonu
  useEffect(() => {
    const path = location.pathname.toLowerCase();
    setSection(path.endsWith('/users') ? 'Users' : 'Overview');
  }, [location.pathname]);

  // Sidebar ögeleri — Users sadece gerçek Admin'e görünür
  const sidebarItems = useMemo(() => {
    const base = [{ key: 'Overview', icon: Home, label: 'Projects Overview' }];
    return isAdminExact ? [...base, { key: 'Users', icon: UserIcon, label: 'Users' }] : base;
  }, [isAdminExact]);

  // Admin değilse Users rotasına girerse overview'a at
  useEffect(() => {
    if (!isAdminExact && location.pathname.toLowerCase().endsWith('/users')) {
      navigate(`/PentaBoard/${slug}`, { replace: true });
    }
  }, [isAdminExact, location.pathname, navigate, slug]);

  const handleLogout = () => {
    clearToken();
    navigate('/PentaLogin');
  };

  // Profil bilgisi (me) çek
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

        // slug yanlışsa düzelt; Users'ta ise Users rota olarak kalsın
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

  // Invite modal
  const [inviteOpen, setInviteOpen] = useState(false);

  // Sidebar tıklamalarında uygun rotaya git
  const goSection = (key) => {
    if (key === 'Users') navigate(`/PentaBoard/${slug}/users`);
    else navigate(`/PentaBoard/${slug}`);
  };

  return (
    <div className="main-container">
      {/* Header */}
      <header className="header">
        <div className="header-left">
          <span className="title">PentaBoard</span>
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

      {/* Content */}
      <div className="content">
        {/* Sidebar */}
        <aside className="sidebar">
          <nav className="nav-list">
            {sidebarItems.map((item) => {
              const Icon = item.icon;
              return (
                <button
                  key={item.key}
                  className={`sidebar-item ${section === item.key ? 'active' : ''}`}
                  onClick={() => goSection(item.key)}
                >
                  <Icon className="sidebar-icon" />
                  <span>{item.label}</span>
                </button>
              );
            })}
          </nav>
        </aside>

        {/* Body */}
        <main className="body">
          <div className="page-head">
            <div className="page-title">
              <h1 className="h1-trim">{loadingMe ? 'Loading…' : (me ? displayName : (slug || 'user'))}</h1>
              {!!subTitle && <div className="h1-sub">{subTitle}</div>}
            </div>

            <div>
              {/* Invite sadece gerçek Admin ve Users sayfasında */}
              {section === 'Users' && isAdminExact && (
                <button className="new-project-btn" onClick={() => setInviteOpen(true)}>
                  <UserPlus /> User Invite
                </button>
              )}
              {section !== 'Users' && (
                <button className="new-project-btn">
                  <Plus /> New project
                </button>
              )}
            </div>
          </div>

          {section === 'Users' ? (
            isAdminExact ? <UsersPage /> : null
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
                  {projects.map((p) => (
                    <div key={p.id} className="project-card">
                      <div className={`project-icon ${p.color}`}>{p.initial}</div>
                      <button className="more-btn"><MoreVertical /></button>
                      <h3>{p.name}</h3>
                    </div>
                  ))}
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

          {/* Invite modal */}
          {section === 'Users' && isAdminExact && (
            <InviteUserPopUp
              open={inviteOpen}
              onClose={() => setInviteOpen(false)}
              onInvited={() => window.dispatchEvent(new CustomEvent('users:refresh'))}
            />
          )}
        </main>
      </div>

      {/* Footer */}
      <footer className="footer">
        <div className="footer-left">
          <span>© 2025 PentaBoard</span> • <button>Privacy & Cookies</button> • <button>Terms of Use</button>
        </div>
        <div className="footer-right">Version 2025.1</div>
      </footer>
    </div>
  );
}
