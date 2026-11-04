# Architecture Spine Reconciliation Review — Companion Documents, Brownfield Code & Canonical Schema

- **Artifact under review:** `_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md`
- **Reviewed against:**
  - Prose architecture companion: `docsss/architecture.md` (v1.1)
  - PRD Addendum: `_bmad-output/initiative-attendta/prd-attendta/addendum.md` (Sections A, B.1–B.12, E)
  - System Specification: `docsss/AttendTa-System-Specification.md` (v2.1, Sections 4, 9–14, 17–19)
  - Canonical database schema & seeds: `database/attendta-mvp-schema.sql`, `database/seed-event-types.sql`, `database/seed-local-admin.sql`
  - Brownfield codebase: `ASI.Basecode.WebApp`, `ASI.Basecode.Services`, `ASI.Basecode.Data`, `ASI.Basecode.Resources`
- **Reviewer:** `ReconcileCompanions`
- **Date:** 2026-11-04
- **Verdict:** **CHANGES REQUIRED** (2 Critical, 5 High, 5 Medium, 2 Low findings)

---

## Executive Summary

This review reconciles the AttendTa Architecture Spine (`architecture-attendta.md`, AD-1 through AD-18) against its authoritative companion specifications (`docsss/architecture.md`, `addendum.md`, `SPEC` v2.1), the current brownfield ASP.NET Core codebase, and the SQL Server database schema.

The spine provides a solid foundation for transaction scripts (AD-3), row locking (AD-4), canonical facts (AD-5), and cookie authentication (AD-8). However, deep reconciliation uncovers significant discrepancies in three areas:

1. **Companion Contradictions & Dropped Decisions (Category A):**
   - **`users.full_name` Update Ownership Dropped:** SPEC §4 explicitly mandates that Officers/Admins can edit member Full Names, stored in `users.full_name`. In AD-2, `users` is assigned only to e1 (create) and e5 (`role`, `is_active`). e2 is assigned `members (profile fields)`, but because `members` has no name column, updating a member's name is either architecturally prohibited or orphaned.
   - **Silent Dropping of Functional Deferrals:** The spine's "## Deferred" table lists only 8 technical engineering items, completely dropping the explicit MVP scope exclusions defined in SPEC §4, §11.1, §14.2, §15 and Addendum §A/§B (password reset/change, member profile editing, re-check-in after invalidation, certificate decline reopening, report exports, login lockout).
   - **Initial Admin Bootstrap Ownership Shift:** Companion documents (`architecture.md` §10, Addendum §C, SPEC §1.4) explicitly assign initial administrator provisioning to Member 5 (Pinca / Administration / e5). The spine moves R35 to Platform Access (e1) without explaining the transfer.

2. **Brownfield Migration Gaps (Category B):**
   - **`members` Table Ownership Fragmentation:** Brownfield `MemberController` and `MemberService` currently implement `ToggleStatus` in Master Data (e2). AD-2 reassigns `members (membership status)` to Administration (e5), but does not state the migration: who removes the brownfield endpoint, who owns `IMemberRepository`, and how e5 executes updates without violating AD-2.
   - **Service Actor Parameter Convention:** Brownfield services uniformly take `int actorUserId` passed from `ControllerBase.GetActorUserId()`. AD-8 requires services to inject `ICurrentActor`, but fails to assign ownership of refactoring existing Master Data service signatures and controllers.
   - **Storage Directory Relative Path Inconsistency:** Brownfield `VenueImageStorage` stores relative paths without the `uploads/` prefix (e.g. `venues/{guid}.jpg`), whereas AD-12 defines areas as `uploads/venues`, `uploads/events`, creating a path mismatch.

3. **Schema Facts & Script Conflicts (Category C):**
   - **Fatal DB Name Guard in Canonical Schema Script:** `database/attendta-mvp-schema.sql` hardcodes `IF DB_NAME() <> N'AttendTaMVP_Test' THROW 51000`. This directly breaks AD-16 and AD-17, which mandate that integration test runners create fresh databases per test class and CI runs in a container.
   - **Un-migrated State of Canonical Schema:** AD-10 declares `database/attendta-mvp-schema.sql` the "single schema source", but the script currently uses string CHECK constraints (no lookup tables), lacks `certificates.issuer_name_snapshot`, has `event_types.description` nullable, and lacks the audit trigger. Furthermore, `seed-event-types.sql` fails if `description` is made NOT NULL because it does not insert descriptions.
   - **Lookup Foreign Key Column Naming Ambiguity:** AD-10 specifies lookup tables (`<x>_id tinyint`), but the spine continues referencing string column names (`users.role`, `status`, `actor_type`), leaving FK column names unspecified.

---

## Detailed Findings

### Category (a): Decisions in Companion Documents Contradicted or Dropped Without Deferring

#### Finding A1: Missing `users.full_name` Update Ownership in AD-2 Table
- **Severity:** High
- **Location:** Spine Section `AD-2 — One writing module per table` (lines 76–88).
- **Evidence:**
  - `docsss/AttendTa-System-Specification.md:108`: *"Full name, course/year, section, mobile, membership type/status: Officer/admin for approved member records."*
  - `docsss/AttendTa-System-Specification.md:114`: *"Current name/email belong to `users`; student number and academic/member data belong to `members`. Profile changes do not rewrite signup snapshots or issued certificates."*
  - `database/attendta-mvp-schema.sql:27, 64-79`: `[full_name]` is defined on `[users]`, NOT on `[members]`. `[members]` contains only academic/student metadata and a FK `user_id`.
  - `architecture-attendta.md:78-82`:
    - `users (create, credentials), account_requests` -> Platform Access (e1)
    - `users.role, users.is_active` -> Administration (e5)
    - `members (profile fields)` -> Master Data (e2)
  - `architecture-attendta.md:74`: *"Only the module listed below may call write methods for the listed table or columns, and only from its own service. Every other module goes through that owner's service interface."*
- **Analysis:**
  When an Officer or Administrator edits a member's profile in Master Data (e2), SPEC §4 requires updating the member's full name. Because `full_name` resides on `users`, and AD-2 strictly limits writes to `users` to Platform Access (e1) for creation and Administration (e5) for role/is_active, Master Data (e2) is structurally prohibited by AD-2 from updating `users.full_name`. The spine leaves `users.full_name` updates completely unassigned and provides no cross-module service contract between e2 and e1 for user profile name updates.
- **Concrete Proposed Fix:**
  In **AD-2**, update the writing module table (line 78–82) and add a cross-module write rule:
  ```markdown
  | Table / column | Writing module (epic) |
  | --- | --- |
  | `users` (create, credentials), `account_requests` | Platform Access (e1) |
  | `users.full_name` | Master Data (e2) through `IUserProfileService` (provided by e1) |
  | `users.role`, `users.is_active` | Administration (e5) |
  ```
  And under *Cross-module writes* (line 90), add:
  - **Member profile updates (R4).** e2 owns the member profile edit use case. When updating a member's full name, e2 calls `IUserProfileService.UpdateFullNameAsync(userId, fullName, ct)` provided by e1, executing inside e2's transaction.

---

#### Finding A2: Silent Dropping of Functional Scope Exclusions and Deferrals
- **Severity:** High
- **Location:** Spine Section `## Deferred` (lines 360–372).
- **Evidence:**
  - `docsss/AttendTa-System-Specification.md:102, 104, 110-111`: *"Do not add login throttling or lockout in this MVP... Known-password changes and forgotten-password recovery are deferred; no change-password form/action or substitute administrator reset workflow is included... Login email: Read-only; email-change workflow deferred. Existing password: Change/reset workflows deferred."*
  - `docsss/AttendTa-System-Specification.md:114`: *"Members view their profile but cannot edit profile details in this release."*
  - `docsss/AttendTa-System-Specification.md:233`: *"Re-check-in after invalidation is deferred: warn before correction and reject a subsequent check-in against the retained invalidated row."*
  - `docsss/AttendTa-System-Specification.md:270`: *"Reopening a declined decision is deferred: no `not_eligible -> pending_approval` action. Warn before decline that the decision cannot be reopened in this release."*
  - `docsss/AttendTa-System-Specification.md:274`: *"No template editor, bulk issuance, email, revocation or correction/reissue."*
  - `docsss/AttendTa-System-Specification.md:293`: *"CSV/PDF report exports are deferred (certificate PDF downloads are distinct)."*
  - `docsss/architecture.md:220`: *"Remove unsupported bulk issuance, QR-print, password-change, manual-account-creation, and report-export actions."*
  - `_bmad-output/initiative-attendta/prd-attendta/addendum.md:50`: *"there is no student-number login. Keep the unique member student-number index for profile identity and manual check-in lookup only; do not add an authentication lookup."*
  - `architecture-attendta.md:360-372`: The spine's "Deferred" table contains only 8 items, all technical engineering details (QR library, Argon2 tuning, certificate number format, renderer limits, scanner JSON fields, report read isolation, HTTPS certificate, log retention).
- **Analysis:**
  A core purpose of an architecture spine for 8 parallel builders is preventing scope creep and conflicting assumptions. In previous iterations, developers added password reset screens, member self-edit forms, report export buttons, and re-check-in flows. The SPEC and PRD addendum explicitly documented these as deferred non-goals. By omitting them from the spine's Deferred section, parallel epic builders are given no authoritative signal that these workflows must not be built.
- **Concrete Proposed Fix:**
  In section `## Deferred`, add a dedicated table or section `### Scope Deferrals & Exclusions (SPEC §4, §11.1, §14.2, §15)`:
  ```markdown
  | Excluded / Deferred Workflow | Authoritative Reference | Invariant Rule |
  | --- | --- | --- |
  | Password change & password reset | SPEC §4; architecture.md §9 | No change-password or reset endpoints/forms; initial credentials hashed only. |
  | Member profile self-editing | SPEC §4 | Members have read-only profile view; edits performed only by Officer/Admin. |
  | Re-check-in after invalidation | SPEC §11.1; Addendum B.1 | Retained invalidated attendance row rejects subsequent check-in attempts. |
  | Reopening declined certificate reviews | SPEC §14.2; DBML v2.1 | `not_eligible` is terminal; no action to return to `pending_approval`. |
  | Report CSV/PDF exports | SPEC §15; architecture.md §9 | On-screen tables and totals only; no export endpoints or report files. |
  | Student-number login & email verification | SPEC §4, §18.1; Addendum B.6 | Login is strictly email/password; student number is not an auth identifier. |
  | Bulk certificate issuance & template editor | SPEC §14.3; architecture.md §9 | Single-certificate issuance from attendee management using fixed Razor template. |
  | Login throttling & account lockout | SPEC §4; Addendum A, E | Omitted from MVP; generic credential error on failed authentication. |
  ```

---

#### Finding A3: Ownership Transfer of Initial Administrator Setup (R35) Without Explanation
- **Severity:** Medium
- **Location:** Spine Section `Capability → Architecture Map` (line 349) vs `docsss/architecture.md:232`, `_bmad-output/initiative-attendta/prd-attendta/addendum.md:76, 86`, `docsss/AttendTa-System-Specification.md:76`.
- **Evidence:**
  - `docsss/architecture.md:232`: *"Pinca owns one-time initial-administrator provisioning; use the real Authentication password hasher for the supplied credentials. No shared/default plaintext password or committed hash."*
  - `_bmad-output/initiative-attendta/prd-attendta/addendum.md:76`: *"Certificates, Reports & Administration, including initial administrator setup and role/access management -> Member 5 (Pinca)"*
  - `_bmad-output/initiative-attendta/prd-attendta/addendum.md:86`: *"Pinca owns administrator bootstrap, role/access administration, account deactivation/reactivation screens and activity-log administration."*
  - `architecture-attendta.md:349`:
    `| R1–R3, R35 account request, approval, login, setup | Platform Access (e1): Account, AccountRequest | AD-3, AD-4, AD-7, AD-8, AD-15 |`
  - `architecture-attendta.md:355`:
    `| R5 (membership status), R31, R32, R34 roles, deactivation, log viewer | Administration (e5) | AD-4 (app lock), AD-6, AD-8 |`
- **Analysis:**
  All companion planning documents explicitly assign R35 (initial administrator setup / bootstrap) to Member 5 (Pinca), who owns Administration (e5). The spine silently moves R35 to Platform Access (e1 / Member 1). While placing the seed script tooling with the hasher in e1 is technically reasonable (since e1 builds `IPasswordHasher`), transferring capability ownership across different teammates without recording the rationale or coordinating with the Administration epic creates team ownership conflicts.
- **Concrete Proposed Fix:**
  In section `Capability → Architecture Map`, clarify the division of responsibility for R35:
  ```markdown
  | Capability / Area | Lives in | Governed by |
  | --- | --- | --- |
  | R1–R3 account request, approval, login; R35 admin hasher CLI tool | Platform Access (e1): Account, AccountRequest | AD-3, AD-4, AD-7, AD-8, AD-15 |
  | R5 (membership status), R31, R32, R34 roles, deactivation; R35 admin execution/verification | Administration (e5) | AD-4 (app lock), AD-6, AD-8 |
  ```
  Note in the memlog that e1 delivers the password hasher utility and canonical seed script, while e5/Pinca retains ownership of administrator provisioning verification on the demo host.

---

#### Finding A4: Preservation of Existing Cancellation Reasons During Bulk Cancellation (SPEC §13 vs AD-6)
- **Severity:** Medium
- **Location:** Spine Section `AD-6 — One participation-cancellation path` (lines 140–149).
- **Evidence:**
  - `docsss/AttendTa-System-Specification.md:254`: *"Atomically mark cancelled, cancel pending/confirmed/waitlisted registrations, clear tokens, close check-in and audit. Preserve previous reasons for already-cancelled rows."*
  - `architecture-attendta.md:144-145`: *"Rule: e3 owns `IRegistrationCancellationService.CancelAsync(selection, cause, reason, ct)`. It is the only code that sets a registration to cancelled. Each call clears `qr_token`, records the reason, actor and time, writes one `registration.cancelled` audit entry per row, and never promotes."*
- **Analysis:**
  When an event is cancelled by an Officer (or when a member with multiple registrations is deactivated), bulk cancellation is invoked across all event registrations. If `CancelAsync` unconditionally updates reason, actor, and timestamp on all passed rows, it will overwrite the historical reasons of participants who had previously self-cancelled or been declined. SPEC §13 explicitly protects these historical reasons.
- **Concrete Proposed Fix:**
  In **AD-6**, amend bullet 1 (line 145):
  > - Each call clears `qr_token`, records the reason, actor and time, writes one `registration.cancelled` audit entry per row, and never promotes. **For bulk operations (such as event cancellation), registrations already in `cancelled` status retain their existing cancellation reason, actor, and timestamp; only active rows (`pending`, `confirmed`, `waitlisted`) receive the new cancellation metadata and audit.**

---

#### Finding A5: Event Cover Photo Invariant as a Publication Guard (SPEC §14.5 vs AD-12)
- **Severity:** Medium
- **Location:** Spine Section `AD-12 — Private file store` (lines 200–209) and Section `AD-5 — Canonical participation facts` (lines 125–139).
- **Evidence:**
  - `docsss/AttendTa-System-Specification.md:284`: *"`users` has no image-path field. `events.image_path` is nullable for drafts but a nonblank validated cover is required for published/completed events; `venues.image_path` is non-null/nonblank."*
  - `_bmad-output/initiative-attendta/prd-attendta/addendum.md:47`: *"Event cover rule: published and completed Events require a cover; cancelled drafts may have none. Cancelled Events that were previously published keep their cover (implied by 'replacement only')."*
  - `docsss/AttendTa-System-Specification.md:212` (Cutoff Matrix): *"Publish: Draft, required cover/master validation, deadline still future."*
  - `database/attendta-mvp-schema.sql:350`: Extended property note: *"Optional for draft only; validated nonblank cover required for published/completed events."*
  - `architecture-attendta.md:200-209` (AD-12): Focuses exclusively on storage roots, magic byte validation, 2 MiB limits, replacement order, and download streaming. It does not state the cross-table lifecycle rule that `events.image_path` cannot be null or blank when transitioning to `published`.
- **Analysis:**
  Because the database schema allows `events.image_path` to be NULL (line 122, to support draft creation), the requirement that published events have a valid cover is purely a service-layer lifecycle guard. If AD-5 and AD-12 omit this invariant, e3 could implement event publishing without validating that a cover photo exists, violating SPEC §10, §14.5 and Addendum B.3.
- **Concrete Proposed Fix:**
  In **AD-5**, add a row to the Canonical Participation Facts table (line 131):
  ```markdown
  | Valid Event Publication | event is draft, now < registration_deadline, and `image_path` is not null and exists in `uploads/events` |
  ```
  And in **AD-12**, add a bullet:
  > - Event cover photos are optional for drafts, but transition to `published` requires a validated relative path to an existing file in `uploads/events`.

---

#### Finding A6: Cross-Table Capacity Check (`participant_limit <= venues.capacity`)
- **Severity:** Low
- **Location:** Spine Section `AD-4 — Lock order and serialization` (lines 104–124) and Section `AD-2` (lines 71–94).
- **Evidence:**
  - `_bmad-output/initiative-attendta/prd-attendta/addendum.md:48`: *"Capacity limit enforcement: `participant_limit <= venues.capacity` is a service-layer rule (FR-9, FR-7). It cannot be enforced through foreign keys."*
  - `docsss/AttendTa-System-Specification.md:359`: *"Cross-table capacity and required existing cover/file rules require service/transaction checks or suitable database logic."*
  - `architecture-attendta.md:115-116`: AD-4 requires locking `venues` before `events`.
- **Analysis:**
  AD-4 correctly places `venues` (step 5) before `events` (step 6) in the lock hierarchy, specifically to allow validating venue capacity during event creation/editing. However, the business invariant itself (`participant_limit <= venues.capacity`) is never articulated in the spine, leaving it possible for an implementer to check venue existence without enforcing the capacity ceiling.
- **Concrete Proposed Fix:**
  In **AD-4**, add to the lock rules:
  > - Event creation and edit operations lock the referenced `venues` row before the `events` row to enforce the invariant `events.participant_limit <= venues.capacity`.

---

### Category (b): Spine Rules That Contradict Brownfield Code Without Stating the Migration

#### Finding B1: Fragmentation of `members` Table Ownership in AD-2 vs Brownfield `MemberController` / `MemberService`
- **Severity:** High
- **Location:** Spine Section `AD-2 — One writing module per table` (lines 74, 80–82) vs `ASI.Basecode.WebApp/Controllers/MemberController.cs:70-82`, `ASI.Basecode.Services/Services/MemberService.cs:43-75, 107-124`.
- **Evidence:**
  - `architecture-attendta.md:74`: *"Each table has one repository class in Data. Only the module listed below may call write methods for the listed table or columns, and only from its own service. Every other module goes through that owner's service interface."*
  - `architecture-attendta.md:80-82`:
    - `members (create on approval)` -> Platform Access (e1)
    - `members (profile fields)` -> Master Data (e2)
    - `members (membership status)` -> Administration (e5)
  - Brownfield `MemberController.cs:70-82`: Contains `ToggleStatus(int id)` which calls `_memberService.ToggleStatus(id, GetActorUserId())`.
  - Brownfield `MemberService.cs:43`: Contains `CreateMemberFromApprovedAccountRequest(int userId, MemberViewModel model, int actorUserId)`.
  - Brownfield `MemberService.cs:107`: Contains `ToggleStatus(int id, int actorUserId)` updating `member.Status`.
  - Brownfield `MemberRepository.cs:57-67`: Holds `AddMember`, `UpdateMember`.
- **Analysis:**
  Brownfield code currently houses the entire member lifecycle (`CreateMemberFromApprovedAccountRequest`, `UpdateMember`, `ToggleStatus`) in Master Data (`MemberService` and `MemberController`).
  The spine shatters `members` across three epics (e1 creates, e2 edits fields, e5 changes status). However:
  1. It contradicts brownfield code by moving membership status to e5 without directing the removal of `MemberController.ToggleStatus` from Master Data.
  2. It violates AD-2's own rule ("Only the module listed below may call write methods... and only from its own service"): if `MemberRepository` is in Data, who owns `MemberRepository`? If e2 owns `MemberRepository`, e1 and e5 cannot call write methods on it directly. If e1 calls `MemberService.CreateMemberFromApprovedAccountRequest`, e1 depends on e2 (inverting the build order where e1 is built before e2).
- **Concrete Proposed Fix:**
  In **AD-2**, explicitly document repository ownership and the cross-module calling paths:
  1. State that `IMemberRepository` in Data is owned by **Master Data (e2)**.
  2. For account approval, e1 defines an interface `IMemberCreationService` in `Services/Interfaces`. e2 implements it. (Or e1 owns the initial create method on `MemberRepository`).
  3. For membership status, e5 calls `IMemberStatusService.SetMembershipStatusAsync(memberId, status, reason, ct)` implemented by e2/e5.
  4. Explicitly state the brownfield cleanup:
     > - **Migration:** e2 deletes `MemberController.ToggleStatus` and `MemberService.ToggleStatus`. Membership status changes are migrated entirely to e5 (Administration), which coordinates deactivation with `IRegistrationCancellationService` per AD-6.

---

#### Finding B2: Service Actor Parameter Convention (`int actorUserId`) vs `ICurrentActor` Migration
- **Severity:** High
- **Location:** Spine Section `AD-8 — Identity, session and authorization` (line 169) vs Brownfield `MemberService.cs`, `EventTypeService.cs`, `VenueService.cs`, `ControllerBase.cs:69-73`.
- **Evidence:**
  - `architecture-attendta.md:169`: *"Services get the actor from `ICurrentActor`. Request models never carry actor ids or roles."*
  - `ASI.Basecode.WebApp/Mvc/ControllerBase.cs:69-73`:
    ```csharp
    protected int GetActorUserId()
    {
        var claimValue = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claimValue, out var userId) ? userId : 0;
    }
    ```
  - `ASI.Basecode.Services/Services/MemberService.cs:43, 77, 107`: Mutating methods take `int actorUserId` as an explicit parameter.
  - `ASI.Basecode.Services/Services/EventTypeService.cs:36, 63`: `AddEventType(model, int actorUserId)`, `UpdateEventType(model, int actorUserId)`.
  - `ASI.Basecode.Services/Services/VenueService.cs:36, 61`: `AddVenue(model, int actorUserId)`, `UpdateVenue(model, int actorUserId)`.
  - Controllers pass `GetActorUserId()` into these service methods.
- **Analysis:**
  The spine correctly mandates that services obtain the ambient actor via `ICurrentActor` rather than trusting client-supplied IDs. However, in the existing brownfield code, the pattern is for *controllers* to read claims via `ControllerBase.GetActorUserId()` and pass `int actorUserId` as a method argument.
  The spine does not state who migrates existing Master Data services and controllers. If e1 introduces `ICurrentActor` but does not update Master Data, two competing actor patterns will coexist across the application, and e2 builders will be confused about whether to keep `actorUserId` parameters.
- **Concrete Proposed Fix:**
  In **AD-8**, add a migration statement:
  > - **Migration:** e1 introduces `ICurrentActor` and its `ClaimsPrincipal` WebApp adapter. When e2 touches `EventTypeService`, `VenueService`, and `MemberService`, it refactors them to inject `ICurrentActor`, removing `int actorUserId` from service method signatures. `ControllerBase.GetActorUserId()` is deprecated and removed once all controllers transition.

---

#### Finding B3: Storage Directory Relative Path Scheme (`uploads/venues` vs `venues`)
- **Severity:** Medium
- **Location:** Spine Section `AD-12 — Private file store` (line 204) vs `ASI.Basecode.WebApp/Storage/VenueImageStorage.cs:24, 66`.
- **Evidence:**
  - `architecture-attendta.md:204`: *"One `IPrivateFileStore` (declared in Services, implemented in `WebApp/Storage`, generalizing `VenueImageStorage`), rooted at `Common:SetupRoot`. Its areas are `uploads/venues`, `uploads/events`, `certificates` and `temporary`."*
  - `architecture-attendta.md:207`: *"SQL stores relative paths only."*
  - `ASI.Basecode.WebApp/Storage/VenueImageStorage.cs:24`:
    `_storageRoot = Path.GetFullPath(Path.Combine(setupRoot, "uploads"));`
  - `ASI.Basecode.WebApp/Storage/VenueImageStorage.cs:66`:
    `var relativePath = $"venues/{Guid.NewGuid():N}{extension}";`
  - `database/attendta-mvp-schema.sql:346`: Extended property: *"Required relative local image path."*
- **Analysis:**
  In brownfield `VenueImageStorage`, the storage root is hardcoded to `<SetupRoot>/uploads`, and the relative path saved to the database is `venues/<guid>.jpg`.
  Under AD-12, `IPrivateFileStore` is rooted at `<SetupRoot>`, with areas `uploads/venues`, `uploads/events`, `certificates`, and `temporary`. If relative paths in SQL are relative to `Common:SetupRoot`, venue photos would be stored as `uploads/venues/<guid>.jpg`, while certificates would be `certificates/<guid>.pdf`.
  This creates an incompatibility between brownfield code/data (which assumes `venues/...`) and target AD-12 (which specifies area `uploads/venues`).
- **Concrete Proposed Fix:**
  In **AD-12**, standardize the relative path convention:
  > - SQL stores relative paths rooted at `Common:SetupRoot` with the area prefix included: `uploads/venues/<guid>.<ext>`, `uploads/events/<guid>.<ext>`, `certificates/<guid>.pdf`, and `temporary/<guid>.<ext>`.
  > - **Migration:** e1 updates `VenueImageStorage` (migrated to `IPrivateFileStore`) so that returned relative paths include the `uploads/` prefix, aligning existing and new storage operations.

---

#### Finding B4: Repositories Migrated Off `SaveChanges` vs Synchronous Service Signatures
- **Severity:** Medium
- **Location:** Spine Section `AD-3 — Caller-owned unit of work` (line 102) and Section `Consistency Conventions` (line 278) vs Brownfield `MemberRepository.cs`, `EventTypeRepository.cs`, `VenueRepository.cs`.
- **Evidence:**
  - `architecture-attendta.md:102`: *"e1 migrates the existing master-data repositories off their per-write `SaveChanges`."*
  - `architecture-attendta.md:278`: *"New and cross-module service and repository methods are `async` and take a `CancellationToken`; existing sync master-data methods convert when touched"*
  - Brownfield repositories and services (`MemberRepository`, `EventTypeRepository`, `VenueRepository`, `MemberService`, `EventTypeService`, `VenueService`) are completely synchronous:
    - `public void AddVenue(Venue venue) { GetDbSet<Venue>().Add(venue); UnitOfWork.SaveChanges(); }`
- **Analysis:**
  If e1 migrates `EventTypeRepository`, `VenueRepository`, and `MemberRepository` off their per-write `SaveChanges`, what does their method signature become? If e1 converts them to `AddVenueAsync(..., CancellationToken ct)`, then the synchronous services `EventTypeService.AddVenue` and `VenueService.AddVenue` will fail to compile unless they are also converted to `async`.
  Leaving "convert when touched" ambiguous while assigning e1 to migrate repositories off `SaveChanges` will break the solution build if e1 modifies repositories without converting the calling services.
- **Concrete Proposed Fix:**
  In **AD-3**, clarify the repository migration scope for e1:
  > - When e1 migrates existing master-data repositories off `SaveChanges`, e1 converts the mutating methods on `IEventTypeRepository`, `IVenueRepository`, and `IMemberRepository` to `async Task Add...Async(entity, ct)` and updates `EventTypeService`, `VenueService`, and `MemberService` to `async Task` transaction scripts wrapped in `IUnitOfWork.ExecuteInTransactionAsync`.

---

#### Finding B5: Obsolete Brownfield Shims (`MemberAccountSummary`, `SessionManager`, `PathManager`)
- **Severity:** Low
- **Location:** Spine Section `AD-8`, `AD-10`, `AD-18` vs `ASI.Basecode.Data/AsiBasecodeDbContext.cs:87-94`, `ASI.Basecode.Data/Repositories/MemberRepository.cs:23-27`, `ASI.Basecode.Services/Manager/SessionManager.cs`, `ASI.Basecode.Data/PathManager.cs`.
- **Evidence:**
  - `AsiBasecodeDbContext.cs:87-94`: Maps keyless entity `MemberAccountSummary` to raw SQL `SELECT [user_id], [full_name], [email] FROM [dbo].[users]`.
  - `MemberRepository.cs:23-27`: `GetMemberRecords()` joins `Member` with `MemberAccountSummary` to work around the unmapped legacy `User` entity.
  - `ASI.Basecode.Services/Manager/SessionManager.cs`: Wrapper around `Microsoft.AspNetCore.Http.ISession`.
  - `ASI.Basecode.Data/PathManager.cs`: Obsolete static file path manager in the Data project.
  - `ASI.Basecode.WebApp/Startup.cs:94`: `services.AddRazorPages().AddRazorRuntimeCompilation();`.
- **Analysis:**
  `MemberAccountSummary` was a temporary hack created because `AsiBasecodeDbContext` only had the legacy starter `User` table (with varchar IDs). When e1 introduces the canonical `users` mapping per AD-10, `MemberAccountSummary` becomes dead code. Similarly, `SessionManager` and `UseSession` are rendered obsolete by cookie authentication (AD-8), and `PathManager` in Data violates layer separation. The spine does not direct their deletion, leaving dead weight in the codebase.
- **Concrete Proposed Fix:**
  In **AD-10** and **AD-18**, explicitly list brownfield shims to remove:
  > - **Migration:** Upon delivering the canonical `users` mapping, e1 removes `MemberAccountSummary` from `AsiBasecodeDbContext`, updates `MemberRepository.GetMemberRecords` to join `Users` directly, and deletes `PathManager.cs` from `ASI.Basecode.Data`. e1 removes `SessionManager.cs` and deletes `services.AddSession()` / `app.UseSession()` from `Startup.cs`.

---

### Category (c): Schema Facts the Spine Gets Wrong (Column Names, Codes, Indexes)

#### Finding C1: Fatal Database Name Guard in Canonical Script (`database/attendta-mvp-schema.sql`)
- **Severity:** Critical
- **Location:** `database/attendta-mvp-schema.sql:17-19` vs Spine Section `AD-10` (line 190), `AD-16` (line 248), `AD-17` (line 260).
- **Evidence:**
  - `database/attendta-mvp-schema.sql:17-19`:
    ```sql
    IF DB_NAME() <> N'AttendTaMVP_Test'
        THROW 51000, 'This schema script may only run in AttendTaMVP_Test.', 1;
    GO
    ```
  - `architecture-attendta.md:190`: *"`database/attendta-mvp-schema.sql` is the single schema source. e1 delivers all ten tables, the lookups, `event_types.description NOT NULL`, `certificates.issuer_name_snapshot` and the audit trigger."*
  - `architecture-attendta.md:248` (AD-16): CI runs against `mcr.microsoft.com/mssql/server:2025-latest` container.
  - `architecture-attendta.md:260` (AD-17): *"`tests/ASI.Basecode.IntegrationTests` runs against the SQL Server named in `ATTENDTA_TEST_SQL`. It creates a fresh database from the canonical schema for each test class."*
- **Analysis:**
  The canonical schema script contains a hardcoded guard that aborts execution if the current database name is anything other than `AttendTaMVP_Test`.
  Under AD-17, integration tests run in parallel and create dynamically named isolated databases per test class (e.g. `AttendTa_Test_<Guid>`). Furthermore, CI runs against a container database. As currently written, running `attendta-mvp-schema.sql` during integration test database creation or CI deployment will immediately throw error 51000 and fail.
- **Concrete Proposed Fix:**
  In **AD-10** and **AD-17**, mandate that e1 removes the rigid `DB_NAME()` guard:
  > - **Rule:** `database/attendta-mvp-schema.sql` must be executable against any database name specified by the caller (CI, integration test runners, or LocalDB). e1 removes lines 17–19 (`IF DB_NAME() <> N'AttendTaMVP_Test' THROW 51000`) and guards against accidental production runs via connection string credentials or deployment parameters.

---

#### Finding C2: Canonical Schema File Currently Lacks Lookups, Audit Trigger, `issuer_name_snapshot`, and NOT NULL `event_types.description`
- **Severity:** High
- **Location:** Spine Section `AD-10 — Lookup-backed states and the single schema` (lines 188–193) vs `database/attendta-mvp-schema.sql` and `database/seed-event-types.sql:31`.
- **Evidence:**
  - `architecture-attendta.md:190`: *"`database/attendta-mvp-schema.sql` is the single schema source. e1 delivers all ten tables, the lookups, `event_types.description NOT NULL`, `certificates.issuer_name_snapshot` and the audit trigger."*
  - Inspection of `database/attendta-mvp-schema.sql`:
    - Lines 30, 49, 73, 121, 148, 173, 185, 205: All state columns are implemented as `nvarchar(255)` with inline `CHECK ([col] IN (...))` constraints. There are zero lookup tables.
    - Line 85: `[description] nvarchar(max)` on `event_types` is nullable.
    - Lines 182–200: `certificates` contains `recipient_name_snapshot`, `event_title_snapshot`, and `event_starts_at_snapshot`, but lacks `issuer_name_snapshot`.
    - Lines 203–216: `activity_logs` has no `INSTEAD OF UPDATE, DELETE` trigger.
  - Inspection of `database/seed-event-types.sql:31`:
    ```sql
    INSERT INTO dbo.event_types ([type_name], [created_by])
    SELECT seed.[type_name], @CreatedByUserId ...
    ```
    The seed script does NOT supply `description`. If `event_types.description` is made `NOT NULL`, executing `seed-event-types.sql` will throw a NOT NULL constraint violation!
- **Analysis:**
  AD-10 presents `database/attendta-mvp-schema.sql` as the single authoritative source. While AD-10 assigns e1 to update it, it does not detail that the current script in the repository is still the old string CHECK constraint version from `new mvp.sql`.
  Crucially, making `event_types.description NOT NULL` breaks `database/seed-event-types.sql`, which the spine claims seeds the MVP starter event types.
- **Concrete Proposed Fix:**
  In **AD-10**, explicitly detail e1's schema migration deliverables:
  > - e1 modifies `database/attendta-mvp-schema.sql` to:
  >   1. Replace string CHECK constraints on `role`, `status`, `check_in_method`, and `actor_type` with lookup tables (`roles`, `account_request_statuses`, `membership_statuses`, `event_statuses`, `registration_statuses`, `check_in_methods`, `certificate_statuses`, `activity_actor_types`) and tinyint FKs.
  >   2. Add `[issuer_name_snapshot] nvarchar(255)` to `certificates`.
  >   3. Alter `event_types.description` to `nvarchar(max) NOT NULL`.
  >   4. Add the `INSTEAD OF UPDATE, DELETE` trigger on `activity_logs`.
  > - **Seed Script Fix:** e1 updates `database/seed-event-types.sql` to include non-empty `description` values for all six seeded event types, preventing seed script crashes.

---

#### Finding C3: Lookup Foreign Key Naming Ambiguity Across ADs
- **Severity:** High
- **Location:** Spine Section `AD-10` (line 188), Section `AD-2` (line 79), Section `AD-5` (lines 133, 135), Section `AD-7` (line 158).
- **Evidence:**
  - `architecture-attendta.md:188`: *"Every enumerated state is a seeded lookup table (`<x>_id tinyint` with explicit values, unique `code`, `label`) referenced by a foreign key."*
  - `architecture-attendta.md:79`: References `users.role, users.is_active` (string column name).
  - `architecture-attendta.md:133`: References `member membership status = active`.
  - `architecture-attendta.md:135`: References `registration status = confirmed`.
  - `architecture-attendta.md:158`: References `actor_type = system`.
- **Analysis:**
  AD-10 establishes that state enums become `<x>_id tinyint` foreign keys. However, the rest of the spine continues to refer to columns by their un-migrated string names (`users.role`, `status`, `actor_type`).
  The spine never defines:
  1. The exact lookup table names (e.g. `roles` vs `user_roles`; `registration_statuses` vs `statuses`).
  2. The exact foreign key column names on the parent tables (e.g. is `users` column `role_id` or `role`? Is `registrations` column `status_id` or `registration_status_id`?).
  If different epic builders create queries and models, one builder might use `user.RoleId` while another assumes `user.Role` or joins `Roles`.
- **Concrete Proposed Fix:**
  In **AD-10**, provide an explicit mapping table for all 8 lookup tables and their FK columns:
  ```markdown
  | Enum / State Domain | Lookup Table | PK Column | Parent Table | FK Column Name | C# Enum Type |
  | --- | --- | --- | --- | --- | --- |
  | User role | `roles` | `role_id` | `users` | `role_id` | `UserRole` |
  | Request status | `account_request_statuses` | `status_id` | `account_requests` | `status_id` | `AccountRequestStatus` |
  | Membership status | `membership_statuses` | `status_id` | `members` | `status_id` | `MembershipStatus` |
  | Event status | `event_statuses` | `status_id` | `events` | `status_id` | `EventStatus` |
  | Registration status | `registration_statuses` | `status_id` | `registrations` | `status_id` | `RegistrationStatus` |
  | Check-in method | `check_in_methods` | `method_id` | `attendance` | `check_in_method_id` | `CheckInMethod` |
  | Certificate status | `certificate_statuses` | `status_id` | `certificates` | `status_id` | `CertificateStatus` |
  | Activity actor type | `activity_actor_types` | `actor_type_id` | `activity_logs` | `actor_type_id` | `ActivityActorType` |
  ```

---

#### Finding C4: Column Name Discrepancy on `members` Status Column
- **Severity:** Medium
- **Location:** Spine Section `AD-2` (line 82) and `AD-5` (line 133) vs `database/attendta-mvp-schema.sql:73`.
- **Evidence:**
  - `architecture-attendta.md:82`: `| members (membership status) | Administration (e5) |`
  - `architecture-attendta.md:133`: `users.is_active and member membership status = active`
  - `database/attendta-mvp-schema.sql:73`:
    `[status] nvarchar(255) CHECK ([status] IN ('active', 'inactive')) NOT NULL DEFAULT 'active'`
  - Note: `members` also has an unrelated column `[membership_type] nvarchar(100)` at line 72.
- **Analysis:**
  The spine repeatedly refers to "membership status", which risks confusion with `members.membership_type`. In the canonical schema, the column name is simply `status` (or `status_id` once lookup FKs are added).
- **Concrete Proposed Fix:**
  In **AD-2** line 82 and **AD-5** line 133, clarify the column reference:
  > Change `members (membership status)` to `members.status (membership status)` and `member membership status = active` to `members.status = 'active'`.

---

#### Finding C5: Restricting `activity_logs.actor_type = system` strictly to Setup Scripts
- **Severity:** Medium
- **Location:** Spine Section `AD-7 — Audit writer and vocabulary` (line 158) vs `docsss/AttendTa-System-Specification.md:335, 352`.
- **Evidence:**
  - `architecture-attendta.md:158`: *"The actor comes from `ICurrentActor`. `actor_type = system` only for setup-script rows. Time comes from `TimeProvider`."*
  - `docsss/AttendTa-System-Specification.md:335`: *"`activity_logs.actor_type = system` remains available for consistent system-generated records, not as a claim of automatic waitlist promotion."*
  - `docsss/AttendTa-System-Specification.md:352`: *"System-created certificate reviews may have null creator, but human review/issuance requires an actor."*
  - `database/attendta-mvp-schema.sql:205`: `[actor_type] nvarchar(255) CHECK ([actor_type] IN ('user', 'system')) NOT NULL`.
- **Analysis:**
  Spine AD-7 states that `actor_type = system` is used *only* for setup-script rows. However, when an event is completed, e6 creates Certificate Reviews via `ICertificateReviewService` inside the completion transaction. While the event completion command is triggered by an officer, Certificate Reviews themselves are system-generated rows (and SPEC §18.3 explicitly notes they may have null creators).
  Furthermore, any automated background migration, administrative seeding, or system cleanup requires logging with `actor_type = system`. Forbidding `actor_type = system` for anything except setup scripts creates an artificial barrier for system-generated domain operations.
- **Concrete Proposed Fix:**
  In **AD-7**, revise bullet 4 (line 158):
  > - The actor comes from `ICurrentActor`. `actor_type = system` is used for setup-script seeding and automated system operations where no authenticated user initiates the write (with `actor_user_id = null`). All user-initiated mutations require `actor_type = user` and `actor_user_id` from `ICurrentActor`.

---

## Summary Matrix of Findings

| ID | Severity | Category | Location | Summary |
| --- | --- | --- | --- | --- |
| **A1** | **High** | Companion Drop | AD-2 (lines 76–88) | `users.full_name` update ownership dropped; e2 prohibited from updating member names. |
| **A2** | **High** | Companion Drop | Deferred (lines 360–372) | Explicit MVP scope exclusions (password reset, profile self-edit, re-check-in, report export) silently dropped. |
| **A3** | **Medium** | Contradiction | Capability Map (line 349) | Initial admin provisioning (R35) shifted from e5 (Pinca) to e1 (Platform Access) without coordination. |
| **A4** | **Medium** | Companion Drop | AD-6 (lines 140–149) | Bulk cancellation omits SPEC §13 rule to preserve existing reasons on already-cancelled registrations. |
| **A5** | **Medium** | Companion Drop | AD-5, AD-12 | Mandatory event cover photo invariant for publication omitted from lifecycle rules. |
| **A6** | **Low** | Companion Drop | AD-4 (lines 104–124) | Service invariant `participant_limit <= venues.capacity` omitted from venue/event rules. |
| **B1** | **High** | Brownfield Migration | AD-2 (lines 74, 80–82) | `members` split across 3 epics; brownfield `MemberController.ToggleStatus` in e2 left un-migrated. |
| **B2** | **High** | Brownfield Migration | AD-8 (line 169) | `ICurrentActor` introduced without assigning refactoring of brownfield `int actorUserId` service parameters. |
| **B3** | **Medium** | Brownfield Migration | AD-12 (line 204) | Relative path prefix mismatch: brownfield `venues/...` vs spine `uploads/venues/...`. |
| **B4** | **Medium** | Brownfield Migration | AD-3 (line 102), Conventions | Migrating repositories off `SaveChanges` will break synchronous Master Data services unless coordinated. |
| **B5** | **Low** | Brownfield Migration | AD-10, AD-18 | Obsolete brownfield shims (`MemberAccountSummary`, `SessionManager`, `PathManager`) left un-cleaned. |
| **C1** | **Critical** | Schema Defect | Schema (lines 17–19), AD-17 | Hardcoded `AttendTaMVP_Test` DB name check in canonical schema script breaks CI and integration test runners. |
| **C2** | **High** | Schema Defect | Schema, AD-10 | Script lacks lookups, trigger, `issuer_name_snapshot`; `seed-event-types.sql` breaks on NOT NULL description. |
| **C3** | **High** | Schema Ambiguity | AD-10, AD-2, AD-5 | Lookup FK table and column names undefined; spine inconsistently mixes string names with tinyint lookups. |
| **C4** | **Medium** | Schema Discrepancy | AD-2 (line 82), AD-5 | Spine calls column `membership status` when schema column name is `status`. |
| **C5** | **Medium** | Schema Discrepancy | AD-7 (line 158) | Spine restricts `actor_type = system` to setup scripts only, contradicting SPEC §18.1. |
