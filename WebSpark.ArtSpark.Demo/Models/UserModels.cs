using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebSpark.ArtSpark.Demo.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [PersonalData]
    [MaxLength(500)]
    public string? Bio { get; set; }
    
    [PersonalData]
    [MaxLength(260)]
    public string? ProfilePhotoFileName { get; set; }
    
    [PersonalData]
    [MaxLength(260)]
    public string? ProfilePhotoThumbnail64 { get; set; }
    
    [PersonalData]
    [MaxLength(260)]
    public string? ProfilePhotoThumbnail128 { get; set; }
    
    [PersonalData]
    [MaxLength(260)]
    public string? ProfilePhotoThumbnail256 { get; set; }
    
    [Obsolete("Use ProfilePhotoThumbnail* properties instead")]
    public string? ProfileImageUrl { get; set; }
    
    public bool EmailVerified { get; set; } = false;

    // Navigation properties
    public virtual ICollection<ArtworkReview> Reviews { get; set; } = new List<ArtworkReview>();
    public virtual ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();
    public virtual ICollection<UserCollection> Collections { get; set; } = new List<UserCollection>();
}

public class AuditLog
{
    public long Id { get; set; }
    
    [Required]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    [Required]
    [MaxLength(100)]
    public string ActionType { get; set; } = string.Empty;
    
    [MaxLength(4000)]
    public string? Details { get; set; }
    
    [Required]
    [MaxLength(450)]
    public string AdminUserId { get; set; } = string.Empty;
    
    [MaxLength(450)]
    public string? TargetUserId { get; set; }
    
    [MaxLength(64)]
    public string? CorrelationId { get; set; }
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser AdminUser { get; set; } = null!;
    public virtual ApplicationUser? TargetUser { get; set; }
}

public class ArtworkReview
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ArtworkId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? ReviewText { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
}

public class UserFavorite
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ArtworkId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
}

public class UserCollection
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // SEO and metadata fields
    public string Slug { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? SocialImageUrl { get; set; }

    // Content fields
    public string? LongDescription { get; set; }
    public string? CuratorNotes { get; set; }
    public string? Tags { get; set; } // Comma-separated tags
    public int ViewCount { get; set; } = 0;
    public bool IsFeatured { get; set; } = false;
    public DateTime? FeaturedUntil { get; set; }    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<CollectionArtwork> Artworks { get; set; } = new List<CollectionArtwork>();
    public virtual ICollection<CollectionContentSection> ContentSections { get; set; } = new List<CollectionContentSection>();
    public virtual ICollection<CollectionMedia> MediaItems { get; set; } = new List<CollectionMedia>();
    public virtual ICollection<CollectionLink> Links { get; set; } = new List<CollectionLink>();
}

public class CollectionArtwork
{
    public int Id { get; set; }
    public int CollectionId { get; set; }
    public int ArtworkId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Enhanced fields for collection items
    public string? Slug { get; set; }
    public string? CustomTitle { get; set; }
    public string? CustomDescription { get; set; }
    public string? CuratorNotes { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsHighlighted { get; set; } = false;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    // Navigation properties
    public virtual UserCollection Collection { get; set; } = null!;
}

// New model for rich content sections within collections
public class CollectionContentSection
{
    public int Id { get; set; }
    public int CollectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; public string SectionType { get; set; } = "text"; // text, quote, highlight, etc.
    public int DisplayOrder { get; set; } = 0;
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual UserCollection Collection { get; set; } = null!;
}

// New model for media items (images, videos, etc.) in collections
public class CollectionMedia
{
    public int Id { get; set; }
    public int CollectionId { get; set; }
    public string MediaUrl { get; set; } = string.Empty; // For external URLs
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty; // image, video, audio, document
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual UserCollection Collection { get; set; } = null!;
}

// New model for external links in collections
public class CollectionLink
{
    public int Id { get; set; }
    public int CollectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string LinkType { get; set; } = "external"; // external, social, reference, etc.
    public int DisplayOrder { get; set; } = 0;
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual UserCollection Collection { get; set; } = null!;
}

// View Models for Authentication
public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Display Name")]
    [StringLength(100, ErrorMessage = "Display name must be at most {1} characters long.")]
    public string DisplayName { get; set; } = string.Empty;
    
    [Display(Name = "Profile Photo (optional)")]
    public IFormFile? ProfilePhoto { get; set; }
}

// View Models for User Profile and Collections
public class ProfileViewModel
{
    [Required]
    [Display(Name = "Display Name")]
    [StringLength(100, ErrorMessage = "Display name must be at most {1} characters long.")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Bio")]
    [StringLength(500, ErrorMessage = "Bio must be at most {1} characters long.")]
    public string? Bio { get; set; }

    [Display(Name = "Profile Photo")]
    public IFormFile? NewProfilePhoto { get; set; }
    
    public string? CurrentPhotoUrl { get; set; }
    public bool RemovePhoto { get; set; }

    [Display(Name = "Member Since")]
    public DateTime CreatedAt { get; set; }
    
    public int BioCharactersRemaining => 500 - (Bio?.Length ?? 0);
}

public class CreateCollectionViewModel
{
    [Required]
    [Display(Name = "Collection Name")]
    [StringLength(100, ErrorMessage = "Collection name must be at most {1} characters long.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Short Description")]
    [StringLength(500, ErrorMessage = "Description must be at most {1} characters long.")]
    public string? Description { get; set; }

    [Display(Name = "Long Description")]
    [StringLength(2000, ErrorMessage = "Long description must be at most {1} characters long.")]
    public string? LongDescription { get; set; }

    [Display(Name = "Curator Notes")]
    [StringLength(1000, ErrorMessage = "Curator notes must be at most {1} characters long.")]
    public string? CuratorNotes { get; set; }

    [Display(Name = "Tags (comma-separated)")]
    [StringLength(500, ErrorMessage = "Tags must be at most {1} characters long.")]
    public string? Tags { get; set; }

    [Display(Name = "Public Collection")]
    public bool IsPublic { get; set; } = true; [Display(Name = "Featured Collection")]
    public bool IsFeatured { get; set; } = false;

    [Display(Name = "Featured Until")]
    [DataType(DataType.DateTime)]
    public DateTime? FeaturedUntil { get; set; }

    // SEO Fields
    [Display(Name = "SEO Title")]
    [StringLength(60, ErrorMessage = "SEO title should be at most {1} characters long.")]
    public string? MetaTitle { get; set; }

    [Display(Name = "SEO Description")]
    [StringLength(160, ErrorMessage = "SEO description should be at most {1} characters long.")]
    public string? MetaDescription { get; set; }

    [Display(Name = "SEO Keywords")]
    [StringLength(255, ErrorMessage = "SEO keywords must be at most {1} characters long.")]
    public string? MetaKeywords { get; set; }

    [Display(Name = "Social Media Image URL")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? SocialImageUrl { get; set; }
}

public class EditCollectionViewModel : CreateCollectionViewModel
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ViewCount { get; set; }
}

public class EditCollectionItemViewModel
{
    public int Id { get; set; }
    public int CollectionId { get; set; }
    public int ArtworkId { get; set; }
    public DateTime AddedAt { get; set; }

    [Display(Name = "URL Slug")]
    [StringLength(100, ErrorMessage = "Slug must be at most {1} characters long.")]
    public string? Slug { get; set; }

    [Display(Name = "Custom Title")]
    [StringLength(200, ErrorMessage = "Custom title must be at most {1} characters long.")]
    public string? CustomTitle { get; set; }

    [Display(Name = "Custom Description")]
    [StringLength(1000, ErrorMessage = "Custom description must be at most {1} characters long.")]
    public string? CustomDescription { get; set; }

    [Display(Name = "Curator Notes")]
    [StringLength(2000, ErrorMessage = "Curator notes must be at most {1} characters long.")]
    public string? CuratorNotes { get; set; }

    [Display(Name = "Display Order")]
    [Range(0, 9999, ErrorMessage = "Display order must be between {1} and {2}.")]
    public int DisplayOrder { get; set; } = 0;

    [Display(Name = "Highlighted Item")]
    public bool IsHighlighted { get; set; } = false;

    [Display(Name = "SEO Title")]
    [StringLength(60, ErrorMessage = "SEO title should be at most {1} characters long.")]
    public string? MetaTitle { get; set; }

    [Display(Name = "SEO Description")]
    [StringLength(160, ErrorMessage = "SEO description should be at most {1} characters long.")]
    public string? MetaDescription { get; set; }

    // Additional properties for display
    public string CollectionName { get; set; } = string.Empty;
    public string CollectionSlug { get; set; } = string.Empty;
    public string ArtworkTitle { get; set; } = string.Empty;
    public string ArtworkArtist { get; set; } = string.Empty;
    public string? ArtworkImageUrl { get; set; }
}

// View Models for SEO-friendly public collection experiences
public class PublicCollectionViewModel
{
    public UserCollection Collection { get; set; } = null!;
    public string CreatorDisplayName { get; set; } = string.Empty;
    public int ArtworkCount { get; set; }
    public List<EnrichedArtworkViewModel> PreviewArtworks { get; set; } = new List<EnrichedArtworkViewModel>();
    public List<string> Tags { get; set; } = new List<string>();
}

public class PublicCollectionDetailsViewModel
{
    public UserCollection Collection { get; set; } = null!;
    public string CreatorDisplayName { get; set; } = string.Empty;
    public List<EnrichedArtworkViewModel> EnrichedArtworks { get; set; } = new List<EnrichedArtworkViewModel>();
    public IEnumerable<CollectionContentSection> ContentSections { get; set; } = new List<CollectionContentSection>();
    public IEnumerable<CollectionMedia> MediaItems { get; set; } = new List<CollectionMedia>();
    public IEnumerable<CollectionLink> Links { get; set; } = new List<CollectionLink>();
    public List<string> Tags { get; set; } = new List<string>();
    public int ArtworkCount { get; set; }
}

public class EnrichedArtworkViewModel
{
    // Basic artwork information from API
    public int ArtworkId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string? DateDisplay { get; set; }
    public string? MediumDisplay { get; set; }
    public string? DimensionsDisplay { get; set; }
    public string? PlaceOfOrigin { get; set; }
    public string? Description { get; set; }
    public string? ImageId { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? StyleTitle { get; set; }
    public string? ClassificationTitle { get; set; }
    public string? DepartmentTitle { get; set; }
    public string? ArtworkTypeTitle { get; set; }
    public bool? IsPublicDomain { get; set; }
    public string? CreditLine { get; set; }
    public string? PublicationHistory { get; set; }
    public string? ExhibitionHistory { get; set; }
    public string? ProvenanceText { get; set; }
    public string? Slug { get; set; } // Added for SEO-friendly URLs

    // Collection-specific information
    public string? CollectionCustomTitle { get; set; }
    public string? CollectionCustomDescription { get; set; }
    public string? CuratorNotes { get; set; }
    public bool IsHighlighted { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime AddedAt { get; set; }

    // Computed properties for SEO
    public string DisplayTitle => !string.IsNullOrEmpty(CollectionCustomTitle) ? CollectionCustomTitle : Title;
    public string DisplayDescription => !string.IsNullOrEmpty(CollectionCustomDescription) ? CollectionCustomDescription : Description ?? string.Empty;
    public bool HasCustomization => !string.IsNullOrEmpty(CollectionCustomTitle) || !string.IsNullOrEmpty(CollectionCustomDescription);
}

// View model for collection links
public class CollectionLinkViewModel
{
    public int Id { get; set; }
    public int CollectionId { get; set; }

    [Required]
    [Display(Name = "Link Title")]
    [StringLength(200, ErrorMessage = "Title must be at most {1} characters long.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "URL")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    [StringLength(500, ErrorMessage = "URL must be at most {1} characters long.")]
    public string Url { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLength(500, ErrorMessage = "Description must be at most {1} characters long.")]
    public string? Description { get; set; }

    [Display(Name = "Link Type")]
    public string LinkType { get; set; } = "external";

    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; } = 0;

    [Display(Name = "Visible")]
    public bool IsVisible { get; set; } = true;
}

// View model for individual collection item details page
public class CollectionItemDetailsViewModel
{
    // Basic artwork info from API
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string? Medium { get; set; }
    public string? Dimensions { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? ExternalUrl { get; set; }

    // Collection item specific info
    public string Slug { get; set; } = string.Empty;
    public string? CustomTitle { get; set; }
    public string? CustomDescription { get; set; }
    public string? CuratorNotes { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsHighlighted { get; set; }

    // SEO fields for the collection item
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }
    public string? SocialImageUrl { get; set; }    // Collection info
    public int CollectionId { get; set; }
    public string CollectionSlug { get; set; } = string.Empty;
    public string CollectionTitle { get; set; } = string.Empty;
    public string? CollectionDescription { get; set; }

    // Timestamps
    public DateTime? UpdatedAt { get; set; }

    // Control flags
    public bool CanEdit { get; set; }

    // Computed properties for display
    public string DisplayTitle => !string.IsNullOrEmpty(CustomTitle) ? CustomTitle : Title;
    public string DisplayDescription => !string.IsNullOrEmpty(CustomDescription) ? CustomDescription : Description ?? string.Empty;
    public string DisplayArtist => Artist;
    public string SeoTitle => !string.IsNullOrEmpty(MetaTitle) ? MetaTitle : DisplayTitle;
    public string SeoDescription => !string.IsNullOrEmpty(MetaDescription) ? MetaDescription : DisplayDescription;
}

// View model for collection details page (for authenticated users)
public class CollectionDetailsViewModel
{
    public UserCollection Collection { get; set; } = null!;
    public IEnumerable<CollectionArtwork> Artworks { get; set; } = new List<CollectionArtwork>();
    public IEnumerable<CollectionContentSection> ContentSections { get; set; } = new List<CollectionContentSection>();
    public IEnumerable<CollectionMedia> MediaItems { get; set; } = new List<CollectionMedia>();
    public IEnumerable<CollectionLink> Links { get; set; } = new List<CollectionLink>();
    public bool CanEdit { get; set; }
}

// View Model for Collection Media operations
public class CollectionMediaViewModel
{
    public int Id { get; set; }
    public int CollectionId { get; set; }

    [Display(Name = "Media URL")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string MediaUrl { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;

    [Display(Name = "Media Type")]
    public string MediaType { get; set; } = string.Empty; // image, video, audio, document

    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    [Display(Name = "Title")]
    [StringLength(200, ErrorMessage = "Title must be at most {1} characters long.")]
    public string? Title { get; set; }

    [Display(Name = "Description")]
    [StringLength(1000, ErrorMessage = "Description must be at most {1} characters long.")]
    public string? Description { get; set; }

    [Display(Name = "Alt Text")]
    [StringLength(255, ErrorMessage = "Alt text must be at most {1} characters long.")]
    public string? AltText { get; set; }

    [Display(Name = "Display Order")]
    [Range(0, 9999, ErrorMessage = "Display order must be between {1} and {2}.")]
    public int DisplayOrder { get; set; } = 0;
}

// View Model for Collection Content Section operations
public class CollectionContentSectionViewModel
{
    public int Id { get; set; }
    public int CollectionId { get; set; }

    [Required]
    [Display(Name = "Section Title")]
    [StringLength(200, ErrorMessage = "Title must be at most {1} characters long.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Content")]
    [StringLength(5000, ErrorMessage = "Content must be at most {1} characters long.")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Section Type")]
    [StringLength(50, ErrorMessage = "Section type must be at most {1} characters long.")]
    public string SectionType { get; set; } = "text"; // text, quote, highlight, etc.

    [Display(Name = "Display Order")]
    [Range(0, 9999, ErrorMessage = "Display order must be between {1} and {2}.")]
    public int DisplayOrder { get; set; } = 0;

    [Display(Name = "Visible")]
    public bool IsVisible { get; set; } = true;
}
