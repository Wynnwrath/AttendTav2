---
title: 'Read-only Activity Log viewer'
type: 'feature'
ticket: '8'
created: '2026-11-06'
status: 'built'
baseline_revision: 'ba67ca030d3cd35c764a1f9421f07034052956b1'
route: 'full'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Administrators cannot inspect the existing activity trail in the application.

**Approach:** Provide an administrator-only, read-only, paged activity log at `/admin/auditlogs`.

## Boundaries & Constraints

**Always:** Show 25 entries per page ordered by occurred time descending and log ID descending. Display Asia/Manila time with its label, actor name and type, action, affected entity type and ID, and reason. Retain system-actor entries without a user. Enforce the Administrator policy on the server.

**Never:** Add editing, deletion, search, audit writing, or alter the schema or existing log records.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Empty | No entries | Clear empty state | No error |
| Paging | 26 or more entries, including equal timestamps | 25 per page, deterministic newest-first order | Normalize invalid and out-of-range pages using shared paging |
| Actor | System entry has no user | Show its actor type and a meaningful system actor label | Preserve the row |
| Access | Officer requests the route | Access refused | Existing authorization behavior |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Models/ActivityLog.cs` and `Models/Lookups/LookupCodes.cs` — audit fields and actor types; no user navigation, so use a left join to users.
- `ASI.Basecode.Data/Repositories/AccountRequestRepository.cs` and `Interfaces/IAccountRequestRepository.cs` — reference for asynchronous no-tracking count and ordered database paging.
- `ASI.Basecode.Services/Services/AccountRequestQueueService.cs`, `ServiceModels/PageRequest.cs`, and `PagedResult.cs` — existing normalized 25-row paging pattern.
- `ASI.Basecode.WebApp/Controllers/AccountRequestController.cs`, `Views/AccountRequest/Index.cshtml`, `Views/Shared/_Pager.cshtml`, and `Formatting/ManilaTimeFormatter.cs` — controller, list, pager and time formatting patterns.
- `ASI.Basecode.WebApp/Startup.DI.cs` — register the reader repository and service.
- `tests/ASI.Basecode.IntegrationTests/AccountRequestQueueTests.cs` — authenticated integration and pagination test pattern.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Data/Interfaces/`, `Repositories/`, `Models/` — add a read-only audit query and projection with count, left join for actor, stable database ordering and bounded paging.
- [x] `ASI.Basecode.Services/Interfaces/`, `Services/`, `ServiceModels/` — expose the paged results through the shared page request and result types.
- [x] `ASI.Basecode.WebApp/Startup.DI.cs` — register the new dependencies.
- [x] `ASI.Basecode.WebApp/Controllers/` and `Views/` — implement the administrator-only route and accessible read-only table, formatter, empty state and shared pager; add administrator navigation where appropriate.
- [x] `tests/ASI.Basecode.IntegrationTests/` — test the matrix with seeded entries, including tied timestamps, actor variants, pagination and authorization.

**Acceptance Criteria:**
- Given 26 seeded logs, when an Administrator opens the route and advances pages, then all entries appear in the required order with the specified fields and no edit or delete controls.
- Given an Officer, when they request the route, then access is refused.

## Implementation Notes

- Implemented the read-only viewer and integration coverage. Build and service tests passed; all 120 integration tests passed after installing the required Playwright browser. The focused activity-log test passed again after improving its timestamp coverage and generating a random test password.

## Plan Change Log

## Review Triage Log

- Quick review: 0 high, 0 medium, 0 low, 1 false, 0 maybe-false. The reviewer noted the existing Master Data ownership boundary; the user's explicit instruction to implement issue 59 authorizes this specific Administration feature, so that boundary does not prohibit the requested change.

## Verification

**Commands:**
- `dotnet build` — solution builds.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — service tests pass.
- `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` with `ATTENDTA_TEST_SQL` set to local SQL Server master — integration tests pass.
