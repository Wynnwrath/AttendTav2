---
id: 3
type: story
title: "Database and persistent file storage backup, restore, and redeploy survival"
parent: epic-release-acceptance
covers: [R5]
after: [1, 6.7]
hitl: true
risk: medium
tracker_id: "76"
remote: "https://github.com/rainyDayssss/AttendTa/issues/76"
tracker_status: backlog
---

# Database and persistent file storage backup, restore, and redeploy survival

## Description

An operator captures the SQL database and the SetupRoot tree (uploads, certificates, keys) in one backup bundle, verifies the data survives an application restart and redeploy, and restores the bundle into a disposable location; persistence failures are filed as bugs back to feature epics.

## Requirements

Verify: An operator takes one backup bundle holding the SQL backup and SetupRoot (uploads, certificates, keys); restarts and redeploys the app with SetupRoot intact; restores the bundle into a disposable location; and verifies database rows, file checksums and the presence of the Data Protection keys.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-release-acceptance/epic-release-acceptance.md
