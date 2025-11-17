# Data Model

## ApplicationUser (existing, extended)
- `ProfilePhotoFileName` (string, nullable, max 260) — generated safe file name for original image stored on disk.
- `ProfilePhotoThumbnail64` (string, nullable, max 260) — relative path to 64px thumbnail file.
- `ProfilePhotoThumbnail128` (string, nullable, max 260) — relative path to 128px thumbnail file.
- `ProfilePhotoThumbnail256` (string, nullable, max 260) — relative path to 256px thumbnail file.
- `Bio` (string, nullable, max 500) — user-authored bio shown on profile pages.
- `EmailVerified` (bool, default false) — indicates email confirmation status.
- Existing Identity fields retained (UserName, NormalizedEmail, ConcurrencyStamp, etc.).
- `[PersonalData]` attribute applied to new personal fields to support export/delete workflows.

**Validation Rules**
- Bio limited to 500 characters with server-side enforcement.
- Profile photo uploads limited to 5 MB and MIME types `image/jpeg`, `image/png`, `image/webp`.
- Photo thumbnails must exist or be null in sync with original photo presence (service maintains invariants).
- EmailVerified toggled only after token validation.

**Relationships**
- `ApplicationUser` (Admin) → `AuditLog` (CreatedBy) : one-to-many (optional).
- `ApplicationUser` (Target) → `AuditLog` (TargetUser) : one-to-many (optional).
- `ApplicationUser` ↔ `IdentityRole` via standard `AspNetUserRoles` join table.

## AuditLog (new table)
- `Id` (long, identity).
- `CreatedAtUtc` (DateTime, required) — timestamp of action.
- `ActionType` (string, required, max 100) — normalized action name (`RoleAssigned`, `ProfilePhotoRemoved`, etc.).
- `Details` (string, nullable) — JSON payload capturing contextual metadata.
- `AdminUserId` (string, FK → ApplicationUser.Id, required) — actor performing the change.
- `TargetUserId` (string, FK → ApplicationUser.Id, nullable) — subject affected by the action (self or other).
- `CorrelationId` (string, nullable, max 64) — links to Serilog correlation.
- `RowVersion` (rowversion) — optimistic concurrency token.

**Constraints & Indexes**
- `IX_AuditLog_AdminUserId_CreatedAtUtc` (composite) for filtering admin activity.
- `IX_AuditLog_TargetUserId_CreatedAtUtc` for auditing target histories.
- `Details` stored as JSON string (respecting SQLite size limits); truncated or summarized if >4 KB.
- `OnDelete: Restrict` for both FK relationships to preserve history when users are removed (soft-delete workflow required for physical removal).

## IdentityRole (existing)
- Seeded roles: `User`, `Admin`.
- Additional metadata unchanged.

## Derived View Models / DTOs
- `UserSummaryDto`: user id, display name, normalized email, roles, profile photo thumbnail path(s), join date, email verified flag, last activity timestamp.
- `AuditLogEntryDto`: action type, admin display name, target display name, created timestamp, parsed detail payload.

## State Transitions
1. **Registration**: User record created with `EmailVerified = false`, no photo paths set, role `User` assigned.
2. **Email Confirmation**: Confirmation token validated → `EmailVerified = true`; audit entry recorded.
3. **Profile Photo Upload**: Photo service saves originals/thumbnails, updates all path fields atomically, emits audit entry.
4. **Photo Removal**: Service deletes disk files, nulls path fields, records audit entry.
5. **Role Assignment**: Admin assigns/removes roles via `UserManager` → audit entry with `ActionType` `RoleAssigned`/`RoleRevoked` and updates junction table.
6. **Admin Disable User**: (If implemented) flag stored either in existing Identity lockout fields; audit entry captured.

## Data Retention & Cleanup
- Profile photo files retained while associated path fields populated; deletion routine keeps DB and disk consistent.
- Audit logs retained for 1 year, then archived/purged via background job (future work) keyed by `CreatedAtUtc`.
- Disk usage monitored via scheduled service leveraging configured thresholds.
