---
type: epic
title: "Members sign in to a real, audited platform"
parent: initiative-attendta
covers: [R1, R2, R3, R33, R35, R36]
sources: [_bmad-output/initiative-attendta/initiative-attendta.md]
companions: [docsss/architecture.md, docsss/design.md, _bmad-output/initiative-attendta/ux-attendta/DESIGN.md, _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, docsss/AttendTa-System-Specification.md]
after: []
refined: true
risk: high
---

# Members sign in to a real, audited platform

## Description

Opening epic and platform baseline. Replaces the login stub, reversible AES passwords, the public Register page and Bootstrap with the PRD's account flow on a canonical schema: an Administrator created by the setup script approves Account Requests, approved Members log in by email, and every later epic reuses its Argon2id hasher, transactional audit writer, 25-row pager, role-aware Tailwind shell, local-time display and CI test harness.

## Outcome

For applicants and Management, membership moves from in-person chasing to a reviewed request and a real email login, shown by UJ-1 running end to end on the dev build.

## Requirements

- R1 (parent R1): An Applicant submits an Account Request (student number, full name, email, course, year level, section, password and confirmation; optional mobile). Missing fields, year level <= 0, malformed email, mismatched confirmation, or an email/student number used by a pending request, Account or Member Profile is refused with field errors. Password kept only as a hash; Applicant sees an acknowledgment only and cannot choose Role or membership values.
- R2 (parent R2): Management approves or rejects a pending Account Request. Approval atomically creates an active Member Account and its Member Profile, moves the hash, re-checks email and student-number uniqueness, and is audited. Rejection needs a reason, clears the hash and creates no Account. Concurrent decisions yield one outcome; a finalized request cannot be reviewed again.
- R3 (parent R3): An active Account logs in with its email and password and logs out. Every failure is generic. Success issues a signed, expiring cookie session routed by Role. Every protected request re-checks current Account active state and Role.
- R4 (parent R35): A one-time setup script creates the first active Administrator from a supplied name, email and password stored as a hash; no default credentials exist and public requests never grant privileged Roles.
- R5 (parent R33): The shared Activity Log writer appends one entry per action inside the caller's transaction (actor, action, entity, reason, UTC time, validated old/new JSON), refuses passwords, hashes, QR tokens and file paths, and the runtime database role cannot update or delete activity_logs; Account Request decisions are logged through it.
- R6 (parent R36): A shared 25-row pager implements every FR-36 rule (deterministic order with ID tie-break, filters and authorization before counting, page fallbacks, hidden single-page navigation, empty state) and pages the Account Request queue.

## Constraints

- Passwords only as salted Argon2id hashes; signed expiring cookies; antiforgery on every mutation; ownership checks on personal resources; every time, capacity and state guard on the server; secrets, hashes and QR tokens never logged; no login throttling (parent Constraints; prd NFR-1).
- Multi-record mutations run in one caller-owned SQL transaction with the audit write, serialized where they compete; uniqueness enforced in the database (parent Constraints; prd NFR-2; architecture §7).
- No hard deletes; Activity Log append-only (parent Constraints; prd NFR-3).
- UTC datetime2(3) storage, Asia/Manila display with a label, no scheduled jobs; every time guard evaluated per request (parent Constraints; prd NFR-5).
- One MVC app and one SQL Server database; Chrome target; mobile-responsive (parent Constraints; prd NFR-6).
- .NET 9 MVC, EF Core database-first, WebApp → Services → Data, compiled Tailwind with no runtime CDN (parent Constraints; architecture §3, §9).
- States are seeded lookup tables with FKs; event_types.description NOT NULL; certificates.issuer_name_snapshot (parent Constraints; architecture §5).
- feature/* or fix/* branches, PRs to dev with green CI and a peer approval, Conventional Commits (parent Constraints; addendum D).
- SPEC v2.1 in docsss/ is authoritative for rule detail unless prd §9 overrides (parent Constraints).

## Done when

- On the dev build, an Administrator provisioned by the setup script approves an Account Request and the new Member logs in by email and lands on the role-routed Tailwind shell (UJ-1); a rejected Applicant can submit a new request.
- The canonical schema script (lookup tables with FKs, NOT NULL event_types.description, issuer_name_snapshot, append-only activity_logs grants) applies cleanly to AttendTaMVP_Test, and EF maps all ten tables, the lookups and the MVP users shape.
- The login stub, AES PasswordManager, public Account/Register and Bootstrap are gone; a deactivated or demoted session is refused on its next request.
- A test project runs in GitHub Actions on every PR, including SQL Server tests for concurrent request approval and request/Account uniqueness.
- Merged to dev with the tracer path verified on the integrated build.

## Boundaries

- Belongs to epic-master-data: Member, Event Type and Venue screens and the image storage; this epic only makes their actor and role claims real.
- Belongs to epic-administration: role assignment, Account deactivation screens and the Activity Log viewer.
- Touch point: `MemberService.CreateMemberFromApprovedAccountRequest` — called by approval; owner: epic-master-data.
- Must not change: Master Data's no-standalone-member-creation rule and its existing uniqueness checks.

## References

- parent — _bmad-output/initiative-attendta/initiative-attendta.md, Requirements R1, R2, R3, R33, R35, R36
- architecture — docsss/architecture.md, §5 database, §6 authentication, §7 transactions, §9 frontend, §10 startup checklist
- spec — docsss/AttendTa-System-Specification.md, §3–§4 accounts and login, §17–§19 audit, schema, lists
- design — _bmad-output/initiative-attendta/ux-attendta/DESIGN.md; docsss/design.md §4 page shells
- ux — _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, UJ-1

## Notes

- Owner: Member 1 (Gabutan) for authentication; Pinca for the setup script (R35); Member 2 for the pager helper (addendum C; parent Decision 2026-11-04).
- Decision: 2026-11-04 — refined from the initiative's lines alone.
- Source conflict: R1, R2, R3 — see parent Notes: login stub, AES passwords, public Register; this epic removes them.
- Decision: 2026-11-04 — tracer bullet: entry 1, Provisioned administrator logs in by email via Argon2id into authenticated session with active role checks; next: entry 2, Canonical database schema update and EF Core mapping for all ten tables with lookup foreign keys (least certain); entry 1 split to separate compiled Tailwind CSS pipeline and navigation shell into entry 9.
- Decision: 2026-11-04 — entry 1 stands up xUnit test project and CI baseline as earliest consumer; entry 7 implements automated SQL Server concurrency integration tests for concurrent approval and uniqueness.
- Decision: 2026-11-04 — no closing end-to-end suite in this epic; epic-release-acceptance runs SPEC §20 across the build.
