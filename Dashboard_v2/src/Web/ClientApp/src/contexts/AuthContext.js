import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';

/**
 * Autenticación basada en cookies HttpOnly.
 * El token JWT vive en una cookie HttpOnly+Secure+SameSite=Strict configurada por el servidor.
 * JavaScript nunca puede leer ese token, lo que elimina el vector de ataque XSS.
 * El navegador envía la cookie automáticamente con credentials: 'include'.
 */
const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  const fetchCurrentUser = useCallback(async () => {
    try {
      // credentials: 'include' asegura que el browser envíe la cookie HttpOnly al servidor
      const response = await fetch('/api/Auth/me', {
        credentials: 'include',
      });
      if (response.ok) {
        const data = await response.json();
        setUser(data);
      } else {
        setUser(null);
      }
    } catch {
      setUser(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchCurrentUser();
  }, [fetchCurrentUser]);

  const login = async (email, password) => {
    const response = await fetch('/api/Auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ email, password }),
    });

    if (!response.ok) {
      const data = await response.json().catch(() => ({}));
      const errors = data?.errors ?? ['Credenciales inválidas.'];
      throw new Error(Array.isArray(errors) ? errors.join(' ') : errors);
    }

    // El servidor ya colocó la cookie, solo cargamos el usuario actual
    await fetchCurrentUser();
  };

  const logout = async () => {
    // Le pedimos al servidor que limpie la cookie
    await fetch('/api/Auth/logout', {
      method: 'POST',
      credentials: 'include',
    }).catch(() => {});
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, refetch: fetchCurrentUser }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
