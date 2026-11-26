/* ==========================================================================
   MetroBalance - public static demo (client side)
   --------------------------------------------------------------------------
   This is a browser-only simulation of the MetroBalance experience. It uses
   the same in-memory dataset that the .NET Web API serves, captured as a
   snapshot (see scripts/export-demo-data.ps1), so the numbers shown here are
   exactly the numbers the API returns.

   NOTHING here is a live DMRC record. Every balance and transaction is
   DEMO DATA. Real card numbers never produce a balance - they produce the
   official "how to read your balance at the station" guidance, exactly like
   the running application does.
   ========================================================================== */
(function () {
    "use strict";

    var DATA = window.METROBALANCE_DEMO_DATA || { cards: {}, guidance: {}, demoNumbers: [] };

    var $ = function (id) { return document.getElementById(id); };

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
        return "\u20B9 " + Number(amount || 0).toLocaleString("en-IN", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    /* ---------------- Card handling (mirrors MetroBalance.Core) ---------------- */

    // Mirrors CardValidator.Normalize + CardNumberPattern.
    function normalizeCardNumber(value) {
        return (value || "").trim().toUpperCase().replace(/ /g, "");
    }

    function localValidation(cardNumber) {
        var value = (cardNumber || "").trim();
        if (!value) {
            return "Please enter your Metro Smart Card number.";
        }
        var normalized = normalizeCardNumber(value);
        if (!/^(?=.{6,19}$)[A-Za-z0-9]+([-\s][A-Za-z0-9]+)*$/.test(normalized)) {
            return "Card number must be 6-19 characters and may contain letters, digits and hyphens only.";
        }
        return null;
    }

    // Mirrors CardNumberClassifier.Classify.
    var CardKind = { Unknown: 0, DemoCard: 1, EngravedId: 2, SmartCardNumber: 3, BankStyleNumber: 4 };

    function classifyCardNumber(raw) {
        var value = (raw || "").trim();
        if (value.length === 0) return CardKind.Unknown;
        if (value.toUpperCase().indexOf("DEMO") !== -1) return CardKind.DemoCard;

        var digits = value.replace(/[^0-9]/g, "");
        if (digits.length === 0) return CardKind.Unknown;
        if (/[A-Za-z]/.test(value)) return CardKind.Unknown;

        if (digits.length >= 10 && digits.length <= 11) return CardKind.EngravedId;
        if (digits.length === 12) return CardKind.SmartCardNumber;
        if (digits.length >= 13 && digits.length <= 19) return CardKind.BankStyleNumber;
        return CardKind.Unknown;
    }

    function describeCardKind(kind) {
        var g = DATA.guidance || {};
        var descriptions = g.cardKind || {};
        switch (kind) {
            case CardKind.DemoCard: return "MetroBalance demo card (simulated data).";
            case CardKind.EngravedId: return descriptions.engravedId || "";
            case CardKind.SmartCardNumber: return descriptions.smartCard || "";
            case CardKind.BankStyleNumber: return descriptions.bankStyle || "";
            default: return descriptions.unknown || "";
        }
    }

    // Mirrors RealCardGuidanceFactory.BuildSteps + PolicyNoteFor.
    function buildGuidance(cardNumber) {
        var g = DATA.guidance || {};
        var kind = classifyCardNumber(cardNumber);
        var steps = (g.baseSteps || []).slice();

        if (kind === CardKind.BankStyleNumber && g.bankFirstStep) {
            steps.unshift(g.bankFirstStep);
        } else if (kind === CardKind.EngravedId && g.engravedFirstStep) {
            steps.unshift(g.engravedFirstStep);
        }

        var policy = g.basePolicy || "";
        if (kind === CardKind.BankStyleNumber && g.bankPolicySuffix) {
            policy = policy + " " + g.bankPolicySuffix;
        }

        return {
            cardNumber: normalizeCardNumber(cardNumber),
            found: false,
            isDemoData: false,
            dataSource: g.dataSource || "",
            cardKind: kind,
            cardKindDescription: describeCardKind(kind),
            balanceAvailableOnline: false,
            policyNote: policy,
            steps: steps,
            officialLinks: g.officialLinks || []
        };
    }

    function findDemoCard(cardNumber) {
        var normalized = normalizeCardNumber(cardNumber);
        var cards = DATA.cards || {};
        var keys = Object.keys(cards);
        for (var i = 0; i < keys.length; i++) {
            if (normalizeCardNumber(keys[i]) === normalized) {
                return cards[keys[i]];
            }
        }
        return null;
    }

    /* ---------------- UI helpers ---------------- */

    var lookupMode = "amount";

    function setAlert(message, type) {
        var box = $("lookupAlert");
        if (!box) return;
        box.className = "alert-mb mt-4 is-visible " + (type === "error" ? "alert-mb--error" : "alert-mb--info");
        box.innerHTML = escapeHtml(message);
    }

    function clearAlert() {
        var box = $("lookupAlert");
        if (!box) return;
        box.className = "alert-mb mt-4";
        box.innerHTML = "";
    }

    function setLoading(isLoading) {
        var button = $("cardLookupButton");
        var text = $("lookupButtonText");
        if (button) button.disabled = isLoading;
        if (text) {
            text.textContent = isLoading
                ? "Checking..."
                : (lookupMode === "amount" ? "Check balance" : "Show balance + history");
        }
    }

    function setMode(mode) {
        lookupMode = mode === "statement" ? "statement" : "amount";

        [["amount", "modeAmount"], ["statement", "modeStatement"]].forEach(function (pair) {
            var btn = $(pair[1]);
            if (!btn) return;
            var active = pair[0] === lookupMode;
            btn.classList.toggle("is-active", active);
            btn.setAttribute("aria-pressed", active ? "true" : "false");
        });

        var text = $("lookupButtonText");
        if (text) text.textContent = lookupMode === "amount" ? "Check balance" : "Show balance + history";

        hideAllResults();
        clearAlert();
    }

    function hideAllResults() {
        ["amountSection", "resultSection", "guidanceSection"].forEach(function (id) {
            var section = $(id);
            if (section) section.classList.remove("is-visible");
        });
        clearAmount();
        clearResults();
    }

    function clearAmount() {
        var value = $("amountValue");
        if (value) value.textContent = "\u2014";
        var meta = $("amountMeta");
        if (meta) meta.textContent = "Card number";
        var updated = $("amountLastUpdated");
        if (updated) updated.textContent = "\u2014";
        var unavailable = $("amountUnavailable");
        if (unavailable) {
            unavailable.classList.remove("is-visible");
            unavailable.innerHTML = "";
        }
    }

    function clearResults() {
        var fields = {
            balanceAmount: "\u2014",
            balanceCardMeta: "Card number",
            balanceHolder: "\u2014",
            balanceCardType: "\u2014",
            balanceStatus: "\u2014",
            balanceLastUpdated: "\u2014",
            balanceTotalRecharged: "\u2014",
            journeyCountLabel: "0 journeys"
        };
        Object.keys(fields).forEach(function (id) {
            var el = $(id);
            if (el) el.textContent = fields[id];
        });
        var recharges = $("rechargeList");
        if (recharges) recharges.innerHTML = "";
        var body = $("transactionTableBody");
        if (body) body.innerHTML = "";
    }

    /* ---------------- Rendering ---------------- */

    function renderRecharges(recharges) {
        var container = $("rechargeList");
        if (!container) return;

        if (!recharges || recharges.length === 0) {
            container.innerHTML = '<div class="empty-state"><div class="empty-state__icon">&#128176;</div>' +
                '<div style="font-weight:600;">No recharges yet</div>' +
                '<div style="font-size:.88rem;">This card has no top-up transactions on record.</div></div>';
            return;
        }

        var items = recharges.map(function (r) {
            return '<li class="timeline__item">' +
                '<span class="timeline__dot timeline__dot--green"></span>' +
                '<div class="timeline__title">Added ' + escapeHtml(r.amountFormatted) +
                ' <span style="font-weight:400;font-size:.85rem;color:#64748b;">via ' + escapeHtml(r.channel) + '</span></div>' +
                '<div class="timeline__meta">' + escapeHtml(r.timestampFormatted) + ' &middot; ' + escapeHtml(r.mode) +
                ' &middot; money left after this: ' + escapeHtml(r.balanceAfterFormatted) + '</div>' +
                '</li>';
        }).join("");

        container.innerHTML = '<ul class="timeline">' + items + '</ul>';
    }

    function renderTransactions(transactions) {
        var body = $("transactionTableBody");
        var countLabel = $("journeyCountLabel");
        if (!body) return;

        if (!transactions || transactions.length === 0) {
            body.innerHTML = '<tr><td colspan="4" style="text-align:center;color:#64748b;padding:1.5rem 0;">No journey transactions on record.</td></tr>';
            if (countLabel) countLabel.textContent = "0 journeys";
            return;
        }

        body.innerHTML = transactions.map(function (t) {
            return '<tr>' +
                '<td style="white-space:nowrap;">' + escapeHtml(t.timestampFormatted) + '</td>' +
                '<td>' + escapeHtml(t.fromStation) + ' &rarr; ' + escapeHtml(t.toStation) + '</td>' +
                '<td><span class="meta-pill">' + escapeHtml(t.line) + '</span></td>' +
                '<td class="amount-debit" style="text-align:right;white-space:nowrap;">' + escapeHtml(t.amountFormatted) + '</td>' +
                '</tr>';
        }).join("");

        if (countLabel) {
            countLabel.textContent = transactions.length + (transactions.length === 1 ? " journey" : " journeys");
        }
    }

    function showAmount(card) {
        var section = $("amountSection");
        var unavailable = $("amountUnavailable");
        if (unavailable) unavailable.classList.remove("is-visible");

        var value = $("amountValue");
        if (value) value.textContent = card.statement.balance.balanceFormatted;
        var meta = $("amountMeta");
        if (meta) meta.textContent = card.cardNumber;
        var updated = $("amountLastUpdated");
        if (updated) {
            updated.textContent = card.statement.balance.lastUpdatedOnFormatted
                ? "Last updated " + card.statement.balance.lastUpdatedOnFormatted
                : "Last update not available";
        }

        if (section) {
            section.classList.add("is-visible");
            section.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    }

    function showResults(card) {
        var statement = card.statement;

        var amount = $("balanceAmount");
        if (amount) amount.textContent = statement.balance.balanceFormatted;
        var cardMeta = $("balanceCardMeta");
        if (cardMeta) cardMeta.textContent = statement.balance.cardNumber;
        var holder = $("balanceHolder");
        if (holder) holder.textContent = statement.balance.cardHolderName;
        var cardType = $("balanceCardType");
        if (cardType) cardType.textContent = statement.balance.cardType;
        var status = $("balanceStatus");
        if (status) status.textContent = statement.balance.status;
        var lastUpdated = $("balanceLastUpdated");
        if (lastUpdated) {
            lastUpdated.textContent = statement.balance.lastUpdatedOnFormatted
                ? "Last updated " + statement.balance.lastUpdatedOnFormatted
                : "Last update not available";
        }

        var recharges = statement.recharges || [];
        var total = recharges.reduce(function (sum, r) { return sum + (r.amount || 0); }, 0);
        var totalRecharged = $("balanceTotalRecharged");
        if (totalRecharged) totalRecharged.textContent = recharges.length === 0 ? "\u2014" : formatInr(total);

        renderRecharges(recharges);
        renderTransactions(statement.transactions);

        var section = $("resultSection");
        if (section) {
            section.classList.add("is-visible");
            section.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    }

    function showGuidance(guidance) {
        var section = $("guidanceSection");
        var steps = $("guidanceSteps");
        var links = $("guidanceLinks");

        if (steps) {
            steps.innerHTML = (guidance.steps || []).map(function (step) {
                return '<li>' + escapeHtml(step) + '</li>';
            }).join("");
        }

        if (links) {
            links.innerHTML = (guidance.officialLinks || []).map(function (link) {
                return '<a class="official-link' + (link.isPrimary ? ' official-link--primary' : '') +
                    '" href="' + escapeHtml(link.url) + '" target="_blank" rel="noopener noreferrer">' +
                    '<span class="official-link__title">' + escapeHtml(link.title) +
                    ' <svg viewBox="0 0 24 24" width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" ' +
                    'stroke-linecap="round" stroke-linejoin="round"><path d="M7 17L17 7"></path><path d="M8 7h9v9"></path></svg>' +
                    '</span>' +
                    '<span class="official-link__desc">' + escapeHtml(link.description) + '</span>' +
                    '</a>';
            }).join("");
        }

        var kind = $("guidanceCardKind");
        if (kind) kind.textContent = guidance.cardKindDescription || "";
        var policy = $("guidancePolicy");
        if (policy) policy.textContent = guidance.policyNote || "";
        var number = $("guidanceCardNumber");
        if (number) number.textContent = guidance.cardNumber || "";

        if (section) {
            section.classList.add("is-visible");
            section.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    }

    /* ---------------- Lookup flow ---------------- */

    function handleSubmit(event) {
        if (event) event.preventDefault();

        var input = $("cardNumberInput");
        var cardNumber = input ? input.value : "";
        var invalid = localValidation(cardNumber);

        if (invalid) {
            hideAllResults();
            setAlert(invalid, "error");
            return;
        }

        clearAlert();
        setLoading(true);

        // The static demo answers instantly; the tiny delay keeps the button
        // state (and therefore the look) identical to the live application.
        window.setTimeout(function () {
            var card = findDemoCard(cardNumber);

            if (!card) {
                // A real card: DMRC publishes no online balance service, so the
                // demo never invents one. Show the official station guidance.
                hideAllResults();
                showGuidance(buildGuidance(cardNumber));
                setLoading(false);
                return;
            }

            if (lookupMode === "amount") {
                var resultSection = $("resultSection");
                var guidanceSection = $("guidanceSection");
                if (resultSection) resultSection.classList.remove("is-visible");
                if (guidanceSection) guidanceSection.classList.remove("is-visible");
                clearResults();
                showAmount(card);
                setLoading(false);
                return;
            }

            var amountSection = $("amountSection");
            var guidance = $("guidanceSection");
            if (amountSection) amountSection.classList.remove("is-visible");
            if (guidance) guidance.classList.remove("is-visible");
            clearAmount();

            showResults(card);

            if (card.cardStatus && card.cardStatus !== "Active") {
                setAlert(card.message || "This card is not active.", "error");
            }
            setLoading(false);
        }, 180);
    }

    function loadDemoCards() {
        var container = $("demoCardChips");
        if (!container) return;

        var numbers = DATA.demoNumbers || [];
        if (numbers.length === 0) {
            container.innerHTML = '<span style="color:#64748b;font-size:.85rem;">No demo cards available.</span>';
            return;
        }

        container.innerHTML = numbers.map(function (card) {
            return '<button type="button" class="demo-chip" data-card="' + escapeHtml(card) + '">' + escapeHtml(card) + '</button>';
        }).join("");

        container.querySelectorAll(".demo-chip").forEach(function (chip) {
            chip.addEventListener("click", function () {
                var input = $("cardNumberInput");
                if (input) {
                    input.value = chip.getAttribute("data-card");
                    input.focus();
                }
                handleSubmit(null);
            });
        });
    }

    function fillSnapshotStamp() {
        var stamp = $("snapshotStamp");
        if (!stamp || !DATA.capturedAtUtc) return;
        try {
            var date = new Date(DATA.capturedAtUtc);
            stamp.textContent = date.toLocaleString("en-IN", {
                day: "2-digit", month: "short", year: "numeric",
                hour: "2-digit", minute: "2-digit"
            });
        } catch (e) {
            stamp.textContent = DATA.capturedAtUtc;
        }
    }

    /* The hero card mirrors the first demo card, so the figures on the page always
       match the API snapshot instead of a hand written number. */
    function fillHeroCard() {
        var numbers = DATA.demoNumbers || [];
        if (numbers.length === 0) return;

        var card = (DATA.cards || {})[numbers[0]];
        if (!card || !card.statement) return;

        var balance = card.statement.balance;
        var journey = (card.statement.transactions || [])[0];

        var set = function (id, value) {
            var el = $(id);
            if (el && value) el.textContent = value;
        };

        set("heroHolder", balance.cardHolderName);
        set("heroCardType", balance.cardType);
        set("heroBalance", balance.balanceFormatted);
        set("heroUpdated", balance.lastUpdatedOnFormatted);
        if (journey) set("heroJourney", journey.fromStation + " \u2192 " + journey.toStation);
    }

    /* ---------------- Raw API samples ---------------- */

    function highlightJson(value) {
        var json = JSON.stringify(value, null, 2) || "";
        return escapeHtml(json)
            .replace(/(&quot;[^&]*?&quot;)(\s*:)/g, '<span class="k">$1</span>$2')
            .replace(/:\s(&quot;.*?&quot;)/g, ': <span class="s">$1</span>')
            .replace(/:\s(\d+\.?\d*|true|false|null)/g, ': <span class="s">$1</span>');
    }

    function renderRawSamples() {
        var samples = DATA.rawSamples || {};
        var targets = {
            rawBalance: samples.balance,
            rawStatement: samples.statement,
            rawEnquiry: samples.enquiry,
            rawAmount: samples.amount
        };
        Object.keys(targets).forEach(function (id) {
            var el = $(id);
            if (!el) return;
            el.innerHTML = targets[id] ? highlightJson(targets[id]) : "// No sample captured.";
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        var form = $("cardLookupForm");
        if (form) form.addEventListener("submit", handleSubmit);

        var modeAmount = $("modeAmount");
        if (modeAmount) modeAmount.addEventListener("click", function () { setMode("amount"); });

        var modeStatement = $("modeStatement");
        if (modeStatement) modeStatement.addEventListener("click", function () { setMode("statement"); });

        var fullStatement = $("amountFullStatementButton");
        if (fullStatement) {
            fullStatement.addEventListener("click", function () {
                setMode("statement");
                handleSubmit(null);
            });
        }

        loadDemoCards();
        fillSnapshotStamp();
        fillHeroCard();
        renderRawSamples();
    });
})();
