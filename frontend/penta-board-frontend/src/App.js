// src/App.js
import { Routes, Route, Navigate, useNavigate, useParams } from 'react-router-dom';
import MainPage from './MainPage';
import Login from './Login';
import NotFound from './NotFound';
import { getToken, API_BASE } from './utils/auth';
import { useEffect } from 'react';
import AcceptInvitePage from './AcceptInvitePage';

// Korumalı route
function RequireAuth({ children }) {
  const token = getToken();
  if (!token) return <NotFound />; // token yoksa 404 gösteriyorsun; istersen PentaLogin'e yönlendirebilirim
  return children;
}

// /PentaBoard -> /PentaBoard/:slug yönlendiren bileşen
function RedirectToUser() {
  const navigate = useNavigate();

  useEffect(() => {
    const token = getToken();
    if (!token) {
      navigate('/PentaLogin', { replace: true });
      return;
    }
    (async () => {
      try {
        const res = await fetch(`${API_BASE}/api/users/me`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        if (!res.ok) throw new Error('unauthorized');
        const me = await res.json();
        navigate(`/PentaBoard/${me.handle}`, { replace: true, state: { me } });
      } catch {
        navigate('/PentaLogin', { replace: true });
      }
    })();
  }, [navigate]);

  return null;
}

// slug ile (ör. /PentaBoard/admin, /PentaBoard/admin/users, /projects/...)
function MainPageWithSlug() {
  const { slug } = useParams();
  return (
    <RequireAuth>
      <MainPage slug={slug} />
    </RequireAuth>
  );
}

export default function App() {
  return (
    <Routes>
      {/* root -> login */}
      <Route path="/" element={<Navigate to="/PentaLogin" replace />} />

      {/* /PentaBoard -> me'ye göre slug'a yönlendir */}
      <Route path="/PentaBoard" element={<RedirectToUser />} />

      {/* Korumalı slug'lı rotalar */}
      <Route path="/PentaBoard/:slug" element={<MainPageWithSlug />} />
      <Route path="/PentaBoard/:slug/users" element={<MainPageWithSlug />} />

      {/* 🔹 Proje rotaları — sadece body değişecek ekranlar */}
      <Route path="/PentaBoard/:slug/projects/:projectKey" element={<MainPageWithSlug />} />
      <Route path="/PentaBoard/:slug/projects/:projectKey/:sub" element={<MainPageWithSlug />} />

      {/* Login / Davet */}
      <Route path="/PentaLogin" element={<Login />} />
      <Route path="/accept-invite" element={<AcceptInvitePage />} />

      {/* Diğer her şey login'e */}
      <Route path="*" element={<Navigate to="/PentaLogin" replace />} />
    </Routes>
  );
}
