# Data-Integrity & Concurrency Review: AD-3 and AD-4 Architecture Spine

**Artifact under review:** `_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md` (specifically AD-3 and AD-4)  
**Supporting sources:** `_bmad-output/initiative-attendta/architecture-attendta/.memlog.md`, `database/attendta-mvp-schema.sql`, `docsss/AttendTa-System-Specification.md`, `docsss/architecture.md`, `_bmad-output/initiative-attendta/prd-attendta/addendum.md`  
**Reviewer lens:** Data Integrity, Concurrency, Lock Protocols, and Deadlock Avoidance  
**Date:** 2026-11-04  

---

## 1. Executive Summary & Verdict

### Verdict: **CONDITIONAL APPROVAL (Requires 6 Targeted Rule Fixes)**

The concurrency foundation defined in AD-3 (caller-owned unit of work) and AD-4 (strict global lock hierarchy, row locks, app locks, discover-lock-recheck) is fundamentally sound and exhibits sophisticated design intent. The parent-lock serialization strategy (`events` row lock gating participation writes; `users` row lock gating participant-specific actions; `attendta:last-admin` serializing administrator changes) successfully prevents race conditions for most lifecycle operations.

However, a strict SQL Server + EF Core 9 concurrency audit reveals **two high-severity gaps** and **four medium-severity vulnerabilities** that will cause runtime deadlocks, data corruption, or reader-writer starvation if not corrected prior to parallel epic implementation:

1. **Reader-Writer Starvation and Blocking (High Severity):** AD-3 specifies default SQL Server `READ COMMITTED` without mandating Read Committed Snapshot Isolation (`READ_COMMITTED_SNAPSHOT ON`). Under default SQL Server `READ COMMITTED`, readers take shared (`S`) locks that block on exclusive (`X`) mutation locks and vice versa. High-throughput check-in scanning, event browsing, and reporting will block each other and cause widespread HTTP request timeouts and conversion deadlocks.
2. **Missing Hierarchy Locks for Venue & Event Type Operations (High Severity):** AD-4 states that event-wide operations lock only the `events` table. This allows concurrent Venue Capacity Edits (R7) to race with Event Creation (R9), Event Participant Limit Edits (R10), and Event Publishing, directly violating the invariant that venue capacity never drops below linked event participant limits. Similarly, Event Type and Venue deactivations can race with Event Publishing.
3. **Omission of Member Signup from Per-Participant Lock Hierarchy (Medium Severity):** AD-4 line 119 omits Member Registration / Signup (`REG-01`) from the list of operations locking the `users` row. This allows new registrations to slip past concurrent account deactivation (FR-32) and event cancellations (R12).
4. **Key-Range Conversion Deadlocks from `HOLDLOCK` (Medium Severity):** AD-4 mandates `SELECT ... WITH (UPDLOCK, HOLDLOCK, ROWLOCK)`. In SQL Server, `HOLDLOCK` (equivalent to `SERIALIZABLE`) takes range locks (`RangeS-U`) when applied across non-unique indexes or missing keys. If child tables or range queries use `HOLDLOCK`, concurrent transactions will encounter fatal conversion deadlocks (SQL Error 1205).
5. **Execution Strategy Retries vs. Dirty ChangeTracker (Medium Severity):** If EF Core's `EnableRetryOnFailure` is enabled without special handling, user-initiated transactions throw `InvalidOperationException`. If wrapped in `IExecutionStrategy`, transient retries re-execute against a tainted `DbContext.ChangeTracker`, producing entity tracking collisions and duplicate side effects.
6. **Lock Escalation Deadlocks on Batch Participation Operations (Medium Severity):** Mass cancellations (R12, R32) updating hundreds of `registrations` rows will trigger SQL Server's 5,000-lock threshold, escalating row locks to an exclusive table lock (`X`), causing system-wide deadlocks against concurrent events.

---

## 2. Exhaustive Trace of the 11 PRD Competing Pairs

Each pair below is traced against AD-3, AD-4, the canonical schema (`attendta-mvp-schema.sql`), and EF Core 9 behaviors.

```
                    GLOBAL LOCK HIERARCHY (AD-4)
 ┌─────────────────────────────────────────────────────────────┐
 │ 1. App Lock: 'attendta:last-admin' (sp_getapplock, Excl)    │
 └──────────────────────────────┬──────────────────────────────┘
                                ▼
 ┌─────────────────────────────────────────────────────────────┐
 │ 2. account_requests (ascending request_id)                  │
 └──────────────────────────────┬──────────────────────────────┘
                                ▼
 ┌─────────────────────────────────────────────────────────────┐
 │ 3. users (ascending user_id)                                │
 └──────────────────────────────┬──────────────────────────────┘
                                ▼
 ┌─────────────────────────────────────────────────────────────┐
 │ 4. event_types (ascending type_id)                          │
 └──────────────────────────────┬──────────────────────────────┘
                                ▼
 ┌─────────────────────────────────────────────────────────────┐
 │ 5. venues (ascending venue_id)                              │
 └──────────────────────────────┬──────────────────────────────┘
                                ▼
 ┌─────────────────────────────────────────────────────────────┐
 │ 6. events (ascending event_id)                              │
 └──────────────────────────────┬──────────────────────────────┘
                                ▼
 ┌─────────────────────────────────────────────────────────────┐
 │ 7. registrations, attendance, certificates (child tables)   │
 └─────────────────────────────────────────────────────────────┘
```

---

### Pair 1: Last-Seat Approvals vs. Promotion (R16 vs. R17)
- **Scenario:** Event $E$ has participant limit 50 and 49 confirmed seats. Officer A approves pending applicant $M_1$. Officer B promotes waitlisted participant $M_2$.
- **Lock Sequence:**
  - *Approval ($M_1$):* Locks $M_1$ in `users` (level 3) $\rightarrow$ Locks Event $E$ in `events` (level 6).
  - *Promotion ($M_2$):* Discovers top waitlist candidate $M_2$ $\rightarrow$ Locks $M_2$ in `users` (level 3) $\rightarrow$ Locks Event $E$ in `events` (level 6).
- **Serialization & Recheck:**
  - Both operations must acquire the row lock on Event $E$. They serialize at level 6.
  - Whichever acquires Event $E$ first checks confirmed count (`SELECT COUNT(*) FROM registrations WHERE event_id = E AND status = 'confirmed'`).
  - First transaction sees 49 confirmed seats $< 50$, updates registration to `confirmed`, generates QR token, commits.
  - Second transaction acquires Event $E$, executes the mandatory recheck under Event $E$'s lock, sees 50 confirmed seats $\ge 50$.
  - For approval: capacity is unavailable; registration is placed on the waitlist or rejected per R16.
  - For promotion: capacity is unavailable; transaction aborts without mutating state per R17.
- **Deadlock Assessment:**
  - Officer A locks $M_1 \rightarrow E$. Officer B locks $M_2 \rightarrow E$. Because $M_1 \neq M_2$, neither holds a lock the other requests. Both progress monotonically up the hierarchy ($3 \rightarrow 6$). **No deadlock.**
- **Verdict:** **PREVENTED & DEADLOCK-FREE** (provided approval rechecks the entire waitlist priority under the event lock).

---

### Pair 2: Member Cancel vs. Approval (R18 vs. R16)
- **Scenario:** Member $M_1$ cancels their pending/confirmed registration $R_1$ on Event $E$ while an Officer approves $R_1$.
- **Lock Sequence:**
  - *Member Cancel:* Locks $M_1$ in `users` (level 3) $\rightarrow$ Locks Event $E$ in `events` (level 6).
  - *Approval:* Locks $M_1$ in `users` (level 3) $\rightarrow$ Locks Event $E$ in `events` (level 6).
- **Serialization & Recheck:**
  - Both operations target the same participant $M_1$. They serialize immediately on $M_1$'s `users` row at level 3.
  - *If Member Cancel wins:* $R_1$ status becomes `cancelled`. When Approval acquires $M_1$ and $E$, its recheck discovers status is `cancelled`. Approval aborts and throws `DomainException(OutcomeCode.Stale)`.
  - *If Approval wins:* $R_1$ status becomes `confirmed`. When Member Cancel acquires $M_1$ and $E$, R18 permits cancellation of confirmed registrations before event start. Cancel releases the seat and clears the QR token.
- **Deadlock Assessment:**
  - Identical lock order ($M_1 \rightarrow E$). **No deadlock.**
- **Verdict:** **PREVENTED & DEADLOCK-FREE.**

---

### Pair 3: Check-in vs. Deactivation (R23/R24 vs. R32)
- **Scenario:** Participant $M_1$ attempts QR or manual check-in at Event $E$ while Administrator deactivates $M_1$'s account or membership.
- **Lock Sequence:**
  - *Check-in:* Locks $M_1$ in `users` (level 3) $\rightarrow$ Locks Event $E$ in `events` (level 6).
  - *Deactivation:* Locks $M_1$ in `users` (level 3) $\rightarrow$ Locks all upcoming events for $M_1$ in ascending ID order ($E_{a} < E_{b} < \dots$).
- **Serialization & Recheck:**
  - Both lock $M_1$'s `users` row first. They serialize at level 3.
  - *If Check-in wins:* Verifies $M_1$ is active per AD-5 canonical facts (`users.is_active = 1` and `members.status = 'active'`). Inserts `attendance` row, commits. When Deactivation runs, R32 states: *"Deactivation cancels the person's Registrations on not-yet-started Events that lack Valid Attendance"*. Because valid attendance now exists for Event $E$, Event $E$'s registration is preserved.
  - *If Deactivation wins:* Sets $M_1$ inactive, locks upcoming events, cancels registrations lacking attendance. When Check-in runs, recheck finds $M_1$ inactive (failing AD-5) and registration cancelled. Check-in throws `DomainException(OutcomeCode.Forbidden)`.
- **Deadlock Assessment:**
  - Both lock $M_1$ before any event. Monotonic ascending acquisition prevents circular wait. **No deadlock.**
- **Verdict:** **PREVENTED & DEADLOCK-FREE.**

---

### Pair 4: Event Cancel vs. Deactivation vs. Check-in (R12 vs. R32 vs. R23)
- **Scenario:** Three-way competition on Event $E$ for participant $M_1$: Management cancels Event $E$; Admin deactivates $M_1$; Officer scans $M_1$'s QR code.
- **Lock Sequence:**
  - *Check-in ($M_1$):* Locks $M_1$ (level 3) $\rightarrow$ Locks Event $E$ (level 6).
  - *Deactivation ($M_1$):* Locks $M_1$ (level 3) $\rightarrow$ Locks Event $E$ (level 6, plus other events ascending).
  - *Event Cancel ($E$):* Locks Event $E$ (level 6).
- **Serialization & Recheck:**
  - Check-in and Deactivation serialize on $M_1$ at level 3.
  - Whichever wins $M_1$ contends with Event Cancel for Event $E$ at level 6.
  - *If Event Cancel acquires $E$ first:* R12 requires verification that no valid attendance and no issued certificate exists on Event $E$. Finding none, Event Cancel sets $E$ to `cancelled`, `is_check_in_open = 0`, and cancels all child registrations. Check-in subsequently acquires $E$, discovers $E$ is cancelled and check-in closed, and fails. Deactivation subsequently acquires $E$, discovers $M_1$'s registration is already cancelled, and safely skips it.
  - *If Check-in acquires $E$ first:* Check-in records attendance and commits. Event Cancel subsequently acquires $E$, executes the mandatory attendance check, discovers Valid Attendance exists, and aborts with `DomainException(OutcomeCode.Invalid, "Cannot cancel event with valid attendance")`.
- **Deadlock Assessment:**
  - Check-in and Deactivation lock $3 \rightarrow 6$. Event Cancel locks only $6$. Event Cancel never requests level 3 (`users`). No cycle can form. **No deadlock.**
- **Verdict:** **PREVENTED & DEADLOCK-FREE.**

---

### Pair 5: Attendance Invalidation vs. Issuance (R25 vs. R28)
- **Scenario:** Management invalidates an erroneous attendance record for participant $M_1$ on Event $E$ while an Officer issues $M_1$'s certificate.
- **Lock Sequence:**
  - *Invalidation:* Locks $M_1$ (level 3) $\rightarrow$ Locks Event $E$ (level 6).
  - *Issuance (Architecture §8 two-phase):* Phase 1 renders PDF in memory without locks. Phase 2 opens short transaction: Locks $M_1$ (level 3) $\rightarrow$ Locks Event $E$ (level 6).
- **Serialization & Recheck:**
  - Both serialize on $M_1$ (level 3) and Event $E$ (level 6).
  - *If Invalidation wins:* Checks that certificate is not issued. Sets `attendance.invalidated_at = SYSUTCDATETIME()`, updates `certificates.status = 'not_eligible'`, commits. When Issuance Phase 2 acquires locks, recheck evaluates AD-5 `Valid Attendance` (`invalidated_at IS NULL`). Valid Attendance is false. Issuance aborts, deletes generated PDF, and throws `DomainException(OutcomeCode.Invalid)`.
  - *If Issuance Phase 2 wins:* Rechecks attendance is valid and status is `eligible`. Updates `certificates.status = 'issued'`, records certificate number and snapshots, commits. When Invalidation acquires locks, recheck discovers `certificates.status = 'issued'`. Per R25, issued certificates block ordinary invalidation. Invalidation aborts and throws `DomainException(OutcomeCode.Invalid)`.
- **Deadlock Assessment:**
  - Identical lock order ($3 \rightarrow 6$). Phase 1 PDF rendering runs outside SQL transactions, ensuring minimal lock duration in Phase 2. **No deadlock.**
- **Verdict:** **PREVENTED & DEADLOCK-FREE.**

---

### Pair 6: Certificate Approval vs. Invalidation (R27 vs. R25)
- **Scenario:** Officer approves certificate review (setting `eligible`) for participant $M_1$ on completed Event $E$ while an Officer invalidates $M_1$'s attendance.
- **Lock Sequence:**
  - *Approval:* Locks $M_1$ (level 3) $\rightarrow$ Locks Event $E$ (level 6).
  - *Invalidation:* Locks $M_1$ (level 3) $\rightarrow$ Locks Event $E$ (level 6).
- **Serialization & Recheck:**
  - Both lock $M_1 \rightarrow E$.
  - *If Invalidation wins:* Invalidates attendance and transitions the certificate review to `not_eligible` with notes. When Approval runs, recheck finds Valid Attendance is false and review is already concluded. Approval fails.
  - *If Approval wins:* Sets review to `eligible`. Because the certificate is not yet *issued*, Invalidation can subsequently run, invalidating attendance and flipping the review from `eligible` to `not_eligible` (R25).
- **Deadlock Assessment:**
  - Identical lock order. **No deadlock.**
- **Verdict:** **PREVENTED & DEADLOCK-FREE.**

---

### Pair 7: Venue Capacity Edit vs. Event Create/Edit/Publish (R7 vs. R9/R10)
- **Scenario:** Management reduces Venue $V$'s capacity from 150 to 100 (R7: *"Capacity never drops below the participant limit of a linked draft or published Event"*). Simultaneously, an Officer creates an event, edits an event's participant limit to 120, or publishes an event in Venue $V$.
- **Vulnerability Trace in Current AD-4:**
  - AD-4 line 120 states: *"Event-wide operations lock only the event. These are publish, edit, cancel, complete and the check-in toggle."*
  - Under this rule:
    1. Venue Capacity Edit locks Venue $V$ (level 5). Queries `SELECT MAX(participant_limit) FROM events WHERE venue_id = V AND status IN ('draft', 'published')`. Sees max is 80.
    2. Concurrently, Event Edit locks Event $E$ (level 6). It does **not** lock Venue $V$. It reads Venue $V$'s old capacity (150) and updates $E$'s limit to 120.
    3. Both transactions commit! Venue $V$ now has capacity 100, but linked Event $E$ has limit 120. **Constraint violation of R7.**
- **Deadlock Assessment:**
  - If Venue Edit locks Venue $V$ (level 5), and Event Edit/Publish locks Venue $V$ (level 5) before Event $E$ (level 6), both follow hierarchy order $5 \rightarrow 6$.
  - Event operations serialize behind Venue Capacity Edits on Venue $V$. **No deadlock.**
- **Verdict:** **RACE CONDITION DETECTED IN SPINE. REQUIRES FIX 2.**

---

### Pair 8: Event Type / Venue Deactivation vs. Publish (R6/R7 vs. R10)
- **Scenario:** Management deactivates Event Type $T$ (R6) or Venue $V$ (R7) while an Officer publishes draft Event $E$ referencing $T$ and $V$ (FR-10 requires active type and venue).
- **Vulnerability Trace in Current AD-4:**
  - Deactivation of Type $T$ locks Type $T$ (level 4). Sets `is_active = 0`.
  - Deactivation of Venue $V$ locks Venue $V$ (level 5). Sets `is_active = 0`.
  - Publishing Event $E$ locks only Event $E$ (level 6 per current AD-4).
  - Without locking Type $T$ and Venue $V$, Publish reads their active status as `true`, while deactivation executes concurrently. Both commit. A published event now points to an inactive Type or Venue.
- **Deadlock Assessment:**
  - By requiring Event Publish to acquire row locks on Type $T$ (level 4) $\rightarrow$ Venue $V$ (level 5) $\rightarrow$ Event $E$ (level 6), Publish serializes deterministically against deactivations without deadlocks.
- **Verdict:** **RACE CONDITION DETECTED IN SPINE. REQUIRES FIX 2.**

---

### Pair 9: Account Approval Duplicates (R2 vs. R2)
- **Scenario:** Two officers concurrently approve the same account request $Req_1$, or approve two distinct requests $Req_1$ and $Req_2$ that share the same student number or email.
- **Lock Sequence:**
  - *Same Request ($Req_1$):* Both attempt to lock $Req_1$ in `account_requests` (level 2). First acquires lock, verifies `status = 'pending'`, creates user and member, sets status to `approved`, commits. Second acquires lock, rechecks status, discovers it is `approved`, rolls back and throws `DomainException(OutcomeCode.Stale)`.
  - *Distinct Requests ($Req_1, Req_2$) with duplicate email or student number:*
    - Both acquire locks on their respective requests at level 2.
    - Both attempt to insert into `users` (`UNIQUE ([email])`) and `members` (`UNIQUE ([student_number])`).
    - SQL Server enforces unique constraints. The second committer fails with SQL Error 2601/2627.
    - Per AD-9, the Data layer maps SQL 2601/2627 into `DomainException(OutcomeCode.Duplicate)`.
- **Deadlock Assessment:**
  - Both lock level 2 then insert into levels 3/members. **No deadlock.**
- **Verdict:** **PREVENTED & DEADLOCK-FREE.**

---

### Pair 10: Concurrent Role Demotion/Deactivation of Admins (R31/R32 vs. R31/R32)
- **Scenario:** System has exactly two active administrators ($A_1$ and $A_2$). Admin $A_1$ attempts to demote/deactivate $A_2$, while Admin $A_2$ attempts to demote/deactivate $A_1$.
- **Lock Sequence:**
  - Both transactions enter AD-4 step 1:
    `sp_getapplock @Resource = 'attendta:last-admin', @LockMode = 'Exclusive', @LockOwner = 'Transaction'`
- **Serialization & Recheck:**
  - `sp_getapplock` is mutually exclusive. Only one transaction acquires the application lock; the second blocks.
  - Transaction 1 acquires `attendta:last-admin`.
  - Transaction 1 locks $A_2$ in `users` (level 3).
  - Transaction 1 counts active administrators: `SELECT COUNT(*) FROM users WITH (UPDLOCK, HOLDLOCK) WHERE role = 'admin' AND is_active = 1`.
  - Count is 2. Demoting $A_2$ leaves 1 active admin. Transaction 1 executes update, commits, and releases `attendta:last-admin`.
  - Transaction 2 unblocks, acquires `attendta:last-admin`.
  - Transaction 2 locks $A_1$ in `users` (level 3).
  - Transaction 2 recounts active administrators: count is now 1.
  - Demoting $A_1$ would reduce active admins to 0. Transaction 2 aborts and throws `DomainException(OutcomeCode.Invalid, "Cannot demote or deactivate the last active administrator")`.
- **Deadlock Assessment:**
  - Serialized at step 1 before any database row locks are requested. **Deadlock is mathematically impossible.**
- **Verdict:** **PREVENTED & DEADLOCK-FREE** (with minor hardening in Fix 6).

---

### Pair 11: FR-32 Deactivation Spanning Many Events (R32 vs. Concurrent Operations)
- **Scenario:** User $M_1$ has active registrations across 15 upcoming events ($E_1, E_2, \dots, E_{15}$). Administrator deactivates $M_1$. Simultaneously, multiple officers approve registrations, cancel events, and check in attendees across those same 15 events.
- **Trace & Hierarchy Enforcement:**
  - Deactivation locks $M_1$ in `users` (level 3) **first**.
  - Under $M_1$'s lock, Deactivation discovers all upcoming registrations for $M_1$.
  - It sorts the affected event IDs in strictly ascending order: $E_1 < E_2 < \dots < E_{15}$.
  - It acquires row locks on $E_1, E_2, \dots, E_{15}$ in that exact sequence.
  - Under each event lock, it verifies that no valid attendance exists, then cancels the registration via `IRegistrationCancellationService`.
- **Why Circular Deadlocks Are Avoided:**
  - Any single-event operation on $E_k$ (e.g., Check-in or Approval for another user $M_x$) locks $M_x$ (level 3) $\rightarrow E_k$ (level 6). Because $M_x \neq M_1$, Deactivation never waits on $M_x$.
  - If another user $M_2$ is also being deactivated across an overlapping subset of events (e.g. $E_2, E_8$), both deactivations request event locks in strictly ascending order ($E_2 \rightarrow E_8$). Neither can hold $E_8$ while waiting for $E_2$.
- **Vulnerabilities Identified:**
  1. *Omission of Member Signup from AD-4:* If a member can submit a new registration without locking `users` first, $M_1$ could register for an event while deactivation is in flight.
  2. *Lock Escalation:* Updating hundreds of registrations during mass cancellation can trip SQL Server's 5,000-lock threshold, escalating to an exclusive table lock and deadlocking other events.
- **Verdict:** **PREVENTED & DEADLOCK-FREE SUBJECT TO FIXES 3 AND 5.**

---

## 3. Deep-Dive Technical Risk Analysis

### 3.1 Transaction Isolation & Reader-Writer Starvation (READ COMMITTED vs. RCSI)
- **The Engine Reality:** In SQL Server, default `READ COMMITTED` is a locking isolation level. When a transaction mutates data, it holds Exclusive (`X`) row/page locks until commit. Plain read queries (`SELECT`) request Shared (`S`) locks.
  - `S` locks block on `X` locks.
  - `X` locks block on `S` locks.
- **The AttendTa Failure Mode:** During event check-in, hundreds of participants browse their "My Events" feed, while officers scan QR passes (writing to `attendance`) and generate participation reports (aggregating over `registrations`). Under default `READ COMMITTED`, reporting queries holding `S` locks will freeze QR check-in writes, while check-in writes holding `X` locks will freeze user screen loads, causing connection pool exhaustion and conversion deadlocks.
- **The Solution:** Read Committed Snapshot Isolation (RCSI) must be enabled on the database:
  ```sql
  ALTER DATABASE [AttendTaMVP_Test] SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
  ```
  Under RCSI, readers view the point-in-time snapshot of committed data via `tempdb` row versions without taking `S` locks. Readers never block writers; writers never block readers. Mutating transactions still acquire `UPDLOCK`, `HOLDLOCK`, and `X` locks to serialize with each other.

### 3.2 Key-Range Locking and Conversion Deadlocks from `HOLDLOCK`
- **The Engine Reality:** AD-4 mandates `SELECT … WITH (UPDLOCK, HOLDLOCK, ROWLOCK)`. `HOLDLOCK` is a synonym for `SERIALIZABLE`.
  - On a clustered unique index with an existing key (e.g. `WHERE event_id = @id`), SQL Server locks the specific row (`Key:U`). This is safe.
  - On a non-unique column, an index range, or a non-existent row, SQL Server cannot lock an individual row. It acquires `RangeS-U` key-range locks over the entire interval between keys in the index.
- **The Deadlock:** If two concurrent transactions execute `SELECT ... WITH (UPDLOCK, HOLDLOCK) WHERE event_id = @id AND status = 'confirmed'`, both acquire `RangeS-U` locks on the same index key range. When both subsequently attempt to insert or update within that range, both request `RangeX-X` conversion. Neither can convert because the other holds `RangeS-U`. Result: **Instant Deadlock (Error 1205)**.
- **The Invariant Rule:** `Lock…Async` methods must **only** lock records by unique primary key. Child rows must never be locked with range `HOLDLOCK` hints; they are serialized strictly by locking their parent's primary key (`events.event_id`).

### 3.3 Lock Escalation on Batch Mutations
- **The Engine Reality:** When a single transaction or statement acquires $\ge 5,000$ row/page locks on a single table, SQL Server's lock manager escalates the locks to an Exclusive Table Lock (`X` table lock).
- **The Impact on AttendTa:** Event cancellation (R12), member deactivation (R32), and event completion (R13) update or insert large numbers of child rows in `registrations` and `activity_logs`. If lock escalation triggers on `registrations`, a table lock is taken on `registrations`, instantly blocking all operations on *every other event in the entire system*, triggering cascading timeouts and deadlocks.
- **The Fix:** Lock escalation must be disabled on high-traffic tables:
  ```sql
  ALTER TABLE [registrations] SET (LOCK_ESCALATION = DISABLE);
  ALTER TABLE [attendance] SET (LOCK_ESCALATION = DISABLE);
  ALTER TABLE [activity_logs] SET (LOCK_ESCALATION = DISABLE);
  ```

### 3.4 EF Core ChangeTracker & Execution Strategy Interplay
- **The Engine Reality:** AD-3 specifies `IUnitOfWork.ExecuteInTransactionAsync(work, ct)`. In EF Core with SQL Server:
  1. If `EnableRetryOnFailure()` is configured in `UseSqlServer`, calling `context.Database.BeginTransactionAsync()` directly throws:
     `InvalidOperationException: The configured execution strategy 'SqlServerRetryingExecutionStrategy' does not support user-initiated transactions.`
  2. Wrapping the transaction in `strategy.ExecuteAsync()` causes EF Core to re-execute `work` on transient SQL errors (such as 1205 deadlock).
  3. However, on retry, the request-scoped `DbContext.ChangeTracker` still holds the added, modified, and deleted entities from the first attempt! Calling `context.Add(...)` again throws tracking collision exceptions, and any non-database side effects (such as file writes) are duplicated.
- **The Architecture Invariant:** Per AD-4 line 122 (*"Never retry a mutation blindly after a timeout"*), mutating units of work must **not** execute blind automatic retries. `ExecuteInTransactionAsync` must either disable automatic transaction retries or execute `ChangeTracker.Clear()` and re-read state upon receiving a recoverable failure.

---

## 4. Itemized Findings & Concrete Rule Fixes

| ID | Severity | Location | Title |
| :--- | :--- | :--- | :--- |
| **F-01** | **High** | AD-3 (`architecture-attendta.md:99`), AD-16, Schema | Unspecified Read Committed Snapshot Isolation (RCSI) causes reader-writer blocking |
| **F-02** | **High** | AD-4 (`architecture-attendta.md:120`) | Missing Venue and Event Type locks on Event Create/Edit/Publish |
| **F-03** | **Medium** | AD-4 (`architecture-attendta.md:119`) | Omission of Member Signup from per-participant lock hierarchy |
| **F-04** | **Medium** | AD-4 (`architecture-attendta.md:109`) | `HOLDLOCK` on non-unique predicates causes key-range conversion deadlocks |
| **F-05** | **Medium** | AD-4 (`architecture-attendta.md:108-124`), Schema | Lock escalation table locks during mass cancellations |
| **F-06** | **Low** | AD-4 (`architecture-attendta.md:111`) | Unbounded timeout and missing return check on `sp_getapplock` |

---

### Finding F-01: Unspecified RCSI Causes Reader-Writer Starvation and Deadlocks
- **Severity:** High
- **Location:** AD-3 (line 99), AD-16 (lines 245–249), and `database/attendta-mvp-schema.sql`
- **Evidence:**
  > `architecture-attendta.md:99`: *"This opens one READ COMMITTED transaction on the request-scoped `DbContext`, runs `work`, calls `SaveChangesAsync` once and commits."*
  > `database/attendta-mvp-schema.sql`: Contains no `ALTER DATABASE ... SET READ_COMMITTED_SNAPSHOT ON;`
- **Impact:** In default SQL Server, readers take `S` locks that block writers' `X` locks and vice versa. High-throughput check-in and dashboard browsing will deadlock and freeze Kestrel worker threads.
- **Proposed Fix:**
  1. Add to `database/attendta-mvp-schema.sql` (after line 19):
     ```sql
     ALTER DATABASE CURRENT SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
     ```
  2. Update AD-3 rule text (line 99):
     > *"Each mutating use case runs inside `IUnitOfWork.ExecuteInTransactionAsync(work, ct)`. This opens one READ COMMITTED transaction on the request-scoped `DbContext` (with `READ_COMMITTED_SNAPSHOT ON` configured on the database so readers never acquire shared locks or block writers), runs `work`, calls `SaveChangesAsync` once and commits. On any exception it rolls back."*

---

### Finding F-02: Missing Venue and Event Type Locks on Event Create/Edit/Publish
- **Severity:** High
- **Location:** AD-4 (lines 118–120)
- **Evidence:**
  > `architecture-attendta.md:120`: *"Event-wide operations lock only the event. These are publish, edit, cancel, complete and the check-in toggle."*
- **Impact:** Venue capacity reductions (R7) execute concurrently with Event Creations (R9), Event Participant Limit edits (R10), and Publishing, allowing an event's limit to exceed venue capacity. Additionally, Event Type and Venue deactivations (R6, R7) race with Event Publishing.
- **Proposed Fix:**
  Replace AD-4 line 120 with:
  > *"Event-wide operations acquire locks strictly in AD-4 hierarchy order:
  > - **Event Publish:** locks Type (level 4) $\rightarrow$ Venue (level 5) $\rightarrow$ Event (level 6). Verifies both Type and Venue are active.
  > - **Event Create / Edit of Participant Limit or Venue:** locks Venue (level 5) $\rightarrow$ Event (level 6 if editing existing). Verifies participant limit does not exceed venue capacity.
  > - **Event Cancel, Complete, Check-in Toggle, and Descriptive Edits:** lock only the Event (level 6)."*

---

### Finding F-03: Omission of Member Signup from Per-Participant Lock Hierarchy
- **Severity:** Medium
- **Location:** AD-4 (line 119)
- **Evidence:**
  > `architecture-attendta.md:119`: *"Per-participant operations lock the owner's `users` row first, then the event. These are approve, decline, promote, member cancel, admin cancel, check-in, attendance invalidation, eligibility decision and issuance."*
- **Impact:** Member Registration / Signup (`REG-01`) is omitted. A member can register while an administrator deactivates the account (FR-32) or while an officer cancels the event (R12), producing inconsistent registration state.
- **Proposed Fix:**
  Update AD-4 line 119 to include `signup`:
  > *"Per-participant operations lock the owner's `users` row first, then the event. These are **signup**, approve, decline, promote, member cancel, admin cancel, check-in, attendance invalidation, eligibility decision and issuance."*

---

### Finding F-04: `HOLDLOCK` on Non-Unique Predicates Causes Key-Range Conversion Deadlocks
- **Severity:** Medium
- **Location:** AD-4 (line 109)
- **Evidence:**
  > `architecture-attendta.md:109`: *"Locks are taken through repository `Lock…Async` methods that run `SELECT … WITH (UPDLOCK, HOLDLOCK, ROWLOCK)`."*
- **Impact:** If `HOLDLOCK` is used on non-unique columns (such as `event_id` on `registrations`), SQL Server acquires key-range locks (`RangeS-U`), causing conversion deadlocks with concurrent inserts and updates.
- **Proposed Fix:**
  Add an explicit constraint to AD-4 line 109:
  > *"Locks are taken through repository `Lock…Async` methods that run `SELECT … WITH (UPDLOCK, HOLDLOCK, ROWLOCK)` **strictly by unique primary key**. Repositories must never issue `HOLDLOCK` on non-unique columns, non-clustered index ranges, or non-existent rows. Child table rows (`registrations`, `attendance`, `certificates`) serialize on their parent event's primary key lock (`events.event_id`), not via key-range locks."*

---

### Finding F-05: Lock Escalation Table Locks During Mass Cancellations
- **Severity:** Medium
- **Location:** AD-4 (lines 108–124) and Schema
- **Evidence:** In mass event cancellations (R12), member deactivations spanning many events (R32), or event completions (R13), transactions modifying hundreds of child rows will trigger SQL Server's 5,000-lock threshold, escalating to an exclusive table lock on `registrations`.
- **Impact:** A table lock on `registrations` blocks all registration and check-in operations across all events in the system, causing cascading deadlocks.
- **Proposed Fix:**
  1. Add to `database/attendta-mvp-schema.sql`:
     ```sql
     ALTER TABLE [registrations] SET (LOCK_ESCALATION = DISABLE);
     ALTER TABLE [attendance] SET (LOCK_ESCALATION = DISABLE);
     ALTER TABLE [activity_logs] SET (LOCK_ESCALATION = DISABLE);
     ```
  2. Add to AD-4:
     > *"To prevent lock escalation deadlocks during mass cancellations (R12, R32), table lock escalation is disabled on `registrations`, `attendance`, and `activity_logs` in the schema."*

---

### Finding F-06: Unbounded Timeout and Missing Return Check on `sp_getapplock`
- **Severity:** Low
- **Location:** AD-4 (line 111)
- **Evidence:**
  > `architecture-attendta.md:111`: *"1. App lock `attendta:last-admin` (`sp_getapplock`, Exclusive, owner = Transaction), only when a role or access change could remove an active administrator."*
- **Impact:** Omitting `@LockTimeout` defaults to infinite wait (`-1`), which can hang worker threads indefinitely if a transaction stalls. Furthermore, negative return codes from `sp_getapplock` (-1 timeout, -2 cancelled, -3 deadlock) must be handled.
- **Proposed Fix:**
  Update AD-4 line 111:
  > *"1. App lock `attendta:last-admin` (`sp_getapplock`, Exclusive, owner = Transaction, `@LockTimeout = 5000`), only when a role or access change could remove an active administrator. Any return code $< 0$ must immediately throw `DomainException(OutcomeCode.Stale, "Administrative lock acquisition timed out")`."*

---

## 5. Review Summary Matrix for Epic Handoff

```
┌─────────────────────────────────┬──────────┬───────────────────────────┬───────────────────────────────────────────┐
│ Competing Pair / Requirement   │ Status   │ Governing Rule            │ Required Action                           │
├─────────────────────────────────┼──────────┼───────────────────────────┼───────────────────────────────────────────┤
│ 1. Last-seat approval vs promot │ PASS     │ AD-4 Event lock (lvl 6)   │ Recheck confirmed count under event lock  │
│ 2. Member cancel vs approval    │ PASS     │ AD-4 User lock (lvl 3)    │ Serialize on users.user_id                │
│ 3. Check-in vs deactivation     │ PASS     │ AD-4 User lock (lvl 3)    │ Serialize on users.user_id                │
│ 4. Event cancel vs deact vs chk │ PASS     │ AD-4 Event lock (lvl 6)   │ Refuse cancel if valid attendance exists  │
│ 5. Invalidation vs issuance     │ PASS     │ AD-4 User (3) + Event (6) │ Two-phase issuance; recheck valid attend  │
│ 6. Cert approval vs invalid     │ PASS     │ AD-4 User (3) + Event (6) │ Flip review to not_eligible on invalid    │
│ 7. Venue capacity vs Event edit │ DEFECT   │ AD-4 missing Venue lock   │ Apply Fix F-02 (lock Venue before Event)  │
│ 8. Type/Venue deact vs Publish  │ DEFECT   │ AD-4 missing Type/Venue   │ Apply Fix F-02 (lock Type/Venue on Pub)   │
│ 9. Account approval duplicates  │ PASS     │ AD-4 Request lock + UQ    │ Schema UX constraints translate to Dupl   │
│ 10. Last-admin concurrent demot │ PASS     │ AD-4 sp_getapplock        │ Apply Fix F-06 (5s timeout & check code)  │
│ 11. FR-32 multi-event deactiv   │ PASS*    │ AD-4 Ascending event lock │ Apply Fix F-03 (signup in lvl 3) & F-05   │
└─────────────────────────────────┴──────────┴───────────────────────────┴───────────────────────────────────────────┘
```
