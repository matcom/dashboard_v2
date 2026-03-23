import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [step, setStep] = useState('credentials'); // 'credentials' | 'roleSelection'
  const [availableRoles, setAvailableRoles] = useState([]);
  const [selectedRole, setSelectedRole] = useState('');

  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from?.pathname || '/';

  const handleCredentialsSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const result = await login(email, password);
      if (result?.requiresRoleSelection) {
        setAvailableRoles(result.availableRoles);
        setSelectedRole(result.availableRoles[0] ?? '');
        setStep('roleSelection');
      } else {
        navigate(from, { replace: true });
      }
    } catch (err) {
      setError(err.message || 'Error al iniciar sesión.');
    } finally {
      setLoading(false);
    }
  };

  const handleRoleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await login(email, password, selectedRole);
      navigate(from, { replace: true });
    } catch (err) {
      setError(err.message || 'Error al iniciar sesión.');
    } finally {
      setLoading(false);
    }
  };

  if (step === 'roleSelection') {
    return (
      <div className="login-screen">
        <div className="login-card">
          <div className="login-card__header">
            <div className="login-card__logo">
              <i className="bi bi-grid-3x3-gap-fill"></i>
            </div>
            <h1 className="login-card__title">Dashboard v2</h1>
            <p className="login-card__subtitle">Selecciona el rol para esta sesión</p>
          </div>

          <form onSubmit={handleRoleSubmit} className="login-card__form">
            {error && (
              <div className="login-error">
                <i className="bi bi-exclamation-circle-fill"></i>
                {error}
              </div>
            )}

            <div className="form-field">
              <label className="form-field__label">Rol activo</label>
              <div className="role-list">
                {availableRoles.map((role) => (
                  <label
                    key={role}
                    className={`role-option${selectedRole === role ? ' role-option--selected' : ''}`}
                  >
                    <input
                      type="radio"
                      name="role"
                      value={role}
                      checked={selectedRole === role}
                      onChange={() => setSelectedRole(role)}
                    />
                    <i className="bi bi-person-badge me-2"></i>
                    {role}
                  </label>
                ))}
              </div>
            </div>

            <button type="submit" className="login-btn" disabled={loading || !selectedRole}>
              {loading ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                  Iniciando sesión...
                </>
              ) : (
                <>
                  <i className="bi bi-box-arrow-in-right me-2"></i>
                  Continuar
                </>
              )}
            </button>

            <button
              type="button"
              className="btn btn-link w-100 mt-2"
              onClick={() => { setStep('credentials'); setError(''); }}
            >
              Volver
            </button>
          </form>
        </div>
      </div>
    );
  }

  return (
    <div className="login-screen">
      <div className="login-card">
        <div className="login-card__header">
          <div className="login-card__logo">
            <i className="bi bi-grid-3x3-gap-fill"></i>
          </div>
          <h1 className="login-card__title">Dashboard v2</h1>
          <p className="login-card__subtitle">Inicia sesión para continuar</p>
        </div>

        <form onSubmit={handleCredentialsSubmit} className="login-card__form">
          {error && (
            <div className="login-error">
              <i className="bi bi-exclamation-circle-fill"></i>
              {error}
            </div>
          )}

          <div className="form-field">
            <label className="form-field__label" htmlFor="email">
              Correo electrónico
            </label>
            <div className="form-field__input-wrapper">
              <i className="bi bi-envelope form-field__icon"></i>
              <input
                id="email"
                type="email"
                className="form-field__input"
                placeholder="correo@ejemplo.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                autoFocus
                autoComplete="email"
              />
            </div>
          </div>

          <div className="form-field">
            <label className="form-field__label" htmlFor="password">
              Contraseña
            </label>
            <div className="form-field__input-wrapper">
              <i className="bi bi-lock form-field__icon"></i>
              <input
                id="password"
                type={showPassword ? 'text' : 'password'}
                className="form-field__input"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                autoComplete="current-password"
              />
              <button
                type="button"
                className="form-field__toggle"
                onClick={() => setShowPassword(!showPassword)}
                tabIndex={-1}
                aria-label={showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'}
              >
                <i className={`bi bi-${showPassword ? 'eye-slash' : 'eye'}`}></i>
              </button>
            </div>
          </div>

          <button
            type="submit"
            className="login-btn"
            disabled={loading}
          >
            {loading ? (
              <>
                <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                Iniciando sesión...
              </>
            ) : (
              <>
                <i className="bi bi-box-arrow-in-right me-2"></i>
                Iniciar sesión
              </>
            )}
          </button>
        </form>
      </div>
    </div>
  );
}
