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
    // Notification helpers (1F)
    // ========================
    async function ensureNotificationPermission() {
        if (!("Notification" in window)) {
            console.warn("Browser nepodporuje notifikácie");
            return false;
        }

        if (Notification.permission === "granted") return true;

        if (Notification.permission !== "denied") {
            const perm = await Notification.requestPermission();
            return perm === "granted";
        }

        return false;
    }

    function showNotification(title, body) {
        if (Notification.permission !== "granted") return;

        new Notification(title, {
            body,
            icon: "/favicon.ico"
        });
    }

    // ========================
    // Grab elements
    // ========================
    const grid = $("grid");
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

    if (!monthLabel || !selectedLabel || !dayEventsEl) {
        console.debug("site.js: some calendar elements missing -> skipping.");
        return;
    }

    // ========================
    // Calendar state
    // ========================
    let eventsCache = [];
    let remindersScheduled = false;

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

    function toLocalDateStr(iso) {
        return isoDate(new Date(iso));
    }

    function formatTimeRange(ev) {
        const s = new Date(ev.start);
        const e = new Date(ev.end);
        return `${pad2(s.getHours())}:${pad2(s.getMinutes())}–${pad2(e.getHours())}:${pad2(e.getMinutes())}`;
    }

    // ========================
    // API
    // ========================
    async function loadEventsFromApi() {
        const res = await fetch("/api/my-events", { headers: { Accept: "application/json" } });
        eventsCache = res.ok ? await res.json() : [];
    }

    async function loadRemindersAndSchedule() {
        if (remindersScheduled) return;

        const allowed = await ensureNotificationPermission();
        if (!allowed) return;

        try {
            const res = await fetch("/api/my-reminders", {
                headers: { Accept: "application/json" }
            });

            if (!res.ok) return;

            const reminders = await res.json();
            const now = Date.now();

            reminders.forEach(r => {
                const fireAt = new Date(r.fireAtUtc).getTime();
                const delay = fireAt - now;

                if (delay <= 0) return;

                setTimeout(() => {
                    showNotification(r.eventTitle, r.message ?? "Pripomienka udalosti");

                    // ✅ opakovanie (ak príde z API)
                    const everyMin = Number(r.repeatEveryMinutes || 0);
                    let left = Number(r.repeatCountLeft || 0);

                    if (everyMin > 0 && left > 0) {
                        const intervalMs = everyMin * 60_000;

                        const tick = () => {
                            if (left <= 0) return;
                            left -= 1;
                            showNotification(r.eventTitle, "Opakovaná pripomienka");
                            if (left > 0) setTimeout(tick, intervalMs);
                        };

                        setTimeout(tick, intervalMs);
                    }
                }, delay);
            });

            remindersScheduled = true;
        } catch (e) {
            console.error("Failed to schedule reminders", e);
        }
    }

    function eventsForDate(dateStr) {
        return eventsCache.filter(ev => toLocalDateStr(ev.start) === dateStr);
    }

    // ========================
    // Buttons
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

    on(newBtn, "click", () => {
        const dayStr = isoDate(selected);
        window.location.href = `/Events/Create?date=${encodeURIComponent(dayStr)}`;
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
            row.innerHTML = `<strong>${ev.title}</strong>
                <small class="text-muted">${formatTimeRange(ev)}</small><br>
                <small>${ev.description || ""}</small>`;

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

        const firstOfMonth = new Date(y, m, 1);
        const startOffset = (firstOfMonth.getDay() + 6) % 7;
        const gridStart = new Date(y, m, 1 - startOffset);

        for (let i = 0; i < 42; i++) {
            const date = new Date(gridStart);
            date.setDate(gridStart.getDate() + i);

            const cell = document.createElement("div");
            cell.className = "day" + (date.getMonth() === m ? "" : " outside");

            if (isoDate(date) === isoDate(selected)) {
                cell.classList.add("selected");
            }

            cell.innerHTML = `<div class="dayNumber">${date.getDate()}</div>`;

            eventsForDate(isoDate(date)).slice(0, 2).forEach(ev => {
                const it = document.createElement("div");
                it.className = "eventItem";
                it.textContent = ev.title;

                it.onclick = e => {
                    e.stopPropagation();
                    window.location.href = `/Events/Details/${ev.id}`;
                };

                cell.appendChild(it);
            });

            cell.onclick = async () => {
                selected = new Date(date);
                if (date.getMonth() !== m) {
                    current = monthStart(date);
                    await renderAll();
                    return;
                }
                renderCalendar();
                renderSidebar();
            };

            cell.ondblclick = () => {
                window.location.href = `/Events/Create?date=${encodeURIComponent(isoDate(date))}`;
            };

            grid.appendChild(cell);
        }
    }

    async function renderAll() {
        current = monthStart(current);
        await loadEventsFromApi();
        renderCalendar();
        renderSidebar();
        loadRemindersAndSchedule(); // ✅ 1F
    }

    renderAll();
})();