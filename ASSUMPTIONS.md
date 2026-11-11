# Assumptions

Points the brief left open, what was chosen, and why.

## Behaviour

- A student may hold or own at most one active application per class (HELD or CONFIRMED). Applying to several classes is allowed, since the brief limits it per class only.
- A student may apply again to a class after an earlier application there expired or was cancelled. The earlier attempt no longer holds a seat.
- A confirmed admission cannot be cancelled. The brief has no refund flow.
- Class names must be unique, ignoring case and surrounding spaces, so two identical rows never confuse guardians.
- A class name is 1 to 100 characters; seats are 1 to 10000.
- The birth registration number is checked only as exactly 17 digits. The brief gives no further format rules.
- A held seat is freed at the exact second it expires. Reads treat a HELD application past its deadline as EXPIRED; a background job only updates the stored rows.
- IDs are UUID v7 strings rather than the `c_123` style in the examples, which the brief leaves to us.
- There is no login. The birth registration number is sent in the `X-Student-Id` header and the frontend keeps it in the browser's local storage.
- UI text is in English.

## Seat limit under concurrency and multiple processes

- The only operation that adds to a class's held plus confirmed count is creating an application.
- Each one runs in a transaction that first locks the class row (`SELECT ... FOR UPDATE`), then counts active applications and inserts only if a seat is free. Postgres gives that lock to one transaction at a time, so applications for the same class are handled one after another.
- Confirming moves a seat from held to confirmed, and cancel and expiry only free seats, so none of them can exceed the limit.
- The lock lives in the database, not in application memory, so the guarantee is the same with one backend process or many.
- Isolation stays at READ COMMITTED, so the count taken after the lock is granted sees the row committed by the previous holder.
- A partial unique index on (class, student) for active applications also blocks duplicates at the database level.
- Different classes do not wait on each other.
- A test sends 200 parallel applications to a 10-seat class and checks that exactly 10 succeed. With the lock removed, the same test let 49 through.

## Status codes

The brief fixes only 409 for a full class. The others follow common HTTP use:

| Case | Status |
|---|---|
| Invalid body or missing/invalid `X-Student-Id` | 400 |
| Application belongs to another student | 403 |
| Unknown or malformed class or application id | 404 |
| Class full, already applied, expired, cancelled, already confirmed, name taken | 409 |
| Repeat confirm | 200 (no change) |
| Repeat cancel | 204 (no change) |

Errors use the RFC 7807 problem format with an extra `code` field, for example `CLASS_FULL`.

## Operations

- PostgreSQL is published on 127.0.0.1:5433 by default to avoid clashing with a local Postgres on 5432.
- EF Core SQL command logs are kept at Warning so busy periods do not log one line per query.
