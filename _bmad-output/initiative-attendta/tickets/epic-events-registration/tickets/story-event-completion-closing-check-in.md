---
id: 12
type: story
title: "Event completion closing check-in"
parent: epic-events-registration
covers: [R5, R14]
after: [11]
hitl: false
risk: low
tracker_id: "41"
remote: "https://github.com/rainyDayssss/AttendTa/issues/41"
tracker_status: backlog
---

# Event completion closing check-in

## Description

Completes a Published Event at or after its start (injected `TimeProvider`, refused before start) under the event lock through `IEventLifecycleService`, setting status to Completed, clearing `is_check_in_open` (with `check_in_closed_by/at` as in entry 11), recording `completed_by` and `completed_at`, and writing an `event.completed` entry; this entry declares `IEventCompletionHandler` in Services and invokes every registered implementation (`IEnumerable`) inside the same transaction, with an empty list valid until epic-certificates registers one.

## Requirements

Verify: An officer completes an Event at or after its start time, observing status Completed, check-in closed and an Activity Log entry; an attempt before start is refused; and a test-double `IEventCompletionHandler` registered in IntegrationTests runs once inside the completion transaction, and its exception rolls completion back.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-events-registration/epic-events-registration.md
