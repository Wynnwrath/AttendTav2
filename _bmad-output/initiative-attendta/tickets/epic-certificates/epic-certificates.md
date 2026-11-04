---
type: epic
title: "Officers issue retained PDF certificates"
parent: initiative-attendta
covers: [R13, R20, R25, R27, R28, R29, R33, R36]
sources: [_bmad-output/initiative-attendta/initiative-attendta.md]
companions: [docsss/architecture.md, docsss/design.md, _bmad-output/initiative-attendta/ux-attendta/DESIGN.md, _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, docsss/AttendTa-System-Specification.md]
after: []
refined: true
risk: high
---

# Officers issue retained PDF certificates

## Description

Turns completed, attended Events into certificates: completion creates Certificate Reviews for valid attendees, Management approves or declines each, issues one fixed-template A4 PDF at a time rendered by Playwright, and the owner or Management downloads it through an authorized endpoint. Attendance corrections and issuance are serialized.

## Outcome

For Members and Officers, certificate eligibility rests on recorded attendance and approval, shown by UJ-4 ending with a downloaded PDF.

## Requirements

- R1 (parent R13): Event completion creates a Certificate Review for every Registration with Valid Attendance inside the completion transaction.
- R2 (parent R20): My Events shows download links for the Member's issued Certificates.
- R3 (parent R25): Invalidating an Attendance Record turns any unissued Certificate Review not_eligible with the correction reason, is blocked once a Certificate is issued, and is serialized against issuance.
- R4 (parent R27): Management approves (eligible) or declines (not_eligible, reason required, final) each pending Certificate Review, recording reviewer and time. Approval re-checks Valid Attendance, serialized against invalidation.
- R5 (parent R28): Management issues one eligible Certificate at a time: a landscape A4 PDF from one fixed template with embedded fonts, a unique certificate number, and snapshots of recipient name, Event title and start, issue date and issuer. Issuance re-checks approval, Valid Attendance and completion; duplicate issuance is impossible; a failed write leaves it unissued; a retry never replaces an issued PDF; success shows only after the final commit.
- R6 (parent R29): The active Account owner or Management downloads an issued Certificate streamed as application/pdf through an endpoint that checks permission per request, never as a static file or caller-supplied path. An inactive Account is blocked; inactive membership is not. Later edits never alter the retained PDF.
- R7 (parent R33): Certificate decisions and issuance each append an Activity Log entry.
- R8 (parent R36): Certificate review and issuance queues are paged.

## Constraints

- Passwords only as salted Argon2id hashes; signed expiring cookies; antiforgery on every mutation; ownership checks on personal resources; every time, capacity and state guard on the server; secrets, hashes and QR tokens never logged; no login throttling (parent Constraints; prd NFR-1).
- Multi-record mutations run in one caller-owned SQL transaction with the audit write, serialized where they compete; uniqueness enforced in the database (parent Constraints; prd NFR-2; architecture §7).
- No hard deletes; Activity Log append-only (parent Constraints; prd NFR-3).
- Files on persistent server-local storage outside the web root, separate venue/event/certificate directories, relative paths only in SQL (parent Constraints; prd NFR-4; architecture §8).
- UTC datetime2(3) storage, Asia/Manila display with a label, no scheduled jobs; every time guard evaluated per request (parent Constraints; prd NFR-5).
- .NET 9 MVC, EF Core database-first, WebApp → Services → Data, compiled Tailwind with no runtime CDN (parent Constraints; architecture §3, §9).
- States are seeded lookup tables with FKs; event_types.description NOT NULL; certificates.issuer_name_snapshot (parent Constraints; architecture §5).
- feature/* or fix/* branches, PRs to dev with green CI and a peer approval, Conventional Commits (parent Constraints; addendum D).
- SPEC v2.1 in docsss/ is authoritative for rule detail unless prd §9 overrides (parent Constraints).

## Done when

- UJ-4 runs on the dev build: completion creates reviews, an Officer approves and issues, and the Member downloads the PDF from My Events.
- The PDF is landscape A4 from the fixed template with embedded fonts, a unique number and snapshots, rendered by pinned Playwright .NET on the dev host.
- SQL Server tests show concurrent issuance yields one Certificate and invalidation versus issuance resolves one way (SM-4 certificates).
- Unauthorized and inactive-Account downloads are refused; a retry never replaces an issued PDF.
- Merged to dev with integrated verification; official issuance waits for the configured signatory (Q1).

## Boundaries

- Belongs to epic-check-in: invalidation itself and re-check-in refusal.
- Belongs to epic-participation-reports: attendance figures.
- Out: template editor, bulk issuance, revocation, reissue (parent Boundaries).

## References

- parent — _bmad-output/initiative-attendta/initiative-attendta.md, Requirements R13, R20, R25, R27–R29, R33, R36
- architecture — docsss/architecture.md, §7 correction versus issuance, §8 certificate pipeline
- spec — docsss/AttendTa-System-Specification.md, §13 certificates
- design — _bmad-output/initiative-attendta/ux-attendta/DESIGN.md; docsss/design.md
- ux — _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, UJ-4

## Notes

- Owner: Pinca (addendum C). Certificate-eligibility pure rules exist on branch 0be8898 (architecture §1) and may be reused after validation.
- Open question: Q1 — authorized signatory name, title and signature; PM (Talisic); needed before official issuance (parent Notes).
- Decision: 2026-11-04 — refined from the initiative's lines alone.
- Waits on epic-events-registration because: the completion transaction and My Events.
- Waits on epic-check-in because: Attendance Records with invalidation.
- Waits on epic-platform-access because: the canonical certificates mapping (1.2).
- Waits on epic-master-data because: the shared private storage adapter for the certificates directory (2.2).
- Decision: 2026-11-04 — tracer bullet: entry 1, Playwright .NET setup and certificate PDF issuance tracer; next: entry 2, Fixed-template landscape A4 Razor certificate rendering with embedded fonts (least certain).
- Decision: 2026-11-04 — no closing end-to-end suite in this epic; epic-release-acceptance runs SPEC §20 across the build.
- Decision: 2026-11-04 — entry 10 concurrency tests folded into entry 7 (transactional issuance and invalidation race resolution) to satisfy the test-only story rule; entry 10 deleted, leaving id 10 retired.
