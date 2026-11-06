---
id: 4
type: story
title: "Create certificate reviews in event completion transaction"
parent: epic-certificates
covers: [R1]
after: [1.2, 1.3, 3.12, 4.3]
hitl: false
risk: medium
tracker_id: "64"
remote: "https://github.com/rainyDayssss/AttendTa/issues/64"
tracker_status: backlog
---

# Create certificate reviews in event completion transaction

## Description

Implements IEventCompletionHandler, which epic-events-registration's completion transaction (3.12) declares and calls inside its own transaction, inserting through CertificateRepository a pending_approval certificate review for each Registration with Valid Attendance (ParticipationFacts, AD-5) under the event lock completion already holds, with created_by set to the completing officer and no file or PDF work, delivering R1.

## Requirements

Verify: Completing an event creates pending_approval review rows for every Registration with Valid Attendance and none for absent or invalidated attendance, all in the completion's database transaction, and a completion that fails rolls back every review.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-certificates/epic-certificates.md
