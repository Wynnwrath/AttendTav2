---
type: epic
title: "The integrated build passes SPEC §20 and is demo-ready"
parent: initiative-attendta
covers: []
sources: [_bmad-output/initiative-attendta/initiative-attendta.md]
companions: [docsss/architecture.md, docsss/design.md, _bmad-output/initiative-attendta/ux-attendta/DESIGN.md, _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, docsss/AttendTa-System-Specification.md]
after: []
refined: true
risk: medium
---

# The integrated build passes SPEC §20 and is demo-ready

## Description

Delivers the initiative's Done when on the integrated build: the full SPEC §20 acceptance run, the owners' concurrency tests together, the demo host with trusted HTTPS for phone scanning over the hotspot, persistent storage with backup and restore, and the Tier 3 release from dev to main.

## Outcome

For the client and approver, the MVP is shown to meet every acceptance item, signalled by SM-1 and SM-2 at 100%.

## Requirements

- R1 (prd SM-1; spec §20): Every SPEC §20 acceptance scenario passes on the integrated dev build, with the evidence recorded.
- R2 (prd SM-2): All 26 functional IDs, FR-35 and FR-36 are demonstrated end to end on the integrated dev build.
- R3 (prd SM-4): The feature epics' seat, attendance and certificate concurrency tests pass together on SQL Server with zero overbooking or duplicates.
- R4 (prd NFR-1, NFR-6, Q2): Officers' phones scan a QR Pass in Chrome over the hotspot through trusted HTTPS on the demo host, with manual lookup still available.
- R5 (prd NFR-4): SQL Server and stored images and Certificates survive app restart and redeploy and are backed up and restored together into a disposable location.
- R6 (prd SM-3; addendum D): Tier 3 manual E2E passes and dev is released to main.

## Constraints

- Passwords only as salted Argon2id hashes; signed expiring cookies; antiforgery on every mutation; ownership checks on personal resources; every time, capacity and state guard on the server; secrets, hashes and QR tokens never logged; no login throttling (parent Constraints; prd NFR-1).
- Files on persistent server-local storage outside the web root, separate venue/event/certificate directories, relative paths only in SQL (parent Constraints; prd NFR-4; architecture §8).
- One MVC app and one SQL Server database; Chrome target; mobile-responsive (parent Constraints; prd NFR-6).
- feature/* or fix/* branches, PRs to dev with green CI and a peer approval, Conventional Commits (parent Constraints; addendum D).
- SPEC v2.1 in docsss/ is authoritative for rule detail unless prd §9 overrides (parent Constraints).

## Done when

- Every SPEC §20 acceptance scenario passes on the integrated dev build (SM-1).
- All 26 functional IDs, FR-35 and FR-36 are demonstrated end to end (SM-2).
- The seat, attendance and certificate concurrency tests pass together (SM-4).
- A phone in Chrome scans a QR Pass over the hotspot under trusted HTTPS, and SQL plus stored files survive restart, redeploy and a restore to a disposable location (NFR-4, NFR-6, Q2).
- Tier 3 manual E2E passes and dev is released to main (SM-3).

## Boundaries

- Belongs to the feature epics: fixing behavior; this epic runs and records acceptance and reports defects back as bugs.
- Out: production hosting, uptime, cloud services (architecture §10).

## References

- parent — _bmad-output/initiative-attendta/initiative-attendta.md, Done when
- prd — _bmad-output/initiative-attendta/prd-attendta/prd-attendta.md, §8 Success Metrics, §10 Q2
- spec — docsss/AttendTa-System-Specification.md, §20 acceptance coverage
- architecture — docsss/architecture.md, §10 local and hotspot configuration, §11 verification
- qa process — docsss/git_qa_guidelines.pdf, Tier 3

## Notes

- Owner: team; Technical Manager (Gabutan) for the hotspot and HTTPS setup (Q2).
- Open question: Q2 — host address and trusted HTTPS certificate for officers' phones (parent Notes).
- Decision: 2026-11-04 — refined from the initiative's Done when; covers no requirement id because it verifies the whole set.
- Waits on epic-platform-access, epic-master-data, epic-events-registration, epic-check-in, epic-administration, epic-certificates and epic-participation-reports because: the acceptance run needs every finished feature epic (8.5 waits on each closing entry), the concurrency run needs 3.7, 4.3 and 6.7, phone scanning needs 4.4, and host provisioning needs the Playwright runtime from 6.1.
- Decision: 2026-11-04 — tracer bullet: entry 1, Demo host runtime environment provisioning; next: entry 2, Hotspot network binding and trusted HTTPS mobile camera scanning (least certain).
- Decision: 2026-11-04 — sequencing: host provisioning (1) unlocks hotspot scanning (2) and backup/restore (3); concurrency (4) and automated SPEC §20 (5) run once feature epics land; manual checklist (6) and functional demo walkthrough (7) gate Tier 3 regression (8) before dev-to-main release (9); sweep (10) closes.
- Decision: 2026-11-04 — keep entry 10 (Refactor sweep) although the epic is verification-only; the suggestion to drop it was declined by the user.
