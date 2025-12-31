# Security Gateway Module

The Security Gateway centralizes CAPTCHA verification and OTP delivery behind stable, versioned APIs. It is configuration-driven, provider-agnostic, and stores challenges in the database so multiple app instances can participate safely.

## Configuration

App settings are bound to `SecurityGatewayOptions`:

```json
{
  "SecurityGateway": {
    "Captcha": {
      "Provider": "GoogleRecaptcha",
      "SecretKey": "<recaptcha secret>",
      "MinimumScore": 0.5,
      "Enabled": true
    },
    "Otp": {
      "DefaultLength": 6,
      "DefaultExpirySeconds": 300,
      "CooldownSeconds": 30,
      "MaxAttempts": 5,
      "MaxSendsPerHour": 5,
      "MaxSendsPerDay": 20
    },
    "Messaging": {
      "DefaultProviderName": "Mock",
      "Providers": {
        "Mock": {},
        "Twilio": {
          "AccountSid": "",
          "AuthToken": "",
          "FromSms": "",
          "FromWhatsapp": "",
          "FromEmail": ""
        },
        "Kaleyra": {
          "ApiKey": "",
          "SenderId": ""
        },
        "Nexg": {
          "ApiKey": "",
          "SenderId": ""
        }
      }
    }
  }
}
```

The `Messaging:DefaultProviderName` selects which provider the router will use. Providers can be swapped without touching controllers or business logic.

## Endpoints

All endpoints are versioned under `/api/security/v1/*`. Enums are serialized as strings.

### POST `/api/security/v1/captcha/verify`

Request body:

```json
{
  "token": "<recaptcha-token>",
  "action": "login",
  "userId": "user-123"
}
```

Response:

```json
{
  "success": true,
  "score": 0.9,
  "reason": null,
  "correlationId": "c0e6f3f6939b4cfda8c8e93c20beab5d"
}
```

Logs are persisted to `CaptchaVerificationLogs` with hashed tokens and provider responses.

### POST `/api/security/v1/otp/send`

Request body:

```json
{
  "userId": "user-123",
  "destination": "user@example.com",
  "channel": "Email",
  "purpose": "Login2FA"
}
```

Response:

```json
{
  "correlationId": "8d4f6c836b1e4b509b8d8b4b9014286c",
  "expiresUtc": "2024-09-05T12:00:00Z",
  "cooldownSeconds": 30,
  "status": "Pending"
}
```

Rate limits are enforced per destination/channel/purpose for hourly and daily send counts, plus cooldown between sends. OTP codes are generated using a cryptographically secure RNG, hashed with PBKDF2 and salted before storage. All sends and resend attempts are logged to `OtpAudits`.

### POST `/api/security/v1/otp/verify`

Request body:

```json
{
  "correlationId": "8d4f6c836b1e4b509b8d8b4b9014286c",
  "destination": "user@example.com",
  "channel": "Email",
  "purpose": "Login2FA",
  "code": "123456"
}
```

Response on success:

```json
{
  "success": true,
  "status": "Verified",
  "verifiedUtc": "2024-09-05T11:57:00Z",
  "failureReason": null
}
```

Response when invalid/expired/locked returns `success: false`, a `status` describing the state, and a `failureReason`. Verification attempts are counted against `MaxAttempts` and logged to `OtpAudits`. Challenges are locked when attempts exceed the configured limit or when expired.

### POST `/api/security/v1/otp/resend`

Request body:

```json
{
  "correlationId": "8d4f6c836b1e4b509b8d8b4b9014286c"
}
```

Response mirrors `/otp/send` and re-issues a fresh code, respecting cooldowns and send limits.

## Data model

- `OtpChallenge` persists the salted OTP hash, status, and counters for attempts and sends.
- `OtpAudit` captures every send/resend/verify action (success or failure), including provider name and remote IP.
- `CaptchaVerificationLog` stores hashed tokens, scores, provider payloads, and request metadata.

All tables are part of `CrmDbContext` with migrations under `BOSGlobal.Crm.Infrastructure/Migrations/Security`.

## Provider routing

- `IMessagingProvider` abstracts messaging capabilities.
- `ProviderRouter` selects the provider by name using configuration (default: `Mock`).
- `MockMessagingProvider` logs payloads without external calls. Stub classes for Twilio, Kaleyra, and Nexg are registered so production providers can be plugged in later without API changes.

## Operational notes

- Controllers return `ProblemDetails` for validation, rate limit, or state errors (e.g., cooldown, locked challenges).
- Captcha verification honors the configured minimum score and logs all outcomes.
- OTP generation uses PBKDF2 hashing and cryptographically secure random numbers; codes are never stored in plaintext.
- Apply migrations with `dotnet ef database update` using the `CrmDbContext` to create the new tables before deploying.
