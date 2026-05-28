const ACCESS_TOKEN_KEY = 'access_token';

export function authJsonHeaders(): Record<string, string> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  const token = localStorage.getItem(ACCESS_TOKEN_KEY);
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  return headers;
}
