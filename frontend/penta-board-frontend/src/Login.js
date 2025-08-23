import { useState } from 'react';
import { Eye, EyeOff } from 'lucide-react';
import './Login.css';
import { useNavigate } from 'react-router-dom';

const API_BASE =
  (typeof import.meta !== 'undefined' && import.meta.env?.VITE_API_BASE_URL) ||
  process.env.REACT_APP_API_BASE_URL ||
  'http://localhost:5206';

export default function ModernLoginPageClean() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const res = await fetch(`${API_BASE}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || 'Invalid email or password');
      }

      const { token } = await res.json();

      const storage = rememberMe ? window.localStorage : window.sessionStorage;
      storage.setItem('pb_token', token);

      // ✅ Başarılı giriş → yönlendir
      navigate('/PentaBoard', { replace: true });
    } catch (err) {
      setError(err.message || 'Login failed');
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleLogin = () => {
    // TODO: Google OAuth akışı
    console.log('Google login clicked');
  };

  return (
    <div className="login-container">
      {/* Left Side - Login Form */}
      <div className="login-form-section">
        <div className="login-form-container">
          <div className="logo-container" />

          <div className="login-header">
            <h1 className="login-title">PentaBoard Login</h1>
            <p className="login-subtitle">“Stay organized. Stay ahead.”</p>
          </div>

          <button onClick={handleGoogleLogin} className="google-login-btn">
            {/* Google icon svg burada kalabilir */}
            <span className="google-login-text">Sign in with Google</span>
          </button>

          <div className="divider">
            <div className="divider-line"></div>
            <span className="divider-text">or Sign in with Email</span>
            <div className="divider-line"></div>
          </div>

          <form className="form-container" onSubmit={handleSubmit}>
            <div className="form-group">
              <label htmlFor="email" className="form-label">Email*</label>
              <input
                type="email"
                id="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="mail@website.com"
                className="form-input"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="password" className="form-label">Password*</label>
              <div className="password-container">
                <input
                  type={showPassword ? 'text' : 'password'}
                  id="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Min. 8 character"
                  className="form-input password-input"
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="password-toggle"
                >
                  {showPassword ? <EyeOff className="password-toggle-icon" /> : <Eye className="password-toggle-icon" />}
                </button>
              </div>
            </div>

            <div className="form-options">
              <label className="remember-me">
                <input
                  type="checkbox"
                  checked={rememberMe}
                  onChange={(e) => setRememberMe(e.target.checked)}
                  className="remember-checkbox"
                />
                <span className="remember-label">Remember me</span>
              </label>
              <button type="button" className="forgot-password">Forgot password?</button>
            </div>

            {error && <div className="error-banner">{error}</div>}

            <button type="submit" className="login-btn" disabled={loading}>
              {loading ? 'Logging in...' : 'Login'}
            </button>
          </form>

          <div className="login-footer">
            <p className="copyright">©2025 Semih Zenginoğlu. All rights reserved.</p>
          </div>
        </div>
      </div>

      {/* Right Side - Image */}
      <div className="dashboard-section">
  <div 
  className="dashboard-art" 
  style={{ backgroundImage: 'url("/login.png")' }} 
/>
 {/* img yerine background kullanıyoruz */}
</div>
    </div>
  );
}
