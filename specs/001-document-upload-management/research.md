# Research: Document Upload and Management

## Decision 1: File storage abstraction with local-first implementation
- Decision: Implement `IFileStorageService` with `LocalFileStorageService` as default and keep the service contract compatible with future blob storage implementation.
- Rationale: Satisfies offline-first training constraints while preserving a cloud migration path with no business-logic rewrites.
- Alternatives considered: Direct `wwwroot` storage (rejected: direct URL exposure risk), DB BLOB-only storage (rejected: larger DB burden and weaker migration posture), immediate cloud SDK dependency (rejected: violates local training default).

## Decision 2: Upload transaction sequence and path strategy
- Decision: Use sequence `generate unique path -> write file -> persist metadata` with path shape `{userId}/{projectId|personal}/{guid}.{ext}`.
- Rationale: Prevents orphaned DB rows and duplicate key/path collisions; aligns with stakeholder guidance and existing architecture principles.
- Alternatives considered: `persist metadata -> write file` (rejected: orphan risk), user-provided file names in path (rejected: traversal and collision risk), sequential names (rejected: predictability/collision concerns).

## Decision 3: Malware scan failure behavior
- Decision: If scanning is unavailable/errors/times out, allow upload, mark as unverified, warn user, and block preview/download for non-admins until verified.
- Rationale: Matches clarifications, balances availability and security, and keeps explicit access control around unverified content.
- Alternatives considered: Fail closed for all uploads (rejected: harms training usability during scanner outages), allow all authorized access with warning (rejected: weaker safety boundary).

## Decision 4: Authorization model for document access and sharing
- Decision: Enforce authorization in service layer for all list/search/view/download/share/delete operations; for project-linked documents, only project-authorized users (or admins) can be direct recipients.
- Rationale: Aligns with existing IDOR-resistant service patterns in the codebase and clarified sharing policy.
- Alternatives considered: UI-only authorization (rejected: bypass risk), open organizational sharing for project docs (rejected: breaks least-privilege boundary).

## Decision 5: Concurrency handling for metadata update and file replace
- Decision: Implement version check with conflict warning; after user confirmation, last confirmed write is persisted.
- Rationale: Matches clarified requirement and keeps implementation simple for training scope.
- Alternatives considered: Pessimistic locking (rejected: higher complexity), silent last-write-wins without warning (rejected: hidden data-loss risk).

## Decision 6: Deletion and retention policy
- Decision: Use soft-delete for 30 days with non-user-visible state, scheduled hard delete, and immutable audit event retention.
- Rationale: Reflects clarified policy and preserves auditability while enabling deterministic cleanup.
- Alternatives considered: Immediate hard delete (rejected: no recovery window), indefinite soft-delete (rejected: unnecessary storage growth).

## Decision 7: Search and performance approach
- Decision: Implement EF Core query patterns (indexed filters + text search fields) scoped by authorization; tune for <= 500 in-scope docs and 2s target.
- Rationale: Meets stated scale/performance goals without introducing heavyweight search infrastructure.
- Alternatives considered: External search engine dependency (rejected: unnecessary complexity for initial scale/training scope).
