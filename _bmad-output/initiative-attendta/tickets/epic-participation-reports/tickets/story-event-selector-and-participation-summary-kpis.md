---
id: 1
type: story
title: "Event selector and participation summary KPIs"
parent: epic-participation-reports
covers: [R1]
after: [1.1, 1.9, 3.7, 4.6]
hitl: false
risk: medium
tracker_id: "71"
remote: "https://github.com/rainyDayssss/AttendTa/issues/71"
tracker_status: backlog
---

# Event selector and participation summary KPIs

## Description

Delivers the report pipeline from coherent read queries to a Tailwind view authorized by the Management policy (AD-8), displaying an event selector and per-event summary KPIs including registration status counts, Confirmed count, Valid attendee count (Confirmed registrations with Valid Attendance, excluding invalidated attendance), and attendance rate shown as a percentage per FR-30 (Valid attendees ÷ Confirmed, calculated from the ParticipationFacts decimal, N/A when Confirmed is 0, rounded to two decimal places for display only); provides the base report view for no-shows, distributions, and detail rows; needs canonical schema, role claims, and Tailwind shell from epic-platform-access, event and registration entities from epic-events-registration, and attendance records with invalidation from epic-check-in.

## Requirements

Verify: Sign in as an officer, open /Report, select an event, and verify summary cards display status counts, Confirmed count, Valid attendees (excluding invalidated attendance), and attendance rate formatted as a percentage (e.g. 92.00%, or N/A when zero Confirmed) matching database records.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-participation-reports/epic-participation-reports.md
