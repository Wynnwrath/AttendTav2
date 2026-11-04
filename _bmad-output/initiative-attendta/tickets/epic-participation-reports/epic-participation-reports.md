---
type: epic
title: "Officers read per-event participation reports"
parent: initiative-attendta
covers: [R30, R36]
sources: [_bmad-output/initiative-attendta/initiative-attendta.md]
companions: [_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md, docsss/architecture.md, docsss/design.md, _bmad-output/initiative-attendta/ux-attendta/DESIGN.md, _bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md, docsss/AttendTa-System-Specification.md]
after: []
refined: true
risk: medium
---

# Officers read per-event participation reports

## Description

Gives Management a per-Event participation report computed from canonical Registration, snapshot and attendance facts: status counts, attendees, No-shows, attendance rate, attendee distribution and paged detail rows.

## Outcome

For the organization, participation evidence for advisers comes straight from recorded attendance, shown by the UJ-4 report figures.

## Requirements

- R1 (parent R30): Management views per-Event participation: Registrations by status, Confirmed and Valid attendees, No-shows (Completed Events only), attendance rate (N/A with no Confirmed), attendee course/year distribution from signup snapshots, and detail rows (name, student number, course, registration status, attendance result, check-in time). Totals cover the full dataset; inactivity never hides history.
- R2 (parent R36): Report detail rows are paged while totals cover the full dataset.

## Constraints

- Passwords only as salted Argon2id hashes; signed expiring cookies; antiforgery on every mutation; ownership checks on personal resources; every time, capacity and state guard on the server; secrets, hashes and QR tokens never logged; no login throttling (parent Constraints; prd NFR-1).
- UTC datetime2(3) storage, Asia/Manila display with a label, no scheduled jobs; every time guard evaluated per request (parent Constraints; prd NFR-5; AD-11, AD-16).
- net10.0 MVC (AD-18), EF Core database-first, WebApp → Services → Data (AD-1), compiled Tailwind with no runtime CDN (AD-14).
- feature/* or fix/* branches, PRs to dev with green CI and a peer approval, Conventional Commits (parent Constraints; addendum D).
- SPEC v2.1 in docsss/ is authoritative for rule detail unless prd §9 overrides (parent Constraints).

## Done when

- On the dev build the report for a completed Event shows status counts, Confirmed and Valid attendees, No-shows, rate and distribution matching hand-computed figures.
- Zero Confirmed shows N/A; invalidated attendance and pending/waitlisted/cancelled rows are excluded as FR-30 states, under test.
- Detail rows show the FR-30 fields with 25-row paging and full-dataset totals.
- Merged to dev with integrated verification.

## Boundaries

- Out: report export (parent Boundaries).
- Must not change: reports never mutate participation records; reads only (AD-2).

## References

- parent — _bmad-output/initiative-attendta/initiative-attendta.md, Requirements R30, R36
- spine — _bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md: AD-5 (ParticipationFacts, No-show, Attendance rate), AD-2 (reads only), AD-13 (paging), AD-11 (time), AD-14 (shell), AD-8 (Management policy), AD-18 (net10.0); Capability map row R30. docsss/architecture.md §9 is companion context only.
- spec — docsss/AttendTa-System-Specification.md, §15 reports
- design — _bmad-output/initiative-attendta/ux-attendta/DESIGN.md; docsss/design.md

## Notes

- Owner: Pinca (addendum C).
- Decision: 2026-11-04 — refined from the initiative's lines alone.
- Open question: R30 shows the attendance rate for any Event with Confirmed > 0; SPEC §15 shows it only for Completed Events; SPEC is authoritative unless prd §9 overrides; owner: PM; before entry 1.
- Decision: 2026-11-04 — re-sliced against the architecture spine (AD-2, AD-5, AD-8, AD-11, AD-13, AD-14, AD-18): report consumes canonical ParticipationFacts from e3 without defining facts or mutating data, cuts unneeded status filtering, uses unique-id tiebreak for paging, and gates on Management policy.
- Waits on epic-platform-access because: role claims, Tailwind shell, and shared 25-row pager helper.
- Waits on epic-events-registration because: Registration states, snapshots and Completed Events.
- Waits on epic-check-in because: Attendance Records with invalidation.
- Decision: 2026-11-04 — tracer bullet: entry 1, Event selector and participation summary KPIs; next: entry 2, Completed-event no-shows and signup snapshot attendee distributions (least certain); then: entry 3, Paged participant detail rows with full-dataset totals completing the report view.
- Decision: 2026-11-04 — entry 3 waits on entry 2 because participant detail paging verifies stability of full-dataset totals and attendee distributions, and both extend the report view (single ordered lane).
- Decision: 2026-11-04 — three entries total (≤ 3); no refactor sweep required per slice rules.
- Decision: 2026-11-04 — no closing end-to-end suite in this epic; epic-release-acceptance runs SPEC §20 across the build.
