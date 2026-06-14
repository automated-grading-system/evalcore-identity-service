# EvalCore Identity Service

ASP.NET Core Identity Service for the Automated Grading System. It owns account registration, login, JWT access token issuance, current-user profile, and admin user management.

## Architecture

Layer flow is intentionally strict:

`Controller -> Application Service -> Repository Interface -> Repository Implementation -> IdentityDbContext -> PostgreSQL`

Projects:

- `src/Identity.Api`: controllers, auth, Swagger, CORS, middleware, health endpoint.
- `src/Identity.Application`: DTOs, response envelopes, service interfaces, application services, validation results.
- `src/Identity.Domain`: `Account` entity and role constants.
- `src/Identity.Infrastructure`: EF Core DbContext, migrations, repositories, JWT provider, password hashing, development seeder.
- `tests/Identity.Tests`: unit tests for role validation, password hashing, and auth service behavior.

## Requirements

- .NET 8 SDK
- PostgreSQL from the separate `prn232-ops` repository
- Optional: Docker

Expected local PostgreSQL connection:

```text
Host=localhost;Port=5432;Database=ags;Username=ags;Password=ags_password
```

## Configuration

Copy `.env.example` to `.env` for local development or export equivalent environment variables.

Required variables:

```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:8081
DATABASE_URL=Host=localhost;Port=5432;Database=ags;Username=ags;Password=ags_password
JWT_SECRET=change-me-local-secret-for-dev-only-32-bytes
JWT_ISSUER=ags
JWT_AUDIENCE=ags-api
JWT_EXPIRES_MINUTES=1440
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5173
```

`JWT_SECRET` must be provided by environment or `.env`; it is not hardcoded in application settings.

## Build And Test

```bash
dotnet restore
dotnet build
dotnet test
```

## Database

EF Core uses PostgreSQL schema `identity` and table `identity.accounts`.

Clean local setup from the repository root:

```bash
cp .env.example .env
dotnet tool restore
dotnet ef database update --project src/Identity.Infrastructure --startup-project src/Identity.Api
```

The EF design-time factory loads `.env` automatically and only needs database configuration. It does not require `JWT_SECRET` for migration commands.

In `Development`, the API automatically applies migrations and seeds demo accounts on startup. PostgreSQL must already be running from `prn232-ops`.

## Run Locally

```bash
cp .env.example .env
dotnet run --project src/Identity.Api
```

The API loads `.env` before validating JWT configuration, including `ASPNETCORE_URLS`.

Health check:

```bash
curl http://localhost:8081/health
```

Swagger is enabled in Development at:

```text
http://localhost:8081/swagger
```

## Demo Accounts

All demo passwords are `Password123!`.

| Role | Email |
| --- | --- |
| admin | `admin@ags.local` |
| lecturer | `lecturer@ags.local` |
| student | `student@ags.local` |

## API Summary

| Method | Path | Auth |
| --- | --- | --- |
| `POST` | `/api/auth/register` | public |
| `POST` | `/api/auth/login` | public |
| `GET` | `/api/users/me` | student, lecturer, admin |
| `GET` | `/api/admin/users?page=1&pageSize=20&role=student&keyword=nam` | admin |
| `PATCH` | `/api/admin/users/{userId}/lock` | admin |
| `PATCH` | `/api/admin/users/{userId}/unlock` | admin |
| `GET` | `/health` | public |

Normal API responses use:

```json
{
  "success": true,
  "data": {}
}
```

Errors use:

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": {}
  }
}
```

## Example Requests

Register:

```bash
curl -X POST http://localhost:8081/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Student Demo","email":"student2@ags.local","password":"Password123!","role":"student"}'
```

Login:

```bash
curl -X POST http://localhost:8081/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"student@ags.local","password":"Password123!"}'
```

Current user:

```bash
curl http://localhost:8081/api/users/me \
  -H "Authorization: Bearer <access_token>"
```

## Docker

Build:

```bash
docker build -t evalcore-identity-service:local .
```

Run:

```bash
docker run --rm -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e DATABASE_URL="Host=host.docker.internal;Port=5432;Database=ags;Username=ags;Password=ags_password" \
  -e JWT_SECRET="<at-least-32-bytes-secret>" \
  -e JWT_ISSUER=ags \
  -e JWT_AUDIENCE=ags-api \
  -e JWT_EXPIRES_MINUTES=1440 \
  evalcore-identity-service:local
```

Published image names:

- DockerHub: `dorrissdang/evalcore-identity-service`
- GHCR: `ghcr.io/automated-grading-system/evalcore-identity-service`

## CI/CD

Workflows:

- `Identity PR Check`: restore, format check, build, test, Docker build.
- `Identity Main CI`: restore, format check, build, test, Docker build on `main`.
- `Identity Backend CD`: publishes multi-arch Docker images to DockerHub and GHCR.
- `ai-code-review.yml`: Vietnamese AI review for pull requests when `OPENCODE_API_KEY` is configured.
- `ai-docs.yml`: Vietnamese PR description update when `OPENCODE_API_KEY` is configured.

Required publishing secrets:

- `DOCKERHUB_USERNAME`
- `DOCKERHUB_TOKEN`

GHCR publishing uses `GITHUB_TOKEN`.
