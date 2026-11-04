---
type: epic
title: "Confirmed members get a QR Pass and officers record attendance"
parent: initiative-attendta
covers: [R20, R21, R22, R23, R24, R25, R26, R33, R36]
sources: [_bmad-output/initiative-attendta/initiative-attendta.md]
companions: [_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md, docsss/architecture.md, docsss/design.md, _bmad-output/initiative-attendta/ux-attendta/DESIGN.md, _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, docsss/AttendTa-System-Specification.md]
after: []
refined: true
risk: high
---

# Confirmed members get a QR Pass and officers record attendance

## Description

Gives each Confirmed Registration a downloadable QR Pass and gives Management a check-in window, a scanner and a manual fallback that record exactly one Attendance Record per Registration, plus invalidation of mistakes. Members see their pass and attendance in My Events.

## Outcome

For Officers at the door, check-in becomes fast and duplicate-proof, shown by UJ-3 on a laptop camera.

## Requirements

- R1 (parent R20): My Events shows the QR Pass for Confirmed Registrations and each Registration's Attendance Record state, including invalidation.
- R2 (parent R21): The owner of a Confirmed Registration views its QR Pass and downloads it as a PNG generated on demand. The QR encodes only the token. Ownership is checked; cancelled or unconfirmed Registrations have no usable pass.
- R3 (parent R22): Management opens and closes Check-in for a Published Event at any time, each toggle audited. Completion and cancellation close it; nothing opens it automatically.
- R4 (parent R23): Management scans a QR Pass. A valid scan records one Attendance Record (method, recorder, UTC time) and shows name and student number. A repeat scan shows the original time and inserts nothing. Wrong-event, ineligible, cancelled, unconfirmed, invalidated, closed or unpublished, and unknown tokens are rejected without inserting or revealing other records.
- R5 (parent R24): Management finds a Confirmed participant by name or student number and checks them in under the same validations as a scan, recording manual_search or student_number. Searching, viewing or downloading never records attendance.
- R6 (parent R25): Management invalidates an Attendance Record after a confirmation and reason, keeping the row; it leaves reports and eligibility and re-check-in after invalidation is refused. Its Certificate Review effect and serialization with issuance belong to epic-certificates.
- R7 (parent R26): A Member sees their own Attendance Records with invalidated ones marked, and cannot check in or correct themselves.
- R8 (parent R33): Check-in toggles, each new Attendance Record and each invalidation append an Activity Log entry.
- R9 (parent R36): Manual search results and attendance histories are paged.

## Constraints

- Passwords only as salted Argon2id hashes; signed expiring cookies; antiforgery on every mutation; ownership checks on personal resources; every time, capacity and state guard on the server; secrets, hashes and QR tokens never logged; no login throttling (parent Constraints; prd NFR-1; AD-7, AD-8, AD-15).
- Multi-record mutations run in one caller-owned SQL transaction with the audit write, serialized where they compete; uniqueness enforced in the database (parent Constraints; prd NFR-2; AD-3, AD-4, AD-10).
- No hard deletes; Activity Log append-only (parent Constraints; prd NFR-3).
- UTC datetime2(3) storage, Asia/Manila display with a label, no scheduled jobs; every time guard evaluated per request (parent Constraints; prd NFR-5; AD-11, AD-16).
- One MVC app and one SQL Server database; Chrome target; mobile-responsive (parent Constraints; prd NFR-6; AD-1, AD-16).
- QR Pass carries no personal data; unknown-token and unauthorized responses reveal nothing about other records; QR tokens never logged or stored in audit payloads (parent Constraints; prd NFR-7; AD-7, AD-9).
- net10.0 MVC (AD-18), EF Core 10 fluent mapping in AsiBasecodeDbContext with no EF migrations over database/attendta-mvp-schema.sql (AD-10), WebApp → Services → Data (AD-1), compiled Tailwind and local scripts with no runtime CDN (AD-14).
- feature/* or fix/* branches, PRs to dev with green CI and a peer approval, Conventional Commits (parent Constraints; addendum D).
- SPEC v2.1 in docsss/ is authoritative for rule detail unless prd §9 overrides (parent Constraints).

## Done when

- UJ-3 runs on the dev build with a laptop camera on localhost: a valid scan shows name and student number, a repeat shows the original time, and a student-number lookup checks in a participant.
- Every FR-23 scan outcome has an automated test in tests/ASI.Basecode.Services.Tests, and concurrent duplicate scans create one Attendance Record, proven in tests/ASI.Basecode.IntegrationTests against ATTENDTA_TEST_SQL (SM-4 attendance; AD-4, AD-17).
- An invalidated record stays stored, is marked for the Member and blocks re-check-in.
- A Member downloads their QR Pass PNG; another Member's pass is refused.
- Merged to dev with integrated verification.

## Boundaries

- Belongs to epic-events-registration: QR token generation at confirmation, the My Events page, creation of ParticipationFacts.cs, and the initial Valid Attendance fact (AD-5; this epic owns later changes to Valid Attendance).
- Belongs to epic-certificates: the not_eligible effect of invalidation and its serialization with issuance. This epic declares IAttendanceInvalidationHandler and calls every registered implementation inside the invalidation transaction (AD-2).
- Belongs to epic-participation-reports: reads Valid Attendance and invalidated rows for participation reporting.
- Belongs to epic-release-acceptance: phone scanning over the hotspot with trusted HTTPS.
- Must not change: check-in never changes the Registration's status.

## References

- parent — _bmad-output/initiative-attendta/initiative-attendta.md, Requirements R20–R26, R33, R36
- spine — _bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md: AD-1 (layering, controllers), AD-2 (toggle via SetCheckInWindowAsync, IAttendanceInvalidationHandler hook), AD-3 (ExecuteInTransactionAsync), AD-4 (lock order, LockAttendanceAsync), AD-5 (Valid Attendance), AD-7 (audit codes, no token in payload), AD-8 (Management policy, antiforgery), AD-9 (scanner outcome/refusal shapes), AD-10 (unique keys, lookup tables), AD-11 (TimeProvider), AD-13 (25-row paging), AD-14 (_PassPartial, local assets, no CDN), AD-15 (password hasher), AD-16 (single host), AD-17 (test projects), AD-18 (net10.0, WebApp/Rendering, no System.Drawing); Deferred: server QR library and browser scanner library
- spec — docsss/AttendTa-System-Specification.md, §11–§12 QR and attendance
- design — _bmad-output/initiative-attendta/ux-attendta/DESIGN.md; docsss/design.md
- ux — _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, UJ-3

## Notes

- Owner: Member 4 (addendum C); the server QR library and the browser scanner library are chosen by this epic within AD-18 (spine Deferred); record both in the spine Stack table when chosen.
- Decision: 2026-11-04 — refined from the initiative's lines alone; scanning verified on a laptop camera on localhost, phone over hotspot in epic-release-acceptance.
- Waits on epic-events-registration because: Confirmed Registrations with QR tokens (3.7), IEventLifecycleService and completion (3.1, 3.12), ParticipationFacts (3.3), and My Events (3.13).
- Decision: 2026-11-04 — tracer bullet: entry 1, On-demand QR Pass PNG rendering and download; least-certain story is entry 4, In-browser camera QR scanner on localhost, which is deferred to fourth in build order because it requires the check-in window toggle (entry 2) and server check-in endpoint (entry 3) to post scanned tokens and verify attendance.
- Decision: 2026-11-04 — entry 8 (concurrent check-in serialization tests) folded into entry 3 as tests belong in the story whose behavior they prove, leaving entry 8 deleted.
- Decision: 2026-11-04 — no closing end-to-end suite in this epic; epic-release-acceptance runs SPEC §20 across the build.
- Decision: 2026-11-04 — concurrency test for entry 3 lives in tests/ASI.Basecode.IntegrationTests against ATTENDTA_TEST_SQL (AD-17).
- Decision: 2026-11-04 — re-sliced against the architecture spine (AD-1..AD-18): 4.2 sets check-in window via IEventLifecycleService.SetCheckInWindowAsync after 3.12 (and 3.1); 4.6 declares IAttendanceInvalidationHandler; 4.1 renders QR PNG in WebApp/Rendering without System.Drawing; 4.4 serves local scanner library with antiforgery header after entries 1, 2, 3; entry 3 adds attendance Lock method LockAttendanceAsync; no new entry added as 3.3 writes ParticipationFacts.
- Check beyond criteria, entry 3: a reviewer compares the check-in transaction's lock sequence (users → event → attendance) with AD-4 before merge.
- Check beyond criteria, entry 4: during the laptop check, confirm no scan payload appears in server logs (spine Consistency: Diagnostics).
