# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project follows a pre-release versioning scheme.

---

## [alpha-0.1] - 2026-06-12

### Added

#### Account Management

- `POST /account/register`
- `PATCH /account/me` (user profile update)
- `POST /account/password` (password change)

#### Changed

- Project status updated: all planned OAuth2/OIDC and account lifecycle endpoints are now fully implemented

#### Security

- All OAuth2/OIDC endpoints are now authorization-secured (role/scope-based access control applied)

---

## [alpha-0] - 2026-06-11

### Added

#### Core OAuth2 / OIDC Endpoints

- `POST /connect/token` (JWT access tokens, RSA signing, refresh token rotation)
- `POST /connect/revocation`
- `GET /.well-known/oauth-authorization-server`
- `GET /.well-known/jwks.json`

#### OIDC Session & Logout

- `POST /connect/logout` (end_session_endpoint)

#### Device Authorization Flow

- `POST /connect/device_authorize`
- `POST /connect/device`
- `GET /connect/device_status`

#### Advanced OAuth Extensions

- `POST /connect/par` (Pushed Authorization Requests)
- `POST /connect/register` (Dynamic Client Registration)

### Security

- RSA-based JWT signing with `kid` support
- Refresh token rotation with reuse detection
- Session-based token revocation

### Architecture

- API-first OAuth2/OIDC provider design (no browser-based SSO)
- Separation of concerns:
  - `AccountController` for authentication/session management
  - `/connect/*` endpoints for protocol operations
- Centralized exception handling middleware with spec-compliant OAuth error responses

### DTO & Contract Design

- Strict protocol-aligned DTOs
- `snake_case` JSON naming via explicit attributes
- Separation between request binding models and response contracts

---

## Versioning Notes

- `alpha-*`: Early development milestones, breaking changes expected
- Future versions will follow a similar structured format
