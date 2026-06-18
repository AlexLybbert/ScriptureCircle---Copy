import { findBook, findVolume, scriptureCatalog } from '../scriptureCatalog.js';

const NOTE_TAG = 'study-note';
const CROSS_REFERENCE_PREFIX = 'xref:';
const RESOURCE_LINK_PREFIX = 'link:';

export function parseStudyPath(pathname) {
  const [, route, volumeId, bookId, chapterValue] = pathname.split('/');
  if (route !== 'study') {
    return null;
  }

  const fallback = getDefaultReference();
  const volume = findVolume(volumeId || fallback.volumeId);
  const book = findBook(volume.id, bookId || fallback.bookId);
  const chapter = clamp(Number(chapterValue || fallback.chapter), 1, book.chapters);

  return {
    volumeId: volume.id,
    bookId: book.id,
    bookTitle: book.title,
    chapter,
  };
}

export function getDefaultReference() {
  const volume = scriptureCatalog[0];
  const book = volume.books[0];

  return {
    volumeId: volume.id,
    bookId: book.id,
    bookTitle: book.title,
    chapter: 1,
  };
}

export function createStudyPath(reference) {
  const verse = formatVerseQuery(reference.verseStart, reference.verseEnd);
  return `/study/${reference.volumeId}/${reference.bookId}/${reference.chapter}${verse ? `?verse=${verse}` : ''}`;
}

export function parseVerseQuery(search) {
  const value = new URLSearchParams(search).get('verse');
  if (!value) {
    return { verseStart: null, verseEnd: null };
  }

  const match = value.match(/^(\d+)(?:[-–](\d+))?$/);
  if (!match) {
    return { verseStart: null, verseEnd: null };
  }

  const verseStart = Math.max(Number(match[1]), 1);
  const verseEnd = match[2] ? Math.max(Number(match[2]), verseStart) : verseStart;
  return { verseStart, verseEnd };
}

export function normalizeSelection(start, end, maxVerse) {
  if (!start || !Number.isFinite(start)) {
    return { verseStart: null, verseEnd: null };
  }

  const verseStart = clamp(Number(start), 1, maxVerse);
  const verseEnd = clamp(Number(end || start), verseStart, maxVerse);
  return { verseStart, verseEnd };
}

export function normalizeDragSelection(start, end, maxVerse) {
  if (!start || !end || !Number.isFinite(start) || !Number.isFinite(end)) {
    return { verseStart: null, verseEnd: null };
  }

  const lower = Math.min(Number(start), Number(end));
  const upper = Math.max(Number(start), Number(end));
  return {
    verseStart: clamp(lower, 1, maxVerse),
    verseEnd: clamp(upper, 1, maxVerse),
  };
}

export function formatVerseRange(start, end) {
  if (!start) {
    return '';
  }

  return !end || end === start ? String(start) : `${start}-${end}`;
}

export function createSelectionLabel(reference, selection) {
  if (selection.startOffset !== null && selection.startOffset !== undefined) {
    return selection.selectedText
      ? `${reference.bookTitle} ${reference.chapter}:${selection.verseStart} phrase`
      : `${reference.bookTitle} ${reference.chapter}:${selection.verseStart}`;
  }

  const range = formatVerseRange(selection.verseStart, selection.verseEnd);
  return range ? `${reference.bookTitle} ${reference.chapter}:${range}` : 'No verse selected';
}

export function getAdjacentReference(reference, direction) {
  const volumeIndex = scriptureCatalog.findIndex((volume) => volume.id === reference.volumeId);
  const volume = scriptureCatalog[volumeIndex];
  if (!volume) {
    return null;
  }

  const bookIndex = volume.books.findIndex((book) => book.id === reference.bookId);
  const book = volume.books[bookIndex];
  if (!book) {
    return null;
  }

  const nextChapter = reference.chapter + direction;
  if (nextChapter >= 1 && nextChapter <= book.chapters) {
    return {
      ...reference,
      chapter: nextChapter,
    };
  }

  const nextBook = volume.books[bookIndex + direction];
  if (nextBook) {
    return {
      volumeId: volume.id,
      bookId: nextBook.id,
      bookTitle: nextBook.title,
      chapter: direction > 0 ? 1 : nextBook.chapters,
    };
  }

  const nextVolume = scriptureCatalog[volumeIndex + direction];
  if (!nextVolume) {
    return null;
  }

  const edgeBook = direction > 0 ? nextVolume.books[0] : nextVolume.books.at(-1);
  if (!edgeBook) {
    return null;
  }

  return {
    volumeId: nextVolume.id,
    bookId: edgeBook.id,
    bookTitle: edgeBook.title,
    chapter: direction > 0 ? 1 : edgeBook.chapters,
  };
}

export function populateVolumeSelect(selectElement) {
  selectElement.replaceChildren(
    ...scriptureCatalog.map((volume) => createOption(volume.id, volume.title)),
  );
}

export function populateBookSelect(selectElement, volumeId) {
  const volume = findVolume(volumeId);
  selectElement.replaceChildren(
    ...volume.books.map((book) => createOption(book.id, book.title)),
  );
}

export function populateChapterSelect(selectElement, volumeId, bookId) {
  const book = findBook(volumeId, bookId);
  const options = Array.from({ length: book.chapters }, (_, index) => {
    const chapter = index + 1;
    return createOption(String(chapter), String(chapter));
  });
  selectElement.replaceChildren(...options);
}

export function renderVerses(container, chapter) {
  if (!chapter?.verses?.length) {
    container.replaceChildren(createParagraph('No verses were returned for this chapter.', 'empty-state'));
    return;
  }

  const list = document.createElement('ol');
  list.className = 'verse-list';

  chapter.verses.forEach((verse) => {
    const item = document.createElement('li');
    item.className = 'verse-item';
    item.dataset.verse = String(verse.number);
    item.tabIndex = 0;
    item.role = 'button';
    item.setAttribute('aria-label', `Select verse ${verse.number}`);

    const number = document.createElement('span');
    number.className = 'verse-number';
    number.textContent = String(verse.number);

    const text = document.createElement('span');
    text.className = 'verse-text';
    text.dataset.plainText = verse.text;
    text.textContent = verse.text;

    item.append(number, text);
    list.append(item);
  });

  container.replaceChildren(list);
}

export function applyVerseSelection(container, selection) {
  container.querySelectorAll('.verse-item').forEach((item) => {
    const verseNumber = Number(item.dataset.verse);
    const selected = Boolean(
      selection.verseStart
      && verseNumber >= selection.verseStart
      && verseNumber <= (selection.verseEnd || selection.verseStart),
    );

    item.classList.toggle('is-selected', selected);
    item.setAttribute('aria-pressed', selected ? 'true' : 'false');
  });
}

export function applyHighlights(container, annotations) {
  container.querySelectorAll('.verse-item').forEach((item) => {
    item.dataset.highlightStyle = '';
    item.removeAttribute('data-highlight-style');
    item.classList.remove('has-highlight');
    const verseText = item.querySelector('.verse-text');
    if (verseText?.dataset.plainText) {
      verseText.textContent = verseText.dataset.plainText;
    }
  });

  const phraseAnnotationsByVerse = new Map();

  annotations.forEach((annotation) => {
    if (isStudyNote(annotation)) {
      return;
    }

    const start = annotation.contentAnchor?.startVerse ?? annotation.reference?.verseStart;
    const end = annotation.contentAnchor?.endVerse ?? annotation.reference?.verseEnd ?? start;
    if (!start) {
      return;
    }

    if (isPhraseAnnotation(annotation)) {
      const list = phraseAnnotationsByVerse.get(start) || [];
      list.push(annotation);
      phraseAnnotationsByVerse.set(start, list);
      return;
    }

    for (let verseNumber = start; verseNumber <= end; verseNumber += 1) {
      const item = container.querySelector(`[data-verse="${verseNumber}"]`);
      if (!item) {
        continue;
      }

      item.classList.add('has-highlight');
      item.dataset.highlightStyle = annotation.highlightStyle || 'Yellow';
    }
  });

  phraseAnnotationsByVerse.forEach((verseAnnotations, verseNumber) => {
    const item = container.querySelector(`[data-verse="${verseNumber}"]`);
    const verseText = item?.querySelector('.verse-text');
    if (verseText) {
      renderPhraseHighlights(verseText, verseAnnotations);
    }
  });
}

export function applyNoteMarkers(container, annotations) {
  container.querySelectorAll('.verse-item').forEach((item) => {
    item.classList.remove('has-note');
    item.removeAttribute('data-note-count');
  });

  getStudyNotes(annotations).forEach((annotation) => {
    const start = annotation.contentAnchor?.startVerse ?? annotation.reference?.verseStart;
    const end = annotation.contentAnchor?.endVerse ?? annotation.reference?.verseEnd ?? start;
    if (!start) {
      return;
    }

    for (let verseNumber = start; verseNumber <= end; verseNumber += 1) {
      const item = container.querySelector(`[data-verse="${verseNumber}"]`);
      if (!item) {
        continue;
      }

      const count = Number(item.dataset.noteCount || 0) + 1;
      item.classList.add('has-note');
      item.dataset.noteCount = String(count);
    }
  });
}

export function createHighlightAnnotationPayload(reference, selection, highlightStyle, tags = [], visibility = 'Private') {
  return createAnnotationPayload(reference, selection, {
    highlightStyle,
    visibility,
    notePlainText: '',
    tags,
  });
}

export function createNoteAnnotationPayload(reference, selection, notePlainText, tags = [], visibility = 'Private') {
  return createAnnotationPayload(reference, selection, {
    highlightStyle: 'Yellow',
    visibility,
    notePlainText,
    tags: [...tags, NOTE_TAG],
  });
}

function createAnnotationPayload(reference, selection, { highlightStyle, visibility, notePlainText, tags }) {
  const verseStart = selection.verseStart;
  const verseEnd = selection.verseEnd || selection.verseStart;
  const isPhrase = selection.startOffset !== null
    && selection.startOffset !== undefined
    && selection.endOffset !== null
    && selection.endOffset !== undefined;

  return {
    reference: {
      volumeId: reference.volumeId,
      bookId: reference.bookId,
      bookTitle: reference.bookTitle,
      chapter: reference.chapter,
      verseStart,
      verseEnd,
    },
    highlightStyle,
    visibility,
    notePlainText,
    tags,
    contentAnchor: {
      anchorType: isPhrase ? 'Phrase' : 'VerseRange',
      startVerse: verseStart,
      endVerse: isPhrase ? verseStart : verseEnd,
      startOffset: isPhrase ? selection.startOffset : null,
      endOffset: isPhrase ? selection.endOffset : null,
      paragraphId: null,
    },
  };
}

export function getStudyNotes(annotations) {
  return annotations.filter(isStudyNote);
}

export function getHighlightAnnotations(annotations) {
  return annotations.filter((annotation) => !isStudyNote(annotation));
}

export function formatNoteSummary(annotations) {
  const count = getStudyNotes(annotations).length;
  if (count === 0) {
    return 'No notes saved for this chapter yet.';
  }

  return `${count} saved note${count === 1 ? '' : 's'} in this chapter.`;
}

export function formatHighlightSummary(annotations) {
  const count = getHighlightAnnotations(annotations).length;
  if (count === 0) {
    return 'No highlights saved for this chapter yet.';
  }

  return `${count} saved highlight${count === 1 ? '' : 's'} in this chapter.`;
}

export function renderNotesList(container, annotations, reference, notebooks = []) {
  const notes = getStudyNotes(annotations);
  if (notes.length === 0) {
    container.replaceChildren(createParagraph('Saved notes will appear here.', 'empty-state'));
    return;
  }

  const fragment = document.createDocumentFragment();
  notes.forEach((note) => {
    fragment.append(createNoteCard(note, reference, notebooks));
  });
  container.replaceChildren(fragment);
}

export function parseTagInput(value, extraTags = []) {
  const names = String(value || '')
    .split(',')
    .map((tag) => normalizeTagName(tag))
    .filter((tag) => isVisibleTag(tag));

  extraTags.forEach((tag) => {
    const normalized = normalizeTagName(tag);
    if (normalized) {
      names.push(normalized);
    }
  });

  return uniqueTags(names);
}

export function getVisibleTags(annotation) {
  return uniqueTags((annotation.tags || [])
    .map((tag) => normalizeTagName(tag))
    .filter((tag) => isVisibleTag(tag)));
}

export function getTagsFromAnnotations(annotations) {
  return uniqueTags(annotations.flatMap(getVisibleTags))
    .sort((left, right) => left.localeCompare(right));
}

export function filterAnnotationsByTag(annotations, tagName) {
  const normalized = normalizeTagName(tagName).toLowerCase();
  if (!normalized) {
    return annotations;
  }

  return annotations.filter((annotation) => (
    getVisibleTags(annotation).some((tag) => tag.toLowerCase() === normalized)
  ));
}

export function populateTagSuggestions(datalist, tags) {
  datalist.replaceChildren(
    ...uniqueTags(tags.map((tag) => tag.name || tag))
      .sort((left, right) => left.localeCompare(right))
      .map((tag) => createOption(tag, tag)),
  );
}

export function populateTagFilter(selectElement, tags, selectedTag = '') {
  const options = [
    createOption('', 'All tags'),
    ...uniqueTags(tags.map((tag) => tag.name || tag))
      .sort((left, right) => left.localeCompare(right))
      .map((tag) => createOption(tag, tag)),
  ];
  selectElement.replaceChildren(...options);
  selectElement.value = selectedTag;
  if (selectElement.value !== selectedTag) {
    selectElement.value = '';
  }
}

export function parseCrossReferenceInput(value) {
  return uniqueTags(String(value || '')
    .split(/[,;]+/)
    .map((reference) => createCrossReferenceTag(reference.trim()))
    .filter(Boolean));
}

export function parseResourceLinkInput(value) {
  return uniqueTags(String(value || '')
    .split(/[\n,]+/)
    .map((link) => createResourceLinkTag(link.trim()))
    .filter(Boolean));
}

export function getCrossReferences(annotation) {
  return (annotation.tags || [])
    .filter((tag) => tag.startsWith(CROSS_REFERENCE_PREFIX))
    .map(parseCrossReferenceTag)
    .filter(Boolean);
}

export function getResourceLinks(annotation) {
  return (annotation.tags || [])
    .filter((tag) => tag.startsWith(RESOURCE_LINK_PREFIX))
    .map(parseResourceLinkTag)
    .filter(Boolean);
}

export function getPhraseSelectionFromWindowSelection(selection, container) {
  if (!selection || selection.rangeCount === 0 || selection.isCollapsed) {
    return null;
  }

  const range = selection.getRangeAt(0);
  const startText = getVerseTextElement(range.startContainer);
  const endText = getVerseTextElement(range.endContainer);
  if (!startText || startText !== endText || !container.contains(startText)) {
    return null;
  }

  const verseItem = startText.closest('[data-verse]');
  const verseNumber = Number(verseItem?.dataset.verse);
  const startOffset = getTextOffset(startText, range.startContainer, range.startOffset);
  const endOffset = getTextOffset(startText, range.endContainer, range.endOffset);
  const selectedText = selection.toString().trim();
  if (!verseNumber || startOffset === null || endOffset === null || startOffset === endOffset || !selectedText) {
    return null;
  }

  return {
    verseStart: verseNumber,
    verseEnd: verseNumber,
    startOffset: Math.min(startOffset, endOffset),
    endOffset: Math.max(startOffset, endOffset),
    selectedText,
  };
}

export function populateVerseRangeControls(startSelect, endSelect, chapter, selection) {
  const verses = chapter?.verses || [];
  const options = verses.map((verse) => createOption(String(verse.number), String(verse.number)));
  startSelect.replaceChildren(...options);
  endSelect.replaceChildren(...options.map((option) => option.cloneNode(true)));

  const fallback = verses[0]?.number || 1;
  startSelect.value = String(selection.verseStart || fallback);
  endSelect.value = String(selection.verseEnd || selection.verseStart || fallback);
}

export function getVerseCount(chapter) {
  return chapter?.verses?.at(-1)?.number || 1;
}

export function updateReaderTitle(titleElement, chapter) {
  titleElement.textContent = chapter
    ? `${chapter.bookTitle} ${chapter.chapter}`
    : 'Scripture Reader';
}

function createOption(value, label) {
  const option = document.createElement('option');
  option.value = value;
  option.textContent = label;
  return option;
}

function createParagraph(text, className) {
  const paragraph = document.createElement('p');
  paragraph.className = className;
  paragraph.textContent = text;
  return paragraph;
}

function isPhraseAnnotation(annotation) {
  const anchor = annotation.contentAnchor;
  return anchor?.anchorType === 'Phrase'
    || (anchor?.startOffset !== null
      && anchor?.startOffset !== undefined
      && anchor?.endOffset !== null
      && anchor?.endOffset !== undefined);
}

function isStudyNote(annotation) {
  return Boolean(annotation.notePlainText?.trim())
    && Array.isArray(annotation.tags)
    && annotation.tags.some((tag) => tag.toLowerCase() === NOTE_TAG);
}

function createNoteCard(note, reference, notebooks) {
  const article = document.createElement('article');
  article.className = 'note-card';
  article.dataset.noteId = note.id;

  const heading = document.createElement('h4');
  heading.textContent = createSelectionLabel(reference, {
    verseStart: note.contentAnchor?.startVerse ?? note.reference?.verseStart,
    verseEnd: note.contentAnchor?.endVerse ?? note.reference?.verseEnd,
    startOffset: note.contentAnchor?.startOffset,
    endOffset: note.contentAnchor?.endOffset,
    selectedText: note.contentAnchor?.startOffset !== null
      && note.contentAnchor?.startOffset !== undefined
      ? 'phrase'
      : '',
  });

  const text = document.createElement('p');
  text.className = 'note-text';
  text.textContent = note.notePlainText;

  const visibility = createVisibilityRow(note);
  const tags = createTagList(getVisibleTags(note));
  const links = createLinkList([
    ...getCrossReferences(note).map((referenceLink) => ({
      label: referenceLink.label,
      href: referenceLink.href,
      external: false,
    })),
    ...getResourceLinks(note).map((resourceLink) => ({
      label: resourceLink.label,
      href: resourceLink.href,
      external: true,
    })),
  ]);

  const form = document.createElement('form');
  form.className = 'note-edit-form';
  form.hidden = true;

  const label = document.createElement('label');
  label.className = 'visually-hidden';
  label.setAttribute('for', `note-edit-${note.id}`);
  label.textContent = 'Edit note';

  const textarea = document.createElement('textarea');
  textarea.id = `note-edit-${note.id}`;
  textarea.name = 'note-text';
  textarea.rows = 3;
  textarea.maxLength = 1000;
  textarea.value = note.notePlainText || '';

  const tagLabel = document.createElement('label');
  tagLabel.className = 'visually-hidden';
  tagLabel.setAttribute('for', `note-tags-${note.id}`);
  tagLabel.textContent = 'Edit note tags';

  const tagInput = document.createElement('input');
  tagInput.id = `note-tags-${note.id}`;
  tagInput.name = 'annotation-tags';
  tagInput.type = 'text';
  tagInput.value = getVisibleTags(note).join(', ');

  const visibilityLabel = document.createElement('label');
  visibilityLabel.className = 'visually-hidden';
  visibilityLabel.setAttribute('for', `note-visibility-${note.id}`);
  visibilityLabel.textContent = 'Edit note visibility';

  const visibilitySelect = document.createElement('select');
  visibilitySelect.id = `note-visibility-${note.id}`;
  visibilitySelect.name = 'annotation-visibility';
  ['Private', 'Unlisted', 'Public'].forEach((value) => {
    const option = createOption(value, value === 'Unlisted' ? 'Unlisted share link' : value);
    visibilitySelect.append(option);
  });
  visibilitySelect.value = note.visibility || 'Private';

  const referenceLabel = document.createElement('label');
  referenceLabel.className = 'visually-hidden';
  referenceLabel.setAttribute('for', `note-crossrefs-${note.id}`);
  referenceLabel.textContent = 'Edit cross references';

  const referenceInput = document.createElement('input');
  referenceInput.id = `note-crossrefs-${note.id}`;
  referenceInput.name = 'annotation-crossrefs';
  referenceInput.type = 'text';
  referenceInput.value = getCrossReferences(note).map((link) => link.label).join(', ');

  const resourceLabel = document.createElement('label');
  resourceLabel.className = 'visually-hidden';
  resourceLabel.setAttribute('for', `note-links-${note.id}`);
  resourceLabel.textContent = 'Edit resource links';

  const resourceInput = document.createElement('input');
  resourceInput.id = `note-links-${note.id}`;
  resourceInput.name = 'annotation-links';
  resourceInput.type = 'text';
  resourceInput.value = getResourceLinks(note).map((link) => `${link.label} | ${link.href}`).join(', ');

  const editActions = document.createElement('div');
  editActions.className = 'note-actions';
  editActions.append(
    createButton('submit', 'secondary-button', 'Update note', { action: 'save-note-edit' }),
    createButton('button', 'text-button', 'Cancel', { action: 'cancel-note-edit' }),
  );
  form.append(
    label,
    textarea,
    tagLabel,
    tagInput,
    referenceLabel,
    referenceInput,
    resourceLabel,
    resourceInput,
    visibilityLabel,
    visibilitySelect,
    editActions,
  );

  const actions = document.createElement('div');
  actions.className = 'note-actions note-card-actions';
  actions.append(
    createButton('button', 'text-button', 'Edit', { action: 'edit-note' }),
    createButton('button', 'text-button danger-button', 'Delete', { action: 'delete-note' }),
  );

  article.append(heading, visibility, text, tags, links, createNotebookPicker(note, notebooks), form, actions);
  return article;
}

function createVisibilityRow(annotation) {
  const row = document.createElement('p');
  row.className = 'visibility-row';
  const visibility = annotation.visibility || 'Private';
  row.append(createVisibilityBadge(visibility));

  if (visibility !== 'Private' && annotation.shareSlug) {
    row.append(document.createTextNode(' '));
    const link = document.createElement('a');
    link.href = `/api/annotations/shared/${annotation.shareSlug}`;
    link.target = '_blank';
    link.rel = 'noreferrer';
    link.textContent = 'Share link';
    row.append(link);
  }

  return row;
}

function createVisibilityBadge(visibility) {
  const badge = document.createElement('span');
  badge.className = 'visibility-badge';
  badge.dataset.visibility = visibility;
  badge.textContent = visibility === 'Unlisted' ? 'Unlisted' : visibility;
  return badge;
}

function createNotebookPicker(note, notebooks) {
  const form = document.createElement('form');
  form.className = 'notebook-picker';
  form.dataset.action = 'add-note-to-notebook';

  if (!notebooks.length) {
    form.hidden = true;
    return form;
  }

  const label = document.createElement('label');
  label.className = 'visually-hidden';
  label.setAttribute('for', `note-notebook-${note.id}`);
  label.textContent = 'Notebook';

  const select = document.createElement('select');
  select.id = `note-notebook-${note.id}`;
  select.name = 'notebook-id';
  select.append(createOption('', 'Choose notebook'));
  notebooks.forEach((notebook) => {
    const option = createOption(notebook.id, notebook.title);
    option.disabled = notebook.annotationIds?.includes(note.id);
    option.textContent = option.disabled ? `${notebook.title} (added)` : notebook.title;
    select.append(option);
  });

  form.append(
    label,
    select,
    createButton('submit', 'secondary-button', 'Add to notebook', { action: 'submit-note-notebook' }),
  );
  return form;
}

function createTagList(tags) {
  const list = document.createElement('ul');
  list.className = 'tag-list';
  list.setAttribute('aria-label', 'Tags');

  if (tags.length === 0) {
    list.hidden = true;
    return list;
  }

  list.replaceChildren(...tags.map((tag) => {
    const item = document.createElement('li');
    item.className = 'tag-chip';
    item.textContent = tag;
    return item;
  }));
  return list;
}

function createLinkList(links) {
  const list = document.createElement('ul');
  list.className = 'link-list';
  list.setAttribute('aria-label', 'Cross references and links');

  if (links.length === 0) {
    list.hidden = true;
    return list;
  }

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

function createButton(type, className, text, data = {}) {
  const button = document.createElement('button');
  button.type = type;
  button.className = className;
  button.textContent = text;
  Object.entries(data).forEach(([key, value]) => {
    button.dataset[key] = value;
  });
  return button;
}

function normalizeTagName(tag) {
  return String(tag || '')
    .trim()
    .replace(/^#+/, '')
    .replace(/\s+/g, ' ')
    .slice(0, 40);
}

function isVisibleTag(tag) {
  return Boolean(tag)
    && tag.toLowerCase() !== NOTE_TAG
    && !tag.startsWith(CROSS_REFERENCE_PREFIX)
    && !tag.startsWith(RESOURCE_LINK_PREFIX);
}

function createCrossReferenceTag(value) {
  const parsed = parseReferenceText(value);
  if (!parsed) {
    return null;
  }

  const verse = parsed.verseStart
    ? `/${formatVerseRange(parsed.verseStart, parsed.verseEnd)}`
    : '';
  return `${CROSS_REFERENCE_PREFIX}${parsed.volumeId}/${parsed.bookId}/${parsed.chapter}${verse}`;
}

function parseCrossReferenceTag(tag) {
  const value = tag.slice(CROSS_REFERENCE_PREFIX.length);
  const [volumeId, bookId, chapterValue, verseValue] = value.split('/');
  const volume = findVolume(volumeId);
  const book = findBook(volume.id, bookId);
  const chapter = Number(chapterValue);
  if (!book || !chapter) {
    return null;
  }

  const verse = parseStoredVerseRange(verseValue);
  const reference = {
    volumeId,
    bookId,
    bookTitle: book.title,
    chapter,
    verseStart: verse.verseStart,
    verseEnd: verse.verseEnd,
  };

  return {
    label: createSelectionLabel(reference, reference),
    href: createStudyPath(reference),
  };
}

function createResourceLinkTag(value) {
  if (!value) {
    return null;
  }

  const [rawLabel, rawUrl] = value.includes('|') ? value.split('|') : ['', value];
  const url = normalizeUrl(rawUrl || rawLabel);
  if (!url) {
    return null;
  }

  const label = normalizeLinkLabel(rawUrl ? rawLabel : new URL(url).hostname);
  return `${RESOURCE_LINK_PREFIX}${label}|${url}`.slice(0, 80);
}

function parseResourceLinkTag(tag) {
  const value = tag.slice(RESOURCE_LINK_PREFIX.length);
  const separatorIndex = value.indexOf('|');
  if (separatorIndex < 0) {
    return null;
  }

  const label = value.slice(0, separatorIndex);
  const href = normalizeUrl(value.slice(separatorIndex + 1));
  if (!label || !href) {
    return null;
  }

  return { label, href };
}

function parseReferenceText(value) {
  const match = value.match(/^(.+?)\s+(\d+)(?::(\d+)(?:[-–](\d+))?)?$/);
  if (!match) {
    return null;
  }

  const book = findBookByTitle(match[1]);
  if (!book) {
    return null;
  }

  const chapter = Number(match[2]);
  const verseStart = match[3] ? Number(match[3]) : null;
  const verseEnd = match[4] ? Number(match[4]) : verseStart;
  if (!chapter || chapter > book.book.chapters) {
    return null;
  }

  return {
    volumeId: book.volume.id,
    bookId: book.book.id,
    bookTitle: book.book.title,
    chapter,
    verseStart,
    verseEnd,
  };
}

function findBookByTitle(title) {
  const normalized = normalizeReferenceName(title);
  for (const volume of scriptureCatalog) {
    const book = volume.books.find((candidate) => normalizeReferenceName(candidate.title) === normalized);
    if (book) {
      return { volume, book };
    }
  }

  return null;
}

function parseStoredVerseRange(value) {
  if (!value) {
    return { verseStart: null, verseEnd: null };
  }

  const [start, end] = value.split('-').map(Number);
  return {
    verseStart: start || null,
    verseEnd: end || start || null,
  };
}

function normalizeReferenceName(value) {
  return String(value || '').toLowerCase().replace(/[^a-z0-9]/g, '');
}

function normalizeUrl(value) {
  try {
    const url = new URL(String(value || '').trim());
    return url.protocol === 'https:' || url.protocol === 'http:' ? url.href : null;
  } catch {
    return null;
  }
}

function normalizeLinkLabel(value) {
  return String(value || 'Resource').trim().replace(/\s+/g, ' ').slice(0, 18) || 'Resource';
}

function uniqueTags(tags) {
  const seen = new Set();
  const unique = [];

  tags.forEach((tag) => {
    const key = tag.toLowerCase();
    if (!key || seen.has(key)) {
      return;
    }

    seen.add(key);
    unique.push(tag);
  });

  return unique;
}

function renderPhraseHighlights(verseText, annotations) {
  const plainText = verseText.dataset.plainText || verseText.textContent || '';
  const ranges = annotations
    .map((annotation) => ({
      start: clamp(Number(annotation.contentAnchor?.startOffset), 0, plainText.length),
      end: clamp(Number(annotation.contentAnchor?.endOffset), 0, plainText.length),
      style: annotation.highlightStyle || 'Yellow',
    }))
    .filter((range) => range.end > range.start)
    .sort((left, right) => left.start - right.start || left.end - right.end);

  const fragment = document.createDocumentFragment();
  let cursor = 0;

  ranges.forEach((range) => {
    if (range.start < cursor) {
      return;
    }

    if (range.start > cursor) {
      fragment.append(document.createTextNode(plainText.slice(cursor, range.start)));
    }

    const span = document.createElement('span');
    span.className = 'phrase-highlight';
    span.dataset.highlightStyle = range.style;
    span.textContent = plainText.slice(range.start, range.end);
    fragment.append(span);
    cursor = range.end;
  });

  if (cursor < plainText.length) {
    fragment.append(document.createTextNode(plainText.slice(cursor)));
  }

  verseText.replaceChildren(fragment);
}

function getVerseTextElement(node) {
  const element = node.nodeType === Node.ELEMENT_NODE ? node : node.parentElement;
  return element?.closest?.('.verse-text') || null;
}

function getTextOffset(root, targetNode, targetOffset) {
  const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
  let offset = 0;
  let node = walker.nextNode();

  while (node) {
    if (node === targetNode) {
      return offset + targetOffset;
    }

    offset += node.textContent.length;
    node = walker.nextNode();
  }

  return targetNode === root ? targetOffset : null;
}

function formatVerseQuery(start, end) {
  if (!start) {
    return '';
  }

  return !end || end === start ? String(start) : `${start}-${end}`;
}

function clamp(value, min, max) {
  if (!Number.isFinite(value)) {
    return min;
  }

  return Math.min(Math.max(value, min), max);
}
