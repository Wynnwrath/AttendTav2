# Data-model reconciliation: AttendTa-ERD.md / AttendTa-MVP.dbml vs PRD + Addendum

> **Historical snapshot (2026-11-04).** This review predates the latest owner-confirmed decisions and source-document alignment. Treat its findings as historical; the current `prd-attendta.md`, `addendum.md`, and current `docsss/` sources govern.

**Reviewed:** `prd-attendta.md` (§0–§11), `addendum.md` (A–F), `docsss/AttendTa-ERD.md` (DBML block), `docsss/AttendTa-MVP.dbml`. SPEC v2.1 (`docsss/AttendTa-System-Specification.md`) cited where it is the authority (PRD §0, §9). The ERD's DBML block and the standalone DBML read as identical (not byte-diffed). Line references are to DBML/SPEC where available; otherwise by table.field.

**Severity:** High = explicit PRD requirement the schema cannot honour or that is audit-critical; Medium = rule expressible in DB but left to service code, or a model/term mismatch; Low = hygiene, naming, or indexing. No Critical findings.

**Summary:** The schema covers all 10 tables, all state enums, the core uniqueness rules (NFR-2) and the positive/ordering checks. The main gaps are (1) the `check_in_method` value set, (2) missing state-specific NOT NULL/CHECK rules for reasons and paired actor/time fields, (3) pending-only uniqueness on account requests, (4) the issuer snapshot and Certificate Review/Certificate model mismatch, and (5) append-only and JSON validity for the Activity Log.

---

## A. Attendance and check-in

- **G-01 · High · `check_in_method` does not match the PRD's method set (FR-23, FR-24).**
  - Source: DBML `Enum check_in_method { qr_code, manual_search, student_number }`; `attendance.check_in_method`. SPEC §11 (line 227): QR scans record automatically; "name/student-number lookup requires selecting the participant and clicking Check in" (one manual pathway with two search keys).
  - PRD: Glossary "Attendance Record" (method = QR, name search, student number); FR-23 (QR scan); FR-24 (one manual fallback by name *or* student number, then Check in).
  - Gap: No value is named `name_search`, the Glossary term. `student_number` is a search key, not a distinct check-in path, so the schema cannot record which lookup was used. UJ-3's "Ben searches Carlo's student number" has no unambiguous value (`manual_search` or `student_number`?). The enum and the Glossary disagree on the set.
  - Fix: Pick one model and align Glossary, FR-24 and the SPEC §18 seed list. (a) Keep `qr_code` and `manual_search`, add `lookup_key` (`name` | `student_number`, NULL when `qr_code`), and CHECK `(check_in_method = 'qr_code') = (lookup_key IS NULL)`. This fits FR-24's single fallback. (b) Use three values, `qr_code`, `name_search`, `student_number_search`, matching the Glossary verbatim. Either way, make the value set explicit in the PRD.

- **G-02 · High · `attendance` invalidation fields are not paired or required (FR-25).**
  - Source: DBML `attendance.invalidated_by`, `invalidated_at`, `invalidation_reason` (all nullable); no CHECK.
  - PRD: FR-25 (confirm and give a reason; original row kept). SPEC §11 (line 231): "server fills actor/time".
  - Fix: CHECK that the three are all NULL or all set, with `invalidation_reason` non-blank when set. Example: `(invalidated_at IS NULL AND invalidated_by IS NULL AND invalidation_reason IS NULL) OR (invalidated_at IS NOT NULL AND invalidated_by IS NOT NULL AND LEN(LTRIM(RTRIM(invalidation_reason))) > 0)`.

## B. Registrations

- **G-03 · High · Reviewed-at is not required for waitlisted/confirmed rows (FR-17 queue order).**
  - Source: DBML `registrations.reviewed_at`, `reviewed_by` (nullable); index `(event_id, status, reviewed_at, registration_id)`.
  - PRD: FR-17 (promotion chooses earliest by `reviewed_at`, then `registration_id`); Glossary "Waitlist". FR-16 (approval records review).
  - Gap: In SQL Server, NULLs sort first in ascending order. A waitlisted or confirmed row with NULL `reviewed_at` would silently become the next promotion candidate.
  - Fix: CHECK `status = 'pending' OR (reviewed_at IS NOT NULL AND reviewed_by IS NOT NULL)`. If status is `pending`, both must be NULL.

- **G-04 · High · Cancelled Registrations do not require cancellation actor, time or reason (FR-12, FR-16, FR-18, FR-32).**
  - Source: DBML `registrations.cancelled_by`, `cancelled_at`, `cancellation_reason` (all nullable).
  - PRD: FR-16 (decline "requires a reason"); FR-18 (cancellation "records the reason, actor and time"); FR-32 (reason before deactivation). SPEC §9.2 (line 194) "Decline becomes cancelled with reason"; SPEC §9.4 (line 202) "Save reason/actor/time".
  - Fix: CHECK `status <> 'cancelled' OR (cancelled_by IS NOT NULL AND cancelled_at IS NOT NULL AND LEN(LTRIM(RTRIM(cancellation_reason))) > 0)`.

- **G-05 · Medium · Decline reason target is undefined (FR-16).**
  - Source: DBML `registrations.review_notes` vs `cancellation_reason`.
  - PRD: FR-16 says decline sets `cancelled` and requires a reason. The schema has two text fields and the PRD names neither.
  - Fix: Document that decline writes `cancellation_reason` (with `cancelled_by` = reviewer), and `review_notes` holds only approval notes. Or state the reverse. Pick one and align G-04.

- **G-06 · Medium · `qr_token` lifecycle is not tied to status (FR-16, FR-18, FR-21).**
  - Source: DBML `registrations.qr_token` (nullable, unique); `confirmed_by`, `confirmed_at` nullable.
  - PRD: FR-16/FR-17 (confirmation generates token); FR-21 (cancellation clears token; "Unconfirmed and cancelled Registrations have no usable QR Pass"). SPEC §18 (line 361): "QR tokens are set on confirmation and cleared on cancellation".
  - Fix: CHECK `(status = 'confirmed') = (qr_token IS NOT NULL)`, and CHECK `status <> 'confirmed' OR (confirmed_by IS NOT NULL AND confirmed_at IS NOT NULL)`. `qr_token` must be NULL for `pending`, `waitlisted` and `cancelled`.

## C. Events

- **G-07 · High · Event cancellation reason and actor are not required (FR-12).**
  - Source: DBML `events.cancelled_by`, `cancelled_at`, `cancellation_reason` (nullable).
  - PRD: FR-12 ("A reason is required"); SPEC §13 (line 254) "Require reason/actor/time".
  - Fix: CHECK `status <> 'cancelled' OR (cancelled_by IS NOT NULL AND cancelled_at IS NOT NULL AND LEN(LTRIM(RTRIM(cancellation_reason))) > 0)`.

- **G-08 · Medium · Cover rule is only a comment, not a constraint (FR-10; Glossary "Cover"; SPEC §7 line 141).**
  - Source: DBML `events.image_path` note and the commented check block ("implement the status/cover rule in the service or a suitable trigger").
  - PRD: Glossary "Cover — required before publication"; FR-10 (publication requires a Cover); addendum B.3 (cancelled-after-publish keeps cover; cancelled drafts may have none).
  - Fix: Express it: `CHECK (image_path IS NULL OR LEN(LTRIM(RTRIM(image_path))) > 0)` and `CHECK (status = 'draft' OR image_path IS NOT NULL OR (status = 'cancelled' AND published_at IS NULL))`. The second rule keeps the addendum B.3 exception for cancelled drafts.

- **G-09 · Medium · Lifecycle actor/time pairs are not enforced (FR-10, FR-13, FR-12; SPEC §18 line 352 "paired actors/timestamps").**
  - Source: DBML `events.published_by/published_at`, `completed_by/completed_at`, `cancelled_by/cancelled_at`, `check_in_*`.
  - Gap: Nothing requires `published_by/at` when `status` is `published`, `completed` (or cancelled after publishing); `completed_by/at` when `completed`; or pairs for check-in open/close.
  - Fix: CHECK pairs per status. Example: `status IN ('published','completed') OR published_at IS NULL` (see G-08 for the cancelled exception). Add `status <> 'completed' OR (completed_by IS NOT NULL AND completed_at IS NOT NULL)`. Add `(check_in_opened_by IS NULL) = (check_in_opened_at IS NULL)`, and the same for closed.

- **G-10 · Medium · Check-in can remain open after completion or cancellation, and can open on non-published Events (FR-22; FR-12; FR-13; SPEC §13 line 256 "Close check-in").**
  - Source: DBML `events.is_check_in_open` (default 0; no CHECK).
  - Fix: CHECK `is_check_in_open = 0 OR status = 'published'`.

- **G-11 · Low · Event description is nullable (FR-9; SPEC §7 line 150).**
  - Source: DBML `events.description nvarchar(max)` (nullable).
  - PRD: FR-9 lists description as an event field. SPEC §7 (line 150) lists "Title, description, type, venue, scheduled start, positive participant limit ..." as required content, with no optional marker.
  - Fix: NOT NULL with non-blank CHECK, or record in the PRD that description is optional and correct SPEC §7.

## D. Master data and members

- **G-12 · Medium · Event Type description is nullable (FR-6; §9 "Event Type description: Included").**
  - Source: DBML `event_types.description` (nullable); `type_name` has no non-blank CHECK.
  - PRD: FR-6 "Event Type (unique, non-blank name and a description)".
  - Fix: NOT NULL, plus CHECK non-blank on `type_name` and `description`.

- **G-13 · Low · Required free-text fields have no non-blank checks (FR-6, FR-7, FR-9, FR-1; SPEC §18 line 358 "nonblank required paths").**
  - Source: DBML `users.full_name`, `account_requests.full_name`, `event_types.type_name`, `venues.room_name`, `events.title`.
  - Fix: CHECK `LEN(LTRIM(RTRIM(col))) > 0` on each. `venues.image_path` already has one.

- **G-14 · Low · `membership_type` is free text with no defined domain (FR-5; SPEC §4 line 108).**
  - Source: DBML `members.membership_type nvarchar(100)` nullable.
  - PRD: FR-5 lets Management edit membership type, but the PRD never defines its values or whether it is required.
  - Fix: Decide: a seeded lookup (SPEC §18 says seeded enums are lookup rows), or document free text and nullable. Record the decision in the PRD.

- **G-15 · Low · Where the "full name" lives conflicts between FR-5 and the Glossary.**
  - Source: DBML `users.full_name`; `members` has no name column.
  - PRD: FR-5 lists "full name" among Member Profile fields that Management edits. Glossary "Member Profile" lists student number, course, year, section, mobile, type and status, with no name; Glossary "Account" holds the name. Schema follows the Glossary.
  - Fix: Clarify FR-5 so name edits apply to the Account's `full_name` via the Member Profile screen. No schema change.

- **G-16 · Medium · Cross-table guards on capacity are service-only (FR-7, FR-9; addendum B.4).**
  - Source: DBML `events.participant_limit` (CHECK > 0 only); `venues.capacity` (CHECK > 0 only). No FK or check can express `participant_limit ≤ venues.capacity`.
  - PRD: FR-9 (limit ≤ venue capacity); FR-7 (capacity cannot drop below any linked draft/published limit); FR-10 (valid capacity at publish).
  - Fix: Transactional checks on event save/publish and venue update, with the rule documented in the service contract (addendum B.4 already acknowledges it). Optional trigger for defence in depth.

## E. Account requests and accounts

- **G-17 · High · Pending-only uniqueness for `email` and `student_number` is not expressed (FR-1; NFR-2; addendum B.5).**
  - Source: DBML `account_requests.email` and `student_number` are plain indexes, not unique; the pending-only rule is only in a Note.
  - PRD: FR-1 (rejected if email or student number matches another *pending* request); NFR-2 (uniqueness at DB level where possible). SPEC §18 line 316 ("Enforce one account/email ...").
  - Fix: Filtered unique indexes `UNIQUE (email) WHERE status = 'pending'` and `UNIQUE (student_number) WHERE status = 'pending'`. Keep non-unique lookup indexes for login (FR-3) if needed.

- **G-18 · High · Rejection reason is not enforced (FR-2).**
  - Source: DBML `account_requests.review_notes` (nullable).
  - PRD: FR-2 ("Rejection requires a reason"); SPEC §3.2 (line 94) "Rejection requires reviewer, time and reason".
  - Fix: CHECK `status <> 'rejected' OR LEN(LTRIM(RTRIM(review_notes))) > 0`.

- **G-19 · Medium · Account-request state-specific fields are not paired (FR-2; addendum B note; DBML note on `account_requests`).**
  - Source: DBML `account_requests.password_hash` (nullable), `reviewed_by/at`, `created_user_id`.
  - PRD: FR-2 (approval creates Account, moves hash, clears it from request, records reviewer and time; rejection clears hash). Glossary "Account Request" states.
  - Gap: Nothing enforces that `pending` has a hash and no review, that `approved` has `created_user_id` and reviewer, or that `rejected` has no hash.
  - Fix: CHECK `(status = 'pending') = (password_hash IS NOT NULL)`; CHECK `status = 'pending' OR (reviewed_by IS NOT NULL AND reviewed_at IS NOT NULL)`; CHECK `(status = 'approved') = (created_user_id IS NOT NULL)`.

- **G-20 · Medium · Cross-table uniqueness for requests is service-only (FR-1).**
  - Source: DBML `account_requests` has no FK to `members`; `users.email` and `members.student_number` are unique for accounts and profiles.
  - PRD: FR-1 rejects a request whose email or student number matches "any existing Account or Member Profile (including inactive ones)". `users.email` covers Accounts. `members.student_number` covers profiles, but a request's `student_number` is not checked against it by the schema.
  - Fix: Document a transactional check at request submission and approval. Add a test for an inactive profile's student number.

- **G-21 · Low · Concurrent decision handling has no schema support (FR-2; SPEC §3.2 line 94).**
  - Source: DBML `account_requests` has no row version or review token.
  - PRD: FR-2 ("Two concurrent decisions produce exactly one outcome").
  - Fix: Document a conditional update (`UPDATE ... WHERE request_id = @id AND status = 'pending'` and check rows affected) inside the approval transaction. A `rowversion` column is optional. SPEC §18 notes `timestamp` is rowversion, not event time.

- **G-22 · Low · `users.created_by` is nullable beyond the bootstrap case (SPEC §18 line 352; addendum B.2).**
  - Source: DBML `users.created_by` (nullable).
  - PRD: FR-2 (approval creates an Account; reviewer recorded on the request). FR-35 (setup admin has no creator).
  - Fix: Service sets `created_by` to the reviewing officer on approval. Optional CHECK limiting NULL to the bootstrap admin is hard to express, so document it as a service rule and test it.

- **G-23 · Low · Naming differs between the PRD Glossary and the schema.**
  - Source: DBML `Enum user_role { admin, officer, member }`; `events.registration_deadline`; `certificates` table (the Glossary has "Certificate Review" and "Certificate").
  - PRD: Glossary "Role" (Member, Officer, Administrator); "Signup Deadline"; "Certificate Review".
  - Fix: Rename to the Glossary terms, or add a mapping table in the PRD. Renaming before implementation is cheaper.

## F. Certificates

- **G-24 · High · Certificate state-specific fields are not enforced (FR-27, FR-28; SPEC §16 line 316).**
  - Source: DBML `certificates.reviewed_by/reviewed_at/review_notes`, `certificate_number`, `recipient_name_snapshot`, `event_title_snapshot`, `event_starts_at_snapshot`, `file_path`, `issued_by/issued_at`; `status` enum.
  - PRD: FR-27 (decline reason required; reviewer and time recorded); FR-28 (issued Certificate has unique number, snapshots, issuance metadata and retained PDF); FR-28 "A failed file write leaves the Certificate unissued".
  - Fix: CHECK `status = 'pending_approval' OR (reviewed_by IS NOT NULL AND reviewed_at IS NOT NULL)`; CHECK `status <> 'not_eligible' OR LEN(LTRIM(RTRIM(review_notes))) > 0`; CHECK `status <> 'issued' OR (certificate_number, file_path, issued_by, issued_at, recipient_name_snapshot, event_title_snapshot, event_starts_at_snapshot all NOT NULL)`.

- **G-25 · Medium · Issuer is not snapshotted (FR-28; SPEC §14.3 line 274 "Preserve ... issuer").**
  - Source: DBML `certificates.issued_by` (FK to `users`); no `issuer_name_snapshot`.
  - PRD: FR-28 "snapshots of the recipient's full name, the Event title and start, the issue date and the issuer"; NFR-1/FR-11 (issued Certificates keep snapshots).
  - Gap: Editing the issuer's `users.full_name` would change what the database shows for the issuer, while the retained PDF stays fixed. The DB record cannot reproduce the issuer shown on the PDF.
  - Fix: Add `issuer_name_snapshot nvarchar(255)`, set at issuance. If "issuer" means the organization (a constant), say so in the PRD and drop this item.

- **G-26 · Medium · Certificate Review and Certificate are merged, and the enum mixes them (Glossary "Certificate Review", "Certificate"; SPEC §14.2 line 266).**
  - Source: DBML `certificates` table with `certificate_status { pending_approval, eligible, not_eligible, issued }`.
  - PRD: Glossary "Certificate Review — States: pending_approval → eligible | not_eligible" (no `issued`); "Certificate — the retained PDF issued from an eligible Certificate Review". Two objects are defined.
  - Gap: Glossary states and the DBML enum disagree. One row per Registration is consistent with NFR-2, but the PRD does not record the merge.
  - Fix: Decide and record. (a) Keep one table and update the Glossary: Certificate Review is the `certificates` row with the four states, and `issued` is the final state. (b) Split into `certificate_reviews` and `certificates` (1:1 on `registration_id`). Option (a) is simpler for MVP.

- **G-27 · Low · `certificates.file_path` has no uniqueness (SPEC §14.3 line 276 "unique output path"; NFR-4).**
  - Source: DBML `certificates.file_path` (no unique index).
  - Fix: Filtered unique index `WHERE file_path IS NOT NULL`. Same consideration for `events.image_path` and `venues.image_path` if one path per record is enforced (FR-8).

## G. Activity log

- **G-28 · Medium · Append-only is not enforced (FR-33; NFR-3; SPEC §18 line 333).**
  - Source: DBML `activity_logs` (no constraint or permission).
  - PRD: FR-33 "Entries are append-only"; NFR-3 "Activity Log is append-only".
  - Fix: Grant the application role INSERT and SELECT only on `activity_logs` (DENY UPDATE and DELETE), or add a trigger that rejects UPDATE and DELETE. Document it in the DDL.

- **G-29 · Medium · Actor pairing and JSON validity are not enforced (FR-33; SPEC §15 line 314).**
  - Source: DBML `activity_logs.actor_type`, `actor_user_id` (nullable), `old_values`, `new_values` (`nvarchar(max)`, no validation).
  - PRD: FR-33 "Each entry stores actor type and Account" and "validated JSON old/new values"; SPEC §18 line 358 "JSON validity".
  - Fix: CHECK `(actor_type = 'user' AND actor_user_id IS NOT NULL) OR (actor_type = 'system' AND actor_user_id IS NULL)`; CHECK `old_values IS NULL OR ISJSON(old_values) = 1`; the same for `new_values`.

- **G-30 · Low · Old/new values could carry prohibited data (FR-33 "never contain passwords, hashes, QR tokens or physical file paths"; NFR-1).**
  - Source: DBML `activity_logs.old_values/new_values`; `registrations.qr_token` and `venues/events/certificates` path columns are on the same rows that are logged.
  - Gap: The schema cannot stop a row dump from including `qr_token` or `image_path`.
  - Fix: Explicit allow-list serializer per entity type in the service layer, with a test that `qr_token`, hashes and paths never appear in logged JSON. Image changes log a description, not a path.

## H. Uniqueness expression and identifiers

- **G-31 · Low · `unique` on nullable columns is not valid as written in SQL Server.**
  - Source: DBML `account_requests.created_user_id [unique]`, `registrations.qr_token [unique]`, `certificates.certificate_number [unique]`. Each is nullable; the note says "filtered unique index".
  - PRD: NFR-2 (one Account per request; unique QR token; unique certificate number).
  - Fix: Express each as a filtered unique index `WHERE col IS NOT NULL` in the DDL, and mirror it in the DBML if the DBML is the deployment source.

- **G-32 · Low · Email and student-number normalization and collation are not specified (FR-1; FR-3; SPEC §18 line 358 "Normalize identifiers consistently").**
  - Source: DBML `users.email`, `members.student_number`, `account_requests.email` and `student_number` (no normalization rule).
  - PRD: FR-1 (malformed email rejected); FR-3 (login by email or student number); NFR-2 (unique email and student number).
  - Fix: State the rule (trim, lowercase email, student number format) and the collation (case-insensitive). Login and uniqueness both depend on it.

## I. Indexing for FR-36 lists

- **G-33 · Low · Some list orderings lack an index or are mis-keyed (FR-36).**
  - Source: DBML `registrations` index `(event_id, status, reviewed_at, registration_id)`; `certificates` has no status index; `users` has no role or active index.
  - Gaps: (a) The "review registrations" list shows Pending rows, whose `reviewed_at` is NULL, so the reviewed-at index does not order them. Add `(event_id, status, submitted_at, registration_id)`. (b) The certificate queues (FR-36) have no status index; add `(status, certificate_id)`. (c) The Accounts list has no index; add `(role, is_active, user_id)` or document it as small. (d) Manual name search (FR-24) joins `users.full_name` with no index; acceptable at this scale, but say so in the plan.
  - Fix: Add the indexes above, or document why not, and re-check with the query plans (SPEC §18 line 360).

---

## Verified: consistent with PRD (no gap)

- **Entities:** the 10 tables match addendum B and SPEC §18. Glossary to table: Account Request → `account_requests`; Account + Role → `users` (`role`, `is_active`); Member Profile → `members`; Event Type → `event_types`; Venue → `venues`; Event → `events`; Registration → `registrations`; Attendance Record → `attendance`; Certificate Review + Certificate → `certificates` (see G-26); Activity Log → `activity_logs`.
- **State enums:** `account_request_status`, `event_status`, `registration_status`, `membership_status` (active/inactive), `activity_actor_type` (user/system) and check-in open/closed (`is_check_in_open`) match the Glossary. Event transitions (draft→published→completed; draft/published→cancelled; no republish) are service-enforced by design (NFR-1, SPEC §18 line 359). Derived states need no column: Valid Attendance (`invalidated_at IS NULL`), No-show, Not confirmed before cutoff, and Seat (confirmed count).
- **Uniqueness (NFR-2):** `users.email`; `members.user_id` and `members.student_number`; `registrations (event_id, member_id)`; `attendance.registration_id`; `certificates.registration_id`; `account_requests.created_user_id` (request-to-account 1:1). Caveat: G-31 for nullable columns.
- **Positive and ordering checks:** `year_level > 0` on `account_requests`, `members`, `registrations.year_level_snapshot` (FR-1, FR-5, FR-14); `venues.capacity > 0` (FR-7); `events.participant_limit > 0` (FR-9); `registration_deadline <= starts_at` (FR-9, FR-15).
- **Venue photo:** `venues.image_path` NOT NULL plus non-blank CHECK (FR-7, FR-8, addendum B note 1). Event cover for drafts nullable (FR-9).
- **Snapshots:** `registrations.course_snapshot`, `year_level_snapshot`, `section_snapshot` (FR-14, FR-30). Certificate `recipient_name_snapshot`, `event_title_snapshot`, `event_starts_at_snapshot` (FR-28). Issue date is `issued_at` (FR-28).
- **Actor fields:** each consequential action has an actor column: `reviewed_by`, `confirmed_by`, `cancelled_by`, `published_by`, `check_in_opened_by/closed_by`, `completed_by`, `cancelled_by` (events), `recorded_by`, `invalidated_by`, `reviewed_by`, `issued_by`, and `created_by/updated_by`. Member self-cancel and self-registration use the member's `user_id`. Role, active and membership changes use `users.updated_by` and `members.updated_by`, with the reason in the Activity Log (FR-31/32/33).
- **Login and sessions:** `users.email` unique; `members.student_number` unique for student-number login (FR-3); no session, verification or reset tables (PRD §6; DBML header). `password_hash varchar(255)` is adequate for a salted-hash output (NFR-1; algorithm open per addendum A).
- **Attendance rules:** `attendance.registration_id` unique gives at most one row, and re-check-in after invalidation is rejected (FR-23, FR-25; addendum B.1). The invalidated row is kept (FR-25).
- **QR and files:** `registrations.qr_token varchar(64)` fits a 256-bit unpredictable token. No PNG column, so PNGs are generated on demand (FR-21). File columns hold relative paths (NFR-4, addendum A).
- **No archive or delete fields** (NFR-3); no registration-opening fields (FR-10; SPEC §7 line 165); no user image field (§7.2); check-in defaults closed (FR-22).
- **Queue and feed indexes:** `events (status, starts_at, event_id)` for the feed (FR-36); `registrations (event_id, status, reviewed_at, registration_id)` for waitlist order (FR-17); `registrations (member_id, submitted_at, registration_id)` for My Events (FR-20, FR-36); `account_requests (status, requested_at, request_id)` for the request queue (FR-36).
- **Derived reports:** FR-30 attendance rate, no-shows and course/year distribution of attendees all derive from the stored fields. Cancelled Events are separated by `status`.
- **Activity log ordering:** `(occurred_at, log_id)` and entity/actor indexes support FR-34.
- **Delegated checks, not gaps:** capacity-at-publish, Valid-Attendance-before-cancel (FR-12, FR-18, FR-32), issued-blocks-correction (FR-25) and promotion-on-cancel prohibition (FR-17) need the data the schema already holds. They require transactional service checks, not new columns.

## J. Informational (not schema gaps)

- **I-01 · Config, not data.** Organization timezone (FR-14; NFR-5; PRD §10 Q1) and fixed certificate artwork (FR-28; §10 Q1) are configuration or assets. No table is needed. The PRD still lists them as open and blocking acceptance.
- **I-02 · Addendum consistency.** Addendum B tensions 1–6 match this review. Tension 3 (cover rule) is covered by G-08 and tension 5 (pending-only indexes) by G-17.

## K. Suggested decisions for the PM and architect

1. G-01: choose the check-in method model (option a or b) and update the Glossary, FR-24 and the seed list.
2. G-26: choose single-table or split Certificate Review.
3. G-25: confirm whether "issuer" is a person (snapshot needed) or the organization (no column).
4. G-05: decide where decline reasons are stored.
5. G-14: decide whether membership type is a seeded lookup or free text.
