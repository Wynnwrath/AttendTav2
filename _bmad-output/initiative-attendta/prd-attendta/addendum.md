---
title: AttendTa PRD — Addendum
created: 2026-11-04
updated: 2026-11-04
---

# AttendTa PRD — Addendum

This addendum holds material that belongs in architecture, delivery planning, or QA documents rather than the PRD.

## A. Technical Stack (Plan p.6; SPEC §1.1)

| Concern | Choice |
|---|---|
| Web framework | ASP.NET Core MVC (C#), Razor views, .NET 9 |
| Styling | Tailwind CSS |
| Database | SQL Server; EF Core database-first; times stored as `datetime2(3)` UTC |
| Enums | Physical SQL Server lookup tables with foreign keys (not editable by administrators) |
| File storage | Persistent server-local root outside the web root; database stores relative paths only; explicitly **not** Supabase or browser localStorage |
| QR | Server-side PNG rendered on demand from `registrations.qr_token`; library not chosen |
| PDF | Server-side landscape A4, embedded fonts, fixed HTML/CSS template rendered to retained PDF; Playwright .NET with Chromium selected |
| Auth | Cookie authentication, signed and expiring; per-request current-state checks; Argon2id password hashing selected; no login throttling in MVP |
| Design | Figma |
| IDE / tools | Visual Studio; GitHub + GitHub Actions CI; Jira for tracking |
| Environments | Student project for localhost or intranet use; Asia/Manila timezone; no enterprise or production deployment assumed; one application server assumed |

The QR library remains open for the Attendance owner, with BMAD agents supporting implementation choices. Playwright .NET is selected for PDF rendering but must be pinned and verified on the demo host. Argon2id is selected; the maintained .NET package and tuning parameters remain an engineering choice for the team and BMAD agents. The inspected backend projects target net9.0; their current UserService uses reversible password encryption, so implementing Argon2 hashing and verification remains necessary. A BCrypt package reference alone does not establish that Argon2 is implemented.

## B. Data Model Summary (ERD / DBML v2.1)

Ten business tables:
- `users`
- `account_requests`
- `members`
- `event_types`
- `venues`
- `events`
- `registrations`
- `attendance`
- `certificates`
- `activity_logs`

`AttendTa-MVP.dbml` is the editable source. Known schema tensions for architecture:

1. **Attendance record uniqueness:** `attendance.registration_id` is unique, which is consistent with FR-25: re-check-in after invalidation is deferred.
2. **Nullable creator fields:** `users.created_by` and `certificates.created_by` are nullable: the setup administrator and system-created Certificate Reviews have no human creator. Other tables declare `created_by` as not null.
3. **Event cover rule:** published and completed Events require a cover; cancelled drafts may have none. Cancelled Events that were previously published keep their cover (implied by "replacement only").
4. **Capacity limit enforcement:** `participant_limit ≤ venues.capacity` is a service-layer rule (FR-9, FR-7). It cannot be enforced through foreign keys.
5. **Pending-only unique indexes:** pending-only unique indexes on `account_requests.email` and `student_number` are required, but the DBML does not express them.
6. **Student-number login lookup:** there is no student-number login. Keep the unique member student-number index for profile identity and manual check-in lookup only; do not add an authentication lookup.
7. **Check-in method enum values:** the PRD's Glossary uses the `check_in_method` enum values directly. FR-24 records `manual_search` for a name lookup and `student_number` for a student-number lookup.
8. **Nullable reason and actor fields:** reason and actor fields are nullable in the DBML, but the PRD requires them for certain states:
   - Rejection reason (FR-2)
   - Cancellation reasons (FR-12, FR-16; optional for member self-cancel, FR-18)
   - Invalidation reason (FR-25)
   - Decline notes (FR-27)
   - `reviewed_at` on waitlisted rows (FR-17 ordering)
   - Issuance fields (FR-28)

   Enforce at the service layer inside the transaction and add state-conditional CHECK constraints or paired-field constraints where SQL Server can express them clearly. Do not commit a state that lacks its required reason or actor/time metadata.
9. **Unified certificate table:** Certificate Review and Certificate are one table. In `certificates`, `issued` is a fourth status value. The PRD Glossary treats them as two concepts over one row, which is fine.
10. **Issuer snapshot:** FR-28 requires a historical issuer display name. Add nullable-before-issuance `issuer_name_snapshot`, required at issuance; retain `issued_by` as the actor FK. Do not derive the historical display name from a user's later-edited profile.
11. **Event type description:** the current requirement makes the description nonblank. Make `event_types.description` NOT NULL and validate nonblank input; the DBML has been aligned to this rule.
12. **Append-only activity log:** `activity_logs` must not be editable or deletable. The runtime application database role must have SELECT/INSERT access only for this table; use a separate migration/admin identity for schema maintenance.

Full gap list: `reconcile-data.md`.

## C. Delivery Ownership (SPEC §1.4; Plan p.8)

| Slice | WBS modules | SPEC owner placeholder |
|---|---|---|
| Authentication (account-request authentication flow, email/password login, sessions and shared access checks) | AUTH | Member 1 (Gabutan) |
| Membership & Master Data (including image storage and pagination helpers) | MD | Member 2 |
| Events & Registration (highest lifecycle and concurrency risk) | EVT, REG | Member 3 |
| Attendance & QR | ATT, QR | Member 4 |
| Certificates, Reports & Administration, including initial administrator setup and role/access management | CERT, REP, ADM | Member 5 (Pinca) |

Team (Plan p.8):
- **PM:** Talisic, Jhon Rosell
- **Technical Manager:** Gabutan, Jerhon Clyde
- **Technical Lead:** Singco, John Paul
- **Developers:** Gabutan, Pinca, Singco, Taguiam, Talisic
- **QA:** Taguiam, Jasper
- **Approver:** Santella, Tomas Emmanuel

Member 1 owns authentication/session and shared access-check helpers only. Pinca owns administrator bootstrap, role/access administration, account deactivation/reactivation screens and activity-log administration. All feature owners enforce the relevant access checks and write their own audit entries. The UC assigns certificate eligibility to Member 4; SPEC moves it to Member 5, which takes precedence.

The historical WBS estimate is a planning reference, not an agreed delivery commitment.

## D. Quality Gates (git_qa_guidelines)

- **Branching:** Git Flow. `main` is production and `dev` is integration. Work happens on `feature/*` and `fix/*` branches, and PRs target `dev`. No direct pushes to `main` or `dev`, and no force-pushes.
- **Tier 1, local:** Unit tests, linting, and static analysis pass before pushing.
- **Tier 2, PR:** CI build and tests are green, with at least one peer approval.
- **Tier 3, dev integration:** E2E, cross-feature, and migration testing pass before release to `main`.
- **Commits:** Conventional Commits.

## E. Risk Register (Plan p.10–11)

| Risk | Probability | Mitigation |
|---|---|---|
| Broad scope with unfinalized edge cases | High | Finalize requirements and wireframes first. The Plan proposes deferring certificates; this PRD keeps them (PRD §7.2 note). |
| Power interruptions | High | Offline development workflow, frequent commits |
| Academic schedule conflicts | High | Smaller tasks during exam weeks; reallocate work |
| Integration/API bugs (such as scanner routes) | High | Guidelines, peer review, and regression tests |
| Typhoons/flooding (Sep–Dec) | Medium | Shift deadlines; asynchronous remote work; cloud backups |
| Team miscommunication | Medium | Stand-ups; agree on API contracts before coding |
| Tooling inexperience (ORM) | Low | Onboarding and pairing |
| Data/artifact loss | Low | Daily pushes; back up migration scripts |

Product-level risks (SPEC):
- **Concurrency.** Final Seat, Promotion, issuance, and correction can race.
- **File and SQL non-atomicity.** A file can be written without a matching database commit, leaving orphaned files.
- **Hosting.** Ephemeral hosting disks are unsuitable for storage.
- **Shared screenshots.** A screenshot of a QR Pass does not prove identity, so officer verification is still needed.
- **Transport.** Phone-camera scanning needs a secure context, so HTTPS is required beyond localhost (PRD NFR-1). An intranet certificate (self-signed or internal CA) must be trusted by officers' phones.
- **Authentication.** Login throttling and lockout are excluded from the MVP; password guessing against registered emails is an accepted limitation (PRD §9.1).

## F. Document Inconsistencies (housekeeping)

These are not PRD decisions:
- The Plan's filename says v2.00, but its cover says 1.04.
- The WBS cover is dated 2026/09/11, but its revision history says 2026/11/03.
- The team member is spelled "Sinco" in one source and "Singco" in another.
- The current SPEC now links to its supplied UC source as `docsss/UC-Project-FL.pdf`.
- The UC PDF's project description is truncated during extraction.
