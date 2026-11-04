---
type: epic
title: "Members sign in to a real, audited platform"
parent: initiative-attendta
covers: [R1, R2, R3, R33, R35, R36]
sources: [_bmad-output/initiative-attendta/initiative-attendta.md]
companions: [_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md, docsss/architecture.md, docsss/design.md, _bmad-output/initiative-attendta/ux-attendta/DESIGN.md, _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, docsss/AttendTa-System-Specification.md]
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

- Passwords only as salted Argon2id hashes; signed expiring cookies; antiforgery on every mutation; ownership checks on personal resources; every time, capacity and state guard on the server; secrets, hashes and QR tokens never logged; no login throttling (parent Constraints; prd NFR-1; spine AD-8, AD-15).
- Multi-record mutations run in one caller-owned SQL transaction with the audit write, serialized where they compete; uniqueness enforced in the database (parent Constraints; prd NFR-2; spine AD-3, AD-4, AD-10).
- No hard deletes; Activity Log append-only (parent Constraints; prd NFR-3).
- UTC datetime2(3) storage, Asia/Manila display with a label, no scheduled jobs; every time guard evaluated per request (parent Constraints; prd NFR-5; spine AD-11).
- One MVC app and one SQL Server database; Chrome target; mobile-responsive (parent Constraints; prd NFR-6).
- net10.0 LTS MVC (AD-18); WebApp → Services → Data, with Resources as the shared leaf (AD-1); fluent EF Core mapping against the canonical script, no EF migrations (AD-10); Tailwind compiled from DESIGN.md tokens, no runtime CDN (AD-14).
- States are seeded lookup tables with FKs; event_types.description NOT NULL; certificates.issuer_name_snapshot (parent Constraints; spine AD-10).
- feature/* or fix/* branches, PRs to dev with green CI and a peer approval, Conventional Commits (parent Constraints; addendum D; spine Consistency Conventions, Git row).
- SPEC v2.1 in docsss/ is authoritative for rule detail unless prd §9 overrides (parent Constraints).

## Done when

- On the dev build, an Administrator provisioned by the setup script approves an Account Request and the new Member logs in by email and lands on the role-routed Tailwind shell (UJ-1); a rejected Applicant can submit a new request.
- The canonical schema script (lookup tables with FKs, NOT NULL event_types.description, issuer_name_snapshot, activity_logs UPDATE and DELETE blocked by the INSTEAD OF trigger (AD-7), READ_COMMITTED_SNAPSHOT ON (AD-3), and the database-name allow-list (AD-10)) applies cleanly to AttendTaMVP_Test, and EF maps all ten tables, the lookups and the MVP users shape.
- The login stub, AES PasswordManager, SessionManager, JwtBearer and /api/token stack, MemberAccountSummary, PathManager, BCrypt, the committed TokenAuthentication:SecretKey, public Account/Register and Bootstrap are gone; a deactivated or demoted session is refused on its next request.
- Both xUnit v3 test projects (tests/ASI.Basecode.Services.Tests and tests/ASI.Basecode.IntegrationTests) run in GitHub Actions on every PR to dev against an mssql/server:2025-latest service container, including the SQL Server concurrency tests for approval and request/Account uniqueness (AD-16, AD-17).
- Merged to dev with the tracer path verified on the integrated build.
- Released to main through epic-release-acceptance (SM-3).

## Boundaries

- Belongs to epic-master-data: Member, Event Type and Venue screens, the image storage and the actorUserId removal. This epic converts their repositories and services to the caller-owned transaction and removes MemberAccountSummary (spine AD-3, AD-8, AD-10); it changes no master-data rule.
- Belongs to epic-administration: role assignment, Account deactivation screens and the Activity Log viewer.
- Touch point: `MemberService.CreateMemberFromApprovedAccountRequest` — called by approval; owner: epic-master-data. This epic changes only the transaction wrapping, async signatures and the approval call; master-data's rules and actorUserId parameters stay with epic-master-data (AD-2, AD-3, AD-8).
- Must not change: Master Data's no-standalone-member-creation rule and its existing uniqueness checks.

## References

- parent — _bmad-output/initiative-attendta/initiative-attendta.md, Requirements R1, R2, R3, R33, R35, R36
- architecture — _bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md (spine, status final): AD-1, AD-2, AD-3, AD-4, AD-7, AD-8, AD-9, AD-10, AD-11, AD-13, AD-14, AD-15, AD-16, AD-17, AD-18; Consistency Conventions (Diagnostics, Git); Stack; Structural Seed. docsss/architecture.md is detail only; where it conflicts (for example, .NET 9), the spine governs.
- spine Deferred — Argon2 m/t/p values (e1, benchmarked on the demo host); Konscious to Geralt swap (spine Stack and Deferred).
- prd addendum — _bmad-output/initiative-attendta/prd-attendta/addendum.md, A stack, C ownership, D quality gates
- spec — docsss/AttendTa-System-Specification.md, §3–§4 accounts and login, §17–§19 audit, schema, lists
- design — _bmad-output/initiative-attendta/ux-attendta/DESIGN.md; docsss/design.md §4 page shells
- ux — _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, UJ-1

## Notes

- Owner: Member 1 (Gabutan) for authentication; Pinca for the setup script (R35); Member 2 for the pager helper (addendum C; parent Decision 2026-11-04).
- Decision: 2026-11-04 — refined from the initiative's lines alone.
- Source conflict: R1, R2, R3 — see parent Notes: login stub, AES passwords, public Register; this epic removes them.
- Decision: 2026-11-04 — tracer bullet: entry 10 (net10.0 retarget, package removals, both xUnit v3 test projects, CI baseline) then entry 2 (canonical schema and IT fixture) then entry 1 (provisioned administrator logs in by email via Argon2id into authenticated session with active role checks); entry 1 split to separate compiled Tailwind CSS pipeline and navigation shell into entry 9.
- Decision: 2026-11-04 — entry 10 stands up both xUnit v3 test projects and CI baseline as earliest consumer (AD-17); entry 7 implements automated SQL Server concurrency integration tests for concurrent approval and uniqueness.
- Decision: 2026-11-04 — no closing end-to-end suite in this epic; epic-release-acceptance runs SPEC §20 across the build.
- Decision: 2026-11-04 — re-sliced against the architecture spine (AD-1..AD-18): added entry 10 for net10 retarget/test projects/CI, entry 11 for master-data UoW migration, adopted INSTEAD OF trigger and DB allow-list, reordered build sequence (10, 2, 1, 9, 3, 11, 4, 5, 6, 7, 8).
- Open question: the two `ASI - Backup.*.csproj` legacy copies beside the real projects still reference BCrypt, Hangfire, BulkExtensions, JwtBearer and AutoMapper 11; delete them in entry 10 or leave them outside the solution; owner: Technical Lead; before entry 10.
