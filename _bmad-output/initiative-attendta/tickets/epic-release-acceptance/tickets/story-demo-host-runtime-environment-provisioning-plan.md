---
title: 'Demo host runtime environment provisioning'
type: 'feature'
ticket: '1'
created: '2026-11-06'
status: 'in-review'
baseline_revision: 'ba67ca030d3cd35c764a1f9421f07034052956b1'
route: 'full'
route_source: 'auto'
risk: 'high'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The demo environment has no repeatable, verifiable provisioning procedure or runtime readiness check.

**Approach:** Provide safe operator provisioning instructions and host validation for the canonical database, restricted runtime identity, private storage, installed Chromium, Argon2 benchmark, and running application.

## Boundaries & Constraints

**Always:** Use `AttendTa_Demo` and the canonical schema; protect secrets and private storage; validate using the actual runtime SQL and OS identities; preserve append-only audit permissions. Record real host verification separately from automated tests.

**Never:** Reset or overwrite an existing database; modify shared or production systems; commit credentials or hashes; disable the audit trigger; claim offline verification without a disconnected-host run. HTTPS phone trust and backup/restore belong to later stories.

**Decision:** Make setup portable: each operator supplies host-specific settings and secrets. This machine may be used for a later test with separate confirmation; actual host provisioning and offline acceptance remain unverified until an operator runs them.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Ready host | Demo runtime login, writable directories, staged browser, live app | Each readiness check succeeds; audit test insert is rolled back | Nonzero exit for any failed check |
| Wrong or overprivileged SQL login | Wrong database, owner/admin, or audit UPDATE/DELETE permitted | No readiness pass | Fail explicitly without altering data |
| Missing storage, browser, or login page | Directory absent/unwritable, Chromium cannot launch, HTTP status not 200 | Identifies failing check | Nonzero exit; no secrets printed |
| Offline evidence | Host is disconnected and browser launches | Operator records actual disconnected run | Online run alone is not offline evidence |

</frozen-after-approval>

## Code Map

- `database/attendta-mvp-schema.sql` — canonical schema allows demo database; audit trigger rejects mutations independently of SQL grants.
- `database/seed-local-admin.sql`, `database/seed-event-types.sql` — initial provisioning order; never commit seed values.
- `ASI.Basecode.WebApp/Program.cs` — `/Account/Login` and timed `--hash-password` mode; avoid exposing real credentials in process arguments.
- `ASI.Basecode.WebApp/Certificates/PlaywrightCertificatePdfRenderer.cs` — launches Playwright Chromium under the app identity; match package and browser install.
- `ASI.Basecode.Services/Security/Argon2PasswordHasher.cs` — current fixed m=19456 KiB, t=2, p=1; tests pin these values.
- `docs/database-setup.md` — disposable LocalDB instructions, not demo provisioning; do not reuse destructive reset.

## Tasks & Acceptance

**Execution:**
- [ ] `database/` — add guarded, non-destructive runtime-permission provisioning for the demo database, granting application table access without broad roles and SELECT/INSERT only on activity logs; verify effective permissions and reject elevated identities.
- [ ] `scripts/` and an appropriate .NET helper if needed — implement host readiness checks: actual runtime connection and effective audit permissions, transactional audit insert with rollback, writable required SetupRoot areas, matching Playwright browser launch, and exact localhost login HTTP 200; fail safely and do not print credentials.
- [ ] `docs/demo-host-setup.md` — document prerequisites, explicit host and identity selection, new database and seed steps, environment-only secrets, browser staging, app startup, Argon2 host benchmarking using a disposable secret, and disconnected verification with evidence and limitations.
- [ ] `tests/` — test readiness and permission failure cases with isolated resources and existing test patterns, including wrong database, elevated permissions and cleanup after failure.

**Acceptance Criteria:**
- Given a freshly provisioned demo host, when its operator follows the guide, then the .NET 10 app starts with its canonical demo database and separate runtime identity.
- Given the runtime identity, when readiness checks execute, then audit INSERT succeeds in a rolled-back transaction and effective UPDATE and DELETE permissions are denied independently of the trigger.
- Given an offline demo host and its staged browser, when the runtime identity runs validation, then Chromium launches, all required storage paths are writable, and the localhost login page returns HTTP 200.
- Given the demo host, when its operator benchmarks the decided Argon2 parameters, then the result and host details are recorded without storing a password or hash.

## Implementation Notes

- Added portable host guidance, SQL grants, readiness CLI and isolated readiness tests. Build, 52 service tests and 20 targeted readiness tests passed using the user-local .NET SDK and disposable LocalDB databases.
- Actual demo-host provisioning, real runtime-login success, offline Chromium launch and Argon2 benchmark remain unverified by user decision. The readiness matrix's real-host and disconnected-host rows cannot be fully proven by the simulated coordinator tests; do not mark this plan built until that evidence exists. Formal build review has not yet run.

## Plan Change Log

## Review Triage Log

- Quick review: 3 medium, 0 high, 0 low, 0 false, 0 maybe-false.
- medium / bad_plan — `DemoReadiness.CheckSqlConnectionAsync` checks fixed roles and database permissions but not direct or custom-role server privileges; an elevated login can pass the checks. The provisioning script impersonates only the database user and cannot verify its server permissions.
- medium / bad_plan — Both SQL checks inspect object-level UPDATE but do not inspect column-level UPDATE permissions, leaving a possible audit-permission gap despite the append-only trigger.
- medium / bad_plan — `ValidateStorage` tests the `logs` directory but `Startup.Logger.cs` writes into `logs/<yyyyMM>`, which may already exist with different permissions. A storage PASS does not establish that the current log directory is writable.

## Design Notes

The append-only trigger is not proof of least privilege: check effective SQL permissions separately, and use transaction rollback for the insert test. Browser validation must use the app's Playwright version and OS identity, not a separate system browser. Runtime access to other tables must not implicitly grant mutation access to the audit table.

## Verification

- Run `dotnet build` and targeted service/integration tests, including new readiness coverage, with disposable SQL Server when available.
- Run the documented validator on the actual demo machine disconnected from the internet and capture redacted evidence; do not claim this succeeded unless executed there.
