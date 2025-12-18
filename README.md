# Web API Suite

A **.NET 10 Web API solution** demonstrating secure authentication, containerization, and contributor‑friendly workflows. The solution includes an Issuer service for token generation, an API service protected by those tokens, and a sample Client for integration testing.

---

## ✨ Highlights

- **Issuer service** – Issues JWT tokens with configurable lifetimes and signing keys.
- **API service** – Exposes endpoints secured via Issuer‑issued tokens.
- **Client app** – Demonstrates token acquisition and API consumption.
- **Certificate automation** – PowerShell and POSIX‑compliant shell routines for HTTPS and signing.
- **Security key generation** – Automated scripts to create signing keys for token issuance.
- **Docker support** – Multi‑stage builds with guardrails for onboarding and reproducibility.
- **Cross‑IDE profiles** – Debugging and launch settings for Visual Studio and VS Code.

---

## 📂 Structure

- **src/** – Application code for Issuer, Service, and Client projects
- **scripts/** – Automation routines for certs, keys, and data resets
- **data/** – JSON seed files for local development
- **.vscode/** – Debug/launch profiles for VS Code
- **LICENSE** – MIT license
- **README.md** – Documentation entry point

---

## ⚙️ Prerequisites

- .NET 10 SDK
- OpenSSL available in PATH
- Docker Desktop (optional, for container builds)
- Visual Studio 2022 or VS Code with C# extension

---

## 🔧 Usage

- **Generate HTTPS certificates** for local development:

  ```powershell
  ./scripts/generate-certs.ps1 -Mode API
  ./scripts/generate-certs.ps1 -Mode ISSUER
  ```

- **Generate signing keys** for token issuance:

  ```powershell
  ./scripts/generate-keys.ps1 -Mode API
  ./scripts/generate-keys.ps1 -Mode ISSUER
  ```

- **Reset seeded data**:
  ```powershell
  ./scripts/reset-data.ps1 -Mode API
  ./scripts/reset-data.ps1 -Mode ISSUER
  ```

---

## 🔐 Trust Flow Diagram

```text
   +---------+        issues JWT        +---------+
   |  Issuer | -----------------------> |  Client |
   +---------+                          +---------+
        |                                    |
        | validates token                    | attaches token
        v                                    v
   +---------+ <------------------------- +---------+
   |   API   |        secured calls        |  Client |
   +---------+                             +---------+
```

---

## 📈 Sequence Diagram (Token Request & Validation)

```text
Client            Issuer                API
  |                 |                   |
  |--- Request ---->|                   |
  |   token         |                   |
  |                 |                   |
  |<-- JWT ---------|                   |
  |                 |                   |
  |--- Call API ----------------------->|
  |   with JWT                          |
  |                 |                   |
  |                 |<-- Validate JWT --|
  |                 |                   |
  |                 |--- Public Key --->|
  |                 |                   |
  |<-- Response ------------------------|
```

**Step‑by‑step:**

1. Client requests a token from Issuer.
2. Issuer generates JWT using private signing key.
3. Client calls API with JWT attached in the Authorization header.
4. API validates JWT against Issuer’s public key.
5. API responds with secured data if validation succeeds.

---

## 🧪 Testing & Development

- Default test accounts and endpoints are configured in `appsettings.json`.
- Certificates and keys are copied into each project’s `https/` or `data/` folder automatically.
- Token issuance and API calls can be verified with standard HTTP clients such as `curl` or Postman.

---

## 📌 Notes

- Scripts are designed to be **atomic and rollback‑safe**, ensuring clean onboarding.
- Contributions should maintain repo hygiene: keep `LICENSE`, `SECURITY.md`, and `README.md` aligned.
