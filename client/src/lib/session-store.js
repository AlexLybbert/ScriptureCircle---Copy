const sessionKey = 'scripture-circle-session';
const cachedUserKey = 'scripture-circle-current-user';
const expirySkewMs = 30_000;

class SessionStore {
  save(authResponse) {
    const user = {
      userId: authResponse.userId,
      displayName: authResponse.displayName,
      email: authResponse.email,
      profileSlug: authResponse.profileSlug,
    };
    const session = {
      token: authResponse.token,
      user,
      expiresAt: getJwtExpiry(authResponse.token),
      savedAt: Date.now(),
    };

    sessionStorage.setItem(sessionKey, JSON.stringify(session));
    sessionStorage.setItem(cachedUserKey, JSON.stringify(user));
    return session;
  }

  getSession() {
    const session = parseJson(sessionStorage.getItem(sessionKey));
    if (!session?.token || this.isExpired(session)) {
      this.clear();
      return null;
    }

    return session;
  }

  getToken() {
    return this.getSession()?.token || null;
  }

  getCachedUser() {
    return parseJson(sessionStorage.getItem(cachedUserKey));
  }

  replaceUser(user) {
    const current = this.getSession();
    if (!current) {
      return;
    }

    const next = { ...current, user };
    sessionStorage.setItem(sessionKey, JSON.stringify(next));
    sessionStorage.setItem(cachedUserKey, JSON.stringify(user));
  }

  isExpired(session) {
    return Boolean(session.expiresAt && Date.now() > session.expiresAt - expirySkewMs);
  }

  clear() {
    sessionStorage.removeItem(sessionKey);
    sessionStorage.removeItem(cachedUserKey);
  }
}

function parseJson(value) {
  if (!value) {
    return null;
  }

  try {
    return JSON.parse(value);
  } catch {
    return null;
  }
}

function getJwtExpiry(token) {
  const [, payload] = token.split('.');
  if (!payload) {
    return null;
  }

  try {
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
    const padded = normalized.padEnd(normalized.length + ((4 - (normalized.length % 4)) % 4), '=');
    const decoded = JSON.parse(atob(padded));
    return typeof decoded.exp === 'number' ? decoded.exp * 1000 : null;
  } catch {
    return null;
  }
}

export const sessionStore = new SessionStore();
