# Security Policy

## Supported Versions

We actively maintain the latest version of the WebApi solution. Older versions may not receive security updates.

| Version | Supported |
| ------- | --------- |
| Latest  | ✅        |
| Older   | ❌        |

---

## Reporting a Vulnerability

If you discover a security issue, please **do not open a public issue**.  
Instead, report it privately:

- Email: wu_yuqing@hotmail.com
- Or use GitHub’s [private vulnerability reporting](https://docs.github.com/en/code-security/security-advisories/repository-security-advisories/creating-a-security-advisory)

We will acknowledge receipt within 48 hours and provide updates until the issue is resolved.

---

## Security Practices

- **Private key isolation**  
  Signing keys are generated locally and never committed to source control.

- **Certificate automation**  
  Scripts in `scripts/` generate HTTPS certificates and signing keys. Only trusted artifacts are handled, not raw private keys.

- **Token signing**  
  JWTs are signed using strong algorithms (RS256/ES256). Tokens are scoped (`api.read`, `api.write`) to enforce least privilege.

- **HTTPS enforcement**  
  All services require TLS with SAN‑enabled certificates. Self‑signed certs are only used for local development.

- **Secret management**  
  Sensitive values (connection strings, signing keys) must be injected via environment variables or secret managers (Azure Key Vault, AWS Secrets Manager). Never store secrets in `appsettings.json`.

- **Rotation**  
  Certificates and signing keys should be rotated regularly. Scripts support regeneration for clean resets.

- **Audit logging**  
  Authentication events are logged for visibility without exposing sensitive data.
