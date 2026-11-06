---
title: 'Event publication with active master revalidation and activity audit'
type: 'feature'
ticket: '3'
created: '2026-11-06'
status: 'built'
baseline_revision: '04c6d9ec794478450da8b9b68a9edb1c297106a3'
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

**Problem:** Officers can create drafts and manage covers, but cannot publish events. Downstream participation modules also lack the shared canonical facts needed to apply consistent eligibility, attendance, cutoff, and reporting rules.

**Approach:** Establish all nine AD-5 facts in the Data layer and extend the Event lifecycle service with an audited, transactional publication command. Expose publication through the management event list. Published status and Signup open establish availability for the member feed and registration submission delivered by story 3.5 / issue #33.

## Boundaries & Constraints

**Always:** Require an authenticated Administrator or Officer for publication, and antiforgery on its POST endpoint. Discover reference IDs, lock event type → venue → event, then recheck reference identity and publication conditions against locked state. Refuse stale references without writing. Use the injected TimeProvider, UTC storage, real actor IDs, and one transaction for status, update metadata, and `event.published` audit. Preserve private cover storage and existing draft/cover behavior. Define reusable SQL-translatable expressions or queries for canonical facts; capture now outside expressions so SQL and in-memory evaluation agree.

**Never:** Implement member feed/signup screens, registration writes, promotion, completion, or other later lifecycle stories. Do not change schema, expose storage keys, perform file I/O while locked, or duplicate canonical participation rules in consuming services.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Publish | Draft; active type/venue; cover set; valid capacity; future deadline | Published, actor/time updated, one audit committed | Success notification |
| Invalid masters/cover/capacity | Inactive type or venue, missing cover, or limit beyond capacity | No status or audit change | Domain validation error |
| Deadline boundary | Deadline at or before now | Publication refused | Domain validation error |
| Invalid state | Published, Completed, or Cancelled | No second publication/audit | Domain validation error |
| Missing/stale event | Missing row or changed type/venue after discovery | No mutation | NotFound or Stale |
| Access | Anonymous, Member, invalid actor, or missing antiforgery | No publication | Existing authorization/antiforgery response |
| Persistence failure | Status or audit save fails | Both roll back | Existing failure handling |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/Services/EventLifecycleService.cs` — reuse unit-of-work transactions, actor checks, snapshots, and lock/recheck pattern from cover commands. Existing `LockActiveReferencesAsync` validates master activity, but publication must evaluate the canonical fact against the locked event and masters.
- `ASI.Basecode.Data/Repositories/EventRepository.cs` — `GetEventAsync` is untracked discovery; `LockEventAsync` reloads locked state and refreshes tracked values. Existing get/lock/update operations suffice; type and venue repositories already provide transaction-required row locks.
- `ASI.Basecode.Data/Models/` and `Models/Lookups/` — existing entities and typed status codes support facts without migration. New shared expressions live in `Data/Queries/ParticipationFacts.cs`.
- `ASI.Basecode.Services/Interfaces/IEventLifecycleService.cs` — add publication contract. `Resources/Constants/AuditActions.cs` and audit message resources provide action vocabulary; preserve existing action descriptions and serialization conventions.
- `ASI.Basecode.WebApp/Controllers/EventController.cs`, `Views/Event/Index.cshtml` — management policy and automatic antiforgery already apply; add draft-only publication form and POST using existing notifications/outcomes.
- `tests/ASI.Basecode.Services.Tests/EventLifecycleServiceTests.cs`, `EventCoverServiceTests.cs` — fake repositories, transaction recording, actor and clock conventions. `tests/ASI.Basecode.IntegrationTests/EventDraftFlowTests.cs`, `EventCoverFlowTests.cs` — disposable SQL databases and authenticated HTTP patterns.
- Prior cover plan confirms private-file operations stay outside locks and Published cover reads already work. Issue #33 explicitly owns the paged member feed; this story supplies Published state and Signup open.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Data/Queries/ParticipationFacts.cs` — define Eligible Participant (active user and Active membership), Valid Attendance (existing non-invalidated row), Seat held (Confirmed), event-specific Waitlist priority (review time then registration ID), Start Cutoff passed (now >= start), Signup open (Published and now < deadline), Publishable (AD-5), No-show (Confirmed, Completed, no valid attendance), and decimal Attendance rate (valid attendees / Confirmed; null for zero denominator; no display rounding).
- [x] `ASI.Basecode.Services/Interfaces/IEventLifecycleService.cs`, `Services/EventLifecycleService.cs` — add management-only publication with lock ordering, stale-reference detection, canonical validation, update metadata, and atomic audit.
- [x] `ASI.Basecode.Resources/Constants/AuditActions.cs`, audit message resources — register publication vocabulary where absent, using current project patterns.
- [x] `ASI.Basecode.WebApp/Controllers/EventController.cs`, `Views/Event/Index.cshtml` — expose publication with success/refusal feedback, preserving current pagination and draft-only editing.
- [x] `tests/ASI.Basecode.Services.Tests/ParticipationFactsTests.cs`, publication service tests — cover all nine definitions, fake-clock exact boundaries, ordering ties, inactive membership/user, invalidated attendance, null rate, and publication matrix guards/lock order.
- [x] `tests/ASI.Basecode.IntegrationTests/EventPublicationFlowTests.cs` — prove authenticated publication and refusal, status/audit atomic rollback, antiforgery/role restrictions, and canonical query SQL translation using actual SQL Server.

**Acceptance Criteria:**
- Given a valid covered draft and an Officer, when publication is posted, then persisted status is Published and exactly one `event.published` audit identifies the actor and event.
- Given any refusal scenario in the matrix, when publication is attempted, then status and audit remain unchanged and the caller receives appropriate feedback.
- Given controlled participant, registration, event, and attendance data, when canonical facts run in memory and SQL, then results match AD-5, including exact time boundaries, queue tie-breaks, and unrounded decimal rates.
- Given a status write followed by audit failure, when the transaction aborts, then a fresh database read still finds the original Draft and no publication audit.

## Implementation Notes

- Final coordinator verification after approved LocalDB restart: Services 144/144 and Integration 186/186 passed, none skipped. Canonical predicate composition and populated SQL fact assertions passed after correcting expression parameter binding. Earlier LocalDB timeouts were resolved by restarting the local instance with user permission.

- `NoShows` and `AttendanceRates` compose `SeatHolders` and `ValidAttendances`, so their SQL queries share the canonical Confirmed and non-invalidated predicates.
- Publication service tests cover missing events, changed references, anonymous/member/invalid actors, absent covers, inactive masters, capacity, all non-Draft states, exact and past deadlines, and type -> venue -> event lock order.
- SQL coverage forces an `activity_logs` insert failure and confirms a fresh read finds the original Draft with no audit row. It also executes and asserts results for all nine participation facts against SQL Server.
- Verification on 2026-11-06: `dotnet build` passed; Services tests passed 144/144; Integration tests passed 186/186 with `ATTENDTA_TEST_SQL` set to LocalDB master and Poppler added to the process `PATH`.

## Plan Change Log

## Review Triage Log

- Quick review completed by inspection against the full diff, acceptance criteria, architecture, and repository instructions: no findings; no deferrals.
- Matrix audit: happy path and lock order covered by `PublishLocksReferencesPublishesAndWritesOneAuditEntry`; masters/cover/capacity/state/deadline guards by `PublishRefusalLeavesStatusAndAuditUnchanged`; missing/stale by `PublishMissingEventIsNotFoundAndChangedReferencesAreStaleWithoutWrites`; actor restrictions by `PublishRejectsAnonymousMemberAndInvalidActors`; HTTP antiforgery/member restrictions and real SQL rollback by `OfficerPublishesCoveredDraftWithAntiforgeryAndAuditWhileRefusalsLeaveRowsUnchanged`. All ran successfully in the final 144-service/186-integration verification.

## Design Notes

Publication must compare discovered master IDs with the locked event before using the locked masters; otherwise a concurrent draft edit could validate obsolete references. Capture request-time UTC after locks for the deadline guard. Keep shared fact definitions composable for downstream consumers without bringing service-layer TimeProvider dependencies into Data; pass captured UTC values to expression/query factories.

## Verification

- `dotnet build` — succeeds.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — all pass, including canonical fact and publication coverage.
- With `ATTENDTA_TEST_SQL` set to the local SQL Server master connection, `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` — all pass. Reuse the prior cover plan's local Poppler PATH setup if required by existing certificate tests.
