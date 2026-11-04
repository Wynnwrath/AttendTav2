---
type: epic
title: "Officers publish events and fill seats fairly"
parent: initiative-attendta
covers: [R8, R9, R10, R11, R12, R13, R14, R15, R16, R17, R18, R19, R20, R33, R36]
sources: [_bmad-output/initiative-attendta/initiative-attendta.md]
companions: [_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md, docsss/architecture.md, docsss/design.md, _bmad-output/initiative-attendta/ux-attendta/DESIGN.md, _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, docsss/AttendTa-System-Specification.md]
after: []
refined: true
risk: high
---

# Officers publish events and fill seats fairly

## Description

The highest-risk lifecycle and concurrency work: Management runs an Event from draft to publish, edit, cancel and complete, and Eligible Participants register before the deadline. Reviews, Promotion and cancellations allocate Seats without overbooking and respect the start cutoff through one Registration-owned participation cancellation service, which epic-administration later calls. Members follow their Registrations in My Events.

## Outcome

For Officers and Members, seats are allocated by explicit, fair review instead of manual headcounts, shown by UJ-2 including the concurrent last-seat case.

## Requirements

- R1 (parent R9): Management creates and edits draft Events: title, description, active Event Type, active Venue, start, participant limit > 0 and <= Venue capacity, Signup Deadline <= start, optional Cover. Drafts are hidden from Members.
- R2 (parent R10): Management publishes a draft only with an active type and venue (revalidated), a Cover, a valid capacity and a future Signup Deadline. Publication is audited, shows the Event in the Member feed and opens signup at once.
- R3 (parent R11): On a Published Event only title, description and Cover can change, with old/new values logged. Completed and Cancelled Events are read-only.
- R4 (parent R12): Management cancels a Draft or Published Event that has no Valid Attendance and no issued Certificate, with a reason. Cancellation atomically cancels its pending, confirmed and waitlisted Registrations, invalidates their QR Passes, closes Check-in and is audited; no Promotion.
- R5 (parent R13): Management completes a Published Event at or after its start; completion closes Check-in; nothing completes an Event automatically and completion before start is refused. Creating Certificate Reviews in the completion transaction belongs to epic-certificates.
- R6 (parent R14): An Eligible Participant registers for a Published Event before its Signup Deadline, creating a pending Registration with a course/year/section snapshot. A second Registration for the same Member and Event is refused, even after cancellation. Full Events accept signup with a no-seat-guaranteed warning. Overlapping Events are allowed.
- R7 (parent R15): At or after the Signup Deadline the server refuses new signups, stale forms included; the Event stays Published and existing Registrations are unaffected.
- R8 (parent R16): Before the Event Start Cutoff Management approves or declines (reason required) Pending Registrations, and administratively cancels Confirmed or Waitlisted ones without Valid Attendance (reason required, QR invalidated, Seat released, no Promotion). Approval confirms only when a Seat is free and no earlier eligible Waitlisted Registration has priority; otherwise it waitlists. Concurrent last-seat approvals never overbook. Confirmation generates a unique, unpredictable QR token.
- R9 (parent R17): Before the Event Start Cutoff Management promotes the earliest Waitlisted Registration whose owner is still an Eligible Participant (by review time, then registration ID, across the whole queue) when a Seat is free; otherwise nothing changes. Promotion is serialized with approvals and never automatic.
- R10 (parent R18): After an explicit confirmation, a Member cancels their own pending, confirmed or waitlisted Registration on a Published Event before the Event Start Cutoff and before Valid Attendance. The QR Pass is invalidated, the Seat released, no Promotion follows, and re-registration is refused.
- R11 (parent R19): At or after the Event start the server refuses reviews, confirmations, Promotions and member cancellations. Pending and Waitlisted rows stay unchanged and show as Not confirmed before cutoff; they are not No-shows.
- R12 (parent R20): A Member sees all their Registrations with status in My Events, including Not confirmed before cutoff. QR Pass, attendance state and certificate links are added by epic-check-in and epic-certificates.
- R13 (parent R8): Management uploads, replaces and (while a draft) removes an Event Cover through the shared image storage; publication refuses a missing Cover.
- R14 (parent R33): Event publication, edits, cancellation and completion, Registration reviews, Promotions and cancellations each append an Activity Log entry.
- R15 (parent R36): The Member events feed, Management event list, registration review queue and personal registrations are paged.

## Constraints

- Passwords only as salted Argon2id hashes; signed expiring cookies; antiforgery on every mutation; ownership checks on personal resources; every time, capacity and state guard on the server; secrets, hashes and QR tokens never logged; no login throttling (parent Constraints; prd NFR-1; AD-8; AD-15 for the hasher).
- Multi-record mutations run in one caller-owned SQL transaction with the audit write, serialized where they compete; uniqueness enforced in the database (parent Constraints; prd NFR-2; AD-3, AD-4).
- No hard deletes; Activity Log append-only (parent Constraints; prd NFR-3).
- Files on persistent server-local storage outside the web root, separate venue/event/certificate directories, relative paths only in SQL (parent Constraints; prd NFR-4; AD-12).
- UTC datetime2(3) storage, Asia/Manila display with a label, no scheduled jobs; every time guard evaluated per request against the injected TimeProvider (parent Constraints; prd NFR-5; AD-11).
- One MVC app and one SQL Server database; Chrome target; mobile-responsive (parent Constraints; prd NFR-6).
- net10.0 MVC (AD-18), EF Core database-first, WebApp → Services → Data (AD-1), compiled Tailwind with no runtime CDN (AD-14) (parent Constraints).
- States are seeded lookup tables with FKs; event_types.description NOT NULL; certificates.issuer_name_snapshot (parent Constraints; AD-10).
- feature/* or fix/* branches, PRs to dev with green CI and a peer approval, Conventional Commits (parent Constraints; addendum D).
- SPEC v2.1 in docsss/ is authoritative for rule detail unless prd §9 overrides (parent Constraints).

## Done when

- UJ-2 runs on the dev build: draft with cover, publish, signups, approvals confirm up to the limit and then waitlist, a confirmed Member cancels, and Promote confirms the earliest eligible waitlisted Registration.
- SQL Server tests show concurrent last-seat approvals and Promotion never overbook (SM-4 seats).
- Signup Deadline and Event Start Cutoff refuse stale forms at the exact boundary, stored in UTC and shown in Asia/Manila, using the injected TimeProvider (AD-11).
- Event cancellation cancels its open Registrations atomically through IRegistrationCancellationService, implemented with the four AD-6 methods that epic-administration calls; completion before start is refused.
- Merged to dev with integrated verification.

## Boundaries

- Belongs to epic-check-in: QR PNG rendering, the Check-in toggle (which adds SetCheckInWindowAsync to IEventLifecycleService with this epic's review), scans and attendance.
- Belongs to epic-certificates: Certificate Reviews created at completion through IEventCompletionHandler (AD-2), and certificate links.
- Belongs to epic-administration: deactivation-triggered cancellation, which calls CancelUpcomingForUserAsync and PreviewUpcomingForUserAsync (AD-6); the screens and reason entry are epic-administration's.
- Must not change: nothing promotes automatically; no reapplication after cancellation.

## References

- parent — _bmad-output/initiative-attendta/initiative-attendta.md, Requirements R8–R20, R33, R36
- spine — _bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md: AD-2 (writers, hooks), AD-3 (unit of work), AD-4 (lock order), AD-5 (ParticipationFacts), AD-6 (cancellation service), AD-7 (audit), AD-8 (actor, policies), AD-9 (outcomes), AD-10 (lookups, unique keys), AD-11 (TimeProvider), AD-12 (file store), AD-13 (paging), AD-14 (My Events ownership), AD-17 (test projects); Consistency Conventions (QR token format)
- architecture — docsss/architecture.md
- spec — docsss/AttendTa-System-Specification.md, §7–§10 events, registration, cutoffs
- design — _bmad-output/initiative-attendta/ux-attendta/DESIGN.md; docsss/design.md
- ux — _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, UJ-2

## Notes

- Owner: Member 3 (addendum C).
- Decision: 2026-11-04 — refined from the initiative's lines alone; kept as one epic (split declined, parent Notes).
- Waits on epic-master-data because: active Event Types and Venues and the shared image storage (IPrivateFileStore, AD-12). Member eligibility is the ParticipationFacts fact this epic creates (AD-5).
- Waits on epic-platform-access because: login, ICurrentActor and policies (1.1), canonical EF mappings (1.2), audit writer (1.3), local-time display helper (1.4), pager (1.6), approval-created members (1.7), role shell (1.9), and test projects (1.10).
- Decision: 2026-11-04 — tracer bullet: entry 1, Draft event creation, edit, and management event list (architectural slice through EF mapping, service validation, controller, Tailwind views, and pager); least-certain story entry 7 (seat allocation and SM-4 concurrency) cannot follow entry 1 directly because reviewing registrations functionally requires published events with covers (entries 2, 3) and member registration submission (entry 5).
- Decision: 2026-11-04 — no closing end-to-end suite in this epic; epic-release-acceptance runs SPEC §20 across the build.
- Decision: 2026-11-04 — re-sliced against the architecture spine (AD-2, AD-4, AD-5, AD-6, AD-11, AD-14, AD-18): added entry 15 for IRegistrationCancellationService (AD-6), created all AD-5 facts in ParticipationFacts in entry 3, named IEventLifecycleService in entry 1, declared IEventCompletionHandler in entry 12, moved cutoff guards into the actions they guard, established My Events ownership in entry 13, specified 64-hex QR tokens, and retargeted net10.0.
