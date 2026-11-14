# AttendTa UI/UX phased implementation plan

Status: implementation in progress on `feature/ui-ux-consistency`. Each phase receives a separate commit.
Date: 2026-11-14.
Baseline last verified: local dev tracked upstream/dev at da1f048. Recheck Git state before implementation and preserve concurrent work.

## Agreed scope

Improve affected management and member pages using the existing AttendTa visual identity. Shared components establish consistent spacing, typography, controls, tables, dialogs, navigation and feedback. Update DESIGN.md and PRODUCT.md to reflect agreed behavior; the user's direction supersedes conflicting old row-link/timezone-caption rules.

Bulk approval applies to Membership Account Requests, Registration Review, the event dossier pending roster, and Certificate Eligibility Review. Declines/cancellations and certificate issuance remain individual. Certificate eligibility approval does not issue a PDF. No bulk issuance.

Keep essential identity, comparison and decision data visible. Move secondary information into contextual details dialogs without inventing or exposing data the user is not authorized to view. Details should not require chains of modals.

## Phase 1 — Shared layout and component foundations

Targets: management/member/public shells, Styles/app.css and generated CSS, shared controls, shared dialogs, DESIGN.md and PRODUCT.md.

- Align management content to a consistent 24px gutter beside the sidebar; use 16px on small screens. Remove extra centering gaps for operational lists while retaining readable widths for prose/forms. Align breadcrumbs, headings, toolbars and tables.
- Standardize page/section spacing, panel padding, typography, field/label/helper spacing and status styles.
- Replace text-like row-action styling with proper bordered buttons. Define intentional main-action, row-action and dialog-action sizes, consistent hover/focus/disabled/loading states, and usable touch targets.
- Establish shared search/filter toolbar, read-only details dialog, decision dialog and pagination-footer patterns.
- Standardize Close/Cancel/Confirm placement and keyboard focus. Protect modified forms/reasons from accidental dismissal; unchanged/read-only dialogs close freely. Avoid nested modal chains.
- Remove repeated visible Asia/Manila labels while retaining existing time conversion.

Completion: representative list, form and dialog share alignment and control sizes; CSS builds; keyboard and responsive checks pass. Changes to shared dialog dismissal preserve existing save/retry behavior.

## Phase 2 — Compact tables, search, filters and pagination

Targets: shared PageRequest/PagedResult/pager/result-count contracts and corresponding controllers/services/repositories; management list views.

Visible information and details:

| Page | Visible | Details |
| --- | --- | --- |
| Membership requests | Applicant/student number, course/year, requested date, status, actions | Email, mobile, section, remaining application information, recorded decline reason |
| Members | Name/student number, course/year, status, actions | Email, mobile, section, membership type |
| Events | Title/type, venue, start date/time, status, actions | Deadline, participant limit, description and secondary event information |
| Registration review | Participant/student number, event, status, actions | Exact submission time and secondary registration information |
| Dossier roster | Participant/student number, status, actions | Submission time and secondary registration information; event context remains above roster |
| Certificate eligibility | Member/student number, attendance confirmation, eligibility status, actions | Exact attendance time, full reason, available certificate details |
| Issued registry | Member/student number, certificate number, issue date, View PDF | Exact issuance time and attendance information |
| Reports | Participant/student number, course/year, registration status, attendance result | Exact check-in time and additional available participant information |
| Event types | Name, short description preview, status, actions | Full description when it needs more space |
| Venues | Name, location, capacity, status, actions | Availability notes, contact person, full photo |
| Accounts | Name, email, role, status, actions | Add details only if meaningful additional information exists |
| Activity log | Date/time, actor, action, entity type, details action | Entity ID, actor type, full reason |

- Add/standardize server-backed search and relevant filters. Search becomes Clear for the applied query; editing allows a new search. Clear search preserves unrelated filters. A separate clear-filters action resets the relevant filters explicitly.
- Preserve search/filter/sort/event context across pagination and actions. Changed search/filter resets to page 1.
- Replace fixed 25-row paging with validated server-backed page sizing and Auto-fit, targeting five records where space permits and fewer on short screens. Larger choices only appear when they fit; unrestricted 10/25 choices are superseded by this agreement.
- Measure available content space, not monitor height. Keep page/count metadata synchronized with fetched rows. Avoid unstable resize/refetch loops; page-size changes preserve a meaningful position where possible and clear selection.
- Standardize footer: 'Showing 1–5 of 42 members', filtered totals, exact last-page ranges, 'Page 1 of 1', visible disabled boundary navigation and useful zero-result states. No fabricated totals or pagination when there are zero pages.
- Remove desktop horizontal scrolling through column reduction and wrapping. Use a labelled stacked presentation for narrow screens when necessary. Do not shrink essential text to force a fit.

Completion: small/large/filtered/empty datasets have correct counts and boundaries; no omitted/duplicated records due to page calculations; list context survives navigation; normal desktop tables have no horizontal or internal vertical scrollbar. Natural page scrolling remains available on phones, zoom and long content.

## Phase 3 — Draft events, publishing and cover management

Targets: event create/edit modal and fallback forms, event list/dossier publishing flow, cover view/controller/service responses.

- Group fields into Event information, When and where, Registration and capacity.
- Rename Starts to Event starts and Signup deadline to Registration closes. Explain registration opens immediately on publishing; no scheduled registration-opening field exists.
- Show inline date relationship/capacity guidance. Distinguish draft-save validity from publish readiness.
- Show publication checklist with specific problems and direct fixes: cover missing, registration deadline elapsed, inactive venue/type, capacity mismatch, or changed event state.
- Replace generic publication failure text with specific actionable reasons; backend validation remains authoritative.
- Review schedule, venue, deadline and capacity before publishing; explain which fields become locked afterward.
- Implement Add/Manage Cover as a reusable modal from list/dossier/editor workflow, with current image, selected-image preview, actual type/size guidance, Save/Cancel and confirmed permitted removal. Preserve standalone fallback route.
- Do not stack cover/editor modals or discard an unsaved event editor to switch tasks. Save first or offer an explicit safe transition. Refresh readiness after cover save.

Completion: valid publish path and each blocked condition are understandable; failures preserve input and identify the fix; cover preview/removal and post-publication field restrictions are clear.

## Phase 4 — Registration and certificate review, including bulk approval

Targets: Membership Account Requests, Registration Review, dossier pending roster, Certificate Eligibility Review; shared selection/confirmation/result components; approval endpoints and services.

- Remove always-visible decline/cancellation reason fields; open contextual dialogs on choosing the action. Repeat target identity/event and consequences; preserve reason/context on validation failure.
- Add eligible-pending-row checkboxes, selected count, Select all on this page, Deselect all, and Approve selected. Selection clears on page/search/filter/event changes.
- Confirm selected names/count before submission. Recheck authorization and per-record eligibility on the server; audit each successful approval and return understandable per-record results.
- Registration bulk approval obeys existing capacity, event cutoff and waitlist-priority rules; it must not silently bypass or change them. Review those rules before choosing batching/transaction details.
- Treat certificate eligibility approval as a separate step from individual PDF issuance. Preserve finality warnings on individual declines/issuance.
- Keep ordinary individual approval available.

Completion: valid, mixed-state, stale, duplicate-submission and capacity/cutoff cases have meaningful results; successes/failures are attributable to each selected person; no hidden selections or duplicate decisions.

## Phase 5 — Event detail hierarchy and reports

Targets: management dossier summary, member event details, Participation & Attendance Report, shared event selector.

- Give title/status, event date/time, venue, deadline, capacity/remaining seats and relevant action clear prominence. Reduce duplicated metadata and separate descriptions from operational facts.
- Keep lifecycle/check-in actions contextual and consistent without giving every action equal emphasis.
- Align report summary cards and value baselines across final, non-final, cancelled and empty states. Match breakdown-panel spacing and responsive behavior without unnecessary fixed-height blank space.
- Keep event-wide summary metrics distinct from participant-table filtering; label their scope.
- Distinguish same-title events in selectors with date and venue where available. Keep selected-event context visible.

Completion: users can identify when/where an event happens and its availability without hunting; report states remain aligned and semantically accurate; recurring events are distinguishable.

## Phase 6 — Login and remaining member journeys

Targets: login, membership request/submission pages, Campus Events feed, My Events tabs, My Passes, Profile and certificate viewer.

- Refine login hierarchy, fields, password-control behavior where appropriate, validation and Sign in prominence. Place Request membership as a separate secondary action with adequate text-to-button spacing.
- Match membership request form typography, fields, helpers, errors and actions to the same system; give submitted state a clear next step.
- Align browse-event cards and View details actions; make schedule/venue/signup status scannable. Apply appropriate search/filter/paging behavior.
- Standardize registrations, attendance and certificate history rows/details, statuses, PDF actions, pass layout and profile/form spacing. Member data remains owner-scoped.
- Preserve originating ledger page/search/filter/event context when returning from PDF viewer.
- Replace generic developer-oriented error screen with actionable user recovery and a reference ID.

Completion: login-to-membership, event browsing/registration, pass access, history and certificate return journeys have consistent controls and no avoidable navigation dead ends.

## Phase 7 — Integrated Impeccable critique, polish and verification

- Run Impeccable critique and polish for changed surfaces, with an integrated final pass. Record issues and fixes, rather than treating a clean detector as proof of UX quality.
- Verify rendered desktop/laptop/phone layouts, short windows and 200% zoom, keyboard operation, dialog focus/restore, meaningful touch targets and contrast.
- Exercise long names/titles/reasons, zero/one/many records, filtered results, first/last pages, validation failures, unavailable/stale records, failed requests and repeat submissions.
- Verify Auto-fit does not churn requests, move selected people unexpectedly or obscure pagination. Never enforce a viewport cap by clipping content or disabling necessary scrolling.
- Build solution and CSS; run targeted tests for changed paging/search/filter contracts and approval behavior. Do not manufacture tests that mirror CSS. Broaden testing only for meaningful integration risk or failures.
- Keep DESIGN.md and PRODUCT.md aligned with shipped behavior. Provide changed-page list, validation evidence and remaining limitations.

Completion: shared patterns match across affected pages, behavioral tests pass, rendered checks support the result, and remaining limitations are explicit. Current live-browser audit is incomplete because Computer Use could not verify the Windows browser URL; resolve browser inspection before making visual-verification claims.

## Working boundaries

### Phase 1 delivery evidence

Shared 24px management gutter, aligned breadcrumb, bordered row controls, readable labels/table
headers, consistent editor field spacing and a reusable unsaved-dialog guard implemented.
Existing standalone form-panel width caps retained. Public/member centering retained.
Repeated visible timezone notes removed without changing conversion.

Validation: Release solution build succeeded with 0 warnings/0 errors after restoring current
dependencies; Tailwind CSS regenerated; all 9 JS tests passed; detector returned no findings.
Dual-agent critique and polish corrected coarse-pointer row-control specificity. Browser
component-fixture checks verified controls, modified-input warning, Keep editing, Discard changes
and focus return. Full authenticated-page/mobile visual verification remains for Phase 7.

Preserve the current visual identity and existing authorization, audit, date conversion, capacity and state rules. Add targeted backend support for agreed query/paging/approval capabilities. Do not introduce bulk declines, bulk issuance, a scheduled registration-opening feature, database resets or unrelated business-rule changes as part of UI refinement.

No further user design decisions are presently required. Surface genuinely new business-rule conflicts with a concrete example if implementation reveals one.
