# Architecture Spine Rubric Review: AttendTa Initiative

**Artifact under review:** `_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md`  
**Supporting sources:** `_bmad-output/initiative-attendta/architecture-attendta/.memlog.md`, `_bmad-output/initiative-attendta/initiative-attendta.md`, `_bmad-output/initiative-attendta/tickets/tickets.toml`, `_bmad-output/initiative-attendta/tickets/epic-*/epic-*.md`, `database/attendta-mvp-schema.sql`, `docsss/architecture.md`, `docsss/AttendTa-System-Specification.md`, codebase under `ASI.Basecode.*`  
**Reviewer role:** RubricWalker (Good-Spine Checklist & Invariants Gate)  
**Date:** 2026-11-04  

---

## 1. Executive Summary & Gate Verdict

### Verdict: **CONDITIONAL APPROVAL (Requires 3 High and 4 Medium Fixes Prior to Handoff)**

The architecture spine for AttendTa establishes a cohesive, robust, and disciplined foundation for building the 8 epics in parallel. It correctly rejects accidental complexity: it preserves the brownfield 4-project layered monolith, establishes strict single-writing-module boundaries per database table (AD-2), institutes a caller-owned transactional unit of work (AD-3), introduces a clear SQL Server lock hierarchy to prevent deadlocks (AD-4), mandates a single source of truth for temporal rules via `System.TimeProvider` (AD-11), isolates private storage outside `wwwroot` (AD-12), and standardizes outcomes (AD-9) and pagination (AD-13).

However, evaluated against the 10-point Good-Spine Rubric, **three high-severity gaps** and **four medium-severity vulnerabilities** require explicit refinement before the spine is locked as the build substrate:

1. **AD-4 Lock Order Omission of Member Signup & Venue/Event Concurrency Race (High Severity):** AD-4 line 119 omits Member Registration / Signup (`R14`) from the per-participant locking rule (`users` row then `events` row). This creates a deadlock risk against concurrent account deactivation (`R32`) and allows signups to race with deactivations. Furthermore, event-wide operations lock only `events`, permitting Venue Capacity edits (`R7`) to race with Event Limit updates (`R9`/`R10`), violating the invariant that venue capacity must remain $\ge$ linked event limits.
2. **Circular Module Dependency & Missing Invalidation Interface in AD-2 (High Severity):** The Services module diagram specifies `CERT --> REG` (Certificates calls Events & Registration), but AD-2 line 91 states that Event Completion in `REG` calls `ICertificateReviewService` in `CERT`, introducing a bidirectional circular coupling. Additionally, AD-2 omits the required cross-module write interface for Attendance Invalidation (`R25`), where Check-in (`e4`) must transition unissued Certificate Reviews in `e6` to `not_eligible`.
3. **Pessimistic Reader-Writer Blocking under Default Read Committed (High Severity):** AD-3 mandates standard SQL Server `READ COMMITTED`. Without explicitly requiring Read Committed Snapshot Isolation (`READ_COMMITTED_SNAPSHOT ON` / RCSI), shared read locks taken by list views, reports, and "My Events" will block writers holding exclusive row locks during door check-in, causing request timeouts and conversion deadlocks under concurrent event operations.
4. **Missing Canonical Facts for Reporting & Ambiguous "My Events" Aggregation (Medium Severity):** AD-5 omits the canonical definitions for "No-show" (R30/R19) and "Attendance Rate" (R30), creating a divergence hazard between `e3`/`e4` and `e7`. Furthermore, "My Events" spans `e3`, `e4`, and `e6`, but the spine does not define how this cross-module screen is composed without violating Services module boundaries.
5. **AutoMapper CVE-2026-32933 CI Vulnerability Gate Hazard (Medium Severity):** Pinning `AutoMapper` 14.0.0 in the Stack while leaving Open Question 2 unresolved creates an immediate CI failure under automated vulnerability scanners (Dependabot, `dotnet list package --vulnerable`). The spine should decisively adopt explicit static mapping extension methods.
6. **Schema Non-Null `activity_logs.description` Omitted from AD-7 Contract (Medium Severity):** `database/attendta-mvp-schema.sql` defines `[description] nvarchar(max) NOT NULL`, but AD-7 only specifies action codes, entity types, and old/new JSON, leaving the required `description` field undefined across epics.
7. **EF Core Sibling Package Version Fragmentation (Low Severity):** The spine pins `Microsoft.EntityFrameworkCore.SqlServer` to 9.0.22, but leaves brownfield references to sibling packages (`Microsoft.EntityFrameworkCore`, `Design`, `Tools`) at 9.0.6, which will cause runtime assembly mismatches.

---

## 2. Rubric Evaluation Matrix

| # | Checklist Criterion | Status | Primary Finding & Section |
| :--- | :--- | :---: | :--- |
| **1** | **Fixes real divergence points for 8 epics & misses none** | **ACTION NEEDED** | Missing `R25` invalidation interface in AD-2; circular `REG <-> CERT` coupling; ambiguous "My Events" composition across e3, e4, e6. |
| **2** | **Every AD Rule is enforceable & prevents stated divergence** | **ACTION NEEDED** | AD-4 omits Member Signup from per-participant lock order; AD-3 lacks RCSI, enabling reader-writer deadlocks. |
| **3** | **Nothing under Deferred could let two epics diverge** | **PASS** | All 8 deferred items (QR library, Argon2 tuning, cert template format, renderer timeouts, scanner JSON, report isolation, hotspot cert, log retention) are genuinely isolated to single modules. |
| **4** | **Named tech is verified-current (memlog versions)** | **PASS WITH NOTE** | Versions match 2026-11-04 web verification. Open Question 1 (.NET 9 EOL on 2026-11-10) and Open Question 2 (AutoMapper CVE) should be resolved rather than deferred. |
| **5** | **Ratifies rather than contradicts brownfield codebase** | **PASS WITH NOTE** | Preserves 4 projects, Serilog, TempData, `IUnitOfWork`, and file storage layout. Small diagram note: WebApp does not implement `IActivityLogWriter`. |
| **6** | **Covers every ticket requirement (R1-R36, Constraints, Notes)** | **PASS** | Comprehensive coverage across ADs and Capability Map. All 36 functional IDs, NFR-1..NFR-7, and 11 constraints mapped. |
| **7** | **Operational envelope decided, deferred, or open question** | **PASS WITH NOTE** | Environments, SQL containers, Kestrel LAN HTTPS, and secrets covered in AD-16. Playwright offline browser installation on demo host needs explicit note. |
| **8** | **Decisions not rationale (memlog holds rationale)** | **PASS** | Exemplary discipline. Terse `Binds`, `Prevents`, and `Rule` format maintained throughout without conversational debate. |
| **9** | **Diagrams valid mermaid and carrying structure** | **PASS WITH NOTE** | All 4 diagrams are syntactically valid mermaid. Services module diagram has an inverted arrow between `REG` and `CERT`. |
| **10** | **No template comments or placeholders** | **PASS** | Zero template markers (`<!-- -->`), `TODO`, `TBD`, or `FIXME`. Mechanical linter flagged `{controller}/{action}` route tokens (verified benign). |

---

## 3. Tiered Findings & Proposed Fixes

### Finding 1: AD-4 Concurrency & Lock Hierarchy — Omission of Member Signup & Venue/Event Concurrency Race
- **Severity:** `HIGH`
- **Location:** AD-4 (lines 118–120)
- **Evidence:**
  > `architecture-attendta.md:118-120`:  
  > `"- Rows in registrations, attendance and certificates are written only while holding their event's lock."`  
  > `"- Per-participant operations lock the owner's users row first, then the event. These are approve, decline, promote, member cancel, admin cancel, check-in, attendance invalidation, eligibility decision and issuance."`  
  > `"- Event-wide operations lock only the event. These are publish, edit, cancel, complete and the check-in toggle."`
- **Analysis:**
  1. Member Signup (`R14`) writes a new row to `registrations`. However, line 119 omits `signup` from the explicit list of per-participant operations that lock the owner's `users` row followed by the `events` row. If an `epic-events-registration` developer locks only the `events` row or locks `events` before checking/locking `users`, a concurrent Account Deactivation (`R32`, which locks `users` then `events` in ascending order) will produce a classic AB/BA deadlock. Furthermore, without locking `users`, a member whose account is being deactivated could successfully register for an event.
  2. AD-4 states that event-wide operations lock only `events`. However, Venue Management (`R7`) requires that venue capacity never drops below the participant limit of any linked draft or published event. If an Officer edits a Venue's capacity (`e2`, locking `venues` at level 5) while another Officer edits an Event's participant limit or creates/publishes an event (`e3`, locking `events` at level 6), the two operations can interleave and commit an invalid state where event participant limit exceeds venue capacity.
- **Proposed Fix:**
  Update AD-4 lines 119–120 to read:
  ```markdown
  - Per-participant operations lock the owner's `users` row first, then the event. These are signup, approve, decline, promote, member cancel, admin cancel, check-in, attendance invalidation, eligibility decision and issuance.
  - Event-wide operations lock only the event. These are publish, edit, cancel, complete and the check-in toggle.
  - Cross-entity capacity checks: modifying an event's venue or participant limit, or publishing an event, must acquire the event's `venues` row lock (level 5) before acquiring the `events` row lock (level 6), ensuring venue capacity validation serializes against concurrent venue capacity edits.
  ```

---

### Finding 2: Module Dependency Cycle & Unspecified Cross-Module Invalidation Interface in AD-2
- **Severity:** `HIGH`
- **Location:** Design Paradigm diagram (lines 39–60) and AD-2 (lines 90–94)
- **Evidence:**
  > `architecture-attendta.md:48-49`:  
  > `CERT["Certificates"] --> CHK`  
  > `CERT --> REG`  
  > `architecture-attendta.md:91`:  
  > `"- Event completion. e3 owns it. e6 adds Certificate Review creation, through ICertificateReviewService, to e3's completion transaction."`
- **Analysis:**
  1. Line 39 states: *"Module dependencies within Services (the arrow points toward the module being called)"*. The diagram shows `CERT --> REG` (Certificates calls Events & Registration). Yet AD-2 line 91 states that on Event Completion, `e3` (`REG`) calls `e6` (`CERT`) via `ICertificateReviewService`. If `REG` calls `CERT` and `CERT` calls `REG`, this forms a compile-time/architectural circular dependency between modules inside `ASI.Basecode.Services`.
  2. In `R25`, Check-in (`e4`) invalidates an attendance record, which must immediately transition any unissued Certificate Review to `not_eligible`. AD-2 strictly rules: *"Each table has one repository class in Data. Only the module listed below may call write methods for the listed table or columns, and only from its own service."* AD-2 line 87 assigns `certificates` exclusively to Certificates (`e6`). AD-2 details cross-module writes for Event Completion and Check-in toggle, but completely omits the interface `e4` must call on `e6` during attendance invalidation. Without an explicit interface contract, `e4` and `e6` will diverge on how reviews are updated upon invalidation.
- **Proposed Fix:**
  1. In the Services diagram, clarify that collaborator interfaces live in `ASI.Basecode.Services/Interfaces` and decouple callers, or invert the arrow so `REG --> CERT` represents the completion review creation call.
  2. In AD-2 (under *"Cross-module writes go through the owner's service, inside the caller's transaction"*), add:
  ```markdown
  - **Attendance invalidation (R25).** e4 owns the invalidation command and rules. It updates Certificate Reviews through `ICertificateReviewService.InvalidateReviewForAttendanceAsync(attendanceId, reason, ct)`, which e6 implements, inside e4's invalidation transaction.
  ```

---

### Finding 3: Default SQL Server Read Committed vs. Reader-Writer Starvation and Deadlocks
- **Severity:** `HIGH`
- **Location:** AD-3 (line 99) and AD-16 (lines 245–253)
- **Evidence:**
  > `architecture-attendta.md:99`:  
  > `"Each mutating use case runs inside IUnitOfWork.ExecuteInTransactionAsync(work, ct). This opens one READ COMMITTED transaction on the request-scoped DbContext, runs work, calls SaveChangesAsync once and commits. On any exception it rolls back."`
- **Analysis:**
  On standard SQL Server instances (including LocalDB and Linux containers), the default `READ COMMITTED` isolation level acquires shared locks (`S`) for all SELECT queries and holds exclusive locks (`X`) for writes. Shared locks block exclusive locks, and exclusive locks block shared locks.
  During an active event, door check-in produces a continuous stream of mutations on `events` and `attendance`. Under default `READ COMMITTED`, read-only queries (e.g. students refreshing "My Events", officers viewing attendance rosters, or background report counts) will acquire `S` locks that block check-in transactions, and check-in row locks will block reads. This leads to HTTP timeouts and classic reader-writer conversion deadlocks.
- **Proposed Fix:**
  In AD-3, AD-10, and AD-16, mandate Read Committed Snapshot Isolation (RCSI):
  ```markdown
  - The database must have `READ_COMMITTED_SNAPSHOT ON` enabled in `database/attendta-mvp-schema.sql`.
  - Mutating transactions execute under READ COMMITTED with snapshot reads enabled: read queries read the last committed row version without acquiring `S` locks, completely eliminating reader-writer blocking while mutations serialize via AD-4 row locks.
  ```

---

### Finding 4: Missing Canonical Participation Facts for Reporting & Ambiguous "My Events" Aggregation
- **Severity:** `MEDIUM`
- **Location:** AD-5 (lines 131–139) and Capability Map (lines 353–354)
- **Evidence:**
  > `architecture-attendta.md:131-139`: ParticipationFacts table includes: Eligible Participant, Valid Attendance, Seat held, Waitlist priority, Event Start Cutoff passed, Signup open.  
  > `architecture-attendta.md:353-354`:  
  > `R14–R20 signup, review, promotion, cancellation, My Events | Events & Registration (e3): Registration, MyEvents`  
  > `R21–R26 QR Pass, check-in, invalidation | Check-in (e4): Attendance, MyEvents`
- **Analysis:**
  1. `R30` mandates reporting on "No-shows (Completed Events only)" and "attendance rate (N/A with no Confirmed)". `R19` clarifies that pending and waitlisted rows at cutoff are *not* no-shows. Leaving "No-show" and "Attendance Rate" out of `ParticipationFacts.cs` allows `e7` (Reports) and `e3`/`e4` to compute contradictory participation metrics.
  2. "My Events" (`R20`) requires presenting a Member's registrations with status (`e3`), QR Pass when confirmed (`e4`), attendance record state including invalidation (`e4`), and certificate download links (`e6`). Assigning `MyEvents` across `e3` and `e4` without defining the assembly point risks circular references in Services or fragmented controllers in WebApp.
- **Proposed Fix:**
  1. Add the following canonical definitions to the table in AD-5:
  ```markdown
  | No-show | Registration status = confirmed on a completed event AND Valid Attendance does not exist |
  | Attendance Rate | Valid Attendees / Confirmed Registrations (evaluates to null / "N/A" when Confirmed = 0) |
  ```
  2. In AD-1 and Consistency Conventions, specify:
  ```markdown
  `MyEventsController` lives in `WebApp` and serves the personal dashboard. It injects read-only query interfaces from `e3` (registrations), `e4` (attendance/QR), and `e6` (certificates) to construct the unified `MyEventsViewModel`, preserving the unidirectional Services DAG.
  ```

---

### Finding 5: AutoMapper CVE-2026-32933 Blocks CI Vulnerability Scanning
- **Severity:** `MEDIUM`
- **Location:** Stack (line 292), AD-18 (line 270), and Open Questions (line 376)
- **Evidence:**
  > `architecture-attendta.md:270`: `"AutoMapper stays pinned at 14.0.0 and maps only flat view models. It never maps entity graphs."`  
  > `architecture-attendta.md:376`: `"2. AutoMapper 14.0.0 has an unpatched CVE (CVE-2026-32933). Options: keep it for flat maps only, take the Lucky Penny community license for 16.x, or switch to explicit mapping. Owner: Technical Lead."`
- **Analysis:**
  While AD-18 specifies that AutoMapper will only be used for flat maps to avoid recursive DoS (CVE-2026-32933), standard security analyzers in CI (`dotnet list package --vulnerable`, Dependabot, Snyk) flag CVE-2026-32933 as a High Severity vulnerability regardless of code-level call patterns. Because version 14.0.0 is the terminal MIT release and 15+ transitioned to commercial licensing, keeping 14.0.0 creates a recurring obstacle for green CI gates (SM-3).
- **Proposed Fix:**
  Resolve Open Question 2 by updating AD-18 and Stack to remove AutoMapper entirely:
  ```markdown
  - Remove AutoMapper from all projects.
  - Map models using explicit static extension methods in `ASI.Basecode.Services/Mapping/` (e.g. `ToViewModel()` / `ToEntity()`). Explicit mapping delivers compile-time safety, zero reflection overhead, and eliminates CVE-2026-32933.
  ```

---

### Finding 6: Unspecified `activity_logs.description` in AD-7 Audit Payload Contract
- **Severity:** `MEDIUM`
- **Location:** AD-7 (lines 154–159)
- **Evidence:**
  > `database/attendta-mvp-schema.sql:210`:  
  > `[description] nvarchar(max) NOT NULL,`  
  > `architecture-attendta.md:154-159`: AD-7 defines `IActivityLogWriter.Append(AuditEntry)`, action codes, entity types, old/new JSON, actor, and time, but is silent on `description`.
- **Analysis:**
  `activity_logs.description` is defined as `NOT NULL` in the canonical database schema. PRD FR-33 states each entry stores *"description or reason"*. If `AuditEntry` does not mandate a non-null `Description`, or if individual epic developers supply only `Reason`, database inserts will fail with a SQL NOT NULL violation (`Cannot insert the value NULL into column 'description'`).
- **Proposed Fix:**
  In AD-7, define the explicit `AuditEntry` properties:
  ```markdown
  - `AuditEntry` is a C# record requiring `Action` (constant), `EntityType` (table name), `EntityId` (int), and `Description` (concise human-readable text, defaulting to `$"{Action} on {EntityType} #{EntityId}"`), with optional `Reason` (string), `OldValues` (object), and `NewValues` (object). `IActivityLogWriter.Append` guarantees `description` is never null or empty.
  ```

---

### Finding 7: EF Core Sibling Package Version Fragmentation
- **Severity:** `LOW`
- **Location:** Stack (line 290)
- **Evidence:**
  > `architecture-attendta.md:290`: `"Microsoft.EntityFrameworkCore.SqlServer | 9.0.22 (repo at 9.0.6)"`  
  > `ASI.Basecode.Data.csproj:10-16`: References `Microsoft.EntityFrameworkCore` 9.0.6, `Microsoft.EntityFrameworkCore.Design` 9.0.6, `Microsoft.EntityFrameworkCore.Tools` 9.0.6.
- **Analysis:**
  The spine explicitly upgrades `Microsoft.EntityFrameworkCore.SqlServer` to 9.0.22, but does not explicitly instruct developers to upgrade the companion EF Core packages (`Microsoft.EntityFrameworkCore`, `Design`, `Tools`). Mixing 9.0.6 core abstractions with a 9.0.22 provider can cause assembly binding warnings or runtime `MissingMethodException` during DbContext initialization.
- **Proposed Fix:**
  In Stack and AD-18, specify:
  ```markdown
  - All Microsoft.EntityFrameworkCore.* packages (`Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools`) must be aligned in lockstep to `9.0.22`.
  ```

---

### Finding 8: Offline Playwright Chromium Availability on Demo Host
- **Severity:** `LOW`
- **Location:** AD-16 (lines 245–253) and Structural Seed (line 309)
- **Evidence:**
  > `architecture-attendta.md:249`: `"Demo | Local SQL Server instance; ... Kestrel HTTPS endpoint bound to the LAN or hotspot interface ..."`
- **Analysis:**
  The `Microsoft.Playwright` NuGet package does not bundle Chromium browser binaries. On an offline demonstration host connected only to an isolated Wi-Fi hotspot (NFR-6), attempting to download Chromium at runtime will fail, preventing certificate PDF generation.
- **Proposed Fix:**
  Add a bullet point to AD-16:
  ```markdown
  - On the Demo host, Playwright Chromium binaries must be pre-installed during machine staging via `pwsh bin/Debug/net9.0/playwright.ps1 install chromium` before disconnecting from external networks.
  ```

---

## 4. Detailed Rubric Checklist Audit

### 4.1. Divergence Points Between the 8 Epics
The spine successfully identifies and resolves the vast majority of cross-epic boundaries:
- **e1 (Platform Access) $\leftrightarrow$ e2 (Master Data):** Handled cleanly via AD-2 and AD-8. e1 establishes user identities and authentication; e2 manages member profiles. (Note: `members (create on approval)` is correctly assigned to e1 in AD-2, reconciling a note in `epic-platform-access.md`).
- **e2 (Master Data) $\leftrightarrow$ e3 (Events & Registration):** Handled cleanly via AD-12 (shared private image store for venue photos and event covers) and AD-10 (event types and venues).
- **e3 (Events & Registration) $\leftrightarrow$ e4 (Check-in):** Check-in window toggle is cleanly delegated via `IEventLifecycleService.SetCheckInWindowAsync`. However, as noted in Finding 1, signup locking must be explicitly unified with per-participant locking.
- **e4 (Check-in) $\leftrightarrow$ e6 (Certificates):** **Gap identified.** As captured in Finding 2, Attendance Invalidation (`R25`) transitions unissued Certificate Reviews to `not_eligible`. AD-2 must define `ICertificateReviewService.InvalidateReviewForAttendanceAsync`.
- **e3 (Events & Registration) $\leftrightarrow$ e5 (Administration):** Handled cleanly via AD-6 (`IRegistrationCancellationService.CancelAsync`).
- **e3 / e4 $\leftrightarrow$ e7 (Reports):** **Gap identified.** As captured in Finding 4, "No-show" and "Attendance Rate" must be codified in AD-5 to prevent conflicting report math.

### 4.2. Rule Enforceability & Divergence Prevention (AD-1 to AD-18)
Every AD was audited for enforceability:
- **AD-1 (Layered monolith):** Enforceable via `.csproj` project reference restrictions and architecture tests.
- **AD-2 (Writing module per table):** Enforceable via code review, interface segregation, and repository unit tests.
- **AD-3 (Caller-owned unit of work):** Enforceable via unit tests asserting `IUnitOfWork` ambient transactions. (Requires RCSI per Finding 3).
- **AD-4 (Lock order):** Enforceable via SQL Server concurrency tests (`SM-4`, `tests/ASI.Basecode.IntegrationTests`). (Requires signup and venue/event additions per Finding 1).
- **AD-5 (Canonical facts):** Enforceable via static references to `ParticipationFacts.cs`. (Requires No-show addition per Finding 4).
- **AD-6 (One cancellation path):** Enforceable via search for registration status updates.
- **AD-7 (Audit writer):** Enforceable via `IActivityLogWriter.Append` and SQL trigger preventing update/delete. (Requires description contract per Finding 6).
- **AD-8 (Identity & cookies):** Enforceable via ASP.NET Core `CookieAuthenticationEvents` and policy authorization attributes.
- **AD-9 (Outcomes):** Enforceable via custom middleware and exception filter mapping `DomainException`.
- **AD-10 (Lookup states):** Enforceable via database FK constraints and enum matching.
- **AD-11 (One clock):** Enforceable via Roslyn analyzer / grep forbidding `DateTime.Now`/`DateTime.UtcNow` in Services.
- **AD-12 (File store):** Enforceable via controller action tests and storage path validation.
- **AD-13 (Paging):** Enforceable via `PageRequest`/`PagedResult` tests.
- **AD-14 (UI shell):** Enforceable via Tailwind CLI compilation in CI.
- **AD-15 (Argon2 hasher):** Enforceable via test vector verification in `Services.Tests`.
- **AD-16 (Environments):** Enforceable via GitHub Actions workflow and demo checklist.
- **AD-17 (Test harness):** Enforceable via `dotnet test` execution in CI.
- **AD-18 (Dependency hygiene):** Enforceable via package reference audit.

### 4.3. Deferred Items Audit
All 8 deferred items were audited against cross-epic divergence:
1. *Server QR & browser scanner library:* Confined to `e4`. QR token payload format (32 random bytes / 64 hex chars) is firmly bound in Consistency Conventions. **No divergence.**
2. *Argon2 m/t/p values:* Confined to `e1`. Hashes encode parameters in PHC format, allowing parameter tuning without invalidating earlier hashes. **No divergence.**
3. *Certificate number format & template layout:* Confined to `e6`. Database enforces unique certificate number. **No divergence.**
4. *Renderer concurrency & timeout:* Internal to Playwright adapter in `e6`. **No divergence.**
5. *Scanner JSON fields beyond `{outcome, message}`:* Confined to `e4` scanner endpoint and view. **No divergence.**
6. *Report read isolation:* Confined to `e7`. Reports are read-only. **No divergence.**
7. *HTTPS certificate mechanism:* Confined to `e8` demo environment staging. **No divergence.**
8. *Log retention & Seq sink:* Out of scope per ticket. **No divergence.**

### 4.4. Technology Currency & Ecosystem Compatibility
- **Runtime:** .NET 9.0.22 verified. Open Question 1 correctly highlights the impending EOL on 2026-11-10; however, as `net9.0` is an explicit ticket constraint, maintaining `net9.0` with `DOTNET_ROLL_FORWARD=LatestMajor` allows execution on modern .NET 10 developer SDKs.
- **Database:** SQL Server 2025 container (`mcr.microsoft.com/mssql/server:2025-latest`) verified live on MCR.
- **Security:** `Konscious.Security.Cryptography.Argon2` (1.3.1) verified active and MIT-licensed.
- **UI:** `@tailwindcss/cli` (4.3.4) verified on npm.
- **Testing:** `xunit.v3` (4.0.2) verified on NuGet.
- **PDF:** `Microsoft.Playwright` (1.64.1) verified on NuGet.

### 4.5. Ticket Requirements & Constraints Coverage
- **Functional Requirements (R1–R36):** Traceable to specific ADs and capability map assignments with 100% coverage.
- **Constraints:**
  - Salted Argon2id, cookie auth, antiforgery, ownership checks, no throttling (NFR-1) $\rightarrow$ AD-8, AD-15.
  - Caller-owned transaction, audit, DB uniqueness (NFR-2) $\rightarrow$ AD-3, AD-7, AD-10.
  - No hard deletes, append-only log (NFR-3) $\rightarrow$ AD-7, AD-10.
  - Storage outside webroot, relative paths in SQL (NFR-4) $\rightarrow$ AD-12.
  - UTC datetime2(3), Asia/Manila display, no jobs (NFR-5) $\rightarrow$ AD-11, AD-16.
  - One host, Chrome, responsive phone scanning (NFR-6) $\rightarrow$ AD-1, AD-14, AD-16.
  - QR carries token only, safe error responses (NFR-7) $\rightarrow$ AD-9, Consistency Conventions.
  - .NET 9 MVC, EF Core DB-first, Tailwind, Playwright $\rightarrow$ AD-1, AD-10, AD-14, AD-18, Stack.
  - Lookup tables with FKs, NOT NULL description, issuer snapshot $\rightarrow$ AD-10.
  - Git Flow, PRs to dev, green CI, Conventional Commits $\rightarrow$ AD-16, AD-17, Consistency Conventions.

### 4.6. Operational Envelope
- **Deployment & Environments:** Explicitly defined for Dev (LocalDB), CI (GitHub Actions + SQL Server 2025 container), and Demo (Local SQL Server + Kestrel HTTPS).
- **Secrets:** Bound to environment variables and user-secrets; committed keys removed (AD-16).
- **Observability:** Serilog correlation ID and structured events specified; audit separated from diagnostic logging.
- **Backup & Restore:** SQL database and `SetupRoot` filesystem backed up together to maintain relational integrity with relative file paths.

### 4.7. Diagram Syntax and Semantics
- Diagram 1 (Layered Monolith, lines 28–37): Valid mermaid flowchart TD. Correctly shows leaf Resources and WebApp implementing Services interfaces.
- Diagram 2 (Services Module Dependencies, lines 41–60): Valid mermaid flowchart LR. (Contains inverted coupling between REG and CERT addressed in Finding 2).
- Diagram 3 (Deployment Topology, lines 301–312): Valid mermaid flowchart LR. Cleanly depicts demo host boundaries, Kestrel HTTPS, and CI runner.
- Diagram 4 (Entity Relationships, lines 314–325): Valid mermaid erDiagram. Accurately models domain relationships.

### 4.8. Format & Discipline
- The spine adheres strictly to "Decisions, not rationale". It avoids narrative debates, delegating historical context to `.memlog.md`.
- No placeholders, TODOs, or unresolved template comments exist.

---

## 5. Summary Recommendation for Finalize

The spine is in outstanding shape and ready for finalization once the following targeted edits are applied:
1. **Apply Finding 1:** Add `signup` to the AD-4 per-participant lock order and add the venue/event cross-entity locking rule.
2. **Apply Finding 2:** Add `ICertificateReviewService.InvalidateReviewForAttendanceAsync` to AD-2 cross-module writes and reconcile the module diagram coupling.
3. **Apply Finding 3:** Mandate `READ_COMMITTED_SNAPSHOT ON` in AD-3 and AD-16.
4. **Apply Finding 4:** Add "No-show" and "Attendance Rate" to AD-5 and specify `MyEventsController` composition in WebApp.
5. **Apply Finding 5:** Resolve Open Question 2 by dropping AutoMapper in favor of explicit static mapping extensions.
6. **Apply Finding 6:** Define `AuditEntry.Description` non-null contract in AD-7.
7. **Apply Findings 7 & 8:** Note EF Core sibling package alignment and offline Playwright installation in AD-16/AD-18.
