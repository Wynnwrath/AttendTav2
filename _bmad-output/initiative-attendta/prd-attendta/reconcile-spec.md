# Reconciliation: SPEC v2.1 vs PRD and Addendum

> **Historical snapshot (2026-11-04).** This review predates the latest owner-confirmed decisions and source-document alignment. Treat its findings as historical; the current `prd-attendta.md`, `addendum.md`, and current `docsss/` sources govern.

**Inputs read in full:** `docsss/AttendTa-System-Specification.md` (SPEC, §1–§20), `prd-attendta.md` (PRD, §0–§11, FR-1…FR-36), `addendum.md`.
**Not opened:** the source PDFs and the missing `AttendTa-User-Stories.md`. Findings are against the SPEC text as supplied.
**Honoured as recorded decisions (not reported as gaps):** email-or-student-number login override; certificates fully in MVP; no institutional email domain; acceptance-based success metrics; user-stories file missing (already PRD §10 Q3).

**Verdict:** Not ready to finalize. One High conflict (membership inactivity), one High acceptance-coverage gap (SM-1), and several Medium items. No Critical items.

Format per gap: SPEC cite → PRD location → severity → suggested fix.

---

## 1. Authority, scope, deferred list (SPEC §1)

- **SPEC §1 / §1.1 — "added design details must not be represented as explicit UC requirements"; inconsistencies must be resolved explicitly.**
  PRD: partial (§0 says SPEC wins; §9 logs source conflicts). No rule separating UC requirements from design detail.
  Severity: **Low**.
  Fix: add one sentence to §0 or §9 stating that addendum design detail is not a UC requirement.

- **SPEC §1.2 / §13 — deferred items "image galleries/cropping/editors" and "completion before scheduled start and exceptional lifecycle recovery" are out of this release.**
  PRD: **missing** from §6 and §7.2 (FR-8 and FR-13 do not exclude them).
  Severity: **Low**.
  Fix: add both to §6 Non-Goals and/or §7.2.

- **SPEC §1.2 — deferred items are "this release" items; SPEC uses no version labels.** PRD §7.2 says "Deferred to v2" and "declines are final in v1". The "cancel and recreate instead" guidance for operational edits (§7.2) is not in SPEC.
  PRD: §7.2 (invented version labels; unrecorded workaround guidance).
  Severity: **Low**.
  Fix: replace "v1/v2" with "this release"; delete "cancel and recreate instead" or record it as a decision in §9.

## 2. Actors and permissions (SPEC §2)

- **SPEC §2 — "Management privileges never bypass participation eligibility or capacity."** Also: "Officers are members promoted by an admin using the same account and linked profile; Administrators need a member profile only to participate."
  PRD: **missing** as a rule. Glossary "Management" and FR-31 state neither point.
  Severity: **Medium**.
  Fix: add the "never bypass" rule to the Management glossary entry and to FR-31 consequences; state that officer/admin participation requires a linked active Member Profile.

- **SPEC §2, §9.2, §9.3, §14.3 — ordinary role permissions apply to own participation (review and promotion) and own certificates (approval and issuance), with the same validation, queue priority and audit.**
  PRD: FR-27 covers own certificate *review* only, tagged `[ASSUMPTION]`. FR-16 (review), FR-17 (promotion) and FR-28 (issuance) are silent. §11 index lists only FR-27.
  Severity: **Medium**.
  Fix: add an explicit own-participation consequence to FR-16, FR-17 and FR-28. Tag each with `[ASSUMPTION: per SPEC §2]` and add entries to §11.

## 3. Account request and approval (SPEC §3)

- **SPEC §3.2 — approval saves `created_user_id`; rejection records reviewer and time; submitted information is preserved as historical snapshots.**
  PRD: FR-2 says approval "records the reviewer and time" but does not say rejection records reviewer and time. Snapshot retention and the request-to-user link are not stated.
  Severity: **Low**.
  Fix: add to FR-2: rejection records reviewer and time; request submission snapshots are retained; approval links the resulting Account.

## 4. Authentication and profile management (SPEC §4)

- **SPEC §4 — "Replace placeholder login and reversible credential storage before deployment."**
  PRD: **missing** as a deployment gate. NFR-1 and FR-35 require hashing but do not state the replacement gate.
  Severity: **Low**.
  Fix: add to NFR-1 or §10: no placeholder login or reversible storage may ship.

- **SPEC §4 / §6 — current full name belongs to `users`; student and academic data belong to `members`.**
  PRD: FR-5 lists "full name" as a Member Profile edit. Glossary "Member Profile" omits name; glossary "Account" includes name.
  Severity: **Low**.
  Fix: in FR-5, state that full name is edited on the Account and that the profile fields are the member fields only.

## 5. Deactivation and membership inactivity (SPEC §5)

- **SPEC §5 — "Before applying either change [account or membership inactivity], show its effect on upcoming registrations, require a reason and audit affected actions." For events with `now < starts_at`, cancel pending/waitlisted and confirmed-without-valid-attendance registrations, invalidate tokens, release seats; no promotion.**
  PRD: FR-5 says setting membership inactive "blocks new participation" and does not say it changes existing registrations. That implies existing registrations are kept, which contradicts SPEC §5. FR-32's cancellation wording covers only "deactivation", so membership inactivity is not clearly covered.
  Severity: **High**.
  Fix: in FR-5 and FR-32, state that membership inactivity runs the same preview, reason, cancellation, token invalidation, seat release and no-promotion rules as account deactivation for not-yet-started events.

- **SPEC §5 — reactivation restores only access/membership state, not cancelled registrations, seats or attendance.**
  PRD: FR-32 covers account reactivation. Membership reactivation is not separately stated.
  Severity: **Low**.
  Fix: state in FR-5 or FR-32 that membership reactivation has the same restriction.

## 6. Master data and required images (SPEC §6, §6.1)

- **SPEC §6.1 — "Draft event cover: Management may remove while drafting."** Published covers and venue photos are replacement-only.
  PRD: FR-9 says the cover is optional but gives no removal rule. FR-8 says only that a venue photo "can be replaced but not removed". Draft removal is not stated.
  Severity: **Low**.
  Fix: add to FR-9: a draft cover may be removed; add to FR-8: published covers and venue photos are replace-only.

- **SPEC §6 — room names need not be globally unique.**
  PRD: **missing** (no uniqueness statement for venue room names).
  Severity: **Low**.
  Fix: add to FR-7 that room names are not unique.

## 7. Event lifecycle and signup (SPEC §7)

- **SPEC §7.1 — "No scheduled end or automatic completion is required."**
  PRD: FR-9 and FR-13 do not state there is no scheduled end time.
  Severity: **Low**.
  Fix: add to FR-9: no scheduled end; completion is explicit only.

- **SPEC §7.3 — deadline closure "does not cancel existing requests or open check-in."**
  PRD: FR-15 covers existing registrations but not check-in.
  Severity: **Low**.
  Fix: add "and does not change Check-in state" to FR-15.

## 8. Event edits (SPEC §8)

- No gap. FR-9 (draft: all fields) and FR-11 (published: title, description, cover replacement only; schedule, type, venue, limit and deadline locked; completed and cancelled read-only) match.

## 9. Registration lifecycle (SPEC §9)

- **SPEC §9.2 — "Record reviewer/time and notes" for each review outcome.** Also `reviewed_at` is the waitlist ordering key (SPEC §9.3, FR-17).
  PRD: FR-16 says only "Confirmation records the actor and time". Decline and waitlist review records are not specified. Review notes are not mentioned. FR-17 orders by `reviewed_at`, but the PRD never states when it is set.
  Severity: **Medium**.
  Fix: in FR-16, state that every review outcome (confirm, waitlist, decline) records reviewer, `reviewed_at` and optional notes.

- **SPEC §9.4 / §10 member-cancellation row — owner may cancel "even after signup deadline".**
  PRD: FR-18 is silent on the deadline. Its guards are published, before start and no valid attendance.
  Severity: **Low**.
  Fix: add "regardless of Signup Deadline" to FR-18.

- **SPEC §9.5 — at the cutoff, "disable processing/check-in".** Conflict with §10 and §11; see §10 below.

## 10. Cutoff matrix check (SPEC §10) and check-in conflict

- **SPEC §10 — certificate review/issuance guard: "Completed, valid attendance, required approvals and permissions."**
  PRD: FR-27 (review decision) has no explicit Completed or valid-attendance guard. FR-13 creates reviews and FR-28 re-checks at issuance, but the review decision itself is not guarded.
  Severity: **Low**.
  Fix: add to FR-27: a review decision requires a Completed Event and a Valid Attendance record.

- **SPEC §10 — check-in guard has no start-time bound; "Early Monday check-in does not move either cutoff."** SPEC §9.5 says the cutoff "disables processing/check-in".
  PRD: FR-23 and FR-24 have no time guard, matching §10 and §11. FR-19 does not mention check-in. The SPEC conflict (§9.5 vs §10/§11) is resolved silently.
  Severity: **Medium**.
  Fix: state the decision explicitly in FR-19 and FR-23 (for example, "Check-in is not gated by the Event Start Cutoff; it is opened and closed manually") and record it in §9 as a source-conflict decision.

- **SPEC §10 — "checked-in participants cannot cancel."**
  PRD: FR-18 "no Valid Attendance" covers this. No gap.

## 11. Attendance control and correction (SPEC §11)

- **SPEC §11 — "Store method, authenticated recorder and UTC time with audit."**
  PRD: FR-33 lists "Check-in toggles" and "attendance invalidations" but not attendance recording itself (QR or manual).
  Severity: **Medium**.
  Fix: add "attendance recorded (method, recorder)" to the FR-33 list.

- **SPEC §11.1 — arrival is recorded, not duration; early departure does not automatically invalidate attendance.**
  PRD: §7.2 defers duration tracking. It does not say that early departure leaves attendance valid.
  Severity: **Low**.
  Fix: add to FR-25 or §7.2 that early departure does not invalidate attendance.

## 12. QR credentials (SPEC §12)

- **SPEC §12 — "A shared screenshot is not proof of identity; officer verification remains necessary."**
  PRD: **missing** from the body. Only Addendum E's risk register mentions it. NFR-7 says the QR Pass "carries no personal data" but does not warn about identity.
  Severity: **Medium**.
  Fix: add to FR-21 or NFR-7: officers must verify identity at check-in; a saved QR image does not prove identity.

## 13. Cancellation and completion (SPEC §13)

- **SPEC §13 — "Preserve previous reasons for already-cancelled rows."**
  PRD: FR-12 covers the event cancellation reason and the Member view. It does not say existing per-registration cancellation reasons are preserved when the event is cancelled.
  Severity: **Low**.
  Fix: add to FR-12: already-cancelled registrations keep their original reason.

## 14. Certificates and file storage (SPEC §14)

- **SPEC §14.3 / §16 — "On failed database commit remove or reconcile an unreferenced file"; "controlled cleanup of unreferenced files is storage maintenance, not permission to delete business history or issued PDFs."**
  PRD: FR-28 says "a failed database commit leaves no orphaned file referenced". This misstates the rule, since an orphan is by definition unreferenced. The cleanup rule and its limit are not in NFR-4 or FR-28. Addendum E names orphan files only as a risk.
  Severity: **Medium**.
  Fix: rewrite FR-28 to "a failed database commit removes or reconciles the unreferenced file". Add to NFR-4: controlled cleanup of unreferenced files only; never deletes referenced or issued PDFs.

- **SPEC §6.1 / §14.5 — "Authorize uploads and serving. Images are exposed publicly only intentionally."** Event covers appear in the Member feed.
  PRD: FR-8 says images are "stored privately" but gives no serving rule. The PRD does not say who may view covers or venue photos, or that serving goes through an authorized endpoint (FR-29 covers only certificates).
  Severity: **Medium**.
  Fix: add to FR-8: cover and venue photo serving is through an authorized endpoint; state which roles and Members may view each.

- **SPEC §14.2 — review lifecycle is `pending_approval → eligible → issued` or `→ not_eligible`.**
  PRD: glossary "Certificate Review" ends at `eligible | not_eligible`. The `issued` step appears only on the Certificate (FR-28).
  Severity: **Low**.
  Fix: state in the glossary that a Certificate Review moves to issued when its Certificate is issued.

- **SPEC §14.5 — path-containment and traversal rejection; validate existence and validity of files, not the client path; capacity monitoring; single application server assumed.**
  PRD: FR-8 covers content and size validation. NFR-4 covers persistence and backup. Path containment, existence checks, capacity monitoring and the single-server assumption are not in the PRD body (the single-server assumption is in addendum A and §10 Q4).
  Severity: **Low**.
  Fix: add these points to NFR-4.

## 15. Participation reports (SPEC §15)

- **SPEC §15 — "A separate snapshot-correction editor is not promised."**
  PRD: FR-5 says profile edits never rewrite snapshots. It does not say a snapshot-correction editor is out of scope.
  Severity: **Low**.
  Fix: add to FR-30 or §7.2.

## 16. Retention without archive/restore (SPEC §16)

- **SPEC §16 — no `deleted_*` fields, deletion reasons or cascading soft deletion; reactivation does not republish events or restore attendance.**
  PRD: NFR-3 says no hard deletes and no archive/restore. It does not prohibit soft-delete fields. FR-32 covers reactivation without attendance, but not republishing events.
  Severity: **Low**.
  Fix: extend NFR-3 and FR-32 with these items.

## 17. Audit and integrity (SPEC §17)

- **SPEC §17 — "Do not log deferred workflows as implemented operations"; user actions require a real actor and system validation does not invent a user; latest-update fields do not replace history; required state-specific fields and paired actor/timestamp fields.**
  PRD: FR-33 covers actor type, entries, append-only and no secrets. The four rules above are not stated.
  Severity: **Low**.
  Fix: add the four rules as FR-33 consequences.

## 18. ERD and SQL Server conventions (SPEC §18)

- **SPEC §18.4 — "The DBML is a design artifact, not evidence that a deployed database already enforces the rules."**
  PRD: not stated. Addendum B does not say this either.
  Severity: **Low**.
  Fix: add the sentence to addendum B.

## 19. Shared list browsing (SPEC §19)

- **SPEC §19 — default sort orders: requests `requested_at/request_id` ASC; feed `starts_at/event_id` ASC; members `name/member_id`; logs `occurred_at/log_id` DESC; waitlist `reviewed_at/registration_id` ASC; "define stable defaults for remaining lists".**
  PRD: FR-36 gives a unique tie-breaker and deterministic order only. The waitlist order is in FR-17. Other default orders are missing.
  Severity: **Medium**.
  Fix: add a default-order table to FR-36.

- **SPEC §19 — other list rules: exact student-number lookup needs no paging; no page-size selector; reset on filter change; preserve query state in navigation; refresh or fall back after mutations; no raw SQL ordering.**
  PRD: FR-36 covers 25 rows, Previous/Next, page normalization, empty state, authorization before counts and revalidation. The rules above are not stated.
  Severity: **Low**.
  Fix: add the rules to FR-36 consequences.

## 20. Delivery verification and configuration (SPEC §20)

- **SPEC §20 — minimum acceptance coverage (eight bullets). SM-1 (PRD §8) states that "every SPEC §20 acceptance scenario passes", target 100%, but lists only a subset.**
  Omitted from SM-1's bullet list: publication-open signup and stale forms; mandatory venue photo and invalid or oversized files; no reapplication after cancellation; full-queue fairness and no automatic promotion on cancellation or deactivation; manual check-in fallback; early and reopened check-in; re-check-in block after invalidation; completion and eligibility; missing-file handling; safe private download; reports independent of paging, restricted counts and corrected attendance; inactive historical records; rejection and resubmission; inactive and current-role access checks; initial admin setup; backups and restores; build, review and integration testing; demonstration.
  Severity: **High**.
  Fix: replace SM-1's bullet list with the eight §20 bullets, or state that the list is the full §20 set and link to SPEC §20.

- **SPEC §20 — "All MD/MM estimates remain TBD ... state working days/month before conversion"; "no new delivery commitment is inferred from the historical October 7 deadline".**
  PRD: §10 Q2 says estimates are not agreed. The October 7 statement and the working-day basis are not stated.
  Severity: **Low**.
  Fix: add to §10 Q2: October 7 is historical, not a commitment; state the working-day basis for conversion.

- **SPEC §20 / §11 / §13 — "The reduced scope does not authorize adding public tracking, operational rescheduling or exceptional certificate repair silently."**
  PRD: SM-C1 lists exports, email and auto-promotion only. §7.2 covers reschedule and reissue. Public tracking and certificate repair are not named in the counter-metric.
  Severity: **Low**.
  Fix: extend SM-C1 to name public tracking, rescheduling and certificate repair.

## 21. Cross-slice contracts (SPEC §1.3–§1.4), addendum C

- **SPEC §1.4 — cross-slice contracts: Member 1 deactivation and Member 2 membership inactivity call Member 3 cancellation logic; Member 3 creates tokens and Member 4 renders and validates them; Member 4 correction coordinates with Member 5 eligibility; schema and service contracts are agreed before parallel work; common components are estimated once; member labels are placeholders, not equal effort.**
  PRD: addendum C gives owners only. The contracts and the estimation rule are missing.
  Severity: **Low**.
  Fix: add a "Cross-slice contracts" subsection to addendum C.

---

## §10 Cutoff matrix verification

| SPEC §10 row | Governing PRD FR(s) | Result |
|---|---|---|
| Publish: draft; cover, master and capacity validation; deadline in future | FR-9, FR-10 | Match. Revalidation of type and venue at publish is stated. |
| New signup: published; eligible; no existing registration; `now < deadline` | FR-14, FR-15 | Match. Duplicate check includes cancelled rows. |
| Pending review/confirmation: published; eligible; `now < starts_at` | FR-16 | Match. Reviews allowed after the Signup Deadline. Own-participation review not stated (see §2). Review record fields incomplete (see §9). |
| Officer-triggered promotion: published; eligible queue; free capacity; `now < starts_at` | FR-17 | Match. Own-participation promotion not stated (see §2). |
| Member cancellation: published; owner; no valid attendance; `now < starts_at` | FR-18 | Match on guards. "Even after signup deadline" not stated (see §9). |
| Check-in: published; manually open; confirmed and eligible; no attendance row | FR-22, FR-23, FR-24 | Match to §10 and §11. Conflicts with SPEC §9.5 "disable check-in" wording (see §10). |
| Completion: published; concluded; `now >= starts_at` | FR-13 | Match. "Concluded" is an officer judgment (`[ASSUMPTION]`). |
| Certificate review/issuance: completed; valid attendance; approvals and permissions | FR-27, FR-28, FR-13 | Issuance match. Review decision guard not explicit (see §10). |

Also in §10 prose: early check-in does not move cutoffs (not stated in PRD; low, implicit). A Friday deadline and Monday start allow reviews between them (implicit in FR-16 and FR-17).

---

## Verified as consistent (no gap)

- SPEC §1.1 refinements: email/password login (with override), fixed roles, four event states, publish opens signup, manual promotion, independent check-in, persistent storage, no profile pictures, mandatory venue photo, cover before publication (FR-3, FR-8–FR-11, FR-17, FR-33, addendum A).
- SPEC §3.1 and §3.3: pending-only duplicate rejection, acknowledgment only, no tracking token (FR-1, FR-2, UJ-1).
- SPEC §5: account inactivity blocks access; membership inactivity does not change account access; issued certificates preserved (FR-3, FR-5, FR-29, FR-32).
- SPEC §9.1–§9.5 registration states, approve or decline, waitlist ordering, no auto-promotion, no reapplication, cutoff rows shown as "Not confirmed before cutoff" (FR-14, FR-16–FR-19).
- SPEC §11 manual and QR check-in, duplicate handling, correction and no re-check-in (FR-23–FR-25).
- SPEC §12 scan-result table: all six outcomes present in FR-23 and §12.
- SPEC §14.1 eligibility as all three conditions; §14.4 authorized streaming; §14.3 concurrency rules (FR-27, FR-28).
- SPEC §15 attendance rate, N/A on zero denominator, no-show rules, full-dataset aggregates (FR-30).
- SPEC §17 audit list matches FR-33 apart from the check-in recording gap (§11 above).
- SPEC §19 pagination size, Previous/Next, normalization, authorization before counts (FR-36).
- SPEC §1.2 deferred scope: all items present in PRD §6 and §7.2 except those listed in §1 above.
