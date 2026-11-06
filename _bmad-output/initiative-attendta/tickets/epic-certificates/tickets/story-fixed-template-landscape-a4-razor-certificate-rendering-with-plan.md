---
title: "Fixed-template certificate rendering"
type: feature
ticket: 2
created: '2026-11-06'
status: built
baseline_revision: '08480597817960bcbbedd0d7a9c116f0b1206718'
route: full
route_source: auto
risk: high
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The certificate tracer produces a generic sample and cannot render certificate snapshot values in the required institutional format.

**Approach:** Add a fixed Razor certificate template and a reusable PDF rendering contract for supplied snapshots. Keep it clearly watermarked as development output until the later signatory story.

## Boundaries & Constraints

**Always:** Render one landscape A4 page with locally packaged, licensed Cinzel fonts and institutional AttendTa branding. Show recipient name, event title, event start, issue date and certificate number; format both dates with the shared Asia/Manila formatter and label. HTML-encode snapshot values. Embed required assets in the document and block network requests during rendering. Preserve the Development-only tracer and its existing authorization, private storage and cleanup behavior. Keep the watermark visible in all outputs of this story, regardless of environment.

**Never:** Implement official issuance, eligibility, certificate number generation, database writes, signatory details, signatures, template editing or new public endpoints. Never use caller-provided HTML, remote assets or disclose private paths.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Certificate sample | Snapshot values including UTC event and issue times | One landscape A4 PDF includes all values, labeled Manila dates, embedded Cinzel, branding and visible development watermark | No error expected |
| Untrusted text | Names or titles include HTML metacharacters | Values appear as text and cannot inject HTML or trigger requests | No network access |
| Long values | Long recipient or event title | Values remain legible and fit on the single page | Test representative boundary values |
| Invalid timestamp or rendering failure | Local-kind DateTime or renderer fails | Reject invalid time or propagate safe failure to existing tracer cleanup | No partial official issuance |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/Interfaces/ICertificatePdfRenderer.cs` and `Services/CertificateTracerService.cs` — current sample-only renderer contract and protected sample orchestration; preserve tracer behavior while exposing typed snapshot rendering for future issuance.
- `ASI.Basecode.WebApp/Certificates/PlaywrightCertificatePdfRenderer.cs` — Playwright 1.64.1 adapter already generates A4 landscape; replace sample markup with a rendered, self-contained Razor template, prohibit network access and wait for fonts before PDF generation.
- `ASI.Basecode.WebApp/Formatting/ManilaTimeFormatter.cs` — format UTC or unspecified database times; reject local-kind timestamps and retain the `Asia/Manila` label.
- `ASI.Basecode.WebApp/Startup.DI.cs`, `Views/CertificateTracer/Index.cshtml` and `Controllers/CertificateTracerController.cs` — existing DI and Development-only protected tracer; do not relax access or storage behavior.
- `ASI.Basecode.WebApp/wwwroot/img/attendta-logo.jpg`, `wwwroot/fonts/` — existing logo; add locally licensed Cinzel assets in an appropriate packaged location. `_bmad-output/initiative-attendta/ux-attendta/DESIGN.md` specifies typography and landscape A4; reference image is composition-only and has no actual certificate content.
- `tests/ASI.Basecode.IntegrationTests/CertificateTracerFlowTests.cs`, `ManilaTimeFormatterTests.cs`, `tests/ASI.Basecode.Services.Tests/CertificateTracerServiceTests.cs` — preserve tracer and formatter coverage; extend real Chromium verification and update fakes if contract changes.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Services/Interfaces/ICertificatePdfRenderer.cs` and related service model — define typed snapshot input and render API without breaking the sample tracer; keep snapshot data separate from HTML.
- [x] `ASI.Basecode.WebApp/Rendering/`, font assets and license — add a fixed Razor view with embedded logo and Cinzel fonts, bounded layout, labeled Manila dates and a conspicuous development watermark.
- [x] `ASI.Basecode.WebApp/Certificates/PlaywrightCertificatePdfRenderer.cs` and `Startup.DI.cs` — render the Razor view with encoded values and self-contained assets, block network, await font loading and return A4 landscape PDF; retain cleanup and cancellation behavior.
- [x] `tests/ASI.Basecode.IntegrationTests/` and `tests/ASI.Basecode.Services.Tests/` — exercise the matrix with real Chromium PDF inspection, encoded hostile input, long values, date conversion, actual embedded font and denied network; preserve existing tracer behavior.
- [x] `docs/database-setup.md` or relevant test documentation — document any additional reproducible PDF inspection prerequisites, font license and verification commands when needed.

**Acceptance Criteria:**
- Given supplied certificate snapshots, when the renderer generates a PDF, then its single landscape A4 page contains the supplied name, event, number and Manila-labeled dates with embedded Cinzel and a visible development watermark.
- Given the template renders, when snapshot text contains markup or external URLs, then it is displayed as text and no external request is made.
- Given a confirmed registration and authorized Management user in Development, when the existing tracer is invoked, then it still returns a sample PDF and cleans up its private files without creating a certificate record.

## Implementation Notes

Implemented the typed snapshot renderer and self-contained Razor PDF with bundled fonts, blocked network access, and the existing protected tracer. The solution build, 42 service tests, and 118 serial integration tests passed. Poppler confirmed page size, text and embedded fonts; normal and maximum-length PDFs were visually inspected. CI installs Poppler for these checks.

## Plan Change Log

## Review Triage Log

- Quick review: 0 high, 1 medium, 0 low, 0 false, 0 maybe-false.
- Medium — patch: compact name and event boxes hide overflow while accepting 255 characters; wide values may be clipped. Reduce compact typography enough to fit accepted values and assert complete wide-character values in the PDF.

## Design Notes

The typed snapshot renderer is an integration seam for later issuance, not an issuance workflow. A deterministic sample can exercise that seam without inventing an official signatory or certificate number format. Use embedded asset bytes rather than host-dependent paths or network resources.

## Verification

**Commands:**
- `dotnet build` — full solution builds.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — service tests pass.
- Set `ATTENDTA_TEST_SQL` to LocalDB master and run the integration suite serially as documented in `docs/database-setup.md`; verify real Chromium rendering and the tracer access/cleanup matrix.
- Inspect the generated PDF with an available PDF text/font/page tool and render it to an image; confirm one A4 landscape page, embedded Cinzel, legible values, branding and watermark.





