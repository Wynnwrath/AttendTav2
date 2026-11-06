---
id: 6
type: story
title: "Management certificate review queue and approval decisions"
parent: epic-certificates
covers: [R4, R7, R8]
after: [4, 5, 1.3, 1.6, 1.9]
hitl: false
risk: medium
tracker_id: "66"
remote: "https://github.com/rainyDayssss/AttendTa/issues/66"
tracker_status: backlog
---

# Management certificate review queue and approval decisions

## Description

Implements the 25-row paged review queue on the shared PageRequest and _Pager (AD-13), and the approve and decline decisions: each discovers its candidate, locks the owner's users row and then the event, rechecks pending_approval status and Valid Attendance through ParticipationFacts, and refuses as Stale a candidate that changed; decline displays the SPEC §14.2 warning that the decision cannot be reopened in this release and requires a non-blank reason validated inside the transaction; each decision appends a certificate.approved or certificate.declined entry, delivering R8, R4 and R7.

## Requirements

Verify: An Officer approves a pending review to mark it eligible and declines another with a required reason to make it not_eligible; both append Activity Log entries, a decline without a reason is refused, the queue shows 25 rows per page, and a decision on a review invalidated since it was listed is refused as Stale.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-certificates/epic-certificates.md
