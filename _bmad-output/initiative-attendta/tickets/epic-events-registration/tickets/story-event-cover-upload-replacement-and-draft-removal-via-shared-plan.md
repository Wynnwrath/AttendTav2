---
title: 'Event Cover upload, replacement, and draft removal via shared storage'
type: 'feature'
ticket: '2'
created: '2026-11-06'
status: 'built'
baseline_revision: '210d964c4fddb077b93b8c0eeee1cc884b0630d4'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context:
  - 'AGENTS.md'
  - '_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Event drafts exist, but officers cannot attach, replace, or remove their covers. Members need an authorized way to load published covers without exposing private storage.

**Approach:** Extend the Event lifecycle service with cover commands using the shared private store. Add a dedicated management cover screen linked from the event list, and an authorized streaming endpoint addressed by event ID.

## Boundaries & Constraints

**Always:** Accept JPEG/PNG by magic bytes, at most 2 MiB, through `PrivateFileArea.EventImages`. Persist only generated relative keys. Allow upload/replacement on Draft and Published; removal only on Draft; Completed/Cancelled remain read-only. Require Management and antiforgery for mutations. Recheck state under SQL locks in the established type → venue → event order. Save a new file before the transaction, commit the row and `event.updated` audit together, then delete the old file. On rollback clean up the new file. Use actor IDs and the injected clock. Audit a cover-change indicator/presence, never a path. Management may read existing covers; Members may read Published covers, never Draft covers. Return private, non-cacheable image responses.

**Never:** Add static file exposure, client-provided storage keys, schema changes, publication, member feed, or unrelated lifecycle edits. Do not put `IFormFile` in Services or perform file I/O while database locks are held.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Upload/replace | Draft or Published; valid JPEG/PNG | New cover stored and audited; old file removed after commit | No error |
| Remove | Draft with cover | Null image key and audit committed before deletion | No error |
| Invalid upload | Empty, oversized, wrong signature/extension | Existing cover and row unchanged | Form validation error |
| Invalid state | Published removal; Completed/Cancelled mutation | No mutation; staged file cleaned up | Domain validation error |
| Transaction failure | New file saved, persistence/audit fails | Old cover preserved; new file cleaned up | Existing failure handling |
| Cleanup failure | Old-file delete fails after commit | Successful mutation remains successful | Best-effort cleanup; no path disclosure |
| Authorized read | Management, or Member with Published event | Stream stored image via event ID | Missing event/file/cover returns not found |
| Unauthorized read | Anonymous, unsupported role, Member with Draft | No cover bytes disclosed | Authentication/authorization or not found |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/Services/EventLifecycleService.cs`, `Interfaces/IEventLifecycleService.cs` — existing transaction, actor, event lock, and audit patterns; add stream-based cover operations.
- `ASI.Basecode.Data/Repositories/EventRepository.cs` — existing get/lock/update suffice; `Models/Event.cs` already has nullable `ImagePath`.
- `ASI.Basecode.Services/Interfaces/IPrivateFileStore.cs`, `ASI.Basecode.WebApp/Storage/PrivateFileStore.cs` — reuse event area, validation, generated keys, reads/deletes; already registered.
- `ASI.Basecode.Services/Services/VenueService.cs`, `ASI.Basecode.WebApp/Controllers/VenueController.cs` — photo lifecycle and controller stream precedent.
- `ASI.Basecode.WebApp/Controllers/EventController.cs` — Management-only; keep draft-edit restrictions. Use a separate cover-read controller so Members do not inherit Management policy.
- `ASI.Basecode.WebApp/Views/Event/Index.cshtml` — add cover-management navigation; use existing Tailwind view conventions.
- `tests/ASI.Basecode.IntegrationTests/VenueTransactionPhotoTests.cs`, `VenuePhotoFlowTests.cs`, `PrivateFileStoreTests.cs`, `EventDraftFlowTests.cs` — transaction, storage, cookie, and SQL fixture patterns.
- Prior draft plan leaves covers to 3.2 and preserves existing list ordering and draft-only metadata editing.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Services/Interfaces/IEventLifecycleService.cs`, `Services/EventLifecycleService.cs` — implement upload/replacement, draft removal, authorized cover retrieval, and path-free audit values; preserve existing draft commands.
- [x] `ASI.Basecode.Services/ServiceModels/EventCoverViewModel.cs` — add dedicated cover screen state without exposing a storage key; use an appropriate stream result for service reads.
- [x] `ASI.Basecode.WebApp/Controllers/EventController.cs` — add cover screen and upload/remove POST actions with validation and notifications.
- [x] `ASI.Basecode.WebApp/Controllers/EventCoverController.cs` — authorize Management/Member cover reads and stream with correct media type and private/no-store headers.
- [x] `ASI.Basecode.WebApp/Views/Event/Cover.cshtml`, `Views/Event/Index.cshtml` — add preview, multipart upload/replacement, draft removal, and navigation.
- [x] `tests/ASI.Basecode.Services.Tests/EventCoverServiceTests.cs` — exercise matrix guards, auditing, transaction ordering, rollback and cleanup failures with fakes.
- [x] `tests/ASI.Basecode.IntegrationTests/EventCoverFlowTests.cs` — prove SQL/file lifecycle, real store validation, authenticated reads, antiforgery, and inaccessible static paths.

**Acceptance Criteria:**
- Given an officer with a draft, when they upload JPEG, remove it, then upload PNG, then each successful command updates SQL and the private file store consistently.
- Given a Published event, when an officer replaces its cover or attempts removal, then replacement succeeds and removal is refused.
- Given invalid uploads, when submitted through the form, then validation appears and the previous cover remains readable.
- Given a Member, when requesting a Published cover, then the authorized action streams it; when requesting a Draft cover or static storage URL, then no image is disclosed.
- Given a successful cover change, when inspecting its audit entry, then no file path appears and the change is identifiable.

## Implementation Notes

- Implemented cover operations in the existing lifecycle service using the shared store, type → venue → event locks, path-free cover audit records, and best-effort cleanup outside transactions. Inactive references do not prevent cover-only edits.
- Added a dedicated management screen and private/no-store streaming action. Members can retrieve Published covers but cannot access management screens or mutations.
- `dotnet build`: passed with 0 warnings and 0 errors. Service suite: 128 passed, 0 failed, 0 skipped. Full integration suite: 184 passed, 0 failed, 0 skipped using disposable LocalDB databases.
- Certificate inspection initially failed because Poppler tools were not on PATH. Final integration verification used the existing local `C:/Users/vv/AppData/Local/Temp/opencode/poppler-26.09/poppler-26.09.0/Library/bin` on the process PATH; no application change was required.
- Matrix audit: upload/removal ordering and rollback/cleanup failures are covered by `EventCoverServiceTests`; real JPEG upload → removal → PNG reupload, Published replacement/removal guards, invalid uploads, audit persistence, Management/Member/anonymous access and static-path refusal are covered by `EventCoverFlowTests`; Completed/Cancelled mutation guards, unsupported roles and missing files are covered by service tests. All covering tests ran and passed.
- Quick review completed with no actionable findings and no deferrals. The user subsequently requested a commit, push, and pull request to dev.

## Plan Change Log

## Review Triage Log

Verdicts: high 0, medium 0, low 0, false 1, maybe-false 0.

| Finding | Verdict | Route | Evidence |
|---|---|---|---|
| Event work extends beyond the older Master Data assignment described in AGENTS.md | false | rejected | The user explicitly selected Event story 3.2 / GitHub issue 30, requested its feature branch, and approved this Event-cover plan. The assignment reminder is historical scope context; this requested work is the current authorized task. Review found no implementation bugs or unmet acceptance criteria. |

## Design Notes

The dedicated cover screen supports Published covers without widening the draft metadata editor. Read the previous key from the locked row to avoid deleting a concurrently replaced file. Cover-only changes need lock ordering but must not reject inactive references unnecessarily. Post-commit deletion failure cannot turn a committed change into a reported rollback.

## Verification

- `dotnet build` — successful build.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — all pass, including cover matrix tests.
- With `ATTENDTA_TEST_SQL` set to the local SQL Server master connection, run `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` — all pass; investigate any failures, including the preceding plan's reported missing Playwright browser.
