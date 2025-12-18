(() => {
    // ========================
    // Helpers
    // ========================
    function $(id) {
        return document.getElementById(id);
    }

    function on(el, ev, fn) {
        if (el) el.addEventListener(ev, fn);
    }

    function nowMs() {
        return Date.now();
    }

    function parseUtcToMs(utc) {
        // utc je napr. "2025-12-17T12:00:00Z" alebo bez Z (podľa serializeru)
        const d = new Date(utc);
        const t = d.getTime();
        return Number.isFinite(t) ? t : NaN;
    }

    // ========================
    // Reminder UI (Bootstrap modal + fallback)
    // ========================
    function showReminderModal(title, message, fireAtUtc) {
        const modalEl = $("reminderModal");
        const t = $("reminderModalTitle");
        const b = $("reminderModalBody");
        const tm = $("reminderModalTime");

        // fallback, ak modal nemáš v _Layout.cshtml
        if (!modalEl || !t || !b) {
            const when = fireAtUtc ? new Date(fireAtUtc).toLocaleString("sk-SK") : "";
            alert(`${title || "Pripomienka"}\n${message || ""}\n${when}`);
            return;
        }

        t.textContent = title || "Pripomienka";
        b.textContent = message || "Máte udalosť.";

        if (tm) {
            tm.textContent = fireAtUtc
                ? `Čas pripomienky: ${new Date(fireAtUtc).toLocaleString("sk-SK")}`
                : "";
        }

        if (window.bootstrap && bootstrap.Modal) {
            const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
            modal.show();
        } else {
            alert(`${title || "Pripomienka"}\n\n${message || ""}`);
        }
    }

    // ========================
    // Optional browser Notification (ak je povolené)
    // ========================
    function tryShowBrowserNotification(title, body) {
        try {
            if (!("Notification" in window)) return;
            if (Notification.permission !== "granted") return;
            new Notification(title, { body, icon: "/favicon.ico" });
        } catch {
            // ignore
        }
    }

    // ========================
    // Reminders polling (funguje na každej stránke)
    // ========================
    let reminderPollStarted = false;

    // aby sa jeden reminder nezobrazil 2x (aj keby API vrátilo opakovane)
    const shownIds = new Set();

    // tolerancia: ak sa trafíme ±10s okolo času, zobrazíme
    const DUE_TOLERANCE_MS = 10_000;

    async function markSeen(id) {
        try {
            await fetch(`/api/my-reminders/${id}/seen`, { method: "POST" });
        } catch (e) {
            console.error("Failed to mark reminder as seen", e);
        }
    }

    function isDueNow(reminder) {
        if (!reminder || !reminder.fireAtUtc) return false;

        const fireAt = parseUtcToMs(reminder.fireAtUtc);
        if (!Number.isFinite(fireAt)) return false;

        const diff = fireAt - nowMs();

        // zobrazíme iba keď je už čas (alebo maximálne 10s do budúcna / 10s do minulosti)
        return Math.abs(diff) <= DUE_TOLERANCE_MS || diff <= 0;
    }

    async function pollRemindersOnce() {
        try {
            const res = await fetch("/api/my-reminders", {
                headers: { Accept: "application/json" }
            });

            // ak nie si prihlásená, API vráti 401 → nič nerobíme
            if (res.status === 401) return;
            if (!res.ok) return;

            const reminders = await res.json();
            if (!Array.isArray(reminders) || reminders.length === 0) return;

            // zobraz iba tie, ktoré sú "due" teraz
            const due = reminders.filter(r => r && r.id && !shownIds.has(r.id) && isDueNow(r));

            for (const r of due) {
                shownIds.add(r.id);

                showReminderModal(r.eventTitle, r.message, r.fireAtUtc);
                tryShowBrowserNotification(
                    r.eventTitle || "Pripomienka",
                    r.message || "Máte udalosť."
                );

                await markSeen(r.id);
            }
        } catch (e) {
            console.error("Reminder polling failed", e);
        }
    }

    function startReminderPolling() {
        if (reminderPollStarted) return;
        reminderPollStarted = true;

        // prvý fetch hneď
        pollRemindersOnce();

        // potom pravidelne
        setInterval(pollRemindersOnce, 5_000);
    }

    // spustíme polling na každej stránke
    startReminderPolling();

    // ========================
    // Calendar UI (iba ak je na stránke)
    // ========================
    const grid = $("grid");
    if (!grid) return;

    const monthLabel = $("monthLabel");
    const prevBtn = $("prevBtn");
    const nextBtn = $("nextBtn");
    const todayBtn = $("todayBtn");
    const newBtn = $("newBtn");
    const selectedLabel = $("selectedLabel");
    const dayEventsEl = $("dayEvents");

    if (!monthLabel || !selectedLabel || !dayEventsEl) return;

    let eventsCache = [];

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

    async function loadEventsFromApi() {
        const res = await fetch("/api/my-events", { headers: { Accept: "application/json" } });
        eventsCache = res.ok ? await res.json() : [];
    }

    function eventsForDate(dateStr) {
        const dayStart = new Date(dateStr + "T00:00:00");
        const dayEnd = new Date(dateStr + "T23:59:59");

        return eventsCache.filter(ev => {
            const evStart = new Date(ev.start);
            const evEnd = new Date(ev.end);

            return evStart <= dayEnd && evEnd >= dayStart;
        });
    }

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

    function renderSidebar() {
        selectedLabel.textContent = selected.toLocaleDateString("sk-SK", {
            weekday: "long",
            year: "numeric",
            month: "long",
            day: "numeric"
        });

        const dayStr = isoDate(selected);
        const evs = eventsForDate(dayStr);

        dayEventsEl.innerHTML = "";
        if (evs.length === 0) {
            dayEventsEl.textContent = "Žiadne udalosti";
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

        monthLabel.textContent = new Date(y, m, 1).toLocaleDateString("sk-SK", {
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
    }

    renderAll();
})();