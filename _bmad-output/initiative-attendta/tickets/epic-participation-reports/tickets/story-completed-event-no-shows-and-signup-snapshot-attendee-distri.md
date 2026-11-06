---
id: 2
type: story
title: "Completed-event no-shows and signup snapshot attendee distributions"
parent: epic-participation-reports
covers: [R1]
after: [1, 3.11, 3.12]
hitl: false
risk: medium
tracker_id: "72"
remote: "https://github.com/rainyDayssss/AttendTa/issues/72"
tracker_status: backlog
---

# Completed-event no-shows and signup snapshot attendee distributions

## Description

Extends the entry 1 report view to calculate Completed-only no-shows from ParticipationFacts (excluding pending, waitlisted, and cancelled registrations from expected attendance and no-shows; for a cancelled Event, no-shows are not shown because the No-show fact requires Completed and the rate follows AD-5 as N/A, while status counts remain visible), and derive actual-attendee course and year distributions strictly from signup snapshots while preserving inactive member history.

## Requirements

Verify: View a completed event and verify the no-show count equals the Confirmed registrations with no Valid Attendance (no-show fact from ParticipationFacts), with pending, waitlisted and cancelled rows excluded; view a cancelled event to confirm no-shows are omitted and rate follows AD-5; and verify course and year distributions reflect actual attendees using their signup snapshots, verifying inactive accounts/members from seeded inactive users/members rows in the test database without waiting on epic-administration.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-participation-reports/epic-participation-reports.md
