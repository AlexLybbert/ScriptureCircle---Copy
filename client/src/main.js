import { ApiError, apiClient } from './lib/api.js';
import {
  applyHighlights,
  applyNoteMarkers,
  createStudyPath,
  applyVerseSelection,
  createHighlightAnnotationPayload,
  createNoteAnnotationPayload,
  createSelectionLabel,
  formatHighlightSummary,
  formatNoteSummary,
  filterAnnotationsByTag,
  getTagsFromAnnotations,
  getVisibleTags,
  getCrossReferences,
  getResourceLinks,
  getVerseCount,
  getAdjacentReference,
  getDefaultReference,
  getPhraseSelectionFromWindowSelection,
  normalizeDragSelection,
  normalizeSelection,
  parseStudyPath,
  parseCrossReferenceInput,
  parseResourceLinkInput,
  parseVerseQuery,
  parseTagInput,
  populateBookSelect,
  populateChapterSelect,
  populateTagFilter,
  populateTagSuggestions,
  populateVerseRangeControls,
  populateVolumeSelect,
  renderNotesList,
  renderVerses,
  updateReaderTitle,
} from './lib/scripture-reader.js';
import { sessionStore } from './lib/session-store.js';
import './styles.css';

const routes = new Set(['/', '/login', '/register', '/dashboard']);
const authPanel = document.querySelector('[data-auth-panel]');
const dashboardPanel = document.querySelector('[data-dashboard-panel]');
const profilePanel = document.querySelector('[data-profile-panel]');
const readerPanel = document.querySelector('[data-reader-panel]');
const loginForm = document.querySelector('[data-login-form]');
const registerForm = document.querySelector('[data-register-form]');
const authMessage = document.querySelector('[data-auth-message]');
const dashboardMessage = document.querySelector('[data-dashboard-message]');
const userSummary = document.querySelector('[data-user-summary]');
const continueStudyCopy = document.querySelector('[data-continue-study-copy]');
const continueStudyLink = document.querySelector('[data-continue-study-link]');
const dashboardStats = document.querySelector('[data-dashboard-stats]');
const recentAnnotations = document.querySelector('[data-recent-annotations]');
const dashboardSearchForm = document.querySelector('[data-dashboard-search-form]');
const dashboardSearchMessage = document.querySelector('[data-dashboard-search-message]');
const dashboardSearchResults = document.querySelector('[data-dashboard-search-results]');
const notebookForm = document.querySelector('[data-notebook-form]');
const notebookMessage = document.querySelector('[data-notebook-message]');
const notebookList = document.querySelector('[data-notebook-list]');
const lessonForm = document.querySelector('[data-lesson-form]');
const lessonMessage = document.querySelector('[data-lesson-message]');
const lessonList = document.querySelector('[data-lesson-list]');
const subscriptionMessage = document.querySelector('[data-subscription-message]');
const subscriptionFeed = document.querySelector('[data-subscription-feed]');
const lessonVolumeSelect = document.querySelector('[data-lesson-volume]');
const lessonBookSelect = document.querySelector('[data-lesson-book]');
const lessonChapterSelect = document.querySelector('[data-lesson-chapter]');
const profileSummary = document.querySelector('[data-profile-summary]');
const profileMessage = document.querySelector('[data-profile-message]');
const profileAnnotations = document.querySelector('[data-profile-annotations]');
const profileNotebooks = document.querySelector('[data-profile-notebooks]');
const readerMessage = document.querySelector('[data-reader-message]');
const readerTitle = document.querySelector('[data-reader-title]');
const readerSummary = document.querySelector('[data-reader-summary]');
const readerVerses = document.querySelector('[data-reader-verses]');
const readerLoading = document.querySelector('[data-reader-loading]');
const volumeSelect = document.querySelector('[data-volume-select]');
const bookSelect = document.querySelector('[data-book-select]');
const chapterSelect = document.querySelector('[data-chapter-select]');
const previousChapterLink = document.querySelector('[data-previous-chapter]');
const nextChapterLink = document.querySelector('[data-next-chapter]');
const selectionPanel = document.querySelector('[data-selection-panel]');
const selectionLabel = document.querySelector('[data-selection-label]');
const selectionHelp = document.querySelector('[data-selection-help]');
const selectionStartSelect = document.querySelector('[data-selection-start]');
const selectionEndSelect = document.querySelector('[data-selection-end]');
const clearSelectionButton = document.querySelector('[data-clear-selection]');
const highlightForm = document.querySelector('[data-highlight-form]');
const highlightFieldset = document.querySelector('[data-highlight-fieldset]');
const highlightSummary = document.querySelector('[data-highlight-summary]');
const saveHighlightButton = document.querySelector('[data-save-highlight]');
const annotationTagsInput = document.querySelector('[data-annotation-tags]');
const annotationCrossrefsInput = document.querySelector('[data-annotation-crossrefs]');
const annotationLinksInput = document.querySelector('[data-annotation-links]');
const annotationVisibilitySelect = document.querySelector('[data-annotation-visibility]');
const tagSuggestions = document.querySelector('[data-tag-suggestions]');
const tagFilter = document.querySelector('[data-tag-filter]');
const noteForm = document.querySelector('[data-note-form]');
const noteText = document.querySelector('[data-note-text]');
const saveNoteButton = document.querySelector('[data-save-note]');
const noteSummary = document.querySelector('[data-note-summary]');
const noteList = document.querySelector('[data-note-list]');
const protectedLinks = document.querySelectorAll('[data-protected-link]');
const signOutButtons = document.querySelectorAll('[data-sign-out]');

let activeUser = sessionStore.getCachedUser();
let activeReference = null;
let activeChapter = null;
let activeSelection = { verseStart: null, verseEnd: null };
let activeAnnotations = [];
let dashboardAnnotations = [];
let activeTags = [];
let activeNotebooks = [];
let activeLessons = [];
let activeSubscriptionFeed = [];
let activeTagFilter = '';
let activeDashboardSearch = { query: '', scope: 'all' };
let dragSelection = null;
let suppressNextVerseClick = false;

init();

function init() {
  loginForm.addEventListener('submit', (event) => {
    event.preventDefault();
    void submitLogin(new FormData(loginForm));
  });

  registerForm.addEventListener('submit', (event) => {
    event.preventDefault();
    void submitRegister(new FormData(registerForm));
  });

  document.addEventListener('click', (event) => {
    const link = event.target.closest('a[data-route]');
    if (!link) {
      return;
    }

    event.preventDefault();
    navigate(link.getAttribute('href'));
  });

  signOutButtons.forEach((button) => {
    button.addEventListener('click', () => {
      signOut();
    });
  });

  populateVolumeSelect(volumeSelect);
  populateVolumeSelect(lessonVolumeSelect);
  populateBookSelect(lessonBookSelect, lessonVolumeSelect.value);
  populateChapterSelect(lessonChapterSelect, lessonVolumeSelect.value, lessonBookSelect.value);

  volumeSelect.addEventListener('change', () => {
    const volumeId = volumeSelect.value;
    populateBookSelect(bookSelect, volumeId);
    populateChapterSelect(chapterSelect, volumeId, bookSelect.value);
    navigate(createStudyPath({
      volumeId,
      bookId: bookSelect.value,
      chapter: Number(chapterSelect.value),
    }));
  });

  bookSelect.addEventListener('change', () => {
    populateChapterSelect(chapterSelect, volumeSelect.value, bookSelect.value);
    navigate(createStudyPath({
      volumeId: volumeSelect.value,
      bookId: bookSelect.value,
      chapter: Number(chapterSelect.value),
    }));
  });

  chapterSelect.addEventListener('change', () => {
    navigate(createStudyPath({
      volumeId: volumeSelect.value,
      bookId: bookSelect.value,
      chapter: Number(chapterSelect.value),
    }));
  });

  lessonVolumeSelect.addEventListener('change', () => {
    populateBookSelect(lessonBookSelect, lessonVolumeSelect.value);
    populateChapterSelect(lessonChapterSelect, lessonVolumeSelect.value, lessonBookSelect.value);
  });

  lessonBookSelect.addEventListener('change', () => {
    populateChapterSelect(lessonChapterSelect, lessonVolumeSelect.value, lessonBookSelect.value);
  });

  notebookForm.addEventListener('submit', (event) => {
    event.preventDefault();
    void createNotebook(new FormData(notebookForm));
  });

  notebookList.addEventListener('click', (event) => {
    const action = event.target.closest('[data-action]');
    if (action) {
      handleNotebookAction(action);
    }
  });

  notebookList.addEventListener('submit', (event) => {
    const form = event.target.closest('.notebook-edit-form');
    if (!form) {
      return;
    }

    event.preventDefault();
    void updateNotebook(form);
  });

  lessonForm.addEventListener('submit', (event) => {
    event.preventDefault();
    void createLesson(new FormData(lessonForm));
  });

  dashboardSearchForm.addEventListener('submit', (event) => {
    event.preventDefault();
    submitDashboardSearch(new FormData(dashboardSearchForm));
  });

  profileSummary.addEventListener('click', (event) => {
    const action = event.target.closest('[data-subscription-action]');
    if (action) {
      void handleProfileSubscription(action);
    }
  });

  readerVerses.addEventListener('click', (event) => {
    if (suppressNextVerseClick) {
      suppressNextVerseClick = false;
      event.preventDefault();
      return;
    }

    const verse = event.target.closest('[data-verse]');
    if (!verse) {
      return;
    }

    selectVerse(Number(verse.dataset.verse));
  });

  readerVerses.addEventListener('pointerdown', (event) => {
    const verse = event.target.closest('[data-verse]');
    if (!verse || event.button !== 0) {
      return;
    }

    startDragSelection(event, Number(verse.dataset.verse));
  });

  readerVerses.addEventListener('pointermove', (event) => {
    updateDragSelection(event);
  });

  readerVerses.addEventListener('pointerup', (event) => {
    finishDragSelection(event);
  });

  readerVerses.addEventListener('pointercancel', () => {
    cancelDragSelection();
  });

  readerVerses.addEventListener('keydown', (event) => {
    if (event.key !== 'Enter' && event.key !== ' ') {
      return;
    }

    const verse = event.target.closest('[data-verse]');
    if (!verse) {
      return;
    }

    event.preventDefault();
    selectVerse(Number(verse.dataset.verse));
  });

  selectionStartSelect.addEventListener('change', () => {
    updateSelectionFromControls();
  });

  selectionEndSelect.addEventListener('change', () => {
    updateSelectionFromControls();
  });

  clearSelectionButton.addEventListener('click', () => {
    clearSelection();
  });

  tagFilter.addEventListener('change', () => {
    activeTagFilter = tagFilter.value;
    renderAnnotationState();
    applyVerseSelection(readerVerses, activeSelection);
  });

  highlightForm.addEventListener('submit', (event) => {
    event.preventDefault();
    void saveHighlight(new FormData(highlightForm));
  });

  noteForm.addEventListener('submit', (event) => {
    event.preventDefault();
    void saveNote(new FormData(noteForm));
  });

  noteList.addEventListener('click', (event) => {
    const action = event.target.closest('[data-action]');
    if (!action) {
      return;
    }

    handleNoteAction(action);
  });

  noteList.addEventListener('submit', (event) => {
    const editForm = event.target.closest('.note-edit-form');
    const notebookFormElement = event.target.closest('.notebook-picker');
    if (editForm) {
      event.preventDefault();
      void updateNote(editForm);
    }

    if (notebookFormElement) {
      event.preventDefault();
      void addNoteToNotebook(notebookFormElement);
    }
  });

  window.addEventListener('popstate', () => {
    renderRoute();
  });

  protectedLinks.forEach((link) => {
    link.addEventListener('click', (event) => {
      if (!sessionStore.getSession()) {
        event.preventDefault();
        navigate(`/login?returnTo=${encodeURIComponent(link.getAttribute('href'))}`);
      }
    });
  });

  renderCachedUser();
  void validateSession();
  renderRoute();
}

async function submitLogin(formData) {
  setFormBusy(loginForm, true);
  showMessage(authMessage, 'Signing you in...', 'info');

  try {
    const auth = await apiClient.login({
      email: String(formData.get('login-email')).trim(),
      password: String(formData.get('login-password')),
    });
    completeSignIn(auth);
  } catch (error) {
    showMessage(authMessage, getErrorMessage(error, 'Unable to sign in.'), 'error');
  } finally {
    setFormBusy(loginForm, false);
  }
}

async function submitRegister(formData) {
  setFormBusy(registerForm, true);
  showMessage(authMessage, 'Creating your account...', 'info');

  try {
    const auth = await apiClient.register({
      displayName: String(formData.get('display-name')).trim(),
      email: String(formData.get('register-email')).trim(),
      password: String(formData.get('register-password')),
    });
    completeSignIn(auth);
  } catch (error) {
    showMessage(authMessage, getErrorMessage(error, 'Unable to create account.'), 'error');
  } finally {
    setFormBusy(registerForm, false);
  }
}

function completeSignIn(auth) {
  const session = sessionStore.save(auth);
  activeUser = session.user;
  renderCachedUser();
  showMessage(authMessage, 'You are signed in.', 'success');
  navigate(getReturnTo() || '/dashboard');
}

async function validateSession() {
  const session = sessionStore.getSession();
  if (!session) {
    activeUser = null;
    renderCachedUser();
    renderRoute();
    return;
  }

  try {
    const user = await apiClient.me();
    activeUser = {
      userId: user.userId,
      displayName: user.displayName,
      email: user.email,
      profileSlug: user.profileSlug,
    };
    sessionStore.replaceUser(activeUser);
    renderCachedUser();
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      sessionStore.clear();
      activeUser = null;
      renderCachedUser();
      renderRoute();
      return;
    }

    showMessage(dashboardMessage, 'Session check failed. You can keep working, but some account features may be unavailable.', 'error');
  }
}

function signOut() {
  sessionStore.clear();
  activeUser = null;
  renderCachedUser();
  showMessage(authMessage, 'You have been signed out.', 'success');
  navigate('/login');
}

function renderRoute() {
  const studyReference = parseStudyPath(window.location.pathname);
  const profileSlug = parseProfilePath(window.location.pathname);
  const path = studyReference ? '/study' : profileSlug ? '/profile' : routes.has(window.location.pathname) ? window.location.pathname : '/';
  const hasSession = Boolean(sessionStore.getSession());

  if (path === '/profile') {
    showPanel('profile');
    void renderPublicProfile(profileSlug);
    return;
  }

  if (path === '/study') {
    showPanel('reader');
    void renderScriptureReader(studyReference);
    return;
  }

  if ((path === '/' || path === '/dashboard') && !hasSession) {
    showPanel('auth');
    setAuthMode('login');
    return;
  }

  if (path === '/register') {
    showPanel('auth');
    setAuthMode('register');
    return;
  }

  if (path === '/login') {
    showPanel('auth');
    setAuthMode('login');
    return;
  }

  showPanel('dashboard');
  renderDashboard();
}

function renderDashboard() {
  const session = sessionStore.getSession();
  if (!session) {
    navigate('/login?returnTo=%2Fdashboard');
    return;
  }

  const user = activeUser || session.user;
  userSummary.replaceChildren(
    element('p', 'eyebrow', 'Signed in as'),
    element('h1', '', user.displayName),
    element('p', 'muted', user.email),
    element('p', 'session-note', getSessionSummary(session)),
    createProfileLink(user.profileSlug),
  );
  showMessage(dashboardMessage, 'Your account session is active. Scripture study features can now use your authenticated API token.', 'success');
  renderDashboardOverview();
  renderRecentAnnotations(true);
  renderDashboardSearchResults(true);
  void renderWorkspace();
}

async function renderWorkspace() {
  if (!sessionStore.getSession()) {
    return;
  }

  const [notebookResult, lessonResult, annotationResult, feedResult] = await Promise.allSettled([
      apiClient.notebooks(),
      apiClient.lessons(),
      apiClient.annotations(),
      apiClient.subscriptionFeed(),
  ]);

  activeNotebooks = getSettledValue(notebookResult, []);
  activeLessons = getSettledValue(lessonResult, []);
  dashboardAnnotations = getSettledValue(annotationResult, []);
  activeSubscriptionFeed = getSettledValue(feedResult, []);

  renderDashboardOverview();
  renderRecentAnnotations();
  renderDashboardSearchResults();
  renderNotebookList();
  renderLessonList();
  renderSubscriptionFeed();

  showMessage(notebookMessage, notebookResult.status === 'rejected' ? getErrorMessage(notebookResult.reason, 'Notebooks could not be loaded.') : '', notebookResult.status === 'rejected' ? 'error' : 'info');
  showMessage(lessonMessage, lessonResult.status === 'rejected' ? getErrorMessage(lessonResult.reason, 'Lessons could not be loaded.') : '', lessonResult.status === 'rejected' ? 'error' : 'info');
  showMessage(subscriptionMessage, feedResult.status === 'rejected' ? getErrorMessage(feedResult.reason, 'Subscription feed could not be loaded.') : '', feedResult.status === 'rejected' ? 'error' : 'info');

  if (annotationResult.status === 'rejected') {
    showMessage(dashboardMessage, getErrorMessage(annotationResult.reason, 'Recent activity could not be loaded.'), 'error');
  }
}

function submitDashboardSearch(formData) {
  activeDashboardSearch = {
    query: String(formData.get('dashboard-search') || '').trim(),
    scope: String(formData.get('dashboard-search-scope') || 'all'),
  };
  renderDashboardSearchResults();
}

async function renderPublicProfile(profileSlug) {
  profileSummary.replaceChildren(
    element('p', 'eyebrow', 'Public Profile'),
    element('h1', '', 'Reader Profile'),
    element('p', 'muted', 'Loading public study activity...'),
  );
  profileAnnotations.replaceChildren(element('p', 'empty-state', 'Loading public annotations...'));
  profileNotebooks.replaceChildren(element('p', 'empty-state', 'Loading public notebooks...'));
  showMessage(profileMessage, '', 'info');

  try {
    const [profile, annotations, notebooks] = await Promise.all([
      apiClient.publicProfile(profileSlug),
      apiClient.publicProfileAnnotations(profileSlug),
      apiClient.publicProfileNotebooks(profileSlug),
    ]);

    profileSummary.replaceChildren(
      element('p', 'eyebrow', 'Public Profile'),
      element('h1', '', profile.displayName),
      element('p', 'muted', `@${profile.profileSlug}`),
      createSubscriptionControl(profile),
    );
    renderPublicAnnotations(annotations);
    renderPublicNotebooks(notebooks);
  } catch (error) {
    profileSummary.replaceChildren(
      element('p', 'eyebrow', 'Public Profile'),
      element('h1', '', 'Profile not found'),
      element('p', 'muted', `No public profile was found for @${profileSlug}.`),
    );
    profileAnnotations.replaceChildren(element('p', 'empty-state', 'Public annotations could not be loaded.'));
    profileNotebooks.replaceChildren(element('p', 'empty-state', 'Public notebooks could not be loaded.'));
    showMessage(profileMessage, getErrorMessage(error, 'Unable to load public profile.'), 'error');
  }
}

async function handleProfileSubscription(action) {
  if (!sessionStore.getSession()) {
    navigate(`/login?returnTo=${encodeURIComponent(window.location.pathname)}`);
    return;
  }

  const creatorUserId = action.dataset.creatorUserId;
  const profileSlug = action.dataset.profileSlug;
  action.disabled = true;
  showMessage(profileMessage, action.dataset.subscriptionAction === 'subscribe' ? 'Subscribing...' : 'Unsubscribing...', 'info');

  try {
    if (action.dataset.subscriptionAction === 'subscribe') {
      await apiClient.subscribeToCreator(creatorUserId);
    } else {
      await apiClient.unsubscribeFromCreator(creatorUserId);
    }

    await renderPublicProfile(profileSlug);
    showMessage(profileMessage, action.dataset.subscriptionAction === 'subscribe' ? 'Subscribed to this reader.' : 'Subscription removed.', 'success');
  } catch (error) {
    showMessage(profileMessage, getErrorMessage(error, 'Unable to update subscription.'), 'error');
    action.disabled = false;
  }
}

async function renderScriptureReader(reference) {
  const resolvedReference = reference || getDefaultReference();
  const selection = parseVerseQuery(window.location.search);
  activeReference = resolvedReference;
  activeChapter = null;
  activeSelection = selection;
  activeAnnotations = [];
  activeTagFilter = '';
  syncReaderControls(resolvedReference);
  resetSelectionPanel();
  updateReaderTitle(readerTitle, {
    bookTitle: resolvedReference.bookTitle,
    chapter: resolvedReference.chapter,
  });
  readerSummary.textContent = 'Loading scripture text...';
  readerLoading.hidden = false;
  readerVerses.replaceChildren();
  showMessage(readerMessage, '', 'info');
  updateChapterLinks(resolvedReference);

  try {
    const chapter = await apiClient.chapter(
      resolvedReference.volumeId,
      resolvedReference.bookId,
      resolvedReference.chapter,
      selection.verseStart,
      selection.verseEnd,
    );
    if (!isActiveReference(resolvedReference)) {
      return;
    }

    updateReaderTitle(readerTitle, chapter);
    readerSummary.textContent = chapter.summary || `${chapter.delineation || 'Chapter'} ${chapter.chapter}`;
    activeChapter = chapter;
    activeSelection = normalizeSelection(selection.verseStart, selection.verseEnd, getVerseCount(chapter));
    renderVerses(readerVerses, chapter);
    await loadAnnotations(resolvedReference);
    await loadTags();
    await loadNotebooks();
    renderAnnotationState();
    applyVerseSelection(readerVerses, activeSelection);
    updateSelectionPanel();
  } catch (error) {
    if (!isActiveReference(resolvedReference)) {
      return;
    }

    readerVerses.replaceChildren();
    activeChapter = null;
    activeSelection = { verseStart: null, verseEnd: null };
    activeAnnotations = [];
    activeTagFilter = '';
    resetSelectionPanel();
    readerSummary.textContent = 'Scripture text could not be loaded.';
    showMessage(readerMessage, getErrorMessage(error, 'Unable to load scripture chapter.'), 'error');
  } finally {
    if (isActiveReference(resolvedReference)) {
      readerLoading.hidden = true;
    }
  }
}

async function loadNotebooks() {
  if (!sessionStore.getSession()) {
    activeNotebooks = [];
    return;
  }

  try {
    activeNotebooks = await apiClient.notebooks();
  } catch (error) {
    activeNotebooks = [];
  }
}

async function createNotebook(formData) {
  const payload = {
    title: String(formData.get('notebook-title') || '').trim(),
    type: String(formData.get('notebook-type') || 'Personal'),
    isPublic: formData.has('notebook-public'),
  };
  if (!payload.title) {
    showMessage(notebookMessage, 'Notebook title is required.', 'error');
    return;
  }

  setFormBusy(notebookForm, true);
  showMessage(notebookMessage, 'Creating notebook...', 'info');

  try {
    const notebook = await apiClient.createNotebook(payload);
    activeNotebooks = [...activeNotebooks, notebook].sort(compareByTitle);
    notebookForm.reset();
    renderNotebookList();
    showMessage(notebookMessage, 'Notebook created.', 'success');
  } catch (error) {
    showMessage(notebookMessage, getErrorMessage(error, 'Unable to create notebook.'), 'error');
  } finally {
    setFormBusy(notebookForm, false);
  }
}

function handleNotebookAction(action) {
  const card = action.closest('[data-notebook-id]');
  if (!card) {
    return;
  }

  if (action.dataset.action === 'edit-notebook') {
    setNotebookEditing(card, true);
    return;
  }

  if (action.dataset.action === 'cancel-notebook-edit') {
    setNotebookEditing(card, false);
    return;
  }

  if (action.dataset.action === 'delete-notebook') {
    void deleteNotebook(card.dataset.notebookId);
  }
}

async function updateNotebook(form) {
  const card = form.closest('[data-notebook-id]');
  const notebook = activeNotebooks.find((item) => item.id === card?.dataset.notebookId);
  if (!notebook) {
    return;
  }

  const formData = new FormData(form);
  const payload = {
    title: String(formData.get('notebook-title') || '').trim(),
    type: String(formData.get('notebook-type') || 'Personal'),
    isPublic: formData.has('notebook-public'),
  };
  if (!payload.title) {
    showMessage(notebookMessage, 'Notebook title is required.', 'error');
    return;
  }

  setNotebookCardBusy(card, true);
  showMessage(notebookMessage, 'Updating notebook...', 'info');

  try {
    const updated = await apiClient.updateNotebook(notebook.id, payload);
    activeNotebooks = activeNotebooks.map((item) => (item.id === updated.id ? updated : item)).sort(compareByTitle);
    renderNotebookList();
    showMessage(notebookMessage, 'Notebook updated.', 'success');
  } catch (error) {
    showMessage(notebookMessage, getErrorMessage(error, 'Unable to update notebook.'), 'error');
  } finally {
    setNotebookCardBusy(card, false);
  }
}

async function deleteNotebook(notebookId) {
  showMessage(notebookMessage, 'Deleting notebook...', 'info');

  try {
    await apiClient.deleteNotebook(notebookId);
    activeNotebooks = activeNotebooks.filter((notebook) => notebook.id !== notebookId);
    renderNotebookList();
    showMessage(notebookMessage, 'Notebook deleted.', 'success');
  } catch (error) {
    showMessage(notebookMessage, getErrorMessage(error, 'Unable to delete notebook.'), 'error');
  }
}

async function createLesson(formData) {
  const bookOption = lessonBookSelect.selectedOptions[0];
  const payload = {
    title: String(formData.get('lesson-title') || '').trim(),
    summary: String(formData.get('lesson-summary') || '').trim(),
    reference: {
      volumeId: lessonVolumeSelect.value,
      bookId: lessonBookSelect.value,
      bookTitle: bookOption?.textContent || lessonBookSelect.value,
      chapter: Number(lessonChapterSelect.value),
      verseStart: null,
      verseEnd: null,
    },
    isPublic: formData.has('lesson-public'),
  };
  if (!payload.title || !payload.summary) {
    showMessage(lessonMessage, 'Lesson title and summary are required.', 'error');
    return;
  }

  setFormBusy(lessonForm, true);
  showMessage(lessonMessage, 'Creating lesson...', 'info');

  try {
    const lesson = await apiClient.createLesson(payload);
    activeLessons = [lesson, ...activeLessons];
    lessonForm.reset();
    populateBookSelect(lessonBookSelect, lessonVolumeSelect.value);
    populateChapterSelect(lessonChapterSelect, lessonVolumeSelect.value, lessonBookSelect.value);
    renderLessonList();
    showMessage(lessonMessage, 'Lesson created.', 'success');
  } catch (error) {
    showMessage(lessonMessage, getErrorMessage(error, 'Unable to create lesson.'), 'error');
  } finally {
    setFormBusy(lessonForm, false);
  }
}

async function loadAnnotations(reference) {
  if (!sessionStore.getSession()) {
    activeAnnotations = [];
    highlightSummary.textContent = 'Sign in to save and view your private highlights.';
    return;
  }

  try {
    activeAnnotations = await apiClient.annotations(reference);
    highlightSummary.textContent = formatHighlightSummary(activeAnnotations);
  } catch (error) {
    activeAnnotations = [];
    if (error instanceof ApiError && error.status === 401) {
      sessionStore.clear();
      activeUser = null;
      renderCachedUser();
      highlightSummary.textContent = 'Sign in again to save and view highlights.';
      return;
    }

    highlightSummary.textContent = 'Highlights could not be loaded.';
  }
}

async function loadTags() {
  if (!sessionStore.getSession()) {
    activeTags = [];
    populateTagSuggestions(tagSuggestions, []);
    populateTagFilter(tagFilter, [], activeTagFilter);
    tagFilter.disabled = true;
    return;
  }

  try {
    activeTags = await apiClient.tags();
  } catch (error) {
    activeTags = [];
  }
}

async function saveHighlight(formData) {
  const hasSelection = Boolean(activeSelection.verseStart);
  if (!hasSelection) {
    showMessage(readerMessage, 'Select a verse before saving a highlight.', 'error');
    return;
  }

  if (!sessionStore.getSession()) {
    navigate(`/login?returnTo=${encodeURIComponent(window.location.pathname + window.location.search)}`);
    return;
  }

  const highlightStyle = String(formData.get('highlight-style') || 'Yellow');
  const tags = getCurrentAnnotationMetadata();
  const payload = createHighlightAnnotationPayload(activeReference, activeSelection, highlightStyle, tags, getAnnotationVisibility());
  setHighlightBusy(true);
  showMessage(readerMessage, 'Saving highlight...', 'info');

  try {
    const annotation = await apiClient.createAnnotation(payload);
    activeAnnotations = [annotation, ...activeAnnotations];
    mergeActiveTags(tags);
    clearAnnotationMetadataInputs();
    renderAnnotationState();
    showMessage(readerMessage, `${highlightStyle} highlight saved for ${createSelectionLabel(activeReference, activeSelection)}.`, 'success');
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      sessionStore.clear();
      activeUser = null;
      renderCachedUser();
      navigate(`/login?returnTo=${encodeURIComponent(window.location.pathname + window.location.search)}`);
      return;
    }

    showMessage(readerMessage, getErrorMessage(error, 'Unable to save highlight.'), 'error');
  } finally {
    setHighlightBusy(false);
  }
}

async function saveNote(formData) {
  const hasSelection = Boolean(activeSelection.verseStart);
  const text = String(formData.get('note-text') || '').trim();
  if (!hasSelection) {
    showMessage(readerMessage, 'Select a verse or phrase before saving a note.', 'error');
    return;
  }

  if (!text) {
    showMessage(readerMessage, 'Write a note before saving.', 'error');
    return;
  }

  if (!sessionStore.getSession()) {
    navigate(`/login?returnTo=${encodeURIComponent(window.location.pathname + window.location.search)}`);
    return;
  }

  const payload = createNoteAnnotationPayload(activeReference, activeSelection, text, [], getAnnotationVisibility());
  payload.tags = [...payload.tags, ...getCurrentAnnotationMetadata()];
  setNoteBusy(true);
  showMessage(readerMessage, 'Saving note...', 'info');

  try {
    const annotation = await apiClient.createAnnotation(payload);
    activeAnnotations = [annotation, ...activeAnnotations];
    mergeActiveTags(payload.tags);
    noteForm.reset();
    clearAnnotationMetadataInputs();
    renderAnnotationState();
    applyVerseSelection(readerVerses, activeSelection);
    showMessage(readerMessage, `Note saved for ${createSelectionLabel(activeReference, activeSelection)}.`, 'success');
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      sessionStore.clear();
      activeUser = null;
      renderCachedUser();
      navigate(`/login?returnTo=${encodeURIComponent(window.location.pathname + window.location.search)}`);
      return;
    }

    showMessage(readerMessage, getErrorMessage(error, 'Unable to save note.'), 'error');
  } finally {
    setNoteBusy(false);
  }
}

function handleNoteAction(action) {
  const card = action.closest('[data-note-id]');
  if (!card) {
    return;
  }

  if (action.dataset.action === 'edit-note') {
    setNoteCardEditing(card, true);
    return;
  }

  if (action.dataset.action === 'cancel-note-edit') {
    setNoteCardEditing(card, false);
    return;
  }

  if (action.dataset.action === 'delete-note') {
    void deleteNote(card.dataset.noteId);
  }
}

async function updateNote(form) {
  const card = form.closest('[data-note-id]');
  const note = activeAnnotations.find((annotation) => annotation.id === card?.dataset.noteId);
  const formData = new FormData(form);
  const text = String(formData.get('note-text') || '').trim();
  const tags = parseTagInput(formData.get('annotation-tags'), ['study-note']);
  const metadata = [
    ...tags,
    ...parseCrossReferenceInput(formData.get('annotation-crossrefs')),
    ...parseResourceLinkInput(formData.get('annotation-links')),
  ];
  const visibility = String(formData.get('annotation-visibility') || note.visibility || 'Private');
  if (!note || !text) {
    showMessage(readerMessage, 'Write a note before saving changes.', 'error');
    return;
  }

  setNoteCardBusy(card, true);
  showMessage(readerMessage, 'Updating note...', 'info');

  try {
    const updated = await apiClient.updateAnnotation(note.id, {
      highlightStyle: note.highlightStyle,
      visibility,
      notePlainText: text,
      tags: metadata,
    });
    activeAnnotations = activeAnnotations.map((annotation) => (
      annotation.id === updated.id ? updated : annotation
    ));
    mergeActiveTags(metadata);
    renderAnnotationState();
    applyVerseSelection(readerVerses, activeSelection);
    showMessage(readerMessage, 'Note updated.', 'success');
  } catch (error) {
    showMessage(readerMessage, getErrorMessage(error, 'Unable to update note.'), 'error');
  } finally {
    setNoteCardBusy(card, false);
  }
}

async function deleteNote(noteId) {
  const note = activeAnnotations.find((annotation) => annotation.id === noteId);
  if (!note) {
    return;
  }

  showMessage(readerMessage, 'Deleting note...', 'info');

  try {
    await apiClient.deleteAnnotation(note.id);
    activeAnnotations = activeAnnotations.filter((annotation) => annotation.id !== note.id);
    renderAnnotationState();
    applyVerseSelection(readerVerses, activeSelection);
    showMessage(readerMessage, 'Note deleted.', 'success');
  } catch (error) {
    showMessage(readerMessage, getErrorMessage(error, 'Unable to delete note.'), 'error');
  }
}

async function addNoteToNotebook(form) {
  const card = form.closest('[data-note-id]');
  const notebookId = new FormData(form).get('notebook-id');
  if (!card || !notebookId) {
    showMessage(readerMessage, 'Choose a notebook first.', 'error');
    return;
  }

  setNoteCardBusy(card, true);
  showMessage(readerMessage, 'Adding note to notebook...', 'info');

  try {
    await apiClient.addAnnotationToNotebook(String(notebookId), card.dataset.noteId);
    activeNotebooks = activeNotebooks.map((notebook) => (
      notebook.id === notebookId
        ? { ...notebook, annotationIds: [...(notebook.annotationIds || []), card.dataset.noteId] }
        : notebook
    ));
    renderAnnotationState();
    applyVerseSelection(readerVerses, activeSelection);
    showMessage(readerMessage, 'Note added to notebook.', 'success');
  } catch (error) {
    showMessage(readerMessage, getErrorMessage(error, 'Unable to add note to notebook.'), 'error');
  } finally {
    setNoteCardBusy(card, false);
  }
}

function selectVerse(verseNumber) {
  if (!activeChapter || !activeReference) {
    return;
  }

  activeSelection = normalizeSelection(verseNumber, verseNumber, getVerseCount(activeChapter));
  applyVerseSelection(readerVerses, activeSelection);
  updateSelectionPanel();
  replaceUrlForSelection();
}

function startDragSelection(event, verseNumber) {
  if (!activeChapter || !activeReference) {
    return;
  }

  const isPhraseDrag = Boolean(event.target.closest('.verse-text'));
  dragSelection = {
    pointerId: event.pointerId,
    startVerse: verseNumber,
    endVerse: verseNumber,
    phraseDrag: isPhraseDrag,
    moved: false,
  };

  if (!isPhraseDrag) {
    readerVerses.classList.add('is-dragging');
    readerVerses.setPointerCapture?.(event.pointerId);
  }

  activeSelection = normalizeDragSelection(verseNumber, verseNumber, getVerseCount(activeChapter));
  applyVerseSelection(readerVerses, activeSelection);
  updateSelectionPanel();
}

function updateDragSelection(event) {
  if (!dragSelection || event.pointerId !== dragSelection.pointerId || !activeChapter) {
    return;
  }

  const verse = document.elementFromPoint(event.clientX, event.clientY)?.closest?.('[data-verse]');
  if (!verse || !readerVerses.contains(verse)) {
    return;
  }

  const endVerse = Number(verse.dataset.verse);
  if (dragSelection.phraseDrag && endVerse === dragSelection.startVerse) {
    return;
  }

  if (dragSelection.phraseDrag && endVerse !== dragSelection.startVerse) {
    dragSelection.phraseDrag = false;
    readerVerses.classList.add('is-dragging');
    readerVerses.setPointerCapture?.(event.pointerId);
  }

  if (endVerse !== dragSelection.endVerse) {
    dragSelection.moved = true;
    dragSelection.endVerse = endVerse;
    activeSelection = normalizeDragSelection(dragSelection.startVerse, endVerse, getVerseCount(activeChapter));
    applyVerseSelection(readerVerses, activeSelection);
    updateSelectionPanel();
  }
}

function finishDragSelection(event) {
  if (!dragSelection || event.pointerId !== dragSelection.pointerId) {
    return;
  }

  const phraseSelection = dragSelection.phraseDrag
    ? getPhraseSelectionFromWindowSelection(window.getSelection(), readerVerses)
    : null;
  const wasRangeDrag = dragSelection.moved && dragSelection.startVerse !== dragSelection.endVerse;

  if (phraseSelection) {
    activeSelection = phraseSelection;
    applyVerseSelection(readerVerses, activeSelection);
    updateSelectionPanel();
  }

  if (readerVerses.hasPointerCapture?.(event.pointerId)) {
    readerVerses.releasePointerCapture(event.pointerId);
  }
  readerVerses.classList.remove('is-dragging');
  replaceUrlForSelection();
  suppressNextVerseClick = Boolean(phraseSelection) || wasRangeDrag;
  dragSelection = null;
}

function cancelDragSelection() {
  dragSelection = null;
  readerVerses.classList.remove('is-dragging');
}

function updateSelectionFromControls() {
  if (!activeChapter || !activeReference) {
    return;
  }

  activeSelection = normalizeSelection(
    Number(selectionStartSelect.value),
    Number(selectionEndSelect.value),
    getVerseCount(activeChapter),
  );
  applyVerseSelection(readerVerses, activeSelection);
  updateSelectionPanel();
  replaceUrlForSelection();
}

function clearSelection() {
  activeSelection = { verseStart: null, verseEnd: null };
  applyVerseSelection(readerVerses, activeSelection);
  updateSelectionPanel();
  replaceUrlForSelection();
}

function updateSelectionPanel() {
  const hasSelection = Boolean(activeSelection.verseStart);
  const hasPhraseSelection = hasSelection
    && activeSelection.startOffset !== null
    && activeSelection.startOffset !== undefined;
  selectionPanel.toggleAttribute('data-has-selection', hasSelection);
  selectionLabel.textContent = createSelectionLabel(activeReference, activeSelection);
  selectionHelp.textContent = getSelectionHelpText(hasSelection, hasPhraseSelection);
  selectionStartSelect.disabled = !activeChapter;
  selectionEndSelect.disabled = !activeChapter;
  clearSelectionButton.disabled = !hasSelection;
  saveHighlightButton.disabled = !hasSelection;
  highlightFieldset.disabled = !hasSelection;
  noteText.disabled = !hasSelection;
  saveNoteButton.disabled = !hasSelection;
  annotationTagsInput.disabled = !hasSelection;
  annotationCrossrefsInput.disabled = !hasSelection;
  annotationLinksInput.disabled = !hasSelection;
  annotationVisibilitySelect.disabled = !hasSelection;

  if (activeChapter) {
    populateVerseRangeControls(selectionStartSelect, selectionEndSelect, activeChapter, activeSelection);
  }
}

function getSelectionHelpText(hasSelection, hasPhraseSelection) {
  if (hasPhraseSelection) {
    return 'Save this highlighted phrase, or use the range controls to switch back to whole verses.';
  }

  return hasSelection
    ? 'Use the range controls to include more verses.'
    : 'Click a verse, or drag across words to highlight part of a verse.';
}

function resetSelectionPanel() {
  selectionLabel.textContent = 'No verse selected';
  selectionHelp.textContent = 'Click or press Enter on a verse to select it.';
  selectionStartSelect.replaceChildren();
  selectionEndSelect.replaceChildren();
  selectionStartSelect.disabled = true;
  selectionEndSelect.disabled = true;
  clearSelectionButton.disabled = true;
  saveHighlightButton.disabled = true;
  highlightFieldset.disabled = true;
  annotationTagsInput.value = '';
  annotationTagsInput.disabled = true;
  annotationCrossrefsInput.value = '';
  annotationCrossrefsInput.disabled = true;
  annotationLinksInput.value = '';
  annotationLinksInput.disabled = true;
  annotationVisibilitySelect.value = 'Private';
  annotationVisibilitySelect.disabled = true;
  tagFilter.disabled = true;
  populateTagSuggestions(tagSuggestions, []);
  populateTagFilter(tagFilter, [], '');
  noteText.value = '';
  noteText.disabled = true;
  saveNoteButton.disabled = true;
  highlightSummary.textContent = 'Select a verse to highlight it.';
  noteSummary.textContent = 'Select a verse to add a note.';
  noteList.replaceChildren();
  selectionPanel.removeAttribute('data-has-selection');
}

function setHighlightBusy(isBusy) {
  saveHighlightButton.disabled = isBusy || !activeSelection.verseStart;
  highlightFieldset.disabled = isBusy || !activeSelection.verseStart;
  annotationVisibilitySelect.disabled = isBusy || !activeSelection.verseStart;
  highlightForm.toggleAttribute('aria-busy', isBusy);
}

function setNoteBusy(isBusy) {
  noteText.disabled = isBusy || !activeSelection.verseStart;
  saveNoteButton.disabled = isBusy || !activeSelection.verseStart;
  annotationVisibilitySelect.disabled = isBusy || !activeSelection.verseStart;
  noteForm.toggleAttribute('aria-busy', isBusy);
}

function setNoteCardEditing(card, isEditing) {
  card.querySelector('.note-text').hidden = isEditing;
  card.querySelector('.note-edit-form').hidden = !isEditing;
  card.querySelector('.note-card-actions').hidden = isEditing;
  if (isEditing) {
    card.querySelector('textarea')?.focus();
  }
}

function setNoteCardBusy(card, isBusy) {
  card?.querySelectorAll('input, select, textarea, button').forEach((control) => {
    control.disabled = isBusy;
  });
}

function renderAnnotationState() {
  const availableTags = getAvailableTags();
  if (activeTagFilter && !availableTags.some((tag) => tag.toLowerCase() === activeTagFilter.toLowerCase())) {
    activeTagFilter = '';
  }

  populateTagSuggestions(tagSuggestions, availableTags);
  populateTagFilter(tagFilter, availableTags, activeTagFilter);
  tagFilter.disabled = availableTags.length === 0;

  const visibleAnnotations = filterAnnotationsByTag(activeAnnotations, activeTagFilter);
  applyHighlights(readerVerses, visibleAnnotations);
  applyNoteMarkers(readerVerses, visibleAnnotations);
  highlightSummary.textContent = sessionStore.getSession()
    ? formatHighlightSummary(visibleAnnotations)
    : 'Sign in to save and view your private highlights.';
  noteSummary.textContent = sessionStore.getSession()
    ? formatNoteSummary(visibleAnnotations)
    : 'Sign in to save and view your private notes.';
  renderNotesList(noteList, visibleAnnotations, activeReference, activeNotebooks);
}

function renderNotebookList() {
  if (!activeNotebooks.length) {
    notebookList.replaceChildren(element('p', 'empty-state', 'Create a notebook to organize saved notes and highlights.'));
    return;
  }

  notebookList.replaceChildren(...activeNotebooks.map(createNotebookCard));
}

function renderLessonList() {
  if (!activeLessons.length) {
    lessonList.replaceChildren(element('p', 'empty-state', 'Created lessons will appear here.'));
    return;
  }

  lessonList.replaceChildren(...activeLessons.map(createLessonCard));
}

function renderSubscriptionFeed() {
  if (!activeSubscriptionFeed.length) {
    subscriptionFeed.replaceChildren(element('p', 'empty-state', 'Subscribe to public profiles to see their public notes and highlights here.'));
    return;
  }

  subscriptionFeed.replaceChildren(...activeSubscriptionFeed.map(createPublicAnnotationCard));
}

function renderDashboardOverview() {
  const annotationCount = dashboardAnnotations.length;
  const noteCount = dashboardAnnotations.filter((annotation) => Boolean(annotation.notePlainText)).length;
  const publicCount = dashboardAnnotations.filter((annotation) => annotation.visibility === 'Public').length;
  const highlightCount = annotationCount - noteCount;

  dashboardStats.replaceChildren(
    createStat('Annotations', annotationCount),
    createStat('Notes', noteCount),
    createStat('Highlights', highlightCount),
    createStat('Public items', publicCount),
    createStat('Notebooks', activeNotebooks.length),
    createStat('Lessons', activeLessons.length),
  );

  updateContinueStudyLink();
}

function renderRecentAnnotations(isLoading = false) {
  if (isLoading) {
    recentAnnotations.replaceChildren(element('p', 'empty-state', 'Loading recent study activity...'));
    return;
  }

  if (!dashboardAnnotations.length) {
    recentAnnotations.replaceChildren(createNewDashboardEmptyState());
    return;
  }

  recentAnnotations.replaceChildren(...dashboardAnnotations.slice(0, 5).map(createDashboardAnnotationCard));
}

function renderDashboardSearchResults(isLoading = false) {
  const query = activeDashboardSearch.query;

  if (!query) {
    showMessage(dashboardSearchMessage, '', 'info');
    dashboardSearchResults.replaceChildren(element('p', 'empty-state', 'Enter search terms to find saved study material.'));
    return;
  }

  if (isLoading) {
    showMessage(dashboardSearchMessage, 'Loading searchable study data...', 'info');
    dashboardSearchResults.replaceChildren(element('p', 'empty-state', 'Preparing search results...'));
    return;
  }

  const items = buildDashboardSearchItems()
    .filter((item) => activeDashboardSearch.scope === 'all' || item.scope === activeDashboardSearch.scope)
    .filter((item) => matchesDashboardQuery(item, query))
    .slice(0, 12);

  if (!items.length) {
    showMessage(dashboardSearchMessage, `No dashboard results found for "${query}".`, 'info');
    dashboardSearchResults.replaceChildren(element('p', 'empty-state', 'Try a scripture reference, tag, notebook title, lesson title, or note phrase.'));
    return;
  }

  showMessage(dashboardSearchMessage, `${items.length} result${items.length === 1 ? '' : 's'} found for "${query}".`, 'success');
  dashboardSearchResults.replaceChildren(...items.map(createDashboardSearchResultCard));
}

function renderPublicAnnotations(annotations) {
  if (!annotations.length) {
    profileAnnotations.replaceChildren(element('p', 'empty-state', 'No public notes or highlights yet.'));
    return;
  }

  profileAnnotations.replaceChildren(...annotations.map(createPublicAnnotationCard));
}

function renderPublicNotebooks(notebooks) {
  if (!notebooks.length) {
    profileNotebooks.replaceChildren(element('p', 'empty-state', 'No public notebooks yet.'));
    return;
  }

  profileNotebooks.replaceChildren(...notebooks.map(createPublicNotebookCard));
}

function getCurrentAnnotationMetadata() {
  return [
    ...parseTagInput(annotationTagsInput.value),
    ...parseCrossReferenceInput(annotationCrossrefsInput.value),
    ...parseResourceLinkInput(annotationLinksInput.value),
  ];
}

function getAnnotationVisibility() {
  return annotationVisibilitySelect.value || 'Private';
}

function clearAnnotationMetadataInputs() {
  annotationTagsInput.value = '';
  annotationCrossrefsInput.value = '';
  annotationLinksInput.value = '';
  annotationVisibilitySelect.value = 'Private';
}

function mergeActiveTags(tags) {
  const names = parseTagInput(tags);
  const existingNames = activeTags.map((tag) => tag.name || tag);
  activeTags = parseTagInput([...existingNames, ...names]).map((name) => ({ name }));
}

function getAvailableTags() {
  const savedTags = activeTags.map((tag) => tag.name || tag);
  return parseTagInput([...savedTags, ...getTagsFromAnnotations(activeAnnotations)]);
}

function replaceUrlForSelection() {
  if (!activeReference) {
    return;
  }

  window.history.replaceState({}, '', createStudyPath({
    ...activeReference,
    ...activeSelection,
  }));
}

function syncReaderControls(reference) {
  volumeSelect.value = reference.volumeId;
  populateBookSelect(bookSelect, reference.volumeId);
  bookSelect.value = reference.bookId;
  populateChapterSelect(chapterSelect, reference.volumeId, reference.bookId);
  chapterSelect.value = String(reference.chapter);
}

function updateChapterLinks(reference) {
  const previous = getAdjacentReference(reference, -1);
  const next = getAdjacentReference(reference, 1);

  updateChapterLink(previousChapterLink, previous, 'Previous');
  updateChapterLink(nextChapterLink, next, 'Next');
}

function updateChapterLink(link, reference, label) {
  if (!reference) {
    link.hidden = true;
    link.removeAttribute('href');
    return;
  }

  link.hidden = false;
  link.href = createStudyPath(reference);
  link.textContent = `${label}: ${reference.bookTitle} ${reference.chapter}`;
}

function isActiveReference(reference) {
  return activeReference?.volumeId === reference.volumeId
    && activeReference?.bookId === reference.bookId
    && activeReference?.chapter === reference.chapter;
}

function renderCachedUser() {
  document.querySelectorAll('[data-current-user]').forEach((target) => {
    target.textContent = activeUser ? activeUser.displayName : 'Guest';
  });

  document.body.toggleAttribute('data-authenticated', Boolean(activeUser));
}

function showPanel(panel) {
  authPanel.hidden = panel !== 'auth';
  dashboardPanel.hidden = panel !== 'dashboard';
  profilePanel.hidden = panel !== 'profile';
  readerPanel.hidden = panel !== 'reader';
}

function setAuthMode(mode) {
  const isRegister = mode === 'register';
  loginForm.hidden = isRegister;
  registerForm.hidden = !isRegister;
  authPanel.querySelector('[data-auth-title]').textContent = isRegister ? 'Create your account' : 'Sign in to ScriptureCircle';
  authPanel.querySelector('[data-auth-copy]').textContent = isRegister
    ? 'Start saving private scripture notes, highlights, and study notebooks.'
    : 'Continue your scripture study with saved annotations and notebooks.';
  showMessage(authMessage, '', 'info');
}

function navigate(path) {
  window.history.pushState({}, '', path);
  renderRoute();
}

function getReturnTo() {
  const value = new URLSearchParams(window.location.search).get('returnTo');
  return value?.startsWith('/') ? value : null;
}

function setFormBusy(form, isBusy) {
  form.querySelectorAll('input, select, textarea, button').forEach((control) => {
    control.disabled = isBusy;
  });
  form.toggleAttribute('aria-busy', isBusy);
}

function showMessage(target, message, type) {
  target.textContent = message;
  target.dataset.messageType = type;
  target.hidden = !message;
}

function getErrorMessage(error, fallback) {
  return error instanceof Error ? error.message : fallback;
}

function getSessionSummary(session) {
  if (!session.expiresAt) {
    return 'Session is stored for this browser tab and will be rechecked with the API.';
  }

  const expiresAt = new Date(session.expiresAt);
  return `Session expires ${expiresAt.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' })}.`;
}

function createNotebookCard(notebook) {
  const card = document.createElement('article');
  card.className = 'workspace-card';
  card.dataset.notebookId = notebook.id;

  const count = notebook.annotationIds?.length || 0;
  const form = document.createElement('form');
  form.className = 'notebook-edit-form';
  form.hidden = true;
  form.append(
    createLabeledInput(`notebook-title-${notebook.id}`, 'Title', 'notebook-title', notebook.title),
    createNotebookTypeField(`notebook-type-${notebook.id}`, notebook.type),
    createCheckbox('notebook-public', 'Make notebook public', notebook.isPublic),
    createActionRow([
      createActionButton('submit', 'secondary-button', 'Update notebook', 'save-notebook-edit'),
      createActionButton('button', 'text-button', 'Cancel', 'cancel-notebook-edit'),
    ]),
  );

  const actions = createActionRow([
    createActionButton('button', 'text-button', 'Edit', 'edit-notebook'),
    createActionButton('button', 'text-button danger-button', 'Delete', 'delete-notebook'),
  ]);
  actions.classList.add('notebook-card-actions');

  card.append(
    element('h3', '', notebook.title),
    element('p', 'muted', `${notebook.type} · ${notebook.isPublic ? 'Public' : 'Private'} · ${count} item${count === 1 ? '' : 's'}`),
    createShareLink(notebook.isPublic, `/api/notebooks/shared/${notebook.shareSlug}`),
    form,
    actions,
  );
  return card;
}

function createLessonCard(lesson) {
  const card = document.createElement('article');
  card.className = 'workspace-card';

  const reference = lesson.reference;
  const link = document.createElement('a');
  link.className = 'secondary-link';
  link.href = createStudyPath(reference);
  link.dataset.route = '';
  link.textContent = 'Open scripture';

  card.append(
    element('h3', '', lesson.title),
    element('p', 'muted', `${reference.bookTitle} ${reference.chapter} · ${lesson.isPublic ? 'Public' : 'Private'} · ${lesson.authorName}`),
    element('p', '', lesson.summary),
    createShareLink(lesson.isPublic, '/api/lessons'),
    link,
  );
  return card;
}

function createDashboardAnnotationCard(annotation) {
  const card = document.createElement('article');
  card.className = 'workspace-card dashboard-activity-card';

  card.append(
    element('h3', '', annotation.notePlainText ? 'Saved note' : `${annotation.highlightStyle} highlight`),
    element('p', 'muted', `${createAnnotationReferenceLabel(annotation)} · ${annotation.visibility} · ${formatRelativeDate(annotation.updatedAt)}`),
    createAnnotationScriptureLink(annotation),
  );

  if (annotation.notePlainText) {
    card.append(element('p', '', annotation.notePlainText));
  }

  const tags = getVisibleTags(annotation);
  if (tags.length) {
    card.append(createInlineList(tags, 'tag-list', 'tag-chip', 'Tags'));
  }

  const links = [
    ...getCrossReferences(annotation).map((link) => ({ ...link, external: false })),
    ...getResourceLinks(annotation).map((link) => ({ ...link, external: true })),
  ];
  if (links.length) {
    card.append(createProfileLinkList(links));
  }

  return card;
}

function createDashboardSearchResultCard(item) {
  const card = document.createElement('article');
  card.className = 'workspace-card dashboard-search-card';

  const heading = element('h3', '', item.title);
  const meta = element('p', 'muted', `${item.kind} · ${item.meta}`);
  card.append(heading, meta);

  if (item.description) {
    card.append(element('p', '', item.description));
  }

  if (item.tags.length) {
    card.append(createInlineList(item.tags, 'tag-list', 'tag-chip', 'Tags'));
  }

  if (item.href) {
    const link = document.createElement('a');
    link.className = 'secondary-link';
    link.href = item.href;
    link.dataset.route = '';
    link.textContent = item.actionLabel;
    card.append(link);
  }

  return card;
}

function buildDashboardSearchItems() {
  return [
    ...dashboardAnnotations.map((annotation) => {
      const label = createAnnotationReferenceLabel(annotation);
      const tags = getVisibleTags(annotation);
      return {
        scope: 'annotations',
        kind: annotation.notePlainText ? 'Note' : 'Highlight',
        title: annotation.notePlainText ? `Note at ${label}` : `${annotation.highlightStyle} highlight at ${label}`,
        description: annotation.notePlainText || '',
        meta: `${annotation.visibility} · ${formatRelativeDate(annotation.updatedAt)}`,
        tags,
        href: createAnnotationHref(annotation),
        actionLabel: 'Open scripture',
        searchableText: [
          label,
          annotation.notePlainText,
          annotation.highlightStyle,
          annotation.visibility,
          annotation.authorName,
          ...tags,
        ],
      };
    }),
    ...activeNotebooks.map((notebook) => {
      const count = notebook.annotationIds?.length || 0;
      return {
        scope: 'notebooks',
        kind: 'Notebook',
        title: notebook.title,
        description: `${notebook.type} notebook with ${count} saved item${count === 1 ? '' : 's'}.`,
        meta: notebook.isPublic ? 'Public' : 'Private',
        tags: [],
        href: '/dashboard',
        actionLabel: 'View notebook',
        searchableText: [notebook.title, notebook.type, notebook.isPublic ? 'public' : 'private'],
      };
    }),
    ...activeLessons.map((lesson) => ({
      scope: 'lessons',
      kind: 'Lesson',
      title: lesson.title,
      description: lesson.summary,
      meta: `${lesson.reference.bookTitle} ${lesson.reference.chapter} · ${lesson.isPublic ? 'Public' : 'Private'}`,
      tags: [],
      href: createStudyPath(lesson.reference),
      actionLabel: 'Open scripture',
      searchableText: [
        lesson.title,
        lesson.summary,
        lesson.reference.bookTitle,
        String(lesson.reference.chapter),
        lesson.isPublic ? 'public' : 'private',
      ],
    })),
    ...activeSubscriptionFeed.map((annotation) => {
      const label = createAnnotationReferenceLabel(annotation);
      const tags = getVisibleTags(annotation);
      return {
        scope: 'subscriptions',
        kind: annotation.notePlainText ? 'Followed note' : 'Followed highlight',
        title: annotation.notePlainText ? `${annotation.authorName} note at ${label}` : `${annotation.authorName} ${annotation.highlightStyle} highlight at ${label}`,
        description: annotation.notePlainText || '',
        meta: `${annotation.visibility} · ${formatRelativeDate(annotation.updatedAt)}`,
        tags,
        href: createAnnotationHref(annotation),
        actionLabel: 'Open scripture',
        searchableText: [
          label,
          annotation.authorName,
          annotation.notePlainText,
          annotation.highlightStyle,
          annotation.visibility,
          ...tags,
        ],
      };
    }),
  ];
}

function matchesDashboardQuery(item, query) {
  const terms = query.toLowerCase().split(/\s+/).filter(Boolean);
  const haystack = [
    item.title,
    item.description,
    item.meta,
    ...item.searchableText,
  ].join(' ').toLowerCase();

  return terms.every((term) => haystack.includes(term));
}

function createNewDashboardEmptyState() {
  const container = document.createElement('div');
  container.className = 'empty-dashboard-state';

  const link = document.createElement('a');
  link.className = 'secondary-link';
  link.href = '/study/oldtestament/genesis/1';
  link.dataset.route = '';
  link.textContent = 'Start in Genesis 1';

  container.append(
    element('p', 'empty-state', 'Your saved notes and highlights will appear here after you start studying.'),
    link,
  );
  return container;
}

function createStat(label, value) {
  const row = document.createElement('div');
  const term = document.createElement('dt');
  const definition = document.createElement('dd');
  term.textContent = label;
  definition.textContent = String(value);
  row.append(term, definition);
  return row;
}

function createShareLink(isPublic, href) {
  if (!isPublic || !href) {
    return element('p', 'visibility-row', 'Private');
  }

  const row = document.createElement('p');
  row.className = 'visibility-row';
  const badge = document.createElement('span');
  badge.className = 'visibility-badge';
  badge.dataset.visibility = 'Public';
  badge.textContent = 'Public';
  const link = document.createElement('a');
  link.href = href;
  link.target = '_blank';
  link.rel = 'noreferrer';
  link.textContent = 'Share link';
  row.append(badge, document.createTextNode(' '), link);
  return row;
}

function createProfileLink(profileSlug) {
  if (!profileSlug) {
    return element('p', 'muted', 'Public profile link will appear after the session refreshes.');
  }

  const paragraph = document.createElement('p');
  paragraph.className = 'session-note';
  const link = document.createElement('a');
  link.href = `/users/${profileSlug}`;
  link.dataset.route = '';
  link.textContent = 'View public profile';
  paragraph.append(link);
  return paragraph;
}

function createSubscriptionControl(profile) {
  const row = document.createElement('p');
  row.className = 'profile-actions';

  if (!sessionStore.getSession()) {
    const link = document.createElement('a');
    link.className = 'secondary-link';
    link.href = `/login?returnTo=${encodeURIComponent(`/users/${profile.profileSlug}`)}`;
    link.dataset.route = '';
    link.textContent = 'Sign in to subscribe';
    row.append(link);
    return row;
  }

  if (activeUser?.userId === profile.id) {
    row.append(element('span', 'muted', 'This is your public profile.'));
    return row;
  }

  const button = document.createElement('button');
  button.type = 'button';
  button.className = profile.isSubscribed ? 'secondary-button' : 'primary-button';
  button.dataset.subscriptionAction = profile.isSubscribed ? 'unsubscribe' : 'subscribe';
  button.dataset.creatorUserId = profile.id;
  button.dataset.profileSlug = profile.profileSlug;
  button.textContent = profile.isSubscribed ? 'Unsubscribe' : 'Subscribe';
  row.append(button);
  return row;
}

function createPublicAnnotationCard(annotation) {
  const card = document.createElement('article');
  card.className = 'workspace-card';

  card.append(
    element('h3', '', annotation.notePlainText ? 'Public note' : `${annotation.highlightStyle} highlight`),
    element('p', 'muted', `${annotation.authorName} · ${annotation.visibility}`),
    createAnnotationScriptureLink(annotation),
  );

  if (annotation.notePlainText) {
    card.append(element('p', '', annotation.notePlainText));
  }

  const tags = getVisibleTags(annotation);
  if (tags.length) {
    card.append(createInlineList(tags, 'tag-list', 'tag-chip', 'Tags'));
  }

  const links = [
    ...getCrossReferences(annotation).map((link) => ({ ...link, external: false })),
    ...getResourceLinks(annotation).map((link) => ({ ...link, external: true })),
  ];
  if (links.length) {
    card.append(createProfileLinkList(links));
  }

  return card;
}

function createPublicNotebookCard(notebook) {
  const card = document.createElement('article');
  card.className = 'workspace-card';
  const count = notebook.annotationIds?.length || 0;

  card.append(
    element('h3', '', notebook.title),
    element('p', 'muted', `${notebook.type} · Public · ${count} item${count === 1 ? '' : 's'}`),
    createShareLink(true, `/api/notebooks/shared/${notebook.shareSlug}`),
  );
  return card;
}

function createInlineList(items, listClass, itemClass, label) {
  const list = document.createElement('ul');
  list.className = listClass;
  list.setAttribute('aria-label', label);
  list.replaceChildren(...items.map((value) => {
    const item = document.createElement('li');
    item.className = itemClass;
    item.textContent = value;
    return item;
  }));
  return list;
}

function createProfileLinkList(links) {
  const list = document.createElement('ul');
  list.className = 'link-list';
  list.setAttribute('aria-label', 'Cross references and links');
  list.replaceChildren(...links.map((link) => {
    const item = document.createElement('li');
    const anchor = document.createElement('a');
    anchor.href = link.href;
    anchor.textContent = link.label;
    if (link.external) {
      anchor.target = '_blank';
      anchor.rel = 'noreferrer';
    } else {
      anchor.dataset.route = '';
    }
    item.append(anchor);
    return item;
  }));
  return list;
}

function createAnnotationScriptureLink(annotation) {
  const scriptureLink = document.createElement('a');
  scriptureLink.className = 'secondary-link';
  scriptureLink.href = createAnnotationHref(annotation);
  scriptureLink.dataset.route = '';
  scriptureLink.textContent = createAnnotationReferenceLabel(annotation);
  return scriptureLink;
}

function createAnnotationHref(annotation) {
  const reference = annotation.reference;
  const selection = getAnnotationSelection(annotation);
  return createStudyPath({
    ...reference,
    verseStart: selection.verseStart,
    verseEnd: selection.verseEnd,
  });
}

function createAnnotationReferenceLabel(annotation) {
  return createSelectionLabel(annotation.reference, getAnnotationSelection(annotation));
}

function getAnnotationSelection(annotation) {
  const reference = annotation.reference;
  return {
    verseStart: annotation.contentAnchor?.startVerse ?? reference.verseStart,
    verseEnd: annotation.contentAnchor?.endVerse ?? reference.verseEnd,
    startOffset: annotation.contentAnchor?.startOffset,
    endOffset: annotation.contentAnchor?.endOffset,
    selectedText: annotation.contentAnchor?.startOffset !== null
      && annotation.contentAnchor?.startOffset !== undefined
      ? 'phrase'
      : '',
  };
}

function updateContinueStudyLink() {
  const latestAnnotation = dashboardAnnotations[0];
  if (!latestAnnotation) {
    continueStudyCopy.textContent = 'Open the scripture reader and create your first saved note or highlight.';
    continueStudyLink.href = '/study/oldtestament/genesis/1';
    continueStudyLink.textContent = 'Open Genesis 1';
    return;
  }

  const reference = latestAnnotation.reference;
  const selection = getAnnotationSelection(latestAnnotation);
  continueStudyCopy.textContent = `Return to your latest saved item at ${createSelectionLabel(reference, selection)}.`;
  continueStudyLink.href = createStudyPath({
    ...reference,
    verseStart: selection.verseStart,
    verseEnd: selection.verseEnd,
  });
  continueStudyLink.textContent = 'Continue latest study';
}

function formatRelativeDate(value) {
  if (!value) {
    return 'recently';
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return 'recently';
  }

  const diffMs = Date.now() - date.getTime();
  const minuteMs = 60 * 1000;
  const hourMs = 60 * minuteMs;
  const dayMs = 24 * hourMs;

  if (diffMs < minuteMs) {
    return 'just now';
  }

  if (diffMs < hourMs) {
    const minutes = Math.max(1, Math.round(diffMs / minuteMs));
    return `${minutes} minute${minutes === 1 ? '' : 's'} ago`;
  }

  if (diffMs < dayMs) {
    const hours = Math.max(1, Math.round(diffMs / hourMs));
    return `${hours} hour${hours === 1 ? '' : 's'} ago`;
  }

  return date.toLocaleDateString([], { month: 'short', day: 'numeric', year: 'numeric' });
}

function getSettledValue(result, fallback) {
  return result.status === 'fulfilled' ? result.value : fallback;
}

function parseProfilePath(pathname) {
  const match = pathname.match(/^\/users\/([^/]+)$/);
  return match ? decodeURIComponent(match[1]) : null;
}

function createLabeledInput(id, labelText, name, value) {
  const field = document.createElement('div');
  field.className = 'field';
  const label = document.createElement('label');
  label.setAttribute('for', id);
  label.textContent = labelText;
  const input = document.createElement('input');
  input.id = id;
  input.name = name;
  input.type = 'text';
  input.required = true;
  input.value = value;
  field.append(label, input);
  return field;
}

function createNotebookTypeField(id, value) {
  const field = document.createElement('div');
  field.className = 'field';
  const label = document.createElement('label');
  label.setAttribute('for', id);
  label.textContent = 'Type';
  const select = document.createElement('select');
  select.id = id;
  select.name = 'notebook-type';
  ['Personal', 'Lesson', 'Talk', 'Topic'].forEach((type) => {
    const option = document.createElement('option');
    option.value = type;
    option.textContent = type;
    select.append(option);
  });
  select.value = value;
  field.append(label, select);
  return field;
}

function createCheckbox(name, labelText, checked) {
  const label = document.createElement('label');
  label.className = 'check-row';
  const input = document.createElement('input');
  input.type = 'checkbox';
  input.name = name;
  input.checked = checked;
  label.append(input, element('span', '', labelText));
  return label;
}

function createActionRow(buttons) {
  const row = document.createElement('div');
  row.className = 'note-actions';
  row.append(...buttons);
  return row;
}

function createActionButton(type, className, text, action) {
  const button = document.createElement('button');
  button.type = type;
  button.className = className;
  button.textContent = text;
  button.dataset.action = action;
  return button;
}

function setNotebookEditing(card, isEditing) {
  card.querySelector('.notebook-edit-form').hidden = !isEditing;
  card.querySelector('.notebook-card-actions').hidden = isEditing;
  if (isEditing) {
    card.querySelector('input')?.focus();
  }
}

function setNotebookCardBusy(card, isBusy) {
  card?.querySelectorAll('input, select, button').forEach((control) => {
    control.disabled = isBusy;
  });
}

function compareByTitle(left, right) {
  return left.title.localeCompare(right.title);
}

function element(tagName, className, text) {
  const node = document.createElement(tagName);
  if (className) {
    node.className = className;
  }
  node.textContent = text;
  return node;
}
