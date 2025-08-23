import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { API_BASE } from './utils/auth';
import './AcceptInvitePage.css'

export default function AcceptInvitePage() {
  const navigate = useNavigate();
  const [sp] = useSearchParams();
  const token = sp.get('token') || '';

  const [loading, setLoading] = useState(true);
  const [invite, setInvite] = useState(null); // { email, role, expiresAt?, note? }
  const [err, setErr] = useState('');

  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [password, setPassword] = useState('');
  const [password2, setPassword2] = useState('');

  const [submitting, setSubmitting] = useState(false);
  const [ok, setOk] = useState(false);

  useEffect(() => {
    if (!token) {
      setErr('Missing token');
      setLoading(false);
      return;
    }

    (async () => {
      try {
        setLoading(true);
        setErr('');

        // ⬇️ token doğrulama
        const res = await fetch(
          `${API_BASE}/api/users/invite/verify?token=${encodeURIComponent(token)}`
        );
        if (!res.ok) {
          const txt = await res.text();
          throw new Error(txt || `HTTP ${res.status}`);
        }

        const data = await res.json(); // beklenen: { email, role, note, expiresAt, inviterEmail? ... }
        setInvite(data);
      } catch (e) {
        setErr(e.message || 'Invite verification failed');
      } finally {
        setLoading(false);
      }
    })();
  }, [token]);

  const onSubmit = async (e) => {
    e.preventDefault();

    if (!token) return setErr('Missing token');
    if (!firstName || !lastName) return setErr('Please enter your name.');
    if (password.length < 6) return setErr('Password must be at least 6 characters.');
    if (password !== password2) return setErr('Passwords do not match.');

    try {
      setSubmitting(true);
      setErr('');

      // ⬇️ daveti kabul et ve kullanıcıyı oluştur
      const res = await fetch(`${API_BASE}/api/users/invite/accept`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ token, firstName, lastName, password }),
      });
      if (!res.ok) {
        const txt = await res.text();
        throw new Error(txt || `HTTP ${res.status}`);
      }

      setOk(true);
      // kısa bilgi sonrası login sayfasına
      setTimeout(() => navigate('/PentaLogin'), 1500);
    } catch (e) {
      setErr(e.message || 'Failed to accept invite');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <div style={{ padding: 24 }}>Validating invite…</div>;

  if (err)
    return (
      <div style={{ maxWidth: 480, margin: '40px auto', padding: 16 }}>
        <h2>Invite</h2>
        <div
          style={{
            background: '#fee2e2',
            border: '1px solid #fecaca',
            padding: 12,
            borderRadius: 8,
            color: '#991b1b',
          }}
        >
          {err}
        </div>
      </div>
    );

  if (ok)
    return (
      <div style={{ maxWidth: 480, margin: '40px auto', padding: 16 }}>
        <h2>Invite accepted</h2>
        <p>You can now sign in with your email and the password you set.</p>
      </div>
    );

  return (
    <div style={{ maxWidth: 520, margin: '40px auto', padding: 16 }}>
      <h2>Join PentaBoard</h2>
      <p style={{ color: '#6b7280' }}>
        You are invited with <b>{invite?.email}</b>
        {invite?.role ? ` as ${invite.role}` : ''}.
      </p>

      <form onSubmit={onSubmit} style={{ display: 'grid', gap: 12, marginTop: 12 }}>
        <div>
          <label>First name</label>
          <input
            value={firstName}
            onChange={(e) => setFirstName(e.target.value)}
            required
            className="input"
          />
        </div>

        <div>
          <label>Last name</label>
          <input
            value={lastName}
            onChange={(e) => setLastName(e.target.value)}
            required
            className="input"
          />
        </div>

        <div>
          <label>Password</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            className="input"
          />
        </div>

        <div>
          <label>Confirm password</label>
          <input
            type="password"
            value={password2}
            onChange={(e) => setPassword2(e.target.value)}
            required
            className="input"
          />
        </div>

        {err && (
          <div
            style={{
              background: '#fee2e2',
              border: '1px solid #fecaca',
              padding: 10,
              borderRadius: 8,
              color: '#991b1b',
            }}
          >
            {err}
          </div>
        )}

        <button className="btn" disabled={submitting}>
          {submitting ? 'Creating…' : 'Create account'}
        </button>
      </form>
    </div>
  );
}
