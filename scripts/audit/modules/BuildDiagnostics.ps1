<#
.SYNOPSIS
    Build diagnostics collection for WebSpark.ArtSpark
.DESCRIPTION
    Runs dotnet build and dotnet test to collect compiler warnings, errors, and test failures
#>

<#
.SYNOPSIS
    Invoke build diagnostics for the solution
.DESCRIPTION
    Runs dotnet build and dotnet test, parsing output to generate BuildHealthFinding objects
#>
function Invoke-BuildDiagnostics {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$RepoRoot,
        
        [string]$SolutionPath = "WebSpark.ArtSpark.sln"
    )

    $findings = @()
    $fullSolutionPath = Join-Path $RepoRoot $SolutionPath

    if (-not (Test-Path $fullSolutionPath)) {
        Write-Warning "Solution file not found: $fullSolutionPath"
        return $findings
    }

    Write-Host "  Running dotnet build..." -ForegroundColor Gray

    try {
        # Run build and capture output
        $buildOutput = & dotnet build $fullSolutionPath --no-restore 2>&1
        $buildExitCode = $LASTEXITCODE

        # Parse build warnings and errors
        foreach ($line in $buildOutput) {
            $lineStr = $line.ToString()
            
            # Match pattern: path(line,col): warning/error CODE: message [project]
            # Example: C:\Path\File.cs(5,23): warning CS8618: Message... [Project.csproj]
            if ($lineStr -match '^\s*(.+?)\((\d+),\d+\):\s+(warning|error)\s+([A-Z0-9]+):\s+(.+?)\s+\[(.+?\.csproj)\]') {
                $filePath = $matches[1].Trim()
                $lineNumber = [int]$matches[2]
                $severity = if ($matches[3] -eq 'error') { 'Error' } else { 'Warning' }
                $diagnosticId = $matches[4]
                $message = $matches[5].Trim()
                $projectPath = $matches[6]
                $projectName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)

                # Make file path relative to repo root
                if ($filePath.StartsWith($RepoRoot)) {
                    $filePath = $filePath.Substring($RepoRoot.Length + 1)
                }

                $finding = [PSCustomObject]@{
                    SourceProject = $projectName
                    FilePath = $filePath
                    LineNumber = $lineNumber
                    DiagnosticId = $diagnosticId
                    Severity = $severity
                    Category = 'Compiler'
                    Message = $message
                    RecommendedFix = Get-RecommendedFix -DiagnosticId $diagnosticId
                    Owner = ''
                    LinkedDependencies = @()
                    Status = 'Open'
                }

                $findings += $finding
            }
        }

        Write-Host "  Found $($findings.Count) build diagnostics" -ForegroundColor Gray

        # Run tests
        Write-Host "  Running dotnet test..." -ForegroundColor Gray
        $testOutput = & dotnet test $fullSolutionPath --no-build --verbosity quiet 2>&1
        $testExitCode = $LASTEXITCODE

        if ($testExitCode -ne 0) {
            $finding = [PSCustomObject]@{
                SourceProject = 'WebSpark.ArtSpark.Tests'
                FilePath = ''
                LineNumber = 0
                DiagnosticId = 'TEST_FAILURE'
                Severity = 'Error'
                Category = 'Test'
                Message = 'One or more tests failed'
                RecommendedFix = 'Review test output and fix failing tests'
                Owner = ''
                LinkedDependencies = @()
                Status = 'Open'
            }
            $findings += $finding
        }

    }
    catch {
        Write-Warning "Build diagnostics failed: $_"
    }

    return $findings
}

function Get-RecommendedFix {
    param([string]$DiagnosticId)

    $fixes = @{
        'CS8618' = 'Add required modifier, make property nullable, or initialize in constructor'
        'CS8602' = 'Add null check before dereferencing'
        'CS8603' = 'Return non-null value or change return type to nullable'
        'CS8604' = 'Provide non-null argument or change parameter type to nullable'
        'CA1062' = 'Add null validation for public method parameters'
        'CA2000' = 'Dispose object before losing scope'
    }

    if ($fixes.ContainsKey($DiagnosticId)) {
        return $fixes[$DiagnosticId]
    }

    return 'Review compiler documentation for guidance'
}
