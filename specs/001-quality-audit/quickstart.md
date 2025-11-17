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

## 2. Run the Quality Audit Script (prototype)
```powershell
pwsh -File scripts/audit/run-quality-audit.ps1
```
- Produces Markdown output under `docs/copilot/<ISO-DATE>/quality-audit.md`
- Fails fast if another audit execution is in progress

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
