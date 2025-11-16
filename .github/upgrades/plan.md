# .NET 10 Upgrade Plan

## Executive Summary

This plan outlines the upgrade of the WebSpark.ArtSpark solution from .NET 9.0 to .NET 10.0 (Preview). The solution consists of 5 projects with various dependencies that need to be updated systematically.

**Upgrade Scope:**
- **Target Framework:** .NET 10.0 (Preview)
- **Projects:** 5 projects total
- **NuGet Packages:** 11 packages to update from version 9.0.6 to 10.0.0
- **Estimated Effort:** Medium (2-3 hours)
- **Risk Level:** Low-Medium (Preview version of .NET 10)

## 1. Pre-Upgrade Checklist

### 1.1 Prerequisites Verification
- [x] .NET 10 SDK installed and verified
- [x] No global.json conflicts detected
- [x] Git repository on upgrade branch: `upgrade-to-NET10`
- [x] Pending changes committed
- [ ] Backup created (recommended for production environments)

### 1.2 Project Analysis Summary

| Project | Current TFM | Target TFM | Packages to Update | Risk |
|---------|-------------|------------|-------------------|------|
| WebSpark.ArtSpark.Client | net9.0 | net10.0 | 0 | Low |
| WebSpark.ArtSpark.Agent | net9.0 | net10.0 | 0 | Low |
| WebSpark.ArtSpark.Console | net9.0 | net10.0 | 2 | Low |
| WebSpark.ArtSpark.Demo | net9.0 | net10.0 | 4 | Medium |
| WebSpark.ArtSpark.Tests | net9.0 | net10.0 | 5 | Low |

## 2. Upgrade Strategy

### 2.1 Upgrade Order (Dependency-Based)

Based on project dependencies, we'll upgrade in this order:

1. **Tier 1 (No dependencies):**
   - WebSpark.ArtSpark.Client
   - WebSpark.ArtSpark.Agent

2. **Tier 2 (Depends on Tier 1):**
   - WebSpark.ArtSpark.Console
   - WebSpark.ArtSpark.Demo

3. **Tier 3 (Depends on Tier 2):**
   - WebSpark.ArtSpark.Tests

### 2.2 NuGet Package Updates

#### Demo Project (WebSpark.ArtSpark.Demo)
- Microsoft.EntityFrameworkCore.Sqlite: 9.0.6 → 10.0.0
- Microsoft.EntityFrameworkCore.Tools: 9.0.6 → 10.0.0
- Microsoft.EntityFrameworkCore.InMemory: 9.0.6 → 10.0.0
- Microsoft.AspNetCore.Identity.EntityFrameworkCore: 9.0.6 → 10.0.0

#### Tests Project (WebSpark.ArtSpark.Tests)
- Microsoft.AspNetCore.Mvc.Testing: 9.0.6 → 10.0.0
- Microsoft.Extensions.Hosting: 9.0.6 → 10.0.0
- Microsoft.Extensions.DependencyInjection: 9.0.6 → 10.0.0
- Microsoft.Extensions.Logging: 9.0.6 → 10.0.0
- Microsoft.Extensions.Configuration: 9.0.6 → 10.0.0

#### Console Project (WebSpark.ArtSpark.Console)
- Microsoft.Extensions.Configuration.Json: 9.0.6 → 10.0.0
- Microsoft.Extensions.Logging.Console: 9.0.6 → 10.0.0

## 3. Detailed Upgrade Steps

### 3.1 Phase 1: Update Tier 1 Projects

#### Step 1.1: Update WebSpark.ArtSpark.Client
1. Update TargetFramework from `net9.0` to `net10.0`
2. Verify project loads without errors
3. No package updates required (all packages already compatible)

#### Step 1.2: Update WebSpark.ArtSpark.Agent
1. Update TargetFramework from `net9.0` to `net10.0`
2. Verify project loads without errors
3. No package updates required (packages already at version 10.0.0)

**Verification:**
- Both projects should load without errors
- No compilation errors expected at this stage

### 3.2 Phase 2: Update Tier 2 Projects

#### Step 2.1: Update WebSpark.ArtSpark.Console
1. Update TargetFramework from `net9.0` to `net10.0`
2. Update NuGet packages:
   - Microsoft.Extensions.Configuration.Json: 9.0.6 → 10.0.0
   - Microsoft.Extensions.Logging.Console: 9.0.6 → 10.0.0
3. Build project and verify no errors

#### Step 2.2: Update WebSpark.ArtSpark.Demo
1. Update TargetFramework from `net9.0` to `net10.0`
2. Update NuGet packages:
   - Microsoft.EntityFrameworkCore.Sqlite: 9.0.6 → 10.0.0
   - Microsoft.EntityFrameworkCore.Tools: 9.0.6 → 10.0.0
   - Microsoft.EntityFrameworkCore.InMemory: 9.0.6 → 10.0.0
   - Microsoft.AspNetCore.Identity.EntityFrameworkCore: 9.0.6 → 10.0.0
3. Build project and verify no errors

**Verification:**
- Both projects should build successfully
- No runtime errors expected for console project
- Demo web application should start without errors

### 3.3 Phase 3: Update Tier 3 Projects

#### Step 3.1: Update WebSpark.ArtSpark.Tests
1. Update TargetFramework from `net9.0` to `net10.0`
2. Update NuGet packages:
   - Microsoft.AspNetCore.Mvc.Testing: 9.0.6 → 10.0.0
   - Microsoft.Extensions.Hosting: 9.0.6 → 10.0.0
   - Microsoft.Extensions.DependencyInjection: 9.0.6 → 10.0.0
   - Microsoft.Extensions.Logging: 9.0.6 → 10.0.0
   - Microsoft.Extensions.Configuration: 9.0.6 → 10.0.0
3. Build project and verify no errors

**Verification:**
- Test project should build successfully
- All tests should be discoverable

## 4. Testing Strategy

### 4.1 Build Verification
1. Clean solution
2. Restore all NuGet packages
3. Build entire solution
4. Verify 0 errors, document any warnings

### 4.2 Unit Testing
1. Discover all tests in WebSpark.ArtSpark.Tests
2. Run all unit tests
3. Verify all tests pass
4. Document any failing tests for investigation

### 4.3 Integration Testing
1. Start WebSpark.ArtSpark.Demo web application
2. Verify application starts without errors
3. Test basic functionality:
   - Home page loads
   - API endpoints respond
   - Database operations work
   - Search functionality works
4. Test WebSpark.ArtSpark.Console application
5. Verify console app runs without errors

### 4.4 Semantic Kernel Agent Testing
1. Verify Semantic Kernel agents in WebSpark.ArtSpark.Agent still function
2. Test artwork chat functionality
3. Verify AI integrations work as expected

## 5. Known Issues and Breaking Changes

### 5.1 .NET 10 Breaking Changes
**.NET 10 is currently in PREVIEW status.** Key considerations:

1. **Preview Quality:** This version may have bugs and breaking changes
2. **API Changes:** Some APIs may change before final release
3. **Package Compatibility:** Third-party packages may not fully support .NET 10 yet
4. **Production Use:** Not recommended for production environments

### 5.2 Entity Framework Core 10.0
- Review EF Core 10.0 breaking changes documentation
- Database migrations may need regeneration
- Query behavior changes should be tested

### 5.3 ASP.NET Core 10.0
- Middleware pipeline changes
- Authentication/Authorization updates
- Minimal API improvements

## 6. Rollback Plan

If issues are encountered:

### 6.1 Immediate Rollback
```bash
git checkout LastGood
git branch -D upgrade-to-NET10
```

### 6.2 Selective Rollback
- Revert specific project files to net9.0
- Downgrade specific packages back to 9.0.6
- Rebuild and test

## 7. Post-Upgrade Tasks

### 7.1 Verification Checklist
- [ ] All projects build successfully
- [ ] All unit tests pass
- [ ] Web application starts and runs
- [ ] Console application runs
- [ ] No performance regressions
- [ ] Logging works correctly
- [ ] Database operations function properly

### 7.2 Documentation Updates
- [ ] Update README.md with .NET 10 requirement
- [ ] Update developer setup documentation
- [ ] Document any new .NET 10 features utilized
- [ ] Update CI/CD pipeline if needed

### 7.3 Final Commit
- Commit all changes with descriptive message
- Tag commit as: `upgrade-net10-complete`
- Push to remote repository

## 8. Risk Mitigation

### 8.1 Low Risk Items
- Project TFM updates (straightforward)
- Microsoft.Extensions packages (stable API)

### 8.2 Medium Risk Items
- Entity Framework Core updates (database operations)
- ASP.NET Core Identity updates (authentication)
- Preview version stability

### 8.3 Mitigation Strategies
1. **Incremental approach:** Update projects in dependency order
2. **Continuous testing:** Test after each tier completes
3. **Git safety:** All changes on separate branch
4. **Backup:** Current working state preserved on LastGood branch

## 9. Success Criteria

The upgrade will be considered successful when:

1. ✅ All 5 projects target .NET 10.0
2. ✅ All 11 NuGet packages updated to version 10.0.0
3. ✅ Solution builds with 0 errors
4. ✅ All unit tests pass
5. ✅ Web application runs without errors
6. ✅ Console application runs without errors
7. ✅ No runtime exceptions in basic functionality
8. ✅ All changes committed to upgrade branch

## 10. Timeline Estimate

| Phase | Duration | Description |
|-------|----------|-------------|
| Tier 1 Updates | 15 min | Update Client and Agent projects |
| Tier 2 Updates | 30 min | Update Console and Demo projects |
| Tier 3 Updates | 20 min | Update Tests project |
| Build & Test | 45 min | Full solution build and testing |
| Integration Test | 30 min | Manual testing of applications |
| Documentation | 20 min | Update docs and commit |
| **Total** | **2h 40m** | **Complete upgrade cycle** |

## 11. Notes

- **Agent Framework Migration:** A separate scenario for migrating from Semantic Kernel Agents to Agent Framework was detected. This should be considered as a separate, future upgrade after .NET 10 stabilizes.
- **WebSpark.HttpClientUtility:** Notice version differences (1.1.0 vs 2.1.1) across projects - may want to standardize in future.
- **Preview Warning:** .NET 10 is currently in preview. Production deployments should wait for RTM release.

---

**Plan Version:** 1.0  
**Created:** [Current Date]  
**Target Completion:** [Your Timeline]