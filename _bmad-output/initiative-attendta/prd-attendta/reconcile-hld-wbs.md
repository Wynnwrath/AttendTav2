# Reconciliation: HLD (Group 7, rev 1.0) and WBS (rev 1.00) vs PRD and Addendum

> **Historical snapshot (2026-11-04).** This review predates the latest owner-confirmed decisions and source-document alignment. Treat its findings as historical; the current `prd-attendta.md`, `addendum.md`, and current `docsss/` sources govern.

Scope: the 26 functional IDs, the 20 HLD screens, and the screen-level behaviour in the HLD. Read-only on the PRD, addendum and sources. Recorded decisions (SPEC v2.1 wins, student-number login override, certificates in MVP, no institutional email domain, acceptance-based metrics) are not reported as gaps.

Sources: `prd-attendta.md` (§4 FR-1…FR-36, §7, §9), `addendum.md`, HLD text and page renders (all 20 screens viewed), WBS functional table. HLD page renders are low resolution. Copy marked **[low-res]** must be checked against the source PDF before it is edited into the PRD.

## Verdict

Traceable, not yet complete. Every one of the 26 IDs is tagged to at least one FR, and the FR bullets state the core WBS business rules for each ID. The PRD does not yet describe several screen-level behaviours that the HLD specifies, and one rule contradicts itself.

- Gaps: **1 High, 15 Medium, 18 Low** (section 4).
- Fix before the demo script is locked: **G1** (membership-inactivation cascade contradicts itself).
- Scope or decision items needing a PM call: **G3** (print button vs deferral), **G4** (token shown as text), **G8** (member seat visibility), **G16** (end time), **G11** (certificate issuer).

## 1. ID-by-ID

| ID | Covered by FR | Gaps |
|---|---|---|
| AUTH-01 | FR-1 (re-request after rejection: FR-2) | None material. Degree/program list source is undefined (G34). Acknowledgment screen copy not specified (G30). |
| AUTH-02 | FR-2, FR-36 | None material. Finalized-request list view and rejection-reason column not specified (G28). |
| AUTH-03 | FR-3; override in §9 | Email-or-student-number, generic failure, signed expiring cookie and per-request state checks are covered. Role landing and access matrix missing (G5). Login wireframe still says "institutional email" (G21). |
| MD-01 | FR-4 | Covered (view-only; no picture or password change). Field list and QR shortcut not enumerated (G25). |
| MD-02 | FR-5, FR-32 | Student-number correction, reason, uniqueness and read-only login email are covered. Membership-inactivation cascade contradicts itself (G1, High). Email editable in the wireframe modal (G24). |
| MD-03 | FR-6 | Covered. Badge-accent colour column in wireframe has no model field (G26). |
| MD-04 | FR-7, FR-36 | Covered. Effect of deactivating a venue on already-published events not stated (G27). |
| MD-05 | FR-8; FR-9–FR-11; "avatar" label resolved in §9 | Covered. §9 cross-reference cites FR-4 only (G33). |
| EVT-01 | FR-9, FR-10 | Draft fields, publish gates and "publish opens signup" are covered. Wireframes show an end time that the model lacks (G16). Management list filters and paging ambiguous (G19). |
| EVT-02 | FR-10, FR-14, FR-15 | Deadline open/closed, local-time label and server rejection are covered. "Registration closed" member copy and feed filters not specified (G9, G20). |
| EVT-03 | FR-9, FR-11 | Covered. Wireframe locked-fields display matches FR-11. |
| EVT-04 | FR-12, FR-13 | Cancel with reason, cascade, no promotion, block after attendance or issuance, and completion closing check-in and creating reviews are covered. Management filter lacks Cancelled (G19). Mark Completed should be disabled before start (G7). Modal copy (G30). |
| REG-01 | FR-14, FR-20 | One pending request per member and event, academic snapshot, full-event warning and no reapplication are covered. Seat-count visibility undecided (G8). Member status copy missing (G9). |
| REG-02 | FR-16 | Approve/decline, capacity and queue priority, one transaction and QR on confirmation are covered. Queue views and counts not specified (G7). "Decline requires reason" is a PRD addition (G31). |
| REG-03 | FR-18 | Confirmation, seat release, QR cleared and no promotion are covered. FR-18 requires "the reason" but the wireframe dialog has no reason field (G2). Permission after signup closes not stated explicitly (G2). |
| REG-04 | FR-17 | Queue order across the whole queue, seat recheck, no automatic promotion are covered. Promote placement and confirmation step not specified (G7). |
| REG-05 | FR-15, FR-19 | Deadline and start cutoffs, stale forms, and exclusion of unresolved rows from no-shows are covered. The wireframe calls these "history"; the PRD label is "Not confirmed before cutoff" (G9). |
| QR-01 | FR-21 | Owner-only, Confirmed-only, on-demand PNG, token-only QR and token cleared on cancel are covered. Print button (G3), token text under QR (G4), past-pass display (G32). |
| ATT-01 | FR-22, FR-23, FR-24 | Check-in toggle, scan outcome table, manual fallback and uniqueness are covered. Check-in log, camera-failure message and scanner tabs not specified (G15). Message copy (G30). |
| ATT-02 | FR-25 | Reason, retained row, exclusion, issuance block and re-check-in rejection are covered. Entry point for invalidation not specified (G15). |
| ATT-03 | FR-20, FR-26 | Own records, invalidated records marked and no self-service are covered. Check-in time and method not stated (G17). Tab structure and "View certificate" (G12). |
| CERT-01 | FR-27 | Approve/decline, reason on decline, reviewer and time, and final declines are covered. WBS requires rechecking attendance and permissions at decision time, and HLD requires issued decisions to be immutable; FR-27 has neither (G10). Queue structure (G18). |
| CERT-02 | FR-28, FR-29 | One at a time, fixed template, unique number, snapshots, failure handling and authorized streamed download are covered. Issuer identity and certificate-number format undefined (G11). In-app viewer absent (G12). Issue-confirmation copy (G30). |
| REP-01 | FR-30, FR-36 | Per-event counts, N/A rule, no-shows, attendee course/year, full-dataset totals and exclusion of unconfirmed/cancelled are covered. Organization summary, detail-row columns and session audit roster absent (G13). |
| ADM-01 | FR-31, FR-32 | Fixed roles, last-admin protection, reasons, affected-registration preview and no automatic restoration are covered. Cascade ambiguity (G1). Deactivate copy narrower than FR-32 (G22). Summary cards and Protected badge (G23). |
| ADM-02 | FR-33, FR-34 | Append-only entries, actor/action/record/time/reason, no secrets and newest-first are covered. Master-data and initial-admin changes missing from the logged-events list (G14). Basic search (G29). |

WBS pagination: every WBS "25-row pagination" statement (AUTH-02, MD-02–MD-04, EVT-01, REG-01–REG-03, ATT-01–ATT-03, CERT-01, CERT-02, REP-01, ADM-01, ADM-02) maps to a list in FR-36. The one soft spot is "events" (EVT-01), which FR-36 names only as "events feed" (G19).

## 2. Screen-level coverage (20 HLD screens)

| # | HLD screen (role) | PRD coverage | Omitted or under-specified behaviour (gap ref) |
|---|---|---|---|
| 1.0 | Login (all) | FR-3 | Role landing not defined (G5). Email-only wireframe and "institutional" copy (G21). "Request membership" link not specified. |
| 2.0 | Registration / Request membership (applicant) | FR-1 | Program dropdown source (G34). Three sections: identity, program and section, password. "Already have an account? Sign in" link. |
| 3.0 | Account requests (officer/admin) | FR-2 | Pending / Approved / Rejected tabs with counts. Rejection reason column (G28). Pending count badge on nav. |
| 4.0 | My profile (member) | FR-4 | Field list, access-role label, QR shortcut (G25). |
| 5.0 | Members, list and edit (officer/admin) | FR-5, FR-32, FR-36 | Active/Inactive filter. Email editable in modal (G24). "Deactivate membership" button and future-registration note (G1). |
| 6.0 | Event types (officer/admin) | FR-6 | Active/Inactive filter. Badge-accent column (G26). |
| 7.0 | Venues, list and edit (officer/admin) | FR-7, FR-8 | "Deactivate facility" effect on existing events (G27). Required-photo rule is covered. |
| 8.0 | Event management (officer/admin) | FR-9–FR-13, FR-36 | Filter tabs All / Published / Completed / Drafts, no Cancelled (G19). List columns (venue, schedule, registered count). "Drafts stay private" note. |
| 9.0 | Create event (officer/admin) | FR-9, FR-10 | "Save draft" and "Publish & Open Signup" actions. Event date and scheduled time fields, no end time (G16). Cover-required copy. |
| 10.0 | Event details and registrations, with edit, cancel and complete modals (officer/admin) | FR-11–FR-13, FR-16, FR-17 | Three queue tabs (Review requests, Confirmed roster, Waitlist queue) with counts. Seat counter and waitlist counter. Promote placement (G7). Modal copy (G30). |
| 11.0 | Browse events (member) | FR-14, FR-36 | Filter chips All / Live / Upcoming / Past and "Live now" banner (G20). Seat-fill bar (G8). |
| 12.0 | Member event details, with signup-closed and cancel modal (member) | FR-14, FR-18, FR-19 | Status panel states and copy (G9). Seats-filled line (G8). Cancel dialog has no reason field (G2). "Registration guidelines" text. |
| 13.0 | My passes (confirmed member) | FR-21 | "Print pass" button (G3). Token text under each QR (G4). Active and Past sections (G32). |
| 14.0 | Door scanner, with invalidate modal (officer/admin) | FR-22–FR-25 | Recorded check-in log with row actions, and the invalidation entry point (G15). Camera / manual / student-number tabs. Camera-unavailable message (G15). Modal copy (G30). |
| 15.0 | My attendance: Registrations, Attendance, Certificates tabs (member) | FR-20, FR-26, FR-29 | Tab structure not specified. Check-in details (G17). "View certificate" and "Download PDF" (G12). |
| 16.0 | Certificates, with decline and issue modals (officer/admin) | FR-27, FR-28 | Eligibility Review and Issued Registry tabs. Concluded-event filter (G18). Issued-immutable rule (G10). Modal copy (G30). |
| 17.0 | Certificate viewer (owner/management) | FR-29 (download only) | In-app view (G12). Template title, adviser signature block, certificate number (G11). |
| 18.0 | Participation reports (officer/admin) | FR-30, FR-36 | Organization summary cards. Per-event table. Per-event session audit roster. Distribution by academic program and by year level (G13). |
| 19.0 | Accounts and roles, with deactivate modal and Protected row (admin) | FR-31, FR-32 | Summary cards, role and status filter chips, Protected badge (G23). Deactivate copy (G22). Cascade (G1). A top-right orange button may read "+ New user" **[low-res, unverified]**; if present it conflicts with FR-2 (no separate add path). |
| 20.0 | Activity history, "Governance audit trail" (admin) | FR-33, FR-34 | Keyword search box (G29). Logged-events list incomplete (G14). |

## 3. Dashboard and role routing

- HLD 1.0 logic says "route by role", but no destination is given for any role. FR-3 repeats "routed by Role" without a mapping.
- The HLD sidebar for officer and admin shows a **Dashboard** (overview) entry. The PRD has no dashboard screen. See G5.
- No role-by-screen access matrix exists in the PRD or addendum. See G5 and G6.

## 4. Gap list

Severity: **High** = contradictory or ambiguous rule that changes behaviour or data. **Medium** = screen behaviour or requirement missing, or a conflict with a recorded deferral. **Low** = copy, labels or wording.

### High

- **G1 (High). Membership inactivation cascade contradicts itself.**
  - PRD: FR-5 says membership inactive "blocks new participation". FR-32 says management membership changes cancel pending, waitlisted and confirmed Registrations for future Events.
  - Sources: WBS MD-02 ("apply future-registration rules when membership becomes inactive without rewriting historical participation"). WBS ADM-01 and HLD 19.0 put future-registration cancellation under Admin-only account deactivation. HLD 5.0 "Future registration rules" note.
  - Fix: add a rule table to FR-32 with columns Action, Actor, Effect on future Registrations, Effect on access. Suggested rule: membership inactive, by Officer or Admin, blocks new signups only (FR-5). Account deactivated, by Admin, cancels pending, waitlisted and confirmed Registrations with no Valid Attendance, releases seats, no Promotion (FR-32). Align the HLD 5.0 and 19.0 copy to the same rule.

### Medium

- **G2 (Medium). Member cancellation "records the reason" but the UI has no reason field.**
  - PRD FR-18 requires the reason. HLD 12.0 cancel dialog has a finality warning and Keep / Cancel buttons only. WBS REG-03 does not require a reason. FR-18 also omits the WBS wording "even after signup closes".
  - Fix: either add an optional reason input to the dialog and say so in FR-18, or remove "reason" from FR-18 and keep actor and time. Add the explicit window: allowed after the Signup Deadline, before the Event Start Cutoff.

- **G3 (Medium). "Print pass" appears in the wireframe but printing is deferred.**
  - HLD 13.0 shows a Print pass button on each pass card. PRD §7.2 defers in-system QR printing, and FR-21 provides PNG download only.
  - Fix: remove the button from the design, or add a §9 row recording "print deferred; PNG download only" with the reason.

- **G4 (Medium). A human-readable token is printed under each QR.**
  - HLD 13.0 shows a code under each QR **[low-res, e.g. "AT-7KQH-9Q6X"]**. FR-21 says the QR encodes only the token. NFR-1 says QR tokens are never logged, and the addendum lists screenshots as a risk. A printed code is a second, typeable copy of the credential.
  - Fix: decide. Either omit the text code, or state in FR-21 that the code is the token, is treated as a credential, and is cleared on cancel. Add it to the addendum screenshot risk.

- **G5 (Medium). Role routing, landing pages and the Dashboard are not specified.**
  - HLD 1.0 says "route by role". The member sidebar is Events, My Passes, History, Profile. The officer sidebar adds Dashboard, Account Requests, Certificates, Participation Reports, Members, Event Types and Venues. The admin sidebar adds Accounts & Roles and Activity History.
  - PRD FR-3 says "routed by Role" with no destination. No access matrix exists.
  - Fix: add a role × screen matrix and a landing-page rule to the addendum, referenced from FR-3. Decide whether a Dashboard exists. If it does, define its content (FR-30 summary cards are a natural fit). If not, remove it from the wireframe.

- **G6 (Medium). No screen inventory.**
  - PRD §7.1 says features are "delivered across the 20 HLD screens", but no section maps FRs to screens. Screen-level behaviours have no owner.
  - Fix: add a screen map to the addendum (section 2 above is a starting point) and cite the relevant screens from each FR.

- **G7 (Medium). Management event-details behaviours are missing.**
  - HLD 10.0 has three queue tabs (Review requests, Confirmed roster, Waitlist queue) with counts, a confirmed-seat counter ("confirmed / participant limit") and a waitlist counter. Queue columns are student name, degree program, submission date, queue priority, status and actions. Buttons are Edit details, Mark completed, Cancel event and Events list.
  - FR-16 and FR-17 depend on queue order, but no FR says where Management sees the queue, where Promote lives, or whether it asks for confirmation. Mark completed should be disabled before start (FR-13).
  - Fix: add a Management event-view bullet set to §4.4: one view per queue with queue position, seat and waitlist counters, Promote on the Waitlist view, and action states that mirror FR-13 and FR-19.

- **G8 (Medium). Member seat visibility is undecided.**
  - HLD 11.0 cards show a seat-fill bar. HLD 12.0 shows a seats-filled line **[low-res, reads "4 / 35 seats filled"]**. FR-14 mentions only the "no seat guaranteed" warning.
  - Fix: decide. If members see counts, add an aggregate count with no names and no waitlist size to FR-14. Otherwise remove the counts from the wireframes and keep the warning.

- **G9 (Medium). The member status panel has no states, copy or actions.**
  - HLD 12.0 "Your registration status" covers Registration closed, Pending, Waitlisted, Confirmed, Cancelled and the not-confirmed-before-cutoff state, with a "Request a place" action and a "Registration guidelines" box. PRD FR-19 supplies one label. No state, copy and action table exists.
  - Fix: add a state table (state, label, available action) to FR-20, or to the addendum with G30.

- **G10 (Medium). CERT-01 requirements are missing from FR-27.**
  - WBS CERT-01 requires rechecking attendance and permissions when reviewing. HLD 16.0 says issued decisions cannot change. FR-28 rechecks only at issuance, and FR-25 blocks invalidation only after issuance.
  - Fix: add two FR-27 bullets. Decisions re-check completion, Valid Attendance and actor permission inside the transaction. A review with an issued Certificate is immutable.

- **G11 (Medium). Certificate issuer and template content are undefined.**
  - FR-28 requires an "issuer" snapshot but does not say whose name or title. HLD 17.0 sample shows an institutional header, the title "Certificate of Participation", recipient, activity, date, certificate number **[low-res, format appears to be "ATTN-YYYY-NNNN"]**, issue date, and an adviser signature block with name and title. WBS CERT-02 also says "issuer details" without definition. §10 Q1 covers artwork, not who the issuer is.
  - Fix: define the issuer snapshot as signatory name and title. Fix the template text in §10 Q1. Define the certificate-number format. Add to FR-28.

- **G12 (Medium). Certificate viewer and "View certificate" are missing.**
  - HLD 17.0 is a certificate viewer for owner and management. HLD 15.0 My Certificates tab shows "View certificate" and "Download PDF". FR-29 covers download only.
  - Fix: add inline viewing on the same authorized endpoint to FR-29, or record in §7.2 that viewing is download-only.

- **G13 (Medium). The participation report is under-specified.**
  - HLD 18.0 has organization summary cards (scheduled events, attendance totals, certificates issued, overall attendance %). Below them are a per-event table (confirmed, attended, no-shows, attendance %, action), a per-event session audit roster (student name, number, academic program, status, attendance time), and distributions by academic program and by year level.
  - FR-30 defines per-event metrics only. FR-36 paginates "report detail rows" that FR-30 never defines. WBS REP-01 says "show participants/statuses", which implies a roster.
  - Fix: add the summary KPIs to FR-30 with definitions and the N/A rule. Define detail-row columns. State whether distributions are tables or charts, since they are totals and not paged.

- **G14 (Medium). Activity-log coverage is incomplete.**
  - FR-33 does not list Event Type create, edit or activation changes; Venue create, edit or deactivation; initial-admin creation (FR-35); or Event draft creation. §1 says "every consequential action is attributable and retained". HLD 6.0 and 7.0 and WBS ADM-02 ("relevant stored changes") point the same way.
  - Fix: add bullets to FR-33 for master-data changes and initial-admin setup. Decide whether draft creation is consequential enough to log.

- **G15 (Medium). Door-scanner behaviours are missing.**
  - HLD 14.0 has an event selector, a check-in open/closed indicator, Camera / Manual / Student-number tabs, a camera-unavailable message **[low-res, directs officer to student-number lookup]**, and a paged "Recorded check-in log" with per-row actions and an empty state **[low-res, "No check-ins recorded yet"]**. The invalidate modal is opened from this area.
  - FR-22 to FR-25 mention none of the log, the camera fallback or the invalidation entry point.
  - Fix: add to FR-24 that the log lists Valid Attendance rows for the selected event (paged per FR-36, showing name, student number, time, method and recorder). Invalidation is started from the log. A camera failure falls back to manual lookup with a message.

- **G16 (Medium). Event end time: wireframes show a range, the model has a start only.**
  - HLD 10.0, 11.0 and 13.0 show time ranges (for example 09:00–11:00, and 09:00–12:00 in the edit modal). FR-9 and FR-13 use start time only.
  - Fix: either state that events have a start time only and show no end, or add a display-only end time to FR-9 and the schema. An end time must not drive any cutoff; the Event Start Cutoff stays the start.

### Low

- **G17 (Low). Check-in details missing from FR-26.** WBS ATT-03 says "attendance and check-in details". FR-26 lists records and invalidation only. Fix: add check-in time and method.
- **G18 (Low). Certificate queue structure.** HLD 16.0 has Eligibility Review and Issued Registry tabs with counts, a concluded-event filter, and row actions Approve, Decline and Issue. Fix: name both queues and the filter in FR-27 or FR-36.
- **G19 (Low). Management event-list filters and "events feed" ambiguity.** HLD 8.0 uses All / Published / Completed / Drafts, with no Cancelled filter. FR-36 "events feed" could mean the member feed or the management list. Fix: define the management filters (add Cancelled) and state that both lists are paged.
- **G20 (Low). Member browse filters and banner undefined.** HLD 11.0 uses chips All / Live / Upcoming / Past and a "Live now" banner. No FR defines these, the default sort, or the tie-breaker. Fix: define filter values, default sort (start time ascending) and tie-breaker (FR-36 requires deterministic ordering).
- **G21 (Low). Login copy contradicts the override and the domain decision.** HLD 1.0 labels the field "Institutional Email Address" with helper text "verified university email". AUTH-03 logic says "institutional email/password for every role". FR-3 accepts email or student number, and §9 drops the domain restriction. Fix: relabel the field "Email or student number" and remove "institutional" and "university" wording. The override is recorded; the wireframe has not caught up.
- **G22 (Low). Deactivate copy narrower than FR-32.** HLD 19.0 deactivate modal says it "cancels pending registrations". FR-32 cancels pending, waitlisted and confirmed Registrations with no Valid Attendance. Fix: align the copy and show the FR-32 preview list (already required).
- **G23 (Low). Accounts list details.** HLD 19.0 shows summary cards (Total users, Administrators, Officers, Deactivated), role and status filter chips, a Protected badge on the last active administrator, and an "Assigned roles" column with chips. FR-31 uses a single fixed Role. Fix: add the Protected indicator to FR-31. Confirm the single-role model and change the chips to one value. Check the top-right button (see screen 19.0 in section 2).
- **G24 (Low). Edit modal shows email as editable.** HLD 5.0 edit modal has an "Email Address" field. FR-5 says login email is read-only for everyone. Fix: render read-only in the wireframe.
- **G25 (Low). My Profile fields not enumerated.** HLD 4.0 shows student number, name, email, program, year and section, membership category, access role ("Student Member"), contact mobile, "Date enrolled", a status badge, a "last verified" footer, and a "View my event QR pass" button. FR-4 says only "account, academic and membership details". Fix: enumerate fields in FR-4. Keep only fields in the data model (for example "Date enrolled" only if stored). Add the QR shortcut or drop it.
- **G26 (Low). Event-type badge accent.** HLD 6.0 is titled "Event classifications" and has a Badge accent column. FR-6 and the DBML have no colour. Fix: drop the column from the wireframe, or add a field to event_types through a decision. Do not add it silently.
- **G27 (Low). Venue deactivation effect on existing events.** HLD 7.0 has "Deactivate facility". FR-7 blocks only new and draft selection and says nothing about Published or Completed Events. Fix: add "existing Published and Completed Events keep their venue; only new and draft selection is blocked".
- **G28 (Low). Account-request history.** HLD 3.0 has Pending / Approved / Rejected tabs and a Review details column showing the rejection reason. FR-2 covers the pending queue only. Fix: add a read-only finalized list to FR-2, showing decision, reviewer, time and reason, and never the password hash.
- **G29 (Low). Activity-log search.** HLD 20.0 has a keyword search box above the log. FR-34 says advanced filters are not provided and says nothing about basic search. Fix: state that a single keyword search is allowed, or remove the box.
- **G30 (Low). No message or copy catalogue.** The PRD has warning text inside FR bullets but no catalogue. Messages seen in the HLD **[all low-res; verify before use]**:
  - 12.0: "Registration closed" state; "Cancel registration?" with a finality warning.
  - 10.0: "Cancel event session" warning that registrations and QR access are cancelled and the waitlist is not promoted; cancellation reason required. "Mark event as completed?" warning that check-in closes, valid attendance is finalized and certificate review is enabled.
  - 14.0: "Invalidate attendance entry", reason required, warning about no re-check-in; camera-unavailable message; empty states for check-in log and queues.
  - 16.0: "Decline certificate eligibility", final-decision warning; "Issue official certificate?", cannot-change warning.
  - 19.0: "Deactivate user account", compulsory justification.
  - 2.0: acknowledgment after submission (FR-1 says only "acknowledgment").
  - Fix: add a message catalogue to the addendum, with ID, trigger, screen and text. Keep copy out of the PRD body.
- **G31 (Low, informational). "Decline requires reason" is not in the WBS or HLD.** WBS REG-02 and HLD 10.0 say only "record review details". FR-16 adds the reason requirement. Fix: confirm it is in SPEC; if not, list it as a PRD addition in §9.
- **G32 (Low). Past passes after the event ends.** HLD 13.0 shows Active and Past pass sections. FR-21 covers unconfirmed and cancelled Registrations but not completed Events. Fix: state whether past passes stay visible (read-only, no check-in) after completion.
- **G33 (Low). Cross-reference error in §9.** The "MD-05 avatar" row cites FR-4. The image-upload rule is FR-8, and the no-profile-picture rule is FR-4. Fix: cite both.
- **G34 (Low). Program and section lists undefined.** HLD 2.0 and 5.0 use a degree-program dropdown. FR-1 says "course" and names no source. Fix: state whether the list is a managed lookup (addendum lookup tables, which already require FKs) or free text.

## 5. Not gaps (checked and consistent)

- Override of login identifiers (§9): the HLD login copy is the only inconsistency (G21).
- Certificates in MVP and retained PDFs (§9): HLD 16.0 and 17.0 show a retained certificate view. HLD p.54 "future implementation" wording is superseded by FR-28.
- Full-event signup with waitlist warning, no seat assignment until confirmation (FR-14, FR-16): consistent with HLD 11.0, 12.0 and 10.0.
- Certificate eligibility rule (FR-27): consistent with HLD 16.0 Logic (completed event plus valid attendance plus approval).
- Event Type descriptions (FR-6): consistent with WBS MD-03.
- No profile pictures (§9, FR-4): consistent with HLD 4.0 and WBS MD-01.
- HLD and WBS schedule and estimate figures are outside this reconciliation. §10 Q2 already covers the baseline.
