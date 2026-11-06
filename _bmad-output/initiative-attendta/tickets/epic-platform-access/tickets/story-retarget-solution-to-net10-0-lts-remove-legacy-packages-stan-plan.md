---
title: 'Retarget solution to .NET 10 and establish test CI baseline'
type: 'chore'
ticket: '10'
created: '2026-11-05'
status: 'in-progress'
baseline_revision: '0db098c7e68767c054eaf5e82411d3ee11c8574a'
route: 'full'
route_source: 'auto'
risk: 'high'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
context:
  - '_bmad-output/initiative-attendta/tickets/epic-platform-access/epic-platform-access.md'
  - 'AGENTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The solution is still on .NET 9, carries obsolete runtime packages, has no test projects, and has no GitHub Actions verification. Later authentication and data stories require a stable .NET 10, SQL-backed test baseline.

**Approach:** Retarget the solution projects and compatible Microsoft packages to .NET 10, remove only the packages explicitly retired by the ticket, create the two xUnit v3 test projects, and add a PR-to-`dev` workflow that runs the build and tests against SQL Server 2025.

## Boundaries & Constraints

**Always:** Keep the existing four-layer project references; retain AutoMapper 14.0.0 with NU1903 suppressed only at its Services and WebApp references; use xUnit v3; make the CI service wait for healthy SQL Server 2025; and target only pull requests to `dev`.

**Never:** Change feature behavior, silently update unrelated dependencies, add application authentication code, or mask build warnings globally.

**Decision:** Keep both legacy `ASI - Backup.*.csproj` files and retarget/clean them too, so every project file complies with the target framework and retired-package requirements.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Local build | Clean checkout with .NET 10 SDK | Solution builds with zero warnings | Resolve package/framework diagnostics directly |
| Local tests | Services and integration test projects | Both execute with `dotnet test` | Test failure makes the ticket incomplete |
| Pull request | PR targets `dev` | CI builds and runs tests against healthy SQL Server | Health check blocks tests until database is ready |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.sln` -- must include both new test projects.
- `ASI.Basecode.Data/ASI.Basecode.Data.csproj`, `ASI.Basecode.Services/ASI.Basecode.Services.csproj`, `ASI.Basecode.Resources/ASI.Basecode.Resources.csproj`, and `ASI.Basecode.WebApp/ASI.Basecode.WebApp.csproj` -- current .NET 9 targets and package references.
- `ASI.Basecode.Services/ASI - Backup.Basecode.Services.csproj` and `ASI.Basecode.WebApp/ASI - Backup.Basecode.WebApp.csproj` -- preserve these legacy copies, retarget to `net10.0`, and remove retired package references without adding them to the solution.
- `tests/ASI.Basecode.Services.Tests/` and `tests/ASI.Basecode.IntegrationTests/` -- new xUnit v3 projects required by the ticket.
- `.github/workflows/ci.yml` -- new PR workflow with the SQL Server service container and health check.

## Tasks & Acceptance

**Execution:**
- [x] All six project files, including both `ASI - Backup.*.csproj` copies -- retarget to `net10.0`, align available ASP.NET Core/Microsoft.Extensions/EF Core package versions to 10.0.x, and add narrowly scoped AutoMapper NU1903 suppressions only on the real Services and WebApp references.
- [x] All project files -- remove EFCore.BulkExtensions, System.Drawing.Common, ZXing.Net, Hangfire.Core, and BCrypt.Net-Next wherever referenced, including BCrypt from the real WebApp; remove code references only where necessary to restore a warning-free build.
- [x] `tests/ASI.Basecode.Services.Tests/*.csproj`, `tests/ASI.Basecode.IntegrationTests/*.csproj`, and `ASI.Basecode.sln` -- create both xUnit v3 projects, add appropriate project references, one smoke test each, and include them in the solution.
- [ ] `.github/workflows/ci.yml` -- run build and both test projects on pull requests to `dev`, using an `mssql/server:2025-latest` service with a health check.
- [ ] Project and workflow files -- run the specified local checks, record their result, and keep CI failures visible rather than suppressing them.

**Acceptance Criteria:**
- Given the retained solution projects, when their project files are inspected, then each targets `net10.0`, contains no retired package, and only Services/WebApp suppress AutoMapper's NU1903 warning.
- Given a .NET 10 developer environment, when the solution is built and both test projects run, then all commands pass with zero build warnings.
- Given a pull request to `dev`, when GitHub Actions starts, then it waits for SQL Server 2025 health and runs both test projects.

## Implementation Notes

The current checkout has four main projects targeting `net9.0`, no `tests/` directory, and no `.github` workflow. The backup project copies are outside the solution but violate the ticket's "every csproj" and retired-package checks if retained unchanged.

2026-11-05: Implemented the .NET 10 retarget and smoke-test projects. A temporary SDK 10.0.403 was installed under the user's temporary directory for verification because the installed SDK was 9.0.200. Restore passed after resolving a Newtonsoft.Json downgrade and vulnerable transitive dependencies; the solution build passed with zero warnings/errors. The Services smoke test passed 1/1, and the SQL smoke test passed 1/1 against the disposable LocalDB database. The CI workflow requires the repository Actions secret `MSSQL_SA_PASSWORD`. Its GitHub-hosted PR run has not been verified; Docker and actionlint were not available locally.

2026-11-05 follow-up (starting revision 11f87ac312bcc62084d3db443e4ca66cbfb09089): Replace the repository-secret dependency with a masked per-job password and a readiness-checked SQL Server container started in a workflow step. Update AGENTS.md and remove outdated backup WebApp package conflicts. The solution builds with zero warnings; each smoke test passes locally (the integration test uses LocalDB). GitHub Actions and the SQL Server 2025 image remain unverified until an approved push and PR run. The retained backup Services project is outside the solution and has pre-existing standalone build errors and vulnerable dependencies; it is not required to build by this story.

## Plan Change Log

## Review Triage Log

## Design Notes

This story establishes the shared baseline only. Authentication, schema mapping, and feature tests belong to later stories that consume these test projects.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --no-restore` -- expected: zero warnings after restore has completed.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` -- expected: passes.
- `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` -- expected: passes against the configured SQL Server fixture.

**Manual checks:**
- Inspect the pull-request workflow run for a healthy SQL Server service and successful build/test jobs.
