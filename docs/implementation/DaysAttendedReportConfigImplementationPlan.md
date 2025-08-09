# Days Attended Report Configuration Setting - Implementation Plan

## Overview
This document outlines the implementation plan for adding a configuration setting to control whether the Days Attended Report is generated during the year-end reconciliation process. The setting will be a simple boolean flag in `appsettings.json` that defaults to `false` to maintain current functionality.

## Current State Analysis

### Existing Implementation
- **Days Attended Report**: Implemented in `/Reports/DaysAttendedReport.cs`
- **Integration Point**: Hardcoded generation in `ExcelServices.GenerateYearEndRecon()` method (lines 502-505)
- **Current Behavior**: Report is always generated during year-end reconciliation
- **Configuration**: Basic `appsettings.json` exists but no report-specific settings

### Current Integration Code Location
**File**: `SchoolDistrictBilling/Services/ExcelServices.cs` (lines 502-505)
```csharp
// Generate Days Attended Report (simplified version for testing)
var daysAttendedReport = new DaysAttendedReport(context, rootPath);
var daysAttendedFile = daysAttendedReport.Generate(criteria, schoolDistrictName);
fileNames.Add(daysAttendedFile);
```

## Implementation Requirements

### Functional Requirements
1. Add a boolean configuration setting to control Days Attended Report generation
2. Setting should be configurable via `appsettings.json`
3. Default value should be `false` (do not generate report by default)
4. Setting should be accessible throughout the application via dependency injection
5. Implementation should be minimal and non-intrusive

### Technical Requirements
1. Use ASP.NET Core's built-in configuration system
2. Maintain backward compatibility
3. No breaking changes to existing functionality
4. Minimal code changes required

## Implementation Steps

### Step 1: Update Configuration Files

#### 1.1 Update appsettings.json
**File**: `SchoolDistrictBilling/appsettings.json`

**Current Content:**
```json
{
  "ConnectionStrings": {
    "Db": "Server=localhost;Database=omni_test;Uid=test;Pwd=testpass"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

**Updated Content:**
```json
{
  "ConnectionStrings": {
    "Db": "Server=localhost;Database=omni_test;Uid=test;Pwd=testpass"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ReportSettings": {
    "GenerateDaysAttendedReport": false
  }
}
```

#### 1.2 Update appsettings.Development.json (Optional)
**File**: `SchoolDistrictBilling/appsettings.Development.json`

**Current Content:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

**Updated Content (Optional - for development testing):**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ReportSettings": {
    "GenerateDaysAttendedReport": true
  }
}
```

### Step 2: Update ExcelServices Class

#### 2.1 Modify GenerateYearEndRecon Method
**File**: `SchoolDistrictBilling/Services/ExcelServices.cs`

**Current Code (lines 502-505):**
```csharp
// Generate Days Attended Report (simplified version for testing)
var daysAttendedReport = new DaysAttendedReport(context, rootPath);
var daysAttendedFile = daysAttendedReport.Generate(criteria, schoolDistrictName);
fileNames.Add(daysAttendedFile);
```

**Updated Code:**
```csharp
// Generate Days Attended Report (if enabled in configuration)
var generateDaysAttendedReport = configuration.GetValue<bool>("ReportSettings:GenerateDaysAttendedReport", false);
if (generateDaysAttendedReport)
{
    var daysAttendedReport = new DaysAttendedReport(context, rootPath);
    var daysAttendedFile = daysAttendedReport.Generate(criteria, schoolDistrictName);
    fileNames.Add(daysAttendedFile);
}
```

#### 2.2 Update Method Signature
**Current Method Signature:**
```csharp
public static IEnumerable<string> GenerateYearEndRecon(AppDbContext context, string rootPath, ReportCriteriaView criteria)
```

**Updated Method Signature:**
```csharp
public static IEnumerable<string> GenerateYearEndRecon(AppDbContext context, string rootPath, ReportCriteriaView criteria, IConfiguration configuration)
```

### Step 3: Update YearEndReconController

#### 3.1 Inject IConfiguration and Pass to ExcelServices
**File**: `SchoolDistrictBilling/Controllers/YearEndReconController.cs`

**Current Constructor:**
```csharp
private IWebHostEnvironment _hostEnvironment;
private readonly AppDbContext _context;

public YearEndReconController(IWebHostEnvironment environment, AppDbContext db)
{
    _hostEnvironment = environment;
    _context = db;
}
```

**Updated Constructor:**
```csharp
private IWebHostEnvironment _hostEnvironment;
private readonly AppDbContext _context;
private readonly IConfiguration _configuration;

public YearEndReconController(IWebHostEnvironment environment, AppDbContext db, IConfiguration configuration)
{
    _hostEnvironment = environment;
    _context = db;
    _configuration = configuration;
}
```

**Current Generate Method Call:**
```csharp
var files = ExcelServices.GenerateYearEndRecon(_context, _hostEnvironment.WebRootPath, criteria);
```

**Updated Generate Method Call:**
```csharp
var files = ExcelServices.GenerateYearEndRecon(_context, _hostEnvironment.WebRootPath, criteria, _configuration);
```

### Step 4: Add Required Using Statements

#### 4.1 Update ExcelServices.cs
**File**: `SchoolDistrictBilling/Services/ExcelServices.cs`

Add to existing using statements:
```csharp
using Microsoft.Extensions.Configuration;
```

#### 4.2 Update YearEndReconController.cs
**File**: `SchoolDistrictBilling/Controllers/YearEndReconController.cs`

Add to existing using statements (if not already present):
```csharp
using Microsoft.Extensions.Configuration;
```

## Implementation Details

### Configuration Access Pattern
The implementation uses ASP.NET Core's built-in configuration system with the following pattern:
```csharp
var generateDaysAttendedReport = configuration.GetValue<bool>("ReportSettings:GenerateDaysAttendedReport", false);
```

**Benefits:**
- Uses hierarchical configuration key (`ReportSettings:GenerateDaysAttendedReport`)
- Provides default value (`false`) if setting is missing
- Type-safe boolean conversion
- No additional dependencies required

### Default Behavior
- **Default Value**: `false` (do not generate Days Attended Report)
- **Backward Compatibility**: Maintains current behavior where report was not originally part of the system
- **Override Capability**: Can be enabled per environment via configuration files

### Error Handling
- If configuration key is missing, defaults to `false`
- If configuration value is invalid, `GetValue<bool>()` will return `false`
- No additional error handling required due to safe defaults

## Testing Strategy

### Unit Testing
1. **Test Configuration Reading**
   - Verify correct reading of `true` value
   - Verify correct reading of `false` value
   - Verify default behavior when key is missing
   - Verify behavior with invalid configuration values

2. **Test Report Generation Logic**
   - Verify report is generated when setting is `true`
   - Verify report is NOT generated when setting is `false`
   - Verify file list includes/excludes Days Attended Report appropriately

### Integration Testing
1. **Test Year-End Reconciliation Process**
   - Test complete workflow with setting enabled
   - Test complete workflow with setting disabled
   - Verify ZIP file contents match configuration setting

### Manual Testing Scenarios
1. **Default Configuration** (no ReportSettings section)
   - Should NOT generate Days Attended Report
   - Should generate all other reports normally

2. **Explicit False Configuration**
   ```json
   "ReportSettings": {
     "GenerateDaysAttendedReport": false
   }
   ```
   - Should NOT generate Days Attended Report

3. **Explicit True Configuration**
   ```json
   "ReportSettings": {
     "GenerateDaysAttendedReport": true
   }
   ```
   - Should generate Days Attended Report

4. **Environment-Specific Override**
   - Production: `false` (in appsettings.json)
   - Development: `true` (in appsettings.Development.json)
   - Should respect environment-specific setting

## Deployment Considerations

### Configuration Management
1. **Production Deployment**
   - Ensure `appsettings.json` has appropriate default value
   - Document the new configuration option
   - Communicate change to system administrators

2. **Environment-Specific Settings**
   - Development environments can override to `true` for testing
   - Production environments should explicitly set desired value
   - Staging environments should match production configuration

### Rollback Strategy
If issues arise, the feature can be quickly disabled by:
1. Setting `GenerateDaysAttendedReport` to `false` in configuration
2. Restarting the application
3. No code changes required for rollback

## Documentation Updates

### Configuration Documentation
Update existing configuration documentation to include:
```markdown
### Report Settings
- `ReportSettings:GenerateDaysAttendedReport` (boolean, default: false)
  - Controls whether the Days Attended Report is generated during year-end reconciliation
  - Set to `true` to enable report generation
  - Set to `false` to disable report generation
```

### User Documentation
Update user documentation to explain:
- How to enable/disable the Days Attended Report
- Where to find the configuration setting
- Impact on year-end reconciliation process

## Risk Assessment

### Low Risk Changes
- Configuration-based feature toggle
- Maintains existing functionality by default
- No database schema changes
- No breaking API changes

### Potential Issues
1. **Configuration Typos**: Misspelled configuration keys will result in default behavior
2. **Missing Configuration**: Missing section will use default value (safe)
3. **Invalid Values**: Non-boolean values will default to `false` (safe)

### Mitigation Strategies
- Clear documentation of configuration key names
- Default values that maintain safe behavior
- Comprehensive testing of configuration scenarios

## Success Criteria

### Functional Success
- [ ] Configuration setting controls Days Attended Report generation
- [ ] Default behavior (false) maintains current functionality
- [ ] Setting can be overridden per environment
- [ ] No impact on other reports or functionality

### Technical Success
- [ ] Minimal code changes implemented
- [ ] No breaking changes introduced
- [ ] Configuration follows ASP.NET Core patterns
- [ ] Proper dependency injection usage

### Operational Success
- [ ] Easy to configure and deploy
- [ ] Clear documentation provided
- [ ] Rollback strategy available
- [ ] Monitoring and logging maintained

## Timeline

### Phase 1: Implementation (1-2 hours)
1. Update configuration files
2. Modify ExcelServices class
3. Update YearEndReconController
4. Add required using statements

### Phase 2: Testing (2-3 hours)
1. Unit tests for configuration reading
2. Integration tests for report generation
3. Manual testing of various scenarios
4. Verification of ZIP file contents

### Phase 3: Documentation (1 hour)
1. Update configuration documentation
2. Update user documentation
3. Create deployment notes

### Phase 4: Deployment (30 minutes)
1. Deploy to staging environment
2. Verify configuration behavior
3. Deploy to production environment
4. Monitor for issues

**Total Estimated Time: 4-6 hours**

## Post-Implementation

### Monitoring
- Monitor application logs for any configuration-related errors
- Verify report generation behavior matches configuration settings
- Track user feedback on the new configuration option

### Future Enhancements
This implementation provides a foundation for additional report configuration options:
- Individual report enable/disable flags
- Report format options
- Report scheduling settings
- Report retention policies

The simple boolean approach can be extended to a more comprehensive report configuration system if needed in the future.
