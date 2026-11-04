---
type: epic
title: "Management maintains members, event types and venues"
parent: initiative-attendta
covers: [R4, R5, R8, R6, R7, R33, R36]
sources: [_bmad-output/initiative-attendta/initiative-attendta.md]
companions: [docsss/architecture.md, docsss/design.md, _bmad-output/initiative-attendta/ux-attendta/DESIGN.md, _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, docsss/AttendTa-System-Specification.md]
after: []
refined: true
risk: medium
---

# Management maintains members, event types and venues

## Description

Brings the existing Member, Event Type and Venue code to the PRD rules on the real login: Management edits Member Profiles (all fields but membership status), maintains Event Types and Venues with photos, and a Member reads their own profile. It owns the shared private image storage that Event Covers reuse.

## Outcome

For Management, the reference data every Event depends on is maintained in one audited place, shown by an Officer editing a member, a type and a venue with photo on the dev build.

## Requirements

- R1 (parent R4): A Member views their own account, academic and membership details; nothing is editable.
- R2 (parent R5): Management edits a Member's full name, course, year level, section, mobile and membership type; a student-number correction requires a reason, stays unique and logs old/new values; login email is read-only; edits never rewrite Registration snapshots or issued Certificates. The membership-status change belongs to epic-administration.
- R3 (parent R6): Management creates, edits, activates and deactivates Event Types with a unique, non-blank name and a non-blank description. Inactive types cannot be chosen for new or draft Events; existing references are kept.
- R4 (parent R7): Management maintains Venues: room name, positive capacity, location, availability notes, contact person, active state and a required photo. Capacity never drops below the participant limit of a linked draft or published Event. Inactive Venues cannot be chosen for new or draft Events.
- R5 (parent R8): The shared private image storage accepts JPEG/PNG verified by content, at most 2 MiB, under server-generated names in separate venue and event directories, keeping the previous image until the new path commits; Venue photos are uploaded, replaced and displayed through an authorized route. Event Cover upload belongs to epic-events-registration.
- R6 (parent R33): Profile and identity corrections, Event Type and Venue changes, and image changes each append an Activity Log entry.
- R7 (parent R36): The Members, Event Types and Venues lists are paged with the shared pager.

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

- On the dev build an Officer corrects a member's student number with a reason, creates, edits and toggles an Event Type and a Venue with a photo, and each change shows old/new values in activity_logs.
- Images over 2 MiB or whose content is not JPEG/PNG are refused, a failed write keeps the old photo, and Venue photos render through an authorized route.
- A Member opens their own read-only profile; another Member's profile is not reachable.
- Venue capacity below a linked draft/published Event's limit and inactive types/venues for new drafts are refused under test.
- AGENTS.md and docs/database-setup.md state the 2 MiB limit; merged to dev with integrated verification.

## Boundaries

- Belongs to epic-administration: the membership-status change, its preview and cascade.
- Belongs to epic-events-registration: Event Cover upload and Event forms.
- Must not change: no standalone member creation; members are created only by Account Request approval.
- Must not change: no hard deletes of members, types or venues.

## References

- parent — _bmad-output/initiative-attendta/initiative-attendta.md, Requirements R4–R8, R33, R36
- architecture — docsss/architecture.md, §5 database, §8 images
- spec — docsss/AttendTa-System-Specification.md, §6 master data and §14.5 storage
- design — _bmad-output/initiative-attendta/ux-attendta/DESIGN.md; docsss/design.md

## Notes

- Owner: Member 2 (addendum C).
- Decision: 2026-11-04 — refined from the initiative's lines alone.
- Decision: 2026-11-04 — images capped at 2 MiB per PRD FR-8; the existing 5 MiB limit in VenueImageStorage.cs, AGENTS.md and docs/database-setup.md is corrected here.
- Waits on epic-platform-access because: real login with role and numeric actor claims, the audit writer, the pager and the canonical schema.
- Decision: 2026-11-04 — tracer bullet: entry 1, Event Type management with required description, audit logging, and paged Tailwind list; next: entry 2, Shared 2 MiB private image storage and authorized photo serving route (least certain).
- Decision: 2026-11-04 — no closing end-to-end suite in this epic; epic-release-acceptance runs SPEC §20 across the build.
- Decision: 2026-11-04 — sequencing: Entry 1 (tracer bullet) establishes Tailwind CRUD and audit patterns; Entry 2 (least certain) establishes shared storage and doc limits; Venues follow in Entries 3-4 (create/edit with capacity guard and paged list); Entry 5 exposes active reference and eligibility query contracts once types and venues are ready; Member profile management and paging run in Entries 6-7, with Member self profile in Entry 8; Entry 9 closes with refactor sweep.
- Decision: 2026-11-04 — boundary: membership status mutation is removed from Member edit and index in this epic, leaving status read-only until epic-administration wires the deactivation modal and registration cancellation cascade.
- Decision: 2026-11-04 — venue capacity guard against linked draft/published events is verified under test/seeding in this epic prior to epic-events-registration building the event creation UI.
