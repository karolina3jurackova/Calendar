const grid = document.getElementById("grid");
const monthLabel = document.getElementById("monthLabel");

const prevBtn = document.getElementById("prevBtn");
const nextBtn = document.getElementById("nextBtn");
const todayBtn = document.getElementById("todayBtn");
const newBtn = document.getElementById("newBtn");

const selectedLabel = document.getElementById("selectedLabel");
const dayEventsEl = document.getElementById("dayEvents");

const overlay = document.getElementById("modalOverlay");
const modalTitle = document.getElementById("modalTitle");
const titleInput = document.getElementById("eventTitle");
const dateInput = document.getElementById("eventDate");
const descInput = document.getElementById("eventDesc");
const saveBtn = document.getElementById("saveBtn");
const cancelBtn = document.getElementById("cancelBtn");
const deleteBtn = document.getElementById("deleteBtn");

const STORAGE_KEY = "simple_calendar_events";

let current = new Date(); current.setHours(0, 0, 0, 0);
let selected = new Date(current);
let editingId = null;

function pad2(n) { return String(n).padStart(2, "0"); }
function isoDate(d) { return `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`; }
function sameDay(a, b) {
    return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

function loadEvents() {
    try { return JSON.parse(localStorage.getItem(STORAGE_KEY) || "[]"); }
    catch { return []; }
}
function saveEvents(arr) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(arr));
}

function eventsForDate(dateStr) {
    return loadEvents().filter(e => e.date === dateStr);
}

function openModal(dateStr) {
    overlay.classList.remove("hidden");
    dateInput.value = dateStr || "";
    titleInput.focus();
}

function closeModal() {
    overlay.classList.add("hidden");
    titleInput.value = "";
    dateInput.value = "";
    descInput.value = "";
    editingId = null;
    deleteBtn.classList.add("hidden");
    modalTitle.textContent = "Nová událost";
}

cancelBtn.addEventListener("click", closeModal);

newBtn.addEventListener("click", () => {
    editingId = null;
    modalTitle.textContent = "Nová událost";
    deleteBtn.classList.add("hidden");
    openModal(isoDate(selected));
});

deleteBtn.addEventListener("click", () => {
    if (!editingId) return;
    const arr = loadEvents().filter(e => e.id !== editingId);
    saveEvents(arr);
    closeModal();
    renderAll();
});

saveBtn.addEventListener("click", () => {
    const title = titleInput.value.trim();
    const date = dateInput.value;

    if (!title) { alert("Zadej název."); return; }
    if (!date) { alert("Vyber datum."); return; }

    const arr = loadEvents();

    if (editingId) {
        const idx = arr.findIndex(e => e.id === editingId);
        if (idx >= 0) {
            arr[idx].title = title;
            arr[idx].date = date;
            arr[idx].desc = descInput.value.trim();
        }
    } else {
        arr.push({
            id: crypto.randomUUID(),
            title,
            date,
            desc: descInput.value.trim()
        });
    }

    saveEvents(arr);
    closeModal();
    renderAll();
});

prevBtn.addEventListener("click", () => {
    current = new Date(current.getFullYear(), current.getMonth() - 1, 1);
    renderAll();
});
nextBtn.addEventListener("click", () => {
    current = new Date(current.getFullYear(), current.getMonth() + 1, 1);
    renderAll();
});
todayBtn.addEventListener("click", () => {
    current = new Date(); current.setHours(0, 0, 0, 0);
    selected = new Date(current);
    renderAll();
});

function renderSidebar() {
    selectedLabel.textContent = selected.toLocaleDateString("cs-CZ", {
        weekday: "long", year: "numeric", month: "long", day: "numeric"
    });

    const dayStr = isoDate(selected);
    const evs = eventsForDate(dayStr);

    dayEventsEl.innerHTML = "";
    if (evs.length === 0) {
        dayEventsEl.textContent = "Žádné události";
        return;
    }

    evs.forEach(ev => {
        const row = document.createElement("div");
        row.className = "row";
        row.innerHTML = `<strong>${ev.title}</strong><br><small>${ev.desc || ""}</small>`;
        row.addEventListener("click", () => {
            // edit
            editingId = ev.id;
            modalTitle.textContent = "Upravit událost";
            deleteBtn.classList.remove("hidden");
            titleInput.value = ev.title;
            dateInput.value = ev.date;
            descInput.value = ev.desc || "";
            openModal(ev.date);
        });
        dayEventsEl.appendChild(row);
    });
}

function renderCalendar() {
    grid.innerHTML = "";

    const y = current.getFullYear();
    const m = current.getMonth();

    monthLabel.textContent = new Date(y, m, 1).toLocaleDateString("cs-CZ", { month: "long", year: "numeric" });

    const first = new Date(y, m, 1);
    const startOffset = (first.getDay() + 6) % 7; // Po=0
    const daysInMonth = new Date(y, m + 1, 0).getDate();

    // prázdné buňky
    for (let i = 0; i < startOffset; i++) {
        const empty = document.createElement("div");
        empty.className = "day outside";
        grid.appendChild(empty);
    }

    for (let d = 1; d <= daysInMonth; d++) {
        const date = new Date(y, m, d);
        const dayStr = isoDate(date);

        const cell = document.createElement("div");
        cell.className = "day";
        cell.innerHTML = `<div class="dayNumber">${d}</div>`;

        // události v buňce (max 2)
        const evs = eventsForDate(dayStr).slice(0, 2);
        evs.forEach(ev => {
            const it = document.createElement("div");
            it.className = "eventItem";
            it.textContent = ev.title;
            it.addEventListener("click", (e) => {
                e.stopPropagation();
                // edit
                editingId = ev.id;
                modalTitle.textContent = "Upravit událost";
                deleteBtn.classList.remove("hidden");
                titleInput.value = ev.title;
                dateInput.value = ev.date;
                descInput.value = ev.desc || "";
                openModal(ev.date);
            });
            cell.appendChild(it);
        });

        // klik = vybrat den
        cell.addEventListener("click", () => {
            selected = new Date(date);
            renderSidebar();
        });

        // dvojklik = rovnou přidat událost
        cell.addEventListener("dblclick", () => {
            selected = new Date(date);
            editingId = null;
            modalTitle.textContent = "Nová událost";
            deleteBtn.classList.add("hidden");
            openModal(dayStr);
            renderSidebar();
        });

        grid.appendChild(cell);
    }
}

function renderAll() {
    // pokud je current uvnitř měsíce, sjednotíme na 1. den
    current = new Date(current.getFullYear(), current.getMonth(), 1);
    renderCalendar();
    renderSidebar();
}

renderAll();
