# Reconciliation: UC function list and Sequence Diagrams vs PRD + Addendum

> **Historical snapshot (2026-11-04).** This review predates the latest owner-confirmed decisions and source-document alignment. Treat its findings as historical; the current `prd-attendta.md`, `addendum.md`, and current `docsss/` sources govern.

Sources checked: `docsss/UC-Project-FL.pdf` (UC function list, 11 rows), `docsss/AttendTa-Sequence-Diagrams.md` (SEQ, 10 flows). Checked against `prd-attendta.md` (FR-1–FR-36, §3–§11) and `addendum.md`. Source files read-only.

## Verdict

- **UC:** 7 of 11 rows are fully covered or recorded as narrowings (§9 / §7.2). Four rows have gaps: row 10 (no-show list) is medium; rows 4, 5 and 11 are low. The Member-creation narrowing (row 4) is recorded only in §4.1, not in §7.2 or §9.
- **SEQ:** Most flows are covered. Two high-severity gaps: check-in insert is missing from the audit list (FR-33), and the promotion selection rule conflicts with SEQ (FR-17). Seven medium gaps cover atomicity, file/DB ordering and eligibility definitions. Sixteen low gaps are wording items.

## Part A — UC function list coverage

| # | UC function (Module) | PRD location | Status |
|---|---|---|---|
| 1 | User Login (email or student number) | FR-3; §9 "Login identifier" override | Covered, recorded override |
| 2 | Role-Based Access | FR-3, FR-31, §3 Role/Management | Covered |
| 3 | Account Registration Approval | FR-1, FR-2 | Covered |
| 4 | Organization Member Management | FR-4, FR-5 | Covered except A1 |
| 5 | Event Type Management | FR-6 | Covered except A3 |
| 6 | Event Creation and Posting | FR-9, FR-10 | Covered |
| 7 | Online Event Registration (capacity, deadline, status) | FR-14–FR-16; §9 "Full-event signup" | Covered, recorded narrowing |
| 8 | Attendance Check-In (search, QR, student number) | FR-23, FR-24 | Covered |
| 9 | Certificate Eligibility Tracking | FR-13, FR-27; §9 "Certificate eligibility" | Covered, recorded (all three required) |
| 10 | Event Participation Report | FR-30; §9 "Report distribution"; §7.2 | Covered except A2 |
| 11 | Officer and Permission Management | FR-31, FR-32, FR-34; §7.2 "Custom permission levels"; §9 "Permission levels" | Covered except A4 |

### A1 — Member-creation narrowing is recorded only in §4.1, not §7.2 or §9
- **Source:** UC row 4 "Organization Member Management: Maintains member records…"; UC row 3 "approve valid organization members."
- **PRD location:** §4.1 description ("there is no separate 'Add Member' path"). Not in §7.2 or §9.
- **Severity:** low.
- **Suggested fix:** Add a §7.2 row: "Manual Member creation (Add Member). Excluded; Member Profiles are created only by approving an Account Request (FR-2)." Also state that UC "contact details" means mobile number plus login email, with email read-only (FR-5).

### A2 — "No-show list" is not specified as a list
- **Source:** UC row 10: "Shows participants per event, attendance rate, **no-show list**, and course/year distribution."
- **PRD location:** FR-30 lists "No-shows" as a figure. It does not say the individuals are shown. FR-36 mentions "report detail rows" without defining them.
- **Severity:** medium.
- **Suggested fix:** In FR-30, add: "A list of No-show Registrations (full name, student number), shown only for Completed Events and paginated per FR-36." Keep the count as a separate figure.

### A3 — Event Type seed examples
- **Source:** UC row 5: "Creates standard event classifications such as seminar, outreach, meeting, competition, general assembly, or training."
- **PRD location:** FR-6. No seed list and no statement about initial values.
- **Severity:** low.
- **Suggested fix:** Either list the seeded types in FR-6 or add a §9 note that the UC examples are illustrative and not seeded.

### A4 — Per-user activity review is not explicit
- **Source:** UC row 11: "review user activity logs."
- **PRD location:** FR-34 (read-only, newest-first, consequential actions only). §7.2 excludes "advanced log filters" but does not exclude a per-user view explicitly.
- **Severity:** low.
- **Suggested fix:** Add to §7.2: "Per-user activity view. Excluded; the Activity Log is a single newest-first list (FR-34)." Or add a per-actor filter if the UC means it.

Note (no gap): Event Type description is recorded in §9 ("Event Type description", FR-6). Online-registration capacity narrowing is recorded in §9. Certificate all-three rule is recorded in §9. Login override is recorded in §9. Report distribution narrowing is recorded in §9 and §7.2. Permission-level narrowing is recorded in §7.2 and §9.

## Part B — Sequence flow reconciliation

### Flow 1: Browse authorized lists (US-29)

#### B1.1 — Page reset on filter change is not stated
- **Source:** SEQ "Browse authorized lists": "Reset on changed filters and handle empty/out-of-range pages."
- **PRD location:** FR-36 covers empty, out-of-range and invalid pages, filter-before-count and ID tie-breaker. It does not cover reset on filter change.
- **Severity:** low.
- **Suggested fix:** Add to FR-36 consequences: "Changing any filter or search term returns the list to page 1."

Covered: FR-36 (authorization before count, deterministic ordering, counts exclude unauthorized rows); FR-30 (report totals over full dataset); FR-17 (waitlist selection across whole queue).

### Flow 2: Account request and approval

Covered: FR-1 (duplicate checks, hash only, acknowledgment only), FR-2 (atomic approval, hash cleared, one outcome under concurrency, rejection with reason creates no Account), FR-3 (hash validation, signed cookie). No gap.

### Flow 3: Upload or replace event/venue image

#### B3.1 — No orphan-file cleanup and no old-file removal ordering for images
- **Source:** SEQ "Upload or replace event/venue image": "Commit image_path and audit → Remove old file after commit"; "On failed SQL save clean up/reconcile the new file and retain old path/file."
- **PRD location:** FR-8 says only that a failed file or DB write leaves the previous image intact. The cleanup rule exists for certificates (FR-28) only. Addendum §E records the orphan-file risk, but no FR states the rule for images.
- **Severity:** medium.
- **Suggested fix:** Add to FR-8: "Write the new file, commit the new relative path and audit in one DB transaction, then delete the old file. If the commit fails, delete the new file. If deletion fails, the old image stays referenced and the failure is logged without a physical path." Mirror the wording in FR-28.

#### B3.2 — Draft cover removal rule missing
- **Source:** SEQ: "Required images cannot be removed without replacement." Addendum §B.3: published Events require a cover.
- **PRD location:** FR-7 (Venue photo cannot be removed). FR-9 ("optional Cover") and FR-11 (replacement only, after publish) do not state whether a draft Cover can be removed.
- **Severity:** low.
- **Suggested fix:** Add to FR-9: "A draft Cover may be replaced but not removed once set" (or state that removal is allowed on drafts only).

#### B3.3 — Lifecycle restriction not cross-referenced for images
- **Source:** SEQ: "Check management role and event lifecycle."
- **PRD location:** FR-8 does not reference the FR-11 lifecycle locks. FR-11 locks Completed and Cancelled Events, which covers this, but the reader must infer it.
- **Severity:** low.
- **Suggested fix:** Add to FR-8: "Cover replacement follows the lifecycle locks in FR-11."

#### B3.4 — "No physical paths to clients" is not an FR rule for images
- **Source:** SEQ: "Persistent local root outside web/deployment directory; no physical paths sent to clients."
- **PRD location:** NFR-4 covers the root location. FR-29 has the rule for certificates. FR-8 does not state it.
- **Severity:** low.
- **Suggested fix:** Add to FR-8: "Responses and pages never expose physical file paths."

### Flow 4: Publish, register and approve

#### B4.1 — Officer self-registration and self-approval are not stated for registration
- **Source:** SEQ: "Ordinary role permissions also apply to own participation; audit all decisions."
- **PRD location:** FR-14 requires a Member with an active Account and membership. FR-16 has no self-approval rule. FR-27 states the self-approval assumption for certificates only.
- **Severity:** low.
- **Suggested fix:** Add to FR-14 and FR-16: whether an Officer holding a Member Profile may register, and whether they may approve their own Registration. Tag it `[ASSUMPTION]` and index it in §11, matching FR-27.

#### B4.2 — "No arbitrary status editing" is not stated
- **Source:** SEQ: "No arbitrary status editing."
- **PRD location:** Status transitions are defined in FR-12, FR-13, FR-16–FR-19. No FR forbids other edits.
- **Severity:** low.
- **Suggested fix:** Add to §4.4 or NFR-2: "No Management action may set a Registration status except the transitions in FR-16–FR-19."

Covered: FR-10 (validation, cover, audit), FR-14–FR-15 (snapshots, deadline), FR-16 (atomic seat check, token), FR-11 (fields locked after publish).

### Flow 5: Cancel a registration

#### B5.1 — Cancellation after Signup Deadline is implicit
- **Source:** SEQ: "Allowed while event published and before start … including after signup deadline."
- **PRD location:** FR-18 gives the window as Published and before the Event Start Cutoff. It does not say "including after the Signup Deadline."
- **Severity:** low.
- **Suggested fix:** Add to FR-18: "Permitted regardless of the Signup Deadline."

#### B5.2 — Who supplies the cancellation reason is unclear
- **Source:** SEQ shows only "Confirm final cancellation." UC does not address it.
- **PRD location:** FR-18: "Cancellation atomically records the reason, actor and time." The Member's input is not specified.
- **Severity:** low.
- **Suggested fix:** Either state that an optional Member reason is stored, or remove "reason" from the Member-cancel path.

### Flow 6: Officer-triggered waitlist promotion

#### B6.1 — Selection rule conflicts with SEQ (skip ineligible vs block)
- **Source:** SEQ "Officer-triggered waitlist promotion": "Select earliest **eligible** reviewed waitlisted request across the entire queue"; "Select reviewed_at then ID."
- **PRD location:** FR-17: "The Waitlisted Registration chosen is the earliest by reviewed_at, then registration_id … It must still be eligible and a Seat must be free."
- **Conflict:** FR-17 reads as "pick the earliest, then fail if it is ineligible." SEQ reads as "pick the earliest eligible one, skipping ineligible ones." The UJ-2 demo and SM-1 depend on this.
- **Severity:** high.
- **Suggested fix:** Align FR-17 with SEQ: select the earliest by (reviewed_at, registration_id) among eligible Waitlisted Registrations. Ineligible ones are skipped and left unchanged. No eligible candidate means no change. If the PM wants the opposite, record it in §9.

#### B6.2 — "Eligible" is undefined for promotion and review
- **Source:** SEQ flow 8: "Validate … active account/member and confirmed registration"; SEQ flow 6: "eligible waitlisted request."
- **PRD location:** FR-16 and FR-17 use "eligible" without defining it. FR-14 defines only the signup entry condition.
- **Severity:** medium.
- **Suggested fix:** Define once in §3 or FR-17: "Eligible = Event Published, before Event Start Cutoff, Account active, Member Profile active, Registration status waitlisted (or pending, for FR-16), and no Valid Attendance."

#### B6.3 — Membership-inactivation cascade is contradictory
- **Source:** SEQ flow 10: "Membership inactivity alone does not revoke historical certificates." SEQ flow 6 depends on which waitlisted rows remain eligible.
- **PRD location:** FR-5: "Setting membership to inactive blocks new participation. It does not change Account access." FR-32: "Management can change membership status … For Events not yet started, deactivation cancels the person's pending, waitlisted and confirmed Registrations." FR-32 does not say whether "deactivation" means membership or Account.
- **Severity:** medium.
- **Suggested fix:** In FR-32, separate the two cases. State whether membership inactivation cancels upcoming Registrations (cascade) or only blocks new signups (FR-5). Make the choice consistent with FR-5, FR-23 and FR-29.

### Flow 7: Reject signup at the deadline

#### B7.1 — Promotion after the Signup Deadline is not explicit
- **Source:** SEQ: "Existing reviews/promotions/cancellations may continue until start."
- **PRD location:** FR-15 says existing Registrations are unaffected. FR-17 requires only Published and before cutoff. Promotion after the deadline is implied, not stated.
- **Severity:** low.
- **Suggested fix:** Add to FR-17: "Promotion is permitted after the Signup Deadline and until the Event Start Cutoff."

#### B7.2 — Rejection message is unspecified
- **Source:** SEQ: "Reject: registration closed."
- **PRD location:** FR-15 says the server rejects. The user-facing message is not specified.
- **Severity:** low.
- **Suggested fix:** Add to FR-15: "The response says registration is closed and reveals no other state."

Covered: FR-15 (stale forms, server-side), FR-19 (cutoff, non-actionable rows, excluded from no-shows), NFR-5 (no scheduled rewrite), FR-22 (check-in has its own gate).

### Flow 8: Saved QR and attendance check-in

#### B8.1 — Check-in insert is not in the Activity Log list (HIGH)
- **Source:** SEQ "Saved QR and attendance check-in": "Insert attendance + audit"; SEQ preamble: "SQL mutations include audit entries."
- **PRD location:** FR-33 lists "Check-in toggles" and "attendance invalidations." It does not list creation of an Attendance Record through QR scan (FR-23) or manual check-in (FR-24).
- **Severity:** high.
- **Suggested fix:** Add to FR-33: "Attendance Record creation (method, recorder, registration) for QR and manual check-in." FR-23's "Record attendance" row should cite it.

#### B8.2 — Manual check-in does not restate the transactional path
- **Source:** SEQ: "Manual search requires explicit Check in with same service"; "Atomic eligibility + duplicate check."
- **PRD location:** FR-23 covers the unique constraint and transaction logic. FR-24 says "The same validations as FR-23 apply" but not the same transactional path.
- **Severity:** low.
- **Suggested fix:** Add to FR-24: "Uses the same transactional service and rules as FR-23."

#### B8.3 — Screenshot is not identity proof
- **Source:** Addendum §E: "A screenshot of a QR Pass does not prove identity, so officer verification is still needed." SEQ: "Member may use screenshot offline."
- **PRD location:** NFR-6 allows an offline saved QR. NFR-7 says the QR carries no personal data. The verification rule is not stated.
- **Severity:** low.
- **Suggested fix:** Add to FR-23: "A presented pass does not prove identity; the officer checks the displayed name and student number before recording attendance."

Covered: FR-21 (ownership, on-demand PNG, cleared on cancel), FR-22 (audited toggles, closes check-in on complete/cancel), FR-23 (outcome table, unknown token, wrong event), FR-25 (re-check-in rejected, certificate block, serialized), FR-26.

### Flow 9: Complete, approve and issue PDF

#### B9.1 — Approval does not recheck Valid Attendance transactionally
- **Source:** SEQ "Complete, approve and issue PDF": "Approve certificate eligibility → Validate attendance; save reviewer + audit."
- **PRD location:** FR-27 says "same validation and audit" but states no recheck and no serialization. FR-25 makes invalidation set unissued Certificate Reviews to `not_eligible`, and serializes only against issuance. An approval racing with an invalidation could mark an invalid attendee `eligible`.
- **Severity:** medium.
- **Suggested fix:** Add to FR-27: "Approval rechecks Completed status and Valid Attendance in the same transaction used by FR-25, and is serialized with invalidation. Approval for a Registration without Valid Attendance is rejected."

#### B9.2 — Completion atomicity is not stated
- **Source:** SEQ: "Close check-in; complete; audit" and "Create pending valid-attendee reviews."
- **PRD location:** FR-13 lists the effects but not their atomicity. FR-12 says "atomically" for cancellation; FR-13 does not.
- **Severity:** medium.
- **Suggested fix:** Add to FR-13: "Status change, Check-in closure, Certificate Review creation and the Activity Log entry commit in one transaction. Any failure leaves the Event Published."

#### B9.3 — Orphan-PDF cleanup is weaker than SEQ
- **Source:** SEQ: "Recheck; commit issued metadata + audit"; "clean up/reconcile orphan PDF after failed commit."
- **PRD location:** FR-28: "A failed database commit leaves no orphaned file referenced." This allows an unreferenced orphan file to remain. NFR-5 rules out scheduled reconciliation.
- **Severity:** medium.
- **Suggested fix:** Replace the FR-28 wording with: "On a failed commit, the newly written PDF is deleted inline. If deletion fails, the Certificate stays unissued and the failure is logged without a physical path." Apply the same rule to images (B3.1).

#### B9.4 — Approval state transitions are incomplete
- **Source:** SEQ: "Declined decisions cannot reopen." Glossary: `pending_approval → eligible | not_eligible`.
- **PRD location:** FR-27 makes declines final. FR-25 moves unissued `eligible` reviews to `not_eligible` on invalidation. The approve-then-decline path is not stated.
- **Severity:** low.
- **Suggested fix:** Add a state note to FR-27: "Allowed transitions: pending → eligible | not_eligible; eligible → not_eligible only via FR-25; not_eligible is terminal."

Covered: FR-28 (one at a time, fixed template, re-check, failed file leaves unissued, success only after commit, duplicate issuance impossible), FR-13 (concluded = Officer judgement, assumption recorded), §6 (no bulk issuance, revocation or reissue).

### Flow 10: Authorized certificate download

Covered: FR-29 (authorized endpoint, `application/pdf`, ownership or Management, inactive Account blocks, inactive membership does not, no caller-supplied paths, no static serving, edits do not alter PDFs), FR-20 (download links in My Events), NFR-4 (storage outside web root). No gap.

## Part C — Gap count by severity

- **High (2):** B6.1, B8.1
- **Medium (7):** A2, B3.1, B6.2, B6.3, B9.1, B9.2, B9.3
- **Low (16):** A1, A3, A4, B1.1, B3.2, B3.3, B3.4, B4.1, B4.2, B5.1, B5.2, B7.1, B7.2, B8.2, B8.3, B9.4

Total: 25 gaps.

## Part D — Not reconciled here

- SPEC v2.1 was used only where the PRD cites it. `AttendTa-User-Stories.md` is still missing (PRD §10 Q3), so SEQ "US-xx" references could not be checked against acceptance criteria.
- The UC PDF project description is truncated in extraction (addendum §F). Only the function table was used.
