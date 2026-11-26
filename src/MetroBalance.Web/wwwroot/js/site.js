/* MetroBalance front-end scripts (no external dependencies). */
(function () {
    "use strict";

    const apiBase = "/api/cards";

    const $ = (id) => document.getElementById(id);

    function escapeHtml(value) {
        if (value === null || value === undefined) return "";
        return String(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function formatInr(amount) {
        return "₹ " + Number(amount || 0).toLocaleString("en-IN", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function setAlert(message, type) {
        const box = $("lookupAlert");
        if (!box) return;
        box.className = "alert-mb mt-4 " + (type === "error" ? "alert-mb--error" : "alert-mb--info");
        box.innerHTML = escapeHtml(message);
    }

    function clearAlert() {
        const box = $("lookupAlert");
        if (!box) return;
        box.className = "alert-mb mt-4 d-none";
        box.innerHTML = "";
    }

    let lookupMode = "amount";

    function setLoading(isLoading) {
        const button = $("cardLookupButton");
        const spinner = $("lookupSpinner");
        const text = $("lookupButtonText");
        if (!button) return;
        button.disabled = isLoading;
        if (spinner) spinner.classList.toggle("d-none", !isLoading);
        if (text) {
            text.textContent = isLoading
                ? "Checking..."
                : (lookupMode === "amount" ? "Check balance" : "Show balance + history");
        }
    }

    function setMode(mode) {
        lookupMode = mode === "statement" ? "statement" : "amount";

        ["amount", "statement"].forEach(function (value) {
            const btn = $("mode" + (value === "amount" ? "Amount" : "Statement"));
            if (!btn) return;
            const active = value === lookupMode;
            btn.classList.toggle("is-active", active);
            btn.setAttribute("aria-pressed", active ? "true" : "false");
        });

        const text = $("lookupButtonText");
        if (text) {
            text.textContent = lookupMode === "amount" ? "Check balance" : "Show balance + history";
        }

        // Switching modes clears the previous answer so no stale figure stays on screen.
        const amountSection = $("amountSection");
        const resultSection = $("resultSection");
        const guidanceSection = $("guidanceSection");
        if (amountSection) amountSection.classList.remove("is-visible");
        if (resultSection) resultSection.classList.remove("is-visible");
        if (guidanceSection) guidanceSection.classList.remove("is-visible");
        clearResults();
        clearAmount();
        clearAlert();
    }

    async function fetchAmount(cardNumber) {
        const url = apiBase + "/" + encodeURIComponent(cardNumber) + "/amount";
        const response = await fetch(url, {
            headers: { "Accept": "application/json" }
        });

        let payload = null;
        try {
            payload = await response.json();
        } catch (parseError) {
            throw new Error("The server returned an unexpected response. Please try again.");
        }

        if (!response.ok || !payload || payload.success !== true) {
            const message = (payload && payload.message) || "We could not check that card. Please check the number and try again.";
            throw new Error(message);
        }

        return payload.data;
    }

    async function fetchEnquiry(cardNumber) {
        const url = apiBase + "/" + encodeURIComponent(cardNumber) + "/enquiry";
        const response = await fetch(url, {
            headers: { "Accept": "application/json" }
        });

        let payload = null;
        try {
            payload = await response.json();
        } catch (parseError) {
            throw new Error("The server returned an unexpected response. Please try again.");
        }

        if (!response.ok || !payload || payload.success !== true) {
            const message = (payload && payload.message) || "We could not check that card. Please check the number and try again.";
            throw new Error(message);
        }

        return payload.data;
    }

    function renderRecharges(recharges) {
        const container = $("rechargeList");
        if (!container) return;

        if (!recharges || recharges.length === 0) {
            container.innerHTML = '<div class="empty-state"><div class="empty-state__icon">&#128176;</div>' +
                '<div class="fw-semibold">No recharges yet</div>' +
                '<div class="small">This card has no top-up transactions on record.</div></div>';
            return;
        }

        const items = recharges.map(function (r) {
            return '<li class="timeline__item">' +
                '<span class="timeline__dot timeline__dot--green"></span>' +
                '<div class="timeline__title">Added ' + escapeHtml(r.amountFormatted) +
                ' <span class="small text-muted">via ' + escapeHtml(r.channel) + '</span></div>' +
                '<div class="timeline__meta">' + escapeHtml(r.timestampFormatted) + ' &middot; ' + escapeHtml(r.mode) +
                ' &middot; money left after this: ' + escapeHtml(r.balanceAfterFormatted) + '</div>' +
                '</li>';
        }).join("");

        container.innerHTML = '<ul class="timeline">' + items + '</ul>';
    }

    function renderTransactions(transactions) {
        const body = $("transactionTableBody");
        const countLabel = $("journeyCountLabel");
        if (!body) return;

        if (!transactions || transactions.length === 0) {
            body.innerHTML = '<tr><td colspan="4" class="text-center text-muted py-4">No journey transactions on record.</td></tr>';
            if (countLabel) countLabel.textContent = "0 journeys";
            return;
        }

        const rows = transactions.map(function (t) {
            return '<tr>' +
                '<td class="ps-3 text-nowrap">' + escapeHtml(t.timestampFormatted) + '</td>' +
                '<td>' + escapeHtml(t.fromStation) + ' &rarr; ' + escapeHtml(t.toStation) + '</td>' +
                '<td><span class="meta-pill">' + escapeHtml(t.line) + '</span></td>' +
                '<td class="text-end pe-3 amount-debit">' + escapeHtml(t.amountFormatted) + '</td>' +
                '</tr>';
        }).join("");

        body.innerHTML = rows;
        if (countLabel) countLabel.textContent = transactions.length + (transactions.length === 1 ? " journey" : " journeys");
    }

    function renderBalance(balance) {
        $("balanceAmount").innerHTML = escapeHtml(balance.balanceFormatted);
        $("balanceCardMeta").textContent = balance.cardNumber;
        $("balanceHolder").textContent = balance.cardHolderName;
        $("balanceCardType").textContent = balance.cardType;
        $("balanceStatus").textContent = balance.status;
        $("balanceLastUpdated").textContent = balance.lastUpdatedOnFormatted
            ? "Last updated " + balance.lastUpdatedOnFormatted
            : "Last update not available";

        const recharges = $("rechargeList");
        if (recharges) recharges.dataset.total = balance.balanceFormatted;
    }

    function clearAmount() {
        const card = $("amountCard");
        if (card) card.classList.add("d-none");

        const value = $("amountValue");
        if (value) value.textContent = "—";

        const meta = $("amountMeta");
        if (meta) meta.textContent = "Card number";

        const updated = $("amountLastUpdated");
        if (updated) updated.textContent = "—";

        const unavailable = $("amountUnavailable");
        if (unavailable) {
            unavailable.classList.add("d-none");
            unavailable.textContent = "";
        }
    }

    function showAmount(result) {
        const section = $("amountSection");
        const card = $("amountCard");
        const unavailable = $("amountUnavailable");

        if (card) card.classList.add("d-none");
        if (unavailable) unavailable.classList.add("d-none");

        if (result && result.balanceAvailableOnline) {
            const value = $("amountValue");
            if (value) value.textContent = result.amountFormatted || "—";

            const meta = $("amountMeta");
            if (meta) meta.textContent = result.cardNumber || "Card number";

            const updated = $("amountLastUpdated");
            if (updated) {
                updated.textContent = result.lastUpdatedOnFormatted
                    ? "Last updated " + result.lastUpdatedOnFormatted
                    : "Last update not available";
            }

            if (card) card.classList.remove("d-none");
        } else {
            // A real card: DMRC publishes no online balance service, so no amount is shown.
            // Clear any figure rendered earlier so no stale balance survives in the page.
            const staleValue = $("amountValue");
            if (staleValue) staleValue.textContent = "—";

            const staleMeta = $("amountMeta");
            if (staleMeta) staleMeta.textContent = "Card number";

            const staleUpdated = $("amountLastUpdated");
            if (staleUpdated) staleUpdated.textContent = "—";

            if (unavailable) {
                var note = (result && (result.policyNote || result.message)) ||
                    "DMRC does not offer an online card balance service, so this card cannot be read over the internet.";
                unavailable.innerHTML = escapeHtml(note) +
                    ' <a class="fw-semibold" href="/Card?mode=guide">See how to read your balance at the station</a>.';
                unavailable.classList.remove("d-none");
            }
        }

        if (section) {
            section.classList.add("is-visible");
            section.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    }

    function clearResults() {
        const amount = $("balanceAmount");
        if (amount) amount.textContent = "—";

        const cardMeta = $("balanceCardMeta");
        if (cardMeta) cardMeta.textContent = "Card number";

        const holder = $("balanceHolder");
        if (holder) holder.textContent = "—";

        const cardType = $("balanceCardType");
        if (cardType) cardType.textContent = "—";

        const status = $("balanceStatus");
        if (status) status.textContent = "—";

        const lastUpdated = $("balanceLastUpdated");
        if (lastUpdated) lastUpdated.textContent = "—";

        const recharges = $("rechargeList");
        if (recharges) recharges.innerHTML = "";

        const transactions = $("transactionTableBody");
        if (transactions) transactions.innerHTML = "";

        const journeyCount = $("journeyCountLabel");
        if (journeyCount) journeyCount.textContent = "0 journeys";

        const totalRecharged = $("balanceTotalRecharged");
        if (totalRecharged) totalRecharged.textContent = "—";
    }

    function showGuidance(enquiry) {
        const section = $("guidanceSection");
        const steps = $("guidanceSteps");
        const links = $("guidanceLinks");

        // A previous demo lookup may have left data behind; a real card never shows a balance.
        clearResults();
        clearAmount();

        const amountSection = $("amountSection");
        if (amountSection) amountSection.classList.remove("is-visible");

        if (steps) {
            const items = (enquiry.steps || []).map(function (step) {
                return '<li>' + escapeHtml(step) + '</li>';
            }).join("");
            steps.innerHTML = items;
        }

        if (links) {
            const items = (enquiry.officialLinks || []).map(function (link) {
                return '<a class="official-link' + (link.isPrimary ? ' official-link--primary' : '') +
                    '" href="' + escapeHtml(link.url) + '" target="_blank" rel="noopener noreferrer">' +
                    '<span class="official-link__title">' + escapeHtml(link.title) +
                    ' <svg viewBox="0 0 24 24" width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" ' +
                    'stroke-linecap="round" stroke-linejoin="round"><path d="M7 17L17 7"></path><path d="M8 7h9v9"></path></svg>' +
                    '</span>' +
                    '<span class="official-link__desc">' + escapeHtml(link.description) + '</span>' +
                    '</a>';
            }).join("");
            links.innerHTML = items;
        }

        const kind = $("guidanceCardKind");
        if (kind) kind.textContent = enquiry.cardKindDescription || "";

        const policy = $("guidancePolicy");
        if (policy) policy.textContent = enquiry.policyNote || "";

        const number = $("guidanceCardNumber");
        if (number) number.textContent = enquiry.cardNumber || "";

        if (section) {
            section.classList.add("is-visible");
            section.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    }

    function showResults(statement) {
        renderBalance(statement.balance);
        renderRecharges(statement.recharges);
        renderTransactions(statement.transactions);

        const totalRecharged = $("balanceTotalRecharged");
        if (totalRecharged) {
            const recharges = statement.recharges || [];
            const sum = recharges.reduce(function (total, r) { return total + (r.amount || 0); }, 0);
            totalRecharged.textContent = recharges.length === 0 ? "—" : formatInr(sum);
        }

        const section = $("resultSection");
        if (section) {
            section.classList.add("is-visible");
            section.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    }

    function localValidation(cardNumber) {
        const value = (cardNumber || "").trim();
        if (!value) {
            return "Please enter your Metro Smart Card number.";
        }
        // Mirrors CardValidator in MetroBalance.Core: the pattern is applied to the
        // normalised value (spaces removed), so "8177 7696 0017 5160" counts as 16 digits.
        const normalized = value.toUpperCase().replace(/ /g, "");
        if (!/^(?=.{6,19}$)[A-Za-z0-9]+([-\s][A-Za-z0-9]+)*$/.test(normalized)) {
            return "Card number must be 6-19 characters and may contain letters, digits and hyphens only.";
        }
        return null;
    }

    async function handleSubmit(event) {
        event.preventDefault();
        const input = $("cardNumberInput");
        const cardNumber = input ? input.value : "";
        const invalid = localValidation(cardNumber);
        if (invalid) {
            // Drop any previous answer so a stale balance is never left on screen.
            const staleAmount = $("amountSection");
            const staleResult = $("resultSection");
            const staleGuidance = $("guidanceSection");
            if (staleAmount) staleAmount.classList.remove("is-visible");
            if (staleResult) staleResult.classList.remove("is-visible");
            if (staleGuidance) staleGuidance.classList.remove("is-visible");
            clearAmount();
            clearResults();
            setAlert(invalid, "error");
            return;
        }

        clearAlert();
        setLoading(true);
        try {
            const resultSection = $("resultSection");
            const guidanceSection = $("guidanceSection");
            const amountSection = $("amountSection");

            if (lookupMode === "amount") {
                if (resultSection) resultSection.classList.remove("is-visible");
                if (guidanceSection) guidanceSection.classList.remove("is-visible");
                clearResults();

                const amount = await fetchAmount(cardNumber.trim());
                showAmount(amount);
                return;
            }

            if (amountSection) amountSection.classList.remove("is-visible");
            clearAmount();
            const enquiry = await fetchEnquiry(cardNumber.trim());

            if (enquiry.found && enquiry.statement) {
                if (guidanceSection) guidanceSection.classList.remove("is-visible");
                showResults(enquiry.statement);
                if (enquiry.cardStatus && enquiry.cardStatus !== "Active") {
                    setAlert(enquiry.message || "This card is not active.", "error");
                }
            } else {
                if (resultSection) resultSection.classList.remove("is-visible");
                showGuidance(enquiry);
            }
        } catch (error) {
            setAlert(error.message || "Something went wrong while checking the balance.", "error");
        } finally {
            setLoading(false);
        }
    }

    async function loadDemoCards() {
        const container = $("demoCardChips");
        if (!container) return;

        const endpoint = window.metroBalanceDemoCardsEndpoint || (apiBase + "/demo-numbers");
        try {
            const response = await fetch(endpoint, { headers: { "Accept": "application/json" } });
            const payload = await response.json();
            if (!payload || payload.success !== true) throw new Error("failed");

            const cards = payload.data || [];
            if (cards.length === 0) {
                container.innerHTML = '<span class="text-muted small">No demo cards available.</span>';
                return;
            }

            container.innerHTML = cards.map(function (card) {
                return '<button type="button" class="demo-chip" data-card="' + escapeHtml(card) + '">' + escapeHtml(card) + '</button>';
            }).join("");

            container.querySelectorAll(".demo-chip").forEach(function (chip) {
                chip.addEventListener("click", function () {
                    const input = $("cardNumberInput");
                    if (input) {
                        input.value = chip.getAttribute("data-card");
                        input.focus();
                    }
                    const form = $("cardLookupForm");
                    if (form) form.requestSubmit ? form.requestSubmit() : form.dispatchEvent(new Event("submit", { cancelable: true }));
                });
            });
        } catch (error) {
            container.innerHTML = '<span class="text-muted small">Demo cards could not be loaded right now.</span>';
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        const form = $("cardLookupForm");
        if (form) {
            form.addEventListener("submit", handleSubmit);
        }

        const modeAmount = $("modeAmount");
        if (modeAmount) {
            modeAmount.addEventListener("click", function () { setMode("amount"); });
        }

        const modeStatement = $("modeStatement");
        if (modeStatement) {
            modeStatement.addEventListener("click", function () { setMode("statement"); });
        }

        const fullStatement = $("amountFullStatementButton");
        if (fullStatement) {
            fullStatement.addEventListener("click", function () {
                setMode("statement");
                const form = $("cardLookupForm");
                if (form) {
                    form.requestSubmit ? form.requestSubmit() : form.dispatchEvent(new Event("submit", { cancelable: true }));
                }
            });
        }

        loadDemoCards();
    });
})();
