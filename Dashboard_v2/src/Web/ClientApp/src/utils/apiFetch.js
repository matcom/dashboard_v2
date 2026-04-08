/**
 * Wrapper de fetch para llamadas a la API protegida.
 * Las cookies HttpOnly se envían automáticamente gracias a credentials: 'include'.
 * No se maneja ningún token en JavaScript: el browser gestiona la cookie de forma segura.
 */
export async function apiFetch(url, options = {}) {
  const headers = {
    'Content-Type': 'application/json',
    ...(options.headers || {}),
  };

  return fetch(url, { ...options, headers, credentials: 'include' });
}
