---
id: 2
type: story
title: "Hotspot network binding and trusted HTTPS mobile camera scanning"
parent: epic-release-acceptance
covers: [R4]
after: [1, 4.4, 4.5]
hitl: true
risk: high
tracker_id: "75"
remote: "https://github.com/rainyDayssss/AttendTa/issues/75"
tracker_status: backlog
---

# Hotspot network binding and trusted HTTPS mobile camera scanning

## Description

Decides the HTTPS mechanism and host address for the hotspot with Technical Manager Gabutan (Q2) and records the decision in this epic's Notes; then configures Kestrel to serve HTTPS directly on the hotspot LAN interface, with a certificate the officers' phones trust and no IIS or reverse proxy (AD-16); then verifies phone Chrome camera QR scanning where the 4.4 scanner page is served over the hotspot HTTPS origin here, not only localhost, filing scanner defects as bugs back to feature epics.

## Requirements

Verify: With the host disconnected from the internet, an officer opens the hotspot HTTPS URL in mobile Chrome with no certificate warning, grants camera access, scans a valid attendee QR Pass and sees the attendee's name and student number, and finds the same attendee through manual lookup on that phone.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-release-acceptance/epic-release-acceptance.md
