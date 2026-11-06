---
title: 'Master-data writes in the caller-owned transaction'
type: 'refactor'
ticket: '11'
created: '2026-11-05'
status: 'in-review'
baseline_revision: '7331f7948a7a2785c92dbccf65a76992ba835bf1'
route: 'full'
route_source: 'auto'
risk: 'high'
context:
  - '_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md'
  - 'AGENTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The Event Type, Venue and Member repositories save on every write and their services open no transaction, so a failure after a write leaves a partial row. Business failures also leave as `InvalidDataException`, outside the shared outcome contract.

**Approach:** Make the three repositories' mutating methods async and save-free, run each service write inside `IUnitOfWork.ExecuteInTransactionAsync`, and replace `InvalidDataException` with `DomainException` in the three services and their controllers.

## Boundaries & Constraints

**Always:** Repositories only stage changes; the service owns the transaction and calls `SaveChangesAsync` inside it; a nested call joins the open transaction. Keep the `actorUserId` parameters, `GetActorUserId`, the existing uniqueness checks, validation messages and master-data rules. Keep venue photo storage where it is.

**Never:** Change the schema, add EF migrations, add row locks, write audit rows, remove `actorUserId`, or move venue images to the shared file store.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Success | Valid create, edit or toggle | One row written and committed by the service transaction | No error |
| Failure after write | Work throws after the repository staged or saved the row | No row persists; tracked changes cleared | Original exception rethrown |
| Nested | Service called inside an open transaction | Service joins it; only the outer owner commits | No independent commit |
| Invalid input | Blank name/description/photo, duplicate name or student number, unknown id, actor 0 | Refused with the matching outcome | `DomainException`: Invalid, Duplicate, NotFound or Forbidden |
| Storage failure | Bad venue photo | Refused and any saved image cleaned up | `DomainException` FileFailure |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Interfaces/{IEventType,IVenue,IMember}Repository.cs` and `Repositories/{EventType,Venue,Member}Repository.cs` — six mutating methods become `Task …Async(…, CancellationToken)`; drop every `UnitOfWork.SaveChanges()`; reads stay sync.
- `ASI.Basecode.Services/Services/{EventType,Venue,Member}Service.cs` — inject `IUnitOfWork`; validate first, then `ExecuteInTransactionAsync(async ct => { read, re-check uniqueness, mutate, await repo…Async, await SaveChangesAsync })`.
- `ASI.Basecode.Resources/DomainException.cs` — the `OutcomeCode` to throw. `ASI.Basecode.Data/UnitOfWork.cs` already joins nesting, rolls back and maps 2601/2627 to `Duplicate`; do not change.
- `ASI.Basecode.WebApp/Controllers/{EventType,Venue,Member}Controller.cs` — actions become `async Task<IActionResult>`, pass `HttpContext.RequestAborted`, catch `DomainException`; `VenueController` keeps its image cleanup. `Storage/VenueImageStorage.cs` throws `DomainException(FileFailure)` so one catch handles it; behavior and location unchanged.
- `ASI.Basecode.Services/Interfaces/{IEventType,IVenue,IMember}Service.cs` — async signatures, including `CreateMemberFromApprovedAccountRequestAsync`.
- `tests/ASI.Basecode.IntegrationTests/MasterDataTransactionTests.cs` — new; follows `TransactionVerificationTests` (fixture, two contexts, real `UnitOfWork`).
- `AGENTS.md` — rename the documented member-creation method. Do not change views, schema, DI, `IUnitOfWork` or AutoMapper profiles.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.Data/Interfaces/*Repository.cs` and `Repositories/*.cs` — convert the six mutating methods to `…Async(…, CancellationToken)` with no save — removes per-write commits.
- [ ] `ASI.Basecode.Services/Services/EventTypeService.cs`, `VenueService.cs`, `MemberService.cs` — inject `IUnitOfWork`; wrap every write; throw `Forbidden`/`Invalid`/`Duplicate`/`NotFound`; missing venue photo is `Invalid` on `ImagePath`; duplicate member profile or student number is `Duplicate`.
- [ ] `ASI.Basecode.Services/Interfaces/*Service.cs` — async signatures with `CancellationToken`.
- [ ] `ASI.Basecode.WebApp/Controllers/EventTypeController.cs`, `VenueController.cs`, `MemberController.cs` — await the service, catch `DomainException`, drop `System.IO` where it only served `InvalidDataException`; keep venue image save/cleanup and update its cleanup filter to `DomainException`.
- [ ] `ASI.Basecode.WebApp/Storage/VenueImageStorage.cs` — replace its six `InvalidDataException` throws with `DomainException(FileFailure, …)`.
- [ ] `tests/ASI.Basecode.IntegrationTests/MasterDataTransactionTests.cs` — for each service: a stand-in repository that saves then throws proves no row survives; a plain call proves the row commits; assert outcome codes for the matrix.
- [ ] `AGENTS.md` — rename the documented member-creation method.

**Acceptance Criteria:**
- Given a service write whose repository saves and then fails, when the service returns the failure, then the row is absent.
- Given a valid service write with no outer transaction, when it completes, then the row is committed.
- Given a service write inside an outer transaction that later throws, when the outer work fails, then no row persists.
- Given invalid input or a missing record, when the service is called, then it throws `DomainException` with the matching `OutcomeCode` and no row changes.
- Given the six changed files, when searched for `InvalidDataException`, then none is found; when the three repositories are searched for `SaveChanges`, then none is found.

## Implementation Notes

Implemented on .NET SDK 10.0.403 against LocalDB. Release build clean; 16 service tests and 62 integration tests pass (14 new in `MasterDataTransactionTests`). Service writes open or join the transaction and save once; repositories stage only. `VenueImageStorage` reports `FileFailure`, and the venue controller's cleanup filter follows it. No schema, DI, view or AutoMapper changes.

## Design Notes

`ExecuteInTransactionAsync` already joins a nested scope, so the service is safe standalone and inside a caller's transaction. Validation that needs no database runs before the transaction; existence and uniqueness reads run inside it. EF stages changes; only the service saves.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — expected: no errors.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --configuration Release` — expected: all pass.
- With `ATTENDTA_TEST_SQL` set to a local SQL Server master connection, `dotnet test --project tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --configuration Release` — expected: all pass, including the new class.
- `Select-String -Path ASI.Basecode.Data/Repositories/EventTypeRepository.cs,ASI.Basecode.Data/Repositories/VenueRepository.cs,ASI.Basecode.Data/Repositories/MemberRepository.cs -Pattern 'SaveChanges'` — expected: no matches.
- `Select-String -Path ASI.Basecode.Services/Services/EventTypeService.cs,ASI.Basecode.Services/Services/VenueService.cs,ASI.Basecode.Services/Services/MemberService.cs,ASI.Basecode.WebApp/Controllers/EventTypeController.cs,ASI.Basecode.WebApp/Controllers/VenueController.cs,ASI.Basecode.WebApp/Controllers/MemberController.cs -Pattern 'InvalidDataException'` — expected: no matches.
