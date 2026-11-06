---
title: 'Asia/Manila timezone display helper and UTC server-side time standard'
type: 'feature'
ticket: '4'
created: '2026-11-05'
status: 'built'
baseline_revision: '97145106aa3a3f1a3ee8acdc89943cc871577f7b'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
context:
  - 'AGENTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Master-data services read the system clock directly, making timestamps nondeterministic in tests; the application lacks a shared, labeled Manila-time formatter.

**Approach:** Use the injected `TimeProvider` for service timestamps and provide a shared WebApp Razor formatter that converts stored UTC times to labeled Asia/Manila time.

## Boundaries & Constraints

**Always:** Server-side times remain UTC `datetime2(3)`; service and data application clock reads use injected `TimeProvider.GetUtcNow()`. Existing transaction, validation, and master-data behavior must remain intact. Treat SQL `datetime2` values with unspecified `DateTime.Kind` as UTC. Formatting must be independent of the server's local timezone.

**Never:** Change the database schema or SQL defaults, implement unrelated features, or introduce standalone member creation. Do not change timestamp values already stored. No service-layer timezone conversion.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Injected time | Fake clock returns a fixed UTC instant | Service writes that exact UTC timestamp | No error |
| UTC display | 2026-01-01 18:30 UTC, including unspecified kind | Displays 2026-01-02 02:30 and Asia/Manila label | No error |
| Offset display | An instant with a nonzero offset | Displays its equivalent Manila time with label | No error |
| Missing display time | Nullable timestamp is null | Displays an empty value | No error |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/Services/{EventType,Member,Venue}Service.cs` — nine direct `DateTime.UtcNow` writes across create, edit and toggle; inject the clock while preserving transactional sequencing.
- `ASI.Basecode.Services/Services/ActivityLogWriter.cs`, `ASI.Basecode.WebApp/Startup.DI.cs` — already use and register `TimeProvider`; reuse the existing singleton registration. Current Data has no application-clock reads; SQL defaults are database-owned.
- `ASI.Basecode.WebApp/Views/_ViewImports.cshtml` — expose the shared formatter to Razor; no existing views currently display dates. Use a cross-platform Asia/Manila timezone conversion and an explicit zone label, not the host's local timezone.
- `tests/ASI.Basecode.IntegrationTests/MasterDataTransactionTests.cs` — direct service constructions need the new clock dependency; `TransactionVerificationTests.cs` shows an existing fixed-clock pattern.
- `tests/ASI.Basecode.Services.Tests/` references Services; `tests/ASI.Basecode.IntegrationTests/` references WebApp and can test its formatter.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Services/Services/{EventType,Member,Venue}Service.cs` — inject `TimeProvider` and use `GetUtcNow().UtcDateTime` for every timestamp.
- [x] `ASI.Basecode.WebApp/` and `ASI.Basecode.WebApp/Views/_ViewImports.cshtml` — add a single reusable formatter for UTC `DateTime` and `DateTimeOffset`, including nullable inputs; expose it in Razor and label output `Asia/Manila`.
- [x] `tests/ASI.Basecode.Services.Tests/` — use a fixed fake clock and test a real service's create/update/toggle timestamp behavior with test doubles, without relying on wall time.
- [x] `tests/ASI.Basecode.IntegrationTests/` — update direct constructors and test formatter rollover, UTC/unspecified kinds, offset instant and null.

**Acceptance Criteria:**
- Given a fixed injected clock, when a tested service creates, updates or toggles a record, then its written timestamp equals the clock's UTC instant.
- Given Services and Data source, when searched for application wall-clock reads, then no `DateTime.Now` or `DateTime.UtcNow` remains and all application time reads use the injected provider.
- Given a UTC database timestamp, when the Razor-accessible formatter renders it, then the correct Asia/Manila local date and time and zone label appear regardless of host timezone.
- Given the existing transaction tests, when they run against configured SQL Server, then they continue to pass.

## Implementation Notes

Implemented the service clock injection, shared formatter, constructor updates, and tests. Source search found no direct wall-clock reads in Services or Data. Build and tests were not run: the .NET 10 SDK and ATTENDTA_TEST_SQL are unavailable. The user requested skipping SDK-dependent verification to expedite a PR. Matrix tests are written but have not run; implementation remains in progress and review has not run.

Follow-up on 2026-11-05: independently reviewed merged PR #92 at `6ebadb67e7781a7951ad74a98ec5003711afddc5`. A Release build and all 17 service and 87 isolated LocalDB integration tests passed; the build reported three xUnit cancellation warnings. A follow-up rejects local-kind `DateTime` values and passes the test cancellation token. This independent review does not represent a BMAD review run or product acceptance.

Follow-up verification: Release build passed with zero warnings and errors; all 17 service and 88 isolated LocalDB integration tests passed, including rejection of local-kind values through both `DateTime` overloads.

## Plan Change Log

## Review Triage Log

## Design Notes

SQL Server `datetime2` loses `DateTime.Kind`; specify UTC for unspecified values before converting. A `DateTimeOffset` already identifies an instant and should be converted directly. Keep output deterministic, including the date when conversion crosses midnight.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln` — succeeds.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — service tests pass.
- With `ATTENDTA_TEST_SQL` set to a local SQL Server master connection, `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` — formatter and integration tests pass.
- `rg 'DateTime\.(Now|UtcNow)|DateTimeOffset\.(Now|UtcNow)' ASI.Basecode.Services ASI.Basecode.Data` — no application-clock matches.
