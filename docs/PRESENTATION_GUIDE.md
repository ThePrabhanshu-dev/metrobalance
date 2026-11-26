# Presentation guide — MetroBalance for DMRC

**Deck:** `/presentations/dmrc-proposal.html` (standalone, single file, works offline — e-mail it as an attachment or open
it from disk) · **In-app version:** `/showcase` · **PDF:** press `P` in the deck, or use the "Print / export to PDF"
button on `/showcase`.

**Controls:** `←` `→` or `Space` to navigate · `F` fullscreen · `P` print/PDF · `Home` / `End` first/last slide ·
click the dots to jump · deep links like `#7` open a specific slide.

**Total run time:** 12–15 minutes for the deck + 10–20 minutes of demo + Q&A.
**Total meeting:** budget **30 minutes**. If you only get 5 minutes, show slides 1 → 2 → 5 → 12.

---

## Before you present (15-minute checklist)

- [ ] Deck opens on the presentation laptop with `F` fullscreen (no browser chrome).
- [ ] Live app reachable at `https://theprabhanshu-dev.github.io/metrobalance/demo/` — open it in a second tab and pre-select the demo card **DMRC-DEMO-1001** (the chips under the input make this one click).
- [ ] Internet fallback ready: the deck is fully offline-capable; the live demo needs the network.
- [ ] API tab pre-loaded at `https://theprabhanshu-dev.github.io/metrobalance/demo/#api` (it shows the real JSON the application returns). If you are running the .NET build locally, the interactive Swagger UI is at `http://localhost:5080/swagger`.
- [ ] PDF copy of the deck attached to the e-mail (in case they want to forward it).
- [ ] One-page proposal (`docs/DMRC_PROPOSAL.md` exported) in your folder as a printed backup.
- [ ] You have read [DMRC_CONTACTS.md](DMRC_CONTACTS.md) — know who is in the room and what they own.
- [ ] Say the words "**demo data**" out loud at least three times during the demo.

---

## Slide-by-slide speaker notes

### Slide 1 — Cover · "MetroBalance · Proposal to Delhi Metro Rail Corporation" (30 s)
> "Good morning. I'm Prabhanshu, a .NET developer in Delhi. In the next twelve minutes I'll show you a working
> application that gives any rider their Smart Card balance, their recharge history and their journey ledger — on any
> phone, without a login, without an app — and then show you exactly what I'd need from DMRC to make it real."

*Do not* read the four KPI tiles aloud. Move on.

### Slide 2 — "Riders cannot see their balance without touching a machine" (90 s)
The heart of the pitch. Quote DMRC's own FAQ **verbatim** and say it is their published position:
> *"No, you cannot check your card balance online."*

Then: "I want to be very clear — I am not proposing to scrape that. Scraping is not on the table. I read your FAQ and
concluded the opposite: **there is no online interface today, so a live feature needs an authorised data path from
DMRC.** That is why this is a request for a conversation, not a demo of something already built on your data."

Mention the flow of pain: top up → forget → gate rejects → queue at the AVM.

### Slide 3 — "One public enquiry surface for every Smart Card" (60 s)
Three points, one per column: **balance** (₹, to the paisa) · **history** (recharges + journeys) · **no friction**
(no login, no app, no OTP). Stress that DMRC's own FAQ notes all smart cards are anonymous — so a card-number-only,
no-identity enquiry is exactly right for this system.

### Slide 4 — "What riders actually see" (60 s)
Narrate the UI as a rider would see it on a phone: type card number → see ₹ balance and last-updated time → expand to
recharges and journey ledger. Point at the persistent **DEMO DATA** banner and say it is deliberate.

### Slide 5 — "See it working — in under a minute" (2–3 min) ⚡ *the live demo*
Switch to the browser tab with the app. Say the demo card number out loud and type it. Show:
1. `/Card?cardNumber=DMRC-DEMO-1001` → balance + last update;
2. expand **recharge history** → top-ups with dates and amounts;
3. expand **journey transactions** → from/to stations, line, fare;
4. `/Card?cardNumber=DMRC-DEMO-1001&mode=amount` → the balance-only answer;
5. a **real-looking card number** → the app explains it cannot show a balance and points to official channels.

Then say the line that matters: *"For a card that isn't a demo card, this application shows no rupees at all. It will not
guess."*

### Slide 6 — "Clean layers, one replaceable seam" (60 s)
Three layers: **MVC UI → Web API → Core services**. The seam is `IMetroCardProvider`. Today: `MockMetroCardProvider`
(in-memory). Tomorrow: `DmrcMetroCardProvider` — **no UI, API or business-logic change; the contract stays identical.**
This is the slide that proves you are an engineer, not a designer.

### Slide 7 — "A small, stable API — ready for a DMRC feed" (45 s)
Point at the endpoints. Say: seven endpoints, one envelope, Swagger-documented, and *the API contract does not change
when the data source changes.* Offer to share the OpenAPI document.

### Slide 8 — "Privacy and safety are designed in, not bolted on" (60 s)
No card data stored, no personal data collected, no scraping, transparent demo labelling, rate limiting and security
headers proposed for production, DPDP Act 2023 alignment, and DMRC-hosted deployment offered. Quote the hyperlinking
clause you found in their privacy policy and state you will not link to their site without permission.

### Slide 9 — "Four paths to live data — we need only one" (90 s) ⚡ *the credibility slide*
Walk A → D quickly, then dwell on **A (authorised read API)** and **B (ledger/settlement feed)**, because those are the
ones DMRC's IT team can actually decide on. Emphasise **D (guided self-service)** — the app pointing riders to official
AVM/counter/FAQ channels — is deliverable **today with zero permissions**. Then say: *"I don't need all four. I need one
of A or B, and I'd rather have D live this month than A in six months."*

### Slide 10 — "What we will never do" (45 s)
Read the list aloud — it is the fastest way to build trust. Scrape DMRC. Invent endpoints. Show a balance for a real
card. Use the DMRC logo without permission. Link to DMRC's site without permission. Sell rider data. All of it, out
loud, in your own words.

### Slide 11 — "A staged rollout that needs no budget up front" (60 s)
Phase 0 (now, nothing needed) → **Phase 1 UAT** (sandbox credential or sample ledger, 4–6 weeks) → Phase 2 pilot
(DMRC-hosted, rate-limited, audited) → **Phase 3 PRD** (production, SLA, run-book). Say the commercial position out
loud: **no licence fee, no per-use fee, no hardware, no rider app, and DMRC can fork, host or switch it off at any time.**

### Slide 12 — "What we are asking DMRC for" (45 s)
Read the four asks exactly as written: 30 minutes · a named technical owner · a sandbox credential or sample file · a
pilot approval. Say: *"That is the whole ask. Everything else I will do."*

### Slide 13 — "Let's make the balance visible" (20 s)
Close and hand over:
> "The prototype, the deck, the API docs and the source code are in the e-mail. If it is useful, I can have a sandbox
> integration ready in two weeks. If it is not, I am happy to leave the app running for your team to try."

Then **stop talking** and let them respond.

---

## Q&A — the ten questions you will get, and how to answer

**Q1. "DMRC already has the Sarthi app — why do we need yours?"**
> "The Sarthi app is DMRC's own app, and that is exactly the point: if DMRC wants this capability, the right home for it
> is inside Sarthi. My proposal is the read-only enquiry layer and its API — I am happy for it to be integrated into
> Sarthi, embedded on the DMRC website, or hosted independently. It is not a competing app; it is a component."

**Q2. "How do you propose to get the balance data without scraping?"**
> "I don't — that is what the meeting is for. DMRC's back office already records every top-up and every fare deduction.
> The options are an authorised read API, or a periodic ledger export that we compute the balance from. Both need
> DMRC's approval and credentials. Until then the app shows nothing real, and says so."

**Q3. "Why should DMRC trust a third party with card data?"**
> "You should not have to. That is why option A and option B are designed so DMRC controls the data: your servers, your
> credentials, your rate limits, your logs. We are asking for a sandbox, not for production data. And the code is
> MIT-licensed — you can audit it, host it yourself, or throw it away."

**Q4. "Can you get this live without us?"**
> "No. That is the honest answer, and it is the right one. Everything that shows a real balance requires your
> permission and your data path. What I can do without you is the guided self-service version that points riders to the
> official channels — I would still like your guidance on the wording."

**Q5. "What happens to the ₹3,000 cap, the 180-day AVM rule and the 11:45 PM–12:15 AM maintenance window?"**
> "Those are business rules in the integration layer, not in the UI — my architecture keeps DMRC's rules configurable so
> the service never contradicts DMRC's published policy. I have noted them from your FAQ and would want the authoritative
> list from you."

**Q6. "Is this a revenue-share or commercial proposal?"**
> "No. I am not asking for money, a contract or exclusivity. I am asking for a data path and a pilot. If DMRC later
> wants a commercial model, that is a separate conversation and I would be happy to have it."

**Q7. "Who funds and maintains it?"**
> "I do, at my own cost, as an independent contribution. Once it is live, the maintenance model — SLA, support hours,
> security testing — is a line item in the proposal for Phase 3, and it can be zero, or DMRC can simply take it over."

**Q8. "What about the digital payments and QR tickets?"**
> "Out of scope for me — I am not touching payments, refunds or the recharge flow, only the enquiry side. The whole
> point is to keep the blast radius small."

**Q9. "Can you show it on a phone?"**
> "Yes — it is mobile-first." *(Hand over your phone / open on mobile. This is a live demo point; do it every time.)*

**Q10. "What if the data is wrong?"**
> "In the pilot, the source of truth is DMRC's own system; the service only reads. Wrong data can only come from a
> wrong feed, which is exactly why Phase 1 is a sandbox and Phase 2 is a monitored pilot with an audit trail before any
> public launch."

**If you don't know an answer:** "I don't know — I'll get it in writing and revert." Then actually revert. It is far
better than improvising on a system that runs a city.

---

## One-page cheat sheet (print this)

| Slide | One-line message | Time |
|---|---|---|
| 1 | Cover — who and what | 0:30 |
| 2 | DMRC's own FAQ says balance can't be checked online → an authorised data path is needed | 1:30 |
| 3 | One enquiry surface: balance, history, no login | 1:00 |
| 4 | What the rider sees | 1:00 |
| 5 | **Live demo** | 2:30 |
| 6 | One replaceable seam (`IMetroCardProvider`) | 1:00 |
| 7 | Small, stable, documented API | 0:45 |
| 8 | Privacy and safety by design | 1:00 |
| 9 | Four integration paths; need only one | 1:30 |
| 10 | What we will never do | 0:45 |
| 11 | UAT → PRD rollout, zero budget up front | 1:00 |
| 12 | The four asks | 0:45 |
| 13 | Close | 0:20 |

**Total ≈ 13 minutes.** Then Q&A.

**Three sentences to memorise:**

1. "Your FAQ says a balance can't be checked online — so I'm not here to scrape. I'm here to ask for the data path."
2. "Every number in this demo is simulated and labelled as such. For a real card, the app shows no amount at all."
3. "The ask is four things: 30 minutes, a name, a sandbox, and a pilot. Everything else I do, for free."
