---
id: 8
type: story
title: "Authorized certificate streaming download endpoint"
parent: epic-certificates
covers: [R6]
after: [7]
hitl: false
risk: medium
tracker_id: "68"
remote: "https://github.com/rainyDayssss/AttendTa/issues/68"
tracker_status: backlog
---

# Authorized certificate streaming download endpoint

## Description

Delivers R6 through a GET action that takes a certificate id and never a path, admits the owner (matching ActorUserId) or any caller with the Management policy, lets inactive memberships through, relies on the AD-8 session check that refuses inactive Accounts, and streams the file_path read through IPrivateFileStore.OpenReadAsync after a containment check as application/pdf.

## Requirements

Verify: The owner and a Management user each download an issued certificate as application/pdf; an unauthenticated request and an inactive Account are sent to sign-in, a non-owner Member and an unknown id receive the identical not-found response, and no request can name a path.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-certificates/epic-certificates.md
