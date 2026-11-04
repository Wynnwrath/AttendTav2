---
type: epic
title: "Officers issue retained PDF certificates"
parent: initiative-attendta
covers: [R13, R20, R25, R27, R28, R29, R33, R36]
sources: [_bmad-output/initiative-attendta/initiative-attendta.md]
companions: [_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md, docsss/architecture.md, docsss/design.md, _bmad-output/initiative-attendta/ux-attendta/DESIGN.md, _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, docsss/AttendTa-System-Specification.md]
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

- Passwords only as salted Argon2id hashes (AD-15); signed expiring cookies, per-request active and role checks and antiforgery on every mutation (AD-8); ownership checks on personal resources (AD-8); every time, capacity and state guard on the server (AD-4, AD-11); secrets, password and token hashes, QR tokens, file paths and scan payloads never logged (Consistency Conventions, Diagnostics); no login throttling (AD-8; prd NFR-1).
- Multi-record mutations run in one caller-owned SQL transaction with the audit write, serialized where they compete; uniqueness enforced in the database (AD-3, AD-4, AD-10; prd NFR-2).
- No hard deletes; Activity Log append-only (AD-7; prd NFR-3).
- Files on persistent server-local storage outside the web root, separate venue/event/certificate/temporary areas, relative paths only in SQL (AD-12, AD-16; prd NFR-4).
- UTC datetime2(3) storage, Asia/Manila display with a label, no background jobs (AD-16); every time guard evaluated per request (AD-11; prd NFR-5).
- net10.0 ASP.NET Core MVC with fluent EF Core 10 mapping and no migrations, schema from database/attendta-mvp-schema.sql, WebApp → Services → Data (AD-1, AD-10, AD-18), compiled Tailwind with no runtime CDN (AD-14).
- States are seeded lookup tables with FKs; event_types.description NOT NULL; certificates.issuer_name_snapshot (AD-10).
- feature/* or fix/* branches, PRs to dev with green CI and a peer approval, Conventional Commits (Consistency Conventions, Git).
- SPEC v2.1 in docsss/ is authoritative for rule detail unless prd §9 overrides (parent Constraints).

## Done when

- UJ-4 runs on the dev build: completion creates reviews, an Officer approves and issues, and the Member downloads the PDF from My Events.
- The PDF is landscape A4 from the fixed template with embedded fonts, a unique number and snapshots, rendered by Microsoft.Playwright 1.64.1 (Stack; AD-18) on the dev host, and on the demo host after the AD-16 staging install.
- SQL Server integration tests in tests/ASI.Basecode.IntegrationTests show concurrent issuance yields one Certificate and invalidation versus issuance resolves one way (SM-4 certificates; AD-17).
- Unauthorized and inactive-Account downloads are refused; a retry never replaces an issued PDF.
- Merged to dev with integrated verification; official issuance waits for the configured signatory (Q1).

## Boundaries

- Belongs to epic-check-in: invalidation itself, re-check-in refusal, and the IAttendanceInvalidationHandler declaration (AD-2).
- Belongs to epic-events-registration: the completion transaction, the IEventCompletionHandler declaration, MyEventsController and MyEventCardViewModel (AD-2, AD-14).
- Belongs to epic-participation-reports: attendance figures.
- Out: template editor, bulk issuance, revocation, reissue (parent Boundaries).

## References

- parent — _bmad-output/initiative-attendta/initiative-attendta.md, Requirements R13, R20, R25, R27–R29, R33, R36
- spine — _bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md: AD-2 (hooks), AD-3 (unit of work), AD-4 (lock order, no locks during render), AD-5 (ParticipationFacts), AD-7 (audit), AD-8 (sessions and authorization), AD-9 (outcomes), AD-10 (certificates table and schema), AD-11 (time), AD-12 (certificates and temporary areas), AD-13 (paging), AD-14 (My Events partial), AD-16 (Playwright on demo host), AD-17 (test projects), AD-18 (net10.0); Stack table (Microsoft.Playwright 1.64.1); Deferred: certificate number, template, signatory, renderer limits
- architecture — docsss/architecture.md, §7 correction versus issuance, §8 certificate pipeline
- spec — docsss/AttendTa-System-Specification.md, §14 (14.1–14.4)
- design — _bmad-output/initiative-attendta/ux-attendta/DESIGN.md; docsss/design.md
- ux — _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, UJ-4

## Notes

- Owner: Pinca (addendum C). Certificate state rules on branch 0be8898 may be reused after validation; Valid Attendance and Completed status come from ParticipationFacts (AD-5), and pure rule tests live in tests/ASI.Basecode.Services.Tests with a fake TimeProvider (AD-17, AD-11).
- Open question: Q1 — authorized signatory name, title and signature; PM (Talisic); needed before official issuance (parent Notes).
- Decision: 2026-11-04 — refined from the initiative's lines alone.
- Waits on epic-events-registration because: the completion transaction with IEventCompletionHandler declared, and the My Events page.
- Waits on epic-check-in because: Attendance Records with Valid Attendance facts and IAttendanceInvalidationHandler declared in the invalidation transaction.
- Waits on epic-platform-access because: Officer sign-in, ICurrentActor and Management policy (1.1), the canonical certificates mapping (1.2), the caller-owned unit of work and audit writer (1.3), the local-time formatter (1.4), the pager (1.6) and the Tailwind role shell (1.9).
- Waits on epic-master-data because: the IPrivateFileStore certificates and temporary areas with PromoteAsync and OpenReadAsync (2.2, AD-12).
- Decision: 2026-11-04 — tracer bullet: entry 1, Playwright .NET setup and certificate PDF issuance tracer; next: entry 2, Fixed-template landscape A4 Razor certificate rendering with embedded fonts (least certain).
- Decision: 2026-11-04 — no closing end-to-end suite in this epic; epic-release-acceptance runs SPEC §20 across the build.
- Decision: 2026-11-04 — entry 10 concurrency tests folded into entry 7 (transactional issuance and invalidation race resolution) to satisfy the test-only story rule; entry 10 deleted, leaving id 10 retired.
- Decision: 2026-11-04 — re-sliced against the architecture spine (AD-2, AD-3, AD-4, AD-5, AD-7, AD-8, AD-9, AD-10, AD-11, AD-12, AD-13, AD-14, AD-16, AD-17, AD-18): 6.4 and 6.5 implement hooks without editing foreign transactions; 6.7 refuses repeat issuance as Duplicate with render/promote outside locks and inline cleanup; 6.8 returns identical unauthorized/unknown responses; 6.9 contributes _CertificateLinkPartial to e3's My Events; 6.1 drops certificates mapping and host Chromium install.
