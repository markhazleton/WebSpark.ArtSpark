# Research Findings

## Identity Extension & Role Management
Decision: Extend `ApplicationUser` with `[PersonalData]`-annotated profile photo metadata and manage disk-stored photo paths, while seeding `User` and `Admin` roles plus an initial admin assignment during startup using `RoleManager`/`UserManager`.
Rationale: Microsoft Identity guidance recommends storing only sanitized file metadata within the Identity store, keeping binaries on disk, and using scoped initializers to seed roles idempotently while respecting security policies (password complexity, email confirmation).
Alternatives considered: Seed roles via EF model seeding (less flexible, harder to manage secrets); store photos as database blobs (increases table bloat and complicates file serving).

## EF Core Schema & Query Strategy
Decision: Introduce migrations that add nullable profile photo path columns to `AspNetUsers` and create a normalized `AuditLog` table with FK links to users, while using no-tracking, paginated queries for admin dashboards.
Rationale: EF Core best practices favor incremental migrations with nullable additions to avoid downtime, and AsNoTracking projection-based queries provide scalable admin list performance; audit logs need explicit indexing and retention to support compliance.
Alternatives considered: Embed audit data in JSON columns on `AspNetUsers` (limits queryability); perform user paging with `UserManager` per-row calls (causes N+1 query overhead).

## Image Processing Pipeline
Decision: Use ImageSharp to validate uploads (`Image.Identify`), enforce file size and dimension limits, auto-correct EXIF orientation, strip metadata, and generate 64/128/256px thumbnails from a single decoded source with GUID-based filenames stored outside web root.
Rationale: ImageSharp security guidance emphasizes header sniffing, `AutoOrient`, memory limiters, and cloning workflows to avoid DoS and EXIF exploits while generating consistent derivatives.
Alternatives considered: Rely on client-side resizing only (cannot enforce integrity); use System.Drawing (unsupported cross-platform, security concerns).

## Serilog Observability Enhancements
Decision: Emit structured Serilog events for registration, uploads, role changes, and email verification with contextual properties (`UserIdHash`, `Action`, `FileSizeBytes`, `ProcessingDurationMs`, `AuditCorrelationId`) while masking personally identifiable data and leveraging `LogContext` scopes.
Rationale: Serilog structured logging and ASP.NET scopes enable traceable, privacy-conscious telemetry that aligns with Microsoft logging guidance for production apps.
Alternatives considered: Plain string logs (hard to query); logging raw filenames or emails (privacy risk and higher compliance exposure).
