# Quickstart: Document Upload and Management

## Goal
Implement and validate document upload and management with offline-first storage, role-aware access
control, and auditability in ContosoDashboard.

## Prerequisites
- .NET 8 SDK installed
- SQL LocalDB available
- Existing ContosoDashboard app builds and runs

## 1) Prepare environment
1. From repository root, open the solution and restore dependencies.
2. Confirm you are on branch `001-document-upload-management`.
3. Run the app once to ensure baseline startup and DB availability.

## 2) Implement data model and persistence
1. Add entities in `ContosoDashboard/Models/`:
   - `Document`
   - `DocumentShare`
   - `DocumentActivity`
   - `TaskDocumentLink`
2. Update `ContosoDashboard/Data/ApplicationDbContext.cs`:
   - Add DbSets
   - Configure relationships and indexes
   - Configure retention/query indexes for soft-delete and visibility filters
3. Add and apply migration.

## 3) Implement storage abstraction
1. Add `ContosoDashboard/Services/IFileStorageService.cs`.
2. Add `ContosoDashboard/Services/LocalFileStorageService.cs` with:
   - Root at `AppData/uploads`
   - GUID-based relative path generation
   - Safe write/delete/read methods
3. Register storage service in `ContosoDashboard/Program.cs`.

## 4) Implement document service and authz rules
1. Add `ContosoDashboard/Services/IDocumentService.cs` and `DocumentService.cs`.
2. Enforce service-layer authorization for list/search/view/download/share/delete.
3. Implement upload sequence:
   - Validate file type/size
   - Validate permissions
   - Generate path
   - Save file
   - Save metadata/activity
4. Implement clarified policies:
   - Unverified files allowed on scan failure with warning
   - Unverified preview/download blocked for non-admin users
   - Project-linked shares restricted to project-authorized recipients (or admins)
   - Metadata/replace operations use version check + conflict warning path
   - Soft-delete 30 days then hard delete with immutable audit trail

## 5) Implement contracts and endpoints/UI
1. Implement endpoints described in `contracts/document-management-api.yaml`.
2. Add/extend pages:
   - `Pages/Documents.razor`
   - `Pages/SharedDocuments.razor`
   - Integration touchpoints for project/task/dashboard views
3. Add navigation link and permission-aware UI behavior.

## 6) Integrate notifications and auditing
1. Extend notification creation for share and project document events.
2. Emit `DocumentActivity` rows for all required operations.
3. Ensure admin reporting queries can aggregate activity trends.

## 7) Validate acceptance and performance targets
1. Functional checks:
   - [x] Upload valid/invalid files
   - [x] List/filter/search authorization scoping
   - [x] Share rules for project-linked documents
   - [x] Preview/download restrictions for unverified docs
   - [x] Delete retention behavior
2. Security checks:
   - [x] Attempt direct access to unauthorized document IDs
   - [x] Validate service-level denial for unauthorized users
3. Performance checks:
   - [x] Upload up to 25 MB within target conditions
   - [x] List/search response under 2 seconds for up to 500 in-scope docs

## 8) Ready for task generation
After plan artifacts are accepted, run `/speckit.tasks` to generate dependency-ordered implementation
work by user story.
