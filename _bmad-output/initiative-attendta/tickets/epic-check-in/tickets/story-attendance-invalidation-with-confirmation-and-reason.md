---
id: 6
type: story
title: "Attendance invalidation with confirmation and reason"
parent: epic-check-in
covers: [R6, R8]
after: [3, 5]
hitl: false
risk: medium
tracker_id: "49"
remote: "https://github.com/rainyDayssss/AttendTa/issues/49"
tracker_status: backlog
assignee: "rainyDayssss"
---

# Attendance invalidation with confirmation and reason

## Description

Implements Management invalidation of an Attendance Record: a POST under the Management policy with antiforgery, in one IUnitOfWork.ExecuteInTransactionAsync that takes the owner's users row then the event (AD-4). The mandatory reason is validated inside the transaction. The use case declares IAttendanceInvalidationHandler in Services and calls every registered implementation before the write, so a Forbidden throw rolls back. It then sets invalidated_at and invalidation metadata on the retained row and appends one attendance.invalidated entry with the reason, with epic-participation-reports (and epic-certificates via the hook) as consumers of invalidated attendance. Refusal of later check-in is entry 3's rule (delivering R6 and the invalidation audit part of R8).

## Requirements

Verify: An Officer invalidates an Attendance Record after entering a reason and confirming the warning; the row remains with invalidated_at set and one attendance.invalidated entry exists; a later check-in for that Registration is refused; with a test-registered IAttendanceInvalidationHandler that throws Forbidden, the row stays valid and no audit entry is written; with no handler registered (before e6 lands), invalidation commits.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md
