---
id: 3
type: story
title: "Caller-owned transaction unit of work and append-only Activity Log writer"
parent: epic-platform-access
covers: [R5]
after: [1, 2]
hitl: false
risk: high
tracker_id: "13"
remote: "https://github.com/rainyDayssss/AttendTa/issues/13"
tracker_status: done
assignee: "wigglebop25"
---

# Caller-owned transaction unit of work and append-only Activity Log writer

## Description

Adds IUnitOfWork.ExecuteInTransactionAsync (Task and Task<T> overloads; the outermost call opens and commits one transaction on the request-scoped DbContext, nested calls join it, an exception rolls back and calls ChangeTracker.Clear before rethrow, no retry strategy). Adds DomainException(OutcomeCode, message, field?) with the eight OutcomeCodes, and translates SQL 2601 and 2627 into Duplicate in the data layer. Adds the users repository's LockUserAsync(id), a primary-key seek WITH (UPDLOCK, HOLDLOCK, ROWLOCK) that also protects the user's members row (AD-4), and the account_requests repository's LockAccountRequestAsync(id), the same way. Adds IActivityLogWriter.Append, which adds one row built from AuditActions constants and Resources templates, with field-subset JSON, the actor from ICurrentActor, refusal of passwords, hashes, QR tokens and file paths, no SaveChanges, and a throw when no transaction is open. Adds a correlation id to Serilog, moves its file paths from PathManager onto Common:SetupRoot, and deletes PathManager.

## Requirements

Verify: An integration test shows that a business write and its audit row commit together inside ExecuteInTransactionAsync, roll back together when the work throws, and join an outer transaction when nested; a second connection calling LockUserAsync on the same user waits until the first transaction ends. Append throws with no open transaction and refuses a password, hash, QR token or file path. A duplicate key surfaces as DomainException with Duplicate. A log line carries the correlation id, and no PathManager reference remains.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-platform-access/epic-platform-access.md
