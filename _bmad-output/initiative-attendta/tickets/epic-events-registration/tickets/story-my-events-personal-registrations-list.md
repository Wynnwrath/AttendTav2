---
id: 13
type: story
title: "My Events personal registrations list"
parent: epic-events-registration
covers: [R12, R15]
after: [9, 10]
hitl: false
risk: low
tracker_id: "42"
remote: "https://github.com/rainyDayssss/AttendTa/issues/42"
tracker_status: backlog
---

# My Events personal registrations list

## Description

Implements the paged My Events view for logged-in members, scoped to their own registrations, showing event title, Asia/Manila dates through the shared formatter, and status badges including Not confirmed before cutoff, with the badge classes safelisted in source; creates `MyEventsController`, the card view model and the `_PassPartial` and `_CertificateLinkPartial` host slots that epic-check-in and epic-certificates fill (AD-14).

## Requirements

Verify: A member navigates to My Events and views their registrations paged at 25 rows per page, showing accurate statuses (Pending, Confirmed, Waitlisted, Cancelled, Not confirmed before cutoff) with local time display; host slots for _PassPartial and _CertificateLinkPartial are present.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-events-registration/epic-events-registration.md
