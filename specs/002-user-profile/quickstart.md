# Quickstart: Enhanced User Registration & Profile Management

## Prerequisites
- .NET 10 (Preview) SDK installed.
- ImageSharp (or chosen imaging package) added to `WebSpark.ArtSpark.Demo` project.
- SMTP credentials configured via user secrets (`dotnet user-secrets set`) for email verification.
- Local storage directory for profile photos accessible to the Demo (`wwwroot/uploads/profiles` by default) with read/write permissions.

## Setup Steps
1. **Restore & Build**
   ```powershell
   dotnet restore WebSpark.ArtSpark.sln
   dotnet build WebSpark.ArtSpark.Demo/WebSpark.ArtSpark.Demo.csproj
   ```
2. **Apply EF Core Migration** (after implementation)
   ```powershell
   dotnet ef migrations add AddProfilePhotoAndAuditLog --project WebSpark.ArtSpark.Demo
   dotnet ef database update --project WebSpark.ArtSpark.Demo
   ```
3. **Configure AppSettings**
   - `FileUpload:MaxProfilePhotoSize = 5242880`
   - `FileUpload:AllowedImageTypes = "image/jpeg,image/png,image/webp"`
   - `FileUpload:ProfilePhotoPath = "wwwroot/uploads/profiles"`
   - `FileUpload:ThumbnailSizes = [64,128,256]`
   - `Identity:RequireEmailVerification = true` (optional toggle)
   - SMTP settings (`Email:SmtpHost`, `Email:SmtpPort`, `Email:SmtpUsername`, `Email:SmtpPassword`, `Email:FromAddress`, `Email:FromName`)
4. **Seed Default Roles**
   - Ensure startup initializer creates `User` and `Admin` roles and assigns an initial Admin user from configuration or user secrets.
5. **Run Demo**
   ```powershell
   dotnet run --project WebSpark.ArtSpark.Demo
   ```

## Validation Checklist
- Register a new account with and without a profile photo; confirm thumbnails render in navigation and profile pages.
- Attempt oversize or unsupported file uploads and verify informative error messages.
- Edit profile bio and email; ensure 500-character limit and inline validation.
- Assign and revoke Admin role using the new admin dashboard; confirm access restrictions.
- View audit log entries for profile changes and role assignments.
- Trigger email verification flow and confirm tokens expire after 24 hours.
- Run automated tests:
  ```powershell
  dotnet test WebSpark.ArtSpark.Tests --filter "ProfilePhoto|RoleManagement|UserManagement"
  ```
