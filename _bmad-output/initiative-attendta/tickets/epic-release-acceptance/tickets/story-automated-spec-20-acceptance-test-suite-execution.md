---
id: 5
type: story
title: "Automated SPEC §20 acceptance test suite execution"
parent: epic-release-acceptance
covers: [R1]
after: [1.8, 2.9, 3.14, 4.9, 5.10, 6.11, 7.3]
hitl: false
risk: medium
tracker_id: "78"
remote: "https://github.com/rainyDayssss/AttendTa/issues/78"
tracker_status: backlog
---

# Automated SPEC §20 acceptance test suite execution

## Description

Executes the automated SPEC §20 acceptance suite against the integrated dev build, kept in tests/ASI.Basecode.IntegrationTests or tests/ASI.Basecode.Services.Tests (AD-1: no new project), covering account lifecycles from epic-administration, certificate rules from epic-certificates, and report calculations from epic-participation-reports, logging test failures as bugs back to feature epics.

## Requirements

Verify: A runner executes the automated SPEC §20 integration and acceptance test suite against the integrated dev build and all automated test cases pass with green output.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-release-acceptance/epic-release-acceptance.md
