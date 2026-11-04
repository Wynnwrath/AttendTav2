# Architecture Spine Reconciliation Review — Initiative & Epic Tickets

- **Artifact under review:** `_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md`
- **Reviewed against:**
  - Initiative ticket: `_bmad-output/initiative-attendta/initiative-attendta.md` (Requirements R1–R36, Constraints, Done-when, Boundaries, Notes)
  - Shared requirements & build order: `_bmad-output/initiative-attendta/tickets/tickets.toml` (lines 1–8 notes & DAG)
  - Epic tickets (8): `_bmad-output/initiative-attendta/tickets/epic-*/epic-*.md`
  - Voice, tone & companions: `_bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md`, `_bmad-output/initiative-attendta/prd-attendta/addendum.md`
- **Reviewer:** `ReconcileTicket`
- **Date:** 2026-11-04
- **Verdict:** **CHANGES REQUIRED** (1 Critical, 3 High, 4 Medium, 2 Low findings)

---

## Executive Summary

The AttendTa Architecture Spine (`architecture-attendta.md`) successfully establishes the core technical substrate for a layered ASP.NET Core MVC monolith (.NET 9 + SQL Server LocalDB/container), standardizing the unit-of-work transaction script pattern (AD-3), row-level SQL locking (AD-4), canonical participation expressions (AD-5), audit structure (AD-7), and cookie-based identity (AD-8).

However, reconciliation against the initiative ticket, the 8 epic tickets, and the shared-requirement splits in `tickets.toml` reveals several critical omissions and architectural contradictions:
1. **Attendance Invalidation vs. Certificate Issuance (R25):** The spine completely omits the invalidation cascade from AD-2's cross-module writes, while the Services-layer module diagram draws `Certificates --> Check-in`. If Check-in must verify issuance and transition reviews to `not_eligible`, it introduces a circular dependency (`CHK <--> CERT`) that breaks the build order.
2. **`members` Table Ownership & Build Order Inversion (R2, R5):** AD-2 lists Platform Access (e1) as the writing module for member creation, but `epic-platform-access.md` states `MemberService.CreateMemberFromApprovedAccountRequest` is owned by Master Data (e2). Because e1 is built before e2 and the module diagram has `MD --> AUTH` without an `AUTH --> MD` edge, this ownership and calling path is contradictory.
3. **`members` Table Missing from AD-4 Lock Hierarchy:** The lock order table lists `account_requests`, `users`, `event_types`, `venues`, `events`, and participation tables, completely omitting `members` from the acquisition hierarchy.
4. **Silent Dropping of Invariant Constraints:** Core ticket constraints—specifically the explicit ban on login throttling and account lockout (NFR-1), the strict ban on hard deletes of business entities (NFR-3), and the 5 required database uniqueness constraints / filtered unique indexes (NFR-2, Addendum B.5)—are silently omitted from the spine rules.
5. **Quiet Requirements & Governance Gaps:** The Capability Map omits paging (AD-13) from 5 out of 7 list epics, omits Event Cover handling (R8, AD-12) from e3, omits certificate links in `MyEvents` (R20) from e6, provides no architectural composition pattern for `MyEvents` across 3 contributing epics, and drops the UX Voice and Tone constraints (EXPERIENCE.md) regarding misleading feedback copy.

---

## Detailed Findings

### Finding 1: Circular Dependency & Missing Cross-Module Invariant for Attendance Invalidation vs. Certificate Review/Issuance (R25)
- **Severity:** Critical
- **Location:** Section `AD-2 — One writing module per table` (lines 76–93), Section `Design Paradigm` Module Dependencies Diagram (lines 41–60), Section `Capability → Architecture Map` (lines 354, 356).
- **Evidence:**
  - `initiative-attendta.md:47` (R25): *"Management invalidates an Attendance Record after a confirmation and reason, keeping the row. It leaves reports and eligibility, turns any unissued Certificate Review not_eligible, is blocked once a Certificate is issued, and is serialized against issuance."*
  - `tickets.toml:6`: *"R25 — e4 invalidation, reason and re-check-in refusal; e6 not_eligible effect and serialization with issuance."*
  - `epic-check-in.md:30` (R6 / Boundaries): *"Its Certificate Review effect and serialization with issuance belong to epic-certificates."*
  - `epic-certificates.md:27` (R3): *"Invalidating an Attendance Record turns any unissued Certificate Review not_eligible with the correction reason, is blocked once a Certificate is issued, and is serialized against issuance."*
  - `architecture-attendta.md:86-87` (AD-2 table): `attendance` writing module is Check-in (e4); `certificates` writing module is Certificates (e6).
  - `architecture-attendta.md:90-93`: Lists only two cross-module writes: Event completion (e3 -> e6) and Check-in toggle (e4 -> e3). Attendance invalidation is completely absent.
  - `architecture-attendta.md:48` (Mermaid diagram): `CERT["Certificates"] --> CHK["Check-in"]`. There is no edge from `CHK` to `CERT`.
  - Build order: Epic 4 is built before Epic 6 (`tickets.toml:39, 55`).
- **Analysis:**
  When an Officer invalidates attendance in e4, the system must (1) verify that a certificate has not already been issued for that registration (blocking invalidation if issued), and (2) transition any existing unissued Certificate Review to `not_eligible`. Because `certificates` can only be written by e6 under AD-2, e4 cannot update `certificates` directly. If e4 calls an e6 service, it introduces a circular dependency with `CERT --> CHK` in the Services layer. Furthermore, because e4 is built before e6, e4 would fail to compile if it directly coupled to e6.
- **Concrete Proposed Fix:**
  1. In **AD-2**, add a third bullet under *Cross-module writes*:
     > - **Attendance invalidation (R25).** e4 owns the invalidation command and audit. e6 owns the certificate review status change. e4's invalidation use case verifies no issued certificate exists and updates the review to `not_eligible` by invoking `ICertificateInvalidationHandler.HandleAttendanceInvalidatedAsync(registrationId, reason, ct)`. To satisfy build order, e4 defines `ICertificateInvalidationHandler` with a no-op implementation; e6 provides the operational implementation joining e4's transaction.
  2. In the **Module Dependencies Diagram** (lines 41–60), document that `CHK` interacts with `CERT` via interface abstraction in `Services/Interfaces` so that concrete module coupling flows in accordance with the build order, or use a domain event/handler hook.

---

### Finding 2: `members` Table Ownership Contradiction and Build Order Inversion on Account Approval (R2, R5)
- **Severity:** High
- **Location:** Section `AD-2 — One writing module per table` (lines 76–82), Section `Design Paradigm` Module Dependencies Diagram (line 52), `epic-platform-access.md:54-56`.
- **Evidence:**
  - `architecture-attendta.md:80-82`:
    - `| members (create on approval) | Platform Access (e1) |`
    - `| members (profile fields) | Master Data (e2) |`
    - `| members (membership status) | Administration (e5) |`
  - `architecture-attendta.md:74`: *"Rule: Each table has one repository class in Data. Only the module listed below may call write methods for the listed table or columns, and only from its own service."*
  - `epic-platform-access.md:54, 56`: *"Belongs to epic-master-data: Member, Event Type and Venue screens and the image storage; this epic only makes their actor and role claims real."* and *"Touch point: `MemberService.CreateMemberFromApprovedAccountRequest` — called by approval; owner: epic-master-data."*
  - `architecture-attendta.md:52` (Mermaid diagram): `MD --> AUTH`. There is NO dependency from `AUTH` to `MD`.
  - `tickets.toml:23`: Epic 1 has no dependencies (`after = []`); Epic 2 depends on Epic 1 (`after = [{ epic = 1 }]`).
- **Analysis:**
  There is a direct contradiction between the spine and the epic tickets. If Master Data (e2) owns `MemberService.CreateMemberFromApprovedAccountRequest` as stated in `epic-platform-access.md`, then Epic 1 must call Master Data. But Epic 1 is built before Epic 2, and the module diagram explicitly shows `MD --> AUTH` (meaning `AUTH --> MD` would create a circular dependency). Conversely, if Platform Access (e1) owns `members (create on approval)` as stated in the spine's AD-2 table, then `MemberRepository` in Data is written to by three distinct services across three epics (e1, e2, e5), violating the core AD-2 premise: *"Only the module listed below may call write methods for the listed table or columns, and only from its own service."*
- **Concrete Proposed Fix:**
  1. Revise AD-2's rule and table to clarify repository and service boundaries:
     > `MemberRepository` in Data exposes `Create`, `UpdateProfile`, and `UpdateMembershipStatus`.
     > - Platform Access (e1) owns member record creation upon Account Request approval via `IMemberCreationService` (or directly via `MemberRepository.CreateAsync` inside e1's `AccountRequestService`).
     > - Master Data (e2) owns member profile updates (`MemberService`).
     > - Administration (e5) owns membership status updates (`MembershipAdministrationService`).
  2. In `epic-platform-access.md:56`, delete the note stating `MemberService.CreateMemberFromApprovedAccountRequest` is owned by `epic-master-data`, confirming it belongs to `epic-platform-access` (e1).

---

### Finding 3: `members` Table Completely Missing from AD-4 Lock Order Hierarchy
- **Severity:** High
- **Location:** Section `AD-4 — Lock order and serialization` (lines 110–123).
- **Evidence:**
  - `architecture-attendta.md:110-117`:
    ```text
    Locks are taken only in this order:
      1. App lock attendta:last-admin (sp_getapplock, Exclusive, owner = Transaction)...
      2. account_requests.
      3. users, ascending id.
      4. event_types.
      5. venues.
      6. events, ascending id.
      7. registrations, attendance, certificates.
    ```
  - `database/attendta-mvp-schema.sql`: `members` is one of the ten primary relational tables.
  - `initiative-attendta.md:27` (R5): *"Management edits a Member's full name, course, year level, section, mobile, membership type and membership status. A student-number correction requires a reason, stays unique and logs old/new values."*
  - `initiative-attendta.md:54` (R32): Deactivation of membership status cancels upcoming registrations.
- **Analysis:**
  AD-4 enumerates 7 levels of locks to prevent database deadlocks across parallel transactions. However, `members` is omitted entirely. Mutations to `members` occur in e1 (approval creation), e2 (profile edit, student number correction), and e5 (membership status deactivation). If a developer implements row locking for student number corrections or deactivation, they have no invariant specifying whether `members` is locked directly, what lock order it takes relative to `users` or `events`, or whether locking the parent `users` row is required.
- **Concrete Proposed Fix:**
  Update AD-4 lines 112–114 to explicitly integrate `members`:
  > Insert between items 3 and 4:
  > `3. users, ascending id.`
  > `4. members, ascending member_id.`
  > (Renumber remaining items 5 to 8).
  > Add to line 119:
  > *"Per-participant operations lock the owner's users row first (and members row where member fields are modified), then the event."*

---

### Finding 4: Silent Dropping of Core Ticket Constraints (No Throttling/Lockout, No Hard Deletes, Database Uniqueness Constraints)
- **Severity:** High
- **Location:** Section `AD-8 — Identity, session and authorization` (lines 161–172), Section `AD-10 — Lookup-backed states and the single schema` (lines 184–193), Section `Consistency Conventions` (lines 272–284).
- **Evidence:**
  - **No throttling/lockout:** `initiative-attendta.md:62` (Constraints / NFR-1): *"...no login throttling or lockout. (prd NFR-1; addendum A; architecture 6)"*.
    *Spine:* Neither AD-8 nor AD-15 mentions this constraint; "throttling" and "lockout" do not appear anywhere in the spine.
  - **No hard deletes:** `initiative-attendta.md:64` (Constraints / NFR-3): *"No hard deletes of business records and no archive/restore; Activity Log append-only... (prd NFR-3; addendum B.12)"*.
    *Spine:* AD-7 line 159 enforces an `INSTEAD OF UPDATE, DELETE` trigger on `activity_logs`, but nowhere does the spine prohibit repositories or controllers from issuing hard deletes (`DELETE` or `DbContext.Remove`) on business entities (`members`, `event_types`, `venues`, `events`, `registrations`, `attendance`, `certificates`).
  - **Database uniqueness constraints:** `initiative-attendta.md:63` (Constraints / NFR-2): *"uniqueness enforced in the database: Account email, Member student number, Registration per Member/Event, Attendance per Registration, Certificate per Registration. (prd NFR-2; architecture 7)"* and `addendum.md:49` (B.5): *"Pending-only unique indexes on account_requests.email and student_number are required..."*.
    *Spine:* AD-10 line 190 only specifies lookup tables, `event_types.description NOT NULL`, `certificates.issuer_name_snapshot`, and the audit trigger. It never binds the five core business uniqueness constraints or the filtered pending indexes.
- **Analysis:**
  These constraints are architectural invariants that bind all epics. Without an explicit rule in AD-8, an auth developer might introduce rate-limiting or lockout middleware, breaking automated concurrency test suites (SM-4) or demo flows. Without an invariant in AD-1/AD-2, developers could implement standard EF Core `Remove` methods for cancel/decline operations instead of status updates. Without explicit binding of DB unique constraints in AD-10, database-first mapping could omit unique constraints on registrations or attendance.
- **Concrete Proposed Fix:**
  1. In **AD-8 Rule**, add:
     > *"In accordance with NFR-1, no login throttling, rate limiting, or account lockout middleware is implemented in the MVP."*
  2. In **Consistency Conventions**, add a row for *Entity Lifecycles*:
     > `Lifecycles | No hard deletes of business records. Repositories must not expose Delete methods or execute SQL DELETE on business entities. Cancellation, deactivation, and rejection are represented strictly through status transitions and foreign keys.`
  3. In **AD-10 Rule**, amend line 190:
     > *"e1 delivers all ten tables, the lookups, `event_types.description NOT NULL`, `certificates.issuer_name_snapshot`, the audit trigger, and the canonical database unique constraints: `users.email`, `members.student_number`, `(registrations.event_id, registrations.member_id)`, `attendance.registration_id`, `certificates.registration_id`, plus filtered unique indexes on pending `account_requests.email` and `account_requests.student_number`."*

---

### Finding 5: Venue Capacity vs. Linked Events Guard Lacks Governing Architecture Contract (R7)
- **Severity:** Medium
- **Location:** Section `AD-2 — One writing module per table`, Section `Design Paradigm` Module Dependencies Diagram (line 45), `epic-master-data.md:28, 77`.
- **Evidence:**
  - `initiative-attendta.md:29` (R7): *"Capacity never drops below the participant limit of a linked draft or published Event."*
  - `epic-master-data.md:77`: *"Decision: 2026-11-04 — venue capacity guard against linked draft/published events is verified under test/seeding in this epic prior to epic-events-registration building the event creation UI."*
  - `architecture-attendta.md:45` (Mermaid diagram): Shows `REG["Events & Registration"] --> MD["Master Data"]`. There is NO dependency from `MD` to `REG`.
  - Build order: Master Data (e2) is built before Events & Registration (e3) (`tickets.toml:23, 31`).
- **Analysis:**
  To prevent reducing venue capacity below the participant limit of any linked draft or published event, e2's `VenueService` must query `events`. However, the Services module diagram shows `REG --> MD`. If `MD` references `REG` services, a circular dependency is created. Because e2 is implemented before e3, e2 cannot call an e3 service. The spine specifies write-module rules in AD-2, but does not specify how cross-module validation queries are performed across the layer boundaries.
- **Concrete Proposed Fix:**
  Add a rule to **AD-2** or **AD-5**:
  > *"Cross-module validation queries: When a module validates business rules against records owned by a downstream module (e.g., Master Data checking participant limits of linked events in R7), the check is executed via a read-only query on `DbContext.Events` or a Data-layer query in `ParticipationFacts`, avoiding Services-layer dependencies back to downstream modules."*

---

### Finding 6: Silent Dropping of UX Voice, Tone, and Quiet Security/Feedback Constraints
- **Severity:** Medium
- **Location:** Section `AD-9 — Outcome contract` (lines 173–183), Section `AD-14 — UI shell and assets` (lines 221–230), Section `Consistency Conventions` (lines 272–284).
- **Evidence:**
  - `_bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md:35-38`: *"Voice and Tone: Use brief, direct labels and outcomes. 'Request membership,' 'Registration pending,' 'Check-in closed,' and 'No participants found' tell people what happened. Show the next permitted step and a reason when relevant. Do not promise email, automatic promotion or successful issuance before the server confirms it. Label time as Asia/Manila."*
  - `initiative-attendta.md:25` (R3): *"Every failure is generic."*
  - `initiative-attendta.md:68` (NFR-7): *"The QR Pass carries no personal data; unknown-token and unauthorized responses reveal nothing about other records; email is required but unverified."*
  - `architecture-attendta.md:177-183` (AD-9): Focuses on exception throwing and status code translation, but omits: (1) generic login failure messages, (2) the prohibition against promising email/auto-promotion/auto-issuance in UI copy, and (3) that email is unverified.
- **Analysis:**
  Tone and feedback rules in AttendTa carry functional and security implications. For example, promising an email confirmation misleads users since email delivery is explicitly out of scope (Boundaries line 85). Promising auto-promotion violates R17/Boundaries line 86. Revealing whether an email exists on login failure violates R3 and NFR-1. The spine treats AD-9 and AD-14 purely as technical plumbing and omits these behavioral constraints.
- **Concrete Proposed Fix:**
  Add to **AD-9 Rule** or **Consistency Conventions**:
  > `Feedback & Tone | Error messages on login are generic ('Invalid email or password') and never disclose account existence (R3, NFR-1). Flash messages (TempData) and UI labels follow EXPERIENCE.md Voice and Tone: brief, direct, displaying next permitted steps and reasons where required, and never promising email notifications, automatic promotion, or automatic issuance. Email addresses are mandatory for identity but unverified (NFR-7).`

---

### Finding 7: Paging Governance (AD-13) Omitted for 5 of 7 List Epics, Plus Incomplete Epic Mappings (R8, R20, R34, R36)
- **Severity:** Medium
- **Location:** Section `Capability → Architecture Map` (lines 347–359).
- **Evidence:**
  - `tickets.toml:3`: *"R36 (pager) — e1 builds the helper and pages the request queue; every epic pages its own lists."*
  - `architecture-attendta.md:347-359`:
    - Row for e2 (Master Data): Lists `AD-2, AD-4, AD-12` -> **Ommits AD-13** (Members, Event Types, and Venues lists are paged).
    - Row for e3 (Events & Registration): Lists `AD-3, AD-4, AD-5, AD-6, AD-11` -> **Ommits AD-13** (Member events feed, Management events, and registration queue are paged) and **Ommits R8 / AD-12** (Event Cover upload and replacement via `IPrivateFileStore`).
    - Row for e4 (Check-in): Lists `AD-2, AD-4, AD-5, AD-9` -> **Ommits AD-13** (Manual search results and attendance histories are paged).
    - Row for e5 (Administration): Lists `AD-4 (app lock), AD-6, AD-8` -> **Ommits AD-7** (R34 Activity Log viewer) and **Ommits AD-13** (Accounts list and Activity Log viewer are paged).
    - Row for e6 (Certificates): Lists `AD-2, AD-4, AD-12` -> **Ommits AD-13** (Review and Issuance queues are paged) and **Ommits R20** (download links in My Events).
- **Analysis:**
  AD-13 explicitly states: *"Binds: R36 and every list screen"*. Yet in the Capability Map, 5 out of the 7 epics that implement paged list screens fail to list AD-13 in their "Governed by" column. Furthermore, e3's governance omits AD-12 even though e3 manages Event Covers through `IPrivateFileStore` (R8), e5 omits AD-7 for the Activity Log viewer (R34), and e6 omits R20 for My Events certificate download links.
- **Concrete Proposed Fix:**
  Update lines 351–356 of `architecture-attendta.md`:
  - e2: Change Governed by to: `AD-2, AD-4, AD-12, AD-13`.
  - e3 (Event): Change Capability to `R8, R9–R13 event lifecycle and covers`; change Governed by to: `AD-3, AD-4, AD-5, AD-6, AD-11, AD-12, AD-13`.
  - e3 (Registration): Change Governed by to: `AD-4, AD-5, AD-6, AD-11, AD-13`.
  - e4: Change Governed by to: `AD-2, AD-4, AD-5, AD-9, AD-13`.
  - e5: Change Governed by to: `AD-4 (app lock), AD-6, AD-7, AD-8, AD-13`.
  - e6: Change Capability to: `R13 (reviews), R20 (links), R25 (effect), R27–R29 eligibility, issuance, download`; change Governed by to: `AD-2, AD-4, AD-12, AD-13`.

---

### Finding 8: Multi-Epic Co-ownership of `MyEvents` (e3, e4, e6) Lacks Governing View-Composition Pattern (R20)
- **Severity:** Medium
- **Location:** Section `AD-1 — Layered monolith`, Section `AD-2 — One writing module per table`, Section `Capability → Architecture Map` (lines 353, 354, 356).
- **Evidence:**
  - `tickets.toml:5`: *"R20 — e3 My Events registrations and status; e4 QR Pass and attendance state; e6 certificate download links."*
  - `architecture-attendta.md:353, 354`: Lists `MyEvents` under both e3 (`Events & Registration (e3): Registration, MyEvents`) and e4 (`Check-in (e4): Attendance, MyEvents`), but omits it under e6.
  - In ASP.NET Core MVC, `MyEvents` is a single endpoint and view rendered for members.
- **Analysis:**
  Because `MyEvents` is created by e3, then augmented by e4 with QR Pass presentation and attendance check-in status, and then augmented by e6 with certificate PDF download links, three different epics must touch this view. The spine provides no invariant or pattern explaining how this shared view is assembled (e.g. does `MyEventsController` in WebApp aggregate view models from `RegistrationService`, `AttendanceService`, and `CertificateService`, or does e3 define the primary view model with partial view slots?). Without guidance, parallel teams risk overwriting controllers or introducing tight coupling across service boundaries.
- **Concrete Proposed Fix:**
  Add to **Consistency Conventions** under *Routing / Controllers*:
  > `Composite Views | Views that aggregate data across multiple epic boundaries (specifically My Events, R20) are owned at the WebApp layer. MyEventsController invokes IRegistrationService (e3), IAttendanceService (e4), and ICertificateService (e6), assembling a composite MyEventsViewModel. Each contributing epic extends the model and Razor partials without introducing inter-service dependencies in the Application layer.`

---

### Finding 9: Registration Deactivation Preview Contract Missing from AD-6 Interface (R32)
- **Severity:** Low
- **Location:** Section `AD-6 — One participation-cancellation path` (lines 140–149).
- **Evidence:**
  - `initiative-attendta.md:54` (R32): *"Before a membership-status or Account-access deactivation the screen previews affected upcoming Registrations and requires a reason."*
  - `epic-administration.md:27` (R3): *"Before a membership-status or Account-access deactivation the screen previews affected upcoming Registrations and requires a reason."*
  - `architecture-attendta.md:144`: *"Rule: e3 owns IRegistrationCancellationService.CancelAsync(selection, cause, reason, ct). It is the only code that sets a registration to cancelled."*
- **Analysis:**
  AD-6 specifies `IRegistrationCancellationService.CancelAsync` for performing the cancellations during deactivation. However, it does not specify the query contract required by e5 to generate the mandatory pre-deactivation preview of affected registrations.
- **Concrete Proposed Fix:**
  In **AD-6 Rule**, add to line 144:
  > *"`IRegistrationCancellationService` also declares `GetUpcomingAffectedRegistrationsAsync(int userId, CancellationToken ct)` to supply Administration (e5) with the preview required by FR-32 prior to deactivation."*

---

### Finding 10: R34 Activity Log Viewer Query and Administrator Policy Rules Missing from AD-7
- **Severity:** Low
- **Location:** Section `AD-7 — Audit writer and vocabulary` (lines 150–160).
- **Evidence:**
  - `initiative-attendta.md:56` (R34): *"An Administrator views a read-only, newest-first Activity Log showing actor, action, affected record, time and reason."*
  - `architecture-attendta.md:152`: *"Binds: R33, R34; NFR-3; all mutating stories"*.
  - `architecture-attendta.md:154-159`: The text describes exclusively the write path (`IActivityLogWriter.Append`) and DB trigger. It contains zero rules or contracts governing the read/viewer path for R34.
- **Analysis:**
  While AD-7 claims to bind R34 in its header, the rule text completely ignores reading the Activity Log. Critical constraints (Administrator policy restriction, read-only query, deterministic newest-first ordering `occurred_at DESC, log_id DESC`, projection to a view model, and fixed 25-row paging) are absent from the rule.
- **Concrete Proposed Fix:**
  Add a subsection to **AD-7 Rule**:
  > *- **Activity Log Viewer (R34):** e5 implements `IActivityLogQueryService.GetPagedLogsAsync(PageRequest request, CancellationToken ct)` returning `PagedResult<ActivityLogItemViewModel>`. Ordering is fixed at `occurred_at DESC, log_id DESC`. Endpoints viewing the log are authorized strictly under the `Administrator` policy.*

---

## Reconciliation Matrix

| Item | Initiative / Ticket Baseline | Spine Status | Reconciliation Assessment / Gap |
| --- | --- | --- | --- |
| **R1** | Account request validation, hash-only storage, acknowledgment | Covered in AD-15, AD-9 | Governed; pending filtered indexes should be explicit in AD-10. |
| **R2** | Account request review, approval creates account & profile, audited | Split across AD-2, AD-3, AD-4, AD-7 | **Contradiction:** Spine AD-2 assigns member creation to e1, while `epic-platform-access.md` assigns to e2. |
| **R3** | Email login, generic failures, expiring cookie, per-request checks | Covered in AD-8 | Generic failure message rule omitted from AD-8 text. |
| **R4** | Member read-only self profile | Covered in AD-8, Capability Map | Governed. |
| **R5** | Member profile edits (e2) vs. membership status (e5); reason for student number | Covered in AD-2, Conventions | Governed, but `members` lock missing from AD-4. |
| **R6** | Event Types create/edit/toggle, description NOT NULL | Covered in AD-2, AD-10 | Governed. |
| **R7** | Venues create/edit/toggle with capacity guard against linked events | Partial in AD-2, AD-4, AD-12 | Cross-module query for linked events limit lacks explicit rule. |
| **R8** | 2 MiB JPEG/PNG shared image store; e2 builds, e3 reuses for covers | Covered in AD-12 | e3 mapping omits R8 and AD-12 in Capability Map. |
| **R9** | Draft event creation, capacity <= venue, hidden from members | Covered in AD-2, AD-5 | Governed. |
| **R10** | Event publication revalidates active type/venue, cover required | Covered in AD-5 | Governed. |
| **R11** | Published event edits restricted to title, description, cover | Covered in AD-2 | Governed. |
| **R12** | Event cancellation cascades to registrations, closes check-in | Covered in AD-4, AD-6 | Governed. |
| **R13** | Event completion closes check-in, seeds reviews in transaction | Covered in AD-2 | Governed. |
| **R14** | Eligible participant signup before deadline, snapshot | Covered in AD-4, AD-5, schema | Governed. |
| **R15** | Signup deadline enforcement per request | Covered in AD-5, AD-11 | Governed. |
| **R16** | Registration approval/decline/admin cancel, queue priority | Covered in AD-4, AD-5, AD-6 | Governed. |
| **R17** | Waitlist promotion earliest eligible, serialized, never automatic | Covered in AD-4, AD-5 | Governed. |
| **R18** | Member self-cancellation before cutoff and valid attendance | Covered in AD-4, AD-5, AD-6 | Governed. |
| **R19** | Event start cutoff freezes reviews, signups, and cancellations | Covered in AD-5, AD-11 | Governed. |
| **R20** | My Events shows registrations, passes, attendance, certificate links | Split across e3, e4, e6 | **Gap:** Multi-epic composition pattern missing; e6 row omits R20. |
| **R21** | On-demand QR Pass PNG rendering from token | Covered in AD-18, Conventions | Governed. |
| **R22** | Check-in window open/close toggle | Covered in AD-2 | Governed. |
| **R23** | QR Pass door scan, idempotency, duplicate prevention | Covered in AD-2, AD-4, AD-9 | Governed. |
| **R24** | Manual participant search and check-in | Covered in AD-2, AD-4, AD-9 | Governed. |
| **R25** | Attendance invalidation blocks on issuance, updates review to not_eligible | Omitted in AD-2 cross-module writes | **Critical Gap:** Missing cross-module write in AD-2; circular dependency `CHK <--> CERT`. |
| **R26** | Member views attendance history with invalidated marked | Covered in AD-5 | Governed. |
| **R27** | Certificate review approval/decline, re-checks valid attendance | Covered in AD-4 | Governed. |
| **R28** | Certificate issuance (A4 landscape, Playwright, snapshot, atomic) | Covered in AD-4, AD-12, Stack | Governed. |
| **R29** | Streamed certificate download, active account check | Covered in AD-8, AD-12 | Governed. |
| **R30** | Participation reports (KPIs, no-shows, rate, distributions, paged) | Covered in AD-5, AD-13 | Governed. |
| **R31** | Role assignment, last admin protection | Covered in AD-4 | Governed. |
| **R32** | Deactivation preview, upcoming registration cancellation | Partial in AD-4, AD-6, AD-8 | Preview query contract missing from AD-6. |
| **R33** | Activity Log transactional append, sanitization, trigger enforcement | Covered in AD-7 | Governed. |
| **R34** | Read-only newest-first Activity Log viewer for administrators | Mentioned in AD-7 header | Query contract missing from AD-7; AD-7 omitted from e5 in Capability Map. |
| **R35** | One-time admin setup script, Argon2id hash | Covered in AD-7, AD-15 | Governed. |
| **R36** | Fixed 25-row pagination, deterministic sort, page fallbacks | Covered in AD-13 | Omitted from "Governed by" in Capability Map for e2, e3, e4, e5, e6. |
| **NFR-1** | No login throttling or lockout | Ticket Constraint | **Dropped:** Spine omits the ban on throttling/lockout. |
| **NFR-2** | Database uniqueness enforcement for 5 core entities | Ticket Constraint | **Dropped:** Spine AD-10 omits explicit list of uniqueness constraints. |
| **NFR-3** | No hard deletes of business records | Ticket Constraint | **Dropped:** Spine omits hard-delete prohibition for business entities. |
| **Voice & Tone** | Direct labels, no misleading promises of email or auto-promotion | EXPERIENCE.md | **Dropped:** Spine omits UX voice/tone constraints from feedback rules. |
