---
title: 'Shared private file store and authorized venue photos'
type: 'feature'
ticket: '2'
created: '2026-11-05'
status: 'built'
baseline_revision: '7331f7948a7a2785c92dbccf65a76992ba835bf1'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context:
  - '_bmad-output/initiative-attendta/tickets/epic-master-data/epic-master-data.md'
  - '_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md'
  - 'AGENTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Venue image storage is limited to venues, permits 5 MiB, has no authorized read route, and records paths without the shared area prefix. Event covers and certificates need the same private storage contract.

**Approach:** Generalize the venue store into a shared private store, add a Management-authorized venue photo route, and supply an idempotent migration of existing venue path values.

## Boundaries & Constraints

**Always:** Use `Common:SetupRoot` outside `wwwroot` and the deployment tree; validate paths and areas on every operation; generate unique filenames with `CreateNew`; store area-prefixed relative paths; validate JPEG/PNG signatures and a maximum of 2 MiB for venue/event images and `%PDF-` for certificate writes/promotions. Preserve the old photo until the database update succeeds; delete a new file after a failed update.

**Never:** Serve private files through static middleware, expose physical paths, overwrite existing files, run destructive reset or migration against shared/production or `AsiBasecodeDB`, or implement event-cover or certificate business workflows.

**Dependency decision:** Story 1.3 is present. At the user's direction, implement story 2.2 while story 1.11 is in progress; preserve the existing venue write contract and leave its transaction conversion and venue audit integration for coordination after 1.11 lands.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Image upload | Valid JPEG/PNG at 2 MiB | Store under selected image area | Return relative path |
| Invalid image | Over 2 MiB, wrong signature, or extension mismatch | Reject without leaving a file | Clear validation error |
| Certificate | Temporary valid PDF promoted to certificates | Store once under `certificates/` | Reject non-PDF and unsafe paths |
| Venue photo | Management requests a venue's existing photo | Stream its image privately | Reject anonymous/unauthorized, missing or unsafe paths |
| Migration | Stored `venues/<name>` key | Change to `uploads/venues/<name>`; file remains in place | Repeating script makes no further change |
| Long legacy path | Adding `uploads/` would exceed 500 characters | Leave all affected rows unchanged | Report affected venue IDs before any update |
| Replacement failure | New file written but DB update fails | Keep old file and path | Clean up new file |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Storage/VenueImageStorage.cs` and `IVenueImageStorage.cs` -- current 5 MiB store rooted at `<SetupRoot>/uploads`; physical venue files already reside in `uploads/venues`.
- `ASI.Basecode.WebApp/Controllers/VenueController.cs` -- class has `Policies.Management`; Create/Edit already save before DB update and clean up replacements. It has no photo read action.
- `ASI.Basecode.WebApp/Startup.DI.cs` and `Views/Venue/Index.cshtml`, `_Form.cshtml`, `Edit.cshtml` -- register the store and show authorized photo links and the corrected limit.
- `ASI.Basecode.Services/Interfaces/` -- place the HTTP-independent `IPrivateFileStore` contract here; implement it in `ASI.Basecode.WebApp/Storage/` using the methods and areas in architecture AD-12.
- `ASI.Basecode.Data/Interfaces/IUnitOfWork.cs`, `ASI.Basecode.Data/UnitOfWork.cs`, and `ASI.Basecode.Services/Interfaces/IActivityLogWriter.cs` -- story 1.3 supplies `ExecuteInTransactionAsync`, `SaveChangesAsync`, and `Append(AuditEntry)`; audit append requires an active transaction and authenticated actor.
- `ASI.Basecode.Data/Repositories/VenueRepository.cs`, `ASI.Basecode.Services/Services/VenueService.cs`, and `ASI.Basecode.Resources/Constants/AuditActions.cs` -- venue repository writes still call `SaveChanges`; the current audit actions contain only account-request approval. Story 1.11 owns the master-data transaction conversion; coordinate venue audit actions with the team.
- `database/attendta-mvp-schema.sql` and `ASI.Basecode.Data/CanonicalSchemaMappings.cs` -- existing `venues.image_path` is required and limited to 500 characters; preserve the schema and physical files.
- `tests/ASI.Basecode.IntegrationTests/` -- existing MVC host and isolated SQL fixture support route, migration and storage tests.

## Tasks & Acceptance

**Execution:**
- [x] Prerequisite story 1.3 / issue #13 -- verified the merged `ExecuteInTransactionAsync` and `IActivityLogWriter` interfaces on the updated branch.
- [x] Master-data transaction integration -- preserve the existing venue service/repository write contract; record any remaining story 1.11 / issue #14 transaction and venue audit integration as a handoff, without duplicating that conversion.
- [x] `ASI.Basecode.Services/Interfaces/IPrivateFileStore.cs` -- define the AD-12 stream-based save, temporary path, promotion, open and delete contract with explicit areas and cancellation.
- [x] `ASI.Basecode.WebApp/Storage/PrivateFileStore.cs` and `Startup.DI.cs` -- implement and register the contract, enforce containment and area/content/size rules, exclusive creation and safe cleanup; replace the old venue-only adapter.
- [x] `ASI.Basecode.WebApp/Controllers/VenueController.cs` and `Views/Venue/` -- adapt upload and replacement to the shared store; add a Management-policy photo action keyed by venue ID, show photos through it, and avoid logging image paths.
- [x] `database/migrate-venue-image-paths.sql` -- preflight the 500-character limit, then transactionally and idempotently prefix only legacy `venues/` keys with `uploads/`; document deployment order so no running version interprets keys using the wrong root. Do not move files or execute against fallback/shared databases.
- [x] `AGENTS.md` and `docs/database-setup.md` -- document 2 MiB, the `uploads/venues/` stored path and safe migration sequencing.
- [x] `tests/ASI.Basecode.IntegrationTests/PrivateFileStoreTests.cs`, `VenuePhotoFlowTests.cs`, and `VenueImagePathMigrationTests.cs` -- cover storage boundaries and traversal, authorized/unauthorized MVC reads, replacement rollback and isolated-database migration (including long keys and a repeated run).

**Acceptance Criteria:**
- Given a venue with an old stored key and its existing file, when the migration runs twice, then the photo still renders through the authorized route and the second run makes no changes.
- Given a valid 2 MiB JPEG, when an authorized officer uploads it, then it is saved and rendered; a larger or mismatched file is refused without an orphan.
- Given a temporary PDF, when it is promoted, then a private certificate is created without overwriting; non-PDF content and traversal paths are refused.
- Given an anonymous user or non-Management account, when they request a venue photo, then no file content is disclosed.

## Implementation Notes

Initial planning inspected commit `e50e833541f99f9daf3605c156177607d0743c90`. Story 1.3 was verified at commit `7331f7948a7a2785c92dbccf65a76992ba835bf1`. Capture the current baseline before implementation so prerequisite changes are not included in this story's diff.

Implemented private storage, venue photo reads, venue upload adaptation, the path migration and documentation. The solution build passed with zero warnings and errors, and all 53 isolated SQL integration tests passed. The migration has only been run against temporary test databases. The story 1.11 transaction and venue audit integration remains a handoff.

## Plan Change Log

## Review Triage Log

Quick review: 0 findings (high 0, medium 0, low 0, false 0, maybe-false 0). The reviewer inspected the plan, context, repository rules and full diff.

## Design Notes

The old key `venues/<name>` resolves under `<SetupRoot>/uploads`; the new key `uploads/venues/<name>` resolves under `<SetupRoot>`. The migration updates the database key only, so existing files remain in place.

Venue image changes require audit entries under the master-data epic. Agree the venue action/template and non-sensitive fields with the story 1.11 owner; do not include image paths in audit values or logs.

## Verification

- `dotnet build ASI.Basecode.sln` -- solution builds.
- `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` -- isolated SQL and MVC/storage tests pass; use the existing fixture and its documented local SQL configuration.
- Manually confirm an officer can display a migrated venue photo and anonymous access is refused; never run the migration against `AsiBasecodeDB`.
