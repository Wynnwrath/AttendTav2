---
title: 'Canonical database schema and EF Core mapping for all tables'
type: 'feature'
ticket: '2'
created: '2026-11-05'
status: 'built'
baseline_revision: '93abdc8f7f9a190cffa7b49aac8fa665856a4700'
route: 'full'
route_source: 'auto'
risk: 'high'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context:
  - '_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md'
  - 'AGENTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The existing SQL script creates ten tables, but it still stores role and state values as strings and lacks the canonical lookup foreign keys, audit protection, and several required columns. EF Core only maps part of the schema, and MemberRepository uses a keyless projection instead of the mapped user entity.

**Approach:** Make the existing SQL script the canonical schema, map all ten tables and their lookups with fluent EF configuration, and test the schema and mappings against fresh, isolated SQL Server databases. Replace the member projection with a join to the mapped user entity.

## Boundaries & Constraints

**Always:** Keep DDL and EF mappings consistent; use fluent configuration only and no EF migrations. Allow schema execution only in `AttendTaMVP_Test`, `AttendTa_IT_*`, or `AttendTa_Demo`. Keep the destructive reset script limited to `AttendTaMVP_Test`. Preserve the existing authentication-facing `User` API; map its login identifier to the canonical email field without changing authentication behavior.

**Never:** Change unrelated stories, implement authentication, or introduce a data migration. Do not execute the schema against any database outside the allow-list.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Schema allowed | Fresh `AttendTaMVP_Test` or `AttendTa_IT_<guid>` | Schema creates all business and lookup tables, keys, and triggers | No error |
| Schema refused | Database name outside the allow-list | Script stops before creating schema objects | Deliberate guard error |
| Audit immutability | UPDATE or DELETE against `activity_logs` | Existing audit rows remain unchanged | Trigger rejects the operation |
| EF mapping | Fresh schema with empty tables | EF can query every business and lookup table | Mapping or SQL errors fail the test |

</frozen-after-approval>

## Code Map

- `database/attendta-mvp-schema.sql` — Existing ten tables, indexes, and business FKs are reusable; update string statuses, missing lookup tables/FKs, database guard, `READ_COMMITTED_SNAPSHOT`, audit trigger, and canonical columns.
- `database/seed-event-types.sql` and `database/seed-local-admin.sql` — Existing starter values and secure hash substitution; add descriptions and seed/use lookup IDs. Keep `database/reset-local-mvp-test-db.sql` test-only.
- `ASI.Basecode.Data/AsiBasecodeDbContext.cs` — Current five DbSets and partial mappings; expand to all ten business entities and eight lookup tables with explicit fluent column, key, conversion, and relationship mapping.
- `ASI.Basecode.Data/Models/{User,Member,EventType,Venue}.cs` — Existing models to align with canonical columns. Keep `User.UserId` source-compatible for current login code and map it to email.
- `ASI.Basecode.Data/Models/{AccountRequest,Event,Registration,Attendance,Certificate,ActivityLog}.cs` and `ASI.Basecode.Data/Models/Lookups/` — Add missing table/lookup entities and explicit lookup enum IDs.
- `ASI.Basecode.Data/Models/MemberAccountSummary.cs` and `ASI.Basecode.Data/Repositories/MemberRepository.cs:GetMemberRecords` — Remove the projection and join members to the mapped User entity; keep `MemberRecord` output unchanged.
- `tests/ASI.Basecode.IntegrationTests/SqlServerSmokeTests.cs` — Reuse its local-server safety check; replace its single connectivity-only coverage with an isolated per-class schema fixture and table, guard, and trigger tests.
- `tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` and `.github/workflows/ci.yml` — Existing .NET 10, EF Core 10, xUnit v3, and SQL Server 2025 setup; supply the fixture's `ATTENDTA_TEST_SQL` connection.

## Tasks & Acceptance

**Execution:**
- [ ] `database/attendta-mvp-schema.sql` — Create the canonical lookup tables/IDs and foreign keys, required columns, allow-list guard, RCSI setting, and append-only `activity_logs` trigger while retaining existing valid constraints and indexes.
- [ ] `database/seed-local-admin.sql` and `database/seed-event-types.sql` — Seed lookup rows and use their IDs; give all six starter event types non-empty descriptions.
- [ ] `ASI.Basecode.Data/Models/` and `ASI.Basecode.Data/Models/Lookups/` — Add the missing entities and explicit enums; align existing models with the schema without changing auth behavior.
- [ ] `ASI.Basecode.Data/AsiBasecodeDbContext.cs` — Map every business and lookup table, column, FK, relationship, and enum value using fluent configuration only.
- [ ] `ASI.Basecode.Data/Models/MemberAccountSummary.cs` and `ASI.Basecode.Data/Repositories/MemberRepository.cs` — Remove the keyless projection and query member account data through the mapped User entity.
- [ ] `tests/ASI.Basecode.IntegrationTests/` — Create a fresh `AttendTa_IT_<guid>` database per test class from `ATTENDTA_TEST_SQL`, run the canonical script, query all ten tables and lookups, reject an unapproved database, and assert UPDATE/DELETE fail on `activity_logs`; clean up only the fixture database.
- [ ] `.github/workflows/ci.yml` — Pass the SQL Server 2025 master connection through `ATTENDTA_TEST_SQL`, and invoke both suites with `dotnet test --project` so the new Microsoft.Testing.Platform runner discovers them; keep the integration suite in CI.

**Acceptance Criteria:**
- Given a fresh allowed test database, when the canonical script runs, then all ten business tables and lookup tables have the required columns, keys, and foreign keys, including `event_types.description NOT NULL` and `certificates.issuer_name_snapshot`.
- Given an unapproved database name, when the schema script runs, then it throws before creating objects; the existing reset script still targets only `AttendTaMVP_Test`.
- Given the generated integration database, when EF queries each business table and lookup, then every query succeeds and the expected enum IDs agree with seeded lookup IDs.
- Given an audit row, when direct SQL attempts UPDATE or DELETE, then the trigger rejects both operations.
- Given a member with a linked user, when `GetMemberRecords` runs, then it returns the same member fields, full name, and email without `MemberAccountSummary`.

## Implementation Notes
The existing member service writes string status codes, while the architecture assigns membership-status changes to a later story. Keep `Member.Status` as a source-compatible wrapper around the persisted `MembershipStatusId` enum and ignore the wrapper in fluent mapping; this lets existing service behavior continue without moving status ownership into this story. Keep `User.UserId` source-compatible for legacy sign-in code while mapping it to `users.email` and `User.Id` to `users.user_id`.

## Plan Change Log


## Review Triage Log

| # | Location | Verdict | Evidence | Route |
|---|----------|---------|----------|-------|
| 1 | `ASI.Basecode.Services/ServiceModels/EventTypeViewModel.cs`, `ASI.Basecode.WebApp/Views/EventType/_Form.cshtml`, `ASI.Basecode.Services/Services/EventTypeService.cs` | medium | Confirmed. `Description` has no `[Required]`, the form has no validation span, and both service methods assign `model.Description` directly. MVC converts an empty textarea to null, so the now `NOT NULL` column raises SQL 515; `EventTypeController` catches only `InvalidDataException`, so the request becomes a 500. | patch |
| 2 | `ASI.Basecode.Services/Services/UserService.cs` | low | Confirmed. `AddUser` sets `user.CreatedBy`/`UpdatedBy`, which `CanonicalSchemaMappings` ignores; `CreatedById`/`UpdatedById` are never assigned anywhere. The writes are dead and `users.created_by`/`updated_by` stay NULL for the legacy Register path. Named harm: a later reader believes the actor is recorded. | patch |
| 3 | `database/seed-local-admin.sql`, `database/seed-event-types.sql` | low | Scope claim refuted: the story description says the allow-list is "(applied to database/ scripts)", and the IT fixture seeds the admin into `AttendTa_IT_<guid>`, so the guards must accept it. The real defect is the two stale header comments that still name only `AttendTaMVP_Test`. | patch |
| 4 | `AGENTS.md` | medium | Confirmed. The Test Conventions section still prescribes `ConnectionStrings__DefaultConnection` and says the smoke test creates `AttendTaMVP_Test`; the suite now requires `ATTENDTA_TEST_SQL` and creates `AttendTa_IT_<guid>`. Following the documented command fails. | defer |
| 5 | `tests/ASI.Basecode.IntegrationTests/CanonicalSchemaTests.cs` | low | Confirmed. `CanonicalSchemaHasRequiredColumnsAndForeignKeys` counts only the eight named lookup FKs; the business FKs required by the acceptance criterion are never asserted, so a regression there would pass. | patch |


## Design Notes

The current `User` entity is also consumed by the legacy sign-in code, while the canonical table's integer `user_id` is distinct from its email. Preserve the existing string `UserId` API and map it to `users.email`; map the existing integer `Id` to `users.user_id`. Do not change sign-in behavior in this story. For test safety, the automated fixture creates and drops only GUID-suffixed `AttendTa_IT_` databases; use the existing reset script only when manually checking a fresh `AttendTaMVP_Test`.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --no-restore --configuration Release` — expected: solution builds successfully.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --configuration Release` — expected: all existing service tests pass.
- `dotnet test --project tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --configuration Release` with `ATTENDTA_TEST_SQL` set to a local SQL Server `master` connection — expected: fixture creates and drops its own test databases and all schema, mapping, guard, and trigger assertions pass. On this workstation use `Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True;Encrypt=False`.

**Manual checks:**
- On a disposable local SQL Server only, reset `AttendTaMVP_Test` with the existing reset script, apply the schema, and verify it succeeds; never target another database.
