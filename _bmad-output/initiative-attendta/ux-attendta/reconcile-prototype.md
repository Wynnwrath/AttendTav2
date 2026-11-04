# Prototype reconciliation

The visual references come from the frontend repository `Wynnwrath/attendta` at commit `7bd8cf7088601cf7efeee145b5237d1ae4cbb3f0`; source HTML and scripts remain in that repository. The indexed bundle contains current and older screenshots, visual CSS, logo, icons and sample imagery. It is not a complete runnable copy of the frontend. For a close Razor port, use the exact source HTML and its page-specific CSS alongside these references, then compare actual renders. The bundle's older screenshots are deliberately not treated as proof of current behavior or state.

The initial visual inventory records the actual Plus Jakarta Sans pages, teal admin treatment and mismatch between the token register's event-mood description and checked-in markup. The experience inventory records differences in waitlisting, attendance status, cutoff behavior, archived status, bulk actions and mobile navigation. DESIGN.md and EXPERIENCE.md capture the resulting guidance: preserve observed appearance where possible while following the current PRD/SPEC for behavior.

Known gaps: no current styled capture for every surface or error state; actual QR camera use over a phone hotspot was not tested; fonts and Tailwind in the prototype rely on external delivery and require a local strategy for disconnected use. The approved signatory and QR library remain open.
