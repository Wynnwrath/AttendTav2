# Technology Currency & Reality-Check Review: Architecture Spine

**Artifact under review:** `_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md`  
**Supporting sources:** `_bmad-output/initiative-attendta/architecture-attendta/.memlog.md`, `ASI.Basecode.*/*.csproj`, `database/attendta-mvp-schema.sql`  
**Reviewer lens:** Technology Currency, Version Verification, Ecosystem Compatibility, and Runtime Reality  
**Verification date:** 2026-11-04  

---

## 1. Executive Summary & Verdict

### Verdict: **CONDITIONAL APPROVAL (Requires Resolution of 2 High & 3 Medium Findings)**

The architecture spine demonstrates exemplary technical grounding. The vast majority of library versions, runtime features, and SQL Server mechanisms were thoroughly researched rather than hallucinated or asserted from stale training data:
- **Exact versions verified:** Pinned versions for `Microsoft.EntityFrameworkCore.SqlServer` (9.0.22), `Microsoft.Playwright` (1.64.1), `@tailwindcss/cli` (4.3.4), `xunit.v3` (4.0.2), `Konscious.Security.Cryptography.Argon2` (1.3.1), `Serilog` (4.3.0), and the `mcr.microsoft.com/mssql/server:2025-latest` container were all verified against live NuGet, npm, MCR, and Microsoft registries.
- **Architectural mechanisms validated:** `System.TimeProvider`, `sys.sp_getapplock`, `UPDLOCK, HOLDLOCK, ROWLOCK` lock escalation and concurrency semantics, `CookieAuthenticationEvents.OnValidatePrincipal` immediate revocation, `PersistKeysToFileSystem` Data Protection persistence, and the Kestrel HTTPS LAN binding requirement (strictly necessitated by mobile browser `navigator.mediaDevices.getUserMedia` secure-context enforcement for QR camera scanning) are technically sound and integrate cleanly with the stack.

However, a strict currency audit against the live environment on 2026-11-04 reveals **two high-severity** and **three medium-severity** issues that must be addressed:

1. **.NET 9 End of Support in 6 Days (2026-11-10) & Local SDK Mismatch (High Severity):** .NET 9 is an STS release reaching official Microsoft End of Support on November 10, 2026—before the AttendTa student demo occurs. Furthermore, developer workstations running current rolling distributions (such as the local workstation's .NET SDK 10.0.113) do not have the .NET 9 SDK/runtime installed. Marking `net9.0` as `[ADOPTED]` while simultaneously carrying Open Question 1 ("This blocks the Stack pin") creates an immediate release hazard.
2. **AutoMapper 14.0.0 CVE-2026-32933 Blocks CI Vulnerability Gates (High Severity):** While AD-18 restricts AutoMapper to flat view models to prevent the uncontrolled recursion DoS flaw (CVE-2026-32933 / GHSA-rvv3-g6hj-g44x), standard CI tools (`dotnet list package --vulnerable`, Dependabot, Trivy, Snyk) flag CVE-2026-32933 as High Severity regardless of usage pattern. Because 14.0.0 is the final MIT release and version 15+ migrated to Lucky Penny Software commercial licensing, pinning 14.0.0 will fail security compliance gates.
3. **EF Core Sibling Package Fragmentation Across Projects (Medium Severity):** The spine pins `Microsoft.EntityFrameworkCore.SqlServer` to 9.0.22, but the existing csproj files (`ASI.Basecode.Data.csproj` and `ASI.Basecode.WebApp.csproj`) reference `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`, and `Microsoft.EntityFrameworkCore.Tools` at 9.0.6. Sibling EF Core packages must be upgraded in lockstep to 9.0.22 to prevent runtime assembly mismatch errors.
4. **GitHub Actions SQL Server 2025 Service Container Initialization & Health Check (Medium Severity):** AD-16 specifies `mcr.microsoft.com/mssql/server:2025-latest` in CI on `ubuntu-latest`, but lacks container health check specifications. SQL Server containers require 5–15 seconds to boot; without a health check or wait loop using `/opt/mssql-tools18/bin/sqlcmd` with encryption flags (`-No` or `-C`), integration tests fail on connection refused. In addition, the deprecated `SA_PASSWORD` environment variable should be standardized to `MSSQL_SA_PASSWORD`.
5. **Playwright Chromium Offline Availability on Demo Host (Medium Severity):** While `Microsoft.Playwright` 1.64.1 targets `.NETStandard2.0`, the Playwright NuGet package does not bundle browser binaries. On an offline LAN/hotspot demo host (NFR-6), Playwright cannot download Chromium dynamically at runtime. The host deployment instructions must mandate pre-baking or running `playwright.ps1 install chromium` during staging.

---

## 2. Package & Version Verification Matrix

Live checks conducted on 2026-11-04 against NuGet v3, npm registry, and Microsoft Container Registry:

| Technology / Package | Spine Stated Version | Live Checked Version / Status | License | Target Framework | Currency Verdict |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **.NET Runtime** | `9.0.22` (STS) | `9.0.22` (released 2026-10-25; **EOL 2026-11-10**) | MIT | `net9.0` | **Warning:** EOL in 6 days; .NET 10 LTS (`10.0.14`) available |
| **EF Core SqlServer** | `9.0.22` | `9.0.22` (released 2026-10-25) | MIT | `net9.0` / `net8.0` | **Confirmed:** Latest 9.x patch on NuGet |
| **AutoMapper** | `14.0.0` | `14.0.0` (last MIT release; **CVE-2026-32933**) | MIT | `net8.0` / `netstandard2.0` | **Flawed:** Unpatched CVE; 15.0+ is commercial/RPL |
| **Konscious Argon2** | `1.3.1` | `1.3.1` (released 2024-06-19) | MIT | `net8.0`, `net6.0`, `netstandard1.3` | **Confirmed:** Fully compatible with .NET 9 & 10 |
| **Microsoft.Playwright** | `1.64.1` | `1.64.1` (released 2026-10-22) | MIT | `netstandard2.0` | **Confirmed:** Requires browser download staging |
| **@tailwindcss/cli** | `4.3.4` | `4.3.4` (`dist-tags.latest` on npm) | MIT | Node / Standalone CLI | **Confirmed:** Exact npm latest |
| **Serilog** | `4.3.0` | `4.3.0` (repo brownfield; 4.4.0 is latest) | Apache-2.0 | `net8.0` / `netstandard2.0` | **Confirmed:** Adopted from codebase; 4.4.0 available |
| **xunit.v3** | `4.0.2` | `4.0.2` (released 2026) | Apache-2.0 | `net8.0`, `net472` | **Confirmed:** MTP runner included (`xunit.v3.mtp-v2`) |
| **SQL Server Container**| `2025-latest`| `mcr.microsoft.com/mssql/server:2025-latest` | EULA | Linux container (Ubuntu 24.04/22.04)| **Confirmed:** Active tag in MCR |
| **Geralt (evaluated)** | `4.5.0` | `4.5.0` (requires `net10.0`) | MIT | `net10.0` | **Confirmed:** Reason Konscious chosen over Geralt for `net9.0` |

---

## 3. Technology Existence & Semantic Fit

### 3.1. System.TimeProvider (AD-11, AD-7, AD-17)
- **Status:** **VERIFIED & FIT.**
- **Details:** `System.TimeProvider` has been part of the standard Base Class Library since .NET 8 (`System.Runtime.dll`). Its `GetUtcNow()` method returns `DateTimeOffset` in UTC, fulfilling AD-11.
- **Testing:** Pure rule testing in `tests/ASI.Basecode.Services.Tests` can leverage `Microsoft.Extensions.TimeProvider.Testing` (`FakeTimeProvider`) or a lightweight subclass.

### 3.2. sp_getapplock (AD-4)
- **Status:** **VERIFIED & FIT.**
- **Details:** `sys.sp_getapplock` is a built-in SQL Server stored procedure available in all editions including LocalDB and Linux containers.
- **Semantics:** Executing `EXEC sp_getapplock @Resource = 'attendta:last-admin', @LockMode = 'Exclusive', @LockOwner = 'Transaction'` ensures:
  1. Lock duration is tied strictly to the active transaction (`@LockOwner = 'Transaction'`).
  2. The lock is released automatically upon commit or rollback.
  3. Return codes $\ge 0$ denote success (0 = immediate grant, 1 = granted after wait), while negative return codes (-1 = timeout, -3 = deadlock victim) signal rollback.

### 3.3. UPDLOCK, HOLDLOCK, ROWLOCK Semantics (AD-4)
- **Status:** **VERIFIED & FIT.**
- **Details:** SQL Server table hints `WITH (UPDLOCK, HOLDLOCK, ROWLOCK)` in `Lock…Async` repository queries fulfill the serialization contract:
  - `UPDLOCK`: Forces acquisition of update (`U`) locks instead of shared (`S`) locks, preventing simultaneous read-and-escalate conversion deadlocks.
  - `HOLDLOCK`: Holds locks until transaction termination (synonymous with `SERIALIZABLE` isolation).
  - `ROWLOCK`: Directs the optimizer to maintain row-level locking granularity where possible.

### 3.4. Kestrel HTTPS on LAN & Mobile Browser Camera Permissions (AD-16, Structural Seed)
- **Status:** **VERIFIED & FIT.**
- **Critical Browser Constraint:** Under modern mobile web standards (W3C Secure Contexts implemented in Android Chrome and iOS Safari), `navigator.mediaDevices.getUserMedia()` is **disabled by default on insecure origins**.
- **Impact:** An officer accessing the scanner over a local Wi-Fi hotspot via an IP address (e.g. `http://192.168.x.x`) cannot activate the mobile camera unless the connection is served over HTTPS. Thus, AD-16’s requirement for a trusted HTTPS endpoint on Kestrel is a prerequisite for mobile QR scanning.

### 3.5. Data Protection Key Persistence (AD-16, AD-8)
- **Status:** **VERIFIED & FIT.**
- **Details:** Standard ASP.NET Core API:
  ```csharp
  builder.Services.AddDataProtection()
      .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(setupRoot, "keys")))
      .SetApplicationName("AttendTa");
  ```
- **Operational Reality:** Without explicit filesystem persistence, Kestrel stores ephemeral keys in memory or user profile directories. Process restarts on the demo host would invalidate all existing auth cookies and antiforgery tokens. Binding `SetupRoot/keys` to the database backup ensures restore consistency.

### 3.6. Cookie OnValidatePrincipal Immediate Revocation (AD-8)
- **Status:** **VERIFIED & FIT.**
- **Details:** Standard implementation via `CookieAuthenticationEvents`:
  ```csharp
  options.Events = new CookieAuthenticationEvents
  {
      OnValidatePrincipal = async context =>
      {
          var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
          var currentRole = context.Principal?.FindFirstValue(ClaimTypes.Role);
          // Fast SELECT users WHERE user_id = @userId
          if (user == null || !user.is_active || user.role != currentRole)
          {
              context.RejectPrincipal();
              await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
          }
      }
  };
  ```
- **Performance:** A single clustered-index lookup on `users.user_id` takes $<1\,\text{ms}$, fulfilling R31 and R32 immediate revocation without background token blacklists.

---

## 4. Findings & Concrete Proposed Fixes

### Finding 1: .NET 9 End of Support in 6 Days & Local SDK Mismatch
- **Severity:** **HIGH**
- **Location:** Spine Section 5 (`Stack`), AD-16, and Section 7 (`Open Questions`, Q1)
- **Evidence:**
  - Spine line 289: `.NET / ASP.NET Core MVC (net9.0) | 9.0.22 runtime [ADOPTED; see open question 1]`
  - Spine line 375: `1. **.NET 9 support ends on 2026-11-10, before the demo.** Retarget to net10.0 LTS (10.0.14), or keep net9.0 as the ticket constraint requires? Owner: Technical Lead. This blocks the Stack pin.`
  - Microsoft Official Policy (2026-11-04): .NET 9 reaches End of Support on **November 10, 2026** (in 6 days).
  - Workstation state: Developer machines (e.g. running EndeavourOS/Arch/Fedora with `dotnet --info`) have .NET SDK `10.0.113` installed without .NET 9 targeting packs.
- **Problem:** Tagging `9.0.22 runtime` as `[ADOPTED]` contradicts Open Question 1, which notes that the decision blocks the Stack pin. If the team targets `net9.0`, the runtime is unsupported during the student demo, and developers with .NET 10 SDKs face compilation errors unless explicit roll-forward or .NET 9 targeting packs are installed. If the team retargets to `net10.0`, `Geralt 4.5.0` (which targets `net10.0`) becomes available as an alternative to `Konscious.Security.Cryptography.Argon2`.
- **Proposed Fix:**
  In the Stack table (line 289), replace `[ADOPTED; see open question 1]` with `[PROVISIONAL; net10.0 transition recommended]`. Add a resolution note to Open Question 1:
  > *"Recommendation: Retarget all projects to `net10.0` (LTS, supported until November 2028). Both .NET 10 SDK 10.0.113 and EF Core 10.0.14 are GA. If `net9.0` must be retained due to strict ticket constraints, add `rollForward: LatestMajor` to `global.json` and ensure CI installs the `dotnet-sdk-9.0` workload."*

---

### Finding 2: AutoMapper 14.0.0 CVE-2026-32933 Triggers CI Vulnerability Failure
- **Severity:** **HIGH**
- **Location:** Spine Section 5 (`Stack`), AD-18 (line 270), and Section 7 (`Open Questions`, Q2)
- **Evidence:**
  - Spine line 270: `AutoMapper stays pinned at 14.0.0 and maps only flat view models. It never maps entity graphs.`
  - Spine line 376: `2. **AutoMapper 14.0.0 has an unpatched CVE (CVE-2026-32933).** Options: keep it for flat maps only, take the Lucky Penny community license for 16.x, or switch to explicit mapping. Owner: Technical Lead.`
  - Vulnerability Database: GHSA-rvv3-g6hj-g44x / CVE-2026-32933 (CVSS 7.5 High). Patched only in commercial editions `15.1.1` and `16.1.1`.
- **Problem:** While AD-18’s restriction to flat view models eliminates the recursion vector in code, automated vulnerability scanners (GitHub Dependency Graph, `dotnet list package --vulnerable`, Dependabot) inspect package manifests, not call paths. CI builds with vulnerability checking will fail. Furthermore, AutoMapper is only used for flat projections across 8 epics, making a commercial third-party mapper an unnecessary dependency.
- **Proposed Fix:**
  Resolve Open Question 2 in favor of explicit mapping extension methods. In AD-18, replace line 270 with:
  > *"Remove `AutoMapper` entirely. Map domain entities to view models using explicit C# static extension methods (e.g. `user.ToViewModel()`) in `ASI.Basecode.Services/Mapping/`. This eliminates CVE-2026-32933 vulnerability scanner failures and avoids commercial licensing requirements."*

---

### Finding 3: EF Core Sibling Package Version Fragmentation
- **Severity:** **MEDIUM**
- **Location:** Spine Section 5 (`Stack`, line 290) and `ASI.Basecode.Data/ASI.Basecode.Data.csproj`
- **Evidence:**
  - Spine line 290: `| Microsoft.EntityFrameworkCore.SqlServer | 9.0.22 (repo at 9.0.6) |`
  - In `ASI.Basecode.Data.csproj`:
    - `Microsoft.EntityFrameworkCore` Version="9.0.6"
    - `Microsoft.EntityFrameworkCore.Design` Version="9.0.6"
    - `Microsoft.EntityFrameworkCore.Tools` Version="9.0.6"
    - `Microsoft.EntityFrameworkCore.SqlServer` Version="9.0.6"
  - In `ASI.Basecode.WebApp.csproj`:
    - `Microsoft.EntityFrameworkCore.Design` Version="9.0.6"
- **Problem:** The spine lists only `Microsoft.EntityFrameworkCore.SqlServer` as moving to 9.0.22. In Entity Framework Core, mixing package versions (e.g., `SqlServer` at 9.0.22 with `Core` or `Design` at 9.0.6) causes binary incompatibility warnings and runtime `TypeLoadException` or `MissingMethodException` errors.
- **Proposed Fix:**
  Update the Stack table entry (line 290) to encompass all EF Core tooling:
  > `| Microsoft.EntityFrameworkCore.* (SqlServer, Design, Tools) | 9.0.22 (all EF packages upgraded from 9.0.6 in lockstep) |`

---

### Finding 4: GitHub Actions MSSQL 2025 Service Container Readiness & Password Deprecation
- **Severity:** **MEDIUM**
- **Location:** Spine AD-16 (`Environments and operations`, line 248)
- **Evidence:**
  - Spine line 248: `CI | mcr.microsoft.com/mssql/server:2025-latest service container | GitHub Actions ubuntu-latest: build and both test projects on every PR to dev | Required green before merge`
- **Problem:**
  1. SQL Server containers take 5–15 seconds to complete startup. Without a health check or wait loop, test steps run before port 1433 is listening, causing transient test failures in CI.
  2. Microsoft deprecated the `SA_PASSWORD` environment variable in modern container releases in favor of `MSSQL_SA_PASSWORD`.
  3. In SQL Server 2022/2025 container images, `sqlcmd` resides in `/opt/mssql-tools18/bin/sqlcmd` and enforces TLS encryption by default, requiring `-No` (optional encryption) or `-C` (trust certificate) when executing a health query against `localhost`.
- **Proposed Fix:**
  In AD-16, clarify the CI service container definition:
  > *"CI service container specifies `mcr.microsoft.com/mssql/server:2025-latest` with environment variables `ACCEPT_EULA: 'Y'` and `MSSQL_SA_PASSWORD: ${{ secrets.CI_DB_PASSWORD }}`. Include container healthcheck options: `--health-cmd '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $$MSSQL_SA_PASSWORD -No -Q \"SELECT 1\" || exit 1' --health-interval 10s --health-timeout 5s --health-retries 10`."*

---

### Finding 5: Playwright Chromium Offline Availability on Demo Host
- **Severity:** **MEDIUM**
- **Location:** Spine Section 5 (`Stack`, line 294), AD-12, AD-16, and Structural Seed
- **Evidence:**
  - Spine line 294: `| Microsoft.Playwright (Chromium) | 1.64.1 |`
  - Spine line 309: `App --> PW["Playwright Chromium (PDF)"]`
  - AD-16: Demo runs on a local machine connected to LAN or hotspot interface (offline per NFR-6).
- **Problem:** Installing `Microsoft.Playwright` via NuGet supplies only the .NET driver binaries. The Chromium headless browser binary (~150 MB) is downloaded separately. If the demo machine is operated without internet access (hotspot environment), Playwright's first PDF invocation will crash with `PlaywrightException: Executable doesn't exist at ... Run 'playwright.ps1 install'`.
- **Proposed Fix:**
  Add a specific deployment prerequisite to AD-16:
  > *"Demo host setup must pre-install Playwright Chromium binaries while internet-connected by executing `pwsh ./bin/Release/net9.0/playwright.ps1 install chromium` (or embedding the browser path into `SetupRoot/playwright`). Rendering must verify browser existence at startup."*

---

### Finding 6: Inaccurate Project Location for Hangfire.Core in AD-18
- **Severity:** **LOW**
- **Location:** Spine AD-18 (line 268)
- **Evidence:**
  - Spine line 268: `Remove Hangfire.Core, EFCore.BulkExtensions, System.Drawing.Common and ZXing.Net from Data.`
  - `ASI.Basecode.Data/ASI.Basecode.Data.csproj`: Contains `EFCore.BulkExtensions`, `System.Drawing.Common`, `ZXing.Net`. Does **not** contain `Hangfire.Core`.
  - `ASI.Basecode.Services/ASI.Basecode.Services.csproj`: Contains `<PackageReference Include="Hangfire.Core" Version="1.8.20" />` and `<PackageReference Include="EFCore.BulkExtensions" Version="9.0.1" />`.
- **Problem:** `Hangfire.Core` is referenced in the Services layer project, not Data. Developers following AD-18 will fail to find `Hangfire.Core` in `ASI.Basecode.Data.csproj`.
- **Proposed Fix:**
  Update AD-18 line 268 to state:
  > *"Remove `EFCore.BulkExtensions`, `System.Drawing.Common` and `ZXing.Net` from `Data`, and remove `Hangfire.Core` and `EFCore.BulkExtensions` from `Services`."*

---

## 5. Reviewer Sign-Off

The architecture spine's technology stack is grounded in real, verifiable packages and accurate protocol behaviors. Once the .NET 9 support timeline (Finding 1), AutoMapper CVE gate (Finding 2), EF Core sibling package alignment (Finding 3), and CI/demo operational readiness (Findings 4 & 5) are finalized, the spine provides an exceptionally solid foundation for parallel epic implementation.
