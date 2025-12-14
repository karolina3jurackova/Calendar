(() => {
    // ========================
    // Safe DOM helpers
    // ========================
    function $(id) {
        return document.getElementById(id);
    }

    function on(el, ev, fn) {
        if (el) el.addEventListener(ev, fn);
    }

    // ========================
    // Grab elements (may be null on non-calendar pages)
    // ========================
    const grid = $("grid");

    // Guard: ak na stránke nie je kalendár, skript nič nerobí
    if (!grid) {
        console.debug("site.js: calendar UI not present -> skipping.");
        return;
    }

    const monthLabel = $("monthLabel");

    const prevBtn = $("prevBtn");
    const nextBtn = $("nextBtn");
    const todayBtn = $("todayBtn");
    const newBtn = $("newBtn");

    const selectedLabel = $("selectedLabel");
    const dayEventsEl = $("dayEvents");

    // (modal prvky môžu existovať, ale už ich nepoužívame na ukladanie do localStorage)
    const overlay = $("modalOverlay");
    const modalTitle = $("modalTitle");
    const titleInput = $("eventTitle");
    const dateInput = $("eventDate");
    const descInput = $("eventDesc");
    const saveBtn = $("saveBtn");
    const cancelBtn = $("cancelBtn");
    const deleteBtn = $("deleteBtn");

    // Ak chýbajú kľúčové prvky, skonči
    if (!monthLabel || !selectedLabel || !dayEventsEl) {
        console.debug("site.js: some calendar elements missing -> skipping.");
        return;
    }

    // ========================
    // Calendar logic (DB-backed)
    // ========================
    let eventsCache = []; // { id, title, description, start, end } (start/end ISO string)

    let current = new Date();
    current.setHours(0, 0, 0, 0);

    let selected = new Date(current);

    function pad2(n) {
        return String(n).padStart(2, "0");
    }

    function isoDate(d) {
        return `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`;
    }

    function monthStart(d) {
        return new Date(d.getFullYear(), d.getMonth(), 1);
    }

    function toLocalDateStr(isoOrDate) {
        // api vracia DateTime -> JSON typicky "2025-12-21T19:00:00"
        // potrebujeme len yyyy-mm-dd (v lokále)
        const dt = new Date(isoOrDate);
        return isoDate(dt);
    }

    function formatTimeRange(ev) {
        const s = new Date(ev.start);
        const e = new Date(ev.end);
        const sTime = `${pad2(s.getHours())}:${pad2(s.getMinutes())}`;
        const eTime = `${pad2(e.getHours())}:${pad2(e.getMinutes())}`;
        return `${sTime}–${eTime}`;
    }

    async function loadEventsFromApi() {
        try {
            const res = await fetch("/api/my-events", {
                headers: { Accept: "application/json" }
            });

            if (!res.ok) throw new Error(`Failed to load events: ${res.status}`);

            const data = await res.json();
            // očakávame: [{ id, title, description, start, end }]
            eventsCache = Array.isArray(data) ? data : [];
        } catch (e) {
            console.error(e);
            eventsCache = [];
        }
    }

    function eventsForDate(dateStr) {
        return eventsCache.filter(ev => toLocalDateStr(ev.start) === dateStr);
    }

    // ========================
    // Buttons / events
    // ========================
    on(prevBtn, "click", async () => {
        current = new Date(current.getFullYear(), current.getMonth() - 1, 1);
        await renderAll();
    });

    on(nextBtn, "click", async () => {
        current = new Date(current.getFullYear(), current.getMonth() + 1, 1);
        await renderAll();
    });

    on(todayBtn, "click", async () => {
        current = new Date();
        current.setHours(0, 0, 0, 0);
        selected = new Date(current);
        await renderAll();
    });

    // „Nová udalosť“ -> redirect na MVC Create (DB)
    on(newBtn, "click", () => {
        const dayStr = isoDate(selected);
        window.location.href = `/Events/Create?date=${encodeURIComponent(dayStr)}`;
    });

    // Ak máš modal v HTML a nechceš ho, aspoň ho nezobrazuj
    on(cancelBtn, "click", () => {
        if (overlay) overlay.classList.add("hidden");
    });
    on(saveBtn, "click", () => {
        // už sa tu neukladá nič do localStorage
        // nechávame prázdne schválne (alebo môžeš tlačidlo odstrániť z HTML)
        alert("Vytváranie udalostí prebieha cez stránku Events → Create.");
    });
    on(deleteBtn, "click", () => {
        alert("Mazanie udalostí prebieha cez Events → Delete.");
    });

    // ========================
    // Rendering
    // ========================
    function renderSidebar() {
        selectedLabel.textContent = selected.toLocaleDateString("cs-CZ", {
            weekday: "long",
            year: "numeric",
            month: "long",
            day: "numeric"
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

            const time = formatTimeRange(ev);
            row.innerHTML = `<strong>${ev.title}</strong> <small class="text-muted">${time}</small><br><small>${ev.description || ""}</small>`;

            // klik -> otvor Details v MVC
            row.addEventListener("click", () => {
                window.location.href = `/Events/Details/${ev.id}`;
            });

            dayEventsEl.appendChild(row);
        });
    }

    function renderCalendar() {
        grid.innerHTML = "";

        const y = current.getFullYear();
        const m = current.getMonth();

        monthLabel.textContent = new Date(y, m, 1).toLocaleDateString("cs-CZ", {
            month: "long",
            year: "numeric"
        });

        // Začiatok mriežky = pondelok v týždni, kde je 1. deň mesiaca
        const firstOfMonth = new Date(y, m, 1);
        const startOffset = (firstOfMonth.getDay() + 6) % 7; // Po=0
        const gridStart = new Date(y, m, 1 - startOffset);

        // 42 buniek (6 týždňov)
        for (let i = 0; i < 42; i++) {
            const date = new Date(gridStart);
            date.setDate(gridStart.getDate() + i);

            const dayStr = isoDate(date);
            const inCurrentMonth = date.getMonth() === m;

            const cell = document.createElement("div");
            cell.className = "day" + (inCurrentMonth ? "" : " outside");

            if (isoDate(date) === isoDate(selected)) {
                cell.classList.add("selected");
            }

            cell.innerHTML = `<div class="dayNumber">${date.getDate()}</div>`;

            // events (max 2)
            const evs = eventsForDate(dayStr).slice(0, 2);
            evs.forEach(ev => {
                const it = document.createElement("div");
                it.className = "eventItem";
                it.textContent = ev.title;

                it.addEventListener("click", (e) => {
                    e.stopPropagation();
                    window.location.href = `/Events/Details/${ev.id}`;
                });

                cell.appendChild(it);
            });

            // klik = vybrat deň
            cell.addEventListener("click", async () => {
                selected = new Date(date);

                if (!inCurrentMonth) {
                    current = monthStart(date);
                    await renderAll();
                    return;
                }

                renderCalendar();
                renderSidebar();
            });

            // dvojklik = Create
            cell.addEventListener("dblclick", () => {
                const d = isoDate(date);
                window.location.href = `/Events/Create?date=${encodeURIComponent(d)}`;
            });

            grid.appendChild(cell);
        }
    }

    async function renderAll() {
        current = monthStart(current);
        await loadEventsFromApi();
        renderCalendar();
        renderSidebar();
    }

    renderAll();
})();