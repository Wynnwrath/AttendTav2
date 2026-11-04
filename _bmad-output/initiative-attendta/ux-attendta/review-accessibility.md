# Accessibility Review — AttendTa

## Scope

Document-level review of DESIGN.md and EXPERIENCE.md against their accessibility contract and the current [WCAG 2.2](https://www.w3.org/TR/WCAG22/) guidance. This does not assert whether the existing prototype conforms; the spines explicitly say it has not been tested.

## Findings

- **high** Dynamic outcomes beyond scanning have no explicit screen-reader announcement requirement. EXPERIENCE.md §Accessibility Floor (line 73) calls for live announcements for scan results, while §Component Patterns and §State Patterns describe other asynchronous outcomes such as registration, review, retry, access changes, and certificate issuance/download (lines 43–65). A keyboard or screen-reader user may not learn that these server-backed actions completed or failed if focus stays put. *Fix:* specify programmatically exposed status messages for asynchronous success, failure, and relevant progress outcomes, including registration/review and certificate actions; keep scan-result announcements. See WCAG 2.2 SC 4.1.3, [Status Messages](https://www.w3.org/WAI/WCAG22/Understanding/status-messages.html).

- **medium** The responsive accessibility check is underspecified for reflow. EXPERIENCE.md §Accessibility Floor asks for 200% zoom and “phone width,” while §Responsive & Platform names 390px (lines 73, 77); this does not state a 320 CSS px reflow check. Wide ledgers are allowed to scroll horizontally, but the remaining page content should reflow without loss of information or function. *Fix:* add a 320 CSS px viewport/reflow check and define that only genuinely two-dimensional content such as data tables may require horizontal scrolling, while preserving keyboard and assistive-technology access. See WCAG 2.2 SC 1.4.10, [Reflow](https://www.w3.org/WAI/WCAG22/Understanding/reflow.html).

- **medium** The participation report's distributions and report bars have no nonvisual data equivalent. EXPERIENCE.md §Information Architecture (line 29) promises course/year distributions and DESIGN.md §Colors (line 90) permits event accents for report marks; the accessibility floor only mentions visible status text and table reading order. If a distribution is drawn as a chart/bar graphic, users who cannot perceive it need the same values and relationships in text or a data table, and color must not be the sole key. *Fix:* require a concise chart summary plus a structured values table or equivalent, with labels/legend and distinctions that do not depend on color alone. See WCAG 2.2 SC 1.1.1 and 1.4.1, [Non-text Content](https://www.w3.org/WAI/WCAG22/Understanding/non-text-content.html) and [Use of Color](https://www.w3.org/WAI/WCAG22/Understanding/use-of-color.html).

- **low** Touch-target sizing is left qualitative. DESIGN.md §Shapes (line 108) says to provide “usable touch targets,” and gives 40px control height, but does not set a minimum for narrow icon-only or table actions. *Fix:* name a target-size floor and spacing rule for all controls, including compact mobile actions; WCAG 2.2 SC 2.5.8 uses 24 by 24 CSS pixels as its minimum with defined exceptions. See [Target Size (Minimum)](https://www.w3.org/WAI/WCAG22/Understanding/target-size-minimum.html).

## Existing strengths

- Visible labels, associated field errors, keyboard operation, visible focus, and dialog focus entry/return are specified (EXPERIENCE.md §§Component Patterns and Accessibility Floor).
- Camera denial/unavailability and manual name/student-number fallback are covered; the scanner result includes human-verifiable identity and has a live-announcement requirement.
- Reduced motion, non-color status text, 200% zoom, and table reading order are already called out.
- DESIGN.md restricts faint ink and amber text use and calls for a visible focus ring; the linked token register lists contrast values for palette combinations.
