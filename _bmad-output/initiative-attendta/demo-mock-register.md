# AttendTa demo data register

Decision: 2026-11-06 — demo rescope, approved by the project owner. Every piece of demo data
that stands in for a missing backend is listed here with the one ticket that removes it. The
initiative's Delivery section and `delivery-queue.md` give the phase order.

This file is the planning record. The code keeps the same ids in a comment on each demo
class, so a search for the id finds every line to delete.

## Rules every demo-phase ticket follows

1. **Real first.** A screen calls an existing service method wherever one returns the data or
   runs the command. Demo data covers only what no existing method provides. A demo provider
   may compose existing reads (for example the published feed plus the per-event registration
   detail) so real events, registrations and members appear.
2. **One switch, guarded at startup.** The configuration key `DemoData:Enabled` turns demo data
   on. It is off in every committed `appsettings*.json` and does not depend on
   `ASPNETCORE_ENVIRONMENT`. When it is on, the app reads the connected database's name at
   startup and refuses to start, with a clear log message, unless that name is `AttendTa_Demo`
   or exactly equals a second key, `DemoData:AllowDatabase` (for example `AttendTaMVP_Test` on
   a builder's machine). It also refuses to start when `DemoData:TokenKey` is missing (rule 10).
   Nothing about the guard appears on screen. With the switch off, the same screens show their
   normal empty state and an unavailable command returns the standard refusal outcome; nothing
   breaks the existing test suites.
3. **No visible sign.** Nothing rendered to a browser tells a viewer the data is not real: no
   banner, badge, ribbon, watermark, toast, footer line, tooltip, alt text, aria label, page
   title, download file name, console message or CSS class/id containing the words "demo",
   "mock", "sample", "fixture", "prototype", "simulated" or "test". Demo records use realistic
   names and values. Generated PDFs use the real snapshot template (`RenderSnapshotAsync`),
   never the watermarked sample render. Server-side logs may note at startup which register ids
   are active.
4. **No real writes.** A simulated command changes only the in-memory demo store, keyed by real
   record ids. It never inserts or updates SQL rows, never writes under `Common:SetupRoot`, and
   never appends to the Activity Log. Restarting the app resets the demo store's check-ins,
   decisions and issues; passes keep scanning (rule 10).
5. **Coherent flow.** One demo store serves every screen, so an action on one screen shows on
   the others for the rest of the session: check in a member, see the attendance in their
   history, complete the event, approve and issue the certificate, see it in the member's list
   and in the report. Start-up seeds scenarios on records already in the connected database so
   every demo screen opens populated.
6. **Never hides a real backend.** When a real implementation exists for a contract, its demo
   provider is not registered. A failing real service shows its real error; it never falls back
   to demo data.
7. **Overlay only named fields.** Demo state may change only these fields of an existing real
   read, and only for records a demo command changed: the event status shown as Completed and
   the check-in open flag on the officer event list, member feed and event detail (M-04, M-09),
   and the account and membership status on the Accounts and Members lists (M-14a, M-14b,
   M-15a, M-15b).
   Nothing else from a real read is altered.
8. **Same refusals as the real command.** Every demo command reloads the real record's current
   state and applies the real ticket's refusal rules, so an invalid action fails on screen the
   way it will after integration:

   | Demo command | Allowed only when | Real rule |
   |---|---|---|
   | Open or close check-in | The event is Published (not unpublished, cancelled or completed) | 4.2 |
   | Complete event | The event is Published, not cancelled, and its start time has passed | 3.12 |
   | Check in (scan or manual) | The token or registration matches this event, check-in is open, the real registration status is Confirmed and its attendance is not invalidated; a repeat returns the original time | 4.3, 4.5 |
   | Invalidate attendance | A reason is given and the registration's certificate is not issued; a pending or approved review becomes not eligible with that reason | 4.6, 6.5 |
   | Approve or decline review | The review is pending and its attendance is valid; decline needs a reason | 6.6 |
   | Issue certificate | The review is approved, the event is Completed and no certificate is issued yet | 6.7 |
   | Deactivate or reactivate an account | A reason is given; deactivating the last active Administrator is refused | 5.4, 5.5 |
   | Deactivate or reactivate a membership | A reason is given | 5.6, 5.7 |

   A registration cancelled through the real service therefore stops its pass from scanning,
   and a cancelled event cannot be completed or opened for check-in.
9. **One folder, one registration method.** A demo provider implements the same service
   interface the real ticket will implement. All demo code lives in one WebApp folder. One
   registration method, behind the switch, calls one registration file per surface; each
   surface owns its file and adds only its own providers there. Removing a row means deleting
   its classes and its line in that surface's registration file.
10. **Stable, unguessable pass tokens.** A demo pass token is the HMAC-SHA256 of the real
    registration id under `DemoData:TokenKey`, a secret set only in the demo machine's
    environment. The same registration gets the same token after a restart, so printed or
    downloaded passes keep scanning, and nobody can derive a token without the key.
11. **Record it.** A ticket that adds demo data fills its row below in the same pull request
    (interface, classes, seeded scenarios). The one ticket named in "Removed by" deletes the
    classes and sets the row to `removed` with the commit. 8.11 verifies that no row is open.
12. **Hand-over between rows.** When a ticket removes a row whose data another open row still
    reads (for example 4.3 makes check-ins real while M-10a still derives reviews from them),
    the remaining demo provider reads the real source from that commit on. The removing ticket
    makes that switch in the same pull request.

## Register

Each row has exactly one removing ticket. A surface that several tickets make real is split
into lettered rows.

| Id | Surface | Missing backend today | Added by | Removed by | State |
|---|---|---|---|---|---|
| M-01 | Demo seam: `DemoData:Enabled`, the startup database guard, `DemoData:TokenKey`, the demo store (event check-in and completed state, pass tokens, attendance, certificate reviews and issues, account and membership status), the registration method and the start-up seed | — (infrastructure) | 1.14 | 8.13 | open |
| M-02 | My Events: all of a member's registrations across events, with status and cutoff labels | Cross-event owner query (only per-event and upcoming-only reads exist) | 3.17 | 3.13 | open |
| M-03 | Pass card QR image and PNG download (rule 10 token, QR drawn by a locally served browser library) | QR library, token read, authorized PNG endpoint | 4.10 | 4.1 | open |
| M-04 | Check-in window open/close command, the shared control partial, and the open flag overlaid on event reads (rule 7) | Open/close command | 1.14 | 4.2 | open |
| M-05 | Check-in by scan: outcome (success, repeat, invalid, closed) and the attendance record | Validation engine and attendance write | 4.11 | 4.3 | open |
| M-06 | Manual lookup of confirmed participants by name or student number, and manual check-in | Registration-scoped search and manual check-in | 4.11 | 4.5 | open |
| M-07 | Attendance invalidation with reason, and its effect on the review (rule 8) | Invalidation command | 4.11 | 4.6 | open |
| M-08 | Member attendance history | Owner-scoped attendance query | 4.12 | 4.7 | open |
| M-09 | Event completion command and the Completed status overlaid on event reads (rule 7) | Completion command | 1.14 | 3.12 | open |
| M-10a | Certificate reviews created at completion (a pending review for each valid check-in of a completed event) | Review creation hook | 6.12 | 6.4 | open |
| M-10b | Review queue and the approve and decline decisions | Review queue and decisions | 6.12 | 6.6 | open |
| M-11a | Certificate issuance (number from the demo store, nothing retained) | Issuance pipeline and retained file | 6.12 | 6.7 | open |
| M-11b | Officer PDF view and download (rendered in memory with the real template) | Authorized download of the retained file | 6.12 | 6.8 | open |
| M-12 | Member certificate history and download links | Owner-scoped certificate query | 6.13 | 6.9 | open |
| M-13a | Report summary: status counts, Confirmed and Valid attendees, rate | Report summary queries | 7.4 | 7.1 | open |
| M-13b | Report no-shows and course/year breakdowns | No-show and distribution queries | 7.4 | 7.2 | open |
| M-13c | Report paged detail rows | Paged detail query | 7.4 | 7.3 | open |
| M-14a | Account deactivation with reason (the Accounts list shows the new state) | Account deactivation and cascade | 5.11 | 5.4 | open |
| M-14b | Account reactivation with reason | Account reactivation | 5.11 | 5.5 | open |
| M-15a | Membership deactivation with reason (the Members list shows the new state) | Membership deactivation and cascade | 5.11 | 5.6 | open |
| M-15b | Membership reactivation with reason | Membership reactivation | 5.11 | 5.7 | open |

6.5 removes no row; its invalidation guard must be in place before 6.7 removes M-11a. 4.4
removes no row; it verifies the 4.11 camera against the real engine after 4.3.

A demo-phase ticket that finds another missing contract adds the next id (M-16, …) with its
removing ticket. When no ticket owns the real work, the builder names that gap in the pull
request and the row's "Removed by" says `8.13`.

## Known limits of the demo

- Deactivating an account in the demo does not end that person's real session or block their
  login; the real behavior arrives with 5.4. Leave that step out of the demo script.
- Check-ins, review decisions and issued certificates last only until the app restarts; passes
  keep scanning.
- A pass scanned in the demo is matched against rule 10 tokens, not `registrations.qr_token`.
