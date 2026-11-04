# Adversarial Review — AttendTa PRD v1 and Addendum

> **Historical snapshot (2026-11-04).** This review predates the latest owner-confirmed decisions and source-document alignment. Treat its findings as historical; the current `prd-attendta.md`, `addendum.md`, and current `docsss/` sources govern.

**Reviewer role:** adversarial general review (artifact, not code diff)
**Target:** `prd-attendta.md` (5,623 words) and `addendum.md` (890 words), as of 2026-11-04
**Reference set:** `docsss/AttendTa-System-Specification.md` (v2.1, authority per PRD §0), `UC-Project-FL.pdf`, `Alliance_Jumpstart_HLD_Group_7_AttendTa.pdf`, `WBS_J4S_ATTENDTA_20261103.pdf`, `Parent_Project_Plan_J4S_AttendTa_v2.00.pdf`, `git_qa_guidelines.pdf`, `AttendTa-MVP.dbml`
**Assumed reader:** five-person student team implementing in five vertical slices, then an Alliance reviewer grading against the source documents.

**Deliberate decisions excluded from findings:** SPEC-over-UC/HLD/WBS/Plan precedence; email-or-student-number login; certificates fully in MVP with retained PDFs; no institutional email domain; acceptance-based success metrics; 5–8 page target; missing `AttendTa-User-Stories.md` (already §10 Q3). None of these is reported as a gap.

---

## Verdict

**Revise before hand-off.** No P0. Five P1 defects: one operation (membership inactivity) specified twice with opposite scope across two slices; one unrecoverable data state created by a routine administrative action; one credential-reuse hole created by the login override; one missing transport/camera requirement that blocks the primary check-in method as written; and no login throttling over a guessable identifier space. Below that, thirteen P2s are mostly state transitions the PRD leaves to the implementer, plus a handful of false or untestable claims; eight P3s are wording and bookkeeping.

---

## P1 — High

### [P1] Membership inactivity is specified twice, with opposite scope, in two slices owned by two people

**PRD:** §4.2 FR-5 (line 148, Member 2 slice) and §4.8 FR-32 (lines 338–341, Member 1 slice).

**Quote:** FR-5 — *"Setting membership to inactive blocks new participation. It does not change Account access, and Account access changes do not change membership."* FR-32 — *"Management can change membership status. An Administrator can change Account active state. … For Events not yet started, deactivation cancels the person's pending, waitlisted and confirmed Registrations that have no Valid Attendance, invalidates their QR Passes and releases their Seats."*

**Problem:** FR-5 reads as prospective-only ("blocks new participation" — the HLD p.16 phrasing). FR-32 makes the same membership change retroactive: it cancels confirmed registrations and releases seats. Both are consequences for the *same* operation, and the addendum (§C) assigns them to different owners (Member 2 for MD-02, Member 1 for ADM-01). SPEC §5 and the SPEC §1.4 integration note ("Member 1 deactivation and Member 2 membership inactivity invoke Member 3 registration cancellation logic rather than duplicating seat rules") require one shared path; the PRD never says so. Two slices will implement two behaviours, and the demo will show whichever one the officer used.

**Fix:** State the retroactive rule once, in FR-5, and make FR-32 reference it: both membership-inactive and account-inactive go through one cancellation routine, owned by the Registration slice. Add to FR-32: "membership-inactive and account-inactive share one cancellation service; neither slice reimplements seat release."

### [P1] A member made ineligible after the event has started becomes a permanent No-show with no correction path

**PRD:** §4.8 FR-32 (line 342) vs §3 Glossary *No-show* (line 91) and §4.5 FR-24/FR-25 (lines 274–282).

**Quote:** FR-32 — *"For Events that have started or completed, history is kept, but the person cannot be checked in."* Glossary — *"No-show — A Confirmed Registration on a Completed Event without Valid Attendance."*

**Problem:** Reachable in normal use: an officer sets a member's membership to inactive at 6pm (dues not paid, data error, graduating student), and that member is at the door at 7pm for an event that started at 6:30. FR-23/FR-24 reject check-in because membership is inactive. FR-25 cannot help because invalidation requires an existing Attendance Record. Completion then classifies the present member as a No-show, and FR-30 reports it. The same applies to account deactivation. Nothing in the PRD defines an override, a flag, or an exception to the No-show definition. This is the assignment's "membership inactive mid-event" case, and the PRD currently resolves it in the direction that silently corrupts the participation record.

**Note on authority:** SPEC §5 carries the same rule ("For events already started or completed, preserve registration/attendance history. Current ineligibility still blocks new check-in"), so this is inherited, not introduced. That does not make it harmless: the PRD's job is to make state transitions executable, and a five-person team will meet this state in the demo if any member's membership is changed on event day. If the rule is kept, record it as an explicit decision in §9 with its No-show consequence, so the team does not "fix" it differently in two slices.

**Fix:** Pick one and say it: (a) check-in is permitted for a Confirmed registration on an already-started event and the eligibility problem is recorded as a flagged exception rather than a hard reject; or (b) administrative ineligibility after start is blocked until the event completes; or (c) the No-show definition excludes registrations made ineligible by an administrative action, with the count shown separately. Whichever is chosen, FR-32 and the glossary must match.

### [P1] Student-number login turns a correctable profile field into a reusable credential

**PRD:** §4.2 FR-5 (lines 143–146), §4.1 FR-1 (line 108), §4.1 FR-3 (lines 126–131), §5 NFR-3 (line 401).

**Quote:** FR-5 — *"A student-number correction requires a reason, enforces uniqueness, and logs the old and new values."* FR-3 — *"A student number resolves to the Account linked through its Member Profile."*

**Problem:** With student-number login (the recorded override), the student number is a login credential. FR-5 lets Management change it and enforces uniqueness only against current values. NFR-3 reserves identifiers only for *inactive identities*. Consequences, all reachable: (1) once member A is corrected from `2023-01234` to `2024-05678`, the old value is free, and FR-1's duplicate check no longer matches anything, so a new applicant can claim `2023-01234` and thereafter authenticate as that number; (2) member A's login identifier changes silently — she types her old number and gets the generic failure FR-3 mandates, with no recovery path (see the lost-password finding); (3) the recycled number is then searchable at the door: FR-24 looks a participant up by student number and shows name and number for identity checking, so the number that once identified A now identifies B, and the officer's confirmation is against the wrong person; (4) the Activity Log records old/new values for the correction, but nothing on the Account warns the holder. This is a credential change performed by a third party with no user confirmation, in a system whose stated value is attributable, trustworthy records.

**Fix:** Either reserve corrected student numbers permanently (uniqueness check includes historical values in `account_requests` and Activity Log old values, plus an explicit reservation rule in NFR-3), or state that correction changes the member's login identifier and requires an explicit warning on the correction screen. Do not leave both readings open.

### [P1] Neither document requires HTTPS, and the phone-camera scanner cannot work without it

**PRD:** §5 NFR-6 (line 404), NFR-1 (line 399); Addendum §A (lines 19–25), §E (line 99).

**Quote:** NFR-6 — *"Web application, accessed over the intranet. Chrome is supported; layouts are mobile-responsive so check-in works on an officer's phone."* Addendum A — *"Environments: Dev on localhost; staging and production not defined; intranet access; one application server assumed."*

**Problem:** The string "HTTPS" (or TLS/SSL) does not appear anywhere in the PRD or the addendum — verified by search. Two consequences. First, `getUserMedia()` — the only way a Chrome page opens a phone camera for QR scanning — is restricted to secure contexts; on a plain-HTTP intranet origin `navigator.mediaDevices` is `undefined` and the scan button throws (MDN, *MediaDevices: getUserMedia()*, https://developer.mozilla.org/en-US/docs/Web/API/MediaDevices/getUserMedia). A self-signed certificate on an intranet host will also be distrusted by phones, so "just enable HTTPS" is not a one-line fix for a five-person team. The fallback (manual name/student-number lookup) works, so UJ-3 survives, but the PRD's primary check-in method as written is undeliverable on the stated platform. Second, NFR-1 requires hashed passwords and signed cookies but not transport encryption, so passwords and session cookies cross the intranet in clear text — a gap a security-minded grader will find immediately.

**Fix:** Add to NFR-1: "All traffic over TLS; HTTP redirects to HTTPS; cookies marked Secure, HttpOnly, SameSite=Lax." Add to NFR-6: either the secure-context requirement and how the certificate is obtained/trusted on officer phones, or an explicit statement that QR scanning is supported on a laptop with a USB/keyboard-wedge scanner and that camera capture on a phone is not a supported path for v1. Resolve §10 Q4 (deployment target) and this together.

### [P1] No login throttling or lockout anywhere, over a guessable identifier space

**PRD:** §5 NFR-1 (line 399), §4.1 FR-3 (lines 128–131).

**Quote:** NFR-1 — *"Passwords stored only as salted hashes. Signed, expiring cookie sessions. Antiforgery on every mutation. Ownership checks on every personal resource. Server-side enforcement of every time, capacity and guard. Never log secrets, hashes or QR tokens."*

**Problem:** Nothing in FR-1, FR-3 or NFR-1 limits authentication attempts. Student numbers are predictable (`2023-01234` format, visible in UJ-3's own example and on class lists), and password strength is unconstrained (see next finding), so online guessing against a known student-number space is the cheapest attack on the system. FR-3's generic failure message correctly avoids confirming which identifier exists, and the addendum (§B6) correctly notes the student-number lookup "must not reveal whether a student number exists" — but that property is defeated by unlimited retries: a generic message plus unlimited attempts is a working oracle at ~100 attempts. The PRD promises a security NFR but names no rate control, so no slice will build one and no acceptance scenario tests one.

**Fix:** Add a measurable requirement, e.g. "Failed logins are counted per identifier and per source IP; after 5 failures in 15 minutes the account is throttled for 15 minutes with the same generic message; throttling events are logged without the attempted password." Add to SM-1's scenario list.

---

## P2 — Medium

### [P2] "No-show figures final" is false: invalidation after completion changes them

**PRD:** §4.3 FR-13 (line 195) vs §4.5 FR-25 (lines 277–280), §3 Glossary *No-show* (line 91).

**Quote:** FR-13 — *"Completion closes Check-in and creates a Certificate Review for every Registration with Valid Attendance. It makes the No-show figures final."* FR-25 — *"The invalidated record is excluded from reports and eligibility."*

**Problem:** FR-25 lets Management invalidate an Attendance Record on a Completed Event (it is blocked only once a Certificate has been issued). Invalidating a valid attendance record converts that confirmed registration into a No-show, so the report changes after completion. "Final" is therefore false, and a team that takes it literally will either block post-completion invalidation (contradicting FR-25) or produce a report that contradicts FR-13's stated invariant. The same sentence also conflicts with the vision's "no silent rewrites of history" only in appearance — the invalidation is audited — but the *word* "final" is what a reviewer will test.

**Fix:** Replace with "No-show figures are computed from current Valid Attendance and may change only through an audited attendance invalidation (FR-25), which is blocked once a Certificate is issued."

### [P2] The rejected-applicant journey depends on a reason the applicant is structurally unable to see

**PRD:** §2.3 UJ-1 (line 54), §4.1 FR-1 (line 109), FR-2 (line 118).

**Quote:** UJ-1 — *"if Ben rejects the request with a reason, Ana submits a new, corrected request."* FR-1 — *"There is no status page and no tracking token, and requests are not retrievable by ID, email or student number."* FR-2 — *"Rejection requires a reason."*

**Problem:** The reason is captured, audited and never delivered (no email, no status page, no lookup). Ana can only produce a "corrected" request if Ben tells her the reason in person — which the PRD never states as the mechanism, and which makes the formal requirement decorative. A grader following UJ-1 will ask "how does Ana know what to correct?" and the document has no answer. The same question applies to approval: UJ-1 says "an officer tells her offline", but that is journey prose, not a consequence.

**Fix:** Make the communication channel explicit in FR-2: "The rejection reason is shown to Management only and is communicated to the applicant offline; the system makes no delivery guarantee." Then adjust UJ-1 to "Ben rejects the request with a reason and tells Ana offline; she submits a new, corrected request."

### [P2] Certificate Review and Certificate are one row in the data model but two entities in the glossary and two actions in the FRs

**PRD:** §3 Glossary *Certificate Review* (line 92) and *Certificate* (line 93), §4.6 FR-28 (line 298), Addendum §B (lines 29–41), DBML `certificates` (`registration_id` unique, status includes `issued`).

**Quote:** Glossary — *"Certificate Review — … States: `pending_approval → eligible | not_eligible`."* and *"Certificate — The retained fixed-template PDF issued from an `eligible` Certificate Review."* DBML — `certificates` has one row per registration, `registration_id [unique]`, with `status` including `issued`.

**Problem:** The glossary's state machine has no `issued` state on the review and describes the Certificate as a separate object; the schema and FR-28 treat them as one row whose status becomes `issued`. A developer reading the glossary will create a second record (or a second table) and hit the unique constraint in `certificates.registration_id`. FR-25's "Any unissued Certificate Review becomes `not_eligible`" then has no defined target for an already-`eligible` row under the glossary reading, since `eligible` is a terminal review state in the glossary.

**Fix:** One sentence in §4.6: "A Certificate Review and its Certificate share one record: `pending_approval → eligible → issued`, or `pending_approval → not_eligible`; an `eligible` record that is invalidated before issuance moves to `not_eligible`." Update the glossary to the same five-state list.

### [P2] Management has no way to cancel a confirmed or waitlisted Registration, though the state model and SPEC require it

**PRD:** §4.4 FR-16 (lines 214–219), FR-18 (line 229), §3 Glossary *Registration* (line 82); SPEC §9.1 state table (line 190).

**Quote:** FR-16 — *"Management can approve or decline Pending Registrations…"*. Glossary *Registration* — states include `cancelled`. SPEC §9.1 — Cancelled means *"Withdrawn, declined, event-cancelled or administratively cancelled with reason."*

**Problem:** The only Management-driven cancellations in the PRD are declining a *pending* row, cancelling the whole event (FR-12), and deactivating the person (FR-32). A confirmed member who is discovered to be ineligible (wrong section, duplicate identity, withdrawn student) can be removed only by deactivating their membership — which also destroys their registrations for every other event. SPEC recognises "administratively cancelled with reason" as a distinct cause; the PRD claims SPEC authority and 26/26 coverage but has no FR for it. The team will either invent it in the FR-32 screen or omit it, and the omission is a real operational dead end.

**Fix:** Either add the action to FR-16 ("Management may cancel a confirmed or waitlisted Registration with a reason while the event is Published and before the start cutoff; no Promotion follows; the Seat is released") or record it explicitly in §9 as an intentional narrowing, with the consequence stated.

### [P2] Deactivating an Event Type or Venue strands drafts, and the PRD never says whether that is allowed

**PRD:** §4.2 FR-6 (line 153), FR-7 (line 159), §4.3 FR-9 (line 172), FR-10 (line 176).

**Quote:** FR-6 — *"Inactive types cannot be selected for new or draft Events. Existing references and names are preserved."* FR-10 — *"Publication requires an active Event Type and Venue (revalidated)…"*

**Problem:** Nothing defines the remedy. Two readings are equally supported: (a) deactivation is allowed and any draft referencing the type/venue becomes editable only if the officer switches to another active type (but FR-9 requires an *active* type on every draft edit, so even changing the title fails validation); or (b) deactivation should be blocked while drafts reference it (the pattern FR-7 uses for capacity). Published events keep working, so the failure surfaces only at publication time, in front of the client. The team will guess, and Member 2 (master data) and Member 3 (events) will guess differently.

**Fix:** State the rule: "Deactivation is permitted regardless of references. Published and completed Events keep their reference and remain valid. Drafts referencing an inactive type or venue cannot be published or saved until another active master is selected; the validation error names the inactive master." Also state whether capacity may drop below the limit of a *completed* event's venue (FR-7 covers draft/published only).

### [P2] Started-but-never-completed events stay in the member feed forever, with no visibility rule

**PRD:** §4.4 FR-14 (line 204), §4.3 FR-13 (line 196), §4.9 FR-36 (lines 378–390).

**Quote:** FR-13 — *"Time passing never completes an Event."* FR-14 — *"A Member … can view Published Events…"*

**Problem:** Because completion is manual and never enforced, an event whose date has passed remains `published` indefinitely, so it remains in the member feed and in "browse events" (SPEC §19 orders the feed by `starts_at` ascending, which puts the oldest stale event first). The PRD defines what an officer *can* do (complete it) but not what the member sees before they do, and not whether check-in may still be opened for it (FR-22 says "at any time"). A neglected event therefore sits at the top of the feed, accepts no signups (deadline passed) and shows a live "Check-in" control to officers. This is the direct cost of the (deliberate) no-auto-completion decision, and the PRD does not pay it.

**Fix:** Add to FR-14: "The feed lists Published Events with `starts_at` at or after now; a Published Event whose start has passed leaves the feed but stays visible in My Events to Members with a Registration, labelled *Started — not completed* until an officer completes it." Add to FR-22 whether check-in may open after the start time (it currently may, which is defensible, but must be stated).

### [P2] Cancelling or completing an Event leaves Certificate Reviews in an undefined state

**PRD:** §4.3 FR-12 (lines 187–191), §4.5 FR-25 (line 279), §4.6 FR-27/28 (lines 291–303), §4.7 FR-30 (line 324).

**Quote:** FR-12 — *"Management can cancel a Draft or Published Event that has no Valid Attendance and no issued Certificate."* FR-27 — *"…each pending Certificate Review."*

**Problem:** An Event can be cancelled while it has `eligible` (approved, unissued) Certificate Reviews — FR-12 only excludes *issued* certificates. After cancellation, FR-28's re-check of completion makes issuance fail, but no rule moves those reviews to `not_eligible`, and FR-27's queue still lists them on a cancelled event. Separately, FR-30 says only that "cancelled Events are shown separately" — it never defines what a cancelled-event report contains, even though FR-12 can leave Attendance Records (invalidated) and cancelled registrations behind. Member 5 will invent both behaviours.

**Fix:** Add to FR-12: "Cancellation moves every unissued Certificate Review on the Event to `not_eligible` with the cancellation reason and one audit entry per review." Add to FR-30: "A Cancelled Event's report shows historical counts only, with no attendance rate and no No-shows."

### [P2] Approval does not revalidate the identifiers it is about to make unique

**PRD:** §4.1 FR-2 (lines 116–121), §4.2 FR-5 (line 146).

**Quote:** FR-2 — *"Approval atomically does all of the following: creates an active Member-role Account and its linked Member Profile…"* FR-1 — duplicate rejection is specified at submission only.

**Problem:** Between submission and approval, an officer can edit another member's student number to the pending applicant's value, or a concurrent approval can consume the email. FR-1 checks duplicates at submission; FR-2 does not mention revalidation or what the reviewer sees when the atomic insert fails. The likely implementation is a raw unique-constraint exception surfacing as a 500, or (worse) a partially created Account with no Member Profile if the two inserts are not truly atomic.

**Fix:** Add to FR-2: "Approval revalidates email and student number inside the transaction; on conflict it rolls back, leaves the request pending, and shows the conflicting record to the reviewer." This also gives SM-1's "duplicate account decision" scenario something concrete to assert.

### [P2] Where images are served from is unspecified, and NFR-4 makes the obvious answer illegal

**PRD:** §4.2 description (line 137), §4.2 FR-8 (lines 161–165), §5 NFR-4 (line 402), §4.6 FR-29 (line 307).

**Quote:** §4.2 — *"Images are validated and stored privately on the server."* NFR-4 — *"Storage sits outside the web root and deployment directory, and only the application account can read it."* FR-29 covers only Certificates: *"…streamed through an authorized endpoint."*

**Problem:** Members must see Venue photos and Event Covers, but the PRD never specifies an image-serving endpoint, authorization rule, caching header or content-type policy for them. With storage outside the web root, static middleware cannot serve them; a developer who does not notice NFR-4 will either put images under `wwwroot` (violating the NFR) or expose an unauthenticated `/image/{id}` endpoint that serves any record's file, including ones for drafts (which FR-9 says are hidden from Members — a draft cover would leak the existence and content of an unpublished event). FR-8's 2 MiB limit also says nothing about pixel dimensions, so a small PNG can decode to gigabytes in the browser.

**Fix:** Add a short FR or extend FR-8: "Images are served only through an authorized endpoint that checks the viewer's role and the Event's visibility (draft covers are Management-only); responses set `Content-Type` from the validated type and `Cache-Control` appropriate to replaceable content. Uploads are capped at 2 MiB *and* at a maximum pixel dimension (e.g. 6000×6000)."

### [P2] Password strength is unconstrained, so the hashing requirement protects weak secrets

**PRD:** §4.1 FR-1 (lines 104–110), §5 NFR-1 (line 399).

**Quote:** FR-1 — *"Missing required fields, a year level ≤ 0, a malformed email, or a confirmation that doesn't match the password → the request is rejected with field errors."*

**Problem:** The only password rule is that the confirmation matches. There is no minimum length, no blocklist, no reuse rule for the resubmission path, and no requirement on the setup script's password (FR-35 accepts "a supplied … password"). Combined with unlimited login attempts and a public student-number space, "salted hash" protects `123456` perfectly and nothing else. This is also the kind of omission a reviewer tests in thirty seconds.

**Fix:** Add a measurable rule to FR-1 and FR-35: "Passwords are at least 12 characters and are not the student number, email or a value on a small blocklist; violations are field errors and are re-checked at approval." Keep it out of the addendum — it is a requirement, not a stack choice.

### [P2] The session cookie is "signed and expiring" with no expiry, no flags and no fixation rule, so the NFR cannot be tested

**PRD:** §4.1 FR-3 (line 129), §5 NFR-1 (line 399), §6 Non-Goals (line 411).

**Quote:** FR-3 — *"On success, a signed, expiring cookie session is issued and the user is routed by Role."*

**Problem:** "Expiring" without a duration is not a testable consequence; the team will pick 30 days or 30 minutes depending on who writes the startup file. Nothing requires `HttpOnly`, `SameSite`, `Secure` (see the TLS finding), or session-ID regeneration on login — without regeneration, a pre-set cookie survives authentication (session fixation). §6 rules out "session/device management", which a developer may read as "don't think about cookie attributes at all". FR-3's per-request re-check of role and active state mitigates stolen-cookie impact but does not address fixation or expiry.

**Fix:** "Sessions expire after 60 minutes of inactivity and 8 hours absolute; the cookie is `HttpOnly`, `SameSite=Lax`, `Secure`, and the session identifier is regenerated on login and logout." Put it in FR-3, not the addendum, because it is observable behaviour.

### [P2] Password loss is a permanent dead end that the PRD never names

**PRD:** §6 Non-Goals (line 411), §4.1 FR-1 (line 108), §4.2 FR-5 (line 147), §7.2 (lines 431–443).

**Quote:** §6 — *"Not an identity provider: no email verification, password reset or recovery, self-service email or password change, SSO, or session/device management."* FR-1 — the request is rejected if the email or student number *"belongs to any existing Account or Member Profile (including inactive ones)."*

**Problem:** A member who forgets their password cannot reset it (non-goal), cannot change it (no known-password change; deferred), cannot re-register (duplicate rule), and has no admin reset path (SPEC §4 explicitly excludes a substitute administrator reset workflow — and the PRD never repeats that exclusion, so an officer may reasonably expect "Maintain Member Profiles" to include a reset button). For the demo this is invisible; for the client it is a support ticket with no answer, and for a grader it is the first operational question after "what if two people have the same name?". The PRD's §7.2 table does not list lost-password recovery as deferred either, so it is neither in scope nor explicitly out.

**Fix:** Add a row to §7.2: "Password reset/recovery — excluded; a user who forgets the password is re-onboarded by the team operating the database (stated as a demo-only operational workaround)". Or state the sanctioned admin reset. Silence is the worst option because it implies a capability nobody built.

### [P2] The Certificate file path can be orphaned on disk, and the PRD's wording permits it

**PRD:** §4.6 FR-28 (line 302), §5 NFR-4 (line 402), Addendum §E (line 97).

**Quote:** FR-28 — *"A failed file write leaves the Certificate unissued. A failed database commit leaves no orphaned file referenced."*

**Problem:** "No orphaned file *referenced*" is satisfied by a database row that points to nothing — it actually describes the wrong invariant. What SPEC §14.3 requires is that a file written before a failed commit is removed or reconciled ("on failed database commit remove or reconcile an unreferenced file"). As written, a retry after a failed commit leaves an unreferenced PDF on disk forever. Because the certificate number and snapshots are prepared before the write, that orphan is a complete, signed-looking document bearing a number the system does not know about and may later assign elsewhere — storage waste plus an unaudited artifact in a system whose promise is that every certificate is attributable.

**Fix:** "A failed database commit removes or quarantines the file written for that attempt before reporting failure; no PDF bearing an issued certificate number exists outside the issued record." Add "orphan reconciliation" to SM-1's scenario list, since SPEC §20 already requires "missing-file handling".

---

## P3 — Low

### [P3] "Attendance Record" method taxonomy contradicts the manual fallback flow

**PRD:** §3 Glossary *Attendance Record* (line 89), §4.5 FR-24 (line 274).

**Quote:** Glossary — *"method (QR, name search, student number)"*. DBML — `check_in_method` = `qr_code | manual_search | student_number`.

**Problem:** FR-24 describes one flow: "look up … by name or student number, select them and click Check in." If the officer typed a student number, the method is `student_number`; if they typed a name, it is `manual_search`. But FR-24's flow is identical, and the PRD never says how the system decides which value to store — nor whether the officer can search by either field in the same box. Three plausible implementations exist. This is small, but it is exactly the kind of field that ends up inconsistent across two slices.

**Fix:** One clause in FR-24: "The method recorded is `qr_code`, `manual_search` (name lookup) or `student_number` (lookup keyed on the student number), according to which lookup field produced the selected participant."

### [P3] The QR Pass remains available for Completed and Cancelled events

**PRD:** §4.5 FR-21 (lines 247–251), §4.4 FR-20 (line 240), §4.5 FR-23 (line 259).

**Quote:** FR-21 — *"The owner of a Confirmed Registration can view its QR Pass and download it as a PNG."*

**Problem:** FR-21 has no Event-state guard, and FR-20 says My Events shows "a QR Pass when Confirmed". A Confirmed Registration stays confirmed after Completion (FR-23 says "Check-in does not change the Registration's status"), so a member who no-showed still sees a downloadable pass for an event that is over, and the scanner rejects it. The guard exists server-side in FR-23 but is missing from the display rule, so the team must guess whether to hide the button.

**Fix:** "A QR Pass is shown only while the Event is Published and Check-in is open; after completion the entry shows the Attendance Record or *No-show* instead."

### [P3] "Every list screen" paginates, including exact-lookup results — contradicting SPEC §19 and the door-scanner use case

**PRD:** §4.9 FR-36 (lines 378–390); SPEC §19 — *"Single-record forms/exact student-number lookup do not require paging."*

**Quote:** FR-36 — *"Every list screen shows 25 rows per page … manual search results …"*

**Problem:** FR-36 is broader than its authority document. At the door, an officer searching a common surname gets 25 rows and Previous/Next while a queue forms; SPEC explicitly exempts the exact student-number lookup. The PRD never repeats that exemption, so Member 4 will paginate the scanner, or leave it unpaginated and fail FR-36's blanket wording.

**Fix:** "Lists paginate at 25 rows. Exact student-number lookup returns one record and is exempt (SPEC §19); name search results paginate."

### [P3] Membership type has no value set and no default, though approval must create one

**PRD:** §3 Glossary *Member Profile* (line 74), §4.1 FR-2 (line 116), §4.2 FR-5 (line 143); DBML `members.membership_type nvarchar(100)`.

**Quote:** FR-2 — *"…creates an active Member-role Account and its linked Member Profile…"* FR-1 — *"Applicants cannot choose a Role, membership type or membership status."*

**Problem:** Approval must populate `membership_type`, but no source defines the permitted values, the default for a new approval, or whether the field is free text. It is referenced in three places and used by no rule — so it is either a free-text field nobody validates or an enum the team invents. This is a small guessing trap with a schema consequence (lookup table or not).

**Fix:** "Membership type is a free-text label (maximum 100 characters) defaulted by the setup script's configuration; it does not affect eligibility or capacity."

### [P3] SM-1 and SM-3 overclaim what can be observed

**PRD:** §8 SM-1 (lines 450–460), SM-3 (line 464).

**Quote:** SM-1 — *"Target: 100%. Validates FR-1–36."* SM-3 — *"…every merged PR passed Tier 1 (local tests/lint)…"*

**Problem:** SM-1's scenario list does not cover FR-35 (setup script), FR-36's page-normalisation rules, membership-inactive behaviour, or administrative cancellation — so "validates FR-1–36" is false as written. SM-3's Tier 1 is a developer's local action with no artifact; a reviewer cannot audit it, only Tier 2's CI result. Both metrics will be reported as met regardless, which is the failure mode counter-metric SM-C2 warns about.

**Fix:** Change SM-1 to "Validates the FRs cited by SPEC §20's scenario list; FR-35/FR-36 are validated by the demo setup and a paging check". Change SM-3's Tier 1 to "CI runs the same unit/lint gates as Tier 1, so a red local run cannot reach `dev`".

### [P3] "20 HLD screens" is asserted but the PRD merges several of them

**PRD:** §7.1 (line 429), §4.4 FR-20 (line 240), §4.6 FR-29 (line 306).

**Quote:** *"These are delivered across the 20 HLD screens…"*

**Problem:** SPEC §1.3 merges My passes into My Events and the certificate viewer into My Events/event-attendee management; the PRD follows SPEC but keeps the "20 screens" claim without a mapping. A reviewer comparing the PRD to the HLD screen list will look for screen 13 ("My passes") and screen 17 ("Certificate viewer") and not find them named anywhere in the PRD. This is a presentation risk, not a behavioural defect, but it is the cheapest rubric point on the table.

**Fix:** Add the mapping to the addendum (one line per HLD screen: kept, or merged into which FR), and soften the claim to "the 20 HLD screens, with screens 13 and 17 realised inside My Events per SPEC §1.3".

### [P3] The document is roughly twice the recorded length target

**PRD:** whole document, 5,623 words (502 lines).

**Quote:** §0 — *"The PRD builds on these inputs … and does not duplicate them"*; recorded decision: target 5–8 pages, technical detail in the addendum.

**Problem:** At normal density (500–600 words per page) the PRD is 9–11 pages, and the addendum is a further ~1.5. The overflow is concentrated in §0's input inventory, §9's reconciliation table (11 rows, several already resolved), §8's metric bullets, and §11's assumption index (which repeats §2.3, FR-13, FR-27, §10 Q6, NFR-6 verbatim). None of these is wrong; the document is just carrying bookkeeping that a grader will skim and the team will not read twice.

**Fix:** Move §9's table and §11's index to the addendum (keep a two-line pointer in the PRD), and compress §8 to one line per metric. That alone recovers ~1.5 pages without touching a requirement.

### [P3] "Officer" and "Management" are used interchangeably for actions the glossary distinguishes

**PRD:** §3 Glossary *Management* (line 75) and *Promotion* (line 85), §4.5 description (line 245), §4.5 FR-22 (line 254).

**Quote:** Glossary *Promotion* — *"The **Officer** action…"*; FR-17 — *"**Management** can trigger…"*; Glossary *Check-in* — *"The **officer**-controlled window…"*.

**Problem:** The glossary defines Management as Officer + Administrator and states Administrator includes all Officer capabilities, so the two are interchangeable in effect — but FRs use "Management can…" for authorisation while the glossary uses "Officer" for the same act. A literal implementer will add a role check that excludes Administrators from Promotion and Check-in control. Low impact, trivial to fix, and it is the kind of inconsistency a reviewer notices because the words are adjacent in the same section.

**Fix:** Use "Management" in every permission sentence and reserve "Officer" for the role definition and for prose about people (UJs).

---

## Considered and not reported

- **Self-approval of certificates (FR-27, line 294).** Bounded in practice: FR-28 re-checks Valid Attendance and completion at issuance, so an officer can approve their own eligibility only where an audited attendance record exists. It cannot fabricate attendance, and SPEC §2 explicitly permits it. Not a defect; it would become one only if an FR ever allowed the issuer to bypass the attendance re-check.
- **QR screenshot sharing / proxy attendance.** Inherent to the design and named in the addendum (§E). The unique-attendance constraint prevents replay of one token; FR-23 records the officer, so the residual risk is a process control rather than a system hole. The PRD's own value claim ("duplicate-proof") survives. The only wording I would change is in NFR-7, which promises privacy but says nothing about identity verification; that is a documentation improvement, not a defect.
- **Email/student-number login as an enumeration channel via FR-1's duplicate check.** FR-1 necessarily discloses that an identifier is taken (it must, to be usable). It is a deliberate consequence of the override and is already mitigated by the generic login failure. The real exposure is retry volume, reported above.
- **The Plan's certificate deferral, the HLD's "retained PDF is future", the UC's OR-eligibility, and the missing institutional domain.** Recorded decisions (§9); not gaps.
- **Missing `AttendTa-User-Stories.md`.** Already §10 Q3; the PRD's US-04 reference in §7.2 is resolvable from SPEC §1.2, so it is not a new finding.

---

## Verification gaps and judgment calls

- **No runtime or repository evidence was consulted for behaviour.** This is a document review; every finding is traceable to a quoted line and, where relevant, to a source document or a published browser constraint. The one external technical claim (camera requires a secure context) is sourced to MDN.
- **Camera/HTTPS severity.** Rated P1 rather than P0 because the manual fallback keeps UJ-3 demonstrable. If the client's acceptance script requires live QR scanning from a phone on the intranet, it becomes P0.
- **Page-count estimate.** Based on 5,623 words; exact page count depends on the renderer. The direction (over target) is certain; the magnitude is approximate.
- **Specification wording I could not fully verify:** the UC PDF's project-description cell is truncated in extraction, so the UC's "set permission levels" and OR-eligibility readings are taken from the PRD's own §9 table and the SPEC's paraphrase, not from a clean UC sentence. The PRD's recorded decision stands either way.
- **Not checked:** whether the 26 functional IDs in the HLD/WBS match the PRD's §7.1 list one-for-one beyond the count (the count is correct at 26); this belongs to the traceability scout.

---

## Scope and checks performed

Read in full: `prd-attendta.md` (all 502 lines), `addendum.md`, `AttendTa-System-Specification.md` (all 392 lines), `AttendTa-MVP.dbml`, `AttendTa-Sequence-Diagrams.md`, `UC-Project-FL.pdf`, `Alliance_Jumpstart_HLD_Group_7_AttendTa.pdf`, `WBS_J4S_ATTENDTA_20261103.pdf`, `Parent_Project_Plan_J4S_AttendTa_v2.00.pdf` (pp. 5–12), `git_qa_guidelines.pdf`.

Checks: searched both PRD files for "HTTPS/TLS/SSL/rate limit/throttle/lockout/cookie flags" (zero matches); counted functional IDs in §7.1 (26, matches the claim); word count (5,623); cross-checked every FR-to-SPEC claim cited in §9 against SPEC §§3, 5, 9, 11, 13, 14, 19; verified the Plan's browser row and access-mode row at p.6; verified the WBS's ID list and the 46 MD/2.3 MM totals; verified the QA guidelines' branch protections and tier definitions.

Not performed: no file in the PRD, addendum or `docsss/` was modified; no build, test, formatter or Git command was run.
