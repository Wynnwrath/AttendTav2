# Accessibility review

**Scope:** Documentation-only review of `DESIGN.md`, `EXPERIENCE.md`, `.memlog.md`, and `imports/prototype-assets/DESIGN-TOKENS.md`. Findings refer to handoff clarity, not observed UI defects.

## Assessment

No high-severity documentation gaps found. The handoff already specifies the key phone scanner fallback and reachability requirements, keyboard operation and focus, reduced motion, table headers and keyboard scrolling, non-color status cues, async announcements, contrast guidance, and 320px/200% checks. `.memlog.md:74-75` records that these requirements were added and that implementation verification remains outstanding.

## Findings

### P2 — Define form error recovery for keyboard and assistive technology

**Evidence:** `EXPERIENCE.md:45` requires persistent labels and associated errors; `EXPERIENCE.md:68` requires field-specific event-form errors; `EXPERIENCE.md:77` requires tied labels and descriptive error text. The handoff does not say where focus goes after an unsuccessful submit or explicitly say that each inline error is programmatically connected to its field.

**Fix:** Add one sentence to the Input pattern or Accessibility Floor: on failed submission, focus an error summary or the first invalid control; expose the invalid state and programmatically associate each field's error with that control. Keep the existing valid-value retention rule.

### P2 — Specify the dialog's accessible name and modal boundary

**Evidence:** `DESIGN.md:122` calls for a clear dialog title; `EXPERIENCE.md:51` says focus enters, Escape/cancel closes, and focus returns; `EXPERIENCE.md:77` adds focus trapping and restoration. The handoff does not explicitly connect the visible title to the dialog's accessible name or specify that background content is unavailable while the modal is open.

**Fix:** State that the dialog is named by its visible title and that opening it makes background content unavailable to interaction and assistive technology until it closes. Keep the already specified focus entry, containment, Escape/cancel, and return behavior.

## Coverage confirmed

- **Phone scanner:** `EXPERIENCE.md:64,81` covers denied/unavailable camera, reachable controls, event selection, and camera-free fallback; `EXPERIENCE.md:115` describes explicit manual confirmation. `.memlog.md:66,69,75` cautions that screenshots do not verify device camera operation.
- **Tables and reports:** `EXPERIENCE.md:50,77` covers associated headers, reading order, keyboard horizontal scrolling, and non-color report alternatives.
- **Status:** `DESIGN.md:90,117` requires words alongside functional color; `EXPERIENCE.md:49,77` covers exposed selection and announcements for changed results and async outcomes.
- **Visual accessibility:** `DESIGN.md:92,133-134` points to verified contrast pairs, visible focus, reduced motion, and touch target size; the token register restricts faint ink and small amber text (`DESIGN-TOKENS.md:15,22-25,44-50,53-64,206-208`).

## Limitations

This review inspected documentation only. It did not test the prototype or a future app with keyboard, screen readers, zoom, camera permissions, or actual phones; the UX documents themselves state that accessibility conformance is not established (`EXPERIENCE.md:14,77`; `.memlog.md:75`).
