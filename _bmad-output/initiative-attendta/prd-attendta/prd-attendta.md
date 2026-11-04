---
title: AttendTa
status: final
created: 2026-11-04
updated: 2026-11-04
---

# PRD: AttendTa — Student Organization Event Registration & Attendance System

## 0. Document Purpose

This PRD defines what AttendTa must do for the Group 7 Alliance Jumpstart (J4S) project. Client: Alliance Software Inc. It is written for the project team, the reviewer/approver, and downstream UX, architecture, and epic work. **AttendTa System Specification v2.1 (SPEC) is the authoritative baseline.** Where the UC function list, HLD, WBS, or Project Plan differ from SPEC, SPEC takes precedence unless this PRD records an explicit override (§9). The PRD builds on these inputs in `docsss/` and does not duplicate them:

Owner decisions confirmed on 2026-11-04 are reflected in this PRD and the current SPEC: email-only login; no login throttling in this MVP; Member 1 owns Authentication while Pinca owns Administration; overlapping event registrations are allowed; successful QR scans show participant name and student number; retained PDF certificates use a fixed HTML/CSS template; and release requires all 26 functional IDs plus every SPEC §20 acceptance item to pass. Older reconciliation and review reports in this folder are historical snapshots and do not override these decisions or the current PRD/SPEC.

- `AttendTa-System-Specification.md` (SPEC v2.1): detailed rules, cutoff matrix, acceptance coverage.
- `prd.md` (owner's project PRD): project goals, agreed refinements, and owner acceptance baseline; cross-checked during this alignment.
- `architecture.md` and `design.md`: implementation constraints and frontend reference.
- `UC-Project-FL.pdf` (UC): the course function list.
- `Alliance_Jumpstart_HLD_Group_7_AttendTa.pdf` (HLD): 20 screens and 26 functional IDs.
- `WBS_J4S_ATTENDTA_20261103.pdf` (WBS): estimates and functional IDs.
- `Parent_Project_Plan_J4S_AttendTa_v2.00.pdf` (Plan): team, environment, risks.
- `AttendTa-ERD.md` / `AttendTa-MVP.dbml`: the 10-table data model.
- `AttendTa-Sequence-Diagrams.md`: the key transaction flows.
- `git_qa_guidelines.pdf`: branching and QA tiers.

Features are grouped in §4. FRs carry global IDs (FR-1…FR-N) and cite their WBS/HLD functional ID in brackets, such as `[REG-04]`. Glossary terms (§3) are used verbatim. Technical choices live in `addendum.md`. Inferences are tagged `[ASSUMPTION]` and indexed in §11.

## 1. Vision

AttendTa is a student project for local or intranet use by one student organization. Students request membership. Officers publish events. Members sign up and receive a QR Pass. Officers check attendees in at the door. After the event, officers issue certificates and read participation reports. Today these steps are spread across forms, spreadsheets, and paper attendance sheets.

For **members**, the product gives one view (My Events) of every Registration made: whether a seat is confirmed, the QR Pass to present, whether attendance was recorded, and any issued Certificate to download. For **officers**, it replaces manual headcounts with capacity-based seat control and an explicit, fair Waitlist. Check-in becomes fast and duplicate-proof, and certificate eligibility rests on recorded attendance instead of memory. For **administrators**, every consequential action is attributable and retained.

The product's character is **explicit and conservative**. Nothing important happens automatically: no auto-promotion from the Waitlist, no auto-completion of events, no silent rewrites of history. Every state change has a human actor, a reason where it matters, and an audit trail. The scope is the reduced MVP agreed in SPEC v2.1. It keeps all 26 course functional IDs, but drops the broader v1.3 workflows and the UC extras listed in §7.2, so a five-person student team can deliver a correct, demonstrable MVP.

## 2. Target User

### 2.1 Jobs To Be Done

- **Applicant (student):** Get into the organization without chasing an officer in person, and know the request was received.
- **Member:** Find upcoming events, secure a seat or know where I stand, get in quickly on the day, and later prove I attended (Certificate).
- **Officer:** Publish an event once, control who gets a seat without overbooking, run check-in at the door with a phone or laptop, and close out the event with credible attendance numbers and certificates.
- **Administrator:** Decide who holds officer power, cut off access when someone leaves, and see who did what.
- **Organization (social):** Show advisers and the school credible participation evidence (attendance rate, course/year distribution).

### 2.2 Non-Users (v1)

- Multiple organizations or a campus-wide events office. One organization only.
- The public or non-member guests. Only approved Members register.
- Event sponsors, speakers, and external partners. They have no accounts.

### 2.3 Key User Journeys

`[ASSUMPTION: protagonists and beats were derived from SPEC flows and the HLD screens; confirm they match the intended demo script.]`

- **UJ-1. Ana requests membership and gets in.**
  Ana, a 2nd-year IT student, opens the AttendTa request page. She enters her student number, name, email, course, year, section, and a password. She sees an acknowledgment. No email follows; Officer Ben approves her request in the request queue, and an officer tells her offline. Ana logs in with her registered email and password, and lands on the event feed. **Edge case:** if Ben rejects the request with a reason, Ana submits a new, corrected request.

- **UJ-2. Ben publishes a seminar and fills it fairly.**
  Ben creates a draft with type "Seminar", venue Room 301 (capacity 60), a participant limit of 50, a Friday signup deadline, a Monday start, and a cover image. He publishes it, and signup opens immediately. Over the week he approves Pending Registrations; after 50, approvals become Waitlisted. A confirmed member cancels on Saturday. Ben clicks **Promote next eligible participant**, and the earliest-reviewed Waitlisted member is confirmed. **Edge case:** two officers approve the last seat at once. Exactly one succeeds; the other approval lands on the Waitlist.

- **UJ-3. Ana checks in at the door.**
  Ana opens My Events on Monday, sees "Confirmed", and opens her QR Pass. She saved it as a PNG the night before, in case the Wi-Fi is weak. Ben opens Check-in for the event and scans her code. The screen shows "Ana Reyes — 2023-01234 — checked in 09:02". A second scan shows the original time and records nothing. **Edge case:** Carlo's phone is dead, so Ben searches Carlo's student number, selects him and clicks **Check in**.

- **UJ-4. Ben closes the event and issues certificates.**
  After the seminar, Ben marks the event Completed, which closes Check-in. Certificate Reviews appear for every attendee with Valid Attendance. Ben approves them one by one, then issues each Certificate, producing a fixed-template A4 PDF with a unique number. Ana downloads hers from My Events. Ben opens the participation report: 50 confirmed, 46 attended, 4 no-shows, 92% attendance rate, broken down by course and year.

- **UJ-5. Admin Dana removes a departed officer.**
  Dana, the Administrator, deactivates a graduating officer's account with a reason. The screen first shows the upcoming Registrations that will be cancelled. The officer's existing session stops working. The deactivation appears in the Activity Log.

## 3. Glossary

- **Applicant** — A person who has submitted an Account Request and has no Account yet.
- **Account Request** — A membership application: identity, academic details, and a chosen password. States: `pending → approved | rejected`. One approved request creates exactly one Account and one Member Profile.
- **Account** — A login identity (name, email, password hash, Role, active flag). It has zero or one Member Profile.
- **Role** — Fixed permission set on an Account: **Member**, **Officer**, or **Administrator**. Administrator includes all Officer capabilities.
- **Member Profile** — Student number (unique), course, year level, section, optional mobile, membership type, and membership status (active/inactive). Membership status is distinct from Account active state.
- **Eligible Participant** — A Member whose Account is active and whose membership status is active. Signup, review, Promotion, and Check-in all require it. Management Roles never bypass eligibility or capacity for their own participation.
- **Management** — Collective term for Officer and Administrator.
- **Event Type** — A named category (e.g. seminar, outreach). Active or inactive.
- **Venue** — A room with a capacity, location, notes, contact person and a required photo. Active or inactive.
- **Event** — A scheduled activity with a title, description, Event Type, Venue, start time, participant limit, Signup Deadline, and Cover. States: `draft → published → completed`; draft or published → `cancelled`.
- **Cover** — The event image. Required before publication.
- **Signup Deadline** — The time at or after which new Registrations are rejected. The Event stays Published.
- **Event Start Cutoff** — The event's start time. At or after it, reviews, Promotions, and member cancellations are rejected.
- **Registration** — One Member's request for one Event. At most one per Member per Event, ever. States: `pending`, `confirmed`, `waitlisted`, `cancelled`. Captures a course/year/section snapshot at signup.
- **Seat** — Capacity held by a Confirmed Registration. Seats in use never exceed the participant limit.
- **Waitlist** — Reviewed, Waitlisted Registrations, ordered by review time, then registration ID.
- **Promotion** — The Officer action that confirms the next eligible Waitlisted Registration when a Seat is free.
- **Not confirmed before cutoff** — The display label for Pending or Waitlisted Registrations once the Event Start Cutoff has passed.
- **Check-in** — The officer-controlled window (open/closed) during which Attendance Records can be created for a Published Event.
- **QR Pass** — A QR image that encodes only an unpredictable token, unique to one Confirmed Registration.
- **Attendance Record** — The check-in of one Registration: method (`qr_code`, `manual_search` by name, or `student_number` lookup), recorder, and time. At most one per Registration.
- **Valid Attendance** — An Attendance Record that has not been invalidated.
- **No-show** — A Confirmed Registration on a Completed Event without Valid Attendance.
- **Certificate Review** — An eligibility decision for one Registration with Valid Attendance on a Completed Event. States: `pending_approval → eligible | not_eligible`.
- **Certificate** — The retained fixed-template PDF issued from an `eligible` Certificate Review. It has a unique certificate number and immutable snapshots.
- **Activity Log** — An append-only record of consequential actions: actor, action, entity, time, reason, and old/new values.

## 4. Features

### 4.1 Account Requests & Authentication

**Description:** Applicants self-register by submitting an Account Request. Management approves or rejects it. Approval atomically creates the Account and the Member Profile; there is no separate "Add Member" path. No email is sent, and outcomes are communicated offline. Realizes UJ-1.

#### FR-1: Submit Account Request [AUTH-01]
An Applicant can submit an Account Request with student number, full name, email, course, year level, section, password, and confirmation; mobile number is optional.

**Consequences (testable):**
- Missing required fields, a year level ≤ 0, a malformed email, or a confirmation that doesn't match the password → the request is rejected with field errors.
- The password is stored only as a hash. The confirmation is never stored.
- The request is rejected if its email or student number matches another *pending* request, or belongs to any existing Account or Member Profile (including inactive ones).
- The Applicant sees an acknowledgment only. There is no status page and no tracking token, and requests are not retrievable by ID, email, or student number.
- Applicants cannot choose a Role, membership type, or membership status.
- No institutional email domain is enforced.

#### FR-2: Review Account Requests [AUTH-02]
Management can approve or reject *pending* Account Requests.

**Consequences (testable):**
- Approval atomically does all of the following: creates an active Member-role Account and its linked Member Profile, moves the password hash to the Account, clears it from the request, records the reviewer and time, and writes the Activity Log entry.
- At approval, the system re-checks that the email and student number are still unused by any Account or Member Profile. If either is taken, the approval is rejected.
- Rejection requires a reason, clears the request's password hash, and creates no Account.
- Two concurrent decisions on the same request produce exactly one outcome.
- A finalized request cannot be reviewed again. A rejected Applicant can submit a new request; the earlier request is preserved.
- Management screens never show password hashes.

#### FR-3: Log in and log out [AUTH-03]
A user with an active Account can log in with the registered **email address** and password, and log out. Email is the only login identifier; student number remains profile and manual check-in data.

**Consequences (testable):**
- Wrong credentials, a pending request, and an inactive Account each produce a generic failure that does not reveal whether the email exists or the Account is active.
- On success, a signed, expiring cookie session is issued, and the user is routed by Role.
- Logout clears the cookie. Revoking individual copied tokens is not promised.
- Every protected request re-checks the current Account active state and current Role, so deactivation or demotion takes effect on existing sessions.
- Login throttling and lockout are excluded from this MVP.

**Notes:**
- Login email is read-only (FR-5). A mistyped login email cannot be corrected or recovered through a self-service flow in this MVP.

### 4.2 Membership & Master Data

**Description:** Management maintains Member Profiles, Event Types, and Venues. Members can view but not edit their own profile. Images are validated and stored privately on the server.

#### FR-4: View own profile [MD-01]
A Member can view their own account, academic, and membership details.

**Consequences (testable):**
- Editing, profile pictures, and password changes are not available.

#### FR-5: Maintain Member Profiles [MD-02]
Management can edit full name, course, year level, section, mobile number, membership type, and membership status.

**Consequences (testable):**
- A student-number correction requires a reason, enforces uniqueness, and logs the old and new values. A corrected-away number becomes available to new Account Requests; this is an accepted limitation (§9.1).
- Login email is read-only for everyone.
- Setting membership to inactive makes the Member no longer an Eligible Participant. It applies the same preview, reason, and upcoming-Registration cancellation as FR-32. It does not change Account access, and Account access changes do not change membership.
- Profile edits never rewrite Registration snapshots or issued Certificates.

#### FR-6: Maintain Event Types [MD-03]
Management can create and edit Event Types (unique, non-blank name and description) and activate or deactivate them.

**Consequences (testable):**
- Inactive types cannot be selected for new or draft Events. Existing references and names are preserved.

#### FR-7: Maintain Venues [MD-04]
Management can create and edit Venues: room name, positive capacity, location, availability notes, contact person, active state, and a **required photo**.

**Consequences (testable):**
- A Venue cannot be created without a photo. The photo can be replaced but not removed.
- Capacity cannot drop below the participant limit of any linked draft or published Event.
- Inactive Venues cannot be selected for new or draft Events. Availability notes do not imply booking-conflict detection.

#### FR-8: Upload and replace record images [MD-05]
Management can upload a Venue photo or Event Cover.

**Consequences (testable):**
- Only JPEG/PNG, verified by content (not just extension), up to 2 MiB.
- Filenames are generated by the server; one current image per record.
- A failed file write or database write leaves the previous image intact.

### 4.3 Event Lifecycle

**Description:** Events move `draft → published → completed`. A Draft or Published Event can be Cancelled. Publication makes the Event visible and opens signup at once. Completion is always an explicit Management action. Realizes UJ-2, UJ-4.

#### FR-9: Create and edit draft Events [EVT-01, EVT-03]
Management can create and edit draft Events with title, description, active Event Type, active Venue, start time, participant limit (> 0 and ≤ Venue capacity), Signup Deadline (≤ start), and an optional Cover.

**Consequences (testable):**
- Drafts are hidden from Members.

#### FR-10: Publish Event [EVT-01, EVT-02]
Management can publish a draft Event.

**Consequences (testable):**
- Publication requires an active Event Type and Venue (revalidated), a Cover, a valid capacity, and a Signup Deadline in the future.
- A draft referencing an Event Type or Venue deactivated since creation remains editable, but cannot be published until changed to an active one. Published Events keep their references.
- The publisher and time are recorded and audited.
- Publication makes the Event visible in the Member feed and opens signup immediately. There is no separate "open registration" action.

#### FR-11: Edit published Event [EVT-03]
Management can change only the title, description, and Cover (replacement) of a Published Event.

**Consequences (testable):**
- Schedule, type, venue, participant limit, and Signup Deadline are locked.
- Completed and Cancelled Events are read-only.
- Edits log old and new values. Issued Certificates keep their snapshots.

#### FR-12: Cancel Event [EVT-04]
Management can cancel a Draft or Published Event that has no Valid Attendance and no issued Certificate.

**Consequences (testable):**
- A reason is required.
- Cancellation atomically sets the status, cancels all pending, confirmed, and waitlisted Registrations, invalidates their QR Passes, closes Check-in, and writes the audit entry.
- No Promotion happens. A cancelled previously published Event stays visible to affected Members as Cancelled. A cancelled Draft stays hidden.

#### FR-13: Complete Event [EVT-04]
Management can mark a Published Event Completed at or after its start time.

**Consequences (testable):**
- Completion closes Check-in and creates a Certificate Review for every Registration with Valid Attendance. It enables No-show reporting. A later attendance invalidation (FR-25) can still change those figures.
- Time passing never completes an Event. Completion before the start time is rejected. Completed and Cancelled Events cannot be republished.
- `[ASSUMPTION: "concluded" means the Officer judges the event over; the system enforces only now ≥ start.]`

### 4.4 Registration & Waitlist

**Description:** Members sign up; Management reviews. A Seat is held only by a Confirmed Registration. Full Events still accept signups for review, with a warning that no Seat is guaranteed. Promotion is manual and fair across the whole Waitlist. Realizes UJ-2.

#### FR-14: Browse and register [REG-01]
A Member who is an Eligible Participant can view Published Events and submit a Registration before the Signup Deadline.

**Consequences (testable):**
- The Registration is created as `pending` with a course/year/section snapshot and the submission time.
- Signup is rejected if any Registration already exists for this Member and Event, including a cancelled one. There is no reapplication.
- On a full Event, signup is still allowed, with a "no seat guaranteed" warning.
- Members can register for several Events regardless of time overlap.
- The Signup Deadline is shown in the organization's local timezone, with a label.

#### FR-15: Enforce Signup Deadline [EVT-02, REG-05]
At or after the Signup Deadline, the server rejects new signups, including those from stale forms.

**Consequences (testable):**
- The Event stays Published, and existing Registrations are unaffected.

#### FR-16: Review and administratively cancel Registrations [REG-02]
Management can approve or decline Pending Registrations while the Event is Published and before the Event Start Cutoff, including after the Signup Deadline.

**Consequences (testable):**
- Decline sets the Registration to `cancelled` and requires a reason.
- Management can also administratively cancel a Confirmed or Waitlisted Registration before the Event Start Cutoff, provided it has no Valid Attendance. A reason is required. The QR Pass is invalidated, the Seat is released, and no Promotion follows.
- Approve confirms the Registration only if a Seat is free **and** no earlier eligible Waitlisted Registration has priority. Otherwise it becomes `waitlisted`.
- Capacity, queue, and eligibility are rechecked inside one seat-allocation transaction. Concurrent last-seat approvals never overbook.
- Confirmation records the actor and time and generates a unique, unpredictable QR token.

#### FR-17: Promote from Waitlist [REG-04]
Management can trigger **Promote next eligible participant** on a Published Event before the Event Start Cutoff.

**Consequences (testable):**
- The Waitlisted Registration chosen is the earliest one whose owner is still an Eligible Participant, ordered by `reviewed_at`, then `registration_id`, across the whole queue (not the visible page). Ineligible entries are skipped and left unchanged. A Seat must be free.
- No candidate or no free Seat → no change.
- Promotion keeps the original review data, records the confirmation actor and time, and generates a QR token.
- Promotion is serialized with competing approvals. Nothing ever promotes automatically, including cancellations and deactivations.

#### FR-18: Member cancels own Registration [REG-03]
A Member can cancel their own pending, confirmed, or waitlisted Registration while the Event is Published, before the Event Start Cutoff, and before Valid Attendance exists.

**Consequences (testable):**
- An explicit confirmation warns that cancellation is final.
- Cancellation atomically records the actor and time, plus an optional reason. It invalidates the QR Pass and releases the Seat.
- No Promotion follows, and there is no "Register again".

#### FR-19: Enforce Event Start Cutoff [REG-05]
At or after the Event start, the server rejects reviews, confirmations, Promotions, and member cancellations.

**Consequences (testable):**
- Pending and Waitlisted rows stay unchanged and are shown as **Not confirmed before cutoff**. They are not cancelled and not counted as No-shows. Confirmed rows stay confirmed.
- No background job runs. All time guards are evaluated per request, with times stored in UTC.

#### FR-20: My Events [REG-01, ATT-03]
A Member can see their own Registrations with status, a QR Pass when Confirmed, Attendance Record state (including invalidation), and download links for issued Certificates.

**Consequences (testable):**
- Status changes made by Management appear on the Member's next visit. No notifications are sent.

### 4.5 QR Pass & Check-in

**Description:** Each Confirmed Registration gets its own QR Pass; it is not a permanent student QR. Check-in is a window that Management opens and closes explicitly. Check-in uses QR scanning, with a manual fallback by name or student number. Realizes UJ-3.

#### FR-21: View and download QR Pass [QR-01]
The owner of a Confirmed Registration can view its QR Pass and download it as a PNG.

**Consequences (testable):**
- The QR encodes only the token, with no personal data. The PNG is generated on demand and never stored.
- Ownership is checked before display or download.
- Cancelling the Registration clears the token, so saved copies stop working. Unconfirmed and cancelled Registrations have no usable QR Pass.
- The "Print pass" control on HLD screen 13.0 is delivered as **Download PNG**; in-system printing is deferred (§7.2).

#### FR-22: Control Check-in window [ATT-01]
Management can open and close Check-in for a Published Event at any time, including early or reopening.

**Consequences (testable):**
- Each toggle is audited.
- Completion and Cancellation close Check-in. Check-in never opens automatically.

#### FR-23: Check in by QR scan [ATT-01]
Management can scan a QR Pass. The server validates: Check-in open, Event Published, token belongs to *this* Event, Registration Confirmed, Account and membership active, no existing Attendance Record.

| Scan result | Outcome |
|---|---|
| Valid | Record attendance (method, recorder, UTC time); show name and student number |
| Already checked in | Show the original check-in time; no insert |
| Wrong Event | Reject |
| Ineligible, cancelled, unconfirmed, or invalidated attendance | Reject; no insert |
| Check-in closed or Event not Published | Reject |
| Unknown token | Reject without revealing other records |

**Consequences (testable):**
- A unique constraint plus transaction logic guarantees at most one Attendance Record per Registration.
- Check-in does not change the Registration's status.
- A QR Pass, or a screenshot of one, does not prove identity. The scan result shows the name and student number so the Officer can verify the person in front of them. This is an accepted risk (§9.1).

#### FR-24: Manual check-in fallback [ATT-01]
Management can look up a Confirmed participant by name or student number, select them, and click **Check in**.

**Consequences (testable):**
- The method is recorded as `manual_search` (name) or `student_number`.
- The same validations as FR-23 apply.
- Searching, viewing, or downloading never records attendance.

#### FR-25: Invalidate Attendance Record [ATT-02]
Management can invalidate an Attendance Record after confirming and giving a reason.

**Consequences (testable):**
- The original row is kept. The Registration stays Confirmed.
- The invalidated record is excluded from reports and eligibility. Any unissued Certificate Review becomes `not_eligible` with the correction reason.
- Blocked once a Certificate has been issued for the Registration.
- Re-check-in after invalidation is rejected; the UI warns about this before the correction is made.
- Invalidation is serialized against Certificate issuance.

#### FR-26: View own attendance [ATT-03]
A Member can see their own Attendance Records, with invalidated records clearly marked.

**Consequences (testable):**
- Members cannot check themselves in or correct their attendance.

### 4.6 Certificates

**Description:** A Certificate is earned by **Valid Attendance AND a Completed Event AND Management approval**. All three are required. Issuance is one at a time, to a fixed template, and the retained PDF is never rewritten. Realizes UJ-4.

#### FR-27: Review certificate eligibility [CERT-01]
Management can approve (`eligible`) or decline (`not_eligible`, reason required) each pending Certificate Review.

**Consequences (testable):**
- Declines are final; the UI warns before declining. Reviewer and time are recorded.
- At approval, the system re-checks Valid Attendance, serialized against invalidation (FR-25).
- Officers may review their own participation, with the same validation and audit.
- `[ASSUMPTION: no self-approval prohibition, per SPEC §2.]`

#### FR-28: Issue Certificate [CERT-02]
Management can issue one `eligible` Certificate at a time.

**Consequences (testable):**
- The output is a landscape A4 PDF from one fixed template with embedded fonts. It contains a unique certificate number and snapshots of the recipient's full name, the Event title and start, the issue date and the issuer.
- At issuance the system re-checks approval, Valid Attendance, and completion, serialized against attendance corrections.
- Duplicate concurrent issuance for one Registration is impossible.
- A failed file write leaves the Certificate unissued. After a failed database commit, the unreferenced file is removed or reconciled. A retry never replaces a successfully issued PDF.
- Success is reported only after the final commit.

#### FR-29: Download Certificate [CERT-02]
The active Account owner, or Management, can download an issued Certificate.

**Consequences (testable):**
- The PDF is streamed (`application/pdf`) through an authorized endpoint, with permission checked on every request. It is never served as a static file or through caller-supplied paths.
- Inactive membership does not revoke access; an inactive Account blocks it.
- Later edits to the profile or Event never alter a retained PDF.

### 4.7 Participation Reports

#### FR-30: View Event participation report [REP-01]
Management can view, per Event:
- Registrations by status
- Confirmed participants and Valid attendees
- No-shows
- Attendance rate
- Course/year distribution of actual attendees, taken from signup snapshots
- Participant detail rows show the participant's name, student number, course/program, registration status, attendance result, and check-in time when present.

**Consequences (testable):**
- Attendance rate = Confirmed with Valid Attendance ÷ Confirmed × 100. When there are no Confirmed Registrations, the rate shows **N/A**.
- Pending, Waitlisted, and Cancelled rows, including those Not confirmed before cutoff, are excluded from expected attendance and from No-shows.
- No-show figures appear only for Completed Events. Cancelled Events are shown separately.
- Totals are computed over the full dataset; pagination only affects detail rows.
- Account or master-data inactivity never hides historical figures.

### 4.8 Administration & Audit

**Description:** Administrators manage Roles and Account access. Every consequential action is logged in the Activity Log. Realizes UJ-5.

#### FR-31: Manage Roles [ADM-01]
An Administrator can assign Member, Officer, or Administrator to an Account.

**Consequences (testable):**
- The last active Administrator cannot be demoted.
- Roles are fixed; there is no custom permission editor.

#### FR-32: Deactivate and reactivate access [ADM-01]
Management can change membership status (FR-5). An Administrator can change Account active state. Before either change is applied, the screen shows the affected upcoming Registrations and asks for a reason.

**Consequences (testable):**
- For Events not yet started, deactivation cancels the person's pending, waitlisted, and confirmed Registrations that have no Valid Attendance, invalidates their QR Passes, and releases their Seats. No Promotion follows.
- For Events that have started or completed, history is kept, but the person cannot be checked in. A Confirmed Registration without Valid Attendance therefore counts as a No-show once the Event completes. This is an accepted trade-off of SPEC §5 (§9.1).
- Account deactivation ends existing sessions on their next request.
- The last active Administrator cannot be deactivated.
- Reactivation restores access only. It does not bring back cancelled Registrations, released Seats, or past attendance.
- Issued Certificates are preserved.

#### FR-33: Record Activity Log [ADM-02]
The system appends an Activity Log entry for each of the following:
- Request decisions
- Role, access, and membership changes
- Profile and identity corrections
- Image changes
- Event publication, edits, cancellation, and completion
- Check-in toggles
- Registration reviews, Promotions, and cancellations
- Attendance check-ins (each new Attendance Record) and invalidations
- Certificate decisions and issuance

**Consequences (testable):**
- Each entry stores actor type and Account, action, entity type and ID, description or reason, UTC timestamp, and validated JSON old/new values.
- The business change and its log entry commit together.
- Entries never contain passwords, hashes, QR tokens, or physical file paths.
- Entries are append-only.

#### FR-34: View Activity Log [ADM-02]
An Administrator can view a read-only, newest-first list showing actor, action, affected record, time, and reason.

**Consequences (testable):**
- Advanced filters and a field-by-field comparison view are not provided.

#### FR-35: Initial administrator setup
A one-time setup script creates the first active Administrator from a supplied name, email, and password, stored as a hash.

**Consequences (testable):**
- There are no hard-coded default credentials.
- Public requests can never grant privileged Roles.

### 4.9 Shared List Behaviour

#### FR-36: Fixed pagination
Every list screen shows 25 rows per page with Previous/Next navigation. This covers:
- Requests and accounts
- Members, types, and venues
- Events feed
- Review and personal registrations
- Manual search results
- Attendance histories
- Certificate queues
- Report detail rows
- Logs

**Consequences (testable):**
- Filters, authorization, and search are applied before counting and slicing.
- Ordering is deterministic, with a unique ID as the tie-breaker.
- A missing or invalid page shows page 1. An out-of-range page shows the last page.
- Navigation is hidden when there is only one page. An empty state shows when nothing matches.
- Actions are revalidated against current records, not page positions.
- Counts never include records the user is not authorized to see.

## 5. Cross-Cutting NFRs

- **NFR-1 Security:**
  - Passwords are stored only as salted hashes.
  - Every deployment other than localhost uses HTTPS (TLS). This is required for phone-camera QR scanning, which browsers allow only in a secure context, and it protects credentials and session cookies.
  - Sessions use signed, expiring cookies.
  - Every mutation has antiforgery protection.
  - Every personal resource has an ownership check.
  - Every time, capacity, and state guard is enforced on the server.
  - Secrets, hashes, and QR tokens are never logged.
  - Login throttling and lockout are excluded from the MVP (§9.1).
- **NFR-2 Integrity and concurrency:** Seat allocation, Promotion, approval, issuance, and attendance correction are transactional and serialized where they compete. Uniqueness is enforced at the database level where possible: one Account per email, one Member Profile per student number, one Registration per Member/Event, one Attendance Record per Registration, one Certificate per Registration.
- **NFR-3 Retention:** No hard deletes of business records, and no archive or restore functionality. Inactive identities keep their identifiers reserved. The Activity Log is append-only.
- **NFR-4 File storage:** Images and Certificates live on persistent server-local storage that survives restart and redeploy. Storage sits outside the web root and deployment directory, and only the application account can read it. File storage and SQL Server are backed up and restored together.
- **NFR-5 Time:** All times are stored in UTC and shown in the configured organization timezone with a label. No scheduled jobs.
- **NFR-6 Platform:** Web application accessed on localhost or over the intranet; intranet access uses HTTPS (NFR-1). Chrome is the target browser; it must be verified during implementation. Layouts are mobile-responsive so check-in works on an officer's phone over a hotspot. Laptops are designed for 1920×1080. Officers need network access to scan; Members may show an offline saved QR Pass.
- **NFR-7 Privacy:** The QR Pass carries no personal data. Unknown-token and unauthorized responses reveal nothing about other records. Email is required but not verified.

## 6. Non-Goals (Explicit)

- Not a multi-organization or campus-wide platform.
- Not a notification system: no outgoing email (Brevo), SMS, or push.
- Not an identity provider: no email verification, password reset or recovery, self-service email or password change, SSO, or session/device management.
- Not automated: no automatic Waitlist promotion, scheduled closeout, auto-completion, or mass auto-cancellation.
- Not a document designer: no certificate template editor, bulk issuance, revocation, or reissue.
- Not an analytics suite: no CSV/PDF report export, and no advanced log filtering.
- Not a venue-booking system: no conflict detection.

## 7. MVP Scope

### 7.1 In Scope
All 26 functional IDs in the HLD and WBS:
- **Accounts:** AUTH-01–03
- **Master data:** MD-01–05
- **Events:** EVT-01–04
- **Registration:** REG-01–05
- **QR and attendance:** QR-01, ATT-01–03
- **Certificates and reports:** CERT-01–02, REP-01
- **Administration:** ADM-01–02

These are delivered across the 20 HLD screens, together with FR-35 (initial administrator setup) and FR-36 (pagination).

### 7.2 Out of Scope for MVP
| Item | Reason / status |
|---|---|
| Known-password change (US-04) | Deferred to v2 |
| Reopening declined certificate decisions | Deferred; declines are final in v1 |
| Re-check-in after invalidation; attendance duration | Deferred |
| Course/year distribution of confirmed (not just attended) participants | Deferred |
| Operational edits after publish (reschedule, capacity increase) | Deferred; cancel and recreate instead |
| Self-service profile edits; profile pictures | Excluded; Officers correct profiles |
| Applicant-facing request status tracking | Excluded; outcomes communicated offline |
| Custom permission levels (UC "set permission levels") | Narrowed to fixed Roles |
| In-system QR printing | Deferred; HLD 13.0 "Print pass" ships as Download PNG; Members print externally |
| Report exports, advanced log filters, field comparison | Deferred |

`[NOTE FOR PM]` The Plan's risk mitigation (p.10) proposes deferring certificates if the schedule slips. This PRD keeps certificates in full (FR-27–29). If scope must be cut, the cut order should be decided explicitly rather than taken by default.

## 8. Success Metrics

**Primary**
- **SM-1 Acceptance coverage:** every SPEC §20 acceptance scenario passes in the client demo. SPEC §20 is the full checklist. It includes, among others:
  - Duplicate and concurrent account decisions
  - Deadline and start boundaries, including stale forms
  - Concurrent last-seat approval and Promotion
  - Every QR scan outcome in FR-23
  - Issuance concurrent with correction
  - Unauthorized downloads
  - Zero-denominator reports
  - Persistent storage across redeploy

  Target: 100%. Validates FR-1–36.
- **SM-2 Functional completeness:** all 26 functional IDs are demonstrable end to end on the integrated `dev` build. Target: 26/26. Validates §7.1.

**Secondary**
- **SM-3 Quality gates:** every merged PR has green CI and at least one peer approval (Tier 2), and its author attests Tier 1 (local tests and lint) in the PR. The release from `dev` to `main` passes Tier 3 manual E2E. Target: 100% of PRs.
- **SM-4 Integrity under concurrency:** zero overbooked Events, zero duplicate Attendance Records, and zero duplicate Certificates, across scripted concurrency tests. Validates FR-16, FR-17, FR-23, FR-28.

**Counter-metrics (do not optimize)**
- **SM-C1 Feature count beyond scope:** adding deferred items (exports, email, auto-promotion) to look complete. Counterbalances SM-2. Out-of-scope work threatens the correctness of the core flows.
- **SM-C2 Demo-path-only testing:** passing the happy-path demo while boundary and concurrency cases go untested. Counterbalances SM-1. SPEC §20 is weighted toward edge cases on purpose.

## 9. Recorded Decisions & Source Reconciliation

| Topic | Sources disagree | Decision |
|---|---|---|
| Authority | UC/HLD/WBS/Plan vs SPEC | SPEC v2.1 wins unless overridden here |
| Login identifier | UC: email or student number; SPEC and current owner decision: email only | Registered email only; student number is not a login identifier (FR-3) |
| Certificate eligibility | UC: attendance *or* completion *or* approval | SPEC: all three required (FR-27) |
| Certificate PDF retention | HLD p.54: future; WBS: now | Retained now (FR-28) |
| Certificates in MVP | Plan p.10 proposes deferral | In scope (§7.2 note) |
| Permission levels | UC: configurable | Fixed Roles (FR-31) |
| Report distribution | UC: course/year (all) | Actual attendees only (FR-30) |
| Full-event signup | UC: "validates capacity" | Signup allowed; Seat assigned only at confirmation (FR-14, FR-16) |
| Event Type description | HLD omits; WBS includes | Included (FR-6) |
| MD-05 "avatar" label | WBS label vs body | No profile pictures (FR-4) |
| Email domain | HLD "institutional email" | No domain restriction (FR-1) |

### 9.1 Accepted Risks & Known Limitations

- **No login throttling.** Password guessing against registered emails is not rate-limited in this MVP; this is an accepted course-project limitation.
- **Proxy check-in.** A shared QR screenshot can be presented by someone else. The Officer mitigates this by verifying the name and student number on the scan result (FR-23).
- **Student-number reuse.** A number freed by a correction (FR-5) can be claimed by a new Applicant. Corrections are rare and audited.
- **Late ineligibility.** A Member made inactive after an Event starts cannot be checked in and counts as a No-show (FR-32), per SPEC §5.
- **Mistyped login email.** It cannot be corrected in this MVP (FR-5); there is no self-service email-change or credential-recovery flow.

## 10. Open Questions

None of these blocks UX, architecture, or epic planning. Each is deferred with an owner and a revisit trigger.

| ID | Question | Owner | Revisit when |
|---|---|---|---|
| Q1 | **Authorized certificate signatory.** The certificate requires the authorized person's name, title and signature. Their actual details remain to be supplied before official issuance. The fixed HTML/CSS template and Asia/Manila timezone follow the current project documents. | PM (Talisic) | Before official certificate issuance |
| Q2 | **Hotspot setup.** Configure storage, the host address and trusted HTTPS so a phone can scan with Chrome over a hotspot. | Technical Manager (Gabutan) | Before the demo |

## 11. Assumptions Index

- §2.3: User journey protagonists and beats were derived from SPEC and HLD flows and need confirming against the demo script.
- §4.3 FR-13: "Concluded" means Officer judgment; the system enforces only now ≥ start.
- §4.6 FR-27: Officers may approve their own certificate eligibility (no self-approval prohibition, per SPEC §2).
- The certificate subject is the Member; Management can review/issue according to the same validation and audit rules, including for their own participation.
- §5 NFR-6: Mobile-responsive layouts are expected to support phone-based check-in; verify the actual phone, Chrome, camera and hotspot setup.
