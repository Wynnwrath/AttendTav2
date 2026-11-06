---
id: 7
type: story
title: "Member attendance view on My Events"
parent: epic-check-in
covers: [R1, R7, R9]
after: [1, 6]
hitl: false
risk: low
tracker_id: "50"
remote: "https://github.com/rainyDayssss/AttendTa/issues/50"
tracker_status: backlog
assignee: "rainyDayssss"
---

# Member attendance view on My Events

## Description

Adds an attendance query service and attendance view-model fields to e3's My Events card, rendered inside _PassPartial, showing each Registration's Attendance state with invalidated records marked. The attendance history pages at 25 rows with its own page parameter beside e3's registrations list (AD-13, AD-14). This entry adds no check-in or correction action (delivering R7, the attendance state part of R1, and the history paging part of R9).

## Requirements

Verify: A Member opens My Events and sees their valid attendance records with Asia/Manila check-in timestamps; an invalidated record is badged Invalidated with no check-in or correction actions; the attendance history pages at 25 rows with its own page control, independent of the registrations list.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md
