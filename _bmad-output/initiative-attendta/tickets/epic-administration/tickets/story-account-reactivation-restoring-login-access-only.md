---
id: 5
type: story
title: "Account reactivation restoring login access only"
parent: epic-administration
covers: [R3, R4]
after: [4]
hitl: false
risk: low
tracker_id: "56"
remote: "https://github.com/rainyDayssss/AttendTa/issues/56"
tracker_status: backlog
---

# Account reactivation restoring login access only

## Description

Reactivates a deactivated Account with a mandatory reason under the Administrator policy in one ExecuteInTransactionAsync: locks the users row, sets users.is_active to true and appends one user.reactivated Activity Log entry. It restores login access only: cancelled Registrations stay Cancelled with no seats or QR tokens restored, and issued Certificates are untouched.

## Requirements

Verify: Reactivating a deactivated account with a mandatory reason allows the user to log in again, their earlier cancelled Registrations stay Cancelled with no seats restored, and a pre-existing seeded certificate row in the database remains untouched.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-administration/epic-administration.md
