---
id: 1
type: story
title: "Provisioned administrator logs in by email via Argon2id into authenticated session with active role checks"
parent: epic-platform-access
covers: [R3, R4]
after: [2, 10]
hitl: true
risk: high
tracker_id: "11"
remote: "https://github.com/rainyDayssss/AttendTa/issues/11"
tracker_status: done
assignee: "FivePesos"
---

# Provisioned administrator logs in by email via Argon2id into authenticated session with active role checks

## Description

Retires the login stub, UserService.AuthenticateUser and AddUser, the public Register, the AES PasswordManager and SessionManager, and the JwtBearer, CustomJwtDataFormat, /api/token, TokenProviderOptions, TokenValidationParametersFactory and TokenAuthentication configuration stack, and removes the Microsoft.AspNetCore.Authentication.JwtBearer package reference from ASI.Basecode.WebApp.csproj. Replaces them with cookie authentication: claims NameIdentifier = users.user_id and Role = role code; non-persistent, 8-hour, non-sliding; Data Protection keys under Common:SetupRoot/keys; OnValidatePrincipal reloads the users row on every request and rejects an inactive or role-changed session; generic login failure; no lockout; ICurrentActor (WebApp adapter) with the Management and Administrator policies; antiforgery on login and logout. Adds IPasswordHasher writing PHC $argon2id$ strings with a fresh 16-byte salt, verified in fixed time, benchmarked on the host (Konscious.Security.Cryptography.Argon2 1.3.1 assumed). A WebApp command-line mode hashes the supplied password through IPasswordHasher (AD-15) for the setup script, since AD-1 allows no new project. Rewrites the R35 setup script database/seed-local-admin.sql to take a supplied name, email and pre-computed password hash, and write one actor_type system activity_logs row. Removes the committed TokenAuthentication:SecretKey; the connection string comes from environment variables or user-secrets.

## Requirements

Verify: An operator benchmarks Argon2id on the host. On a freshly built AttendTaMVP_Test, the setup script provisions one active administrator with a PHC hash and a system audit row; that administrator logs in by email, and an authenticated request to a Management-policy route returns 200 while the same request made anonymously is redirected to sign-in; deactivating or demoting the row in the database rejects the next request; logout succeeds only as POST with an antiforgery token; public Register is gone; and the test suites pass in GitHub Actions.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-platform-access/epic-platform-access.md
