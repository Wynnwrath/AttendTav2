---
id: 7
type: story
title: "Member membership reactivation restoring eligibility only"
parent: epic-administration
covers: [R1, R4]
after: [6]
hitl: false
risk: medium
tracker_id: "58"
remote: "https://github.com/rainyDayssss/AttendTa/issues/58"
tracker_status: backlog
---

# Member membership reactivation restoring eligibility only

## Description

Reactivates a member's membership status with a reason under the Management policy in one ExecuteInTransactionAsync: locks the users row, sets members.membership_status_id to active, and appends one member.membership_reactivated Activity Log entry. It restores Eligible Participant status only, read through ParticipationFacts (AD-5). Cancelled Registrations stay Cancelled and users.is_active is unchanged.

## Requirements

Verify: Reactivating an inactive member: after reactivation the member's profile shows membership status Active, and an officer can again approve one of their Pending Registrations, while their login works throughout and their previously cancelled Registrations stay Cancelled.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-administration/epic-administration.md
