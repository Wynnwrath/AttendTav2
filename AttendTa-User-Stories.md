# AttendTa — Revised MVP User Stories

Version: 2.1 — UC-aligned reduced MVP
Requirements baseline: `../docs/UC-Project-FL (1).pdf`, plus the explicit refinements agreed in the scope review.
Stated original submission deadline: October 7, 2026 (historical; not a new delivery promise).

These revised copies supersede the broader workflow descriptions for this revised baseline only. Original files in `docs/` remain unchanged. The original specification v1.3 describes the previous scope and must not override these agreed refinements. Ten core business tables are retained; SQL Server lookup tables implement the readable DBML enums.

## Actors

Applicant; member; officer (also an eligible member); administrator; system validation. Administrators need a member profile only to participate. Fixed roles never bypass capacity or personal ownership rules.

## US-01 — Request an account

**As a student, I want to request access for membership verification.**

Acceptance criteria:
- Collect student number, full name, email, course, positive year level, section, optional mobile, password and confirmation; hash passwords and never retain confirmation.
- Create a pending request only; prevent duplicate pending email/student number and existing account/member identities, including inactive identities.
- Show acknowledgment without promising email or public tracking; allow a separate new account request after rejection while retaining prior requests. Event reapplication is a different, deferred workflow.

## US-02 — Review account requests

**As a officer/admin, I want to approve valid organization members.**

Acceptance criteria:
- List/view requests without exposing hashes. Approval atomically creates an active member-role user and profile, links created_user_id and records reviewer/time plus audit.
- Transfer the hash and clear it from the finalized request. Rejection requires a reason, clears the hash and creates no user/profile.
- Applicants cannot select privileged roles; profiles are created only by approval.

## US-03 — Log in and log out

**As a approved user, I want to access only authorized functionality.**

Acceptance criteria:
- Validate email/password using hashing; deny pending applicants/inactive accounts. Replace placeholder login and reversible password storage.
- Use signed expiring cookie authentication; check current account/role on protected requests. Logout clears the cookie; no individual server-side token revocation is promised.

## US-05 — Manage roles and account access

**As a administrator, I want to assign responsibility and control access.**

Acceptance criteria:
- Assign fixed member/officer/admin roles, deactivate/reactivate with reasons, preserve linked profiles and protect the last active admin.
- Block existing/new access on deactivation; apply the shared future-registration cancellation rules, preserving past/in-progress history. Do not automatically promote or restore cancelled seats.

## US-06 — View my profile

**As a member, I want to see my membership information.**

Acceptance criteria:
- Show own account/member details; retrieve current name/email from users and academic/member details from members.
- Profile-detail changes are handled by officers. Profile-picture uploads are not included; password changes are deferred.

## US-07 — Maintain members

**As a officer/admin, I want to keep approved membership information accurate.**

Acceptance criteria:
- List/view/edit approved members, full name, academic/contact details, membership type and active/inactive state; no independent Add Member action.
- Validate student-number uniqueness; require correction reasons and relevant old/new audit values. Email changes are outside this MVP.
- Apply shared loss-of-eligibility rules without rewriting academic signup snapshots or issued PDFs; membership inactivity does not silently deactivate login.

## US-08 — Manage event types

**As a officer/admin, I want to classify events consistently.**

Acceptance criteria:
- Create/edit unique nonblank names and descriptions; activate/deactivate without archive/delete controls.
- Only active types are selectable for draft events; retain historical references and uniqueness across inactive records.

## US-09 — Manage venues

**As a officer/admin, I want to maintain suitable event locations.**

Acceptance criteria:
- Maintain room name, positive capacity, location, availability notes, contact person, required photo and active state.
- Reject creation without a valid photo. Permit replacement, not removal without replacement. Preserve event references on deactivation.
- Prevent venue capacity edits below limits of linked draft/published events. No automated booking-conflict detection.

## US-10 — Manage event and venue images

**As a officer/admin, I want to recognize events and venues.**

Acceptance criteria:
- Officers/admins manage required venue photos and event covers. Draft covers may be absent; publication requires one. User profile-picture uploads are not included.
- Use one shared JPEG/PNG upload service, maximum 2 MiB, validated content and generated filenames. Allow draft-cover removal only while optional.
- Persist relative paths locally; enforce shared storage, replacement cleanup, authorization and event-edit rules.

## US-11 — Create and publish events

**As a officer/admin, I want to announce activities and accept signups.**

Acceptance criteria:
- Create editable drafts with title, description, type, venue, start, limit and deadline. Validate active masters, positive limit within venue capacity and deadline no later than start.
- Require a valid cover and future deadline at publication; save actor/time and audit. Publishing immediately opens signup until deadline; hide drafts from members.

## US-12 — Show registration availability

**As a member, I want to understand whether I can register.**

Acceptance criteria:
- Published events display Open before deadline and Closed at/after the exact deadline, with labelled local date/time. No separate Open Registration control or database fields.
- Server rejects late submissions even if the form was opened earlier. The event remains published when signup closes.

## US-13 — Edit event details

**As a officer/admin, I want to correct descriptions without changing commitments.**

Acceptance criteria:
- Edit all draft fields. After publication permit only title, description and cover replacement; lock schedule, venue, type, participant limit and deadline.
- Completed/cancelled records are read-only; keep required cover and log changes. Issued certificate snapshots never change.

## US-14 — Cancel or complete events

**As a officer/admin, I want to record the actual event outcome.**

Acceptance criteria:
- Cancel eligible drafts/published events with reason; atomically cancel active registrations, invalidate tokens, close check-in and log. Preserve existing cancellation reasons.
- Valid attendance, completed state or an issued certificate blocks cancellation. Affected members can still identify previously published cancelled events.
- Complete published events explicitly at/after start once concluded; close check-in and enable valid-attendee certificate review. Elapsed time alone does not complete an event.

## US-15 — Browse and register

**As a active member, I want to request participation in an event.**

Acceptance criteria:
- Browse published events/details/covers and submit only before deadline with active account/member eligibility.
- Create pending registration plus course/year/section snapshots and submission time. One row per member/event including cancelled rows; no event reapplication.
- Full events may accept pending requests for review/waitlisting with a no-guaranteed-seat warning; no overlapping-event restriction.

## US-16 — Review registrations

**As a officer/admin, I want to assign reviewed requests to seats or the waitlist.**

Acceptance criteria:
- Before event start, Approve or Decline existing pending requests even after signup deadline; require a published event and eligible participant.
- Approve to confirmed only with capacity and no prior eligible waitlist; otherwise waitlisted. Decline to cancelled with reason.
- Record reviewer/time and recheck constraints transactionally. Ordinary role permissions apply, including own participation, with audit. Only confirmed registrations receive unpredictable QR tokens and occupy seats.

## US-17 — Cancel my registration

**As a participant, I want to withdraw before the event starts.**

Acceptance criteria:
- Allow owner cancellation of pending/confirmed/waitlisted rows while published, before start and without valid attendance, including after signup deadline.
- Warn cancellation is final for this release. Save reason/time/actor, invalidate token and release the seat; no reapplication and no automatic promotion.

## US-18 — Promote the next waitlisted participant

**As a officer/admin, I want to fill an available seat fairly.**

Acceptance criteria:
- Explicit action selects earliest currently eligible reviewed waitlisted row ordered by reviewed_at then registration_id across the full queue.
- Require published event, time before start, free capacity and eligible participant. Ordinary role permissions apply without a self-promotion prohibition. Recheck transactionally; keep original review, set confirmation actor/time and token, and audit officer action.
- No eligible candidate/free seat means no promotion. Member sees confirmed status and QR on next visit; no outgoing notification.

## US-19 — Enforce processing cutoffs

**As a organizer, I want to prevent late participation changes.**

Acceptance criteria:
- New signup stops at deadline; reviews, confirmations, officer promotions and member cancellations stop at exact event start, enforced on every server action.
- Retain unresolved pending/waitlisted statuses; show Not confirmed before cutoff and disable actions. No automatic status rewriting or scheduled job.
- Unconfirmed participants cannot check in and are excluded from no-shows; check-in opening is independent.

## US-20 — Access my event QR

**As a confirmed participant, I want to present my attendance credential.**

Acceptance criteria:
- Authorize owner and generate event-registration QR PNG from unpredictable token on demand; permit display, screenshot and PNG download without retained QR files.
- Saved copies work without member internet but officer validation is online. Cancelled/ineligible/unconfirmed rows have no usable QR; displaying QR never records attendance.

## US-21 — Control and record check-in

**As a officer/admin, I want to record arrivals accurately.**

Acceptance criteria:
- Select published event and manually open/close/reopen check-in, including early opening. Completed/cancelled events reject check-in.
- QR scan automatically records after selected-event, confirmed status, active account/member and duplicate checks; show name/student number for identity verification.
- Manual name/student-number search requires explicit Check in and the same validation. Record actor/time/method atomically with audit, prevent concurrent duplicates, and show original time on repeated valid scan.

## US-22 — Correct attendance

**As a officer/admin, I want to exclude mistaken check-ins without losing history.**

Acceptance criteria:
- Invalidate with confirmation, reason, actor/time, preserve original attendance and audit. No duration-based invalidation.
- Exclude invalidated rows from reports and eligibility; set affected unissued eligibility to not eligible with correction reason. Issuance always rechecks attendance.
- Block invalidation if certificate issued. Re-check-in after invalidation is deferred and warned before correction.

## US-23 — View my attendance

**As a member, I want to see my participation record.**

Acceptance criteria:
- Show authorized personal event/check-in details, including corrected records clearly labelled.
- Members cannot mark attendance or invalidate it; preserve history after profile and eligibility changes.

## US-24 — Review certificate eligibility

**As a officer/admin, I want to approve certificates for valid completed participation.**

Acceptance criteria:
- Create pending-approval rows for valid attendees of completed events only; approve eligible or decline not eligible with a reason and reviewer/time.
- Recheck attendance/completion and current role; approval of own participation follows the same rules and audit. Warn before decline that declined decisions cannot be reopened in this release; never change issued decisions.
- No-shows receive no certificate records; no duration requirement.

## US-25 — Issue and download PDF certificates

**As a authorized manager or member, I want to retain proof of completed participation.**

Acceptance criteria:
- Officer/admin issues one eligible certificate at a time with one fixed landscape A4 template, embedded fonts, unique number, recipient/event snapshots and issuer/time; ordinary role permissions also apply to own certificates with audit.
- Retain private PDF at certificates.file_path; prevent duplicate concurrent issuance and recheck valid attendance/completion/approval at final commit. Clean up/reconcile files if commit fails.
- Mark issued only after file and metadata succeed. Active account owner or authorized management can view/download via backend application/pdf stream; membership inactivity alone does not revoke historical certificates.
- Later edits do not rewrite issued PDFs. No email, public static PDFs, bulk issuance, template editor, revocation or correction/reissue.

## US-26 — View participation reports

**As a officer/admin, I want to evaluate turnout.**

Acceptance criteria:
- Show registration statuses, valid attendees, completed-event no-shows, attendance rate and course/year distributions using the shared formulas and signup snapshots.
- Show actual-attendee course/year distribution only, plus participant list, confirmed count, attendee count, rate and no-shows; no confirmed-participant distribution. Pending/waitlisted history is not expected attendance. Cancelled events are shown separately.
- Full-dataset summaries are independent of pagination. No CSV/PDF report export.

## US-27 — Retain historical records

**As a authorized manager, I want to preserve reliable participation history.**

Acceptance criteria:
- Use inactive master/account states, event lifecycle and registration cancellation; no general archive/restore or business-record hard deletes.
- Preserve historical references, unique identities, snapshots, attendance and issued PDFs. Reactivation does not recreate cancelled participation.

## US-28 — Review activity history

**As a administrator, I want to trace important decisions.**

Acceptance criteria:
- Use one read-only list showing actor, action, affected record, timestamp and reason/short description newest first. Retain relevant old/new values in storage, but defer advanced filtering and a field-by-field comparison UI. No log edit/delete controls.
- Log mutations consistently with their database transactions; omit passwords, hashes, QR tokens and physical filesystem paths.

## US-29 — Browse paginated lists

**As a authorized user, I want to find records efficiently.**

Acceptance criteria:
- Use fixed 25-row server-side pages with Previous/Next and necessary search/filters for requests/accounts, masters/events, registrations/search, attendance, certificates, reports and logs.
- Filter authorization/ownership before counts and slicing; use stable sort plus ID tie-breaker. Preserve query state, reset after filter changes, normalize invalid/out-of-range pages and show empty state.
- Counts cannot disclose unauthorized data; report totals and promotion selection use full applicable datasets, never just visible page rows.

## Shared rules and agreed refinements

- One organization; ASP.NET Core MVC, SQL Server, EF Core database-first; UTC `datetime2(3)` storage and explicitly labelled organization-local date/time display.
- UC explicitly lists registration statuses; draft/published/cancelled/completed event states are agreed implementation refinements. Reaching a deadline does not change a published event's status.
- Publishing opens signup. Signup is allowed only while published and `now < registration_deadline`; publication must occur before the deadline. Require deadline no later than event start.
- The deadline stops new signups only. Existing pending reviews and officer-triggered waitlist promotion remain possible until `now < starts_at`. Member cancellation is allowed before start without valid attendance, even after the signup deadline. All these operations also require a published event and current authorization.
- At event start, stop reviews, confirmations, promotions and member cancellations. Pending/waitlisted rows retain their status as non-actionable history with a visible “Not confirmed before cutoff” explanation; they cannot check in and are not no-shows. No scheduled closeout or mass status update is required.
- Waitlist order is ascending `reviewed_at`, then `registration_id`, among currently eligible reviewed waitlisted requests. Existing eligible waitlisted requests take priority over newly approved requests. Promotion is an explicit officer/admin action, never triggered automatically by a cancellation or deactivation.
- Only confirmed registrations occupy seats. Recheck capacity, cutoffs and eligibility transactionally under concurrent reviews/promotions. One registration per member/event; cancellation or decline is final for this release, with no reapplication.
- Draft fields are editable. After publication, only title, description and cover are editable; schedule, venue, type, limit and deadline are locked. Completed/cancelled events are read-only. Covers remain mandatory; replace rather than remove a published cover.
- Manual check-in is independent of signup deadlines and may open early; opening/closing again is allowed while published. Completion closes check-in. No attendance-duration tracking or offline officer synchronization.
- Before an event starts, loss of account/member eligibility cancels future pending/waitlisted registrations and confirmed registrations without valid attendance, preserving reasons and audit history. Release seats without automatic promotion. For started/completed events preserve registration/attendance history; current ineligibility still blocks new check-in. Reactivation does not restore cancelled registrations.
- Ordinary event cancellation requires no valid attendance or issued certificate and an event not already completed/cancelled. Require a reason, cancel active registrations, invalidate QR credentials and close check-in atomically; never promote during cancellation. Completion is an explicit officer action at/after scheduled start when the event has concluded.
- One attendance row per registration. Invalidation preserves the original row and logs, removes its contribution to reports and blocks unissued eligibility/issuance. Re-check-in after invalidation is deferred; show that limitation before correction. Issued certificates block ordinary attendance invalidation.
- Certificate eligibility requires valid attendance, event completion and officer/admin approval. Issue one retained landscape A4 PDF per registration using one fixed template; preserve number, recipient/event snapshots, issuer and timestamp. No template editor, bulk issuance, email, revocation or correction/reissue.
- User profile pictures are not included. Venue photos are mandatory at creation. Event covers are mandatory before publication, optional while drafting. One current image per venue/event; JPEG/PNG, maximum 2 MiB, with content validation and generated filenames. Officers/admins manage these images within lifecycle rules. Required images cannot be removed without replacement.
- Images and private PDFs use a configurable persistent server-local root outside the web/deployment directory, not Supabase or browser localStorage. SQL stores relative paths only. Validate path containment, permission and file content; retain old images until replacement commits. Files must survive redeployment and be backed up with SQL Server. Certificates are served through authorized backend streams, never public static files. File and SQL writes are not a shared transaction: clean up/reconcile orphaned files on database failure and never mark issuance successful after file failure.
- Reports use academic signup snapshots. Completed-event no-show = confirmed registration without valid attendance. Attendance rate = confirmed registrations with valid attendance / confirmed registrations × 100; zero denominator = N/A. Pending/waitlisted/cancelled registrations are excluded. Show actual-attendee course/year distribution only; confirmed counts and participant lists remain. Full-dataset totals never depend on a displayed page.
- No general archive/restore or hard deletes. Use active/inactive master records, account deactivation, event lifecycle states and registration cancellation. Preserve identifiers, references and historical rows.
- Pagination is included in necessary functional list tasks and their estimates, with no separate SHR module/task or pagination estimate. Use fixed 25-row server-side pagination with Previous/Next, necessary search/filters, authorized counts, stable ordering with ID tie-breaker, retained query state and empty/out-of-range handling.
- Enforce password hashing, current account/role checks, ownership, antiforgery protection for mutations, unique constraints and append-only audit writes without credentials/tokens. Protect the last active admin. Management roles do not bypass participation eligibility/capacity; ordinary role permissions allow reviewing/promoting own participation or approving/issuing own certificate, with the same validation and audit requirements.
- Applicant outcome communication remains an explicit open decision: submission acknowledgment and officer-managed offline communication for the demo, with no claimed public lookup, tracking-token service or outgoing email. This does not block retrying login after an externally communicated approval.

## Deferred

Known-password changes (US-04); reopening declined certificate decisions; confirmed-participant course/year distribution; advanced log filtering and field-comparison UI; general archive/restore; automatic waitlist promotion and scheduled registration closeout; event reapplication; separate manual registration opening; operational edits after publication; self-service profile-detail editing; user profile-picture uploads; outgoing email/Brevo; email verification; forgotten-password/reset tokens; self-service email changes; dedicated session/device tables and individual token revocation; custom permission editors; CSV/PDF report exports; image galleries/cropping; certificate template editors/bulk issuance/revocation/reissue; in-system QR printing; attendance duration and re-check-in after invalidation.

## Screen consolidation and delivery ownership

UC traceability retains its five categories: Authentication, Master Data, Transactions, Reports and Administration. For task assignment, the WBS instead uses six familiar functional headings and five owner packages: Member 1 (Gabutan) owns Authentication; Member 2 (Taguiam) Master Data; Member 3 (Singco) Events and Registration; Member 4 (Talisic) Attendance and QR Codes; Member 5 (Pinca) Certificates, Reports and Administration. Member 1 handles authentication/session and shared access-check helpers; Pinca owns administrator setup, role/access management, account deactivation and activity-log administration. These are delivery groupings, not new project scope. Each slice includes database/query work, business logic, UI, authorization/validation, audit integration and tests.

Member 1 leads authentication/session and shared access-check helpers; each owner enforces checks and writes audit entries in their own slices. Pinca owns administrator setup, administration screens, role/access changes, account deactivation and activity-log administration. Member 2 leads shared image storage/upload and pagination components; other owners integrate them. Member 3 owns seat allocation, token creation on confirmation and registration changes invoked by Pinca's account deactivation or Member 2 membership inactivity. Member 4 owns QR rendering/scanning and attendance correction, coordinating certificate consequences with Member 5. Member 5 owns PDF generation/private downloads using shared storage conventions.

My Events combines personal registration status, permitted cancellation, QR, attendance and issued-certificate download in one list/detail journey (US-15, US-17, US-20, US-23 and member side of US-25). Member 3 owns its shell; Members 4/5 contribute their sections. Event-attendee management combines attendance, correction, eligibility and issuance (US-21/22/24 and management side of US-25); Member 4 owns the attendance shell, Member 5 contributes eligibility/issuance. Stories are acceptance slices, not separate screen commitments. Use a basic activity-log viewer.

Agree schema/service contracts together before parallel implementation; use reviewed database scripts/scaffolding updates rather than competing schema edits. Integrate small slices continuously. Member 3 has the highest workflow risk; estimates must verify workload balance, not assume equal effort from package counts. Every owner tests their slice; all five run the end-to-end journey and deployment checks. Shared work is estimated once plus integrations.

US-04 is reserved and deferred; remaining story IDs are unchanged. No additional table or schema removal is needed for deferring password changes; password_hash remains essential for secure login.

## Deployment and acceptance

Create the first active administrator using supplied credentials and a password hash, without hard-coded defaults. Seed fixed lookup values. Include persistent-disk permissions/backups, database constraints, integration, security checks and deployment testing. Estimates remain TBD, not inferred from task counts.

Demo: request → approve → login → create venue with photo → draft event with cover → publish → signup → confirm → QR check-in → complete → approve eligibility → issue/download PDF → reports/logs. Demonstrate cancellation and explicit waitlist promotion separately, plus deadline/start boundary rejection.
