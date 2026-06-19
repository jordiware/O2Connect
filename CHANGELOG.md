# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project follows a pre-release versioning scheme.

---

## [alpha-1] - 2026-06-19

### Added

#### Clients Management

- `GET /management/clients` (lists registered clients)
- `POST /management/clients/search` (search clients using filter)
- `GET /management/clients/{clientID}` (gets specified client)
- `PATCH /management/clients/{clientID}/display_name` (updates client display name)
- `PATCH /management/clients/{clientID}/image_url` (updates client image URL)
- `PATCH /management/clients/{clientID}/redirect_uris` (updates client redirect URIs)
- `PATCH /management/clients/{clientID}/scopes` (updates client allowed scopes)
- `PATCH /management/clients/{clientID}/status` (updates client status)

#### Users Management

- `GET /management/users` (lists registered users)
- `POST /management/users/search` (search users using filter)
- `GET /management/users/{userID}` (gets specified user)
- `PATCH /management/users/{userID}/display_name` (updates user display name)
- `PATCH /management/users/{userID}/image_url` (updates user image URL)
- `PATCH /management/users/{userID}/role` (updates user role)
- `PATCH /management/users/{userID}/status` (updates user status)

#### Profile Management

- `GET /management/me` (gets current user profile data)
- `GET /management/me/consents` (lists current user consented clients)
- `PATCH /management/me/update_display_name` (updates current user display name)
- `PATCH /management/me/update_image_url` (updates current user image URL)
- `PATCH /management/me/update_password` (updates current user password)
- `PATCH /management/me/update_username` (updates current user username)
- `PATCH /management/me/revoke_consent` (revokes previously consented client)

### Changed

- Users can now take **one single role**
- Management exceptions in form of **IEEE RFC 7807** Problem/Details response

### Security

- Extended scopes for **query users and clients**

---

## [alpha-0.1] - 2026-06-12

### Added

#### Account Management

- `POST /account/register`
- `PATCH /account/me` (user profile update)
- `POST /account/password` (password change)

### Changed

- Project status updated: all planned OAuth2/OIDC and account lifecycle endpoints are now fully implemented

### Security

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
