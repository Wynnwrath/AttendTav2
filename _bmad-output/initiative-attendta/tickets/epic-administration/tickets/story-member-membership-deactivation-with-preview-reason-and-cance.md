---
id: 6
type: story
title: "Member membership deactivation with preview, reason and cancellation cascade"
parent: epic-administration
covers: [R1, R3, R4]
after: [3, 2.6, 3.15]
hitl: false
risk: low
tracker_id: "57"
remote: "https://github.com/rainyDayssss/AttendTa/issues/57"
tracker_status: backlog
---

# Member membership deactivation with preview, reason and cancellation cascade

## Description

Adds the membership deactivation action to the Management Member screen, whose status control epic-master-data removed (entry 2.6). The modal shows the preview from entry 3 and requires a non-empty reason. Submitting runs one ExecuteInTransactionAsync under the Management policy: it locks the member's users row (which also protects the members row, AD-4), sets members.membership_status_id to inactive, calls IRegistrationCancellationService.CancelUpcomingForUserAsync(userId, MembershipDeactivated, reason) from epic-events-registration (AD-6), and appends one member.membership_deactivated Activity Log entry. It takes no app lock and leaves users.is_active unchanged.

## Requirements

Verify: Management opens a Member; the deactivation modal lists that member's upcoming Registrations, and submission with an empty reason is refused. With a reason entered, the membership status reads inactive, those Registrations show Cancelled with QR tokens cleared, the user can still log in, and the Activity Log holds one member.membership_deactivated entry.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-administration/epic-administration.md
