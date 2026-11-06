---
id: 9
type: story
title: "Compiled Tailwind CSS pipeline and role-routed layout shell replacing Bootstrap"
parent: epic-platform-access
after: [1]
hitl: false
risk: medium
tracker_id: "12"
remote: "https://github.com/rainyDayssss/AttendTa/issues/12"
tracker_status: done
assignee: "Wynnwrath"
---

# Compiled Tailwind CSS pipeline and role-routed layout shell replacing Bootstrap

## Description

Pins @tailwindcss/cli 4.3.4 to compile wwwroot/css/app.css from the DESIGN.md tokens, removes Bootstrap 4.3.1 and external CDN stylesheets and scripts, safelists dynamic class names in source, serves all scripts and fonts locally, and implements the responsive role-routed navigation shell and base layouts per the visual specification that carry TempData SuccessMessage and ErrorMessage. The layout renders the RequestVerificationToken meta tag that AJAX posts send as a header (AD-8).

## Requirements

Verify: The dev build compiles Tailwind CSS locally without external CDNs, renders the responsive navigation shell showing permitted routes for Administrator, Officer, and Member across desktop and mobile viewports, and removes all Bootstrap stylesheets and CDN script tags.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-platform-access/epic-platform-access.md
