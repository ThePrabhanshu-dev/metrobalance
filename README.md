# MetroBalance

A public, zero-login website for checking **Delhi Metro Smart Card** balance, recharge history and journey transactions.

> ### ⚠️ DEMO DATA ONLY
> Every balance, recharge and transaction in this application is **simulated in memory**.
> MetroBalance does **not** connect to DMRC, does not scrape any website, and does not use
> any real card data. The UI, API responses and Swagger docs are all clearly labelled
> **DEMO DATA**. The provider seam is ready for a future *authorized* DMRC integration
> (see [DMRC integration](#dmrc-integration--where-to-plug-in-the-real-api)).

---

## Tech stack

| Concern | Choice |
| --- | --- |
| Runtime | .NET 10 (`net10.0`) — ASP.NET Core MVC + ASP.NET Core Web API |
| Front end | Bootstrap 5 (vendored locally), plain HTML / CSS / JavaScript, jQuery + jQuery Validate |
| Data | In-memory `List<T>` / `Dictionary<,>` only — **no database, no SQL Server, no EF Core** |
| API docs | Swagger / OpenAPI (Swashbuckle.AspNetCore 11.0.1) |
| Cost | 100 % free — no paid APIs, no subscriptions, no external services |

---

## Solution layout

```
D:\Metro
├── MetroBalance.slnx                     # solution (5 projects)
├── docs/                                 # DMRC pitch package (proposal, contacts, e-mails, guide, deck)
│   ├── DMRC_PROPOSAL.md
│   ├── DMRC_CONTACTS.md
│   ├── OUTREACH_EMAIL.md
│   ├── PRESENTATION_GUIDE.md
│   ├── MetroBalance-DMRC-Proposal.pdf   # 13-slide deck as PDF (e-mail attachment)
│   └── index.html                        # deck copy served by GitHub Pages
├── scripts/
│   ├── sync-docs.ps1                     # copies the deck into docs/ for GitHub Pages
│   └── publish-github.ps1                # gh repo create -> push uat -> push prd -> enable Pages
├── .github/workflows/pages.yml           # deploys docs/ to GitHub Pages on push to prd
└── src
    ├── MetroBalance.Core                 # domain models, contracts, interfaces, exceptions
    │   ├── Contracts/ApiContracts.cs     # response DTOs
    │   ├── Contracts/MoneyFormatter.cs   # ₹ formatting (en-IN)
    │   ├── Exceptions/CardExceptions.cs  # domain exceptions
    │   ├── Interfaces/ICardService.cs
    │   ├── Interfaces/IMetroCardProvider.cs   # ← DMRC-ready seam
    │   ├── Interfaces/CardValidator.cs
    │   └── Models/SmartCard.cs, CardTransaction.cs
    │
    ├── MetroBalance.Infrastructure        # in-memory data + services + DI
    │   ├── Data/InMemoryCardStore.cs      # List/Dictionary store
    │   ├── Data/DemoCardDataFactory.cs    # deterministic demo data (8 cards)
    │   ├── Providers/MockMetroCardProvider.cs
    │   ├── Services/CardService.cs
    │   └── InfrastructureServiceExtensions.cs
    │
    ├── MetroBalance.Api                   # shared API layer (class library)
    │   ├── Controllers/CardsController.cs # REST endpoints under /api/cards
    │   ├── Extensions/ApiExtensions.cs    # Swagger, camelCase JSON, UseMetroBalanceApi()
    │   ├── Extensions/CardExceptionMiddleware.cs
    │   └── GlobalUsings.cs
    │
    ├── MetroBalance.ApiHost               # standalone Web API executable host
    │   └── Program.cs                     # / → /swagger
    │
    └── MetroBalance.Web                   # ASP.NET Core MVC front end
        ├── Controllers/HomeController.cs, CardController.cs
        ├── Models/CardLookupViewModel.cs
        ├── Views/Home/{Index,About,Faq}.cshtml
        ├── Views/Card/Index.cshtml        # server-rendered statement page
        ├── Views/Shared/_Layout.cshtml
        └── wwwroot/{css/site.css, js/site.js, lib/…}
```

### Why the `Api` + `ApiHost` split?

`MetroBalance.Api` is a **class library** (`Microsoft.NET.Sdk` + `FrameworkReference Microsoft.AspNetCore.App`)
that holds the controllers, Swagger setup and exception middleware. `MetroBalance.ApiHost` is the
**executable** Web API host for that library. `MetroBalance.Web` (MVC) also references the class
library so the same `/api/cards/...` endpoints are served by the website itself.

This split avoids a known .NET SDK issue where one Web-SDK project references another Web-SDK
project (scoped CSS bundle / static web asset endpoint mismatch), and it keeps the API layer
reusable by any host.

---

## How to run

```bash
# Website (MVC + embedded API)  →  http://localhost:5080   (default profile port: 5239)
dotnet run --project src/MetroBalance.Web --urls "http://localhost:5080"

# Standalone Web API + Swagger →  http://localhost:5197  ( "/" redirects to /swagger )
dotnet run --project src/MetroBalance.ApiHost --urls "http://localhost:5197"

# Build everything
dotnet build MetroBalance.slnx
```

### URLs (website host)

| URL | Description |
| --- | --- |
| `http://localhost:5080/` | Homepage — hero card checker, features, FAQ teaser |
| `http://localhost:5080/Home/About` | About page |
| `http://localhost:5080/Home/Faq` | FAQ page |
| `http://localhost:5080/Home/Guide` | Real Card Guide — how to read a real balance (AVM / counter / top-up) |
| `http://localhost:5080/showcase` | **Showcase** — the DMRC proposal deck embedded in the site (print/PDF friendly) |
| `http://localhost:5080/presentations/dmrc-proposal.html` | Standalone 13-slide deck — single file, works offline |
| `http://localhost:5080/Card?cardNumber=DMRC-DEMO-1001` | Server-rendered statement page (works without JS) |
| `http://localhost:5080/Card?cardNumber=DMRC-DEMO-1001&mode=amount` | Server-rendered **balance-only** page |
| `http://localhost:5080/Card/Lookup` | POST target of the no-JavaScript form (antiforgery protected) |
| `http://localhost:5080/swagger` | Swagger UI |

---

## DMRC showcase & pitch package

This repository also contains everything needed to **present the project to DMRC** and to request an authorised
data path. Everything is in [`docs/`](docs/):

| File | What it is |
| --- | --- |
| [`docs/DMRC_PROPOSAL.md`](docs/DMRC_PROPOSAL.md) | Formal proposal — problem, solution, architecture, four integration options, compliance, phased rollout, the ask |
| [`docs/DMRC_CONTACTS.md`](docs/DMRC_CONTACTS.md) | **Verified** DMRC contacts (ED/IT, AGM/AFC, Director/BD, GM/Legal, Corporate Communications) with source URLs, procurement routes and FAQ quotes |
| [`docs/OUTREACH_EMAIL.md`](docs/OUTREACH_EMAIL.md) | E-mail templates: primary outreach to ED/IT, commercial variant to Director/BD, PR variant, meeting request, 7-day and 21-day follow-ups, LinkedIn/WhatsApp versions, do-nots |
| [`docs/PRESENTATION_GUIDE.md`](docs/PRESENTATION_GUIDE.md) | Slide-by-slide speaker notes, timings, a one-page cheat sheet and the ten hardest questions with answers |
| [`docs/index.html`](docs/index.html) | Copy of the deck, published to GitHub Pages (shareable link) |
| [`docs/MetroBalance-DMRC-Proposal.pdf`](docs/MetroBalance-DMRC-Proposal.pdf) | The deck as a 13-page PDF — attach this to the e-mail |

### The showcase deck

* In-app: **`/showcase`** (hero, buttons, live 16:9 embed of the deck).
* Standalone: **`/presentations/dmrc-proposal.html`** — one self-contained HTML file, **no CDN, no external fonts,
  no network needed**; e-mail it as an attachment or open it from disk.
* 13 slides: cover → problem (quoting DMRC's own FAQ) → proposal → product → live demo → architecture → API →
  trust & compliance → four integration paths → "what we will never do" → phased rollout (Phase 1 **UAT**, Phase 3
  **PRD**) → the four asks → close.
* Navigation: `←` `→` / `Space`, `F` fullscreen, `P` print-to-PDF, `Home` / `End`, clickable dots, deep links (`#7`).

### Published links (live)

| What | Link |
| --- | --- |
| Showcase deck (GitHub Pages) | <https://theprabhanshu-dev.github.io/metrobalance/> |
| Deck, direct file | <https://theprabhanshu-dev.github.io/metrobalance/dmrc-proposal.html> |
| Source code (public repo) | <https://github.com/ThePrabhanshu-dev/metrobalance> |
| Proposal document (Markdown) | <https://github.com/ThePrabhanshu-dev/metrobalance/blob/prd/docs/DMRC_PROPOSAL.md> |

The deck link is the one to paste into the e-mail — it needs no login and no local setup.
Attach [`docs/DMRC_PROPOSAL.md`](docs/DMRC_PROPOSAL.md) (exported to PDF) as the formal proposal.

### Publishing to GitHub (UAT first, PRD as the main branch)

```powershell
# 1. one-time: authenticate
gh auth login

# 2. copy the deck into docs/ (for GitHub Pages)
powershell -ExecutionPolicy Bypass -File scripts\sync-docs.ps1

# 3. create the repo, push uat first, then prd, and enable Pages
powershell -ExecutionPolicy Bypass -File scripts\publish-github.ps1
```

The script creates the repository (if needed), wires up `origin`, pushes **`uat` first**, then pushes **`prd`**
and makes it the repository's **default branch**, pushes `main`, configures GitHub Pages to publish `docs/` via
[`.github/workflows/pages.yml`](.github/workflows/pages.yml), and prints the shareable links:

```
https://<user>.github.io/<repo>/              <- the deck (paste into the e-mail)
https://github.com/<user>/<repo>              <- the source code
```

Branches:

| Branch | Purpose |
| --- | --- |
| `uat` | UAT — Phase 1 of the DMRC rollout (integration against a sandbox feed). Pushed first. |
| `prd` | PRD — the main/default branch. Production-ready build, published to GitHub Pages. |
| `main` | Mirror of `prd`, kept for tooling that expects a `main` branch. |

`.github/workflows/pages.yml` deploys `docs/` to GitHub Pages on every push to `prd`.

---

## Features

* Premium responsive Bootstrap 5 UI (mobile-first, dark/light cards, sticky demo ribbon).
* Card number input with **client + server** validation (identical regex on both sides).
* Balance display in Indian Rupees (`₹ 1,23,456.00`, `en-IN` numbering).
* Recharge history timeline (amount, channel, mode, resulting balance).
* Journey transaction history table (source → destination, fare, time, entry/exit).
* "Last balance update" timestamp.
* Friendly error handling for invalid format, unknown card, blocked/expired card.
* **Any-card enquiry** — any well-formed number (demo *or* real) is accepted. Real cards get
  a card-kind diagnosis, the DMRC policy note, step-by-step AVM/counter instructions and
  official DMRC links — **never a fabricated balance**.
* **Balance-only mode** — a *Balance only* / *Full statement* switch on the homepage and on
  `/Card`. *Balance only* returns just the available amount in INR (plus the last-update
  stamp) via `GET /api/cards/{cardNumber}/amount`. For a real card it returns
  `balanceAvailableOnline = false`, `amount = null` and the DMRC policy note instead of a figure.
* No login required — public lookup.

---

## API endpoints

Base path: `/api/cards`

| Method | Route | Returns |
| --- | --- | --- |
| `GET` | `/api/cards/demo-numbers` | The 8 demo card numbers |
| `GET` | `/api/cards/{cardNumber}/balance` | Card holder, type, status, balance, last update |
| `GET` | `/api/cards/{cardNumber}/recharges` | Recharge history |
| `GET` | `/api/cards/{cardNumber}/transactions` | Journey transaction history |
| `GET` | `/api/cards/{cardNumber}/statement` | Balance + recharges + transactions in one call |
| `GET` | `/api/cards/{cardNumber}/enquiry` | **Any-card enquiry.** Demo card → full statement; real card → card-kind diagnosis + DMRC guidance (no balance) |
| `GET` | `/api/cards/{cardNumber}/amount` | **Balance only.** Available amount in INR + last-update stamp. Real card → `balanceAvailableOnline: false`, `amount: null` + policy note |

`/enquiry` never returns `404` for a well-formed number — that is the point of the endpoint:
every well-formed card number gets an answer. It only returns `400 INVALID_CARD_NUMBER`
when the format itself is unusable. `/amount` behaves the same way: `200` for any well-formed
number, `400 INVALID_CARD_NUMBER` for a malformed one.

### Balance-only response

```jsonc
// Demo card
{
  "cardNumber": "DMRC-DEMO-1001",
  "balanceAvailableOnline": true,
  "amount": 2725.50,
  "amountFormatted": "₹ 2,725.50",
  "currencyCode": "INR",
  "lastUpdatedOnFormatted": "25 Nov 2026, 02:17 AM",
  "isDemoData": true,
  "dataSource": "MetroBalance demo dataset",
  "message": "Available balance retrieved from the MetroBalance demo dataset (DEMO DATA)."
}

// Real card — no amount is ever invented
{
  "cardNumber": "8177769600175160",
  "balanceAvailableOnline": false,
  "amount": null,
  "amountFormatted": null,
  "currencyCode": "INR",
  "isDemoData": false,
  "message": "DMRC does not publish an online card balance service...",
  "policyNote": "DMRC does not publish a service that returns a card balance over the internet..."
}
```

All responses use a uniform envelope:

```jsonc
{
  "success": true,
  "message": "Statement retrieved successfully (DEMO DATA).",
  "data": { /* payload */ },
  "errorCode": null,
  "timestampUtc": "2026-11-26T05:33:14.1011829Z"
}
```

### Error handling

| Situation | HTTP | `errorCode` |
| --- | --- | --- |
| Malformed card number | `400` | `INVALID_CARD_NUMBER` |
| Card not in the demo store | `404` | `CARD_NOT_FOUND` |
| Card blocked / expired / not active | `409` | `CARD_NOT_ACTIVE` |
| Unexpected failure | `500` | `INTERNAL_ERROR` |

### Sample response

```json
{
  "success": true,
  "message": "Statement retrieved successfully (DEMO DATA).",
  "data": {
    "balance": {
      "cardNumber": "DMRC-DEMO-1001",
      "cardHolderName": "Aarav Sharma",
      "cardType": "Silver",
      "status": "Active",
      "balance": 2725.50,
      "balanceFormatted": "₹ 2,725.50",
      "lastUpdatedOn": "2026-11-25T00:03:05.0691849+05:30",
      "lastUpdatedOnFormatted": "25 Nov 2026, 12:03 AM",
      "issuedOn": "2024-08-14T11:03:05.0691849+05:30",
      "currencyCode": "INR",
      "isDemoData": true,
      "dataSource": "Simulated (DEMO DATA)"
    },
    "recharges": [
      {
        "transactionId": "RCH1101001",
        "timestamp": "2026-11-22T23:03:05.0691849+05:30",
        "timestampFormatted": "22 Nov 2026, 11:03 PM",
        "amount": 200,
        "amountFormatted": "₹ 200.00",
        "balanceAfter": 1025.50,
        "balanceAfterFormatted": "₹ 1,025.50",
        "mode": "Cash (FCB)",
        "channel": "Internet Banking",
        "remarks": "Online top-up"
      }
    ],
    "transactions": []
  },
  "errorCode": null,
  "timestampUtc": "2026-11-26T05:33:14.1011829Z"
}
```

---

## Demo card numbers

| Card number | Notes |
| --- | --- |
| `DMRC-DEMO-1001` | Active |
| `DMRC-DEMO-1002` | Active |
| `DMRC-DEMO-1003` | Active |
| `DMRC-DEMO-1004` | Active |
| `DMRC-DEMO-1005` | Active |
| `DMRC-DEMO-1006` | **Blocked** → `409 CARD_NOT_ACTIVE` |
| `DMRC-DEMO-1007` | **Expired** → `409 CARD_NOT_ACTIVE` |
| `DMRC-DEMO-1008` | Active |

Any other well-formed number → `404 CARD_NOT_FOUND`.

### Card-number format

Regex shared by `CardValidator` (C#) and `site.js` (JS) — **keep both in sync**:

```
^(?=.{6,19}$)[A-Za-z0-9]+([-\s][A-Za-z0-9]+)*$
```

6–19 characters, alphanumeric groups separated by a single hyphen or space.

> **Important:** the pattern is applied to the **normalised** value (trimmed, upper-cased,
> spaces removed) on **both** sides. So `8177 7696 0017 5160` is 19 raw characters but
> **16 digits**, and it validates. The browser input therefore allows 25 raw characters
> (`maxlength="25"`) to accommodate the separators — a 20-digit number is still rejected
> with *"Card number must be 6-19 characters…"*.

### Card-kind classification

`CardNumberClassifier` (Core) decides what kind of identifier was typed, so the site can give
the right advice instead of guessing:

| Shape | `CardNumberKind` | What the user is told |
| --- | --- | --- |
| Contains `DEMO` | `DemoCard` | Simulated data — full statement shown |
| 10–11 digits | `EngravedId` | The number printed on the **back** of a DMRC card; used for web top-ups |
| 12 digits | `SmartCardNumber` | The number printed on the **front** of a DMRC card |
| 13–19 digits | `BankStyleNumber` | Looks like a bank card — not usable for metro travel |
| Anything else | `Unknown` | Unrecognised format |

---

## Can this site show a *live* balance for a real card?

**No — and that is by design.** DMRC publishes no public card-balance service, so no free,
legal API exists to plug into.

Findings (read-only research, no scraping of any endpoint):

| Source | Result |
| --- | --- |
| DMRC customer FAQ (`dmrcsmartcard.com/FAQ.aspx`) | *"Can I check my card balance online?" → **"No, you cannot check your card balance online."*** Balance is read physically from the card at an **AVM** (Inquiry button) at the Customer Care / EFO counter. |
| `dmrcsmartcard.com/Citrus_Payment/QuickTopUp.aspx` | Recharge form accepts an **Engraved ID of max 11 digits** (numeric only) — that is the number printed on the back of a DMRC card. |
| `backend.delhimetrorail.com` (official site backend) | Returns **403 Forbidden** to direct/programmatic requests; no public Swagger, no balance endpoint. |
| `www.delhimetrorail.com` JS bundles | Contain content-page slugs only — no balance API path is exposed. |
| `www.dmrc.org` | Not resolvable / unreachable from general networks. |

Consequences for a real card number:

* The balance is **on the card**, not on a server. A number alone cannot yield a balance.
* A number like `8177 7696 0017 5160` (16 digits, 4‑4‑4‑4 grouping) is longer than the
  11‑digit maximum Engraved ID and matches a **bank/debit-card** layout, not a DMRC card.
  MetroBalance accepts the format and answers with **guidance, never a number**: the
  `GET /api/cards/{cardNumber}/enquiry` endpoint returns `found = false`,
  `balanceAvailableOnline = false`, the card-kind diagnosis, seven step-by-step
  instructions and seven official DMRC links.
* Official ways to read a real balance: tap the card on an **AVM** at any station and press
  *Inquiry*, or ask at the Customer Care / EFO counter. Recharge/web-top-up uses the
  Engraved ID via `dmrcsmartcard.com` or the official DMRC app, then the card is topped up
  at an AVM.

### What a user sees for a real card

Entering a real card number on the homepage or on `/Card` never produces a balance tile.
Instead the site shows:

1. A **"Real card — no online balance"** badge.
2. The **card-kind diagnosis** (Engraved ID / Smart Card number / bank-card-shaped).
3. The **DMRC policy note** — the reason no online balance exists, in plain language.
4. **Step-by-step instructions** to read the balance at an AVM, at the Customer Care
   counter, or via top-up + AVM.
5. **Official DMRC links** (top-up portal, smart-card portal, delhimetrorail.com, the
   Sarthi app, the Chetna chatbot and customer care).

The same content is permanently available, without entering anything, at **`/Home/Guide`**.

Privacy note: never publish a full bank card or smart-card number online — this project
logs no card numbers and stores nothing.

---

## DMRC integration — where to plug in the real API

The application depends on card data **only** through one interface:

```csharp
// src/MetroBalance.Core/Interfaces/IMetroCardProvider.cs
public interface IMetroCardProvider
{
    string ProviderName { get; }          // e.g. "Simulated (DEMO DATA)"
    bool IsSimulated { get; }             // true for the mock provider
    Task<SmartCard?> GetCardAsync(string cardNumber, CancellationToken ct = default);
    Task<IReadOnlyList<CardTransaction>> GetTransactionsAsync(SmartCard card, CancellationToken ct = default);
    Task<IReadOnlyList<CardTransaction>> GetRechargesAsync(SmartCard card, CancellationToken ct = default);
}
```

`MockMetroCardProvider` (in `MetroBalance.Infrastructure/Providers`) is the current implementation,
backed by `InMemoryCardStore`.

**To integrate an authorized DMRC API later:**

1. Add `DmrcMetroCardProvider : IMetroCardProvider` (put it in a new `MetroBalance.Dmrc` project).
2. Register it in `InfrastructureServiceExtensions` — replace the `MockMetroCardProvider`
   registration (or select it via configuration / environment).
3. Nothing else changes: controllers, services, UI and Swagger are provider-agnostic.

Guidelines that this project deliberately follows:

* No scraping of DMRC or any third-party site.
* No invented or "guessed" DMRC endpoint URLs.
* Real data must come from a **formally authorized** DMRC channel.
* `isDemoData` / `dataSource` are surfaced in every payload so the UI can always label the source.

---

## Architecture

```
Browser (site.js)
   │  fetch /api/cards/{card}/enquiry
   ▼
CardsController (MetroBalance.Api)          ← MVC + Web API, same endpoints
   │  ICardService
   ▼
CardService (MetroBalance.Infrastructure)    ← validation, mapping, formatting
   │  IMetroCardProvider
   ▼
MockMetroCardProvider → InMemoryCardStore   ← List / Dictionary, in-memory only
```

For a card number that is not in the demo store, `CardService.EnquireAsync` falls back to
`CardNumberClassifier` + `RealCardGuidanceFactory` (no provider call, no fabricated data).

* **Dependency injection** via `AddMetroBalanceInfrastructure()` and `AddMetroBalanceApi()`.
* **Swagger** registered in `ApiExtensions.AddMetroBalanceApi()` and served by `UseMetroBalanceApi()`.
* **camelCase JSON** configured for both minimal APIs and MVC controllers
  (`AddControllers().AddJsonOptions(...)`).
* Money formatted with `MoneyFormatter` (culture `en-IN`, literal `₹` prefix, `CurrencyCode = "INR"`).

---

## Verification performed

* `dotnet build MetroBalance.slnx` → **0 errors, 0 warnings**.
* Website host: `/`, `/Home/About`, `/Home/Faq`, `/Home/Guide`, `/Card?cardNumber=DMRC-DEMO-1001`,
  `/Card?cardNumber=8177%207696%200017%205160`, `/swagger/index.html` → `200`.
* API host: all 7 endpoints → `200`; error paths → `400` / `404` / `409` as documented.
  `/api/cards/{n}/enquiry` and `/api/cards/{n}/amount` return `200` for **any** well-formed
  number (demo or real) and `400 INVALID_CARD_NUMBER` only for a malformed one.
* Browser end-to-end: enter a card number → *Check balance* → balance, recharge timeline and
  journey table render; the *Total recharged* metric is populated from the recharge list.
* Browser end-to-end for a **real** card number (`8177 7696 0017 5160`, entered with spaces):
  the guidance panel renders with 7 steps and 7 official links and **no balance tile**; the
  statement section stays hidden. The same holds for the server-rendered `/Card/Lookup` POST.
* Browser end-to-end for **balance-only** mode: demo card → compact tile with `₹ 2,725.50` and
  *Last updated*; real card → policy note with **zero rupee symbols** in the result panel.
  Switching modes, submitting an invalid number, or looking up a real card after a demo card
  clears the previous figure (no stale balance survives in the DOM).
* Browser end-to-end for **statement** mode: demo card → balance tile + 4 recharge items + 12
  journey rows; a **real** card afterwards → guidance panel only, and the previous demo
  statement is fully cleared from the DOM (0 journey rows, 0 recharge items, `—` for balance,
  holder and card type) so no rupee figure from a demo lookup survives for a real card.
* Client and server validation agree on every case tested — 16-digit spaced number accepted,
  19 digits accepted, 20 digits rejected, `abc` rejected, empty rejected.
* Mobile viewport 390×844 → `0 px` horizontal overflow, before and after a lookup.

---

## Troubleshooting

| Symptom | Fix |
| --- | --- |
| `dotnet build` fails with *"scoped css bundles do not have matching endpoints"* | A Web-SDK project is referencing another Web-SDK project. Keep the shared API code in the `MetroBalance.Api` **class library** and host it via `MetroBalance.ApiHost`. |
| `The solution file 'MetroBalance.sln' does not exist` | Use `MetroBalance.slnx` (the SDK 11 XML solution format). |
| `dotnet sln add` fails with *"Solution folder 'src' already contains…"* | Edit the `.slnx` XML directly, or add projects one at a time. |
| Swagger page blank | Ensure `app.UseMetroBalanceApi()` is called before routing/endpoint mapping. |
| Card lookup returns 404 for a demo card | Card numbers are case-sensitive: use `DMRC-DEMO-1001`. |
| Build fails with *"file is locked"* | Stop the running `dotnet run` process before rebuilding. |

---

## License

Provided as-is for educational/demo use. Not affiliated with, endorsed by, or connected to
Delhi Metro Rail Corporation (DMRC).
