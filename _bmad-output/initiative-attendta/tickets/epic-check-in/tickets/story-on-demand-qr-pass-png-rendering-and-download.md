---
id: 1
type: story
title: "On-demand QR Pass PNG rendering and download"
parent: epic-check-in
covers: [R1, R2]
after: [3.7, 3.13]
hitl: true
risk: medium
tracker_id: "44"
remote: "https://github.com/rainyDayssss/AttendTa/issues/44"
tracker_status: backlog
assignee: "rainyDayssss"
---

# On-demand QR Pass PNG rendering and download

## Description

Adds server-side QR PNG rendering in ASI.Basecode.WebApp/Rendering with no System.Drawing, chooses and records the server QR library (net10.0-compatible, referenced from WebApp only), and adds a download action that checks ownership in Services and streams the PNG generated in memory, encoding only the 64-character token. The download control renders through e4's _PassPartial inside e3's My Events; e4 adds no action to MyEventsController (delivering R2 and the pass presentation part of R1).

## Requirements

Verify: A logged-in Member opens My Events for a Confirmed Registration, clicks Download PNG, and receives a PNG that decodes to their 64-character lowercase hex token; downloading another Member's pass, an unknown registration id, or a cancelled Registration returns the same 403 Forbidden response and message.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md
