# Validation Report — AttendTa

- **DESIGN.md:** [DESIGN.md](DESIGN.md)
- **EXPERIENCE.md:** [EXPERIENCE.md](EXPERIENCE.md)
- **Run at:** 2026-11-04T18:19:53+08:00

## Overall verdict

The spines provide a useful handoff: all five PRD journeys, the 12 paired component patterns, token references and local links are covered. Before implementation, clarify the approval outcome and several state, confirmation and accessibility behaviors. No critical findings were identified.

Documentation review only. No rendered UI, keyboard/screen-reader session, actual phone-camera scan, pixel parity or accessibility conformance was tested. The design and experience spines were not changed. Earlier report responses are superseded by this independent re-review.

## Category verdicts

- Flow coverage — strong
- Token completeness — strong
- Component coverage — strong
- State coverage — adequate
- Visual reference coverage — adequate
- Bloat & overspecification — adequate
- Inheritance discipline — strong
- Shape fit — strong

## Findings by severity

### Critical (0)


### High (1)

**[Requirements] — Clarify competing last-seat approvals (EXPERIENCE.md:106)**

The failure path permits competing reviews to be rejected or returned with the current outcome. A valid competing approval that loses the last seat must become Waitlisted (PRD FR-16).

Fix: Name Waitlisted explicitly; reserve rejection for an action that is no longer valid.


### Medium (6)

**[Requirements] — Distinguish membership inactivity from account deactivation (EXPERIENCE.md:24–30,64)**

The UX groups access changes without explaining the two outcomes. Membership inactivity blocks participation while preserving account/history access; account deactivation blocks account access. The PRD remains authoritative.

Fix: Give each control its own confirmation summary and upcoming-registration effects; state that neither triggers promotion. Parent triage lowers the reviewer’s High to Medium because this is missing UX clarity, with no explicit override of the PRD.

**[Requirements] — Scope required reasons by action (EXPERIENCE.md:51)**

The generic dialog rule can imply every consequential action requires a reason, although FR-18 makes a member cancellation reason optional.

Fix: Say reason required where specified; explicitly keep member cancellation optional.

**[Requirements] — Warn before final certificate decline (EXPERIENCE.md:65,117–124)**

The flow lists decline but omits the required warning that it cannot be reopened in this release.

Fix: Add the warning and required reason before confirmation.

**[State coverage] — Complete empty-queue and history states (EXPERIENCE.md:59–69)**

Empty states are not explicit for account-request/event management queues, My Events/history and certificate review queues. Existing master-data/log empty states are already covered.

Fix: Describe valid-empty versus no-match states and an appropriate next step for each surface.

**[Accessibility] — Specify form-error focus recovery (EXPERIENCE.md:45,68,77)**

Errors and labels are covered, but failed-submit focus destination and explicit programmatic field-error connections are unspecified.

Fix: Focus an error summary or first invalid field, expose invalid state and connect each error to its control.

**[Accessibility] — Specify dialog naming and modal isolation (DESIGN.md:122; EXPERIENCE.md:51,77)**

Focus containment is specified, but accessible naming and isolation of background content are not explicit.

Fix: Name the dialog from its visible title and make background content unavailable while it is open.


### Low (2)

**[Requirements] — Keep cutoff label distinct from stored status (EXPERIENCE.md:62)**

Not confirmed before cutoff is listed beside actual statuses, inviting an unnecessary additional status.

Fix: Describe it as a derived label for Pending/Waitlisted after the cutoff; stored status remains unchanged.

**[Visual reference coverage] — Indexed references trade inline discoverability for brevity (DESIGN.md:135; imports/README.md; .memlog.md:74)**

Thirty-two imported files remain index-only rather than context-linked. All links resolve; the index is an explicitly accepted handoff choice.

Fix: Optionally add a few contextual links for the highest-value references. This is an accepted tradeoff, not a blocker.

## Reviewer files

- [Rubric](review-rubric.md)
- [Requirements](review-requirements.md)
- [Accessibility](review-accessibility.md)
