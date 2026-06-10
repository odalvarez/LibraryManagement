const API = '/api/v1';

// STATE
let authorsPage = 1;
let booksPage = 1;
const PAGE_SIZE = 10;

// INIT
document.addEventListener('DOMContentLoaded', () => {
  loadAuthors();
  loadBooks();
});

// ─── TAB NAVIGATION ────────────────────────────────────────────
function showTab(tab) {
  document.getElementById('section-authors').classList.toggle('hidden', tab !== 'authors');
  document.getElementById('section-books').classList.toggle('hidden', tab !== 'books');
  document.getElementById('tab-authors').classList.toggle('active', tab === 'authors');
  document.getElementById('tab-books').classList.toggle('active', tab === 'books');
}

// ─── HTTP HELPERS ───────────────────────────────────────────────
async function apiFetch(path, options = {}) {
  const res = await fetch(API + path, {
    headers: { 'Content-Type': 'application/json' },
    ...options
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ title: 'Error desconocido' }));
    throw new Error(err.title || 'Error en la solicitud');
  }
  if (res.status === 204) return null;
  return res.json();
}

function showError(elementId, message) {
  const el = document.getElementById(elementId);
  el.textContent = message;
  el.classList.remove('hidden');
}

function hideError(elementId) {
  document.getElementById(elementId).classList.add('hidden');
}

// ─── AUTHORS ────────────────────────────────────────────────────
async function loadAuthors() {
  hideError('authors-error');
  try {
    const data = await apiFetch(`/authors?page=${authorsPage}&pageSize=${PAGE_SIZE}`);
    renderAuthors(data);
  } catch (e) {
    showError('authors-error', e.message);
  }
}

function renderAuthors(data) {
  const tbody = document.getElementById('authors-body');
  tbody.innerHTML = '';

  if (!data.items?.length) {
    tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:#999;padding:2rem">Sin registros</td></tr>';
  } else {
    data.items.forEach(a => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${a.id}</td>
        <td>${esc(a.fullName)}</td>
        <td>${formatDate(a.birthDate)}</td>
        <td>${esc(a.city)}</td>
        <td>${esc(a.email)}</td>
        <td>
          <button class="btn-icon" onclick="editAuthor(${a.id})">Editar</button>
          <button class="btn-icon delete" onclick="confirmDelete('author', ${a.id})">Eliminar</button>
        </td>`;
      tbody.appendChild(tr);
    });
  }

  renderPagination('authors-pagination', authorsPage, data.totalPages, p => { authorsPage = p; loadAuthors(); });
}

function openAuthorModal(author = null) {
  hideError('author-form-error');
  document.getElementById('author-id').value = author?.id ?? '';
  document.getElementById('author-fullname').value = author?.fullName ?? '';
  document.getElementById('author-birthdate').value = author ? author.birthDate.substring(0, 10) : '';
  document.getElementById('author-city').value = author?.city ?? '';
  document.getElementById('author-email').value = author?.email ?? '';
  document.getElementById('author-modal-title').textContent = author ? 'Editar autor' : 'Nuevo autor';
  openModal('author-modal');
}

function closeAuthorModal() { closeModal('author-modal'); }

async function editAuthor(id) {
  try {
    const author = await apiFetch(`/authors/${id}`);
    openAuthorModal(author);
  } catch (e) {
    showError('authors-error', e.message);
  }
}

async function submitAuthor(e) {
  e.preventDefault();
  hideError('author-form-error');

  const id = document.getElementById('author-id').value;
  const payload = {
    fullName: document.getElementById('author-fullname').value.trim(),
    birthDate: document.getElementById('author-birthdate').value,
    city: document.getElementById('author-city').value.trim(),
    email: document.getElementById('author-email').value.trim()
  };

  try {
    if (id) {
      await apiFetch(`/authors/${id}`, { method: 'PUT', body: JSON.stringify(payload) });
    } else {
      await apiFetch('/authors', { method: 'POST', body: JSON.stringify(payload) });
    }
    closeAuthorModal();
    loadAuthors();
  } catch (e) {
    showError('author-form-error', e.message);
  }
}

// ─── BOOKS ──────────────────────────────────────────────────────
async function loadBooks() {
  hideError('books-error');
  try {
    const data = await apiFetch(`/books?page=${booksPage}&pageSize=${PAGE_SIZE}`);
    renderBooks(data);
  } catch (e) {
    showError('books-error', e.message);
  }
}

function renderBooks(data) {
  const tbody = document.getElementById('books-body');
  tbody.innerHTML = '';

  if (!data.items?.length) {
    tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:#999;padding:2rem">Sin registros</td></tr>';
  } else {
    data.items.forEach(b => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${b.id}</td>
        <td>${esc(b.title)}</td>
        <td>${b.year}</td>
        <td>${esc(b.genre)}</td>
        <td>${b.pages}</td>
        <td>${esc(b.authorName)}</td>
        <td>
          <button class="btn-icon" onclick="editBook(${b.id})">Editar</button>
          <button class="btn-icon delete" onclick="confirmDelete('book', ${b.id})">Eliminar</button>
        </td>`;
      tbody.appendChild(tr);
    });
  }

  renderPagination('books-pagination', booksPage, data.totalPages, p => { booksPage = p; loadBooks(); });
}

async function openBookModal(book = null) {
  hideError('book-form-error');

  // LOAD AUTHORS FOR DROPDOWN
  try {
    const authors = await apiFetch(`/authors?page=1&pageSize=100`);
    const select = document.getElementById('book-author');
    select.innerHTML = '<option value="">Seleccione un autor</option>';
    authors.items.forEach(a => {
      const opt = document.createElement('option');
      opt.value = a.id;
      opt.textContent = a.fullName;
      if (book?.authorId === a.id) opt.selected = true;
      select.appendChild(opt);
    });
  } catch (e) {
    showError('book-form-error', 'No se pudieron cargar los autores.');
  }

  document.getElementById('book-id').value = book?.id ?? '';
  document.getElementById('book-title').value = book?.title ?? '';
  document.getElementById('book-year').value = book?.year ?? '';
  document.getElementById('book-genre').value = book?.genre ?? '';
  document.getElementById('book-pages').value = book?.pages ?? '';
  document.getElementById('book-modal-title').textContent = book ? 'Editar libro' : 'Nuevo libro';
  openModal('book-modal');
}

function closeBookModal() { closeModal('book-modal'); }

async function editBook(id) {
  try {
    const book = await apiFetch(`/books/${id}`);
    openBookModal(book);
  } catch (e) {
    showError('books-error', e.message);
  }
}

async function submitBook(e) {
  e.preventDefault();
  hideError('book-form-error');

  const id = document.getElementById('book-id').value;
  const payload = {
    title: document.getElementById('book-title').value.trim(),
    year: parseInt(document.getElementById('book-year').value),
    genre: document.getElementById('book-genre').value.trim(),
    pages: parseInt(document.getElementById('book-pages').value),
    authorId: parseInt(document.getElementById('book-author').value)
  };

  try {
    if (id) {
      await apiFetch(`/books/${id}`, { method: 'PUT', body: JSON.stringify(payload) });
    } else {
      await apiFetch('/books', { method: 'POST', body: JSON.stringify(payload) });
    }
    closeBookModal();
    loadBooks();
  } catch (e) {
    showError('book-form-error', e.message);
  }
}

// ─── DELETE CONFIRM ─────────────────────────────────────────────
let pendingDelete = null;

function confirmDelete(type, id) {
  pendingDelete = { type, id };
  const label = type === 'author' ? 'autor' : 'libro';
  document.getElementById('confirm-message').textContent = `¿Desea eliminar este ${label}? Esta acción no se puede deshacer.`;
  document.getElementById('confirm-ok').onclick = executeDelete;
  openModal('confirm-modal');
}

async function executeDelete() {
  if (!pendingDelete) return;
  const { type, id } = pendingDelete;
  pendingDelete = null;
  closeConfirmModal();

  try {
    await apiFetch(`/${type === 'author' ? 'authors' : 'books'}/${id}`, { method: 'DELETE' });
    if (type === 'author') loadAuthors();
    else loadBooks();
  } catch (e) {
    const errorId = type === 'author' ? 'authors-error' : 'books-error';
    showError(errorId, e.message);
  }
}

function closeConfirmModal() { closeModal('confirm-modal'); }

// ─── MODAL HELPERS ───────────────────────────────────────────────
function openModal(id) {
  document.getElementById(id).classList.remove('hidden');
  document.getElementById('overlay').classList.remove('hidden');
}

function closeModal(id) {
  document.getElementById(id).classList.add('hidden');
  document.getElementById('overlay').classList.add('hidden');
}

function closeAllModals() {
  ['author-modal', 'book-modal', 'confirm-modal'].forEach(closeModal);
}

// ─── PAGINATION ─────────────────────────────────────────────────
function renderPagination(containerId, currentPage, totalPages, onPageChange) {
  const container = document.getElementById(containerId);
  container.innerHTML = '';
  if (totalPages <= 1) return;

  const prev = document.createElement('button');
  prev.textContent = '‹ Anterior';
  prev.disabled = currentPage === 1;
  prev.onclick = () => onPageChange(currentPage - 1);

  const info = document.createElement('span');
  info.textContent = `Página ${currentPage} de ${totalPages}`;

  const next = document.createElement('button');
  next.textContent = 'Siguiente ›';
  next.disabled = currentPage === totalPages;
  next.onclick = () => onPageChange(currentPage + 1);

  container.append(prev, info, next);
}

// ─── UTILS ──────────────────────────────────────────────────────
function esc(str) {
  return String(str ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

function formatDate(iso) {
  if (!iso) return '';
  const [y, m, d] = iso.substring(0, 10).split('-');
  return `${d}/${m}/${y}`;
}
