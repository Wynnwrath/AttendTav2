# Reconciliation: PRD + Addendum vs Project Plan (v2.00 / rev 1.04) and Git & QA Guidelines

> **Historical snapshot (2026-11-04).** This review predates the latest owner-confirmed decisions and source-document alignment. Treat its findings as historical; the current `prd-attendta.md`, `addendum.md`, and current `docsss/` sources govern.

Scope: `docsss/Parent_Project_Plan_J4S_AttendTa_v2.00.pdf` (Plan) and `docsss/git_qa_guidelines.pdf` (QA) checked against `prd-attendta.md` (PRD) and `addendum.md` (ADD). Read-only on all sources. Recorded user decisions (SPEC wins; student-number login override; certificates in MVP; no domain restriction; acceptance-based metrics; 5–8 page target) are not reported as gaps.

Verification limits: Plan p.4 and p.6 checkbox states (project type, mode of access, browser boxes) could not be rendered (image renderer error), so those are read from extracted text only. The WBS, SPEC and UC were not in this assignment's inputs; WBS figures are taken from ADD §C and PRD §10 Q2 unverified.

Severity: critical / high / medium / low.

## 1. Platform and environment

- **[high] Phone check-in over intranet is not technically addressed.** Source: Plan p.6 (Server: localhost; Mode of Access: intranet per rev 1.04 remark); Plan p.6 (Mobile Responsive: Yes). PRD location: NFR-6; FR-23. ADD location: §A Environments. Issue: PRD NFR-6 says check-in works on an officer's phone, and FR-23 requires camera QR scanning. Browser camera access (getUserMedia) is normally limited to secure contexts (HTTPS or localhost) [INFERENCE: verify on target phone/browser]. A phone on the intranet reaching `http://<lan-host>` would be blocked, and a phone cannot reach a laptop's `localhost`. Fix: state the scanner host (LAN-reachable server), require HTTPS (internal certificate) for check-in, or record that scanning is laptop-only with manual fallback (FR-24) for phones. Add an environment row to ADD §A.

- **[high] Environment for acceptance is undefined.** Source: Plan p.6 (Staging/QA and Production blank; Server information: localhost). PRD location: NFR-4; SM-1 ("persistent storage across redeploy"); §10 Q4. ADD location: §A Environments. Issue: the acceptance scenario for persistent storage across redeploy cannot be shown on a localhost-only environment, and no staging host exists. ADD §A "one application server assumed" is not tagged [ASSUMPTION]. Fix: define the demo/staging environment (host, storage root, backup job) or state that redeploy is simulated; tag the single-server assumption in ADD §A and PRD §11.

- **[medium] NFR-6 and PRD §11 misstate the browser decision as "Chrome-only".** Source: Plan p.6 (Chrome: Yes; Firefox, Safari, Internet Explorer, Microsoft Edge: blank, not "No"). PRD location: NFR-6 ("Chrome is supported"); §11 bullet for NFR-6 ("Chrome-only support ... come from Plan p.6"). Issue: the Plan does not say Chrome-only; blanks are undecided. The PRD presents an unrecorded decision as sourced. Fix: restate NFR-6 as an explicit decision (e.g., "Supported: Chrome desktop and Android Chrome; other browsers unsupported for MVP"), confirmed by the PM, and cite it in §9 rather than §11.

- **[medium] Phone platform is unspecified.** Source: Plan p.6 (iOS, Android, Windows blank; Mobile Responsive: Yes). PRD location: NFR-6 ("layouts are mobile-responsive so check-in works on an officer's phone"); §11 (mobile-responsive "assumed sufficient"). Fix: name the phone OS/browser used for check-in and a test device in SM-1 acceptance.

- **[medium] No browser-side QR decoding decision.** Source: ADD §A QR row (server PNG rendering only); Plan p.10 risk "broken scanner routes". PRD location: FR-21, FR-23. Issue: the scan path (camera decode library, permission flow, fallback) is not addressed; only generation is. Fix: add a row to ADD §A marked "not chosen; owner: architecture", and note the camera/HTTPS dependency from item 1.

- **[medium] Timezone open question is partly answerable from the Plan.** Source: Plan p.4 (Holiday calendar: Alliance Cebu), Plan p.10 (Metro Cebu power risk). PRD location: NFR-5; §10 Q1 (blocking for acceptance). Issue: the Plan places the client and team in Cebu, Philippines, so the organization timezone is very likely Philippine Standard Time (UTC+8). Fix: propose Asia/Manila (UTC+8) as an [ASSUMPTION] for PM confirmation and cite the Plan context.

- **[medium] Power/connectivity risk has no check-in fallback, and the risk is not carried into the PRD.** Source: Plan p.10 ("Power Interruption / Brownouts", High); Plan p.11 ("Typhoons/flooding", Mid). PRD location: FR-23, FR-24, NFR-6 ("Officers need network access to scan"). ADD location: §E (power interruptions mitigated only for dev work). Issue: on event day a power or network outage stops check-in with no degraded mode, and no accepted-risk record exists. Fix: record as an accepted risk in PRD §10, or specify a fallback (e.g., printed roster for manual check-in, decided by PM).

- **[low] Mode of access is supported by the revision remark but not cited.** Source: Plan p.6 text ("Internet Intranet Both"; checkbox state not visible in extraction); Plan p.2 rev 1.04 remark ("intranet"). PRD location: NFR-6 ("accessed over the intranet"). Fix: cite Plan rev 1.04 and confirm the checkbox.

## 2. Project description and objectives

- **[high] The Plan's "basic" description contradicts the PRD's scope claim.** Source: Plan p.4 (Project description: "A basic web-based event registration and attendance system..."). PRD location: §1 ("The scope is deliberately reduced so a five-person student team can deliver..."); §7.1 (26 IDs, 36 FRs, 20 screens, activity log, pagination, initial admin). Issue: the PRD never reconciles "basic" with its MVP size or says which description governs. Fix: rewrite the §1 sentence to say the scope is SPEC v2.1-defined and reduced only from SPEC (e.g., no exports, no email), and add a line in §9 recording that the Plan description is a summary.

- **[medium] Delivery methodology and iteration structure are missing.** Source: Plan p.4 (Project Type: Ground Up; Agile/Iterative; checkbox state not verified); Plan p.10 ("sprint planning", "sprint delivery targets"). PRD location: none; ADD has no delivery section. Fix: add "Ground Up, Agile/iterative" plus iteration cadence and demo points in ADD §C or PRD §7.3.

- **[medium] PRD length may exceed the recorded 5–8 page target.** Source: user-recorded stakes (5–8 pages); PRD is ~500 lines of dense bullets. PRD location: whole document, especially §4.1–§4.8. [INFERENCE: page count not measured; verify by word count and export.] Fix: move testable-consequence detail into ADD or a traceability table if the rendered length exceeds 8 pages; keep FR statements and consequences short.

- **[low] Objectives are consistent (verified).** Plan p.6 items (Razor front-end; ASP.NET Core and C#; Visual Studio; SQL Server; Figma; Tailwind CSS; 1920×1080; Chrome) match ADD §A and PRD NFR-6. No action needed beyond the browser and environment items above.

## 3. Team

- **[medium] Slice owners are not named.** Source: Plan p.8 (named PM, TM, TL, five developers, QA, approver). PRD location: none; ADD §C maps slices to "Member 1–5" only. Fix: map each slice to a named owner in ADD §C so ownership is traceable.

- **[medium] QA is not independent and several leads also develop.** Source: Plan p.8 (Taguiam, Jasper is both Developer and QA; Talisic is PM and Developer; Gabutan is TM and Developer; Singco is TL and Developer). PRD location: SM-1, SM-3 (peer approval); ADD §C. Issue: the same person may author and test code, and PM/TM/TL overlap development. Fix: name the QA owner for SM-1 acceptance runs and add a reviewer rule (reviewer must not be the author; if none is free, the approver or TM reviews).

- **[low] SA/DEV role is blank in the Plan.** Source: Plan p.8 image (SA / DEV row empty; extracted text places Gabutan on an "SA / DEV" line, conflicting). PRD location: ADD §A (library choices "belong to architecture"). Fix: assign the architecture owner explicitly, or confirm the Plan's extraction conflict.

- **[low] Approver and document control are missing from the PRD body.** Source: Plan p.2 (Plan approved by Santella, Tomas Emmanuel; rev 1.04 has no final approval); ADD §C (Approver: Santella). PRD location: §0 ("reviewer/approver", unnamed); no document-control block. Fix: add approver name, status and approval date to the PRD header.

- **[low] Spelling and naming variants.** Source: Plan p.8 ("Pinca, Seth"; "Singco, John Paul"); ADD §F ("Sinco" in one source). ADD location: §C ("Pinca", "Singco"). Fix: use full names consistently.

- **[low] Resource counts and engagement model are not recorded.** Source: Plan p.8 (Count Billed 1.00; Count Internal Buffer 1.00; Count Support 1.00; monthly grid blank). PRD location: none; ADD §C ("46 MD / 2.3 MM"). Fix: state "project-based estimate" and the team allocation, or note the grid is unfilled.

## 4. Timeline and delivery

- **[high] WBS baseline window cannot hold the stated effort.** Source: ADD §C and PRD §10 Q2 (WBS baseline 10/14–10/19/2026, 46 MD); Plan p.16 ("Please refer to the WBS"). PRD location: §10 Q2; ADD §C. Issue: 10/14/2026 is a Wednesday and 10/19/2026 a Monday, which gives 4 working days [computed]. Five people for 4 days gives at most ~20 person-days against 46 MD. The window also predates the Plan's rev 1.00 approval (2026/10/31, Plan p.2). Fix: PM re-baselines the WBS; PRD §10 Q2 should record the mismatch explicitly and give the proposed new window.

- **[high] No milestones or demo/submission dates in the PRD.** Source: Plan p.16 (schedule deferred to WBS); Git guidelines §1 (main = "submission-safe"); PRD SM-1 ("client demo"), SM-2 ("integrated dev build"). PRD location: §7, §8, §10. Fix: add §7.3 Delivery milestones with dates and owners: requirements and wireframe freeze, feature freeze, dev integration (Tier 3), release to main, client demo. Mark dates as TBD only where the PM has not decided.

- **[medium] Requirements and wireframe freeze is not a gate.** Source: Plan p.10 (prevention: "Finalize and lock down the detailed functional requirements and wireframes ... prior to active feature development"). PRD location: §7.3 (missing); ADD §D (Figma design tool) with no status. Fix: add an entry criterion: PRD and wireframes for the 20 HLD screens approved before EVT/REG/ATT build starts.

- **[medium] Academic calendar, holidays and typhoon slack are absent.** Source: Plan p.10 (Academic Schedule Conflicts, High); Plan p.11 (typhoons Sep–Dec, Mid); Plan p.4 (Alliance Cebu Holiday Calendar). PRD location: none; ADD §E mentions only the risk. Fix: add a schedule-risk note with slack and the rule to reallocate work during exam weeks, referencing the holiday calendar.

- **[medium] The Plan revision the PRD cites is unapproved.** Source: Plan p.2 (rev 1.04, 2026/11/03: Final review and approval blank; rev 1.00 approved 2026/10/31). PRD location: PRD header sources list; ADD §F. Fix: note that Plan rev 1.04 is pending approval and request re-approval.

## 5. Risks

- **[high] Scope-cut mitigation from the Plan is not recorded.** Source: Plan p.10 (mitigation: "Freeze the core MVP scope ... and defer non-essential features"; "Broad Feature Scope" rated High). PRD location: §7.2 note ("the cut order should be decided explicitly rather than taken by default"). ADD location: §E row "Broad scope" (mitigation reduced to "finalize requirements"; the freeze and deferral mitigation is dropped). Issue: the PRD defers the cut order to the PM and records no fallback, trigger date or owner. The certificate decision is recorded and is not re-opened here. Fix: add a ranked cut list (items other than certificates, e.g., report detail depth or log presentation), with a trigger date and the owner who decides.

- **[medium] Risk register is only in the addendum and has no owners or triggers.** Source: Plan p.10–11 (full register with prevention and mitigation). PRD location: none (PRD §10 has no risk section); ADD §E (table without owners or triggers). Fix: add a short PRD risk section that links to ADD §E and states owner and trigger for the top 4 High risks.

- **[medium] API contracts are listed as a risk mitigation but not as a gate.** Source: Plan p.10–11 ("document API contracts before coding"). PRD location: none; ADD §E (row only). Fix: add a gate: route and API contract document (including scanner routes) before EVT/REG/ATT implementation.

## 6. Quality gates and Git/QA guidelines

- **[medium] SM-3 Tier 1 is not observable on a merged PR.** Source: Git guidelines §2 (Tier 1 "Developer runs test suite locally before pushing"). PRD location: SM-3 ("every merged PR passed Tier 1 (local tests/lint)"); ADD §D Tier 1. Issue: local runs leave no evidence in the merged PR, so the 100% target cannot be audited. Fix: have CI run the same unit tests, lint and static analysis as a required check, and measure SM-3 from the GitHub check history plus branch protection; leave local runs as a developer obligation only.

- **[medium] SM-3 omits enforcement rules from the guidelines.** Source: Git guidelines §1 (no direct push to main or dev), §4 golden rules 1 and 5 (no force-push or `reset --hard` on shared branches), §3 (Conventional Commits). PRD location: SM-3; ADD §D (rules stated, not measured). Fix: add to SM-3 measurable checks: branch protection on main and dev (PR required, one approval, no force-push), commit-message check for Conventional Commits.

- **[medium] Tier 3 is misstated.** Source: Git guidelines §2 (Tier 3: "Manual system testing on integrated build" covering cross-feature compatibility and database migrations; responsibility "Whole Team / Lead"). PRD location: SM-3 ("release to main passed Tier 3 E2E"); ADD §D Tier 3 ("E2E, cross-feature and migration testing"). Issue: the PRD and ADD drop "manual" and the owner, and name no E2E tool. Fix: restate as manual system testing on the integrated dev build, owner Whole Team/Lead, with migration checks; name the E2E tool in ADD §A if automation is planned.

- **[medium] Test and lint tooling is not named.** Source: Git guidelines §2 (Tier 1 "static code analysis, linting"; Tier 2 "CI build and tests"). PRD location: none; ADD §A (only "GitHub Actions CI"). Fix: add test framework, linter/analyzer and CI pipeline definition to ADD §A, marked "not chosen" where open, with an owner.

- **[medium] Required QA and release artifacts are not listed.** Source: Plan p.14 (templates: Meeting Minutes, Review Minutes, HLD, LLD, Document Review Checklist, Unit/Integration/System Test Cases and Results, Test Release Checklist, Client Release Checklist; file-naming column blank). PRD location: none; ADD (no deliverables table). Fix: add an ADD deliverables table: artifact, owner, due milestone, file-naming convention. Note whether LLD is in scope.

- **[medium] Client demo has no release checklist.** Source: Plan p.14 ("Client Release Checklist"); PRD SM-1 ("every SPEC §20 scenario passes in the client demo"). PRD location: SM-1. Fix: define demo environment, data set and operator, and link them to the Client Release Checklist (ties to environment item above).

- **[medium] Bug triage process is missing from the gates.** Source: Plan p.10 (mitigation: "dedicated triage log"); Plan p.13 (Bug Tracking: Jira). PRD location: SM-3/SM-4; ADD §A (Jira for tracking only). Fix: add a triage rule (severity, owner, SLA before merge to dev) to ADD §D.

- **[low] Release tagging and release approver are missing.** Source: Git guidelines §1 diagram (release tag on main, e.g., "Release Tag v1.0"). PRD location: SM-3 (release to main); ADD §D. Fix: add a release tag per milestone and name the release approver (Approver: Santella, per ADD §C).

- **[low] Branch naming omits bugfix/*.** Source: Git guidelines §1 (`fix/*` or `bugfix/*`). ADD location: §D Branching ("feature/* and fix/* branches"). Fix: add bugfix/* or state the convention once.

- **[low] Reviewer rule for small teams is undefined.** Source: Git guidelines §4 golden rule 1 (at least one review approval); Plan p.8 (five people, overlapping roles). PRD location: SM-3 (target 100% peer approval). Fix: define who can review when no other developer is free (e.g., TM or approver), so the 100% target stays achievable.

## 7. Other checks

- **[low] Assumption not tagged.** ADD location: §A ("one application server assumed"). Fix: tag as [ASSUMPTION] and index in PRD §11.

- **[low] Functional ID traceability is consistent (verified).** All 26 functional IDs map to FRs (checked: AUTH-01–03, MD-01–05, EVT-01–04, REG-01–05, QR-01, ATT-01–03, CERT-01–02, REP-01, ADM-01–02). No action needed.

## 8. Summary

- Verdict: PRD and ADD agree with the Plan on stack, team and risk lists, but they misstate the browser decision (NFR-6 "Chrome-only" is not in the Plan), omit the phone/HTTPS and environment constraints behind check-in, and leave the timeline and SM-3 gates unmeasurable.
- Top items to fix first: (1) phone check-in needs HTTPS or LAN host decision (high); (2) no staging or demo environment for SM-1 redeploy acceptance (high); (3) WBS 10/14–10/19 window vs 46 MD (high); (4) no milestones or demo date (high); (5) unrecorded scope-cut mitigation (high); (6) SM-3 Tier 1 and enforcement rules unmeasurable (medium).
