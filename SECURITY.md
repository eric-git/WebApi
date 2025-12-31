# Security Policy

## Supported Versions

We actively maintain the latest version of the WebApi solution. Older versions may not receive security updates.

| Version | Supported |
| ------- | --------- |
| Latest  | ✅        |
| Older   | ❌        |

---

## Reporting a Vulnerability

If you discover a security issue, **do not open a public issue**.  
Please report it privately via one of the following channels:

- Email: **wu_yuqing@hotmail.com**
- GitHub’s **private vulnerability reporting** feature:  
  https://docs.github.com/en/code-security/security-advisories/repository-security-advisories/creating-a-security-advisory

We acknowledge reports within **48 hours** and provide updates until the issue is resolved.

---

## Security Practices

- **Private key isolation**  
  Signing keys are generated locally and never committed to source control.

- **Certificate automation**  
  Scripts in `scripts/` generate HTTPS certificates and signing keys. Only trusted artifacts are handled—never raw private keys.

- **Token signing**  
  JWTs use strong algorithms (RS256/ES256). Tokens are scoped (`api.read`, `api.write`) to enforce least‑privilege access.

- **HTTPS enforcement**  
  All services require TLS with SAN‑enabled certificates. Self‑signed certificates are limited to local development.

- **Secret management**  
  Sensitive values (connection strings, signing keys) must be provided via environment variables or secret managers (Azure Key Vault, AWS Secrets Manager).  
  **Never** store secrets in `appsettings.json`.

- **Rotation**  
  Certificates and signing keys should be rotated regularly. Provided scripts support clean regeneration.

- **Audit logging**  
  Authentication events are logged for visibility while avoiding exposure of sensitive data.
