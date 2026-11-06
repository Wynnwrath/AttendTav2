---
id: 4
type: story
title: "Account deactivation with registration cancellation cascade and session refusal"
parent: epic-administration
covers: [R3, R4]
after: [2, 3, 1.1, 3.15]
hitl: false
risk: medium
tracker_id: "55"
remote: "https://github.com/rainyDayssss/AttendTa/issues/55"
tracker_status: backlog
---

# Account deactivation with registration cancellation cascade and session refusal

## Description

Applies Account deactivation with a mandatory reason in one ExecuteInTransactionAsync under the Administrator policy. When the target is an active Administrator it takes the app lock attendta:last-admin (AD-4 step 1; refusing deactivation if the target is the last active Administrator), then locks the target's users row, then calls IRegistrationCancellationService.CancelUpcomingForUserAsync(userId, AccountDeactivated, reason) from epic-events-registration (AD-6), which cancels the upcoming Registrations, clears their QR tokens and locks their events in ascending order. It then sets users.is_active to false and appends one user.deactivated Activity Log entry. Session refusal on the next request comes from epic-platform-access's OnValidatePrincipal (AD-8); this entry adds no session code and only verifies it. The confirm step opens the entry 3 preview dialog.

## Requirements

Verify: Executing the UJ-5 flow, deactivating an officer account with a valid reason cancels their upcoming registrations, releases their seats without auto-promoting waitlisted members, records an attributable activity log entry, and immediately refuses their active session on their next request. An integration test in ASI.Basecode.IntegrationTests with parallel connections runs two account deactivations of the two active Administrators at once; exactly one succeeds and one Administrator stays active.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-administration/epic-administration.md
