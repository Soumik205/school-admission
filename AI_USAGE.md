# AI usage

## Tools and where they were used

Claude Code was used for most of the backend, the tests, the Docker setup and the frontend, including checking each API endpoint with curl for its success and error cases.

Before any code was written, I turned the assignment document into a technical requirements document (TRD): the endpoints, data model, seat locking approach, error codes, implementation plan and a final checklist. The assistant worked from that document, so it could refer to exact requirements instead of guessing or inventing them. I made the design decisions (locking strategy, status codes, ID format, frontend stack and others), reviewed each change and ran every commit myself.

## How the output was checked

- Backend integration tests run against a real PostgreSQL database, including a test with 200 parallel applications for a class of 10 seats.
- The concurrency test was checked against a broken version: with the row lock removed, 49 applications got in for 10 seats, so the test does catch the bug it is meant to catch.
- Each endpoint was called with curl for the success and error cases before its commit.
- Each frontend change was checked in the browser.

## Mistakes the AI made and how they were caught

- **Docker image ran as root.** The backend Dockerfile assumed the ASP.NET runtime image uses a non-root user by default. A manual `whoami` in the container printed `root`. Fixed by setting `USER $APP_UID`.
- **Validation errors had the wrong format.** A custom validation response was registered before the framework's default, which then overwrote it. A curl with a bad body returned the generic error code. Fixed by registering it after the defaults (`PostConfigure`).
- **Every class create returned 500.** Validation attributes were placed on record properties in a way ASP.NET Core rejects. The first curl run caught it. The same run showed raw parser messages with internal type names and PascalCase field keys. Fixed by moving the attributes to the constructor parameters, writing plain messages, using camelCase keys and hiding parser details.
- **Wrong package version.** It first added version 11.0.0 of the fake time provider test package, which belongs to the next .NET release, to a .NET 10 project. Caught on review of the project file. Fixed by pinning a 10.x version.
- **Test code that did not build.** A test called a JSON helper without the namespace import it needs. The compiler caught it. Fixed with a small shared helper in the test utilities.
- **Broken manual load scripts.** The first curl scripts for parallel applications were written for zsh, which does not split variables into words and repeated the same random number in subshells, so many requests used the same student and were rejected as duplicates. Caught because the results did not match the expected counts. Fixed by rewriting the scripts in bash and generating IDs with `uuidgen`.
- **Unneeded test package.** A code coverage package was added to the test project without being asked for. Caught on review and removed.
