# MetroBalance — Proposal to Delhi Metro Rail Corporation (DMRC)

**Submitted by:** Prabhanshu Shukla (MetroBalance)
**Version:** 1.0 · November 2026
**Status:** Independent proposal for evaluation — not a commercial quotation
**Companion assets:** live demonstration app, Swagger API, 13-slide showcase deck, full source code

---

## 1. Executive summary

Riders of the Delhi Metro cannot check their Smart Card balance online. DMRC's own public FAQ states plainly that a card
balance cannot be checked over the internet, so the balance is only visible at an Add Value Machine (AVM) or a staffed
counter — and, for most riders, only as a side-effect of a gate rejecting a journey.

**MetroBalance** is a complete, working web application that demonstrates what such a service should look like:

* a public, **read-only** Smart Card enquiry surface — balance, recharge history and journey ledger, in Indian Rupees;
* **no login, no app install, no registration** for the rider;
* **no database and no paid dependency** — .NET 10 ASP.NET Core MVC + Web API, Bootstrap 5, Swagger;
* an architecture with a **single replaceable seam** (`IMetroCardProvider`) so that an authorised DMRC data feed can be
  plugged in later without touching the UI, the API or the business logic;
* an **honesty guarantee**: the application never fabricates a balance, never scrapes any website and labels every
  simulated figure as demo data.

We are asking DMRC for four things — a 30-minute review, a named technical owner, a sandbox credential or sample
ledger extract, and approval for a limited pilot. We bring the working application, the source code, and all future
maintenance at **zero cost and zero commercial terms**.

---

## 2. The problem

| # | Problem | Consequence for the rider | Consequence for DMRC |
|---|---------|--------------------------|----------------------|
| 1 | No online balance service (DMRC FAQ: *"No, you cannot check your card balance online."*) | Riders must find an AVM or a counter to learn a balance they already paid for | Repeat counter and call-centre interactions for a trivial query |
| 2 | Top-ups are invisible until a gate rejects the journey | Fare deduction becomes the only "notification"; refunds and short top-ups are discovered late | More disputes and grievances at the counter |
| 3 | No self-service reconciliation of top-ups against journeys | Students, senior citizens, tourists and daily commuters cannot audit their own card | No visibility of rider-side confusion |
| 4 | Card-type rules (silver, auto top-up, concession) are hard to check remotely | Riders over-top-up to be safe, increasing unspent balances | Working capital stays with the system rather than the operator |
| 5 | Every modern transit system in the world (and every Indian city metro under construction) expects a digital card | Delhi Metro's card experience lags digital expectations | Positions DMRC as less rider-friendly than peer systems |

**Opportunity:** an enquiry layer is not a new system of record. DMRC already records every top-up and every fare
deduction in its back office and every balance is already computed at the gates. The gap is only the *read-only
presentation* of that data to the rider.

---

## 3. The proposed solution

### 3.1 What riders get

1. **Balance in one tap** — type the card number, read the available balance in rupees plus the last-update timestamp.
   No login, no OTP, no app.
2. **Full statement** — recharge history (amount, channel, mode, resulting balance) and a complete journey ledger
   (date, from station, to station, line, fare slab) so a rider can reconcile their own card.
3. **Guidance for real cards** — when DMRC publishes no online balance, the application says so and links to the official
   AVM / counter / FAQ channels instead of inventing a figure.

### 3.2 What DMRC gets

* A **ready-made, white-label enquiry surface** that can be re-skinned to DMRC branding and embedded on the official
  website or hosted inside DMRC's own environment.
* A **documented, versioned REST API** (`/api/cards/{cardNumber}/…`) that any DMRC app, kiosk or chatbot can consume.
* **Zero cost** — no licence fee, no per-transaction fee, no hardware, no rider-side app, no paid SaaS.
* A **provable privacy posture** — no card data stored, no personal data collected, no scraping, no shadow IT.

### 3.3 What we are explicitly *not* asking for

* No exclusivity, no tender bypass, no pre-emption of DMRC's own digital initiatives.
* No use of the DMRC logo or wordmark without written permission.
* No rider data sold, shared or monetised — ever.
* No scraping of DMRC, dmrcsmartcard.com or any partner website.
* No attempt to read card chips, station systems or DMRC access controls.

---

## 4. Solution architecture

```
                 ┌────────────────────────────────────────────┐
   Rider  ──►   │  MetroBalance.Web  (ASP.NET Core MVC)      │  Bootstrap 5, mobile-first
                 │  Home · /Card · /showcase · /guide · FAQ    │
                 └───────────────────┬────────────────────────┘
                                     │ HTTP (same-origin)
                 ┌───────────────────▼────────────────────────┐
                 │  MetroBalance.Api  (ASP.NET Core Web API)  │  7 endpoints, Swagger/OpenAPI
                 │  uniform envelope + 400/404/409 mapping     │
                 └───────────────────┬────────────────────────┘
                 ┌───────────────────▼────────────────────────┐
                 │  MetroBalance.Core                         │  domain models · DTO contracts
                 │  ICardService · IMetroCardProvider         │  validation · exceptions
                 └───────────────────┬────────────────────────┘
                 ┌───────────────────▼────────────────────────┐
                 │  MetroBalance.Infrastructure               │  MockMetroCardProvider
                 │  in-memory List<T> / Dictionary<K,V>       │  demo data factory
                 └────────────────────────────────────────────┘
```

### 4.1 The DMRC seam

`IMetroCardProvider` is the **only** door to card data:

```csharp
public interface IMetroCardProvider
{
    string ProviderName { get; }
    bool IsSimulated { get; }
    Task<SmartCard?> GetCardAsync(string cardNumber, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetDemoCardNumbersAsync(CancellationToken ct = default);
}
```

Today it is implemented by `MockMetroCardProvider` (in-memory, clearly-labelled demo data). When an authorised feed is
granted we add `DmrcMetroCardProvider`, register it in the DI container and flip one configuration flag. **No controller,
view or service code changes** — and the public API contract stays byte-for-byte identical.

### 4.2 API contract (integration surface)

| Endpoint | Purpose |
|---|---|
| `GET /api/cards/{cardNumber}/balance` | Available balance + last update |
| `GET /api/cards/{cardNumber}/recharges` | Recharge / top-up history |
| `GET /api/cards/{cardNumber}/transactions` | Journey fare deductions |
| `GET /api/cards/{cardNumber}/statement` | Everything in one payload |
| `GET /api/cards/{cardNumber}/enquiry` | Card status + guidance for real cards |
| `GET /api/cards/{cardNumber}/amount` | Balance-only answer (`amount` or `null`) |
| `GET /api/cards/demo-numbers` | Sample card numbers for evaluation |

Envelope: `{ success, message, data, errorCode, timestampUtc }` · HTTP `400` invalid card, `404` unknown card,
`409` blocked/expired card.

---

## 5. Integration options — we need only one

| Option | Description | Effort | Backend change |
|---|---|---|---|
| **A — Authorised read API** *(preferred)* | DMRC or its nominated processor exposes a read-only balance/transaction endpoint (mTLS or signed requests). MetroBalance becomes a pure consumer. | Medium | One new `IMetroCardProvider` implementation |
| **B — Settlement / ledger feed** | DMRC's back office already records top-ups and fare deductions. A scheduled export (CSV/JSON/SFTP/API) lets the service answer "what was left" without touching the card or the station network. | Low–medium | Same as A, plus a scheduler |
| **C — AVM / kiosk deep link** | A QR code or NFC tag at the machine; the browser reads what the machine already displays and the app renders it. | Low | None — front-end only |
| **D — Guided self-service** *(available today)* | The app surfaces DMRC's own FAQ channels: AVM locations, counter guidance, recharge instructions, in the rider's language. No live data required. | **Zero** | None — already implemented |

Options A and B are server-side and fit the existing architecture exactly. Options C and D deliver value immediately
while A is being formalised. **We are happy to start with D and pilot A.**

---

## 6. Security, privacy and compliance

* **No card data stored.** The demonstration build holds nothing; a production build is stateless per request.
* **No personal data.** Only the card number typed by the rider is processed — no name, no phone number, no OTP.
* **No scraping.** Nothing is read from any DMRC, partner or third-party website.
* **Transparent labelling.** Every response states whether it is demo or live; a real card never carries a fabricated amount.
* **Anti-forgery** tokens on form posts; uniform error codes; no stack traces or internals in client responses.

Controls proposed for a DMRC production deployment:

* request rate-limiting and per-IP throttling on the enquiry endpoints;
* TLS-only, HSTS, standard security headers, strict Content-Security-Policy;
* anti-forgery tokens plus an audit trail of enquiry requests;
* no card identifiers in logs — truncation and hashing where retention is required;
* data minimisation aligned with the **Digital Personal Data Protection Act, 2023**;
* DMRC-hosted or DMRC-approved hosting with an agreed retention and deletion policy;
* security testing (SAST/DAST) and an annual penetration test, at our cost.

---

## 7. Delivery plan

| Phase | Duration | Outcome | Needs from DMRC |
|---|---|---|---|
| **Phase 0 — Now** | Live today | This demonstration build, the API, the showcase deck and guided self-service content for real cards | Nothing — just a look |
| **Phase 1 — UAT** | 4–6 weeks | Authorised sandbox feed (option A) or ledger export (option B); DMRC-branded pilot on a limited card range | A named owner and a test credential or sample file |
| **Phase 2 — Pilot** | 6–8 weeks | DMRC-hosted environment, rate limits, monitoring, audit reporting; rider feedback loop | Approval to pilot and to publish |
| **Phase 3 — PRD** | 8–12 weeks | Production release: full integration, SLA, support model, documented run-book | Production access and sign-off |

**Commercials:** no licence fee, no per-use cost, no rider cost, no hardware. DMRC may host, fork, extend or discontinue
the service at any time.

---

## 8. Governance

* DMRC approves branding, wording and placement before anything goes live.
* A written agreement on data scope, retention, audit and support precedes enabling the feed.
* DMRC may require the service to run inside its own environment; the codebase is MIT-licensed and auditable.
* DMRC may require disclosure that the service is a third-party initiative, or an official “in partnership with” badge.
* We will not approach any third party with DMRC data, and we will not use DMRC data for any purpose other than the
  service DMRC approves.

---

## 9. The ask

1. **A 30-minute review** — walk through the live build and the deck; tell us what a rider should see.
2. **A technical owner** — someone in IT & Automation, Customer Services or Digital Initiatives to point us at the
   correct data path and the correct approval route.
3. **A sandbox** — a test credential (option A) or a sample ledger extract (option B) so we can prove the integration
   end to end.
4. **A pilot** — approval to run a limited, DMRC-branded pilot, with no public launch without sign-off.

---

## 10. Why now

The application is already built, already honest and already architected for an authorised feed. The only missing pieces
are **permission** and a **data path** — and both of those only DMRC can grant. Every month of delay is another season
of riders queuing at machines to read a number they already know.

---

### Annex A — Demonstration assets

| Asset | Location |
|---|---|
| Live web application | `/` |
| Balance-only result | `/Card?cardNumber=DMRC-DEMO-1001&mode=amount` |
| Full statement | `/Card?cardNumber=DMRC-DEMO-1001` |
| Real-card guidance | `/guide` |
| Showcase deck (in-app) | `/showcase` |
| Showcase deck (standalone, offline) | `/presentations/dmrc-proposal.html` |
| Interactive API docs | `/swagger` |
| Source code | repository link supplied with this proposal |

### Annex B — Disclaimer

MetroBalance is an independent demonstration project. It is not affiliated with, endorsed by, or connected to DMRC or the
Delhi Metro Rail Corporation. All balances, recharges and journeys shown in the demonstration are simulated samples
labelled as demo data and are not real account records.
