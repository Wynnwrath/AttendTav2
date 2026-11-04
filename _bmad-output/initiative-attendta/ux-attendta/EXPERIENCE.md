---
name: AttendTa
status: final
updated: 2026-11-04
sources:
  - ../prd-attendta/prd-attendta.md
  - imports/README.md
---

# AttendTa experience

## Foundation

Responsive web for one student organization, with applicant, member, officer and administrator journeys. Chrome is the target; officers use phone cameras over a hotspot and desktop/laptop screens for management. Current screenshots show a static localStorage prototype, not server-side behavior. [DESIGN.md](DESIGN.md) owns the visual system; this document describes the intended behavior from the [PRD](../prd-attendta/prd-attendta.md). The PRD and system specification take priority when prototype behavior differs. Screenshots do not prove camera operation or accessibility conformance.

## Information Architecture

| Surface | Entry and role | Purpose / reference |
|---|---|---|
| Public landing, request, login | Applicant | Request membership or sign in; [registration](imports/current-screens/current-registration.png), [login](imports/current-screens/current-login.png). |
| Account requests | Officer/admin console | Review pending requests, approve or reject with reason; [older queue reference](imports/reference-screens/SS-33_officer_account_requests.png). |
| Events and event detail | Member header | Browse published events, see deadline/timezone, submit a pending registration; [member events](imports/current-screens/current-member-events.png). |
| My Events / passes / history | Member navigation | Connected registration, cancellation, QR, attendance and issued-certificate journey; may be separate prototype pages joined by contextual links. [Passes](imports/current-screens/current-member-passes.png). |
| Profile | Member; officer/admin management | Member views own details; authorized management maintains permitted member details. No self-service profile editor. |
| Master data | Officer/admin console | Member directory, event types and venues/photos; [members](imports/reference-screens/SS-34_members_directory.png), [event types](imports/reference-screens/SS-35_event_types_management.png), [venues](imports/reference-screens/SS-31_campus_venues_master_directory.png). |
| Events workspace | Officer/admin console | Draft, publish, review registrations, promote waitlist, cancel or complete; [event list](imports/current-screens/current-officer-events.png), [creation](imports/reference-screens/SS-05_event_creation_workstation.png). |
| Check-in | Officer/admin event context | Open/close check-in, scan, manual fallback, inspect and invalidate attendance; [scanner](imports/current-screens/current-officer-scanner.png), [mobile](imports/current-screens/current-mobile-scanner.png). |
| Certificates | Officer/admin; owner download | Review eligibility, issue one retained PDF at a time; [ledger](imports/reference-screens/SS-21_certificates_eligibility_ledger.png), [certificate](imports/reference-screens/SS-24_institutional_certificate_document.png). |
| Participation reports | Officer/admin console | Per-event totals, participant details and actual-attendee distributions; [report](imports/current-screens/current-officer-report.png). |
| Roles and access | Admin console | Existing account role/access changes, last-admin protection; [admin](imports/current-screens/current-admin-users.png). Initial admin is a setup operation. |
| Activity history | Admin console | Read-only, newest-first activity log; [reference](imports/reference-screens/SS-36_activity_history_audit_trail.png). |

**Reference coverage:** current styled captures cover login, registration, member events/passes, officer event list, scanner, reports and admin, including selected 390px views. Older screenshots cover public landing, account requests, master data, event creation, certificate and activity history; their copy and actions can be stale. Profile details, most error/empty states, promotion, registration review, account deactivation and certificate decision interactions are described by these spines and the PRD rather than verified by a current capture. The user selected existing screenshots instead of additional HTML mockups.

## Voice and Tone

Use brief, direct labels and outcomes. “Request membership,” “Registration pending,” “Check-in closed,” and “No participants found” tell people what happened. Show the next permitted step and a reason when relevant. Do not promise email, automatic promotion or successful issuance before the server confirms it. Label time as Asia/Manila.

## Component Patterns

| Component | Behavioral rule |
|---|---|
| Primary button | One clear primary action for the context; prevent repeat submission while a request is pending and restore it on failure. |
| Surface card | Present event details and status together; action leads to the relevant event or personal record. |
| Input | Persistent visible label and associated error; preserve entered values on validation failure except sensitive values where appropriate. |
| Status badge | Label registration and attendance separately; color reinforces the text. |
| Navigation | Show role-appropriate links; server authorization controls destinations. Preserve event/participant context across related screens. |
| Ledger | Filter before paging, show fixed 25-row pages, Previous/Next and full-dataset totals. Do not derive waitlist priority or report totals from only the visible page. |
| Tabs and filters | Expose current selection programmatically; keyboard users can reach and change each filter, and changed results are announced. |
| Data table | Use associated column and row headers, preserve reading order, and allow keyboard-accessible horizontal scrolling only where its two-dimensional layout requires it. |
| Dialog | Confirm consequential actions with effects and required reason; focus enters the dialog, Escape/cancel closes, and focus returns to its trigger. |
| Pass | Only a confirmed registration gets an owner-authorized QR PNG; display/download alone never records attendance. |
| Scanner window | Camera scan records automatically after server validation. Manual name/student-number lookup requires selection and an explicit Check in action. Show name and student number on success. |
| Certificate | Show eligibility/issuance separately; one issuance at a time. Download the retained, authorized PDF; report an unavailable file. |

## State Patterns

| Context | Required states and response |
|---|---|
| Requests/login | Blank, validation errors, duplicate identity, submitted acknowledgment, pending/rejected and generic invalid/inactive login failure. Outcomes are communicated offline. |
| Event feed/detail | Empty, draft hidden, published/open, signup deadline passed, full with no-guaranteed-seat warning, cancelled/completed. Registering creates Pending even if full. |
| Personal registration | Pending, Confirmed, Waitlisted, Cancelled and Not confirmed before cutoff; confirmation and attendance are distinct. Cancellation is final for this MVP. |
| Officer review/queue | Pending review, confirmed, waitlisted after review, decline with reason, no seat/no eligible candidate; explicit promotion ordered by review time then ID. |
| Scanner | Closed/open, camera permission denied/unavailable, wrong event, unknown/invalid token, inactive participant, success, duplicate with original time, manual lookup empty and explicit confirmation. Invalidation requires reason; no re-check-in after invalidation. |
| Certificates/reports | Not yet completed, pending review, eligible, not eligible, issued, failed generation/download, empty report, zero denominator shown N/A, no-shows only after completion. |
| Access/admin | Unauthorized/sign-in required, affected-registration preview, reason, last-active-admin restriction, success and failure. |
| Master data and activity history | For an empty directory or log, explain that there are no records; show an authorized create action only where the workflow permits it, and distinguish no matches after filtering. |
| Event form | Show field-specific errors and retain valid entered values; identify a missing, oversized or invalid cover and allow correction before publication. |
| Network/requests | Loading, failure and retry; never claim a server-side write succeeded or was queued offline without a connection. |

## Interaction Primitives

Tabs and filters expose their selected state and work with keyboard. Tables scroll horizontally when necessary. Server-side authorization, validation and time checks run again on submission, including stale forms. Publishing opens signup; the signup deadline closes new submissions, while review, promotion and allowed cancellation remain possible until event start. Events may overlap. Attendance correction and certificate issuance must respect current server state.

## Accessibility Floor

Use semantic landmarks and headings, labels tied to inputs, descriptive error text and programmatically exposed status messages for scan results and asynchronous success, failure and progress, including registration, review and certificate actions. All controls must work with keyboard and visible focus; dialogs trap and restore focus. Use sufficient text contrast and visible status text, respecting restrictions on {colors.ink-faint} and {colors.amber} in DESIGN.md. Honor reduced motion. Test at 200% zoom and 320 CSS pixels width: reflow ordinary content without loss of functionality; genuinely two-dimensional data tables may scroll horizontally with keyboard access. For any report graphic, provide an equivalent summary and structured values table, with labels that do not rely on color alone. Include camera denial and manual fallback. The current officer/admin rail disappears at small widths without an equivalent menu; supply and verify reachable navigation before claiming mobile support. These are implementation requirements, not a claim that the prototype has passed accessibility testing.

## Responsive & Platform

At 1440px use the captured member header and desktop console rail. At 390px use the [member](imports/current-screens/current-mobile-member-events.png), [login](imports/current-screens/current-mobile-login.png) and [scanner](imports/current-screens/current-mobile-scanner.png) references. Scanner controls must remain reachable and readable; the officer must be able to choose an event and use fallback without a camera. The hotspot phone uses the host's network address; trusted HTTPS and camera permission must be checked on actual devices. A saved QR may be presented offline, but the scanner needs network access to validate and record it.

## Prototype differences to resolve during implementation

Use screenshots for layout only: older captures contain student-number login, outdated institution text and sample data. The prototype may offer direct waitlist signup, change-password, new-user creation, bulk issuance, QR printing, a pass-code tab and archived status; these do not add MVP requirements. The agreed flow starts signup as Pending, waits for officer review, provides name/student-number manual check-in, and issues one retained PDF at a time. Event lifecycle and attendance states follow the PRD, not prototype seed labels. The [reference index](imports/README.md) identifies the visual assets and older captures.

## Key Flows

### UJ-1. Ana requests membership and gets in

1. Ana opens Request membership and supplies identity, academic details, email and password.
2. She sees an acknowledgment; an officer reviews the request and communicates the outcome offline.
3. After approval she signs in using email and password.
4. **Climax:** Ana reaches the member event feed with her approved account.

Failure: show field errors or duplicate identity without claiming a request was created; rejected applicants may make a new request.

### UJ-2. Ben publishes a seminar and fills it fairly

1. Ben creates a draft with a valid venue, type, cover, capacity and deadline, then publishes it.
2. Ana registers; the request is Pending, even if the event is full.
3. Ben reviews requests and confirms or waitlists according to seat availability and queue priority.
4. After a cancellation releases a seat, Ben explicitly promotes the next eligible waitlisted member.
5. **Climax:** the next eligible person sees confirmation and their pass; no silent promotion occurs.

Failure: stale or competing reviews are rejected or returned with the current outcome; no overbooking or false success.

### UJ-3. Ana checks in at the door

1. Ana opens her confirmed event pass and may save the PNG before arriving.
2. Ben selects the event and opens check-in on his connected phone.
3. He scans the QR. After server validation, the system records attendance automatically.
4. **Climax:** the result shows Ana's name and student number for Ben to verify.

Failure: a repeat shows the original time; wrong-event, inactive, unknown or closed-check-in scans do not record. If the camera fails, Ben searches by name or student number, selects the person and explicitly clicks Check in.

### UJ-4. Ben closes the event and issues certificates

1. Ben completes the concluded event at or after its start; check-in closes.
2. He reviews valid attendees' eligibility and approves or declines each decision.
3. He issues an eligible certificate individually and the system retains the PDF.
4. **Climax:** Ana downloads her PDF; Ben sees the event's attendance and no-show report.

Failure: a failed file write or changed eligibility does not produce a false issued state; a missing file shows a failure.

### UJ-5. Admin Dana removes a departed officer

1. Dana opens account access, selects the officer and previews affected upcoming registrations.
2. She supplies a reason and confirms deactivation.
3. **Climax:** the account is blocked on its next request, affected registrations follow the rules, and the action is visible in activity history.

Failure: the last active administrator cannot be deactivated or demoted; failure leaves the previous state visible.
