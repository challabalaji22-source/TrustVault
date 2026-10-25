# TrustVault — Threat Model

## 1. Project Overview

TrustVault is a document management application built using Angular, ASP.NET Core, and PostgreSQL. It supports user authentication, document upload and download, document sharing, and permission revocation.

## 2. Assets to Protect

- User credentials and authentication tokens
- Uploaded documents and their contents
- User identities and account information
- Document-sharing permissions
- Audit history and security-related records
- Database credentials and application configuration

## 3. Potential Threats and Mitigations

| Threat | Potential Impact | Mitigation |
|---|---|---|
| Unauthorized access | An attacker accesses protected APIs or documents. | Require authentication and enforce authorization on protected endpoints. |
| Broken access control | A user accesses another user's document. | Verify document ownership or explicit sharing permissions on every relevant request. |
| Token theft | An attacker uses a stolen authentication token. | Protect tokens, use HTTPS in deployment, and implement appropriate expiration and revocation controls. |
| Insecure file upload | Malicious or unexpected files are uploaded. | Validate file types, sizes, filenames, and storage paths. Consider malware scanning where appropriate. |
| Unauthorized document sharing | A document is shared with an unintended user. | Restrict sharing to authorized users and validate permission changes. |
| Access after revocation | A previously authorized user continues accessing a document. | Recheck current permissions during download and other protected operations. |
| SQL injection | An attacker manipulates database queries. | Use parameterized queries and safe ORM practices. |
| Cross-site scripting (XSS) | Malicious scripts execute in a user's browser. | Apply output encoding, avoid unsafe DOM operations, and validate untrusted content. |
| Sensitive information exposure | Secrets or private documents are disclosed. | Keep secrets out of source control, restrict access, and avoid logging credentials or tokens. |
| Insufficient audit logging | Security incidents are difficult to investigate. | Record appropriate authentication, sharing, permission, and access events without logging sensitive secrets. |
| Denial of service | Excessive requests or large uploads affect availability. | Apply suitable request limits, upload size limits, timeouts, and rate limiting. |

## 4. Trust Boundaries

1. **Browser to Angular frontend:** Treat user input and browser-controlled data as untrusted.
2. **Frontend to ASP.NET Core API:** Treat all incoming requests as untrusted; validate input and enforce server-side authorization.
3. **API to PostgreSQL:** Protect database access using least-privilege credentials and safe queries.
4. **API to document storage:** Restrict filesystem or object-storage access and prevent path traversal.
5. **User-to-user sharing:** Verify the recipient's identity and current permission before allowing document access.

## 5. Security Verification

The following functional checks have been performed during development:

- Unauthenticated API requests were rejected with HTTP 401.
- Document upload and download were tested.
- Sharing with a registered test account was tested.
- Revoking a viewer's permission was tested.
- After revocation, the document was no longer visible in the viewer's shared list.
- A direct download attempt after revocation returned HTTP 404.
- Angular and ASP.NET Core builds completed successfully.

These checks provide initial evidence of expected behavior; they do not constitute a complete security audit or penetration test.

## 6. Remaining Security Work

Before production deployment:

- Verify authorization on every document-related endpoint.
- Review password handling, token expiration, storage, and revocation.
- Enforce HTTPS and secure production configuration.
- Validate uploads and restrict access to stored files.
- Review CORS, CSRF protections where applicable, and security headers.
- Ensure secrets are not committed to GitHub.
- Review audit logging and retention.
- Test common web vulnerabilities and dependency vulnerabilities.
- Configure backups, monitoring, and recovery procedures.

## 7. Conclusion

TrustVault includes document management and access-control functionality. Its security depends on enforcing authorization on the server, protecting credentials and files, and validating permissions on every sensitive operation. Further security testing is required before production use.
