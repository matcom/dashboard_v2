import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export default function ProtectedRoute({ children, requiredPermission }) {
  const { user, loading, hasPermission } = useAuth();
  const location = useLocation();

  if (loading) {
    return (
      <div className="auth-loading">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Cargando...</span>
        </div>
      </div>
    );
  }

  if (!user) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (requiredPermission && !hasPermission(requiredPermission)) {
    return (
      <div className="d-flex flex-column align-items-center justify-content-center" style={{ minHeight: '60vh', gap: '1rem' }}>
        <i className="bi bi-shield-x" style={{ fontSize: '3rem', color: 'var(--bs-danger)' }}></i>
        <h4>Acceso denegado</h4>
        <p className="text-muted">No tienes permiso para ver esta página.</p>
        <Navigate to="/" replace />
      </div>
    );
  }

  return children;
}
