---
id: 9
type: story
title: "Member certificate download links in My Events"
parent: epic-certificates
covers: [R2]
after: [8, 3.13]
hitl: false
risk: low
tracker_id: "69"
remote: "https://github.com/rainyDayssss/AttendTa/issues/69"
tracker_status: backlog
---

# Member certificate download links in My Events

## Description

Delivers R2 by adding the issued-certificate query and link fields to epic-events-registration's MyEventCardViewModel and rendering _CertificateLinkPartial inside its My Events card, filling the _CertificateLinkPartial host slot created by 3.13, so a Member sees a download link only for an issued certificate they own; the controller and card model stay with e3 (AD-14).

## Requirements

Verify: A Member views a completed attended event in My Events and sees a download link for their issued certificate, while a review that is not yet issued shows no link.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-certificates/epic-certificates.md
