---
id: 2
type: story
title: "Check-in window toggle with audit logging"
parent: epic-check-in
covers: [R3, R8]
after: [3.1, 3.12]
hitl: false
risk: low
tracker_id: "45"
remote: "https://github.com/rainyDayssss/AttendTa/issues/45"
tracker_status: backlog
assignee: "rainyDayssss"
---

# Check-in window toggle with audit logging

## Description

Adds SetCheckInWindowAsync to e3's IEventLifecycleService (with e3's review) and a Management command that opens or closes Check-in for a Published Event at any time as a row action on the Management event list from epic-events-registration entry 1 (3.1). The command runs in one IUnitOfWork.ExecuteInTransactionAsync under the Event lock (AD-4), sets is_check_in_open and its actor columns, and appends one audit entry per toggle (event.check_in_opened / event.check_in_closed, AD-7) (delivering R3 and the check-in toggle part of R8).

## Requirements

Verify: An Officer opens Check-in on a Published Event and sees it Open with actor and time shown, plus one event.check_in_opened Activity Log entry; closing it records one event.check_in_closed entry; a toggle on an unpublished, cancelled or completed event is refused and leaves the flag and the log unchanged.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md
