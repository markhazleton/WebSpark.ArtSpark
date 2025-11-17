# Quickstart: Quality Audit & Package Currency

## Prerequisites
- .NET 9 SDK installed (`dotnet --version` ≥ 9.0.0)
- PowerShell 7+ (Windows) or pwsh available on other platforms
- npm 10+ installed if JavaScript packages are present
- Access to private package feeds (if the solution uses them)

## 1. Restore Solution Dependencies
```powershell
pwsh -Command "dotnet restore WebSpark.ArtSpark.sln"
```

## 2. Run the Quality Audit Script
```powershell
pwsh -File scripts/audit/run-quality-audit.ps1
```
- Produces Markdown output under `docs/copilot/<ISO-DATE>/quality-audit.md`
- Generates prioritized backlog in `docs/copilot/<ISO-DATE>/quality-audit-backlog.md`

### Backlog Filtering Options
```powershell
# Filter by severity and limit items
pwsh -File scripts/audit/run-quality-audit.ps1 -Severity Warning -MaxItems 10

# Skip specific audit phases
pwsh -File scripts/audit/run-quality-audit.ps1 -SkipBuild -SkipDependencies

# Generate backlog for errors only
pwsh -File scripts/audit/run-quality-audit.ps1 -Severity Error
```

**Parameters**:
- `-Severity`: Filter backlog items (All, Error, Warning, Info) - default: All
- `-MaxItems`: Limit backlog size (0 = unlimited) - default: 0
- `-SkipBuild`: Skip build diagnostics collection
- `-SkipDependencies`: Skip dependency currency checks
- `-SkipSafeguards`: Skip safeguard audits
- `-OutputDirectory`: Custom output path for reports

## 3. Inspect the Markdown Report
- Section **Build Diagnostics** lists errors/warnings/analyzers grouped by project
- Section **Dependency Currency** shows NuGet/npm deltas with recommended actions
- Section **Safeguard Controls** summarizes AI compliance checks and outcomes

## 4. Plan Remediation
- Use the **Backlog Recommendations** table to prioritize fixes for the next Demo release
- Copy planned items into `docs/Documentation-Update-Summary.md` and the feature backlog

## 5. Optional: Trigger via Internal API
- POST `https://artspark.markhazleton.com/internal/quality-audit/run` (bearer token required)
- Poll `GET /internal/quality-audit/reports/{reportId}` until the report status is available

## 6. Verify Observability Artifacts
- Confirm Demo footer displays updated build metadata after remediation
- Check Serilog logs for configuration warnings flagged by the audit

## Next Steps
- Align remediation tasks with `/specs/001-quality-audit/tasks.md` once generated
- Schedule recurring audit runs (e.g., weekly) using CI once prototype script stabilizes
