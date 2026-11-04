# Prototype reference bundle

The screenshots and prototype assets in this folder were copied from the existing AttendTa frontend to keep the visual reference available when the BMAD UX folder is moved to the backend repository. They show the existing prototype, which uses mock data and may display older wording, branding, controls, or out-of-scope actions.

Use screenshots for composition and appearance only. The current PRD and system specification govern required features and behavior. In particular, login is email only, University of Cebu is the approved institution wording, and password recovery is deferred. A screenshot's filename does not prove that a specific modal or result was actually captured.

The `prototype-assets` files are visual source references, not a claim that the backend already implements them. Some fonts in `tokens.css` are loaded from Google Fonts; an intranet deployment needs a local font strategy if external font access is unavailable.

`current-screens/` contains styled Chrome captures taken from frontend commit `7bd8cf7088601cf7efeee145b5237d1ae4cbb3f0` at 1440px desktop width or 390px mobile width. The Tailwind CDN was available for these captures. `reference-screens/` contains older supplied screenshots; prefer the current captures when both cover a screen. Both sets use prototype data and can show actions that the MVP defers. Camera behavior was not verified by these images.

## Reference index


### current-screens

- [current-admin-users.png](current-screens/current-admin-users.png)
- [current-login.png](current-screens/current-login.png)
- [current-member-events.png](current-screens/current-member-events.png)
- [current-member-passes.png](current-screens/current-member-passes.png)
- [current-mobile-admin.png](current-screens/current-mobile-admin.png)
- [current-mobile-login.png](current-screens/current-mobile-login.png)
- [current-mobile-member-events.png](current-screens/current-mobile-member-events.png)
- [current-mobile-scanner.png](current-screens/current-mobile-scanner.png)
- [current-officer-events.png](current-screens/current-officer-events.png)
- [current-officer-report.png](current-screens/current-officer-report.png)
- [current-officer-scanner.png](current-screens/current-officer-scanner.png)
- [current-registration.png](current-screens/current-registration.png)

### reference-screens

- [SS-00_public_portal_landing.png](reference-screens/SS-00_public_portal_landing.png)
- [SS-01_account_registration.png](reference-screens/SS-01_account_registration.png)
- [SS-02_institutional_login.png](reference-screens/SS-02_institutional_login.png)
- [SS-03_admin_roles_governance.png](reference-screens/SS-03_admin_roles_governance.png)
- [SS-05_event_creation_workstation.png](reference-screens/SS-05_event_creation_workstation.png)
- [SS-06_events_index_roster.png](reference-screens/SS-06_events_index_roster.png)
- [SS-10_student_events_calendar_feed.png](reference-screens/SS-10_student_events_calendar_feed.png)
- [SS-11_student_registration_dossier_open.png](reference-screens/SS-11_student_registration_dossier_open.png)
- [SS-13_officer_waitlist_queue_ranking.png](reference-screens/SS-13_officer_waitlist_queue_ranking.png)
- [SS-16_student_digital_event_passes.png](reference-screens/SS-16_student_digital_event_passes.png)
- [SS-17_door_scanner_checkin_confirmed.png](reference-screens/SS-17_door_scanner_checkin_confirmed.png)
- [SS-21_certificates_eligibility_ledger.png](reference-screens/SS-21_certificates_eligibility_ledger.png)
- [SS-24_institutional_certificate_document.png](reference-screens/SS-24_institutional_certificate_document.png)
- [SS-26_executive_participation_dashboard.png](reference-screens/SS-26_executive_participation_dashboard.png)
- [SS-28_member_academic_dossier.png](reference-screens/SS-28_member_academic_dossier.png)
- [SS-31_campus_venues_master_directory.png](reference-screens/SS-31_campus_venues_master_directory.png)
- [SS-33_officer_account_requests.png](reference-screens/SS-33_officer_account_requests.png)
- [SS-34_members_directory.png](reference-screens/SS-34_members_directory.png)
- [SS-35_event_types_management.png](reference-screens/SS-35_event_types_management.png)
- [SS-36_activity_history_audit_trail.png](reference-screens/SS-36_activity_history_audit_trail.png)
- [SS-37_student_participation_history.png](reference-screens/SS-37_student_participation_history.png)

### prototype-assets

- [account.css](prototype-assets/account.css)
- [attendTa_logo.jpg](prototype-assets/attendTa_logo.jpg)
- [bento-lab.jpg](prototype-assets/bento-lab.jpg)
- [bento-summit.jpg](prototype-assets/bento-summit.jpg)
- [browse-events.css](prototype-assets/browse-events.css)
- [certificates.css](prototype-assets/certificates.css)
- [components.css](prototype-assets/components.css)
- [DESIGN-TOKENS.md](prototype-assets/DESIGN-TOKENS.md)
- [dialogs.css](prototype-assets/dialogs.css)
- [event-assembly.jpg](prototype-assets/event-assembly.jpg)
- [event-cyber.jpg](prototype-assets/event-cyber.jpg)
- [event-outreach.jpg](prototype-assets/event-outreach.jpg)
- [event-workshop.jpg](prototype-assets/event-workshop.jpg)
- [hero-3d-pass-cutout.png](prototype-assets/hero-3d-pass-cutout.png)
- [hero-3d-pass.png](prototype-assets/hero-3d-pass.png)
- [hero-pass.jpg](prototype-assets/hero-pass.jpg)
- [hero-silk-bg.jpg](prototype-assets/hero-silk-bg.jpg)
- [icons.svg](prototype-assets/icons.svg)
- [ink-seam.css](prototype-assets/ink-seam.css)
- [tailwind.config.js](prototype-assets/tailwind.config.js)
- [tokens.css](prototype-assets/tokens.css)
