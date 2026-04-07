# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-upload-management/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: No explicit TDD/test-first requirement was stated in the feature spec, so dedicated test-writing tasks are omitted in this plan.

**Organization**: Tasks are grouped by user story to support independent delivery and validation.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare project configuration and scaffolding for document workflows.

- [X] T001 Add document upload configuration section in ContosoDashboard/appsettings.json
- [X] T002 Add document upload configuration section in ContosoDashboard/appsettings.Development.json
- [X] T003 Create storage options model in ContosoDashboard/Services/Storage/DocumentStorageOptions.cs
- [X] T004 [P] Create document API request/response DTOs in ContosoDashboard/Services/Documents/DocumentDtos.cs
- [X] T005 [P] Create document category constants in ContosoDashboard/Models/DocumentCategories.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core architecture and security primitives required before any user story implementation.

**CRITICAL**: No user story work begins until this phase is complete.

- [X] T006 Create Document entity in ContosoDashboard/Models/Document.cs
- [X] T007 [P] Create DocumentShare entity in ContosoDashboard/Models/DocumentShare.cs
- [X] T008 [P] Create DocumentActivity entity in ContosoDashboard/Models/DocumentActivity.cs
- [X] T009 [P] Create TaskDocumentLink entity in ContosoDashboard/Models/TaskDocumentLink.cs
- [X] T010 Update EF model configuration and DbSets in ContosoDashboard/Data/ApplicationDbContext.cs
- [X] T011 Create initial document feature migration in ContosoDashboard/Data/Migrations/
- [X] T012 Create file storage abstraction interface in ContosoDashboard/Services/IFileStorageService.cs
- [X] T013 Implement local storage provider in ContosoDashboard/Services/LocalFileStorageService.cs
- [X] T014 Create malware scan abstraction in ContosoDashboard/Services/IScanService.cs
- [X] T015 Implement default scan service behavior in ContosoDashboard/Services/DefaultScanService.cs
- [X] T016 Create document service contract in ContosoDashboard/Services/IDocumentService.cs
- [X] T017 Implement authorization helper methods in ContosoDashboard/Services/DocumentAuthorizationService.cs
- [X] T018 Register document services and options in ContosoDashboard/Program.cs
- [X] T019 Add base documents API controller shell in ContosoDashboard/Controllers/DocumentsController.cs
- [X] T020 Implement retention purge background service in ContosoDashboard/Services/DocumentRetentionHostedService.cs

**Checkpoint**: Foundation complete; user stories can proceed.

---

## Phase 3: User Story 1 - Secure Document Upload (Priority: P1) MVP

**Goal**: Authenticated users can securely upload supported files with required metadata and see immediate outcomes.

**Independent Test**: Login as employee, upload valid and invalid files, verify success/error behavior and uploaded item visibility in My Documents.

- [X] T021 [US1] Implement upload validation rules in ContosoDashboard/Services/DocumentService.cs
- [X] T022 [US1] Implement safe path generation and write sequence in ContosoDashboard/Services/LocalFileStorageService.cs
- [X] T023 [US1] Implement POST /api/documents endpoint in ContosoDashboard/Controllers/DocumentsController.cs
- [X] T024 [US1] Implement unverified-on-scan-failure behavior in ContosoDashboard/Services/DocumentService.cs
- [X] T025 [US1] Add upload form and progress UI in ContosoDashboard/Pages/Documents.razor
- [X] T026 [US1] Add upload form state handling code in ContosoDashboard/Pages/Documents.razor.cs
- [X] T027 [US1] Implement My Documents initial query in ContosoDashboard/Services/DocumentService.cs
- [X] T028 [US1] Surface upload success/error messaging in ContosoDashboard/Pages/Documents.razor

**Checkpoint**: User Story 1 is independently functional.

---

## Phase 4: User Story 2 - Organize and Find Documents (Priority: P2)

**Goal**: Users can list, sort, filter, and search documents within their authorization scope.

**Independent Test**: Populate mixed documents and verify sorting/filtering/search only returns authorized results.

- [X] T029 [US2] Implement document list query with filters and sorting in ContosoDashboard/Services/DocumentService.cs
- [X] T030 [US2] Implement GET /api/documents endpoint in ContosoDashboard/Controllers/DocumentsController.cs
- [X] T031 [US2] Implement search query by title/description/tags/uploader/project in ContosoDashboard/Services/DocumentService.cs
- [X] T032 [US2] Implement GET /api/documents/search endpoint in ContosoDashboard/Controllers/DocumentsController.cs
- [X] T033 [US2] Add filter/sort/search UI controls in ContosoDashboard/Pages/Documents.razor
- [X] T034 [US2] Add project-scoped document retrieval in ContosoDashboard/Services/DocumentService.cs
- [X] T035 [US2] Integrate project documents section in ContosoDashboard/Pages/ProjectDetails.razor
- [X] T036 [P] [US2] Add query performance indexes migration in ContosoDashboard/Data/Migrations/

**Checkpoint**: User Story 2 is independently functional.

---

## Phase 5: User Story 3 - Manage Access and Collaboration (Priority: P3)

**Goal**: Authorized users can update metadata, replace files, share documents, and delete with retention rules.

**Independent Test**: Validate metadata edits, replace flow, share restrictions, unverified-access rules, and soft-delete retention behavior by role.

- [X] T037 [US3] Implement metadata update with version conflict checks in ContosoDashboard/Services/DocumentService.cs
- [X] T038 [US3] Implement PATCH /api/documents/{documentId} endpoint in ContosoDashboard/Controllers/DocumentsController.cs
- [X] T039 [US3] Implement file replace workflow with version token in ContosoDashboard/Services/DocumentService.cs
- [X] T040 [US3] Implement POST /api/documents/{documentId}/replace endpoint in ContosoDashboard/Controllers/DocumentsController.cs
- [X] T041 [US3] Implement preview/download authorization including unverified restrictions in ContosoDashboard/Services/DocumentService.cs
- [X] T042 [US3] Implement download and preview endpoints in ContosoDashboard/Controllers/DocumentsController.cs
- [X] T043 [US3] Implement soft-delete plus purge scheduling in ContosoDashboard/Services/DocumentService.cs
- [X] T044 [US3] Implement DELETE /api/documents/{documentId} endpoint in ContosoDashboard/Controllers/DocumentsController.cs
- [X] T045 [US3] Implement project-restricted share recipient validation in ContosoDashboard/Services/DocumentService.cs
- [X] T046 [US3] Implement POST /api/documents/{documentId}/shares endpoint in ContosoDashboard/Controllers/DocumentsController.cs
- [X] T047 [US3] Add shared documents page UI in ContosoDashboard/Pages/SharedDocuments.razor
- [X] T048 [US3] Add share notifications integration in ContosoDashboard/Services/NotificationService.cs

**Checkpoint**: User Story 3 is independently functional.

---

## Phase 6: User Story 4 - Use Documents in Existing Workflows (Priority: P4)

**Goal**: Document workflows are integrated into task, project, dashboard, and notifications surfaces.

**Independent Test**: Validate task-document links, dashboard recent documents/counts, and project activity notifications.

- [X] T049 [US4] Implement task-document link operations in ContosoDashboard/Services/DocumentService.cs
- [X] T050 [US4] Integrate task document panel in ContosoDashboard/Pages/Tasks.razor
- [X] T051 [US4] Add document upload entry from task context in ContosoDashboard/Pages/TaskDetails.razor
- [X] T052 [US4] Add recent documents dashboard query in ContosoDashboard/Services/DashboardService.cs
- [X] T053 [US4] Add recent documents widget in ContosoDashboard/Pages/Index.razor
- [X] T054 [US4] Add document counts to dashboard summary in ContosoDashboard/Pages/Index.razor
- [X] T055 [US4] Add project new-document notification workflow in ContosoDashboard/Services/NotificationService.cs
- [X] T056 [US4] Add Documents navigation entry in ContosoDashboard/Shared/NavMenu.razor

**Checkpoint**: User Story 4 is independently functional.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Cross-story hardening, reporting, and readiness checks.

- [X] T057 Create admin document activity reporting queries in ContosoDashboard/Services/DocumentReportingService.cs
- [X] T058 Add admin document activity report page in ContosoDashboard/Pages/DocumentReports.razor
- [X] T059 [P] Add security headers and download response hardening for document endpoints in ContosoDashboard/Program.cs
- [X] T060 [P] Add audit event coverage for all document actions in ContosoDashboard/Services/DocumentService.cs
- [X] T061 Update feature documentation and operator notes in README.md
- [X] T062 Run end-to-end quickstart validation checklist in specs/001-document-upload-management/quickstart.md

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup (Phase 1): starts immediately.
- Foundational (Phase 2): depends on Setup and blocks all user stories.
- User Stories (Phases 3-6): depend on Foundational completion.
- Polish (Phase 7): depends on completion of selected user stories.

### User Story Dependencies

- US1 (P1): starts immediately after Foundational.
- US2 (P2): starts after Foundational; benefits from US1 document creation flows but remains independently testable.
- US3 (P3): starts after Foundational; uses document base created in US1/US2 but remains independently testable.
- US4 (P4): starts after Foundational; integrates with prior stories and existing pages, still independently testable as integration slice.

### Within Each User Story

- Service/domain logic before endpoint wiring.
- Endpoint wiring before UI integration.
- Authorization and validation checks included before story checkpoint.

## Parallel Opportunities

- Setup: T004-T005 can run in parallel.
- Foundational: T007-T009 can run in parallel; T012-T015 can run in parallel.
- US2: T036 can run in parallel with UI tasks after core query shape is decided.
- Polish: T059-T060 can run in parallel.

## Parallel Example: User Story 1

- Run in parallel:
  - T025 Add upload form and progress UI in ContosoDashboard/Pages/Documents.razor
  - T026 Add upload form state handling code in ContosoDashboard/Pages/Documents.razor.cs
- Then run:
  - T023 Implement POST /api/documents endpoint in ContosoDashboard/Controllers/DocumentsController.cs

## Parallel Example: User Story 2

- Run in parallel:
  - T033 Add filter/sort/search UI controls in ContosoDashboard/Pages/Documents.razor
  - T036 Add query performance indexes migration in ContosoDashboard/Data/Migrations/
- Then run:
  - T032 Implement GET /api/documents/search endpoint in ContosoDashboard/Controllers/DocumentsController.cs

## Parallel Example: User Story 3

- Run in parallel:
  - T047 Add shared documents page UI in ContosoDashboard/Pages/SharedDocuments.razor
  - T048 Add share notifications integration in ContosoDashboard/Services/NotificationService.cs
- Then run:
  - T046 Implement POST /api/documents/{documentId}/shares endpoint in ContosoDashboard/Controllers/DocumentsController.cs

## Parallel Example: User Story 4

- Run in parallel:
  - T052 Add recent documents dashboard query in ContosoDashboard/Services/DashboardService.cs
  - T056 Add Documents navigation entry in ContosoDashboard/Shared/NavMenu.razor
- Then run:
  - T053 Add recent documents widget in ContosoDashboard/Pages/Index.razor

## Implementation Strategy

### MVP First (US1)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 (US1).
3. Validate upload, metadata capture, and secure visibility.
4. Demo MVP before progressing.

### Incremental Delivery

1. Deliver US1 for central secure upload.
2. Deliver US2 for retrieval and search productivity gains.
3. Deliver US3 for collaboration and lifecycle controls.
4. Deliver US4 for embedded workflow integration.
5. Finish with Phase 7 hardening/reporting.

### Parallel Team Strategy

1. Team aligns on Phase 1-2 foundations.
2. After foundation checkpoint:
   - Engineer A: US1 and API upload path
   - Engineer B: US2 discovery experiences
   - Engineer C: US3 collaboration controls
3. Integrate US4 once core document APIs stabilize.
