import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

/**
 * Protege rutas que requieren el rol Administrador.
 * Si el usuario no está autenticado → /login.
 * Si está autenticado pero no es Administrador → / (inicio).
 */
export default function AdminRoute({ children }) {
  const { user, loading } = useAuth();

  if (loading) return null;

  if (!user) return <Navigate to="/login" replace />;

  if (user.role !== 'Superuser') return <Navigate to="/" replace />;

  return children;
}
