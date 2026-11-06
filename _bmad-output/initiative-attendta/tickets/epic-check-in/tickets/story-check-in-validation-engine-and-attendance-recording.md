---
id: 3
type: story
title: "Check-in validation engine and attendance recording"
parent: epic-check-in
covers: [R4, R8]
after: [2, 3.3, 3.7]
hitl: false
risk: high
tracker_id: "46"
remote: "https://github.com/rainyDayssss/AttendTa/issues/46"
tracker_status: backlog
assignee: "rainyDayssss"
---

# Check-in validation engine and attendance recording

## Description

Adds LockAttendanceAsync to the attendance repository (UPDLOCK, HOLDLOCK, ROWLOCK on primary key seek, AD-4). Implements the shared check-in use case behind the scanner endpoint and the manual lookup (entry 5): a Management-only POST that runs in IUnitOfWork.ExecuteInTransactionAsync, takes the owner's users row then the event through repository Lock…Async methods (AD-4), and rechecks event match, Check-in open, Confirmed status and Eligible Participant through ParticipationFacts. A Valid Attendance returns its original time and inserts nothing; an invalidated attendance is refused. Otherwise it inserts one Attendance Record (method qr_code, recorder from ICurrentActor, UTC time) and appends one attendance.recorded entry whose payload excludes qr_token (AD-7). Responds per AD-9 with an HTTP status and JSON {outcome, message}, plus participant name and student number on success (fields Deferred to e4) (delivering the validation and recording part of R4, the attendance record part of R8, and SM-4 attendance concurrency).

## Requirements

Verify: Submitting a valid token for an open Published Event records one Attendance Record (qr_code) and one attendance.recorded entry, and returns name and student number. Resubmitting returns HTTP 200 with the original check-in time and inserts no row. A token whose attendance is invalidated, or that is wrong-event, ineligible, cancelled, unconfirmed, closed, unpublished, or unknown, is refused without an insert. tests/ASI.Basecode.Services.Tests covers each refusal with a fake TimeProvider and ICurrentActor (AD-17). tests/ASI.Basecode.IntegrationTests, against ATTENDTA_TEST_SQL, runs parallel connections for concurrent scans of one Registration and asserts exactly one Attendance Record and one attendance.recorded entry, with competing requests returning the existing record.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md
