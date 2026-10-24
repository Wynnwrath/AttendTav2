# AttendTa — Event & Attendance Management System
## _Group 7 (Master Data Management & Officer Console)_

ASP.NET Core MVC web application for student organization event registration, attendance tracking, member directories, and facility management.

---

## 🚀 Quick Setup & Run (For Any PC / Cloners)

### Step 1: Clone and Enter the Directory
Open PowerShell or your preferred terminal:
```powershell
git clone https://github.com/Wynnwrath/AttendTav2.git
cd AttendTav2
```

---

### Step 2: Initialize Database (SQL Server LocalDB)
The application connects to `(localdb)\MSSQLLocalDB` and uses database `AsiBasecodeDB`. Run the provided setup script once:
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "setup-db.sql"
```
> **Alternative (SSMS / Azure Data Studio):** If you prefer a GUI, simply open `setup-db.sql` in SQL Server Management Studio or Azure Data Studio and click **Execute**.

This creates the database, schema (`members`, `venues`, `event_types`, `Users`), and seeds initial demo data and accounts.

---

### Step 3: Run the Application

#### Option A: Running from Visual Studio 2022 / 2025 (Recommended for UI)
1. Double-click `ASI.Basecode.sln` to open in Visual Studio.
2. In **Solution Explorer**, right-click **`ASI.Basecode.WebApp`** ➔ **Set as Startup Project**.
3. Press **`Ctrl + F5`** (or click the Green Play button ▶).

#### Option B: Running from Terminal / VS Code
Because this project targets .NET 9, depending on what SDK version is installed on your machine:

- **Standard Run (.NET 9 installed as default):**
  ```powershell
  dotnet run --project "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"
  ```
- **If your machine has .NET 10 installed (Roll-Forward enabled):**
  ```powershell
  $env:DOTNET_ROLL_FORWARD = "LatestMajor"
  dotnet run --project "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"
  ```
- **If your system `PATH` points to another drive (e.g. `D:\`) while .NET is in `C:\`:**
  Specify your exact dotnet executable path (e.g.):
  ```powershell
  & "C:\Program Files\dotnet\dotnet.exe" run --project "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"
  ```

Once running, navigate to: **`http://localhost:5000/Account/Login`**

---

## 🔑 Demo Login Accounts

On the sign-in page, you can use the **1-click quick-fill buttons** or enter these credentials:

| Role | User ID / Student Number | Password |
|---|---|---|
| **Administrator** | `admin` | `Password123!` |
| **Officer** | `officer` | `Password123!` |
| **Student Member** | `2023-10482` | `Password123!` |

---

## 🏛️ Clean Code Architecture

This project strictly follows an N-Layer Clean Architecture with separated responsibilities:

- **`ASI.Basecode.Data`**: EF Core 9 `AsiBasecodeDBContext`, entity models, and repository implementations (`BaseRepository`, `UnitOfWork`). No business logic.
- **`ASI.Basecode.Services`**: Business services (`MemberService`, `VenueService`, `EventTypeService`), business exceptions, and DTO viewmodels (`MemberViewModel`, etc.).
- **`ASI.Basecode.WebApp`**: Controllers (`MemberController`, etc.), Razor views (`.cshtml`), layout shells, and static assets (`wwwroot/css/tokens.css`, `components.css`).
- **`ASI.Basecode.Resources`**: Centralized string constants, localized error messages, and enums.
