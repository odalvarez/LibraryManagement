const API = '/api/v1';

let authorsPage = 1;
let booksPage = 1;
const PAGE_SIZE = 10;

document.addEventListener('DOMContentLoaded', () => {
  loadAuthors();
  loadBooks();
});

// ─── TAB NAVIGATION ────────────────────────────────────────────
function showTab(tab) {
  document.getElementById('section-authors').classList.toggle('d-none', tab !== 'authors');
  document.getElementById('section-books').classList.toggle('d-none', tab !== 'books');
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
    if (err.errors) {
      const messages = Object.values(err.errors).flat();
      throw new Error(messages[0] || err.title || 'Error en la solicitud');
    }
    throw new Error(err.title || 'Error en la solicitud');
  }
  if (res.status === 204) return null;
  return res.json();
}

function showError(elementId, message) {
  const el = document.getElementById(elementId);
  el.textContent = message;
  el.classList.remove('d-none');
}

function hideError(elementId) {
  document.getElementById(elementId).classList.add('d-none');
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
    tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted py-4">Sin registros</td></tr>';
  } else {
    data.items.forEach(a => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td class="text-muted small">${a.id}</td>
        <td>${esc(a.fullName)}</td>
        <td>${formatDate(a.birthDate)}</td>
        <td>${esc(a.city)}</td>
        <td>${esc(a.email)}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-outline-secondary me-1" onclick="editAuthor(${a.id})">Editar</button>
          <button class="btn btn-sm btn-outline-danger" onclick="confirmDelete('author', ${a.id})">Eliminar</button>
        </td>`;
      tbody.appendChild(tr);
    });
  }

  renderPagination('authors-pagination', authorsPage, data.totalPages, p => { authorsPage = p; loadAuthors(); });
}

function openAuthorModal(author = null) {
  document.getElementById('author-form').classList.remove('was-validated');
  hideError('author-form-error');
  document.getElementById('author-id').value = author?.id ?? '';
  document.getElementById('author-fullname').value = author?.fullName ?? '';
  document.getElementById('author-city').value = author?.city ?? '';
  document.getElementById('author-email').value = author?.email ?? '';
  document.getElementById('author-modal-title').textContent = author ? 'Editar autor' : 'Nuevo autor';

  const birthdateInput = document.getElementById('author-birthdate');
  birthdateInput.value = author ? author.birthDate.substring(0, 10) : '';
  const maxDate = new Date();
  maxDate.setFullYear(maxDate.getFullYear() - 16);
  birthdateInput.max = maxDate.toISOString().slice(0, 10);
  birthdateInput.min = '1900-01-01';

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
  const form = e.target;
  form.classList.add('was-validated');
  if (!form.checkValidity()) return;
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
    form.classList.remove('was-validated');
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
    tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted py-4">Sin registros</td></tr>';
  } else {
    data.items.forEach(b => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td class="text-muted small">${b.id}</td>
        <td>${esc(b.title)}</td>
        <td>${b.year}</td>
        <td>${esc(b.genre)}</td>
        <td>${b.pages}</td>
        <td>${esc(b.authorName)}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-outline-secondary me-1" onclick="editBook(${b.id})">Editar</button>
          <button class="btn btn-sm btn-outline-danger" onclick="confirmDelete('book', ${b.id})">Eliminar</button>
        </td>`;
      tbody.appendChild(tr);
    });
  }

  renderPagination('books-pagination', booksPage, data.totalPages, p => { booksPage = p; loadBooks(); });
}

async function openBookModal(book = null) {
  document.getElementById('book-form').classList.remove('was-validated');
  hideError('book-form-error');

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
  document.getElementById('book-year').max = new Date().getFullYear();
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
  const form = e.target;
  form.classList.add('was-validated');
  if (!form.checkValidity()) return;
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
    form.classList.remove('was-validated');
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
  bootstrap.Modal.getOrCreateInstance(document.getElementById(id)).show();
}

function closeModal(id) {
  bootstrap.Modal.getInstance(document.getElementById(id))?.hide();
}

// ─── PAGINATION ─────────────────────────────────────────────────
function renderPagination(containerId, currentPage, totalPages, onPageChange) {
  const container = document.getElementById(containerId);
  container.innerHTML = '';
  if (totalPages <= 1) return;

  const ul = document.createElement('ul');
  ul.className = 'pagination pagination-sm mb-0';

  const prevLi = document.createElement('li');
  prevLi.className = `page-item${currentPage === 1 ? ' disabled' : ''}`;
  prevLi.innerHTML = `<button class="page-link">‹</button>`;
  if (currentPage > 1) prevLi.querySelector('button').onclick = () => onPageChange(currentPage - 1);

  const infoLi = document.createElement('li');
  infoLi.className = 'page-item disabled';
  infoLi.innerHTML = `<span class="page-link">${currentPage} / ${totalPages}</span>`;

  const nextLi = document.createElement('li');
  nextLi.className = `page-item${currentPage === totalPages ? ' disabled' : ''}`;
  nextLi.innerHTML = `<button class="page-link">›</button>`;
  if (currentPage < totalPages) nextLi.querySelector('button').onclick = () => onPageChange(currentPage + 1);

  ul.append(prevLi, infoLi, nextLi);
  container.appendChild(ul);
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
