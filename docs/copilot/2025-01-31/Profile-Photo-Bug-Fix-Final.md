# Profile Photo Bug Fix - Final Resolution

## Date
2025-01-31

## Root Cause Found

### The Problem
Files were being created successfully but immediately deleted. The logs clearly showed:

```
[13:03:17 INF] Verifying files exist:
[13:03:17 INF]   Original: ... - Exists: True
[13:03:17 INF]   Thumb64: ... - Exists: True
[13:03:17 INF]   Thumb128: ... - Exists: True
[13:03:17 INF]   Thumb256: ... - Exists: True
[13:03:17 INF] Deleted 4 profile photo files for user ...
```

### The Bug
In `AccountController.cs` lines 230-254, the code flow was:

1. Upload new photo ? Files created ?
2. Check if old photo exists
3. **Delete ALL files matching userId pattern** ?
4. Update database with new filenames

The problem: `DeletePhotoAsync(user.Id)` uses the pattern `$"{userId}_*"` which matches BOTH old AND new files!

### The Fix
**Move the delete BEFORE the upload:**

```csharp
else if (model.NewProfilePhoto != null)
{
    // Delete old photo BEFORE uploading new one (if exists)
    if (!string.IsNullOrEmpty(user.ProfilePhotoFileName))
    {
        await _profilePhotoService.DeletePhotoAsync(user.Id);
    }

    var photoResult = await _profilePhotoService.UploadPhotoAsync(model.NewProfilePhoto, user.Id);
    if (photoResult.Success)
    {
        user.ProfilePhotoFileName = photoResult.FileName;
        user.ProfilePhotoThumbnail64 = photoResult.Thumbnail64;
        user.ProfilePhotoThumbnail128 = photoResult.Thumbnail128;
        user.ProfilePhotoThumbnail256 = photoResult.Thumbnail256;
        // ... rest of code
    }
}
```

## Database Storage Verification

? **CONFIRMED**: Images are NOT stored in database

The database only stores **filenames** (strings):
- `ProfilePhotoFileName` - varchar(260)
- `ProfilePhotoThumbnail64` - varchar(260)
- `ProfilePhotoThumbnail128` - varchar(260)
- `ProfilePhotoThumbnail256` - varchar(260)

Example values:
```
ProfilePhotoFileName: d6cda4ce-8683-4f75-9b53-71f0205fc6b9_f9b1b5fe-3884-4b37-9dab-e3a9583f2ce3.jpg
ProfilePhotoThumbnail64: d6cda4ce-8683-4f75-9b53-71f0205fc6b9_f9b1b5fe-3884-4b37-9dab-e3a9583f2ce3_64x64.jpg
```

The actual image bytes are stored in the file system at:
```
C:\websites\artspark\uploads\profiles\
```

## Testing After Fix

### Step 1: Restart Application
Fully stop and restart the application.

### Step 2: Upload Profile Photo
1. Login to application
2. Navigate to Profile page
3. Select an image
4. Click Save

### Step 3: Verify Files Exist
Check directory:
```powershell
Get-ChildItem "C:\websites\artspark\uploads\profiles" -File
```

You should see 4 files for your user:
- `{userId}_{guid}.jpg` (original)
- `{userId}_{guid}_64x64.jpg`
- `{userId}_{guid}_128x128.jpg`
- `{userId}_{guid}_256x256.jpg`

### Step 4: Verify Display
- Profile page shows photo
- Navigation bar shows photo (24px)
- Dropdown menu shows photo (20px)
- Artwork reviews show photo (32px)

### Step 5: Verify URLs Work
Navigate to artwork and add review, then check browser DevTools:
- Photo should load from `/uploads/profiles/{filename}`
- Should return 200 OK, not 404

### Step 6: Test Update Photo
Upload a different photo:
- Old files should be deleted first
- New files should be created
- Only 4 files in directory for your user

## Files Modified

1. **AccountController.cs** - Lines 230-254
   - Moved DeletePhotoAsync call before UploadPhotoAsync
   - Prevents deletion of newly uploaded files

## Additional Improvements Made During Investigation

### Enhanced Logging
Added comprehensive logging to:
- `ProfilePhotoService.UploadPhotoAsync()`
- `ProfilePhotoService.SaveOptimizedImageAsync()`
- `ProfilePhotoService.GenerateThumbnailAsync()`

Each method now logs:
- File paths
- File existence checks
- File sizes
- Success/failure status
- Exception details

### Diagnostic Endpoint
Created `DiagnosticsController.TestProfilePhotoSetup()`:
- Tests directory existence
- Tests write permissions
- Tests read permissions
- Tests delete permissions
- Shows current files in directory
- Available at: `/Diagnostics/TestProfilePhotoSetup` (Admin only)

### Fixed Original SaveOptimizedImageAsync Bug
- Fixed logic that tried to check file size before file existed
- Now saves file first, then checks size
- Returns actual saved filename (handles extension changes)

## Summary

The profile photo upload was working perfectly - files were being created with correct names, sizes, and in the correct location. However, the AccountController was immediately deleting them right after upload due to the delete happening AFTER the upload instead of BEFORE.

By simply moving the delete operation before the upload, the issue is resolved.

The logs were invaluable in diagnosing this - without the detailed logging showing "Deleted 4 profile photo files" right after "Verifying files exist: True", we wouldn't have found this subtle timing issue.
