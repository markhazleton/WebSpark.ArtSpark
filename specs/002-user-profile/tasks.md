# Tasks: Enhanced User Registration and Profile Management

**Input**: Design documents from `/specs/002-user-profile/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md, contracts/

**Tests**: User stories explicitly request automated coverage; include the test tasks listed below.

**Organization**: Tasks are grouped by user story to keep each increment independently implementable and testable within `WebSpark.ArtSpark.Demo`.

## Format: `[ID] [P?] [Story] Description`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Ensure tooling, packages, and configuration scaffolding exist for the Demo feature work.

- [X] T001 Add `SixLabors.ImageSharp` and `SixLabors.ImageSharp.Web` package references in `WebSpark.ArtSpark.Demo/WebSpark.ArtSpark.Demo.csproj`.
- [X] T002 Create upload storage placeholder at `WebSpark.ArtSpark.Demo/wwwroot/uploads/profiles/.gitkeep` to keep the directory under source control.
- [X] T003 Add `FileUpload` configuration defaults (size, types, path, thumbnails) to `WebSpark.ArtSpark.Demo/appsettings.json` and `WebSpark.ArtSpark.Demo/appsettings.Development.json`.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish shared infrastructure (data model, services, options) required before any story work can begin.

- [X] T004 Update `WebSpark.ArtSpark.Demo/Models/UserModels.cs` to add profile photo path fields, 500 character bio limit enforcement, and `EmailVerified` flag on `ApplicationUser` with `[PersonalData]` annotations.
- [X] T005 Add new audit entity in `WebSpark.ArtSpark.Demo/Models/AuditLog.cs` capturing admin actions with rowversion and FK links to `ApplicationUser`.
- [X] T006 Update `WebSpark.ArtSpark.Demo/Data/ArtSparkDbContext.cs` to register `DbSet<AuditLog>` and configure relationships with restricted deletes and indexed timestamps.
- [X] T007 Add strongly-typed upload options at `WebSpark.ArtSpark.Demo/Options/FileUploadOptions.cs` with max size, allowed MIME types, and thumbnail sizes bound to configuration.
- [X] T008 Create disk photo contract in `WebSpark.ArtSpark.Demo/Services/IProfilePhotoService.cs` defining upload, resize, and removal members.
- [X] T009 Implement ImageSharp-backed storage pipeline in `WebSpark.ArtSpark.Demo/Services/ProfilePhotoService.cs` (validation, orientation fix, thumbnail generation, sanitized filenames).
- [X] T010 Add audit logging abstraction in `WebSpark.ArtSpark.Demo/Services/AuditLogService.cs` to persist entries using `AuditLog` and Serilog correlation IDs.
- [X] T011 Update `WebSpark.ArtSpark.Demo/Program.cs` to register file upload options, profile photo/audit services, and configure `FormOptions.MultipartBodyLengthLimit` per plan.
- [X] T012 Add EF Core migration under `WebSpark.ArtSpark.Demo/Migrations/` introducing new `ApplicationUser` columns and the `AuditLog` table, then update `ArtSparkDbContextModelSnapshot.cs` accordingly.
- [X] T012a Add `WebSpark.ArtSpark.Demo/Options/AuditLogOptions.cs` and `WebSpark.ArtSpark.Demo/Services/AuditLogCleanupService.cs` to enforce 1-year retention with configurable purge cadence.
- [X] T012b Register `AuditLogCleanupService` as a hosted service in `WebSpark.ArtSpark.Demo/Program.cs`, add default `AuditLog:RetentionDays` settings to appsettings, and create retention unit tests in `WebSpark.ArtSpark.Tests/Services/AuditLogCleanupServiceTests.cs`.
- [X] T012c Implement `WebSpark.ArtSpark.Demo/Services/ProfilePhotoStorageMonitor.cs` (and related health check) to measure disk usage for `FileUpload:ProfilePhotoPath` and emit structured alerts when exceeding thresholds (deliver via Serilog Warning level events with structured properties `CurrentUsageMB`, `ThresholdMB`, `PercentageUsed` and health check status degradation to `Degraded` for ASP.NET Core health endpoint monitoring).
- [X] T012d Wire storage monitoring service and health check into `Program.cs`, add `FileUpload:DiskUsageThresholdMB` configuration defaults, and add coverage in `WebSpark.ArtSpark.Tests/Services/ProfilePhotoStorageMonitorTests.cs`.

---

## Phase 3: User Story 1 - User Registration with Profile Photo (Priority: P1) 🎯 MVP

**Goal**: Allow new users to register with an optional profile photo that is validated, resized, stored locally, and displayed immediately after sign-in.

**Independent Test**: Navigate to `/Account/Register`, create an account with and without a photo, and confirm the avatar renders in the navigation bar and profile page with proper validation feedback.

### Tests for User Story 1

- [ ] T013 [US1] Add registration integration tests in `WebSpark.ArtSpark.Tests/Controllers/AccountRegistrationTests.cs` covering photo success, default avatar fallback, and invalid file rejection.
- [ ] T014 [US1] Create `ProfilePhotoService` unit tests in `WebSpark.ArtSpark.Tests/Services/ProfilePhotoServiceTests.cs` validating resizing, orientation correction, and size limits.

### Implementation for User Story 1

- [ ] T015 [US1] Extend `RegisterViewModel` in `WebSpark.ArtSpark.Demo/Models/UserModels.cs` to accept `IFormFile ProfilePhoto` with server-side validation attributes.
- [ ] T016 [US1] Update `WebSpark.ArtSpark.Demo/Views/Account/Register.cshtml` to render photo upload controls, validation messaging, and helper text for supported formats.
- [ ] T017 [US1] Enhance `Register` POST action in `WebSpark.ArtSpark.Demo/Controllers/AccountController.cs` to invoke `IProfilePhotoService`, persist thumbnail paths, and surface validation errors.
- [ ] T018 [US1] Add default avatar asset at `WebSpark.ArtSpark.Demo/wwwroot/img/default-avatar.png` (or SVG) and ensure build includes it.
- [ ] T019 [US1] Update `WebSpark.ArtSpark.Demo/Views/Shared/_Layout.cshtml` to display the current user’s 64px thumbnail (or default avatar) in the account dropdown.
- [ ] T020 [US1] Emit structured registration/photo upload logs via Serilog in `WebSpark.ArtSpark.Demo/Controllers/AccountController.cs` using `AuditLogService` correlation IDs.

---

## Phase 4: User Story 2 - Profile Photo Management (Priority: P1)

**Goal**: Enable authenticated users to upload, replace, or remove their profile photo with consistent display across the Demo.

**Independent Test**: Sign in, navigate to `/Account/Profile`, change the photo, remove it, and verify updates propagate to navigation, collections, and reviews while audit entries capture the actions.

### Tests for User Story 2

- [ ] T021 [US2] Add profile photo management integration tests in `WebSpark.ArtSpark.Tests/Controllers/ProfilePhotoManagementTests.cs` covering upload, replace, and remove flows.
- [ ] T022 [US2] Extend `ProfilePhotoServiceTests` in `WebSpark.ArtSpark.Tests/Services/ProfilePhotoServiceTests.cs` to verify old file cleanup and thumbnail regeneration.

### Implementation for User Story 2

- [X] T023 [US2] Update profile editing view models in `WebSpark.ArtSpark.Demo/Models/UserModels.cs` to include current photo metadata, `IFormFile NewProfilePhoto`, and remove flags.
- [X] T024 [US2] Revise `WebSpark.ArtSpark.Demo/Views/Account/Profile.cshtml` with photo preview, upload/remove controls, and inline validation feedback.
- [X] T025 [US2] Adjust profile POST logic in `WebSpark.ArtSpark.Demo/Controllers/AccountController.cs` to route photo operations through `IProfilePhotoService` and maintain invariants.
- [X] T026 [US2] Expand `WebSpark.ArtSpark.Demo/Services/ProfilePhotoService.cs` to handle replacements, deletion retries, and thumbnail cache busting.
- [X] T027 [US2] Create reusable avatar partial at `WebSpark.ArtSpark.Demo/Views/Shared/_UserAvatar.cshtml` displaying thumbnails with default fallback.
- [ ] T028 [US2] Update identity surfaces (`WebSpark.ArtSpark.Demo/Views/PublicCollections/*.cshtml`, review components, navigation) to consume `_UserAvatar` partial instead of raw URLs.
- [X] T029 [US2] Capture structured audit entries for photo upload/remove actions within `WebSpark.ArtSpark.Demo/Services/AuditLogService.cs` and `AccountController`.

---

## Phase 5: User Story 3 - Role-Based Access Control (Priority: P1)

**Goal**: Provide Admin role capabilities with protected routes, role assignment tools, and audit trails while preventing regular users from accessing admin features.

**Independent Test**: Login as Admin to view `/Admin/Users`, assign/remove roles, and see audit entries; confirm a regular user receives access denied when hitting the same routes directly.

### Tests for User Story 3

- [ ] T030 [US3] Add authorization integration tests in `WebSpark.ArtSpark.Tests/Controllers/AdminUsersAuthorizationTests.cs` ensuring admin-only routes reject non-admins.
- [ ] T031 [US3] Create role management unit tests in `WebSpark.ArtSpark.Tests/Services/AdminUserServiceTests.cs` verifying role assignment safeguards.
- [ ] T031a [US3] Extend admin role tests in `WebSpark.ArtSpark.Tests/Services/AdminUserServiceTests.cs` to cover self-demotion blocking and "last admin" protection scenarios.

### Implementation for User Story 3

- [X] T032 [US3] Define admin service contract in `WebSpark.ArtSpark.Demo/Services/IAdminUserService.cs` for role queries and updates.
- [X] T033 [US3] Implement `AdminUserService` in `WebSpark.ArtSpark.Demo/Services/AdminUserService.cs` using `UserManager`/`RoleManager` with audit hooks.
- [X] T033a [US3] Add guard logic in `AdminUserService` preventing administrators from removing their own Admin role and ensuring at least one Admin remains, returning domain errors when violated.
- [X] T034 [US3] Add identity seeding utility at `WebSpark.ArtSpark.Demo/Data/IdentitySeeder.cs` to ensure `User` and `Admin` roles plus bootstrap admin user exist.
- [X] T035 [US3] Update `WebSpark.ArtSpark.Demo/Program.cs` to register `IAdminUserService`, execute `IdentitySeeder`, and configure `[Authorize(Roles = "Admin")]` policy.
- [X] T036 [US3] Create admin controller `WebSpark.ArtSpark.Demo/Controllers/AdminUsersController.cs` with list and role assignment actions protected by the Admin policy.
- [X] T036a [US3] Handle guard failures in `AdminUsersController` with user-facing validation messages, preserving audit trail entries for blocked attempts.
- [X] T037 [US3] Build `WebSpark.ArtSpark.Demo/Views/AdminUsers/Index.cshtml` to list users with role toggles and audit status badges.
- [ ] T038 [US3] Add partial view `WebSpark.ArtSpark.Demo/Views/AdminUsers/_RoleToggleForm.cshtml` for assigning/removing roles with antiforgery tokens.
- [ ] T039 [US3] Update `WebSpark.ArtSpark.Demo/Views/Shared/_Layout.cshtml` to surface admin navigation links when the current user is in the Admin role.
- [X] T040 [US3] Record role change audits inside `WebSpark.ArtSpark.Demo/Controllers/AdminUsersController.cs` (or service) using `AuditLogService` with actor/target metadata.

---

## Phase 6: User Story 4 - Enhanced Profile Editing (Priority: P2)

**Goal**: Allow users to edit display name, bio, and email with rich validation, helpful guidance, and immediate visual feedback.

**Independent Test**: Edit profile fields with valid/invalid data, observe real-time bio character counter, handle duplicate email errors, and confirm updates persist after refresh.

### Tests for User Story 4

- [ ] T041 [US4] Add profile editing integration tests in `WebSpark.ArtSpark.Tests/Controllers/ProfileEditingTests.cs` covering success paths and validation errors.
- [ ] T042 [US4] Extend controller unit tests in `WebSpark.ArtSpark.Tests/Controllers/AccountControllerTests.cs` to assert duplicate-email handling and confirmation messaging.

### Implementation for User Story 4

- [X] T043 [US4] Enhance `ProfileViewModel` in `WebSpark.ArtSpark.Demo/Models/UserModels.cs` with editable email, character counter metadata, and validation messages.
- [X] T044 [US4] Update profile POST logic in `WebSpark.ArtSpark.Demo/Controllers/AccountController.cs` to handle email changes, uniqueness checks, and `EmailVerified` reset.
- [X] T045 [US4] Refresh `WebSpark.ArtSpark.Demo/Views/Account/Profile.cshtml` with inline guidance, live bio counter placeholders, and confirmation banners.
- [X] T046 [US4] Add front-end script at `WebSpark.ArtSpark.Demo/wwwroot/js/profile-edit.js` for real-time bio character counting and email validation hints.
- [X] T047 [US4] Emit Serilog events for profile updates (field diff, outcome) inside `AccountController` leveraging correlation IDs.

---

## Phase 7: User Story 5 - Admin User Management (Priority: P2)

**Goal**: Provide administrators with searchable, pageable user management dashboards including role edits, account disable/enable, and activity insights.

**Independent Test**: As Admin, search/filter users, open a detail view showing activity and audit history, adjust roles and account status, and confirm changes persist with proper audit logging.

### Tests for User Story 5

- [ ] T048 [US5] Add admin list integration tests in `WebSpark.ArtSpark.Tests/Controllers/AdminUsersManagementTests.cs` covering paging, filtering, and role persistence.
- [ ] T049 [US5] Add audit retrieval tests in `WebSpark.ArtSpark.Tests/Services/AdminUserServiceTests.cs` ensuring logs paginate by timestamp and user.

### Implementation for User Story 5

- [X] T050 [US5] Extend `WebSpark.ArtSpark.Demo/Services/AdminUserService.cs` with paged search queries, detail DTO assembly, and lockout toggles.
- [X] T051 [US5] Update `WebSpark.ArtSpark.Demo/Controllers/AdminUsersController.cs` to expose search, detail, and enable/disable endpoints with validation.
- [X] T052 [US5] Create detailed view at `WebSpark.ArtSpark.Demo/Views/AdminUsers/Details.cshtml` summarizing account info, collections/reviews counts, and activity.
- [ ] T053 [US5] Add audit log partial `WebSpark.ArtSpark.Demo/Views/AdminUsers/_UserAuditTable.cshtml` rendering recent actions with pagination controls.
- [X] T054 [US5] Enhance `WebSpark.ArtSpark.Demo/Views/AdminUsers/Index.cshtml` with search/filter UI, pagination, and role badges.
- [X] T055 [US5] Implement disable/enable workflow (lockout or soft delete) within `AdminUserService` and persist audit entries via `AuditLogService`.
- [X] T056 [US5] Add Serilog instrumentation for admin actions in `AdminUsersController` to capture actor, target, action type, and outcome.

---

## Phase 8: User Story 6 - Password Strength and Security (Priority: P2)

**Goal**: Enforce strong passwords with real-time feedback and common-password detection during registration and password changes.

**Independent Test**: Attempt to register/change password with weak, common, and strong passwords; confirm UI feedback and server validation align with OWASP requirements.

### Tests for User Story 6

- [ ] T057 [US6] Add password validator unit tests in `WebSpark.ArtSpark.Tests/Services/PasswordStrengthServiceTests.cs` covering entropy scoring and common-password detection.
- [ ] T058 [US6] Extend registration integration tests in `WebSpark.ArtSpark.Tests/Controllers/AccountRegistrationTests.cs` to assert strong password enforcement and feedback messaging.

### Implementation for User Story 6

- [X] T059 [US6] Add curated weak password list file at `WebSpark.ArtSpark.Demo/Data/common-passwords.txt` for client/server checks.
- [X] T060 [US6] Implement strength service in `WebSpark.ArtSpark.Demo/Services/PasswordStrengthService.cs` providing scoring and common-password lookup.
- [X] T061 [US6] Update `WebSpark.ArtSpark.Demo/Program.cs` to enforce new `IdentityOptions.Password` requirements and register `PasswordStrengthService`.
- [X] T062 [US6] Enhance `WebSpark.ArtSpark.Demo/Views/Account/Register.cshtml` with strength meter UI bound to real-time feedback placeholders.
- [ ] T063 [US6] Add change password actions/views (`WebSpark.ArtSpark.Demo/Controllers/AccountController.cs`, `WebSpark.ArtSpark.Demo/Views/Account/ChangePassword.cshtml`) incorporating strength checks.
- [X] T064 [US6] Create client script at `WebSpark.ArtSpark.Demo/wwwroot/js/password-strength.js` reading the common-password list slice and updating the meter.
- [ ] T065 [US6] Integrate server-side common-password validation in registration and change-password handlers within `AccountController`.
- [ ] T066 [US6] Add security-focused logging for password update attempts (without sensitive data) using Serilog in `AccountController`.

---

## Phase 9: User Story 7 - Email Verification (Priority: P3)

**Goal**: Require new users to confirm their email via SMTP-delivered tokens with resend support and clear messaging.

**Independent Test**: Register a new account, receive and follow the verification link within 24 hours, check access restrictions prior to verification, and confirm resend handles expired tokens.

### Tests for User Story 7

- [ ] T067 [US7] Add email verification integration tests in `WebSpark.ArtSpark.Tests/Controllers/EmailVerificationTests.cs` validating token issuance, confirmation, and access gating.
- [ ] T068 [US7] Add resend/expiration unit tests in `WebSpark.ArtSpark.Tests/Services/EmailVerificationServiceTests.cs` ensuring expired tokens prompt new emails.

### Implementation for User Story 7

- [X] T069 [US7] Add email options contract at `WebSpark.ArtSpark.Demo/Options/EmailOptions.cs` mapping SMTP settings from configuration.
- [X] T070 [US7] Implement SMTP email sender in `WebSpark.ArtSpark.Demo/Services/SmtpEmailSender.cs` (with `IEmailSender` adapter) supporting templated messages.
- [X] T071 [US7] Update `WebSpark.ArtSpark.Demo/Program.cs` to require confirmed accounts, register the email sender, and bind `EmailOptions` from configuration.
- [ ] T072 [US7] Modify `AccountController` register/login flows to send verification emails, block unverified actions, and expose confirm/resend endpoints.
- [ ] T073 [US7] Add views `WebSpark.ArtSpark.Demo/Views/Account/EmailVerificationRequired.cshtml` and `WebSpark.ArtSpark.Demo/Views/Account/ConfirmEmail.cshtml` with guidance messaging.
- [X] T074 [US7] Extend configuration (`appsettings.json`, `appsettings.Development.json`) with SMTP settings referenced by `EmailOptions`.
- [ ] T075 [US7] Record verification and resend events via `AuditLogService` and Serilog for traceability inside `AccountController`.

---

## Final Phase: Polish & Cross-Cutting Concerns

**Purpose**: Consolidate documentation, quality checks, and operational readiness after user stories are complete.

- [ ] T076 Update documentation (`README.md`, `WebSpark.ArtSpark.Demo/README.md`, `docs/User-Profile-Management.md`, `docs/Role-Based-Access-Control.md`) with setup, configuration, and workflow details.
- [ ] T077 Revise `docs/Live-Testing-Checklist.md` to add registration, profile, admin, password, and verification scenarios.
- [ ] T078 Add release notes and audit references to `docs/Documentation-Update-Summary.md` (or successor) highlighting database migration and configuration changes.
- [ ] T079 Run full automated suite (`dotnet test WebSpark.ArtSpark.Tests`) and capture results in `docs/copilot/2025-11-16/test-report.md`.
- [ ] T080 Perform manual verification per quickstart checklist and log outcomes in `docs/copilot/2025-11-16/manual-validation.md`.
- [ ] T081 Instrument photo upload pipeline timings (Serilog metrics and health checks) to validate SC-002 thresholds and document collection endpoints in `docs/copilot/2025-11-16/metrics-notes.md`.
- [ ] T082 Capture admin dashboard query performance metrics and registration completion analytics (SC-003, SC-008) via logging/dashboard configuration, documenting access in updated README sections.

---

## Dependencies & Execution Order

### Phase Dependencies
- Phase 1 → Phase 2 → all user story phases → Final Phase.
- User story phases may start after Phase 2 completes; follow priority order (P1 → P2 → P3) unless team capacity allows parallelization without conflicts.

### User Story Dependencies
- US1 has no upstream story dependency (MVP once foundational tasks finish).
- US2 depends on US1’s avatar infrastructure to extend management capabilities.
- US3 depends on foundational services and may run in parallel with US2 after Phase 2.
- US4 builds on US2 profile UI/service changes.
- US5 extends US3 admin tooling.
- US6 depends on US1/US4 registration and profile flows for integration points.
- US7 depends on US1 registration and leverages infrastructure from Phase 2.

---

## Parallel Execution Examples
- **US1**: Implement tests (T013, T014) in parallel while front-end updates (T016, T019) proceed once model changes (T015) land.
- **US2**: UI update (T024) and service enhancement (T026) can run concurrently after model update (T023).
- **US3**: Seeder/controller work (T034–T036) can progress while views (T037–T039) are constructed, converging before audit logging (T040).
- **US4**: Script creation (T046) may proceed alongside controller updates (T044) once the view model changes (T043) are in place.
- **US5**: Service paging (T050) and UI enhancements (T054) can advance simultaneously after tests (T048–T049) are authored.
- **US6**: Client script (T064) and service logic (T060) can be built in parallel after common password data (T059) is committed.
- **US7**: Email sender (T070) and controller updates (T072) may develop concurrently once options (T069) exist.

---

## Implementation Strategy

1. Complete Phase 1–2 to lock in schema, services, and configuration scaffolding.
2. Deliver MVP by finishing US1 (registration with avatar) and validating end-to-end.
3. Iterate through P1 user stories (US2, US3) to solidify profile management and admin access.
4. Layer P2 enhancements (US4–US6) for richer profile tools, admin dashboards, and stronger security.
5. Conclude with P3 email verification (US7) and the polish phase to finalize documentation and operational readiness.
