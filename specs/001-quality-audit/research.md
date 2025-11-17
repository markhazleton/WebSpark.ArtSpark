# Research Findings: Quality Audit & Package Currency

## .NET CLI Build Diagnostic Capture
**Decision:** Use `dotnet build`/`dotnet test` with `-warnaserror` for analyzers plus structured MSBuild log parsing to enumerate errors, warnings, and informational diagnostics per project.
**Rationale:** The CLI already reflects compiler and analyzer output across the multi-project solution; enabling warning-as-error surfaces issues consistently while leaving audit tooling responsible for summarizing counts without halting remediation planning.
**Alternatives Considered:** Third-party analyzers (e.g., SonarQube) were considered but declined for this audit iteration to avoid new infrastructure; Visual Studio solution build logs were rejected because they are less reliable in headless CI environments.

## NuGet Package Currency Strategy
**Decision:** Leverage `dotnet list <csproj> package --outdated --include-transitive` across Demo, Agent, Client, and Console with JSON output to capture latest stable versions alongside severity metadata.
**Rationale:** The built-in CLI respects existing package sources, requires no new tooling, and surfaces direct and transitive dependencies—aligned with the audit goal of inventorying remediation backlog without forcing upgrades.
**Alternatives Considered:** External services like Renovate or Dependabot were deferred to keep the audit focused on documentation; manual NuGet web queries are too slow and error-prone for repeatable runs.

## npm Dependency Inventory
**Decision:** Detect `package.json` manifests and, when present, run `npm outdated --json` plus `npm audit --omit=dev` to document version drift and known vulnerabilities; if no manifests exist, log a "No npm workspaces detected" entry.
**Rationale:** This approach respects the spec requirement while adapting gracefully to repositories that may not currently use npm, ensuring future front-end assets are covered without adding redundant tooling today.
**Alternatives Considered:** Yarn/PNPM checks were considered unnecessary because the solution standardizes on npm when JavaScript dependencies are introduced; skipping npm checks entirely would violate the feature requirement.

## Serilog & Observability Verification
**Decision:** Extend the audit to confirm Serilog configuration files and Demo footer build metadata load without warnings by running the Demo project's diagnostics tests and scanning logs for configuration errors.
**Rationale:** Observability is a constitutional principle; verifying Serilog ensures that logging remains trustworthy after dependency updates and highlights misconfigurations early.
**Alternatives Considered:** Implementing live log replay or Application Insights queries requires production access and exceeds the audit scope; relying solely on manual review offers insufficient coverage.

## CI/CD Integration Pattern
**Decision:** Package the audit workflow as a reusable PowerShell script that can run locally and in GitHub Actions, emitting Markdown under `docs/copilot/YYYY-MM-DD/` and exit codes suitable for optional gating.
**Rationale:** A script keeps the process repeatable, supports Windows-based contributors, and aligns with existing repository automation; optional gating lets maintainers run the audit without blocking urgent fixes.
**Alternatives Considered:** Creating a dedicated GitHub Action was postponed to avoid new maintenance overhead; embedding steps directly in the pipeline YAML reduces reusability for local runs.
