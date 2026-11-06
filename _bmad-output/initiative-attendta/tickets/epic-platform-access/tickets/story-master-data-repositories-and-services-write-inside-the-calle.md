---
id: 11
type: story
title: "Master-data repositories and services write inside the caller-owned transaction"
parent: epic-platform-access
after: [2, 3]
hitl: false
risk: high
tracker_id: "14"
remote: "https://github.com/rainyDayssss/AttendTa/issues/14"
tracker_status: done
assignee: "FivePesos"
---

# Master-data repositories and services write inside the caller-owned transaction

## Description

Converts the mutating methods of EventTypeRepository, VenueRepository and MemberRepository to async with a CancellationToken and removes their SaveChanges calls, wraps every EventTypeService, VenueService and MemberService write in ExecuteInTransactionAsync, and replaces their InvalidDataException throws and catch blocks in the Event Type, Venue and Member controllers with DomainException outcomes. The actorUserId parameters and GetActorUserId stay until epic-master-data removes them, and the venue image store is left to epic-master-data.

## Requirements

Verify: A forced failure after a master-data write leaves no partial row; no repository method calls SaveChanges; and no InvalidDataException remains in EventTypeService, VenueService, MemberService or the three controllers.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-platform-access/epic-platform-access.md
