---
title: 'Active Event Type and Venue read queries for event lifecycle'
type: 'feature'
ticket: '5'
created: '2026-11-06'
status: 'built'
baseline_revision: 'f0d7ebae9bab040df3cfcdd87670f3637388c16e'
route: 'full'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context:
  - 'AGENTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The event lifecycle needs a Data-layer way to select active Event Types and Venues, while existing inactive references must remain resolvable.

**Approach:** Add explicit active-only repository reads and verify their filtering and existing lookup behavior against SQL Server.

## Boundaries & Constraints

**Always:** Filter active status in the database; retain inactive rows and unfiltered single-record lookup behavior. Preserve existing management listing, locking, and write contracts. Support cancellation consistent with existing async repository reads.

**Never:** Implement Event management, eligibility facts, schema changes, hard deletes, or change existing references. Active list results alone do not substitute for the event writer's required transaction and row locking when validating competing updates.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Mixed status | Active and inactive types and venues | Each active read returns only its active rows | No error |
| No active rows | All rows inactive or none exist | Each active read returns an empty list | No error |
| Existing reference | Inactive type or venue ID | Existing single lookup still returns the inactive row | No error |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Interfaces/IEventTypeRepository.cs` and `ASI.Basecode.Data/Repositories/EventTypeRepository.cs` — add an async active-list contract and EF query; keep `GetEventType` unfiltered and use existing no-tracking, name/id ordering conventions.
- `ASI.Basecode.Data/Interfaces/IVenueRepository.cs` and `ASI.Basecode.Data/Repositories/VenueRepository.cs` — add the corresponding venue contract and query; keep `GetVenue` unfiltered and follow room-name/id ordering.
- `tests/ASI.Basecode.Services.Tests/EventTypeTimestampTests.cs` and `tests/ASI.Basecode.IntegrationTests/MasterDataTransactionTests.cs` — update repository fakes/wrappers for the added contracts, delegating where appropriate.
- `tests/ASI.Basecode.IntegrationTests/SqlServerDatabaseFixture.cs` and existing `EventTypeManagementTests.cs` / `VenuePagedListTests.cs` — reuse isolated canonical-schema SQL fixture and seeding conventions for a new repository integration test.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Data/Interfaces/IEventTypeRepository.cs`, `ASI.Basecode.Data/Repositories/EventTypeRepository.cs` — add cancellable async active Event Type list, filtered in SQL, no tracking, consistently ordered.
- [x] `ASI.Basecode.Data/Interfaces/IVenueRepository.cs`, `ASI.Basecode.Data/Repositories/VenueRepository.cs` — add equivalent active Venue list.
- [x] `tests/ASI.Basecode.Services.Tests/EventTypeTimestampTests.cs`, `tests/ASI.Basecode.IntegrationTests/MasterDataTransactionTests.cs` — update affected test implementations to satisfy contracts without changing existing behavior.
- [x] `tests/ASI.Basecode.IntegrationTests/ActiveReferenceQueryTests.cs` — seed mixed active/inactive records and verify exact active lists, empty results and unfiltered inactive lookups using the actual SQL-backed repositories.

**Acceptance Criteria:**
- Given active and inactive Event Types and Venues, when each active query runs, then only active records of the corresponding kind are returned.
- Given no active records, when each active query runs, then it returns an empty list.
- Given an inactive Event Type or Venue, when looked up by its existing ID-based repository lookup, then it remains available.

## Implementation Notes

The active queries, repository fakes and SQL integration test are implemented. The solution builds and all 75 service tests pass. The implementer ran the focused SQL test successfully. Full integration verification remains blocked: an initial run reported 151 passed and 8 failures, including missing Poppler `pdftotext` and unrelated authentication failures; a subsequent full run exhausted SQL Server's internal memory pool, reported database fixture failures and timed out. No review or completion is claimed until verification succeeds.

Subsequent verification: the full SQL integration suite passed all 159 tests with no skips using the xUnit executable in serial mode and a temporary Poppler installation on PATH. The solution build and 75 service tests passed. An independent quick review found no issues.

## Plan Change Log

## Review Triage Log

- Quick review: zero findings (high 0, medium 0, low 0, false 0, maybe-false 0).

## Design Notes

The lists are selection reads; future Event writes must acquire the existing event-type and venue locks and recheck current state inside their own transaction to avoid racing deactivation.

## Verification

**Commands:**
- `dotnet build` — solution builds.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — service tests pass.
- With `ATTENDTA_TEST_SQL` set to a local SQL Server master connection, `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` — SQL integration tests pass, including active and inactive reference coverage.
