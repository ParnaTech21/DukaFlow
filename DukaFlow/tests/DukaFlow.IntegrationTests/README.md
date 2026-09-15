# Integration test placeholder

This project builds against the same Application/Infrastructure projects as the API.
Add EF Core InMemory-backed tests here for:

- Registration creates a User + Restaurant
- Duplicate email registration returns a conflict
- Login returns a JWT for valid credentials
- A user cannot fetch/update another restaurant's profile (tenant isolation)

These were intentionally left as a placeholder so you can decide, together with Claude,
how deep to go on Phase 1 test coverage before writing them.
