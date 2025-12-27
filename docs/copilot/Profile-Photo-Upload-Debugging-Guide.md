# Profile Photo Upload Debugging - Step by Step

## Quick Start Testing

### FIRST: Run Diagnostics
1. **Restart your application** (full restart, not hot reload)
2. **Login as Admin**
3. **Navigate to**: `https://localhost:7282/Diagnostics/TestProfilePhotoSetup`
4. **Review the output** - this will tell you if:
   - Directory exists
   - Directory is writable
   - Directory has correct permissions
   - Configuration is loaded correctly

### If Diagnostics Shows Issues:
Fix those first before proceeding with profile photo upload testing.

## Current Situation
- Files are NOT being saved to `C:\websites\artspark\uploads\profiles`
- Profile page shows image during preview but broken link after save
- Need to diagnose where the process is failing

## Enhanced Logging Added

All these methods now have detailed logging:
- `ProfilePhotoService.UploadPhotoAsync()`
- `ProfilePhotoService.SaveOptimizedImageAsync()`
- `ProfilePhotoService.GenerateThumbnailAsync()`

Each step logs:
- File paths being used
- File sizes and existence checks
- Success/failure of each operation
- Exception details if anything fails

## Testing Steps with Enhanced Logging

### Step 1: Stop and Restart Application
**IMPORTANT**: Fully stop and restart the application (not hot reload)

### Step 2: Check Log Location
Logs are written to:
```
c:\websites\artspark\Logs\artspark-YYYYMMDD.txt
```

For example, today's log would be:
```
c:\websites\artspark\Logs\artspark-20250131.txt
```

### Step 3: Clear/Monitor Logs
1. Before testing, either:
   - Delete the current log file, OR
   - Note the current file size to find new entries
2. Keep the log file open in a text editor

### Step 4: Attempt Profile Photo Upload

1. **Login to application**
2. **Navigate to Profile page** (`/Account/Profile`)
3. **Select an image** (try a JPEG first for simplicity)
4. **Click Save**
5. **Immediately check the log file**

### Step 5: Review Log Output

Look for these specific log entries in sequence:

#### Expected Log Sequence:

```
[Information] Starting profile photo upload for user {UserId}. Upload path: C:\websites\artspark\uploads\profiles

[Information] Validation passed for user {UserId}. File: {FileName}, Size: {Size}, Type: {ContentType}

[Information] Generated filename: {fileName}, Full path: {fullPath}

[Information] Image loaded successfully. Dimensions: {Width}x{Height}

[Information] About to save original image to: {Path}

[Information] SaveOptimizedImageAsync called. Path: {Path}, ContentType: {ContentType}

[Information] Converting to JPEG: {Path}
  OR
[Information] Saving as PNG: {Path}
  OR
[Information] Saving as WebP: {Path}

[Information] JPEG saved: {Path}, File exists: True, Size: {fileSize}

[Information] Original image saved as: {FileName}

[Information] Generating thumbnail 64x64

[Information] GenerateThumbnailAsync called. Size: 64, BaseFileName: {baseFileName}, ContentType: {contentType}

[Information] Creating thumbnail at: {Path}

[Information] Thumbnail image cloned and resized to 64x64

[Information] SaveOptimizedImageAsync called. Path: {Path}, ContentType: {ContentType}

[Information] JPEG saved: {Path}, File exists: True, Size: {fileSize}

[Information] Thumbnail saved as: {FileName}

... (repeat for 128x128 and 256x256 thumbnails)

[Information] Profile photo uploaded successfully for user {UserId}. Original: {fileName}, Thumbnails: {t64}, {t128}, {t256}

[Information] Verifying files exist:
[Information]   Original: {path} - Exists: True
[Information]   Thumb64: {path} - Exists: True
[Information]   Thumb128: {path} - Exists: True
[Information]   Thumb256: {path} - Exists: True

[Information] Profile photo updated for user {UserId}
```

### Step 6: Diagnose Based on Log Output

#### Scenario A: No log entries at all
**Problem**: Request not reaching the service
**Check**:
- Is the file being submitted in the form?
- Check browser Network tab for the POST request
- Check ModelState validation errors

#### Scenario B: Validation fails
**Look for**: `Profile photo validation failed`
**Problem**: File doesn't meet requirements
**Check**:
- File size > 5MB?
- File type not JPEG, PNG, or WebP?
- File corrupted?

#### Scenario C: Exception during upload
**Look for**: `Error uploading profile photo` with exception details
**Problem**: Exception during processing
**Check**: The exception message and stack trace in logs

#### Scenario D: Files marked as "Exists: False"
**Problem**: Files being saved but then disappearing
**Possible causes**:
- Permissions issue (no write access)
- Antivirus blocking/deleting files
- Different process cleaning up the directory

#### Scenario E: Files marked as "Exists: True" but not in directory
**Problem**: Files being saved to different location than expected
**Check**:
- The actual path in the log vs expected path
- Search entire C: drive for the filename pattern

### Step 7: Manual Directory Check

After upload attempt, immediately run PowerShell:

```powershell
# List all files in the uploads directory
Get-ChildItem "C:\websites\artspark\uploads\profiles" -File

# Search for any files matching user ID pattern
Get-ChildItem "C:\websites\artspark\uploads\profiles" -Filter "*{yourUserId}*" -File

# Check directory permissions
Get-Acl "C:\websites\artspark\uploads\profiles" | Format-List

# Check if directory is writable
Test-Path "C:\websites\artspark\uploads\profiles" -PathType Container
```

### Step 8: Check Database Values

After upload, query the database:

```sql
SELECT 
    Id, 
    DisplayName, 
    ProfilePhotoFileName, 
    ProfilePhotoThumbnail64, 
    ProfilePhotoThumbnail128, 
    ProfilePhotoThumbnail256,
    CreatedAt,
    EmailVerified
FROM AspNetUsers 
WHERE Id = '{yourUserId}';
```

**Expected Results**:
- `ProfilePhotoFileName`: `{userId}_{guid}.jpg`
- `ProfilePhotoThumbnail64`: `{userId}_{guid}_64x64.jpg`
- `ProfilePhotoThumbnail128`: `{userId}_{guid}_128x128.jpg`
- `ProfilePhotoThumbnail256`: `{userId}_{guid}_256x256.jpg`

## Common Issues and Solutions

### Issue: Permission Denied
**Symptoms**: Exception with "Access denied" or "UnauthorizedAccessException"
**Solution**: 
```powershell
# Grant full control to the directory
icacls "C:\websites\artspark\uploads\profiles" /grant Users:F /T
```

### Issue: Directory Doesn't Exist
**Symptoms**: Exception with "DirectoryNotFoundException"
**Solution**: Service should create it automatically, but manually create if needed:
```powershell
New-Item -Path "C:\websites\artspark\uploads\profiles" -ItemType Directory -Force
```

### Issue: Antivirus Blocking
**Symptoms**: Files momentarily exist then disappear
**Solution**: Add exclusion for uploads directory in antivirus

### Issue: Path Too Long
**Symptoms**: Exception with "PathTooLongException"
**Solution**: Shorten the ProfilePhotoPath in appsettings.json

### Issue: Disk Full
**Symptoms**: Exception with "IOException" about disk space
**Solution**: Free up disk space

## Next Steps After Diagnosis

Once you've run through these steps, share:

1. **Relevant log entries** (copy from log file)
2. **Any exceptions or errors** from the logs
3. **PowerShell command outputs** (especially Get-ChildItem)
4. **Database query results** for your user
5. **Browser Network tab** showing the POST request details

With this information, we can pinpoint the exact issue and fix it.
