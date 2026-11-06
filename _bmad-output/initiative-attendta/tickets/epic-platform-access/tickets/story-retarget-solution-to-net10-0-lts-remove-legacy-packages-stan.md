---
id: 10
type: story
title: "Retarget solution to net10.0 LTS, remove legacy packages, stand up xUnit v3 test projects and CI container baseline"
parent: epic-platform-access
after: []
hitl: true
risk: high
tracker_id: "9"
remote: "https://github.com/rainyDayssss/AttendTa/issues/9"
tracker_status: done
assignee: "LucielCienAmir,wigglebop25"
---

# Retarget solution to net10.0 LTS, remove legacy packages, stand up xUnit v3 test projects and CI container baseline

## Description

Moves every project in the solution to net10.0 and moves the ASP.NET Core, Microsoft.Extensions and EF Core packages to 10.0.x where a 10.0.x release exists (Microsoft.TypeScript.MSBuild and Microsoft.Playwright keep their own versions). Removes EFCore.BulkExtensions, System.Drawing.Common and ZXing.Net from Data, and removes Hangfire.Core and BCrypt.Net-Next wherever referenced (Services, and BCrypt also in WebApp), keeping AutoMapper 14.0.0 with NU1903 suppressed only on each AutoMapper 14.0.0 PackageReference (Services and WebApp). Stands up both xUnit v3 test projects (tests/ASI.Basecode.Services.Tests and tests/ASI.Basecode.IntegrationTests with xunit.v3) and creates the GitHub Actions CI workflow on ubuntu-latest with an mssql/server:2025-latest service container and healthcheck that runs both test projects on PR to dev.

## Requirements

Verify: Every csproj in the solution targets net10.0; no csproj references a removed package; the only NU1903 suppressions are on each AutoMapper 14.0.0 PackageReference (Services and WebApp); both test projects build and execute under dotnet test; the GitHub Actions workflow runs green against the SQL Server 2025 container; and the solution builds with zero warnings.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-platform-access/epic-platform-access.md
