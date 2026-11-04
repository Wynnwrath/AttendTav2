---
type: epic
title: "Administrators control roles, access and the audit trail"
parent: initiative-attendta
covers: [R5, R31, R32, R33, R34, R36]
sources: [_bmad-output/initiative-attendta/initiative-attendta.md]
companions: [docsss/architecture.md, docsss/design.md, _bmad-output/initiative-attendta/ux-attendta/DESIGN.md, _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, docsss/AttendTa-System-Specification.md]
after: []
refined: true
risk: medium
---

# Administrators control roles, access and the audit trail

## Description

Gives Administrators control of Roles and Account access and gives Management the membership-status change, both with a preview of affected upcoming Registrations, a reason, and cancellation through the Registration-owned service. It protects the last active Administrator and adds the read-only Activity Log viewer.

## Outcome

For Administrators, cutting off a departed officer is one attributable action, shown by UJ-5 on the dev build.

## Requirements

- R1 (parent R5): Setting membership to inactive makes the Member no longer an Eligible Participant and applies the FR-32 preview, reason and upcoming-Registration cancellation without changing Account access.
- R2 (parent R31): An Administrator assigns the Member, Officer or Administrator Role to an Account. The last active Administrator cannot be demoted. Roles are fixed.
- R3 (parent R32): Before a membership-status or Account-access deactivation the screen previews affected upcoming Registrations and requires a reason. Deactivation cancels the person's Registrations on not-yet-started Events that lack Valid Attendance, invalidates their QR Passes and releases Seats without Promotion; Account deactivation ends existing sessions; the last active Administrator cannot be deactivated. Reactivation restores access only; issued Certificates are kept.
- R4 (parent R33): Role, access and membership changes each append an Activity Log entry.
- R5 (parent R34): An Administrator views a read-only, newest-first Activity Log showing actor, action, affected record, time and reason.
- R6 (parent R36): The accounts list and the Activity Log list are paged.

## Constraints

- Passwords only as salted Argon2id hashes; signed expiring cookies; antiforgery on every mutation; ownership checks on personal resources; every time, capacity and state guard on the server; secrets, hashes and QR tokens never logged; no login throttling (parent Constraints; prd NFR-1).
- Multi-record mutations run in one caller-owned SQL transaction with the audit write, serialized where they compete; uniqueness enforced in the database (parent Constraints; prd NFR-2; architecture §7).
- No hard deletes; Activity Log append-only (parent Constraints; prd NFR-3).
- UTC datetime2(3) storage, Asia/Manila display with a label, no scheduled jobs; every time guard evaluated per request (parent Constraints; prd NFR-5).
- .NET 9 MVC, EF Core database-first, WebApp → Services → Data, compiled Tailwind with no runtime CDN (parent Constraints; architecture §3, §9).
- feature/* or fix/* branches, PRs to dev with green CI and a peer approval, Conventional Commits (parent Constraints; addendum D).
- SPEC v2.1 in docsss/ is authoritative for rule detail unless prd §9 overrides (parent Constraints).

## Done when

- UJ-5 runs on the dev build: an Administrator previews and deactivates an officer with a reason, the officer's session is refused on its next request, their upcoming Registrations are cancelled, and the Activity Log shows the change.
- A SQL Server test shows two concurrent demotions or deactivations cannot remove the last active Administrator.
- Membership inactivation cascades the same way without touching Account access; reactivation restores access only.
- The Activity Log viewer lists entries newest-first, read-only.
- Merged to dev with integrated verification.

## Boundaries

- Belongs to epic-events-registration: the participation cancellation logic, which this epic calls and never duplicates.
- Belongs to epic-platform-access: per-request active/role re-checks and the audit writer.
- Must not change: Master Data's other Member fields and their edit screen behavior.

## References

- parent — _bmad-output/initiative-attendta/initiative-attendta.md, Requirements R5, R31–R34, R36
- architecture — docsss/architecture.md, §4 module contracts, §7 last administrator
- spec — docsss/AttendTa-System-Specification.md, §5 deactivation, §17 audit
- design — _bmad-output/initiative-attendta/ux-attendta/DESIGN.md; docsss/design.md
- ux — _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, UJ-5

## Notes

- Owner: Pinca (addendum C). Admin-role pure rules exist on branch 7477e95 (architecture §1) and may be reused after validation.
- Decision: 2026-11-04 — refined from the initiative's lines alone.
- Waits on epic-master-data because: the Member edit screen whose status control this epic replaces.
- Waits on epic-events-registration because: the participation cancellation service.
- Waits on epic-check-in because: Valid Attendance exempts Registrations from cancellation.
- Waits on epic-platform-access because: the authenticated navigation shell, shared 25-row pager and Activity Log writer.
- Decision: 2026-11-04 — tracer bullet: entry 1, Console accounts list with role and status badges; next: entry 2, Role assignment with SQL Server guard for last-active-Administrator protection (least certain).
- Decision: 2026-11-04 — no closing end-to-end suite in this epic; epic-release-acceptance runs SPEC §20 across the build.
- Decision: 2026-11-04 — test-only entry 9 deleted and its UJ-5 verification folded into entry 4 in accordance with no closing end-to-end suite.
- Decision: 2026-11-04 — entry 1 hitl set to false as it requires no manual starter or user setup.
