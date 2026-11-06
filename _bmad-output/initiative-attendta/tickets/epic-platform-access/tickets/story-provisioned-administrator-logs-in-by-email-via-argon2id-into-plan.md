---
title: 'Provisioned administrator logs in by email via Argon2id into authenticated session with active role checks'
type: 'feature'
ticket: '1'
created: '2026-11-05'
status: 'built'
baseline_revision: '9cda73c3dd03110c4a14e0b1800b06f320310514'
route: 'full'
route_source: 'auto'
risk: 'high'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context:
  - '_bmad-output/initiative-attendta/tickets/epic-platform-access/epic-platform-access.md'
  - '_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md'
  - 'AGENTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Sign-in is a stub that issues user ID 0; passwords are reversible AES; the JWT-in-cookie stack and session state are legacy; the public Register page creates accounts; no role claim or active-account recheck exists; the setup script creates an admin with no audit row.

**Approach:** Replace the legacy stack with ASP.NET Core cookie authentication, an Argon2id PHC hasher, email login, named Management/Administrator policies, `ICurrentActor`, POST-only logout with antiforgery, a WebApp CLI hash mode, and a setup script that takes name/email/hash and writes one system audit row.

## Boundaries & Constraints

**Always:** Claims `NameIdentifier` = `users.user_id` and `Role` = role code; cookie non-persistent, 8 hours, non-sliding; Data Protection keys under `Common:SetupRoot/keys`; `OnValidatePrincipal` reloads the user and rejects inactive or role-changed sessions; one generic login failure; no lockout; antiforgery on login and logout; POST logout; no secrets, hashes or passwords in logs; build on the canonical schema and mappings from story 1.2; keep master-data behavior unchanged.

**Never:** Add the Account Request flow, Tailwind shell, role-management screens, or the master-data unit-of-work migration; change the schema; commit the connection string, token key or any secret; keep JWT, `/api/token`, session state, AES `PasswordManager`, `SessionManager`, public Register, or `UserService.AuthenticateUser`/`AddUser`.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|----------------------------|----------------|
| Active admin, correct password | email + password | 302 Home; auth cookie issued | None |
| Unknown email, wrong password, or inactive | credentials | 200 login; one generic message; no cookie | No account enumeration |
| Deactivated or demoted after sign-in | existing cookie | Next request rejected; redirect to login | Principal rejected |
| Anonymous | GET /Member | 302 /Account/Login | Return URL preserved |
| Officer or admin | GET /Member | 200 | None |
| Member | GET /Member | 302 /Account/AccessDenied | None |
| Logout | GET /Account/Logout | 405 | None |
| Logout | POST with antiforgery token | Signed out; 302 login | None |
| CLI hash | `--hash-password <secret>` | PHC `$argon2id$...` on stdout | No plaintext output |
| Setup script | name + email + hash | Active admin + one `actor_type_id = 2` audit row | Reject missing/invalid hash |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/CanonicalSchemaMappings.cs`, `Models/Lookups/`, `Models/User.cs` — canonical `users` mapping from 1.2; `User.Id` = `user_id`, `User.UserId` = email, `User.Password` = hash, `RoleId`, `IsActive`.
- `ASI.Basecode.Data/Repositories/UserRepository.cs`, `Interfaces/IUserRepository.cs` — add `GetByEmailAsync`/`GetByIdAsync`; remove `UserExists`/`AddUser`.
- `ASI.Basecode.WebApp/Authentication/` — delete `CustomJwtDataFormat`, `SignInManager`, `TokenProvider*`, `TokenValidationParametersFactory`; add `HttpCurrentActor`.
- `ASI.Basecode.WebApp/Startup.Auth.cs`, `Startup.cs`, `Startup.DI.cs` — cookie auth, Data Protection, policies, DI; remove JWT/session/token wiring.
- `ASI.Basecode.WebApp/Controllers/AccountController.cs`, `Models/LoginViewModel.cs`, `Views/Account/Login.cshtml`, `Views/Shared/_Header.cshtml` — email login, antiforgery, POST logout; delete Register.
- `ASI.Basecode.WebApp/Controllers/{Member,EventType,Venue}Controller.cs` — replace `[Authorize(Roles=...)]` with `[Authorize(Policy = Policies.Management)]`.
- `ASI.Basecode.Services/Interfaces/{IPasswordHasher,ICurrentActor,IAccountService}.cs`, `Services/AccountService.cs`, `Security/Argon2PasswordHasher.cs`, `ServiceModels/AuthenticatedUser.cs` — new auth services.
- `ASI.Basecode.WebApp/Program.cs` — `--hash-password` mode; `public partial class Program`.
- `database/seed-local-admin.sql`, `database/seed-event-types.sql` — variables, system audit row, active-admin lookup.
- `ASI.Basecode.WebApp/ASI.Basecode.WebApp.csproj`, `ASI.Basecode.Services/ASI.Basecode.Services.csproj`, `appsettings.json` — remove JwtBearer/token config/connection string; add Konscious 1.3.1.
- `tests/ASI.Basecode.Services.Tests/`, `tests/ASI.Basecode.IntegrationTests/` — hasher, account service, auth flow tests.
- `AGENTS.md`, `docs/database-setup.md`, `bruno/Login.bru` — setup and login docs.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Services/` — add `IPasswordHasher` and `Argon2PasswordHasher` (PHC, 16-byte salt, fixed-time verify, m=19456/t=2/p=1 start) and the Konscious package; add `ICurrentActor` and `AuthenticatedUser`.
- [x] `ASI.Basecode.Services/Services/AccountService.cs` + `UserRepository` — email lookup, active check, hash verify, role code; remove `UserService`, `IUserService`, `UserViewModel`, and the AutoMapper map.
- [x] `Startup.Auth.cs` + `Startup.cs` + `Startup.DI.cs` — cookie auth (8h, non-sliding), Data Protection under `SetupRoot/keys`, `OnValidatePrincipal`, fallback auth, `Management`/`Administrator` policies; remove JWT, session, token config.
- [x] `AccountController` + `LoginViewModel` + `Login.cshtml` + `_Header.cshtml` — email login, antiforgery, generic failure, POST logout, AccessDenied view; delete Register.
- [x] `Member/EventType/Venue` controllers — use the Management policy.
- [x] `Program.cs` — `--hash-password` CLI mode; add `public partial class Program`.
- [x] `seed-local-admin.sql` — `$(AdminName)`, `$(AdminEmail)`, `$(AdminPasswordHash)`, validate `$argon2id$`, insert active admin + one system audit row; `seed-event-types.sql` — select the active admin by role.
- [x] `appsettings.json` + WebApp csproj — remove `TokenAuthentication`, `ConnectionStrings`, JwtBearer; add a clear startup error when the connection string is missing.
- [x] Tests — hasher unit tests; AccountService integration tests; WebApplicationFactory auth/policy flow tests.
- [x] `AGENTS.md`, `docs/database-setup.md`, `bruno/Login.bru` — document the new setup and login.

**Acceptance Criteria:**
- Given a fresh `AttendTaMVP_Test` and the setup script, when the provisioned admin logs in by email, then a non-persistent 8-hour cookie is issued and `/Member` returns 200.
- Given an anonymous request to `/Member`, when it is made, then the response redirects to `/Account/Login`.
- Given a member account, when it requests `/Member`, then it is redirected to `/Account/AccessDenied`.
- Given an admin who is deactivated or demoted in the database, when the next request is made, then the session is rejected and the user is redirected to login.
- Given a wrong password, unknown email, or inactive account, when login is attempted, then the same generic message is shown and no cookie is issued.
- Given a GET to `/Account/Logout`, then 405; given a POST with an antiforgery token, then the user is signed out.
- Given `--hash-password`, then stdout is a PHC `$argon2id$` string that verifies; no plaintext is printed.
- Given the setup script, then exactly one active admin and one `actor_type_id = 2` `activity_logs` row exist; the hash is absent from the log.
- Given the solution, when `dotnet build` and both test projects run, then they pass.

## Implementation Notes

`feature/11` is now based on team `dev` at `9cda73c`, so the canonical `User`/lookup mappings and the 1.2 integration fixture are present. The setup script's audit row is written directly in SQL because the `IActivityLogWriter` belongs to story 1.3.

2026-11-05 implementation:
- Added `[Route("Member")]` and `[Route("Member/Index")]` to `MemberController.Index`: the approved matrix and tests use `/Member`, while the existing Bruno collection still uses `/Member/Index`.
- `Startup.cs` resolves the `IFileProvider` from `IWebHostEnvironment.WebRootPath` so the `WebApplicationFactory` host serves static assets from the WebApp content root.
- `Program.cs` uses `WebApplication.CreateBuilder(args)` and exposes `public partial class Program` so the integration host can start the real pipeline.
- Added `PasswordHasherCliTests` to cover the CLI matrix row and asserted the `/Home/Index` redirect in the admin sign-in test.
- Verification: `dotnet build ASI.Basecode.sln --no-restore` = 0 warnings/0 errors; Services tests 15/15; Integration tests 35/35 with `ATTENDTA_TEST_SQL` on LocalDB. The implementer also ran the manual fresh-database flow (schema, hash, admin seed, login, deactivate, logout); it was not re-run in this review session.

2026-11-05 review patches (quick lens):
- `database/seed-local-admin.sql`: widened the three admin variables to 4000 characters so the `LEN(...) > 255` guard sees the untruncated value; added an overlong-hash case to `AdminSeedTests`.
- `docs/database-setup.md` and `AGENTS.md`: build first and run the hasher with `--no-build` so `$hash` captures only the PHC line.
- `ASI.Basecode.Services/Security/Argon2PasswordHasher.cs`: reject `memorySize < 4 * parallelism` so a corrupted hash returns `false` instead of throwing; added the matching unit test.
- Re-verification: build 0 warnings/0 errors; Services tests 16/16; Integration tests 36/36.

## Plan Change Log

## Review Triage Log

| # | Location | Verdict | Evidence | Route |
|---|----------|---------|----------|-------|
| 1 | `database/seed-local-admin.sql` | low | Confirmed. `@AdminPasswordHash`, `@AdminName` and `@AdminEmail` are declared at `varchar(255)`/`nvarchar(255)`, so an overlong sqlcmd value is truncated at the declaration before the `LEN(...) > 255` guard runs. A truncated value that still starts with `$argon2id$v=19$m=` passes the prefix-only `LIKE` check and is inserted, leaving an unusable administrator. The intended CLI hash is ~97 characters, so this is an operator edge case, but the guard is dead as written. | patch |
| 2 | `docs/database-setup.md` | medium | Confirmed. The documented `$hash = dotnet run ...` captures `dotnet run`'s build output on stdout together with the PHC hash, so `AdminPasswordHash` is not the clean hash the seed script validates. A developer following the guide cannot provision the administrator. The plan's own manual check uses `--no-build`. | patch |
| 3 | `ASI.Basecode.Services/Security/Argon2PasswordHasher.cs` | medium | Confirmed. `TryParse` accepts `m=8,p=16` because it checks only `m >= 8` and `p <= 16`, not Konscious's `m >= 4 * p` rule. `Verify` then calls `Derive`, and Konscious throws `InvalidOperationException` instead of returning `false`, so a corrupted hash row turns login into a 500. The existing unreasonable-cost test covers only the upper bound. | patch |

## Design Notes

Use `CookieAuthenticationDefaults.AuthenticationScheme`; the cookie name is `AttendTa.Auth`. The role claim is the lowercase lookup code (`admin`, `officer`, `member`). `OnValidatePrincipal` compares the stored role with the current `users.role_id`; any mismatch or inactive row rejects the principal. The CLI prints the hash to stdout and the elapsed milliseconds to stderr so the setup script can capture only the hash.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --no-restore` — expected: zero warnings/errors.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — expected: hasher tests pass.
- `dotnet test --project tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` with `ATTENDTA_TEST_SQL` set — expected: schema, account, and auth-flow tests pass.

**Manual checks:**
- Reset `AttendTaMVP_Test`, apply `database/attendta-mvp-schema.sql`, run `dotnet run --project ... --no-build -- --hash-password "<password>"`, run `seed-local-admin.sql` with the name/email/hash, then `seed-event-types.sql`.
- Start the app with `ConnectionStrings__DefaultConnection` set; log in with the provisioned email; confirm `/Member` loads; deactivate the row and confirm the next request redirects to login; restore it; confirm GET logout is refused and POST logout works.
