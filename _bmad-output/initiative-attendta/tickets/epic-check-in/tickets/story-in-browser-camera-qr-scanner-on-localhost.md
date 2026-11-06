---
id: 4
type: story
title: "In-browser camera QR scanner on localhost"
parent: epic-check-in
covers: [R4]
after: [2, 3]
hitl: true
risk: high
tracker_id: "47"
remote: "https://github.com/rainyDayssss/AttendTa/issues/47"
tracker_status: backlog
assignee: "rainyDayssss"
---

# In-browser camera QR scanner on localhost

## Description

Adds the Management door scanner page where an Officer points the laptop camera at a Confirmed Member's Pass; the scanner page also runs on the hotspot HTTPS origin in epic-release-acceptance entry 2 (no hard-coded localhost), while this entry verifies it on localhost. A browser QR decoder library chosen in this entry is served from wwwroot, with no runtime CDN (AD-14). The page reads the laptop camera and posts each decoded token to the entry 3 endpoint with the RequestVerificationToken header read from the layout meta tag (AD-8), then shows the JSON outcome and message (AD-9). Scan payloads are not logged (Consistency: Diagnostics) (delivering the camera scan interaction of R4).

## Requirements

Verify: An Officer opens the scanner on localhost on a laptop webcam, points it at a Confirmed Member's QR Pass, and sees name and student number on success; presenting the same pass again shows the original check-in time; the browser network log shows no request to any host other than localhost.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md
