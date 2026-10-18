# Authentication contracts (COMP-83)

Registration continues to use email as the Identity username. No separate username
field was introduced. `POST /api/auth/register` requires `email`, `password`,
`confirmPassword`, `firstName`, `lastName`, `phoneNumber`.

- Email: non-empty, valid address, at most 256 characters, unique after Identity normalization.
- Password: 8–128 characters, uppercase, lowercase, digit and special character.
- Confirmation: required and equal to the password.
- First/last name: non-whitespace, at most 100 characters each.
- Phone: non-whitespace, at most 30 characters (no locale-specific format imposed).

Angular mirrors these rules. The server returns HTTP 400 with the shared
`{ message, errors: [{ propertyName, errorMessage }] }` validation contract.
Identity remains responsible for password hashing and normalized username uniqueness.
Old clients must send `confirmPassword`; the Pages frontend and opt-in smoke client
are updated in the same change.

Login still accepts `{ email, password }`. Refresh still accepts a JSON string
containing the refresh token and returns the top-level login response. Logout
accepts the existing `{ token }` JSON shape through a dedicated request DTO.
Unknown, expired, revoked refresh tokens and inactive accounts return HTTP 401.
An absent/empty token is a validation error (400).

Logout and refresh rotation revoke the old token in place, preserving `IsRevoked`
and nullable UTC `RevokedAt`. Repeated logout preserves the original revocation
timestamp. The additive `TrackRefreshTokenRevocation` migration adds only that nullable
column; it does not delete existing token data. API startup applies migrations as before.

Request cancellation is passed from controllers through MediatR and refresh-token
service EF calls. ASP.NET Identity's UserManager methods have no public cancellation
parameter; handlers and the async uniqueness validator check cancellation around
those operations. Token generation and existing one-hour/seven-day lifetimes remain unchanged.
