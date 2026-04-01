# Data Model: Document Upload and Management

## Entity: Document
- Primary key: `DocumentId` (int)
- Required fields:
  - `Title` (string, max 200)
  - `Category` (string, max 100; text value, not enum int)
  - `FilePath` (string, max 500; unique relative path)
  - `FileNameOriginal` (string, max 255)
  - `FileType` (string, max 255)
  - `FileSizeBytes` (long)
  - `UploadedByUserId` (int, FK -> User.UserId)
  - `CreatedDateUtc` (datetime)
  - `UpdatedDateUtc` (datetime)
  - `IsUnverified` (bool)
  - `IsDeleted` (bool)
- Optional fields:
  - `Description` (string, max 2000)
  - `ProjectId` (int?, FK -> Project.ProjectId)
  - `DeletedDateUtc` (datetime?)
  - `PurgeAfterUtc` (datetime?)
  - `VersionToken` (string or row token for optimistic conflict check)
- Relationships:
  - Many Documents -> one User (uploader)
  - Many Documents -> zero/one Project
  - One Document -> many DocumentShares
  - One Document -> many DocumentActivities
  - One Document -> many TaskDocumentLinks
- Validation rules:
  - File size <= 25 MB
  - File extension/type must be in whitelist
  - `Title` and `Category` required
  - Project-linked upload requires uploader project authorization
  - Unverified docs cannot be previewed/downloaded by non-admin users
- State transitions:
  - `ActiveVerified`: default after successful scan
  - `ActiveUnverified`: scanner unavailable/error/timeout
  - `SoftDeleted`: user confirms delete, hidden from user-facing lists
  - `Purged`: physical file and active metadata removed after retention window

## Entity: DocumentShare
- Primary key: `DocumentShareId` (int)
- Required fields:
  - `DocumentId` (int, FK -> Document.DocumentId)
  - `SharedByUserId` (int, FK -> User.UserId)
  - `SharedWithUserId` (int, FK -> User.UserId)
  - `CreatedDateUtc` (datetime)
- Optional fields:
  - `RevokedDateUtc` (datetime?)
  - `Reason` (string, max 500)
- Validation rules:
  - For project-linked docs, recipient must already have project access unless sharer is admin
  - Duplicate active share to same user should be idempotent (reuse existing or update timestamp)

## Entity: DocumentActivity
- Primary key: `DocumentActivityId` (int)
- Required fields:
  - `DocumentId` (int, FK -> Document.DocumentId)
  - `ActorUserId` (int, FK -> User.UserId)
  - `ActivityType` (string, max 50)
  - `CreatedDateUtc` (datetime)
- Optional fields:
  - `Details` (string, max 2000)
- Activity types:
  - `Upload`, `ScanUnavailable`, `ScanVerified`, `Download`, `Preview`, `MetadataUpdate`, `Replace`, `Share`, `Unshare`, `SoftDelete`, `HardDelete`
- Validation rules:
  - Audit entries are immutable

## Entity: TaskDocumentLink
- Primary key: `TaskDocumentLinkId` (int)
- Required fields:
  - `TaskId` (int, FK -> TaskItem.TaskId)
  - `DocumentId` (int, FK -> Document.DocumentId)
  - `LinkedByUserId` (int, FK -> User.UserId)
  - `CreatedDateUtc` (datetime)
- Validation rules:
  - If task has project, linked document `ProjectId` must be null or match task project
  - Link creation requires access to both task and document

## Query and Index Notes
- Suggested indexes:
  - Document: `(UploadedByUserId, CreatedDateUtc DESC)`
  - Document: `(ProjectId, CreatedDateUtc DESC)`
  - Document: `(IsDeleted, IsUnverified)`
  - DocumentShare: `(SharedWithUserId, RevokedDateUtc)`
  - TaskDocumentLink: `(TaskId, DocumentId)` unique
- Search fields:
  - Document `Title`, `Description`, `Category`
  - Uploader user display name
  - Project name
