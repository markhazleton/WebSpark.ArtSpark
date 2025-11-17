# Feature Specification: Enhanced User Registration and Profile Management

**Feature Branch**: `002-user-profile`  
**Created**: November 16, 2025  
**Status**: Draft  
**Input**: User description: "Improve the User registration and profile experience to be more best in class according the industry best practices. Have the ability to upload a profile photo, Implement roles of User and Admin"  
**Live Demo Impact**: This specification enhances the Demo's user authentication and profile management system with industry best practices including profile photo uploads, role-based access control (User/Admin), improved registration flows, and enhanced profile management capabilities that increase user engagement and enable administrative oversight.  
**Related Projects**: 
- **Demo**: Primary project - adds profile photo upload functionality, role management UI, enhanced registration/profile editing workflows
- **Client/Agent/Console**: No contract changes required - feature is entirely within Demo boundaries

## Clarifications

### Session 2025-11-16

- Q: What storage architecture should be used for profile photos in production environments? → A: Local file system storage only (simple, suitable for single-server deployments)
- Q: How does content get flagged for admin moderation? → A: Manual admin discovery only (admins find issues through browsing/reports outside system)
- Q: What storage quota limits should apply per user or system-wide? → A: No hard quota initially (monitor and add limits if needed based on usage)
- Q: What email service provider should be used for verification emails? → A: SMTP configuration (standard protocol, works with any email provider)
- Q: How should commonly compromised passwords be detected? → A: Client-side common password list (pre-loaded list of top weak passwords, fast validation)
- Q: How should generated profile photo thumbnails be persisted relative to ApplicationUser fields? → A: Store filenames/relative paths in the database while keeping original and resized images on disk
- Q: What maximum character length should apply to user bios? → A: Bio limited to 500 characters (fits UI while providing expressive space)
- Q: How long should audit log entries be retained before archival/purge? → A: Retain for 1 year then archive or purge

## User Scenarios & Testing *(mandatory)*

### User Story 1 - User Registration with Profile Photo (Priority: P1)

A new visitor wants to create an account with a complete profile including a photo to personalize their experience and build credibility when creating collections and reviews.

**Why this priority**: First impression matters - allowing users to establish a complete identity during registration increases engagement and perceived value of the platform for Demo visitors.

**Independent Test**: Can be fully tested by navigating to registration page, completing form with photo upload, and verifying account creation with photo displayed in profile - delivers immediate user identity value.

**Acceptance Scenarios**:

1. **Given** a visitor on the registration page, **When** they complete the form with email, password, display name, and optional profile photo, **Then** account is created and they are logged in with their photo visible in the navigation
2. **Given** a user uploading a profile photo during registration, **When** the file exceeds size limits or is invalid format, **Then** clear error message displays with acceptable formats and size constraints
3. **Given** a user completing registration without a photo, **When** account is created, **Then** a default avatar placeholder displays and they can add a photo later
4. **Given** a user uploading a profile photo, **When** the upload completes, **Then** the image is resized/optimized automatically and stored securely with proper access controls

---

### User Story 2 - Profile Photo Management (Priority: P1)

An existing user wants to upload, update, or remove their profile photo to keep their profile current and represent themselves accurately.

**Why this priority**: Profile completeness drives engagement - users with photos are more likely to contribute reviews and create collections, directly benefiting Demo content quality.

**Independent Test**: Can be tested by logging in as existing user, navigating to profile edit, uploading/changing/removing photo, and verifying changes persist and display correctly across the site.

**Acceptance Scenarios**:

1. **Given** an authenticated user on their profile page, **When** they click "Change Photo" and upload a new image, **Then** the photo updates immediately with optimized version stored
2. **Given** a user with an existing photo, **When** they choose "Remove Photo", **Then** the photo is deleted and default avatar displays
3. **Given** a user viewing their profile after photo change, **When** they navigate to any page with their identity visible, **Then** the new photo displays consistently in navigation, reviews, and collections
4. **Given** a user uploading an image, **When** the file is an unusual format or orientation, **Then** the system automatically corrects orientation and converts to standard web format

---

### User Story 3 - Role-Based Access Control (Priority: P1)

Administrators need to manage site content, featured collections, user accounts, and moderate reviews while regular users have appropriate permissions for content creation.

**Why this priority**: Essential for Demo operations - enables administrators to curate featured content, handle moderation, and maintain site quality without manual database access.

**Independent Test**: Can be tested by logging in as Admin user, accessing admin-only features (user management, featured collection control, moderation tools), and verifying regular users cannot access these features.

**Acceptance Scenarios**:

1. **Given** an administrator logs in, **When** they navigate the site, **Then** they see additional menu options for user management, content moderation, and featured collection curation
2. **Given** a regular user logs in, **When** they attempt to access admin routes directly via URL, **Then** they receive access denied error and are redirected appropriately
3. **Given** an administrator on user management page, **When** they assign or remove Admin role from a user, **Then** changes take effect immediately and user's permissions update
4. **Given** an administrator reviewing user content through user management dashboard, **When** they take moderation action on a review or collection, **Then** actions are logged with timestamp and admin identity for audit trail

---

### User Story 4 - Enhanced Profile Editing (Priority: P2)

Users want to maintain complete, up-to-date profiles including display name, bio, email, and photo with validation and helpful guidance.

**Why this priority**: Profile completeness increases user investment in the platform and improves trust signals for other users viewing collections and reviews.

**Independent Test**: Can be tested by editing various profile fields, attempting invalid inputs, and verifying validation feedback and successful updates.

**Acceptance Scenarios**:

1. **Given** a user editing their profile, **When** they update display name and bio, **Then** changes save successfully with confirmation message and display immediately
2. **Given** a user entering an invalid email format, **When** they attempt to save, **Then** inline validation displays with specific guidance before form submission
3. **Given** a user with a long bio, **When** they type beyond character limit, **Then** real-time character counter displays remaining characters
4. **Given** a user updating their email, **When** the new email is already registered, **Then** clear error indicates email is in use and suggests recovery options

---

### User Story 5 - Admin User Management (Priority: P2)

Administrators need to view all users, search/filter by criteria, assign roles, and manage accounts to maintain platform quality and respond to support requests.

**Why this priority**: Enables Demo operators to handle user support, manage problematic accounts, and ensure proper role assignments without database access.

**Independent Test**: Can be tested by accessing admin user list, performing searches/filters, editing user roles and properties, and verifying changes persist.

**Acceptance Scenarios**:

1. **Given** an administrator on user management page, **When** they view the user list, **Then** they see paginated users with display name, email, join date, role, and recent activity
2. **Given** an administrator searching users, **When** they enter email or name criteria, **Then** results filter in real-time with matching users highlighted
3. **Given** an administrator viewing a user profile, **When** they access detailed view, **Then** they see user's collections, reviews, favorites, and activity history
4. **Given** an administrator managing a user account, **When** they disable/enable the account, **Then** user access is immediately affected and audit log records the action

---

### User Story 6 - Password Strength and Security (Priority: P2)

Users registering or changing passwords need clear guidance on requirements with real-time feedback to create secure credentials meeting industry standards.

**Why this priority**: Security is foundational - prevents weak passwords and reduces account compromise risk, protecting both users and Demo reputation.

**Independent Test**: Can be tested by attempting various password combinations during registration and password change, verifying strength indicator and requirement feedback.

**Acceptance Scenarios**:

1. **Given** a user entering a password during registration, **When** they type, **Then** real-time strength indicator displays with visual feedback (weak/medium/strong)
2. **Given** a user with a weak password, **When** they attempt to submit, **Then** clear requirements list shows unmet criteria (length, complexity, common password check)
3. **Given** a user creating a strong password, **When** they meet all requirements, **Then** positive confirmation displays and submit becomes available
4. **Given** a user entering a commonly used weak password (from top 10,000 list), **When** system validates against client-side password list, **Then** warning displays suggesting a stronger alternative

---

### User Story 7 - Email Verification (Priority: P3)

New users receive email verification to confirm account ownership, improving security and reducing spam/fake accounts.

**Why this priority**: Enhances platform integrity and email deliverability - verified emails ensure communication reliability for future features.

**Independent Test**: Can be tested by registering new account, checking for verification email, clicking verification link, and confirming account activation.

**Acceptance Scenarios**:

1. **Given** a user completes registration, **When** account is created, **Then** verification email sends immediately with secure time-limited token
2. **Given** a user with unverified email, **When** they attempt certain actions (posting reviews, creating public collections), **Then** they receive prompt to verify email first
3. **Given** a user clicking verification link, **When** token is valid, **Then** account activates and confirmation displays with invitation to complete profile
4. **Given** a user whose token expired, **When** they attempt verification, **Then** option to resend new verification email displays with clear instructions

---

### Edge Cases

- How does the Demo behave when a user uploads an extremely large image file (10MB+) or animated GIF as profile photo? System should reject files over configured limit (e.g., 5MB) with clear error message and suggested alternatives.
- What happens if an administrator attempts to remove their own Admin role? System should prevent this action and display warning that at least one Admin must exist, requiring another Admin to perform role changes.
- How does photo display work when local file storage is unavailable? System should fall back to default avatar without breaking page rendering and log storage errors for admin attention.
- What safeguards prevent a user from uploading inappropriate profile photos? Admins manually review user profiles through user management dashboard and can remove inappropriate photos with moderation actions logged in audit trail.
- What happens when a user changes their email to one that's already verified by another account? Clear error message displays indicating email is in use with suggestion to use account recovery if they own both accounts.
- How does the system handle concurrent Admin role changes to the same user? Last write wins with optimistic concurrency, and conflict notification prompts Admin to refresh and retry.
- What happens when profile photo storage grows large? System monitors disk usage for observability and alerting (per FR-016) without enforcing hard quotas initially; administrators receive alerts to manually review and apply cleanup policies based on actual usage patterns.

## Demo Surface & Dependencies *(mandatory)*

- **Affected Pages/Routes**: 
  - `/Account/Register` - Enhanced with profile photo upload field and improved validation
  - `/Account/Login` - Unchanged but profile photo displays after login
  - `/Account/Profile` - Profile editing page with photo upload/change/remove capabilities
  - `/Admin/Users` - New admin-only user management dashboard
  - `/Admin/Users/{id}` - New admin-only user detail/edit page
  - Navigation partial views - Updated to display user profile photo
  - All pages displaying user identity (collections, reviews) - Updated to show profile photos

- **Feature Flags / Configuration**: 
  - `FileUpload:MaxProfilePhotoSize` - Maximum file size in bytes (default 5242880 = 5MB)
  - `FileUpload:AllowedImageTypes` - Comma-separated allowed MIME types (default "image/jpeg,image/png,image/webp")
  - `FileUpload:ProfilePhotoPath` - Local file system storage path for profile photos (default "wwwroot/uploads/profiles")
  - `FileUpload:ThumbnailSizes` - JSON array of thumbnail dimensions to generate (default [64, 128, 256])
  - `Identity:RequireEmailVerification` - Enable/disable email verification requirement (default false for initial release)
  - `Identity:DefaultRoles` - Roles to seed on application start (User, Admin)
  - `Email:SmtpHost` - SMTP server hostname for sending verification emails
  - `Email:SmtpPort` - SMTP server port (default 587 for TLS)
  - `Email:SmtpUsername` - SMTP authentication username
  - `Email:SmtpPassword` - SMTP authentication password (store in user secrets/Key Vault)
  - `Email:FromAddress` - Sender email address for verification emails
  - `Email:FromName` - Sender display name (default "ArtSpark")
  - New database migration required for role assignments and photo storage fields

- **Shared Library Touchpoints**: 
  - No changes to Client/Agent/Console contracts
  - Demo-only feature using existing ASP.NET Core Identity infrastructure
  - File upload handling entirely within Demo project

- **Rollback Plan**: 
  - Feature can be disabled by reverting migration to remove new fields
  - Existing uploaded photos remain in storage but won't display if feature is rolled back
  - Role functionality can be disabled via configuration without data loss
  - Safe to deploy and revert without impact on existing user data or shared libraries

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Demo MUST allow users to upload profile photos during registration with file validation (format: JPEG/PNG/WebP, max size 5MB)
- **FR-002**: Demo MUST automatically resize and optimize uploaded profile photos to standard sizes (64px, 128px, 256px thumbnails) to improve performance using JPEG quality 85% for JPEG sources, lossless WebP encoding for WebP sources, and PNG-to-JPEG conversion for PNG uploads exceeding 1MB
- **FR-003**: Demo MUST display user profile photos consistently across all user-identity contexts (navigation, profile page, collection author, review author)
- **FR-004**: Demo MUST implement two roles (User, Admin) using ASP.NET Core Identity with proper authorization attributes on controllers/actions
- **FR-005**: Demo MUST provide Admin-only user management interface with search, filter, role assignment, and account status controls
- **FR-006**: Demo MUST implement real-time password strength indicator during registration and password change with visual feedback
- **FR-007**: Demo MUST validate password complexity (minimum 8 characters, uppercase, lowercase, number, special character) and check against client-side common password list (top 10,000 weak passwords) following OWASP guidelines
- **FR-008**: Demo MUST store uploaded profile photos in local file system with proper directory permissions preventing unauthorized file access and serve through authorized endpoints only
- **FR-009**: Demo MUST provide profile editing page allowing users to update display name, bio (limited to 500 characters with inline counter feedback), email, and profile photo
- **FR-010**: Demo MUST implement email verification workflow using SMTP configuration with secure time-limited tokens (24-hour expiration) and resend capability
- **FR-011**: System MUST persist role assignments in database with proper foreign key relationships to ApplicationUser
- **FR-012**: System MUST log all administrative actions (role changes, account modifications) with timestamp and admin identity for audit trail
- **FR-012a**: Audit log entries MUST be retained for 1 year then purged (hard delete from database) by automated background service; archival to external storage deferred to future enhancement
- **FR-013**: System MUST prevent administrators from removing their own Admin role while ensuring at least one Admin exists
- **FR-014**: System MUST handle file upload errors gracefully with clear user feedback (file too large, invalid format, storage failure)
- **FR-015**: Observability MUST include structured logging for registration events, role changes, photo uploads, and authentication failures
- **FR-016**: System MUST monitor disk storage usage for profile photos without enforcing hard quotas initially, with alerting when storage exceeds configurable thresholds (specified as absolute MB limit in `FileUpload:DiskUsageThresholdMB`; alert triggers at 80% of threshold via Serilog Warning and health check degradation)

### Key Entities

- **ApplicationUser**: Extended with ProfilePhotoFileName, ProfilePhotoThumbnail64, ProfilePhotoThumbnail128, ProfilePhotoThumbnail256 string fields (each storing relative file paths for disk-based images) and EmailVerified boolean field
- **IdentityRole**: Standard ASP.NET Core Identity roles (User, Admin) - no customization required
- **IdentityUserRole**: Standard junction table linking ApplicationUser to IdentityRole
- **AuditLog** (new): Tracks administrative actions with AdminUserId, TargetUserId, ActionType, Details, Timestamp fields for compliance and troubleshooting

## AI & Cultural Safeguards *(not applicable)*

This feature does not involve AI personas or content generation. Standard content moderation applies to user-uploaded profile photos through manual admin review queue (future enhancement).

## Observability & Testing Plan *(mandatory)*

- **Automated Tests**:
  - Unit tests for ProfilePhotoService (upload, resize, validate, delete)
  - Unit tests for RoleService (assign, remove, validate permissions)
  - Integration tests for registration flow with photo upload
  - Integration tests for profile edit workflow
  - Integration tests for admin user management operations
  - Authorization tests verifying Admin-only route protection
  - File validation tests for various formats, sizes, and edge cases
  - Target projects: WebSpark.ArtSpark.Tests (add new test classes: ProfilePhotoServiceTests, RoleManagementTests, UserManagementIntegrationTests)

- **Logging & Metrics**:
  - Serilog structured logging for all authentication events (login, registration, password change) with user ID and timestamp
  - Log profile photo uploads with file size, format, and processing time
  - Log role assignment changes with admin ID, target user ID, old role, new role, and timestamp
  - Log failed login attempts with username/email and failure reason for security monitoring
  - Track registration completion rates and drop-off points to identify UX issues
  - Monitor file upload failures and storage errors for operational alerting

- **Manual Validation**:
  - Register new account with profile photo and verify display across site
  - Edit existing profile including photo change and removal
  - Test role assignment as Admin and verify permission enforcement
  - Access admin pages as regular user and confirm denial
  - Upload various file formats and sizes to verify validation
  - Test email verification flow end-to-end
  - Verify password strength indicator with various password combinations
  - Test concurrent admin operations and verify conflict handling

## Documentation & Release Updates *(mandatory)*

- **Docs to Update**:
  - `README.md` (root) - Add section on user roles and profile features
  - `WebSpark.ArtSpark.Demo/README.md` - Document profile photo upload configuration, role management, and file storage setup
  - Create new `docs/User-Profile-Management.md` - Comprehensive guide for profile features, admin user management, and file upload configuration
  - Create new `docs/Role-Based-Access-Control.md` - Document role structure, permission assignments, and admin capabilities
  - Update `docs/Live-Testing-Checklist.md` - Add profile and role management test scenarios

- **Release Notes**:
  - "Enhanced User Registration: Users can now upload profile photos during registration and customize their profiles with bio and display name"
  - "Role-Based Administration: New Admin role with user management dashboard, role assignment, and moderation capabilities"
  - "Improved Security: Real-time password strength validation and optional email verification"
  - "Breaking Change: Database migration required - run `dotnet ef database update` before deployment"
  - "Configuration Required: Set FileUpload:ProfilePhotoPath and related settings in appsettings.json"

- **Post-Deployment Checks**:
  - Verify profile photo uploads work with different file types and sizes
  - Confirm admin routes are protected and accessible only to Admin role
  - Check file storage permissions and disk space utilization
  - Verify email verification emails are sending (if enabled)
  - Monitor error logs for authentication and file upload failures
  - Test user registration flow end-to-end on live site
  - Validate role assignments persist correctly after deployment

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users complete registration with profile photo in under 2 minutes with clear visual feedback throughout process
- **SC-002**: Profile photo uploads process and optimize in under 5 seconds for files up to 5MB with automatic retry on transient failures
- **SC-003**: Admin user management dashboard loads and displays all users in under 1 second with pagination supporting 10,000+ users
- **SC-004**: Zero unauthorized access to Admin-only features during security testing with comprehensive authorization coverage
- **SC-005**: Password strength validation provides real-time feedback within 200ms of user input with accurate security scoring
- **SC-006**: Profile photos display consistently across all user contexts (navigation, collections, reviews) with 95%+ cache hit rate
- **SC-007**: Documentation covers all configuration options, admin workflows, and troubleshooting scenarios with code examples
- **SC-008**: User registration completion rate increases by 15% after introducing profile photo capability (capture 30-day pre-deployment baseline via existing analytics; compare 30-day post-deployment completion rate defined as successful registrations / registration page visits)
- **SC-009**: 90% of uploaded profile photos successfully process and store without errors or manual intervention
- **SC-010**: All administrative actions (role changes, account modifications) are logged with complete audit trail for compliance reporting
