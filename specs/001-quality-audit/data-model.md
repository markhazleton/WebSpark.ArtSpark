# Data Model: Quality Audit & Package Currency

## Package Currency Entry
- **Identifier:** composite key of `SourceProject` + `PackageName`
- **Fields:**
  - `SourceProject` (string) — project file or npm workspace emitting the dependency
  - `PackageName` (string) — NuGet or npm package identifier
  - `CurrentVersion` (string) — version pinned in the project file or lock file
  - `LatestStableVersion` (string) — highest available stable release at audit time
  - `ReleaseAge` (TimeSpan) — elapsed time between latest and current versions (optional if data unavailable)
  - `Severity` (enum: UpToDate, Minor, Major, Security) — triage level based on version delta or advisories
  - `CompatibilityRisk` (string) — free-form notes about breaking changes or migration steps
  - `RecommendedAction` (string) — next step (e.g., "Upgrade to 5.0.2", "Investigate beta")
  - `Notes` (string) — additional context gathered during audit
- **Relationships:**
  - May reference multiple `Build Health Finding` entries when dependency upgrades unlock warning cleanup.

## Build Health Finding
- **Identifier:** deterministic hash of `SourceProject`, `FilePath`, `DiagnosticId`
- **Fields:**
  - `SourceProject` (string) — project emitting the diagnostic
  - `FilePath` (string) — relative path to affected file (if available)
  - `DiagnosticId` (string) — compiler or analyzer identifier (e.g., CS8602, CA2000)
  - `Severity` (enum: Error, Warning, Info)
  - `Message` (string) — diagnostic text
  - `LineNumber` (int?) — line location when present
  - `Category` (enum: Compiler, Analyzer, Build, Test)
  - `RecommendedFix` (string) — remediation guidance recorded during audit
  - `Owner` (string) — maintainer or team responsible for follow-up
  - `LinkedDependencies` (list<string>) — packages whose upgrade may resolve the finding
  - `Status` (enum: Open, Planned, Resolved, Deferred)
- **Relationships:**
  - Optional many-to-many association with `Safeguard Control Check` when diagnostics relate to AI compliance modules.

## Safeguard Control Check
- **Identifier:** control name (e.g., `PersonaPromptIntegrity`)
- **Fields:**
  - `ControlName` (string)
  - `Description` (string) — statement of expected behavior
  - `Evidence` (string) — link or reference to verification proof (test output, doc excerpt)
  - `Outcome` (enum: Pass, Fail, Needs Follow-Up)
  - `FollowUpAction` (string) — required remediation when outcome is not Pass
  - `Owner` (string) — responsible reviewer
  - `DueDate` (date?) — target for remediation completion when applicable
  - `RelatedComponents` (list<string>) — Demo pages/services impacted
- **Relationships:**
  - Aggregates supporting `Build Health Finding` entries when AI guardrails produce diagnostics.

## Audit Report (Document Aggregate)
- **Composition:** Contains ordered sections for Build Diagnostics, Dependency Currency, and Safeguards.
- **Fields:**
  - `ReportDate` (date)
  - `GeneratedBy` (string) — user or CI agent running the audit
  - `ExecutionDuration` (TimeSpan)
  - `Summary` (string) — high-level outcomes
  - `Diagnostics` (list<Build Health Finding>)
  - `Dependencies` (list<Package Currency Entry>)
  - `Safeguards` (list<Safeguard Control Check>)
  - `BacklogRecommendations` (list<string>) — ordered remediation suggestions for iteration planning
- **Notes:**
  - Stored as Markdown with consistent headings to support manual review and automated parsing.
