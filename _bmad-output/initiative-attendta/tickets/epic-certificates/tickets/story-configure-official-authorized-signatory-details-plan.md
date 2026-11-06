---
title: 'Configure official certificate signatory'
type: 'feature'
ticket: 3
created: '2026-11-06'
status: 'built'
baseline_revision: 'ba67ca030d3cd35c764a1f9421f07034052956b1'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The fixed certificate template has no approved signatory and is explicitly marked as a development sample, so it cannot satisfy the official signatory requirement.

**Approach:** Configure the approved name and title, embed the approved signature asset in the fixed template, and render official output without the development watermark once the approved identity is supplied.

**Signatory provided by the user:** Christian Kyle Ducay, Organization President. Use the user-requested generated signature-style graphic in `proposed-christian-kyle-ducay-signature.png`; it is a generated design, not a copy of a handwritten signature.

## Boundaries & Constraints

**Always:** Use the approved signatory details and signature only; render a self-contained, landscape A4 PDF with the existing branding, embedded fonts, snapshot values and Manila-labeled dates. Encode text and block external network requests. Preserve the protected Development tracer and its private-file cleanup.

**Never:** Substitute a different signatory or signature graphic, add a template editor, implement issuance or change eligibility, storage, or authorization behavior. Do not expose arbitrary file paths or accept caller-supplied HTML.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Approved signatory configured | Name, title and approved packaged signature | Single landscape A4 PDF shows the three details and has no development watermark | Render succeeds |
| Missing or invalid signatory configuration or asset | Required input absent or invalid | No misleading official PDF is produced | Fail clearly before returning output |
| Untrusted text | Snapshot or configured text includes markup | Encoded text appears as text; no network request is made | No injected markup |
| Existing tracer | Authorized Development request | Existing sample path and cleanup remain functional | Existing access and cleanup rules remain enforced |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Certificates/PlaywrightCertificatePdfRenderer.cs` renders a typed snapshot through Razor, reads local logo/fonts as data URIs, blocks HTTP(S), and generates A4 landscape. Preserve encoding, timestamp validation, font loading and cancellation.
- `ASI.Basecode.WebApp/Rendering/CertificateTemplateViewModel.cs` carries template values; `ASI.Basecode.WebApp/Views/Certificates/CertificateTemplate.cshtml` contains the layout, static development watermark and banner.
- `ASI.Basecode.WebApp/appsettings.json` and `Startup.DI.cs` are the existing configuration and DI entry points. Package the approved signature as a WebApp rendering asset, and use non-secret configuration for name and title.
- `tests/ASI.Basecode.IntegrationTests/CertificateTracerFlowTests.cs` verifies Chromium output, page dimensions, text, fonts, request blocking and tracer behavior; update and extend these checks. `tests/ASI.Basecode.Services.Tests/CertificateTracerServiceTests.cs` protects tracer orchestration.
- Story 6.2 is merged at the baseline revision. Its template and tests are the prerequisite; do not replace its rendering contract or introduce official issuance here.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.WebApp/appsettings.json`, `Startup.DI.cs` and a configuration type under `Rendering/` — bind and validate approved non-secret name and title and the packaged signature asset; fail safely when required content is missing.
- [x] `ASI.Basecode.WebApp/Rendering/`, `Certificates/PlaywrightCertificatePdfRenderer.cs` and `Views/Certificates/CertificateTemplate.cshtml` — embed the approved signature, display the approved name and title, remove development markings from official output and preserve the existing self-contained rendering safeguards.
- [x] `tests/ASI.Basecode.IntegrationTests/CertificateTracerFlowTests.cs` — verify the output and missing/invalid configuration, encoding, network isolation, page size and existing tracer behavior.
- [x] `docs/database-setup.md` or an appropriate deployment guide — document the non-secret configuration and packaged asset requirements without claiming placeholder details are approved.

**Acceptance Criteria:**
- Given the approved signatory configuration and image, when the fixed template is rendered, then one landscape A4 PDF shows the approved name, title and signature and contains no development watermark or banner.
- Given missing or invalid required signatory content, when official output is requested, then rendering fails clearly without producing an official-looking PDF.
- Given the existing authorized Development tracer, when it renders and streams a sample, then its authorization and private-file cleanup continue to work.

## Implementation Notes

Implemented official rendering with the user-selected signatory and packaged generated graphic, with a separate watermarked tracer sample. Full build passed with zero warnings; 52 service tests and 122 LocalDB integration tests passed. A representative PDF was rendered and visually inspected for layout, page size and signature.

## Plan Change Log

## Review Triage Log

- Quick review: 0 high, 0 medium, 0 low, 1 false, 0 maybe-false.
- False — rejected: the reviewer identified the general Master Data assignment boundary, but the user explicitly requested certificate story 6.3 and approved its implementation plan, authorizing this specific work.

## Design Notes

The development tracer and official rendering currently share one renderer. The implementation must distinguish sample output where necessary so incomplete or unapproved identity never produces a misleading official PDF.

## Verification

**Commands:**
- `dotnet build` — full solution builds.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — service tests pass.
- Set `ATTENDTA_TEST_SQL` to local SQL Server master and run `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` — certificate rendering and existing integration tests pass, with Chromium and Poppler available.
- Inspect a rendered PDF's page dimensions, text, embedded fonts and signature image; visually verify layout and absence of development markings.
