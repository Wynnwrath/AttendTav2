---
title: 'Playwright certificate PDF issuance tracer'
type: 'feature'
ticket: 1
created: '2026-11-05'
status: 'built'
baseline_revision: 'ff761a9d47a7c23f1bf0b5c7b5682cfaed80340d'
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

**Problem:** The certificate feature has no verified path from a confirmed registration through PDF rendering, private storage, and an authorized response.

**Approach:** Build a minimal development-only tracer that renders a clearly identified sample PDF, promotes it to private certificate storage, and streams it to management. This proves the integration without claiming official issuance.

## Boundaries & Constraints

**Always:** Pin Microsoft.Playwright 1.64.1 in WebApp; keep rendering behind a Services interface and implement the adapter in WebApp. Accept a registration ID, check its confirmed status server-side, use the Management policy, POST and antiforgery, and revalidate active sessions through existing authentication. Render a self-contained, non-official sample without caller-supplied HTML or external resources. Produce A4 landscape PDF bytes, save and promote through `IPrivateFileStore`, then stream the promoted file as `application/pdf`. Never expose keys or accept paths. Make the tracer unavailable outside Development. Delete owned temporary files on failure and promoted files after response disposal or failure; never delete while the stream is in use.

**Never:** Write certificate rows or audit entries, implement official template, eligibility, locking, certificate numbering or production download flows; these belong to later stories. Do not add a second certificates mapping. The epic's 2026-11-04 decision drops host installation as a deliverable; install Chromium for local and CI verification, with no deployment changes.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Tracer | Active Officer, valid antiforgery, confirmed registration, Development | PDF response from promoted `certificates/` key, `%PDF-` header and landscape A4 page | No error expected |
| Registration | Unknown or non-confirmed registration | No PDF or file created | Return a non-success response |
| Access | Anonymous, member, inactive account, missing antiforgery, or non-Development | No issuance or PDF | Existing auth/antiforgery rejection or not found |
| Failure | Renderer, storage, promotion, open or stream fails | Clean owned temporary files and close and delete any promoted file after use | Return a safe error where possible without exposing paths |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Models/Registration.cs`, `Models/Lookups/LookupCodes.cs`, `AsiBasecodeDbContext.cs` — mapped registrations and `RegistrationStatusCode.Confirmed`; follow repository/service layering for lookup.
- `ASI.Basecode.Services/Interfaces/IPrivateFileStore.cs`, `ASI.Basecode.WebApp/Storage/PrivateFileStore.cs` — temporary save, PDF-validating promotion, read and delete; certificates keys remain private.
- `ASI.Basecode.WebApp/Startup.DI.cs`, `Startup.Auth.cs`, `Controllers/VenueController.cs` — scoped DI, Management policy, cookie revalidation, controller and streamed-file patterns.
- `tests/ASI.Basecode.IntegrationTests/SqlServerDatabaseFixture.cs`, `AttendTaWebApplicationFactory.cs`, `VenuePhotoFlowTests.cs`, `PrivateFileStoreTests.cs` — isolated canonical database, authenticated requests, private setup root and storage assertions.
- `ASI.Basecode.Data/CanonicalSchemaMappings.cs` already maps certificates; do not change schema or mapping for this tracer.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Data/Interfaces/` and `ASI.Basecode.Data/Repositories/` — add an asynchronous existence/status lookup by registration ID; the sample requires no event data or schema changes.
- [x] `ASI.Basecode.Services/Interfaces/` and `ASI.Basecode.Services/Services/` — define renderer and tracer contracts; orchestrate lookup, render, temporary save, promotion, open and failure cleanup using the private store.
- [x] `ASI.Basecode.WebApp/ASI.Basecode.WebApp.csproj`, `ASI.Basecode.WebApp/` renderer and `Startup.DI.cs` — pin Playwright, implement isolated Chromium PDF rendering, dispose resources, and register dependencies.
- [x] `ASI.Basecode.WebApp/Controllers/` and a minimal Development-only view — provide an antiforgery-enabled form and Management POST, call the service, and stream without leaking paths; dispose the stream before deleting the promoted file.
- [x] `tests/ASI.Basecode.Services.Tests/` and `tests/ASI.Basecode.IntegrationTests/` — cover the matrix with deterministic failures and a real Chromium/Officer/SQL request. Assert the promoted key, bytes, A4 dimensions, cleanup, unchanged certificate count, each authorization case and Production denial.
- [x] `.github/workflows/ci.yml` and `docs/database-setup.md` — provision matching Chromium and dependencies for CI and document local test prerequisites and cleanup.

**Acceptance Criteria:**
- Given a confirmed registration and active Officer, when the protected tracer is invoked in Development, then the response is a landscape A4 PDF promoted to private certificate storage.
- Given the tracer is invoked, when it completes or fails, then it never writes an official certificate record or presents the result as an official certificate.
- Given an unauthorized request or invalid registration, when the tracer is invoked, then no PDF is issued or disclosed.

## Implementation Notes

- Implemented the Development-only tracer, private-file cleanup, authenticated form, and real Chromium/SQL coverage. The integration runner is serialized to avoid LocalDB fixture creation contention.
- Verified the .NET 10 build, 23 service tests, and 98 serial SQL integration tests, including the real PDF flow and access matrix. Chromium installation succeeded.

## Plan Change Log

## Review Triage Log

- Quick review: 0 high, 0 medium, 0 low, 1 false, 0 maybe-false.
- False — scope conflict at `CertificateTracerController`: the repository's older Master Data assignment scope is superseded for this work by the user's explicit request to implement story 6.1 and approval of the certificate tracer plan; the implementation intentionally does not implement official issuance or other members' modules.

## Design Notes

The confirmed-state lookup is a nontransactional tracer snapshot; official issuance and race handling belong to story 6.7. The sample has no database association and is deleted after streaming.

## Verification

**Commands:**
- `& 'C:\Program Files\dotnet\dotnet.exe' build` — full solution succeeds with .NET 10.
- `& 'C:\Program Files\dotnet\dotnet.exe' test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — service tests pass.
- Set `ATTENDTA_TEST_SQL` to the local LocalDB master connection; run `& 'C:\Program Files\dotnet\dotnet.exe' tests/ASI.Basecode.IntegrationTests/bin/Debug/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` — integration suite passes, including the real renderer and authenticated tracer, without concurrent database-creation contention.
- After the build, run `pwsh ASI.Basecode.WebApp/bin/Debug/net10.0/playwright.ps1 install chromium`; CI uses its Release script with `install --with-deps chromium`. Verify PDF dimensions in the test.
