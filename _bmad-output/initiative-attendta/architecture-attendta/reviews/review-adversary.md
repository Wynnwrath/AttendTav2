# Architecture Spine Review — Adversarial Stress-Test
**Document under review:** `_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md`  
**Reviewer:** Adversary  
**Date:** 2026-11-04  
**Verdict:** REJECT WITH REQUIRED AMENDMENTS — 12 integration holes identified where pairs of epics obeying all ADs to the letter will fail to build, deadlock at runtime, or corrupt data.

---

## 1. Executive Summary

A spine succeeds only if two independent teams can build adjacent modules against it without private verbal coordination. This adversarial review evaluates AttendTa's spine by constructing concrete implementation scenarios where two epics (e.g., Epic 3 and Epic 5, Epic 4 and Epic 6, Epic 1 and Epic 2) follow every AD to the letter, yet produce:
1. **Incompatible method signatures and unrepresented parameters** (`IRegistrationCancellationService`, `ICurrentActor`, `AuditEntry`).
2. **Missing cross-module write contracts** (attendance invalidation in e4 updating certificate review in e6 under R25).
3. **Silent data corruption via unspecified lookup numeric IDs** (AD-10 delegates enum seeds to e1 without contracts).
4. **Fatal runtime identity key failures** (AD-3's single `SaveChangesAsync` contract breaks EF Core inserts with auto-generated identity foreign keys).
5. **Hardcoded integration test crashes** (AD-17 vs `attendta-mvp-schema.sql` line 18 guard).
6. **Deadlock and throughput collapses** (AD-4 serializing all independent attendee check-ins on a single event row lock).

---

## 2. Clash Pairs Matrix

| Clash ID | Epic Pair | Mechanism / Conflict | Severity | Governed AD |
|---|---|---|---|---|
| **ADV-1** | e3 (Events) vs e5 (Admin) | `IRegistrationCancellationService.CancelAsync` signature mismatch (single vs batch, missing cause enum) | **CRITICAL** | AD-6 |
| **ADV-2** | e4 (Check-in) vs e6 (Certs) | R25 invalidation-to-review update missing from AD-2 cross-module writes; e4 forbidden from writing `certificates` | **CRITICAL** | AD-2, AD-4 |
| **ADV-3** | e1 (Platform) vs e3/e4/e6 | Undefined lookup table names, FK column names, and enum numeric values cause silent data corruption | **CRITICAL** | AD-10 |
| **ADV-4** | e1 (Platform) vs e2 (Master) vs e6 (Certs) | `IPrivateFileStore` rejects PDF certificates due to image magic-byte rule; leaks WebApp types; inconsistent relative paths | **HIGH** | AD-12, AD-1 |
| **ADV-5** | e1 (Platform) vs All Epics | `PagedResult<T>` generic invariance breaks `@model` binding in reusable `_Pager.cshtml`; query filters lost | **HIGH** | AD-13 |
| **ADV-6** | e1 (Platform) vs e2 (Master) vs e5 (Admin) | `users.full_name` has no designated writer in AD-2; `MemberRepository` ownership conflict across e1/e2/e5 | **HIGH** | AD-2 |
| **ADV-7** | e1 (Platform) vs e3 (Events) | `IUnitOfWork` single `SaveChangesAsync` rule prevents retrieving generated identity keys for dependent child records | **HIGH** | AD-3 |
| **ADV-8** | e4 (Check-in) vs e3 (Events) | Coarse event row lock forces all concurrent participant check-ins to serialize, creating hotspot bottlenecks | **HIGH** | AD-4 |
| **ADV-9** | e1 (Platform) vs e8 (Acceptance/CI) | Schema script hardcoded database guard `DB_NAME() <> 'AttendTaMVP_Test'` aborts per-class test databases in CI | **HIGH** | AD-17, AD-10 |
| **ADV-10** | e1 (Platform) vs All Epics | `ICurrentActor` throws or fails on unauthenticated operations (Account Request R1) during audit log appending | **MEDIUM** | AD-7, AD-8 |
| **ADV-11** | e3 (Events) vs e4 (Check-in) vs e6 (Certs) | Uncoordinated three-way concurrent UI composition on `MyEvents` view model and Razor views | **MEDIUM** | AD-14, Cap Map |
| **ADV-12** | e1 (Platform) vs e4 (Check-in) | Global antiforgery validation without standard header contract rejects camera scanner AJAX POST payloads | **MEDIUM** | AD-8, AD-9 |

---

## 3. Detailed Findings & Concrete Fixes

### Finding ADV-1: `IRegistrationCancellationService` Signature and Lock Incompatibility
- **Severity:** **CRITICAL**
- **Location:** AD-6 (`_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md:144-148`)
- **Evidence:**
  > "e3 owns `IRegistrationCancellationService.CancelAsync(selection, cause, reason, ct)`. It is the only code that sets a registration to cancelled.
  > Each call clears `qr_token`, records the reason, actor and time, writes one `registration.cancelled` audit entry per row, and never promotes.
  > It joins the caller's transaction and assumes the caller already holds the AD-4 locks.
  > Callers are event cancel, decline, admin cancel and member cancel (e3), and membership or account deactivation (e5)."
- **Adversary Attack Scenario:**
  1. Developer 3 (e3) builds `IRegistrationCancellationService` tailored to its own use cases (member cancel, admin cancel, decline):
     ```csharp
     public Task CancelAsync(int registrationId, CancellationCause cause, string? reason, CancellationToken ct);
     public enum CancellationCause { MemberSelfCancel, AdminCancel, EventCancelled, Declined }
     ```
  2. Developer 5 (e5) implements account/membership deactivation (R5, R32). An administrator deactivates Member 42, who has upcoming confirmed registrations in Event 10 and Event 14.
  3. Developer 5 cannot call `CancelAsync`:
     - e5 has a `memberId`, not a single `registrationId`. Calling in a loop requires e5 to discover registrations, but e3 owns `registrations` table writes and e5 has no contract query to retrieve upcoming registration IDs.
     - `CancellationCause` does not have `AccountDeactivation` or `MembershipDeactivation`.
     - AD-6 states `CancelAsync` "assumes the caller already holds the AD-4 locks." To cancel registrations across Event 10 and Event 14, e5 must acquire locks in ascending order (`Event 10`, then `Event 14`). But e5 does not know which events the member is registered for without querying e3's repository! If e5 loops registrations and locks events out of order, it causes SQL deadlocks.
- **Proposed Fix:**
  Replace AD-6 rule text with an explicit interface and contract definition:
  ```markdown
  ### AD-6 — One participation-cancellation path
  - **Rule:** e3 declares and implements `IRegistrationCancellationService` in Services:
    ```csharp
    public interface IRegistrationCancellationService {
        Task CancelAsync(RegistrationCancellationTarget target, CancellationCause cause, string reason, CancellationToken ct);
    }
    public record RegistrationCancellationTarget(
        int? SingleRegistrationId = null,
        int? MemberId = null,
        int? EventId = null
    );
    ```
    - `CancellationCause` is an enum in `ASI.Basecode.Resources.Constants`: `{ MemberRequested, AdminAction, EventCancelled, RegistrationDeclined, MemberDeactivated, AccountDeactivated }`.
    - `reason` is required for all causes except `MemberRequested` (where null/empty is stored as "Member self-cancellation").
    - When `target.MemberId` is provided, `CancelAsync` queries upcoming registrations for that member, locks each affected event row in ascending `event_id` order (conforming to AD-4), updates the registration status to `cancelled`, clears `qr_token`, and appends audit logs.
  ```

---

### Finding ADV-2: Missing Cross-Module Invalidation-to-Review Mutation Contract (R25 vs AD-2)
- **Severity:** **CRITICAL**
- **Location:** AD-2 (`_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md:74-93`)
- **Evidence:**
  > "Only the module listed below may call write methods for the listed table or columns, and only from its own service... `certificates`: Certificates (e6)"
  > "Cross-module writes go through the owner's service, inside the caller's transaction:
  > - **Event completion.** e3 owns it. e6 adds Certificate Review creation, through `ICertificateReviewService`, to e3's completion transaction.
  > - **Check-in toggle (R22).** e4 owns the command and the rules..."
- **Adversary Attack Scenario:**
  1. PRD R25 explicitly states: *"attendance invalidation marks certificate review as not_eligible if not yet issued."*
  2. Developer 4 (e4) implements Attendance Invalidation (R25).
  3. Developer 4 looks at AD-2: `certificates` table can ONLY be written by e6 (`Certificates`).
  4. Developer 4 looks at AD-2's list of cross-module writes: it lists Event Completion and Check-in Toggle. It DOES NOT list attendance invalidation!
  5. Furthermore, e4 is built in Sprint 2, while e6 is built in Sprint 3. e6 does not exist.
  6. If Developer 4 writes directly to `certificates`, they violate AD-2. If Developer 4 leaves it out, R25 is broken and invalid attendees receive certificates upon issuance.
- **Proposed Fix:**
  Add attendance invalidation to AD-2 cross-module writes and specify the collaborator interface:
  ```markdown
  In AD-2 under "Cross-module writes go through the owner's service, inside the caller's transaction":
  - **Attendance invalidation (R25).** e4 owns the invalidation command and transaction. If an unissued Certificate Review exists for the registration, e4 calls `ICertificateReviewService.MarkNotEligibleOnInvalidationAsync(int registrationId, string reason, CancellationToken ct)`.
  - e6 declares `ICertificateReviewService` in `ASI.Basecode.Services.Interfaces`. e1 or e4 provides a default no-op implementation until e6 registers the real implementation.
  ```

---

### Finding ADV-3: Undefined Lookup Table Names, FK Columns, and Enum Numeric Values
- **Severity:** **CRITICAL**
- **Location:** AD-10 (`_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md:188-193`)
- **Evidence:**
  > "Rule: Every enumerated state is a seeded lookup table (`<x>_id tinyint` with explicit values, unique `code`, `label`) referenced by a foreign key.
  > C# enums in `ASI.Basecode.Data/Models/Lookups` declare numeric values equal to the seeded ids. The codes keep the current values (e.g. `pending_approval`, `qr_code`).
  > `database/attendta-mvp-schema.sql` is the single schema source. e1 delivers all ten tables, the lookups..."
- **Adversary Attack Scenario:**
  1. `database/attendta-mvp-schema.sql` currently has 0 lookup tables (all status columns use string `CHECK` constraints: `'draft'`, `'published'`, `'pending'`, `'confirmed'`, etc.).
  2. AD-10 delegates writing the lookup tables to e1, but does NOT define the lookup table names, FK column names, or explicit numeric values in the spine.
  3. Developer 1 creates `registration_statuses` with `status_id tinyint`: `1 = pending, 2 = confirmed, 3 = waitlisted, 4 = cancelled`.
  4. Developer 3 (working in parallel on e3) defines C# enum without explicit values (default C# 0-indexed):
     ```csharp
     public enum RegistrationStatus : byte { Pending = 0, Confirmed = 1, Waitlisted = 2, Cancelled = 3 }
     ```
  5. Developer 3 confirms a registration: C# writes value `1` (`RegistrationStatus.Confirmed`) to `registrations.status_id`.
  6. In SQL Server, value `1` maps to `pending`! The user was confirmed in C# but remains `pending` in SQL. Queries filtering `WHERE status_id = 2` find 0 confirmed members.
  7. Furthermore, Developer 1 renames column `status` to `status_id`, while Developer 3 maps `status`. The EF Core model fails mapping at startup.
- **Proposed Fix:**
  Specify the exact canonical lookup names, column names, and explicit 1-based integer values directly in AD-10:
  ```markdown
  ### AD-10 — Lookup-backed states and the single schema
  All lookup tables have primary key `<lookup>_id tinyint`, `code varchar(50) UNIQUE NOT NULL`, `label nvarchar(100) NOT NULL`.
  Foreign key columns in business tables are named `<lookup>_id tinyint NOT NULL`.
  Seeded values and C# enum definitions in `ASI.Basecode.Data/Models/Lookups`:
  1. `roles` (`role_id`): `Admin = 1 ('admin', 'Administrator')`, `Officer = 2 ('officer', 'Officer')`, `Member = 3 ('member', 'Member')`.
  2. `account_request_statuses` (`account_request_status_id`): `Pending = 1`, `Approved = 2`, `Rejected = 3`.
  3. `member_statuses` (`member_status_id`): `Active = 1`, `Inactive = 2`.
  4. `event_statuses` (`event_status_id`): `Draft = 1`, `Published = 2`, `Cancelled = 3`, `Completed = 4`.
  5. `registration_statuses` (`registration_status_id`): `Pending = 1`, `Confirmed = 2`, `Waitlisted = 3`, `Cancelled = 4`.
  6. `attendance_methods` (`attendance_method_id`): `QrCode = 1 ('qr_code')`, `ManualSearch = 2 ('manual_search')`, `StudentNumber = 3 ('student_number')`.
  7. `certificate_statuses` (`certificate_status_id`): `PendingApproval = 1 ('pending_approval')`, `Eligible = 2 ('eligible')`, `NotEligible = 3 ('not_eligible')`, `Issued = 4 ('issued')`.
  8. `actor_types` (`actor_type_id`): `User = 1 ('user')`, `System = 2 ('system')`.
  ```

---

### Finding ADV-4: `IPrivateFileStore` Rejection of PDF Certificates, Framework Type Leakage, and Path Inconsistencies
- **Severity:** **HIGH**
- **Location:** AD-12 (`_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md:204-208`), AD-1 (`:68`)
- **Evidence:**
  > "One `IPrivateFileStore` (declared in Services, implemented in `WebApp/Storage`, generalizing `VenueImageStorage`), rooted at `Common:SetupRoot`. Its areas are `uploads/venues`, `uploads/events`, `certificates` and `temporary`...
  > Images are accepted only if they are JPEG or PNG by magic bytes and at most 2 MiB. Filenames are generated by the server and written with `CreateNew`.
  > Replacement order: write the new file, commit the database row, then delete the replaced file...
  > SQL stores relative paths only.
  > Downloads stream through authorized controller actions after a containment check."
- **Adversary Attack Scenario:**
  1. Existing `VenueImageStorage` has `SaveAsync(IFormFile image)`. If Developer 2 generalizes this in `Services`, using `IFormFile` violates AD-1 clean layering by dragging ASP.NET Core HTTP abstractions into the `Services` library.
  2. Developer 2 implements the magic-byte validation described in AD-12: checking for JPEG/PNG header bytes (`0xFF 0xD8 0xFF` or `0x89 0x50 0x4E 0x47`) and max 2 MiB.
  3. In Sprint 3, Developer 6 (Certificates) attempts to save an issued landscape A4 PDF certificate. The PDF header begins with `%PDF-` (`0x25 0x50 0x44 0x46`). `IPrivateFileStore` rejects the certificate as invalid magic bytes!
  4. Furthermore, Playwright requires a file path on disk to output PDF files (`page.PdfAsync(new() { Path = tempPath })`). `IPrivateFileStore` has no API to provide a temporary file target or ingest Playwright output.
  5. Finally, existing `VenueImageStorage` resolves paths relative to `SetupRoot/uploads`. If e2 stores `"venues/{guid}.jpg"` in the DB, while e6 stores `"certificates/{guid}.pdf"` relative to `SetupRoot`, download streaming controller actions fail containment checks and resolve incorrect paths.
- **Proposed Fix:**
  Tighten AD-12 with explicit method signatures, area policies, and path root conventions:
  ```markdown
  ### AD-12 — Private file store
  - **Interface contract (`ASI.Basecode.Services.Interfaces.IPrivateFileStore`):**
    ```csharp
    public interface IPrivateFileStore {
        Task<string> SaveAsync(Stream stream, string area, string fileExtension, CancellationToken ct = default);
        Task<Stream> OpenReadAsync(string relativePath, CancellationToken ct = default);
        void Delete(string relativePath);
        string GetTemporaryFilePath(string fileExtension);
    }
    ```
  - **Area rules:**
    - `uploads/venues` and `uploads/events`: strictly JPEG/PNG magic bytes verified on stream, max 2 MiB.
    - `certificates`: strictly `%PDF-` magic bytes, max 10 MiB.
    - `temporary`: staging area for Playwright outputs before atomic rename to `certificates`.
  - **Path convention:** All database paths and `IPrivateFileStore` arguments are relative to `Common:SetupRoot` using `/` (e.g. `uploads/venues/{guid}.jpg`, `certificates/{guid}.pdf`).
  ```

---

### Finding ADV-5: `PagedResult<T>` Invariance Breaks `@model` Binding in `_Pager.cshtml`
- **Severity:** **HIGH**
- **Location:** AD-13 (`_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md:214-220`)
- **Evidence:**
  > "e1 builds one `PageRequest`/`PagedResult<T>` (in `Services/ServiceModels`) and one `_Pager` partial.
  > Page size is fixed at 25."
- **Adversary Attack Scenario:**
  1. Developer 1 creates `public class PagedResult<T> { public List<T> Items { get; set; } ... }` in `Services`.
  2. Developer 1 writes `Views/Shared/_Pager.cshtml`. In Razor, `@model` must be strongly typed. Developer 1 types it as `@model PagedResult<object>` or `@model dynamic`.
  3. In C#, generic classes are invariant: `PagedResult<MemberViewModel>` cannot be cast to `PagedResult<object>`. In e2, rendering `<partial name="_Pager" model="Model.Members" />` crashes with an `InvalidCastException`.
  4. If typed as `@model dynamic`, runtime binder exceptions occur if property names diverge.
  5. Furthermore, Developer 1 generates page links with `asp-route-page="@i"`. In e2 (Member list) or e3 (Event roster), users filter by status or search keyword (`?search=CS&status=confirmed&page=1`). Clicking page 2 strips all query parameters, resetting filters on every page change.
- **Proposed Fix:**
  Update AD-13 to mandate a non-generic `IPagedResult` interface and query-string preservation:
  ```markdown
  ### AD-13 — Paging
  - `ASI.Basecode.Services.ServiceModels` declares:
    ```csharp
    public interface IPagedResult {
        int PageNumber { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }
    public class PagedResult<T> : IPagedResult {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
    ```
  - `_Pager.cshtml` declares `@model IPagedResult` and preserves all ambient HTTP GET query parameters across pagination links using `ViewContext.HttpContext.Request.Query`.
  ```

---

### Finding ADV-6: Orphaned Column Ownership for `users.full_name` & `MemberRepository` Split Ownership
- **Severity:** **HIGH**
- **Location:** AD-2 (`_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md:78-82`)
- **Evidence:**
  > "| Table / column | Writing module (epic) |
  > | `users` (create, credentials), `account_requests` | Platform Access (e1) |
  > | `users.role`, `users.is_active` | Administration (e5) |
  > | `members` (create on approval) | Platform Access (e1) |
  > | `members` (profile fields) | Master Data (e2) |
  > | `members` (membership status) | Administration (e5) |"
- **Adversary Attack Scenario:**
  1. In the canonical schema (`attendta-mvp-schema.sql`), `full_name` and `email` live on `users`, NOT `members`. `members` has no name column.
  2. AD-2 designates e1 as writer for `users (create, credentials)` and e5 for `role, is_active`. NO module is assigned to update `users.full_name`!
  3. When an active member edits their profile (R4) or an administrator updates a member's record (R5), updating the name requires updating `users.full_name`.
  4. AD-2 dictates: *"Only the module listed below may call write methods for the listed table or columns, and only from its own service."* If e2 updates `users.full_name`, it violates AD-2. If e2 does not, profile name editing is impossible.
  5. Furthermore, AD-2 says: *"Each table has one repository class in Data."* If e2 owns `MemberRepository`, e1 (built before e2) cannot approve account requests because e2 does not exist yet.
- **Proposed Fix:**
  Amend AD-2's table:
  ```markdown
  | Table / column | Writing module (epic) | Notes |
  | `users` (create, credentials) | Platform Access (e1) | Initial user creation upon account request approval |
  | `users.full_name` | Master Data (e2) | Synchronized when member profile name is updated (R4, R5) |
  | `users.role`, `users.is_active` | Administration (e5) | Role and account status management (R31, R32) |
  | `members` | Data Layer Baseline (e1) | e1 creates `MemberRepository` and `UserRepository` with specific method boundaries for e1, e2, and e5. |
  ```

---

### Finding ADV-7: `IUnitOfWork` Single `SaveChangesAsync` Prevents Retrieving Generated Identity Keys
- **Severity:** **HIGH**
- **Location:** AD-3 (`_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md:98-101`)
- **Evidence:**
  > "Each mutating use case runs inside `IUnitOfWork.ExecuteInTransactionAsync(work, ct)`. This opens one READ COMMITTED transaction on the request-scoped `DbContext`, runs `work`, calls `SaveChangesAsync` once and commits. On any exception it rolls back.
  > A call made while a transaction is already open joins it and does not commit."
- **Adversary Attack Scenario:**
  1. In SQL Server with EF Core, primary keys on `users`, `account_requests`, `events`, `registrations` are `IDENTITY(1, 1)`. Identity values are generated by SQL Server upon INSERT and populated onto EF entities only when `SaveChangesAsync` executes.
  2. Because `AsiBasecodeDbContext` maps scalar foreign keys (`public int UserId { get; set; }` on `Member`) without EF navigation graph tracking:
  3. In e1, account request approval must:
     a. Insert `User` record.
     b. Insert `Member` record referencing `user.UserId`.
  4. Under AD-3, `SaveChangesAsync` is called ONLY ONCE at the end of `ExecuteInTransactionAsync`. While `work` executes, `user.UserId` remains `0`.
  5. Step (b) inserts `Member` with `user_id = 0`, immediately throwing a foreign key violation `FK_members_users`!
  6. Furthermore, `ExecuteInTransactionAsync` does not support returning values (e.g. `Task<T>`), forcing callers into awkward closure mutations.
- **Proposed Fix:**
  Update AD-3 to allow intermediate flush and add generic return overload:
  ```markdown
  ### AD-3 — Caller-owned unit of work
  - `IUnitOfWork` declares:
    ```csharp
    Task ExecuteInTransactionAsync(Func<Task> work, CancellationToken ct = default);
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> work, CancellationToken ct = default);
    Task FlushChangesAsync(CancellationToken ct = default);
    ```
  - `ExecuteInTransactionAsync` runs `work`, calls `SaveChangesAsync` if uncommitted changes remain, and commits the transaction.
  - When auto-generated identity keys are needed by dependent child records before completion (e.g. creating `Member` with generated `user.UserId`), `work` may call `_unitOfWork.FlushChangesAsync(ct)`, which executes `SaveChangesAsync` within the open transaction without committing it.
  ```

---

### Finding ADV-8: Coarse Event Lock Bottlenecks Concurrency Across Independent Participants
- **Severity:** **HIGH**
- **Location:** AD-4 (`_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md:118-120`)
- **Evidence:**
  > "Rows in `registrations`, `attendance` and `certificates` are written only while holding their event's lock.
  > Per-participant operations lock the owner's `users` row first, then the event. These are approve, decline, promote, member cancel, admin cancel, check-in, attendance invalidation, eligibility decision and issuance.
  > Event-wide operations lock only the event. These are publish, edit, cancel, complete and the check-in toggle."
- **Adversary Attack Scenario:**
  1. At a major campus event with 300 participants, two officers are scanning attendees at two venue entrances using their phones (hotspot/LAN).
  2. Officer 1 scans Participant A (User 101, Event 5). Officer 1 locks User 101, then acquires an exclusive lock on Event 5.
  3. Simultaneously, Officer 2 scans Participant B (User 202, Event 5). Officer 2 locks User 202, and tries to lock Event 5.
  4. Officer 2 blocks waiting for Officer 1's transaction to commit.
  5. Because check-in involves database queries, audit logging, and camera decode roundtrips, every scan takes 100–300 ms. Under load, phone requests queue, hit lock timeouts, and trigger false failure screens.
  6. Independent attendee check-ins have zero data dependencies with each other (`attendance` rows are unique per registration); serializing them on the entire event table row is an artificial concurrency bottleneck that violates NFR-1 responsiveness.
- **Proposed Fix:**
  Refine AD-4 lock semantics:
  ```markdown
  ### AD-4 — Lock order and serialization
  - **Lifecycle locks:** Event-wide lifecycle transitions (publish, edit, cancel, complete, check-in toggle) acquire an exclusive lock on the `events` row: `SELECT 1 FROM events WITH (UPDLOCK, HOLDLOCK, ROWLOCK) WHERE event_id = @id`.
  - **Participant locks:** Per-participant operations (check-in, invalidation, issuance) verify event validity (published and check-in open) using standard READ COMMITTED, and acquire an exclusive lock ONLY on the participant's `users` row and the specific `registrations` row: `SELECT 1 FROM registrations WITH (UPDLOCK, HOLDLOCK, ROWLOCK) WHERE registration_id = @id`.
  - Concurrent check-ins for different participants in the same event proceed in parallel without blocking each other.
  ```

---

### Finding ADV-9: Schema Script Hardcoded Database Guard Aborts Per-Class CI Test Databases
- **Severity:** **HIGH**
- **Location:** AD-17 (`_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md:260`), `database/attendta-mvp-schema.sql:17-19`
- **Evidence:**
  > `attendta-mvp-schema.sql:17-19`:
  > ```sql
  > IF DB_NAME() <> N'AttendTaMVP_Test'
  >     THROW 51000, 'This schema script may only run in AttendTaMVP_Test.', 1;
  > GO
  > ```
  > AD-17: *"tests/ASI.Basecode.IntegrationTests runs against the SQL Server named in ATTENDTA_TEST_SQL. It creates a fresh database from the canonical schema for each test class."*
- **Adversary Attack Scenario:**
  1. Developer 1 sets up xUnit v3 integration test harness per AD-17.
  2. To run tests in parallel without data collision, xUnit test fixtures create a fresh disposable database per test class (e.g. `AttendTa_Test_RegistrationTests_d3e4`).
  3. The harness runs `attendta-mvp-schema.sql` against the new database.
  4. SQL Server throws error 51000: `'This schema script may only run in AttendTaMVP_Test.'` on line 18!
  5. 100% of integration tests fail during fixture setup in CI.
- **Proposed Fix:**
  In AD-10 and `database/attendta-mvp-schema.sql`:
  Replace the restrictive database name check with a safety check preventing execution against production system databases:
  ```sql
  IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
      THROW 51000, 'This schema script may not run in system databases.', 1;
  ```

---

### Finding ADV-10: `ICurrentActor` Throws or Fails on Unauthenticated Account Requests During Audit
- **Severity:** **MEDIUM**
- **Location:** AD-7 (`_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md:158`), AD-8 (`:169`)
- **Evidence:**
  > "The actor comes from `ICurrentActor`. `actor_type = system` only for setup-script rows. Time comes from `TimeProvider`."
  > "Services get the actor from `ICurrentActor`. Request models never carry actor ids or roles."
- **Adversary Attack Scenario:**
  1. A student visitor submits an Account Request (R1). The visitor is not authenticated.
  2. The use case appends an audit entry (`account_request.created`).
  3. `IActivityLogWriter.Append` fetches the actor from `ICurrentActor`.
  4. If Developer 1 modeled `ICurrentActor.UserId` as a non-nullable `int` (throwing `InvalidOperationException` when unauthenticated), registration crashes with HTTP 500.
  5. Even if nullable, AD-7 states: *"`actor_type = system` only for setup-script rows."* If `actor_type` must be `'user'`, `actor_user_id` is null, which violates the premise that user actions have a known actor user ID.
- **Proposed Fix:**
  Clarify `ICurrentActor` and AD-7:
  ```markdown
  ### AD-7 — Audit writer and vocabulary
  - `ICurrentActor` contract in `ASI.Basecode.Services.Interfaces`:
    ```csharp
    public interface ICurrentActor {
        int? UserId { get; }
        string? Role { get; }
        bool IsAuthenticated { get; }
    }
    ```
  - For unauthenticated actions (such as initial account request submission R1), `actor_type` is recorded as `'user'` with `actor_user_id = NULL` and description noting public self-service. `actor_type = 'system'` remains reserved for bootstrap scripts and background maintenance.
  ```

---

### Finding ADV-11: Uncoordinated Three-Way Concurrent UI Composition on `MyEvents` (R20)
- **Severity:** **MEDIUM**
- **Location:** Capability Map (`_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md:353-356`), `tickets.toml:5`
- **Evidence:**
  > `tickets.toml:5`: *"# R20 — e3 My Events registrations and status; e4 QR Pass and attendance state; e6 certificate download links."*
- **Adversary Attack Scenario:**
  1. Developer 3 (e3) builds `MyEventsController` and `Views/MyEvents/Index.cshtml` to render a member's registration cards.
  2. Developer 4 (e4) concurrently implements QR Pass download and attendance badge, editing `MyEventsController.cs` and `Index.cshtml`.
  3. Developer 6 (e6) concurrently implements certificate download links, editing `MyEventsController.cs` and `Index.cshtml`.
  4. When merging e4 and e6 into `dev`, git produces heavy merge conflicts across controllers, view models, and Razor templates.
- **Proposed Fix:**
  Add a structural convention in AD-14:
  `MyEvents` card UI must use modular child partials: e3 owns `MyEventsController` and base registration card model `MyEventCardViewModel`. The card view defines placeholder partial calls: `<partial name="_AttendancePassPartial" model="item" />` and `<partial name="_CertificateDownloadPartial" model="item" />`. e4 and e6 deliver only their respective partial views, eliminating merge collisions in the main view.

---

### Finding ADV-12: Antiforgery Header Mismatch on AJAX Scanner Endpoints
- **Severity:** **MEDIUM**
- **Location:** AD-8 (`_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md:171`), AD-9 (`:181`)
- **Evidence:**
  > "Every POST validates the antiforgery token. Every mutation is a POST, including logout."
  > "The scanner and other AJAX actions return an HTTP status plus JSON `{ outcome, message }`."
- **Adversary Attack Scenario:**
  1. In e1, Developer 1 configures `[AutoValidateAntiforgeryToken]` on all controllers per AD-8.
  2. In e4, Developer 4 writes scanner JavaScript using browser `fetch('/Attendance/CheckIn', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ qrToken }) })`.
  3. The fetch request does not pass the antiforgery token header.
  4. ASP.NET Core Antiforgery middleware intercepts the request and returns HTTP 400 Bad Request before `AttendanceController` is ever invoked.
  5. The camera scanner fails with opaque HTTP 400 errors.
- **Proposed Fix:**
  Add to AD-8:
  ```markdown
  AJAX POST requests validate antiforgery via the standard header `RequestVerificationToken`. The shared layout renders `<meta name="csrf-token" content="@GetAntiforgeryToken()" />`, and client scripts reading scanner or AJAX inputs must attach this header on all fetch POST requests.
  ```

---

## 4. Summary Table of Proposed AD Amendments

| AD ID | Topic | Required Amendment Summary |
|---|---|---|
| **AD-2** | Cross-Module Writes | Add R25 attendance invalidation calling `ICertificateReviewService.MarkNotEligibleOnInvalidationAsync`. Designate e2 as writer for `users.full_name`. |
| **AD-3** | Unit of Work | Allow `FlushChangesAsync()` inside transactions to materialize identity keys before dependent child inserts. Add generic return `ExecuteInTransactionAsync<T>`. |
| **AD-4** | Serialization | Restrict exclusive event row locks to lifecycle commands; use participant user + registration row locks for check-in to avoid entrance bottlenecks. |
| **AD-6** | Cancellation | Provide explicit `IRegistrationCancellationService` contract with `RegistrationCancellationTarget`, `CancellationCause` enum, and member-level batch handling. |
| **AD-7** | Audit Writer | Define `AuditEntry` record and `ICurrentActor` interface; permit null actor user ID for unauthenticated public requests. |
| **AD-8** | Antiforgery | Define `RequestVerificationToken` header convention for AJAX scanner endpoints. |
| **AD-10** | Lookup States | Enumerate exact lookup table names, FK column names, and explicit 1-based integer values in the spine. Remove `DB_NAME` restriction from schema script. |
| **AD-12** | File Store | Define `IPrivateFileStore` contract using streams; separate area validation policies (JPEG/PNG for venues/events vs PDF for certificates); standardize relative paths. |
| **AD-13** | Paging | Introduce `IPagedResult` non-generic interface for `_Pager.cshtml` covariance; preserve query parameters across page links. |
| **AD-14** | View Composition | Establish modular partial conventions for multi-epic pages (`MyEvents`). |
