 function _nullishCoalesce(lhs, rhsFn) { if (lhs != null) { return lhs; } else { return rhsFn(); } } 











export const scriptureCatalog = [
  {
    id: 'oldtestament',
    title: 'Old Testament',
    books: [
      { id: 'genesis', title: 'Genesis', chapters: 50 },
      { id: 'exodus', title: 'Exodus', chapters: 40 },
      { id: 'leviticus', title: 'Leviticus', chapters: 27 },
      { id: 'numbers', title: 'Numbers', chapters: 36 },
      { id: 'deuteronomy', title: 'Deuteronomy', chapters: 34 },
      { id: 'joshua', title: 'Joshua', chapters: 24 },
      { id: 'judges', title: 'Judges', chapters: 21 },
      { id: 'ruth', title: 'Ruth', chapters: 4 },
      { id: '1samuel', title: '1 Samuel', chapters: 31 },
      { id: '2samuel', title: '2 Samuel', chapters: 24 },
      { id: '1kings', title: '1 Kings', chapters: 22 },
      { id: '2kings', title: '2 Kings', chapters: 25 },
      { id: '1chronicles', title: '1 Chronicles', chapters: 29 },
      { id: '2chronicles', title: '2 Chronicles', chapters: 36 },
      { id: 'ezra', title: 'Ezra', chapters: 10 },
      { id: 'nehemiah', title: 'Nehemiah', chapters: 13 },
      { id: 'esther', title: 'Esther', chapters: 10 },
      { id: 'job', title: 'Job', chapters: 42 },
      { id: 'psalms', title: 'Psalms', chapters: 150 },
      { id: 'proverbs', title: 'Proverbs', chapters: 31 },
      { id: 'ecclesiastes', title: 'Ecclesiastes', chapters: 12 },
      { id: 'songofsolomon', title: 'Song of Solomon', chapters: 8 },
      { id: 'isaiah', title: 'Isaiah', chapters: 66 },
      { id: 'jeremiah', title: 'Jeremiah', chapters: 52 },
      { id: 'lamentations', title: 'Lamentations', chapters: 5 },
      { id: 'ezekiel', title: 'Ezekiel', chapters: 48 },
      { id: 'daniel', title: 'Daniel', chapters: 12 },
      { id: 'hosea', title: 'Hosea', chapters: 14 },
      { id: 'joel', title: 'Joel', chapters: 3 },
      { id: 'amos', title: 'Amos', chapters: 9 },
      { id: 'obadiah', title: 'Obadiah', chapters: 1 },
      { id: 'jonah', title: 'Jonah', chapters: 4 },
      { id: 'micah', title: 'Micah', chapters: 7 },
      { id: 'nahum', title: 'Nahum', chapters: 3 },
      { id: 'habakkuk', title: 'Habakkuk', chapters: 3 },
      { id: 'zephaniah', title: 'Zephaniah', chapters: 3 },
      { id: 'haggai', title: 'Haggai', chapters: 2 },
      { id: 'zechariah', title: 'Zechariah', chapters: 14 },
      { id: 'malachi', title: 'Malachi', chapters: 4 },
    ],
  },
  {
    id: 'newtestament',
    title: 'New Testament',
    books: [
      { id: 'matthew', title: 'Matthew', chapters: 28 },
      { id: 'mark', title: 'Mark', chapters: 16 },
      { id: 'luke', title: 'Luke', chapters: 24 },
      { id: 'john', title: 'John', chapters: 21 },
      { id: 'acts', title: 'Acts', chapters: 28 },
      { id: 'romans', title: 'Romans', chapters: 16 },
      { id: '1corinthians', title: '1 Corinthians', chapters: 16 },
      { id: '2corinthians', title: '2 Corinthians', chapters: 13 },
      { id: 'galatians', title: 'Galatians', chapters: 6 },
      { id: 'ephesians', title: 'Ephesians', chapters: 6 },
      { id: 'philippians', title: 'Philippians', chapters: 4 },
      { id: 'colossians', title: 'Colossians', chapters: 4 },
      { id: '1thessalonians', title: '1 Thessalonians', chapters: 5 },
      { id: '2thessalonians', title: '2 Thessalonians', chapters: 3 },
      { id: '1timothy', title: '1 Timothy', chapters: 6 },
      { id: '2timothy', title: '2 Timothy', chapters: 4 },
      { id: 'titus', title: 'Titus', chapters: 3 },
      { id: 'philemon', title: 'Philemon', chapters: 1 },
      { id: 'hebrews', title: 'Hebrews', chapters: 13 },
      { id: 'james', title: 'James', chapters: 5 },
      { id: '1peter', title: '1 Peter', chapters: 5 },
      { id: '2peter', title: '2 Peter', chapters: 3 },
      { id: '1john', title: '1 John', chapters: 5 },
      { id: '2john', title: '2 John', chapters: 1 },
      { id: '3john', title: '3 John', chapters: 1 },
      { id: 'jude', title: 'Jude', chapters: 1 },
      { id: 'revelation', title: 'Revelation', chapters: 22 },
    ],
  },
  {
    id: 'bookofmormon',
    title: 'Book of Mormon',
    books: [
      { id: '1nephi', title: '1 Nephi', chapters: 22 },
      { id: '2nephi', title: '2 Nephi', chapters: 33 },
      { id: 'jacob', title: 'Jacob', chapters: 7 },
      { id: 'enos', title: 'Enos', chapters: 1 },
      { id: 'jarom', title: 'Jarom', chapters: 1 },
      { id: 'omni', title: 'Omni', chapters: 1 },
      { id: 'wordsofmormon', title: 'Words of Mormon', chapters: 1 },
      { id: 'mosiah', title: 'Mosiah', chapters: 29 },
      { id: 'alma', title: 'Alma', chapters: 63 },
      { id: 'helaman', title: 'Helaman', chapters: 16 },
      { id: '3nephi', title: '3 Nephi', chapters: 30 },
      { id: '4nephi', title: '4 Nephi', chapters: 1 },
      { id: 'mormon', title: 'Mormon', chapters: 9 },
      { id: 'ether', title: 'Ether', chapters: 15 },
      { id: 'moroni', title: 'Moroni', chapters: 10 },
    ],
  },
  {
    id: 'doctrineandcovenants',
    title: 'Doctrine and Covenants',
    books: [
      { id: 'doctrineandcovenants', title: 'Doctrine and Covenants', chapters: 138 },
      { id: 'officialdeclaration', title: 'Official Declaration', chapters: 2 },
    ],
  },
  {
    id: 'pearlofgreatprice',
    title: 'Pearl of Great Price',
    books: [
      { id: 'moses', title: 'Moses', chapters: 8 },
      { id: 'abraham', title: 'Abraham', chapters: 5 },
      { id: 'josephsmithmatthew', title: 'Joseph Smith-Matthew', chapters: 1 },
      { id: 'josephsmithhistory', title: 'Joseph Smith-History', chapters: 1 },
      { id: 'articlesoffaith', title: 'Articles of Faith', chapters: 1 },
    ],
  },
];

export function findVolume(volumeId) {
  return _nullishCoalesce(scriptureCatalog.find((volume) => volume.id === volumeId), () => ( scriptureCatalog[2]));
}

export function findBook(volumeId, bookId) {
  const volume = findVolume(volumeId);
  return _nullishCoalesce(volume.books.find((book) => book.id === bookId), () => ( volume.books[0]));
}
