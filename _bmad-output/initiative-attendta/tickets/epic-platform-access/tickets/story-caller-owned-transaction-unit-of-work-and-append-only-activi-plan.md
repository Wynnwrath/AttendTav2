---
title: 'Caller-owned transaction and append-only audit writer'
type: 'feature'
ticket: '3'
created: '2026-11-05'
status: 'built'
baseline_revision: 'e50e833541f99f9daf3605c156177607d0743c90'
route: 'full'
route_source: 'auto'
risk: 'high'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 1
context:
  - '_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md'
  - 'AGENTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Related changes and audit entries lack a shared transaction. Diagnostics lack correlation and use a legacy path manager.

**Approach:** Add transactional audit, locking, duplicate handling and correlated logging under SetupRoot.

## Boundaries & Constraints

**Always:** Follow AD-1, AD-3, AD-4, AD-7 and AD-9. Use caller-controlled saves; nested calls join, outermost commits, failures roll back and clear tracking. Lock account requests before users by primary key; a user lock protects its member. Audit requires an authenticated actor, transaction, UTC, resource constants and templates, and safe field subsets. Preserve master-data behavior.

**Never:** Retry transactions, log secrets, serialize EF entities, open independent audit transactions, add anonymous/system application audit writes, change schema, or migrate master-data writes. Keep the audit trigger and filtered indexes.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Success | Business and audit changes saved in transaction | Both commit together | No error |
| Failure | Work saves then throws, including nested work | Both roll back; tracker cleared | Original failure rethrown |
| Nested | Existing transaction in same scoped context | Inner work joins; only owner commits | No independent commit |
| Lock | Two transactions lock same user | Second waits until first ends | Cancellation propagates |
| Refusal | No transaction, anonymous actor, secret/path/token field or entity payload | No audit row added | Explicit exception |
| Duplicate | SQL 2601 or 2627 including EF wrapper | Duplicate domain outcome | Other SQL errors preserved |
| Diagnostics | HTTP request emits a log line | Correlation ID visible; configured root used | No sensitive diagnostics |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Interfaces/IUnitOfWork.cs`, `UnitOfWork.cs`: master data uses sync SaveChanges; context is scoped; no retry configured.
- `ASI.Basecode.Data/Repositories/UserRepository.cs`, `Interfaces/IUserRepository.cs`: `User.Id` is PK, `User.UserId` is email. `Models/AccountRequest.cs` uses `RequestId`; no repository exists.
- `ASI.Basecode.Data/Models/ActivityLog.cs`, `CanonicalSchemaMappings.cs`: audit entity/mapping; schema blocks UPDATE/DELETE. `ASI.Basecode.Services/Interfaces/ICurrentActor.cs` provides identity; Data cannot reference Services. `Startup.DI.cs` registers services.
- `ASI.Basecode.Resources/`: shared leaf; no audit catalog exists. `Startup.Logger.cs`, `Startup.cs`, `ASI.Basecode.Data/PathManager.cs` contain legacy path references.
- `tests/ASI.Basecode.IntegrationTests/SqlServerDatabaseFixture.cs`: isolated fixture; `MemberRepositoryTests.cs` has an IUnitOfWork fake; `CanonicalSchemaTests.cs` tests immutability.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.Resources/` and `ASI.Basecode.Data/ASI.Basecode.Data.csproj`: define AD-9 outcome/exception contract; translate SQL 2601/2627, including wrapped exceptions, at Data save/transaction boundaries without leaking SQL details.
- [ ] `ASI.Basecode.Data/Interfaces/IUnitOfWork.cs`, `UnitOfWork.cs`: add cancellable Task/Task<T> transaction overloads; preserve sync saves, allow async identity flushes, nested join, rollback/clear; never save implicitly. For an externally opened transaction, join on success and roll it back and clear tracking on failure. Capture nested failures and rethrow the original exception even when outer work catches it.
- [ ] `ASI.Basecode.Data/Interfaces/IUserRepository.cs`, `Repositories/UserRepository.cs`, new `IAccountRequestRepository.cs` and `AccountRequestRepository.cs`: parameterized PK seek with UPDLOCK/HOLDLOCK/ROWLOCK, require an open transaction, pass cancellation; refresh already-tracked rows from the locked query before returning; return missing rows predictably. Register the request repository in `Startup.DI.cs`.
- [ ] `ASI.Basecode.Resources/Constants/AuditActions.cs`, Resources messages: create action constants and nonempty description templates for the initial supported action; keep the catalog extensible without accepting arbitrary actions or descriptions.
- [ ] `ASI.Basecode.Services/Interfaces/`, `ASI.Basecode.Services/Services/`, `Startup.DI.cs`: add scoped audit entry/writer using `ICurrentActor`, the shared scoped context, `TimeProvider.System` and resource templates. Accept only explicit field-subset payloads; validate recursively including serialized names and reject secret/password/hash/token/QR/path fields and EF entities. Add a row only; never save. Use user actor type, nullable reason and UTC timestamp.
- [ ] `ASI.Basecode.WebApp/Startup.Logger.cs`, `Startup.cs`, request middleware: use `Common:SetupRoot` and portable paths; enrich request logs with a correlation ID visible in sink output, without sensitive data. Delete `ASI.Basecode.Data/PathManager.cs` after removing references.
- [ ] `tests/ASI.Basecode.IntegrationTests/`, `tests/ASI.Basecode.Services.Tests/`: test every matrix case, both duplicate codes, nested and JSON-renamed sensitive names, actor/template/time and correlated output. Update the unit-of-work fake.

**Acceptance Criteria:**
- Given a business mutation and audit append, when the caller saves inside a successful transaction, then both rows persist; when it throws after saving, neither persists and tracked changes are cleared.
- Given nested work or concurrent locks, when exercised on independent SQL connections, then the inner work cannot commit early and the second user lock waits until release.
- Given invalid audit inputs or no transaction, when Append is called, then it refuses; valid input produces one user-attributed row with template, UTC and safe JSON.
- Given SQL failures, when writes execute, then only 2601/2627 produce Duplicate.
- Given an HTTP request, when a log line is emitted, then its correlation ID is visible; logs use SetupRoot and PathManager references are absent.

## Implementation Notes

Implemented and verified on .NET SDK 10.0.403 against LocalDB: Release build succeeded with no warnings; 16 service tests and 48 integration tests passed. Quick review fixed derived-entity audit validation; pre-existing venue-path diagnostics were deferred.

## Plan Change Log

- Review loop 1: Specify tracked-row refresh after locks, rollback and tracker clearing when joining an external transaction, preservation of caught nested failures, and secret-named field refusal. Avoid stale state, externally committed failed work, substituted errors, and secret leakage. KEEP: passing independent-connection transaction and locking tests, duplicate code tests, real correlation sink test, existing master-data compatibility, and production PathManager removal.

## Review Triage Log

| Finding | Verdict | Evidence | Route |
|---|---|---|---|
| Lock query may return tracked stale values | high | EF tracking identity resolution returns existing instances without overwriting current values; callers may read before acquiring locks. | bad_plan |
| External transaction survives failed joined work | high | Nested branch only sets a flag; an external transaction owner can catch and commit. | bad_plan |
| Caught nested exception replaced | medium | Outer branch throws a new InvalidOperationException when the failure flag is set. | bad_plan |
| Secret-named fields accepted | high | Sensitive() checks password/hash/token/path/qr but not secret. | bad_plan |
| Derived EF entity accepted as audit payload | high | Exact FindEntityType lookup misses unmapped subclasses whose base CLR type is mapped; inherited properties serialize. | patch |
| Existing venue cleanup logs paths | medium | VenueController logs its relative path and exception; that logging predates this change. | defer |
| Console and Debug lack Serilog correlation | false | The issue requires a Serilog log line with a correlation ID, not correlation in every independent provider; the configured file sink is covered. | reject |

## Design Notes

Resources holds the outcome contract to avoid a dependency cycle; Services hosts the writer to consume `ICurrentActor`. Master-data saves remain until story 1.11. Callers explicitly save before commit.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — solution builds.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --configuration Release` — all unit tests pass.
- Set `ATTENDTA_TEST_SQL` to `Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True;Encrypt=False`, then `dotnet test --project tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --configuration Release` — all integration tests pass against fixture-created disposable databases.
- `rg PathManager ASI.Basecode.Data ASI.Basecode.Services ASI.Basecode.WebApp --glob '*.cs'` — no production references (exit 1 means no matches).
