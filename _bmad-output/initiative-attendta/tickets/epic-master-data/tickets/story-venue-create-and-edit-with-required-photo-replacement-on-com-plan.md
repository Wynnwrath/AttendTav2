---
title: 'Venue create and edit with safe photos and capacity guard'
type: 'feature'
ticket: '3'
created: '2026-11-05'
status: 'built'
baseline_revision: '6ebadb67e7781a7951ad74a98ec5003711afddc5'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context:
  - '_bmad-output/initiative-attendta/tickets/epic-master-data/tickets/story-venue-create-and-edit-with-required-photo-replacement-on-com.md'
  - 'AGENTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Venue edits lack a linked-event capacity guard and audit trail, and photo replacement is orchestrated outside the service without a reliable commit boundary.

**Approach:** Put private-photo lifecycle and audited transactional writes in the venue service; lock the venue before checking linked events and editing its fields.

## Boundaries & Constraints

**Always:** Require a positive capacity and a private venue-area photo; preserve Management authorization, antiforgery, server-generated paths, 2 MiB JPEG/PNG validation, authenticated actor and UTC timestamps. Create/edit and their audit entries commit atomically. Clean up a new photo on failure; delete the old photo only after the actual successful commit. Never include image paths in audit or logs. Preserve the lock ordering contract: future event writers lock the venue before making competing changes.

**Never:** Implement event management, venue status changes beyond their existing behavior, hard deletion, schema migration, or private static-file serving. Do not assume a nested transaction has committed when its inner call returns.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Create | Management supplies valid fields and image | Venue and path-free audit committed | Invalid image leaves no venue or file |
| Edit | No new photo | Update details and retain existing photo | Reject invalid capacity |
| Capacity guard | Linked draft/published event limit 30, capacity 29 | No venue or audit change | Invalid outcome on Capacity |
| Capacity boundary | Capacity equals maximum linked limit, or only cancelled/completed events exceed it | Update succeeds | No error |
| Concurrent edit | Another connection holds venue row lock | Edit waits, then rechecks event limit and current photo | Cancellation/failure leaves files and DB consistent |
| Replacement | New file saved and commit succeeds | New path committed; old file removed afterward | Cleanup failure must not undo committed DB change |
| Commit failure | New file saved; database commit fails | Old path and file retained; new file removed | Propagate failure |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Controllers/VenueController.cs` owns upload and cleanup today; `Views/Venue/_Form.cshtml` displays field errors. Keep its policy, antiforgery and authorized photo endpoint.
- `ASI.Basecode.Services/Services/VenueService.cs`, `Interfaces/IVenueService.cs` and `ServiceModels/VenueViewModel.cs` handle venue writes; `IPrivateFileStore` provides stream-based storage and validates images.
- `ASI.Basecode.Data/Repositories/VenueRepository.cs` and `Interfaces/IVenueRepository.cs` lack a lock and linked-event query; follow `UserRepository.LockUserAsync` for transaction-bound lock and tracked-row refresh. `Models/Lookups/LookupCodes.cs` defines event status codes.
- `ASI.Basecode.Data/UnitOfWork.cs` joins existing transactions; an inner return is not proof of commit. `Services/ActivityLogWriter.cs`, `Resources/Constants/AuditActions.cs` and `Resources/Messages/AuditDescriptions.cs` support transactional, explicit, non-sensitive audit payloads.
- `tests/ASI.Basecode.IntegrationTests/VenuePhotoFlowTests.cs`, `MasterDataTransactionTests.cs`, and `TransactionVerificationTests.cs` provide authenticated MVC, SQL fixtures and lock precedents.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.Data/Interfaces/IVenueRepository.cs` and `Repositories/VenueRepository.cs` — add a cancellable transactional row lock and a query for linked draft/published events above the proposed limit; acquire lock before the query.
- [ ] `ASI.Basecode.Services/Interfaces/IVenueService.cs`, `Services/VenueService.cs` — accept image streams without HTTP types, enforce validation, own storage and cleanup, update locked current state, and append audit inside the transaction. Ensure operations cannot mistake a joined transaction for a committed one; reject an existing ambient transaction before touching files if no safe commit callback is available.
- [ ] `ASI.Basecode.Resources/Constants/AuditActions.cs`, `Messages/AuditDescriptions.cs`, `ASI.Basecode.Services/Services/ActivityLogWriter.cs` — add supported venue create/edit actions and explicit old/new field subsets with photo-change indication but no image key.
- [ ] `ASI.Basecode.WebApp/Controllers/VenueController.cs` and `Views/Venue/_Form.cshtml` — pass uploads to the service, map field-specific domain failures into ModelState, and preserve authorized photo reads and other behavior.
- [ ] `tests/ASI.Basecode.IntegrationTests/` — adapt existing test doubles and add isolated SQL and photo lifecycle coverage for the matrix, including a real blocked row lock and an injected commit failure; assert audit rollback and no image paths.

**Acceptance Criteria:**
- Given an Officer, when they create a venue with a photo and subsequently edit it, then its details and authorized photo are available and each successful write has a path-free activity entry.
- Given an existing external transaction, when a venue write cannot reliably observe its commit, then it is rejected before any file or database changes.
- Given a competing row lock, when an edit is attempted, then it waits until the lock is released and evaluates the current state.

## Implementation Notes

Implementation and integration tests were added, including lock contention and an injected pre-commit failure. Verification is blocked: `dotnet --list-sdks` reports no installed SDK, and `ATTENDTA_TEST_SQL` is unset. Build and both test commands fail before compilation; no matrix test has been verified as passing. Keep this plan in progress until the SDK and local SQL test connection are available and all tests pass. Review has not yet run.

The SDK was subsequently located at `%USERPROFILE%\.dotnet\dotnet.exe`; with that SDK and the local MSSQLLocalDB master connection, the build passed, 17 service tests passed, and 95 integration tests passed, with no skipped tests. Matrix scenarios are covered in `MasterDataTransactionTests.cs`, `VenuePhotoFlowTests.cs`, and `VenueTransactionPhotoTests.cs`.

After the review patch, the solution build, 17 service tests, and 96 integration tests passed with none skipped. The build reports one nullable warning in `MasterDataTransactionTests.cs`.

## Plan Change Log

## Review Triage Log

- medium — patch: `ToggleActiveAsync` read without the venue lock and the repository marks every field modified; a concurrent toggle could restore a replaced photo path and stale venue details. Fixed by taking the venue lock and added a passing concurrency regression test.

## Design Notes

EF transaction ownership must be explicit because `ExecuteInTransactionAsync` joins existing transactions. Unless a safe outer-commit hook exists, reject joined transactions for photo-owning create/edit rather than risking data loss. Deletion after commit is best effort and must not leak paths. The event module must follow the same venue-first locking protocol when it is built.

## Verification

- `dotnet build` — solution builds.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — service tests pass.
- `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` with `ATTENDTA_TEST_SQL` pointing to a local SQL Server master — SQL, concurrency and MVC integration tests pass.
