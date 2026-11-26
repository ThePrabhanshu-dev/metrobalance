# DMRC Outreach — e-mail templates

**Purpose:** get MetroBalance in front of the right DMRC desk, and convert it into a 30-minute technical discussion.
**Tone:** short, factual, non-salesy, engineering-first. DMRC officials read dozens of vendor pitches; the ones that get
opened are the ones that read like an engineer who did the homework.

---

## 0. Before you send — fill these in

| Token | What it is | Value (already filled in) |
|---|---|---|
| `{{DECK_URL}}` | Shareable link to the 13-slide deck (GitHub Pages) | `https://theprabhanshu-dev.github.io/metrobalance/` |
| `{{LIVE_URL}}` | Live application URL — fill this with wherever you host the app, or present from `http://localhost:5080` | `https://<your-host>/` |
| `{{REPO_URL}}` | Source-code repository | `https://github.com/ThePrabhanshu-dev/metrobalance` |
| `{{PDF_ATTACH}}` | The deck exported to PDF | `MetroBalance-DMRC-Proposal.pdf` |
| `{{CPPP_ID}}` | Your CPPP / eTenders vendor registration ID (once registered) | `…` |
| `{{YOUR_NAME}}`, `{{YOUR_PHONE}}`, `{{YOUR_EMAIL}}` | Your details | |
| `{{DECK_PATH}}` | Where the deck is hosted, for the offline copy | `https://theprabhanshu-dev.github.io/metrobalance/dmrc-proposal.html` |

> The deck link is public, needs no login and renders offline — **send that first**. Everything else is optional.

**Links to paste** (all verified working locally):

* Deck (in-app): `/showcase`
* Deck (standalone, offline, single file): `/presentations/dmrc-proposal.html`
* Live balance check: `/Card?cardNumber=DMRC-DEMO-1001`
* Balance-only answer: `/Card?cardNumber=DMRC-DEMO-1001&mode=amount`
* Real-card guidance: `/guide`
* Interactive API docs: `/swagger`
* Statement API sample: `/api/cards/DMRC-DEMO-1001/statement`

**Checklist before the first send**

- [ ] Deck link opens on a phone (test in incognito).
- [ ] `{{LIVE_URL}}` returns 200 and the balance demo works.
- [ ] The e-mail does **not** claim any live balance is real.
- [ ] Attach the PDF as well as the link (some officials block links).
- [ ] The body is under ~220 words — long e-mails get skimmed, not read.
- [ ] You have read [DMRC_CONTACTS.md](DMRC_CONTACTS.md) and re-verified the recipient's e-mail today.
- [ ] Send **one recipient at a time**, from a real (non-free-mail) address, with your name in the signature.
- [ ] Send Tuesday–Thursday, 09:30–11:00 IST.
- [ ] Note the send date + recipients in a tracker so the follow-up is not generic.

---

## 1. Subject-line options

Pick one. The first three are the most reliable.

1. `Working demo: read-only Delhi Metro Smart Card balance enquiry service — request for a 30-minute technical review`
2. `Proposal: an authorised Smart Card balance & history enquiry service for DMRC riders (UAT-ready)`
3. `Smart card balance enquiry — DMRC FAQ says it can't be done online; here is a working prototype and the data path we're asking for`
4. `MetroBalance — free, no-login Smart Card balance enquiry (UAT + PRD proposal, zero cost to DMRC)`
5. `Invitation to evaluate a rider-facing Smart Card enquiry service under DMRC's Innovation Policy (20.11.2025)`

Avoid: *"URGENT"*, *"Best solution"*, *"10x growth"*, *"free forever"*, anything with an attachment over 5 MB.

---

## 2. Email A — primary outreach (to ED/IT, copy AFC + BD) ⭐ recommended

**To:** `vkgupta@dmrc.org` · **Cc:** `ykapil@dmrc.org`, `dir.bd@dmrc.org` · **Subject:** option 1 or 2 above

> Dear Mr. Gupta,
>
> I am a .NET developer in Delhi. I have built a working prototype of a **read-only Smart Card balance and history
> enquiry service** for DMRC riders, and I am writing to request a 30-minute technical review and the name of the right
> team to engage with.
>
> **The problem, in DMRC's own words.** The Smart Card FAQ states: *"No, you cannot check your card balance online."*
> Riders must find an Add Value Machine to learn a balance they have already paid for — and the balance is usually
> discovered only when a gate rejects the journey.
>
> **What I have built.** A public web service (no login, no app, no database) that shows a card's balance in rupees,
> its recharge history and its journey ledger. It is running today, and the whole thing can be reviewed in one link:
>
> • 13-slide proposal deck (with a live walkthrough inside): {{DECK_URL}}
> • Live application: {{LIVE_URL}}
> • API documentation: {{LIVE_URL}}/swagger
> • Source code (MIT-licensed, public): {{REPO_URL}}
> • Offline copy of the deck (single HTML file): {{DECK_PATH}}
>
> Every figure in the demo is clearly labelled **DEMO DATA**. I have not scraped, queried or reverse-engineered any
> DMRC system, and the application never displays an amount for a real card.
>
> **What I am asking for.** Not a contract and not funding — four things:
>
> 1. 30 minutes with the IT/AFC team to confirm whether this addresses a real priority;
> 2. the correct approval path (I note DMRC's Innovation Policy dated 20.11.2025, and I am happy to register on CPPP as
>    a vendor);
> 3. a **sandbox** credential or a sample ledger extract, so the integration can be proven end to end;
> 4. approval to run a limited pilot with a read-only feed, if the concept is sound.
>
> The service would be **free to DMRC and to riders** — no licence fee, no per-use cost, no hardware, no rider app, and
> no personal data collected (card numbers are anonymous by design, as DMRC itself states). The code is MIT-licensed and
> can be hosted, audited, forked or discontinued by DMRC at any time.
>
> I have also attached a one-page summary (deck PDF). If a short call is easier than a review, I am happy to send the
> agenda in advance.
>
> Thank you for your time.
>
> Warm regards,
> **{{YOUR_NAME}}**
> {{YOUR_EMAIL}} · {{YOUR_PHONE}}
> {{LIVE_URL}}

---

## 3. Email B — commercial framing (to Director, Business Development)

**To:** `dir.bd@dmrc.org` · **Cc:** `vkgupta@dmrc.org`, `himangshu@dmrc.org`

> Dear Dr. Garg,
>
> I am reaching out with a rider-experience initiative for DMRC that could also open a **zero-cost digital channel**.
>
> DMRC's Smart Card FAQ says plainly that a card balance cannot be checked online, so riders queue at machines to read a
> number they already know. I have built a complete, working **Smart Card enquiry service** — balance, recharge history
> and journey ledger, in rupees, on any phone, with no login and no app. It is live today: {{LIVE_URL}}, with a
> 13-slide proposal here: {{DECK_URL}} and the source code at {{REPO_URL}}.
>
> Why it belongs with Business Development: DMRC already permits named third parties to sell on its channels (Delhi
> Metro Sarthi App, Paytm, PhonePe, One Delhi, Amazon, IRCTC). This is the **read-only enquiry** counterpart — it adds no
> payment flow, no revenue share, no data risk and no operational burden to DMRC; the rider simply gets to see their own
> card.
>
> The commercial terms proposed are deliberately simple: **DMRC pays nothing**, the service is free to riders, and DMRC
> may host it, re-brand it, take it over or shut it down. All that is requested is an authorised data path and a pilot
> approval.
>
> If this is not the right desk, could you point me to the one that is? I would be grateful.
>
> With thanks,
> **{{YOUR_NAME}}** · {{YOUR_PHONE}} · {{LIVE_URL}}

---

## 4. Email C — PR framing (only after a technical owner has replied)

**To:** `himangshu@dmrc.org`

> Dear Mr. Sarma,
>
> I am following up on a proposal I sent to DMRC's IT and AFC teams regarding a rider-facing Smart Card balance enquiry
> service (working demo: {{LIVE_URL}}; short deck: {{DECK_URL}}).
>
> I am writing to you only because the service has a genuine public-interest angle that may suit DMRC's communications
> work: it gives every rider a **free, no-login way to check their own card balance and journey history**, in line with
> the digital-first direction of the Delhi Metro Sarthi App and the Sarthi/WhatsApp channels.
>
> I am **not** seeking media coverage at this stage — the project is with the technical team and I would not want it
> announced before DMRC has reviewed it. If the initiative is of interest, I would be glad to send the one-page
> summary; if it is not, I will not follow up again.
>
> Thank you for your consideration.
>
> Regards,
> **{{YOUR_NAME}}** · {{YOUR_PHONE}}

---

## 5. Email D — the short "meeting request" (send if Email A is ignored)

**Subject:** `30 minutes on a DMRC Smart Card balance enquiry service?`

> Dear Mr. Gupta,
>
> Following my note of {{DATE}}, I would like to put a concrete 30-minute slot to your team.
>
> **Proposed agenda (30 min):**
> 1. 5 min — the rider problem, using DMRC's published FAQ.
> 2. 10 min — live walkthrough of the working application and the API.
> 3. 10 min — where an authorised data feed would connect (the architecture is already seam-ready).
> 4. 5 min — what a UAT pilot would look like, and what it would cost DMRC (nothing).
>
> The build, the API documentation and the source code are available here: {{LIVE_URL}} · {{DECK_URL}} · {{REPO_URL}}.
>
> Would any of {{TWO_OR_THREE_SLOTS}} suit you? I am happy to work around your team's timings, and to send the agenda
> in advance.
>
> Regards,
> **{{YOUR_NAME}}** · {{YOUR_PHONE}}

---

## 6. Email E — 7-day follow-up

**Subject:** `Re: {{ORIGINAL_SUBJECT}}`

> Dear Mr. Gupta,
>
> A brief follow-up to my note of {{DATE}} — I know an unsolicited proposal is easy to lose.
>
> Two things that may help it reach the right person:
>
> • The prototype is **already running** — no funding, no vendor, no agency is required to evaluate it. A single
>   sandbox credential (or a sample ledger export) is enough to prove the integration end to end.
> • Everything is **explicitly labelled demo data**. The service never shows a balance for a real card, never scrapes
>   DMRC, and never links to DMRC's website without permission.
>
> If it is not a priority, I would genuinely appreciate a one-line "not now" so I stop taking up your inbox.
>
> Thank you,
> **{{YOUR_NAME}}** · {{YOUR_PHONE}} · {{LIVE_URL}}

---

## 7. Email F — final / "close the loop" (day 21)

**Subject:** `Closing the loop on my DMRC Smart Card proposal`

> Dear Mr. Gupta,
>
> I am closing the loop on my proposal of {{DATE}} about a read-only Smart Card balance enquiry service.
>
> The working prototype and the source code remain available at {{LIVE_URL}} and {{REPO_URL}}, and I am happy to revisit
> this whenever the timing is better — for example, alongside the next AFC tender cycle, a DMRC Innovation Policy intake,
> or a pilot with the AFC team.
>
> Thank you for your time.
>
> Regards,
> **{{YOUR_NAME}}** · {{YOUR_PHONE}}

---

## 8. Short versions (LinkedIn / DM / in-person card)

**LinkedIn connection note (≤ 300 characters):**

> Built a working Delhi Metro Smart Card balance enquiry service (no login, no app, free). DMRC's own FAQ says balance
> can't be checked online — I'd like 30 minutes with the IT/AFC team to discuss an authorised data path. Demo: {{LIVE_URL}}

**WhatsApp/SMS (first message, then stop):**

> Hello — I'm a developer in Delhi. I built a working free web service that shows a Delhi Metro Smart Card balance and
> history (no login, no app). DMRC's FAQ says this can't be checked online, so I'm requesting a 30-minute technical
> review. Demo: {{LIVE_URL}} — thank you.

**At a station / counter (verbal, to staff):**

> I'm a developer working on a rider-facing Smart Card enquiry service. I have a working prototype and I'm trying to reach
> the IT and AFC teams at Metro Bhawan. Could you point me to the correct way to submit a technology proposal — a portal,
> a form, or an e-mail address?

---

## 9. Do-nots (these will cost you the meeting)

| Never do this | Why |
|---|---|
| Claim any balance is real | DMRC can disprove it in one click; your credibility is gone |
| Scrape or describe scraping | Explicitly forbidden in the proposal and illegal under the terms you accept |
| Use the DMRC logo or "Delhi Metro" in your product name/branding | Copyright clause — written permission required |
| Link from your app to delhimetrorail.com | Hyperlinking clause in DMRC's privacy policy |
| Send the same e-mail to 15 mailboxes at once | Reads as spam; route it properly (§1 of DMRC_CONTACTS.md) |
| Claim it is "already live for all cards" | Only demo data exists |
| Attach a 40 MB video | Use a 2-minute screen capture link or the deck PDF |
| Chase daily | 7 days, then 21 days, then stop |

---

## 10. The four-bullet summary to keep in your head

1. **DMRC says it can't be done online** — we researched it, and that is precisely why an authorised data path is needed.
2. **The build is done and demonstrable** — no funding, no vendor, no agency required to evaluate it.
3. **The ask is small** — 30 minutes, a name, a sandbox, a pilot.
4. **The cost to DMRC is zero** — free to riders, MIT-licensed, DMRC can take it over or switch it off at any time.
