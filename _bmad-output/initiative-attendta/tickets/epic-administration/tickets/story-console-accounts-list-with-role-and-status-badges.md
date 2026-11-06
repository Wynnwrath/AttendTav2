---
id: 1
type: story
title: "Console accounts list with role and status badges"
parent: epic-administration
covers: [R6]
after: [1.1, 1.2, 1.6, 1.9]
hitl: false
risk: low
tracker_id: "52"
remote: "https://github.com/rainyDayssss/AttendTa/issues/52"
tracker_status: done
assignee: "FivePesos"
---

# Console accounts list with role and status badges

## Description

Renders the paged Accounts and Roles console for the Administrator policy, showing each account's name, email, role name (roles lookup via users.role_id), active status and action links, with the shared 25-row pager and Tailwind shell from epic-platform-access. Filters apply before counting.

## Requirements

Verify: Given at least 26 user accounts seeded in the database, an Administrator logs in, opens Console Accounts & Roles, and sees a 25-row paged table of users with correct role and status badges, working page navigation, and functional search filtering.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-administration/epic-administration.md
