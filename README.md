# School Admission

A school admission system: admins create classes with a seat limit, guardians apply for a seat with a child's birth registration number, the seat is held for a limited time, and the guardian pays to confirm or the seat is released.

## Requirements

Docker with Docker Compose. Nothing else is needed to run the app or the tests.

## Run

```
docker compose up --build
```

- Frontend: http://localhost:3000
- Backend API: http://localhost:8080
- PostgreSQL: 127.0.0.1:5433 (user `postgres`, password `postgres`)

The database schema is created on backend start. Data is kept in a Docker volume, so it survives `docker compose down` and `up`.

## Configuration

| Variable | Default | Meaning |
|---|---|---|
| `HOLD_DURATION_SECONDS` | `600` | How long a seat is held before payment |
| `API_BASE_URL` | `http://localhost:8080` | Backend URL the frontend calls, read when the frontend container starts |
| `DB_HOST_PORT` | `5433` | Host port for PostgreSQL |

Example with 30 second holds:

```
HOLD_DURATION_SECONDS=30 docker compose up -d
```

## Tests

With Docker only (uses the compose database):

```
docker compose up -d db
docker compose --profile test run --rm backend-tests
```

With the .NET 10 SDK installed:

```
docker compose up -d db
cd backend
dotnet test
```

The tests use a separate database, `school_admission_test`, which they recreate on each run. Set `TEST_DB_CONNECTION` to point them at another server. They include a test that sends 200 parallel applications for a class with 10 seats and checks that exactly 10 succeed.

## Project layout

```
backend/
  src/SchoolAdmission.Api/     ASP.NET Core API, EF Core, migrations
  tests/SchoolAdmission.Api.Tests/
frontend/                      React and TypeScript (Vite), served by nginx
docker-compose.yml
```

## Reset data

```
docker compose down -v
```

## Not included

Admins cannot yet edit or delete a class. A next step would be a rename and seat change endpoint that never allows seats below those in use, and a delete that is allowed only for classes with no applications.
