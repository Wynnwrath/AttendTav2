---
id: 5
type: story
title: "Attendance invalidation review disqualification and issued-certificate lock"
parent: epic-certificates
covers: [R3]
after: [4, 4.6]
hitl: false
risk: high
tracker_id: "65"
remote: "https://github.com/rainyDayssss/AttendTa/issues/65"
tracker_status: backlog
---

# Attendance invalidation review disqualification and issued-certificate lock

## Description

Implements IAttendanceInvalidationHandler, which epic-check-in's invalidation transaction (4.6) declares and calls inside its own transaction: throws Forbidden when the registration's certificate is issued, and otherwise sets its pending_approval or eligible review to not_eligible with the correction reason, delivering R3.

## Requirements

Verify: Invalidating an attendance record whose review is pending_approval or eligible sets that review to not_eligible with the reason recorded in the same commit; invalidating one whose certificate is issued is refused with a Forbidden outcome, and the attendance row stays valid.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-certificates/epic-certificates.md
