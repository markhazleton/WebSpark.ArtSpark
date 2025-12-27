# Profile Photo URL Debugging Guide

## Issue RESOLVED
Profile photo URLs were returning 404 errors because files were not being saved to the physical directory during upload.

## Root Cause
Bug in `ProfilePhotoService.SaveOptimizedImageAsync()` method (line 195):
- Attempted to check `FileInfo(path).Length` before the file existed
- This would throw an exception or return 0, preventing file save
- Exception was caught silently in the upload process

## Fix Applied
Updated `SaveOptimizedImageAsync()` to:
1. Save the file first
2. Then check file size (for PNG to JPEG conversion)
3. Return the actual saved filename (handles extension changes)

Also updated:
- `UploadPhotoAsync()` - Use base filename and handle actual saved filenames
- `GenerateThumbnailAsync()` - Accept base filename and return actual saved filename

## Expected URL Format
```
/uploads/profiles/{userId}_{guid}_{size}x{size}.{ext}
```

Example:
```
/uploads/profiles/d6cda4ce-8683-4f75-9b53-71f0205fc6b9_3e143ea2-6004-4ddd-819f-0e3a4c44ac58_128x128.jpg
```

## Configuration Check

### 1. Verify appsettings.json Configuration
Check `appsettings.json` for the `FileUpload` section:
```json
"FileUpload": {
  "ProfilePhotoPath": "c:\\websites\\artspark\\uploads\\profiles",
  ...
}
```

### 2. Verify Physical Directory Exists
Check that the directory `c:\websites\artspark\uploads\profiles` exists.
After uploading a profile photo, you should see 4 files created.

### 3. Verify Static File Middleware (Program.cs lines 177-195)
```csharp
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(profileUploadsPath),
    RequestPath = "/uploads/profiles",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800");
    }
});
```

## Testing After Fix

### Step 1: Stop and restart the application
Hot reload may not be sufficient for this fix. Fully restart the app.

### Step 2: Upload a new profile photo
1. Login to the application
2. Navigate to Profile page
3. Upload a new profile photo (JPEG, PNG, or WebP)
4. Verify success message appears

### Step 3: Verify files created
Check `c:\websites\artspark\uploads\profiles` for 4 new files:
- `{userId}_{guid}.jpg` (original - may be converted to .jpg)
- `{userId}_{guid}_64x64.jpg`
- `{userId}_{guid}_128x128.jpg`
- `{userId}_{guid}_256x256.jpg`

### Step 4: Verify display
- Check navigation bar shows profile photo (24px)
- Check dropdown menu shows profile photo (20px)
- Navigate to an artwork and add a review - verify review shows profile photo (32px)

### Step 5: Verify database
Query to check database values:
```sql
SELECT Id, DisplayName, 
       ProfilePhotoFileName, 
       ProfilePhotoThumbnail64, 
       ProfilePhotoThumbnail128, 
       ProfilePhotoThumbnail256 
FROM AspNetUsers 
WHERE ProfilePhotoThumbnail64 IS NOT NULL;
```

## Code Flow After Fix

### Upload Process
1. User uploads photo via Profile page
2. `ProfilePhotoService.UploadPhotoAsync()` is called
3. Image is loaded and metadata stripped
4. `SaveOptimizedImageAsync()` saves original:
   - PNG > 1MB ? converted to JPEG
   - Other formats ? saved as JPEG
   - WebP ? saved as WebP
5. Three thumbnails generated at 64, 128, and 256 pixels
6. Each thumbnail saved with actual filename returned
7. Database updated with all saved filenames
8. Files physically exist in `c:\websites\artspark\uploads\profiles`

### Display Process
1. `ProfilePhotoViewComponent` retrieves user from UserManager
2. Selects appropriate thumbnail based on size:
   - <= 64px: use Thumbnail64
   - <= 128px: use Thumbnail128 or fallback to Thumbnail64
   - > 128px: use Thumbnail256 or fallback chain
3. Calls `ProfilePhotoService.GetPhotoUrl(filename)` which returns `/uploads/profiles/{filename}`
4. Browser requests the file from that URL
5. Static file middleware serves from physical directory

## Files Modified in Fix
- `WebSpark.ArtSpark.Demo\Services\ProfilePhotoService.cs`
  - `SaveOptimizedImageAsync()` - Fixed file size check logic, return actual filename
  - `UploadPhotoAsync()` - Use base filename, handle actual saved filenames
  - `GenerateThumbnailAsync()` - Accept base filename, return actual saved filename
