---
id: 4
type: story
title: "Whole IntegrationTests suite execution on SQL Server"
parent: epic-release-acceptance
covers: [R3]
after: [1.1, 1.7, 3.7, 4.3, 5.2, 6.7]
hitl: false
risk: medium
tracker_id: "77"
remote: "https://github.com/rainyDayssss/AttendTa/issues/77"
tracker_status: backlog
---

# Whole IntegrationTests suite execution on SQL Server

## Description

Runs the whole concurrency test suite in tests/ASI.Basecode.IntegrationTests together against the SQL Server named by ATTENDTA_TEST_SQL (AD-17), covering seat allocation, attendance recording, certificate issuance, user approval uniqueness, and last-administrator concurrency from epic-platform-access, epic-events-registration, epic-check-in, epic-administration, and epic-certificates, asserting zero overbooking and zero duplicate records; race conditions are filed as bugs back to feature epics.

## Requirements

Verify: With ATTENDTA_TEST_SQL set to the test SQL Server and not AttendTa_Demo, the whole IntegrationTests suite runs, each test class on its own AttendTa_IT_<guid> database, and all pass with zero overbooking, duplicate attendance, duplicate approval, or duplicate last-admin violations.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-release-acceptance/epic-release-acceptance.md
