---
type: initiative
title: "AttendTa: one organization runs membership, events, check-in and certificates in one attributable system"
parent: none
sources: [_bmad-output/initiative-attendta/prd-attendta/prd-attendta.md]
companions: [_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md, _bmad-output/initiative-attendta/prd-attendta/addendum.md, docsss/AttendTa-System-Specification.md, docsss/architecture.md, docsss/design.md, _bmad-output/initiative-attendta/ux-attendta/DESIGN.md, _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, AttendTa-User-Stories.md, database/attendta-mvp-schema.sql, docsss/AttendTa-MVP.dbml, docsss/git_qa_guidelines.pdf]
after: []
risk: high
---

# AttendTa

## Description

AttendTa is the Group 7 Alliance Jumpstart (J4S) MVP: one ASP.NET Core MVC app where students request membership, officers publish events and control seats, members register and present a QR Pass, officers check attendees in at the door, and officers issue retained PDF certificates and read participation reports, with every consequential action attributed in the Activity Log. It delivers all 26 HLD/WBS functional IDs plus FR-35 and FR-36 of the [PRD](prd-attendta/prd-attendta.md). Today only partial Master Data code exists; login is a stub and the transaction modules are absent.

## Outcome

For the organization's applicants, members, officers and administrators, the request-to-certificate path runs in one explicit, auditable system instead of forms, spreadsheets and paper sheets; the signal is the PRD's SM-1 (every SPEC §20 acceptance item passes in the client demo) and SM-2 (26/26 functional IDs end to end).

## Requirements

- R1 (prd FR-1 [AUTH-01]): An Applicant submits an Account Request (student number, full name, email, course, year level, section, password and confirmation; optional mobile). Missing fields, year level <= 0, malformed email, mismatched confirmation, or an email/student number used by a pending request, Account or Member Profile is refused with field errors. Password kept only as a hash; Applicant sees an acknowledgment only and cannot choose Role or membership values.
- R2 (prd FR-2 [AUTH-02]): Management approves or rejects a pending Account Request. Approval atomically creates an active Member Account and its Member Profile, moves the hash, re-checks email and student-number uniqueness, and is audited. Rejection needs a reason, clears the hash and creates no Account. Concurrent decisions yield one outcome; a finalized request cannot be reviewed again.
- R3 (prd FR-3 [AUTH-03]): An active Account logs in with its email and password and logs out. Every failure is generic. Success issues a signed, expiring cookie session routed by Role. Every protected request re-checks current Account active state and Role.
- R4 (prd FR-4 [MD-01]): A Member views their own account, academic and membership details; nothing is editable.
- R5 (prd FR-5 [MD-02]): Management edits a Member's full name, course, year level, section, mobile, membership type and membership status. A student-number correction requires a reason, stays unique and logs old/new values. Login email is read-only. Edits never rewrite Registration snapshots or issued Certificates.
- R6 (prd FR-6 [MD-03]): Management creates, edits, activates and deactivates Event Types with a unique, non-blank name and a non-blank description. Inactive types cannot be chosen for new or draft Events; existing references are kept.
- R7 (prd FR-7 [MD-04]): Management maintains Venues: room name, positive capacity, location, availability notes, contact person, active state and a required photo. Capacity never drops below the participant limit of a linked draft or published Event. Inactive Venues cannot be chosen for new or draft Events.
- R8 (prd FR-8 [MD-05]): Management uploads or replaces a Venue photo or Event Cover: JPEG/PNG verified by content, at most 2 MiB, server-generated filename, one current image per record. A failed file or database write leaves the previous image intact.
- R9 (prd FR-9 [EVT-01, EVT-03]): Management creates and edits draft Events: title, description, active Event Type, active Venue, start, participant limit > 0 and <= Venue capacity, Signup Deadline <= start, optional Cover. Drafts are hidden from Members.
- R10 (prd FR-10 [EVT-01, EVT-02]): Management publishes a draft only with an active type and venue (revalidated), a Cover, a valid capacity and a future Signup Deadline. Publication is audited, shows the Event in the Member feed and opens signup at once.
- R11 (prd FR-11 [EVT-03]): On a Published Event only title, description and Cover can change, with old/new values logged. Completed and Cancelled Events are read-only.
- R12 (prd FR-12 [EVT-04]): Management cancels a Draft or Published Event that has no Valid Attendance and no issued Certificate, with a reason. Cancellation atomically cancels its pending, confirmed and waitlisted Registrations, invalidates their QR Passes, closes Check-in and is audited; no Promotion.
- R13 (prd FR-13 [EVT-04]): Management completes a Published Event at or after its start. Completion closes Check-in and creates a Certificate Review for every Registration with Valid Attendance. Nothing completes an Event automatically; completion before start is refused.
- R14 (prd FR-14 [REG-01]): An Eligible Participant registers for a Published Event before its Signup Deadline, creating a pending Registration with a course/year/section snapshot. A second Registration for the same Member and Event is refused, even after cancellation. Full Events accept signup with a no-seat-guaranteed warning. Overlapping Events are allowed.
- R15 (prd FR-15 [EVT-02, REG-05]): At or after the Signup Deadline the server refuses new signups, stale forms included; the Event stays Published and existing Registrations are unaffected.
- R16 (prd FR-16 [REG-02]): Before the Event Start Cutoff Management approves or declines (reason required) Pending Registrations, and administratively cancels Confirmed or Waitlisted ones without Valid Attendance (reason required, QR invalidated, Seat released, no Promotion). Approval confirms only when a Seat is free and no earlier eligible Waitlisted Registration has priority; otherwise it waitlists. Concurrent last-seat approvals never overbook. Confirmation generates a unique, unpredictable QR token.
- R17 (prd FR-17 [REG-04]): Before the Event Start Cutoff Management promotes the earliest Waitlisted Registration whose owner is still an Eligible Participant (by review time, then registration ID, across the whole queue) when a Seat is free; otherwise nothing changes. Promotion is serialized with approvals and never automatic.
- R18 (prd FR-18 [REG-03]): After an explicit confirmation, a Member cancels their own pending, confirmed or waitlisted Registration on a Published Event before the Event Start Cutoff and before Valid Attendance. The QR Pass is invalidated, the Seat released, no Promotion follows, and re-registration is refused.
- R19 (prd FR-19 [REG-05]): At or after the Event start the server refuses reviews, confirmations, Promotions and member cancellations. Pending and Waitlisted rows stay unchanged and show as Not confirmed before cutoff; they are not No-shows.
- R20 (prd FR-20 [REG-01, ATT-03]): A Member sees all their Registrations with status, the QR Pass when Confirmed, Attendance Record state including invalidation, and download links for issued Certificates.
- R21 (prd FR-21 [QR-01]): The owner of a Confirmed Registration views its QR Pass and downloads it as a PNG generated on demand. The QR encodes only the token. Ownership is checked; cancelled or unconfirmed Registrations have no usable pass.
- R22 (prd FR-22 [ATT-01]): Management opens and closes Check-in for a Published Event at any time, each toggle audited. Completion and cancellation close it; nothing opens it automatically.
- R23 (prd FR-23 [ATT-01]): Management scans a QR Pass. A valid scan records one Attendance Record (method, recorder, UTC time) and shows name and student number. A repeat scan shows the original time and inserts nothing. Wrong-event, ineligible, cancelled, unconfirmed, invalidated, closed or unpublished, and unknown tokens are rejected without inserting or revealing other records.
- R24 (prd FR-24 [ATT-01]): Management finds a Confirmed participant by name or student number and checks them in under the same validations as a scan, recording manual_search or student_number. Searching, viewing or downloading never records attendance.
- R25 (prd FR-25 [ATT-02]): Management invalidates an Attendance Record after a confirmation and reason, keeping the row. It leaves reports and eligibility, turns any unissued Certificate Review not_eligible, is blocked once a Certificate is issued, and is serialized against issuance. Re-check-in after invalidation is refused.
- R26 (prd FR-26 [ATT-03]): A Member sees their own Attendance Records with invalidated ones marked, and cannot check in or correct themselves.
- R27 (prd FR-27 [CERT-01]): Management approves (eligible) or declines (not_eligible, reason required, final) each pending Certificate Review, recording reviewer and time. Approval re-checks Valid Attendance, serialized against invalidation.
- R28 (prd FR-28 [CERT-02]): Management issues one eligible Certificate at a time: a landscape A4 PDF from one fixed template with embedded fonts, a unique certificate number, and snapshots of recipient name, Event title and start, issue date and issuer. Issuance re-checks approval, Valid Attendance and completion; duplicate issuance is impossible; a failed write leaves it unissued; a retry never replaces an issued PDF; success shows only after the final commit.
- R29 (prd FR-29 [CERT-02]): The active Account owner or Management downloads an issued Certificate streamed as application/pdf through an endpoint that checks permission per request, never as a static file or caller-supplied path. An inactive Account is blocked; inactive membership is not. Later edits never alter the retained PDF.
- R30 (prd FR-30 [REP-01]): Management views per-Event participation: Registrations by status, Confirmed and Valid attendees, No-shows (Completed Events only), attendance rate (N/A with no Confirmed), attendee course/year distribution from signup snapshots, and detail rows (name, student number, course, registration status, attendance result, check-in time). Totals cover the full dataset; inactivity never hides history.
- R31 (prd FR-31 [ADM-01]): An Administrator assigns the Member, Officer or Administrator Role to an Account. The last active Administrator cannot be demoted. Roles are fixed.
- R32 (prd FR-32 [ADM-01]): Before a membership-status or Account-access deactivation the screen previews affected upcoming Registrations and requires a reason. Deactivation cancels the person's Registrations on not-yet-started Events that lack Valid Attendance, invalidates their QR Passes and releases Seats without Promotion; Account deactivation ends existing sessions; the last active Administrator cannot be deactivated. Reactivation restores access only; issued Certificates are kept.
- R33 (prd FR-33 [ADM-02]): Every consequential action FR-33 lists appends one Activity Log entry (actor, action, entity type and ID, reason, UTC time, validated old/new JSON) in the same commit as the change. Entries never hold passwords, hashes, QR tokens or file paths, and are never updated or deleted.
- R34 (prd FR-34 [ADM-02]): An Administrator views a read-only, newest-first Activity Log showing actor, action, affected record, time and reason.
- R35 (prd FR-35): A one-time setup script creates the first active Administrator from a supplied name, email and password stored as a hash; no default credentials exist and public requests never grant privileged Roles.
- R36 (prd FR-36): Every list screen FR-36 names shows 25 rows per page with Previous/Next, deterministic ordering with a unique-ID tie-break, filters and authorization applied before counting, missing/invalid pages showing page 1 and out-of-range pages the last page, hidden navigation on one page, and an empty state.

## Constraints

- Passwords only as salted Argon2id hashes; signed expiring cookie sessions; antiforgery on every mutation; ownership check on every personal resource; every time, capacity and state guard on the server; secrets, hashes and QR tokens never logged; HTTPS on every non-localhost deployment; no login throttling or lockout. (prd NFR-1; addendum A; spine AD-8, AD-15)
- Seat allocation, Promotion, approval, issuance and attendance correction run in one caller-owned SQL transaction with the audit write, serialized where they compete; uniqueness enforced in the database: Account email, Member student number, Registration per Member/Event, Attendance per Registration, Certificate per Registration. (prd NFR-2; spine AD-3, AD-4, AD-10)
- No hard deletes of business records and no archive/restore; Activity Log append-only: the schema blocks UPDATE and DELETE on activity_logs and the demo host's runtime login has only SELECT and INSERT on it. (prd NFR-3; addendum B.12; spine AD-7, AD-16)
- Images and Certificates on persistent server-local storage outside the web root and deployment tree, separate venue/event/certificate directories, relative paths only in SQL, backed up with SQL Server. (prd NFR-4; spine AD-12)
- Times stored in UTC as datetime2(3), every guard compared against one injected clock, and shown in Asia/Manila with a label; no scheduled or background jobs. (prd NFR-5; spine AD-11)
- One ASP.NET Core MVC app and one SQL Server database on one host, reached on localhost or intranet; Chrome target; mobile-responsive for phone check-in over a hotspot; laptop layouts at 1920x1080. (prd NFR-6; spine AD-16)
- The QR Pass carries no personal data; unknown-token and unauthorized responses reveal nothing about other records; email is required but unverified. (prd NFR-7)
- .NET 10 (net10.0 LTS) MVC with Razor views on every project; EF Core database-first from the single canonical schema script with fluent mapping and no EF migrations; layers WebApp -> Services -> Data; Tailwind compiled into the app with no runtime CDN; Playwright .NET with Chromium renders the certificate PDF; QR PNG rendered server-side from registrations.qr_token. (addendum A; spine AD-1, AD-10, AD-14, AD-18)
- States are seeded lookup tables with foreign keys; event_types.description is NOT NULL; certificates carries issuer_name_snapshot at issuance. (addendum A, B.10, B.11; spine AD-10)
- Git Flow: feature/* and fix/* branches, PRs to dev with green CI (build plus both test projects against a SQL Server container) and one peer approval, Tier 3 manual E2E before release to main; Conventional Commits. (addendum D; spine AD-16, AD-17)
- Every epic builds against the architecture spine: one writing module per table, cross-module writes only through the owner's service or a hook it declares; one caller-owned transaction per mutating use case with row locks in the AD-4 order; one set of participation facts, one cancellation path, one audit writer, one outcome contract, one private file store, one pager and one UI shell. (spine AD-2..AD-14)
- SPEC v2.1 (docsss/AttendTa-System-Specification.md) is authoritative unless PRD 9 records an override. (prd 0, 9)

## Done when

- Every SPEC §20 acceptance scenario passes in the client demo (prd SM-1, 100%).
- All 26 functional IDs, FR-35 and FR-36 are demonstrable end to end on the integrated `dev` build, released to `main` after Tier 3 manual E2E (prd SM-2).
- Every merged PR had green CI and at least one peer approval (prd SM-3).
- Scripted SQL Server concurrency tests show zero overbooked Events, zero duplicate Attendance Records and zero duplicate Certificates (prd SM-4).
- On the demo host, a phone scans a QR Pass in Chrome over the hotspot under trusted HTTPS, and stored images and Certificates survive an app restart and redeploy (prd NFR-4, NFR-6, Q2).

## Boundaries

- Out: multiple organizations, guests, sponsors or speakers with accounts (prd 2.2).
- Out: email, SMS or push notifications; email verification, password reset or change, SSO, session/device management (prd 6).
- Out: automatic Waitlist promotion, auto-completion, scheduled jobs, mass auto-cancellation (prd 6).
- Out: certificate template editor, bulk issuance, revocation, reissue; report export; advanced log filters; venue booking conflicts (prd 6).
- Out: every item deferred in prd 7.2 (known-password change, reopening declined certificates, re-check-in after invalidation, post-publish reschedule or capacity change, self-service profile edits and pictures, applicant status tracking, custom permissions, in-system QR printing).
- Touch point: visual prototype (`F:/AttendTa`, imported under `ux-attendta/imports/`) — read-only visual reference for Razor views; never edited; owner: project owner (prototype repository), consumed by every epic's views.
- Tracer path: admin setup → applicant request → approval → email login → event type and venue → draft with cover → publish → signup → confirmation → QR check-in → completion → eligibility approval → PDF issuance and download → participation report and Activity Log.

## References

- prd — `_bmad-output/initiative-attendta/prd-attendta/prd-attendta.md`, sections 3 Glossary, 4 Features (FR-1..FR-36), 5 NFRs, 7 Scope, 8 Success Metrics
- prd addendum — `_bmad-output/initiative-attendta/prd-attendta/addendum.md`, A stack, B schema tensions, C delivery ownership, D quality gates
- spec — `docsss/AttendTa-System-Specification.md` (SPEC v2.1, owner decisions 2026-11-04), §20 acceptance coverage
- architecture spine — `_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md`, AD-1..AD-18, Consistency Conventions, Stack, Capability → Architecture Map, Deferred
- architecture source — `docsss/architecture.md` v1.1, distilled into the spine
- design — `docsss/design.md` (visual specification), `_bmad-output/initiative-attendta/ux-attendta/DESIGN.md`
- ux — `_bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md`
- user stories — `AttendTa-User-Stories.md` (US-01..US-29)
- schema — `database/attendta-mvp-schema.sql`; `docsss/AttendTa-MVP.dbml`
- qa process — `docsss/git_qa_guidelines.pdf` (branching, Tier 1–3 gates)

## Notes

- Assumption: User journeys UJ-1..UJ-5 were derived from SPEC/HLD flows; confirm against the demo script (prd 11).
- Assumption: FR-13 "concluded" means the Officer judges the event over; the system enforces only now ≥ start (prd 11).
- Assumption: FR-27 Management may approve eligibility for their own participation; no self-approval prohibition (prd 11).
- Assumption: `newdocs/` is a superseded 2026-11-01 snapshot (it gives Administration to Member 1); `docsss/` SPEC (owner decisions 2026-11-04) and root `AttendTa-User-Stories.md` are current.
- Open question: Q1 — authorized certificate signatory name, title and signature not yet supplied; PM (Talisic) owns it; needed before official issuance, not before build.
- Open question: Q2 — hotspot storage, host address and trusted HTTPS for phone scanning; Technical Manager (Gabutan) owns it; before the demo.
- Open question: spine wording — AD-18 moves all Microsoft.* packages in lockstep but the Stack pins Microsoft.Playwright 1.64.1 and Microsoft.TypeScript.MSBuild versions separately; AD-18 lists Hangfire.Core under Data although only Services references it; AD-4 forbids process-local locks while Deferred lets epic-certificates bound renderer concurrency; owner: architecture spine (`bmad-architecture`); before epic-certificates.
- Open question: R30 shows the attendance rate for any Event with Confirmed > 0; SPEC §15 shows it only for Completed Events; recorded in epic-participation-reports.
- Source conflict: R3, prd NFR-1 — code signs every login in as user `0` (`AccountController.cs:88-91`, `SignInManager.cs:63`), logs in by `UserId` not email, and stores passwords with reversible AES keyed from the JWT secret (`PasswordManager.cs`); PRD requires email login and Argon2id.
- Source conflict: R1, R2 — public `Account/Register` creates login accounts directly (`AccountController.cs:119-134`), bypassing Account Request review.
- Source conflict: R8 — `VenueImageStorage.cs:13`, `AGENTS.md` and `docs/database-setup.md` allow 5 MiB images; PRD FR-8, SPEC and architecture §8 cap them at 2 MiB.
- Source conflict: Constraints (lookup tables, description, issuer snapshot) — `database/attendta-mvp-schema.sql` uses string CHECK states, a nullable `event_types.description` and no `issuer_name_snapshot`; addendum A/B and architecture §5 require lookup FKs, NOT NULL and the snapshot column.
- Source conflict: Constraints (Tailwind) — `_Layout.cshtml` loads Bootstrap 4.3.1; addendum A and architecture §9 require compiled Tailwind.
- Decision: 2026-11-04 — epics cut one per PRD capability (one deployable web app, so no deployment split); they coincide with addendum C owners. Build order: platform-access, master-data, events-registration, check-in, administration, certificates, participation-reports, release-acceptance.
- Decision: 2026-11-04 — events and registration stay one epic (shared cancellation and seat-allocation service, one owner); split into lifecycle/registration declined.
- Decision: 2026-11-04 — code conflicts resolve toward PRD and architecture: 2 MiB images (AGENTS.md and setup docs corrected), lookup tables with FKs, compiled Tailwind replacing Bootstrap, Argon2id replacing AES, public Register removed.
- Decision: 2026-11-04 — R35 setup script and the R36 pager helper live in epic-platform-access because the tracer path needs them first; their stories keep addendum owners (Pinca, Member 2).
- Decision: 2026-11-04 — epic-check-in verifies scanning with a laptop camera on localhost; phone scanning over the hotspot with trusted HTTPS is verified in epic-release-acceptance (Q2).
- Decision: 2026-11-04 — keep a closing release-acceptance epic for SPEC §20, SM-4, hotspot HTTPS, backup/restore and the dev-to-main release.
- Decision: 2026-11-04 — `docsss/architecture.md` v1.1 adopted as the shared-decision home (transactions, storage, auth, lookups); no separate architecture run.
- Decision: 2026-11-04 — every epic is refined from the initiative's lines alone and planned into stories in this run.
- Decision: 2026-11-04 — architecture spine `architecture-attendta/architecture-attendta.md` (AD-1..AD-18) adopted as the shared-decision home, superseding the decision above that named `docsss/architecture.md` with no separate run; `docsss/architecture.md` stays a companion as the spine's source.
- Decision: 2026-11-04 — every project targets net10.0 LTS (spine AD-18, resolved in the architecture run), replacing .NET 9; the AGENTS.md roll-forward note becomes obsolete.
- Decision: 2026-11-04 — epic-events-registration writes every AD-5 fact in `ParticipationFacts.cs`, Valid Attendance included as the spine defines it; epic-check-in owns later changes to Valid Attendance; epic-master-data builds only active Event Type and Venue reads.
- Decision: 2026-11-04 — spine re-slice: unstarted entries revised in place with ids kept; new entries e1.10 (net10 baseline, test projects, CI), e1.11 (master-data unit-of-work migration), e3.15 (participation-cancellation service); epic-platform-access builds baseline, then schema, then the login tracer. Cut: Activity Log search (Boundaries: advanced log filters out) and report status filtering (not in R30).
