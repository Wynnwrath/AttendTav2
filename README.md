# C# BaseCode
## _ASI Bridge/JumpStart Program BaseCode_

This is the base code that will be used during the bridge and jumpstart program.
```
```
## Quick Setup for New Clones

### 1. Database Setup (SQL Server LocalDB)
The application expects `AsiBasecodeDB` on `(localdb)\MSSQLLocalDB`. Run the provided setup script once in PowerShell:
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "setup-db.sql"
```
*(Or execute `setup-db.sql` in SQL Server Management Studio / Azure Data Studio).*

### 2. Build & Run
If using modern .NET 10 SDK with this .NET 9 project, enable major version roll-forward:
```powershell
$env:DOTNET_ROLL_FORWARD = "LatestMajor"
dotnet build ASI.Basecode.sln
dotnet run --project "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"
```
Then navigate to `http://localhost:5000/Account/Login` in your browser. Any credentials will log in to the demo officer console.

## Visual Studio 2022 / 2025 Setup
1. Open `ASI.Basecode.sln`.
2. Ensure connection string in `ASI.Basecode.WebApp\appsettings.json` matches your local server:
   `Server=(localdb)\MSSQLLocalDB; Database=AsiBasecodeDB; Trusted_Connection=True; MultipleActiveResultSets=true; TrustServerCertificate=True`
3. Set `ASI.Basecode.WebApp` as the Startup Project.
4. Press `F5` or `Ctrl+F5` to run.

## Code Structure

- `ASI.Basecode.Data`
    - This project contains the repositories and other logics that involves with database.
    - Make sure that the repository is for db processing only.
    - If it involves additional logic, move the logic into the service file.
- `ASI.Basecode.Resources`
    - This project is dedicated to storing messages, labels, and other resources used by the website.
    - It serves as a centralized location for managing static content that is displayed to users.
    - Store all messages, labels, translations, and any non-code assets required for the site in this project.
- `ASI.Basecode.Services`
    - This project contains the services and other processing logics before connecting to the repository.
    - This can also contains classes that can be use by the WebApp project with/without db processing.
- `ASI.Basecode.WebApp`
    - This project contains the main codes especially the `Controllers` and `Views`.
    - This is where you put the connection logic to the APIs.
    - Make sure that the controllers are clean and no other logic should involve.
    - If it involves additional logic, move the logic into the service file.
```
```
