import { sessionStore } from './session-store.js';

export class ApiError extends Error {
  constructor(message, status) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

export class ApiClient {
  constructor(baseUrl = '') {
    this.baseUrl = baseUrl.replace(/\/+$/, '');
  }

  async register({ displayName, email, password }) {
    return this.#request('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify({ displayName, email, password }),
    });
  }

  async login({ email, password }) {
    return this.#request('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
  }

  async me() {
    return this.#request('/api/auth/me');
  }

  async chapter(volumeId, bookId, chapter, selectedStart = null, selectedEnd = null) {
    const params = new URLSearchParams();
    if (selectedStart) {
      params.set('selectedStart', String(selectedStart));
    }
    if (selectedEnd) {
      params.set('selectedEnd', String(selectedEnd));
    }
    const query = params.toString();

    return this.#request(`/api/content/scriptures/${volumeId}/${bookId}/${chapter}${query ? `?${query}` : ''}`);
  }

  async annotations(reference) {
    if (!reference) {
      return this.#request('/api/annotations');
    }

    const params = new URLSearchParams({
      volumeId: reference.volumeId,
      bookId: reference.bookId,
      chapterNumber: String(reference.chapter),
    });

    return this.#request(`/api/annotations?${params}`);
  }

  async tags() {
    return this.#request('/api/tags');
  }

  async notebooks() {
    return this.#request('/api/notebooks');
  }

  async createNotebook({ title, type = 'Personal', isPublic = false }) {
    return this.#request('/api/notebooks', {
      method: 'POST',
      body: JSON.stringify({ title, type, isPublic }),
    });
  }

  async updateNotebook(id, { title, type = 'Personal', isPublic = false }) {
    return this.#request(`/api/notebooks/${id}`, {
      method: 'PUT',
      body: JSON.stringify({ title, type, isPublic }),
    });
  }

  async deleteNotebook(id) {
    return this.#request(`/api/notebooks/${id}`, {
      method: 'DELETE',
    });
  }

  async addAnnotationToNotebook(notebookId, annotationId) {
    return this.#request(`/api/notebooks/${notebookId}/annotations`, {
      method: 'POST',
      body: JSON.stringify({ annotationId }),
    });
  }

  async removeAnnotationFromNotebook(notebookId, annotationId) {
    return this.#request(`/api/notebooks/${notebookId}/annotations/${annotationId}`, {
      method: 'DELETE',
    });
  }

  async lessons() {
    return this.#request('/api/lessons');
  }

  async createLesson({ title, summary, reference, isPublic = false }) {
    return this.#request('/api/lessons', {
      method: 'POST',
      body: JSON.stringify({ title, summary, reference, isPublic }),
    });
  }

  async publicProfile(profileSlug) {
    return this.#request(`/api/users/${encodeURIComponent(profileSlug)}`);
  }

  async publicProfileAnnotations(profileSlug) {
    return this.#request(`/api/users/${encodeURIComponent(profileSlug)}/annotations`);
  }

  async publicProfileNotebooks(profileSlug) {
    return this.#request(`/api/users/${encodeURIComponent(profileSlug)}/notebooks`);
  }

  async subscribeToCreator(creatorUserId) {
    return this.#request(`/api/subscriptions/${creatorUserId}`, {
      method: 'POST',
    });
  }

  async unsubscribeFromCreator(creatorUserId) {
    return this.#request(`/api/subscriptions/${creatorUserId}`, {
      method: 'DELETE',
    });
  }

  async subscriptionFeed() {
    return this.#request('/api/subscriptions/feed');
  }

  async createAnnotation({
    reference,
    highlightStyle,
    visibility = 'Private',
    notePlainText = '',
    tags = [],
    contentAnchor = null,
  }) {
    return this.#request('/api/annotations', {
      method: 'POST',
      body: JSON.stringify({
        reference,
        highlightStyle,
        visibility,
        notePlainText,
        noteHtml: null,
        tags,
        contentAnchor,
      }),
    });
  }

  async updateAnnotation(id, {
    highlightStyle = 'Yellow',
    visibility = 'Private',
    notePlainText = '',
    noteHtml = null,
    tags = [],
  }) {
    return this.#request(`/api/annotations/${id}`, {
      method: 'PUT',
      body: JSON.stringify({
        highlightStyle,
        visibility,
        notePlainText,
        noteHtml,
        tags,
      }),
    });
  }

  async deleteAnnotation(id) {
    return this.#request(`/api/annotations/${id}`, {
      method: 'DELETE',
    });
  }

  async #request(path, options = {}) {
    const headers = new Headers(options.headers);
    headers.set('Content-Type', 'application/json');

    const token = sessionStore.getToken();
    if (token) {
      headers.set('Authorization', `Bearer ${token}`);
    }

    const response = await fetch(`${this.baseUrl}${path}`, {
      ...options,
      headers,
    });
    const body = await response.text();
    let data = null;

    if (body.trim()) {
      try {
        data = JSON.parse(body);
      } catch {
        throw new ApiError(`Expected JSON response from ${path}`, response.status);
      }
    }

    if (!response.ok) {
      const message = data?.message || `Request failed with ${response.status}`;
      throw new ApiError(message, response.status);
    }

    return data;
  }
}

export const apiClient = new ApiClient(import.meta.env.VITE_API_BASE_URL ?? '');
