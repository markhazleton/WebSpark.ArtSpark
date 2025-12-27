using Microsoft.EntityFrameworkCore;
using WebSpark.ArtSpark.Demo.Data;
using WebSpark.ArtSpark.Demo.Models;
using WebSpark.ArtSpark.Demo.Utilities;

namespace WebSpark.ArtSpark.Demo.Services;

public interface IReviewService
{
    Task<IEnumerable<ArtworkReview>> GetReviewsForArtworkAsync(int artworkId);
    Task<ArtworkReview?> GetUserReviewAsync(int artworkId, string userId);
    Task<ArtworkReview> CreateOrUpdateReviewAsync(int artworkId, string userId, int rating, string? reviewText);
    Task<bool> DeleteReviewAsync(int reviewId, string userId);
    Task<double> GetAverageRatingAsync(int artworkId);
    Task<int> GetReviewCountAsync(int artworkId);
}

public class ReviewService : IReviewService
{
    private readonly ArtSparkDbContext _context;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(ArtSparkDbContext context, ILogger<ReviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ArtworkReview>> GetReviewsForArtworkAsync(int artworkId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ArtworkId == artworkId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ArtworkReview?> GetUserReviewAsync(int artworkId, string userId)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.ArtworkId == artworkId && r.UserId == userId);
    }

    public async Task<ArtworkReview> CreateOrUpdateReviewAsync(int artworkId, string userId, int rating, string? reviewText)
    {
        var existingReview = await GetUserReviewAsync(artworkId, userId);

        if (existingReview != null)
        {
            existingReview.Rating = rating;
            existingReview.ReviewText = reviewText;
            existingReview.UpdatedAt = DateTime.UtcNow;
            _context.Reviews.Update(existingReview);
        }
        else
        {
            existingReview = new ArtworkReview
            {
                ArtworkId = artworkId,
                UserId = userId,
                Rating = rating,
                ReviewText = reviewText
            };
            _context.Reviews.Add(existingReview);
        }

        await _context.SaveChangesAsync();
        return existingReview;
    }

    public async Task<bool> DeleteReviewAsync(int reviewId, string userId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

        if (review == null) return false;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<double> GetAverageRatingAsync(int artworkId)
    {
        var ratings = await _context.Reviews
            .Where(r => r.ArtworkId == artworkId)
            .Select(r => r.Rating)
            .ToListAsync();

        return ratings.Any() ? ratings.Average() : 0;
    }

    public async Task<int> GetReviewCountAsync(int artworkId)
    {
        return await _context.Reviews
            .CountAsync(r => r.ArtworkId == artworkId);
    }
}

public interface IFavoriteService
{
    Task<bool> AddToFavoritesAsync(int artworkId, string userId);
    Task<bool> RemoveFromFavoritesAsync(int artworkId, string userId);
    Task<bool> IsArtworkFavoritedAsync(int artworkId, string userId);
    Task<IEnumerable<UserFavorite>> GetUserFavoritesAsync(string userId);
}

public class FavoriteService : IFavoriteService
{
    private readonly ArtSparkDbContext _context;

    public FavoriteService(ArtSparkDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddToFavoritesAsync(int artworkId, string userId)
    {
        var existing = await _context.Favorites
            .FirstOrDefaultAsync(f => f.ArtworkId == artworkId && f.UserId == userId);

        if (existing != null) return false;

        _context.Favorites.Add(new UserFavorite
        {
            ArtworkId = artworkId,
            UserId = userId
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromFavoritesAsync(int artworkId, string userId)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.ArtworkId == artworkId && f.UserId == userId);

        if (favorite == null) return false;

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsArtworkFavoritedAsync(int artworkId, string userId)
    {
        return await _context.Favorites
            .AnyAsync(f => f.ArtworkId == artworkId && f.UserId == userId);
    }
    public async Task<IEnumerable<UserFavorite>> GetUserFavoritesAsync(string userId)
    {
        return await _context.Favorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }
}

public interface ICollectionService
{
    // Basic collection operations
    Task<IEnumerable<UserCollection>> GetUserCollectionsAsync(string userId);
    Task<UserCollection?> GetCollectionByIdAsync(int collectionId, string userId);
    Task<UserCollection?> GetCollectionBySlugAsync(string slug, string? userId = null);
    Task<CollectionDetailsViewModel?> GetCollectionDetailsAsync(int collectionId, string? userId = null);
    Task<CollectionDetailsViewModel?> GetCollectionDetailsBySlugAsync(string slug, string? userId = null);

    // Collection CRUD operations
    Task<UserCollection> CreateCollectionAsync(CreateCollectionViewModel model, string userId);
    Task<bool> UpdateCollectionAsync(int collectionId, EditCollectionViewModel model, string userId);
    Task<bool> DeleteCollectionAsync(int collectionId, string userId);

    // Artwork operations
    Task<bool> AddArtworkToCollectionAsync(int collectionId, int artworkId, string userId);
    Task<bool> RemoveArtworkFromCollectionAsync(int collectionId, int artworkId, string userId);
    Task<IEnumerable<CollectionArtwork>> GetCollectionArtworksAsync(int collectionId, string userId);
    Task<bool> IsArtworkInCollectionAsync(int collectionId, int artworkId);
    Task<bool> UpdateCollectionArtworkAsync(int collectionId, int artworkId, string userId, CollectionArtwork updatedArtwork);

    // Content sections
    Task<CollectionContentSection> AddContentSectionAsync(int collectionId, string userId, CollectionContentSectionViewModel model);
    Task<bool> UpdateContentSectionAsync(int sectionId, string userId, CollectionContentSectionViewModel model);
    Task<bool> DeleteContentSectionAsync(int sectionId, string userId);
    Task<bool> ReorderContentSectionsAsync(int collectionId, string userId, List<int> sectionIds);    // Media operations
    Task<CollectionMedia> AddMediaAsync(int collectionId, string userId, string mediaUrl, string mediaType, string? title = null, string? description = null);
    Task<bool> UpdateMediaAsync(int mediaId, string userId, string? title, string? description);
    Task<bool> DeleteMediaAsync(int mediaId, string userId);
    Task<bool> ReorderMediaAsync(int collectionId, int[] mediaIds, string userId);

    // Link operations
    Task<CollectionLink> AddLinkAsync(int collectionId, string userId, CollectionLinkViewModel model);
    Task<bool> UpdateLinkAsync(int linkId, string userId, CollectionLinkViewModel model);
    Task<bool> DeleteLinkAsync(int linkId, string userId);
    // Public collections
    Task<IEnumerable<UserCollection>> GetPublicCollectionsAsync(int page = 1, int pageSize = 20);
    Task<IEnumerable<UserCollection>> GetPublicCollectionsAsync(int page, int pageSize, string? search, string? tag);
    Task<IEnumerable<UserCollection>> GetFeaturedCollectionsAsync();
    Task<IEnumerable<UserCollection>> GetFeaturedCollectionsAsync(int limit);
    Task<IEnumerable<UserCollection>> SearchCollectionsAsync(string searchTerm, int page = 1, int pageSize = 20);

    // SEO and analytics
    Task<bool> IncrementViewCountAsync(int collectionId);
    Task<bool> IncrementViewCountAsync(string slug);
    Task<string> GenerateUniqueSlugAsync(string name, int? existingCollectionId = null);
}

public class CollectionService : ICollectionService
{
    private readonly ArtSparkDbContext _context;
    private readonly ILogger<CollectionService> _logger;

    public CollectionService(ArtSparkDbContext context, ILogger<CollectionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Basic Collection Operations

    public async Task<IEnumerable<UserCollection>> GetUserCollectionsAsync(string userId)
    {
        return await _context.Collections
            .Include(c => c.Artworks)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserCollection?> GetCollectionByIdAsync(int collectionId, string userId)
    {
        return await _context.Collections
            .Include(c => c.Artworks)
            .Include(c => c.ContentSections.OrderBy(cs => cs.DisplayOrder))
            .Include(c => c.MediaItems.OrderBy(m => m.DisplayOrder))
            .Include(c => c.Links.OrderBy(l => l.DisplayOrder))
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);
    }

    public async Task<UserCollection?> GetCollectionBySlugAsync(string slug, string? userId = null)
    {
        var query = _context.Collections
            .Include(c => c.Artworks.OrderBy(a => a.DisplayOrder).ThenByDescending(a => a.AddedAt))
            .Include(c => c.ContentSections.OrderBy(cs => cs.DisplayOrder))
            .Include(c => c.MediaItems.OrderBy(m => m.DisplayOrder))
            .Include(c => c.Links.OrderBy(l => l.DisplayOrder))
            .Include(c => c.User)
            .Where(c => c.Slug == slug);

        if (userId != null)
        {
            query = query.Where(c => c.IsPublic || c.UserId == userId);
        }
        else
        {
            query = query.Where(c => c.IsPublic);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<CollectionDetailsViewModel?> GetCollectionDetailsAsync(int collectionId, string? userId = null)
    {
        var collection = await _context.Collections
            .Include(c => c.Artworks.OrderBy(a => a.DisplayOrder).ThenByDescending(a => a.AddedAt))
            .Include(c => c.ContentSections.OrderBy(cs => cs.DisplayOrder))
            .Include(c => c.MediaItems.OrderBy(m => m.DisplayOrder))
            .Include(c => c.Links.OrderBy(l => l.DisplayOrder))
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == collectionId && (c.IsPublic || c.UserId == userId));

        if (collection == null) return null;

        return new CollectionDetailsViewModel
        {
            Collection = collection,
            Artworks = collection.Artworks,
            ContentSections = collection.ContentSections,
            MediaItems = collection.MediaItems,
            Links = collection.Links,
            CanEdit = userId != null && collection.UserId == userId
        };
    }

    public async Task<CollectionDetailsViewModel?> GetCollectionDetailsBySlugAsync(string slug, string? userId = null)
    {
        var collection = await GetCollectionBySlugAsync(slug, userId);
        if (collection == null) return null;

        return new CollectionDetailsViewModel
        {
            Collection = collection,
            Artworks = collection.Artworks,
            ContentSections = collection.ContentSections,
            MediaItems = collection.MediaItems,
            Links = collection.Links,
            CanEdit = userId != null && collection.UserId == userId
        };
    }

    #endregion

    #region Collection CRUD Operations

    public async Task<UserCollection> CreateCollectionAsync(CreateCollectionViewModel model, string userId)
    {
        var slug = await GenerateUniqueSlugAsync(model.Name);

        var collection = new UserCollection
        {
            UserId = userId,
            Name = model.Name,
            Description = model.Description,
            IsPublic = model.IsPublic,
            Slug = slug,
            MetaTitle = model.MetaTitle ?? model.Name,
            MetaDescription = model.MetaDescription ?? SlugGenerator.GenerateMetaDescription(model.Description, model.Name),
            MetaKeywords = model.MetaKeywords,
            SocialImageUrl = model.SocialImageUrl,
            LongDescription = model.LongDescription,
            CuratorNotes = model.CuratorNotes,
            Tags = model.Tags,
            IsFeatured = model.IsFeatured,
            FeaturedUntil = model.FeaturedUntil,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Collections.Add(collection);
        await _context.SaveChangesAsync();
        return collection;
    }

    public async Task<bool> UpdateCollectionAsync(int collectionId, EditCollectionViewModel model, string userId)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);

        if (collection == null) return false;

        // Generate new slug if name changed
        if (collection.Name != model.Name)
        {
            collection.Slug = await GenerateUniqueSlugAsync(model.Name, collectionId);
        }

        collection.Name = model.Name;
        collection.Description = model.Description;
        collection.IsPublic = model.IsPublic;
        collection.MetaTitle = model.MetaTitle ?? model.Name;
        collection.MetaDescription = model.MetaDescription ?? SlugGenerator.GenerateMetaDescription(model.Description, model.Name);
        collection.MetaKeywords = model.MetaKeywords;
        collection.SocialImageUrl = model.SocialImageUrl;
        collection.LongDescription = model.LongDescription;
        collection.CuratorNotes = model.CuratorNotes;
        collection.Tags = model.Tags;
        collection.IsFeatured = model.IsFeatured;
        collection.FeaturedUntil = model.FeaturedUntil;
        collection.UpdatedAt = DateTime.UtcNow;

        _context.Collections.Update(collection);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCollectionAsync(int collectionId, string userId)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);

        if (collection == null) return false;

        _context.Collections.Remove(collection);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Artwork Operations

    public async Task<bool> AddArtworkToCollectionAsync(int collectionId, int artworkId, string userId)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);

        if (collection == null) return false;

        var existing = await _context.CollectionArtworks
            .FirstOrDefaultAsync(ca => ca.CollectionId == collectionId && ca.ArtworkId == artworkId);

        if (existing != null) return false;

        // Get the next display order
        var maxOrder = await _context.CollectionArtworks
            .Where(ca => ca.CollectionId == collectionId)
            .MaxAsync(ca => (int?)ca.DisplayOrder) ?? 0;

        var collectionArtwork = new CollectionArtwork
        {
            CollectionId = collectionId,
            ArtworkId = artworkId,
            DisplayOrder = maxOrder + 1,
            AddedAt = DateTime.UtcNow
        };

        _context.CollectionArtworks.Add(collectionArtwork);

        // Update collection timestamp
        collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveArtworkFromCollectionAsync(int collectionId, int artworkId, string userId)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);

        if (collection == null) return false;

        var collectionArtwork = await _context.CollectionArtworks
            .FirstOrDefaultAsync(ca => ca.CollectionId == collectionId && ca.ArtworkId == artworkId);

        if (collectionArtwork == null) return false;

        _context.CollectionArtworks.Remove(collectionArtwork);

        // Update collection timestamp
        collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CollectionArtwork>> GetCollectionArtworksAsync(int collectionId, string userId)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);

        if (collection == null) return new List<CollectionArtwork>();

        return await _context.CollectionArtworks
            .Where(ca => ca.CollectionId == collectionId)
            .OrderBy(ca => ca.DisplayOrder)
            .ThenByDescending(ca => ca.AddedAt)
            .ToListAsync();
    }

    public async Task<bool> IsArtworkInCollectionAsync(int collectionId, int artworkId)
    {
        return await _context.CollectionArtworks
            .AnyAsync(ca => ca.CollectionId == collectionId && ca.ArtworkId == artworkId);
    }

    public async Task<bool> UpdateCollectionArtworkAsync(int collectionId, int artworkId, string userId, CollectionArtwork updatedArtwork)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);

        if (collection == null) return false;

        var collectionArtwork = await _context.CollectionArtworks
            .FirstOrDefaultAsync(ca => ca.CollectionId == collectionId && ca.ArtworkId == artworkId);

        if (collectionArtwork == null) return false;

        // Generate slug if custom title is provided
        if (!string.IsNullOrEmpty(updatedArtwork.CustomTitle) && string.IsNullOrEmpty(updatedArtwork.Slug))
        {
            updatedArtwork.Slug = SlugGenerator.GenerateSlug(updatedArtwork.CustomTitle);
        }

        collectionArtwork.Slug = updatedArtwork.Slug;
        collectionArtwork.CustomTitle = updatedArtwork.CustomTitle;
        collectionArtwork.CustomDescription = updatedArtwork.CustomDescription;
        collectionArtwork.CuratorNotes = updatedArtwork.CuratorNotes;
        collectionArtwork.DisplayOrder = updatedArtwork.DisplayOrder;
        collectionArtwork.IsHighlighted = updatedArtwork.IsHighlighted;
        collectionArtwork.MetaTitle = updatedArtwork.MetaTitle;
        collectionArtwork.MetaDescription = updatedArtwork.MetaDescription;

        // Update collection timestamp
        collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Content Sections

    public async Task<CollectionContentSection> AddContentSectionAsync(int collectionId, string userId, CollectionContentSectionViewModel model)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);

        if (collection == null)
            throw new UnauthorizedAccessException("Collection not found or access denied");

        // Get the next display order
        var maxOrder = await _context.CollectionContentSections
            .Where(cs => cs.CollectionId == collectionId)
            .MaxAsync(cs => (int?)cs.DisplayOrder) ?? 0;

        var contentSection = new CollectionContentSection
        {
            CollectionId = collectionId,
            Title = model.Title,
            Content = model.Content,
            SectionType = model.SectionType,
            DisplayOrder = maxOrder + 1,
            IsVisible = model.IsVisible,
            CreatedAt = DateTime.UtcNow
        };

        _context.CollectionContentSections.Add(contentSection);

        // Update collection timestamp
        collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return contentSection;
    }

    public async Task<bool> UpdateContentSectionAsync(int sectionId, string userId, CollectionContentSectionViewModel model)
    {
        var contentSection = await _context.CollectionContentSections
            .Include(cs => cs.Collection)
            .FirstOrDefaultAsync(cs => cs.Id == sectionId && cs.Collection.UserId == userId);

        if (contentSection == null) return false;

        contentSection.Title = model.Title;
        contentSection.Content = model.Content;
        contentSection.SectionType = model.SectionType;
        contentSection.IsVisible = model.IsVisible;
        contentSection.UpdatedAt = DateTime.UtcNow;

        // Update collection timestamp
        contentSection.Collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteContentSectionAsync(int sectionId, string userId)
    {
        var contentSection = await _context.CollectionContentSections
            .Include(cs => cs.Collection)
            .FirstOrDefaultAsync(cs => cs.Id == sectionId && cs.Collection.UserId == userId);

        if (contentSection == null) return false;

        _context.CollectionContentSections.Remove(contentSection);

        // Update collection timestamp
        contentSection.Collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReorderContentSectionsAsync(int collectionId, string userId, List<int> sectionIds)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);

        if (collection == null) return false;

        var sections = await _context.CollectionContentSections
            .Where(cs => cs.CollectionId == collectionId && sectionIds.Contains(cs.Id))
            .ToListAsync();

        for (int i = 0; i < sectionIds.Count; i++)
        {
            var section = sections.FirstOrDefault(s => s.Id == sectionIds[i]);
            if (section != null)
            {
                section.DisplayOrder = i + 1;
            }
        }

        collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Media Operations

    public async Task<CollectionMedia> AddMediaAsync(int collectionId, string userId, string mediaUrl, string mediaType, string? title = null, string? description = null)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);

        if (collection == null)
            throw new UnauthorizedAccessException("Collection not found or access denied");        // Get the next display order
        var maxOrder = await _context.CollectionMedia
            .Where(m => m.CollectionId == collectionId)
            .MaxAsync(m => (int?)m.DisplayOrder) ?? 0;

        var media = new CollectionMedia
        {
            CollectionId = collectionId,
            MediaUrl = mediaUrl,
            MediaType = mediaType,
            Title = title,
            Description = description,
            DisplayOrder = maxOrder + 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.CollectionMedia.Add(media);

        // Update collection timestamp
        collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return media;
    }
    public async Task<bool> UpdateMediaAsync(int mediaId, string userId, string? title, string? description)
    {
        var media = await _context.CollectionMedia
            .Include(m => m.Collection)
            .FirstOrDefaultAsync(m => m.Id == mediaId && m.Collection.UserId == userId);

        if (media == null) return false;

        media.Title = title;
        media.Description = description;
        media.UpdatedAt = DateTime.UtcNow;

        // Update collection timestamp
        media.Collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> DeleteMediaAsync(int mediaId, string userId)
    {
        var media = await _context.CollectionMedia
            .Include(m => m.Collection)
            .FirstOrDefaultAsync(m => m.Id == mediaId && m.Collection.UserId == userId); if (media == null) return false;

        _context.CollectionMedia.Remove(media);        // Update collection timestamp
        media.Collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReorderMediaAsync(int collectionId, int[] mediaIds, string userId)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);

        if (collection == null) return false;

        var mediaItems = await _context.CollectionMedia
            .Where(m => m.CollectionId == collectionId && mediaIds.Contains(m.Id))
            .ToListAsync();

        for (int i = 0; i < mediaIds.Length; i++)
        {
            var mediaItem = mediaItems.FirstOrDefault(m => m.Id == mediaIds[i]);
            if (mediaItem != null)
            {
                mediaItem.DisplayOrder = i + 1;
                mediaItem.UpdatedAt = DateTime.UtcNow;
            }
        }

        collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Link Operations

    public async Task<CollectionLink> AddLinkAsync(int collectionId, string userId, CollectionLinkViewModel model)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);

        if (collection == null)
            throw new UnauthorizedAccessException("Collection not found or access denied");

        // Get the next display order
        var maxOrder = await _context.CollectionLinks
            .Where(l => l.CollectionId == collectionId)
            .MaxAsync(l => (int?)l.DisplayOrder) ?? 0;

        var link = new CollectionLink
        {
            CollectionId = collectionId,
            Title = model.Title,
            Url = model.Url,
            Description = model.Description,
            LinkType = model.LinkType,
            DisplayOrder = maxOrder + 1,
            IsVisible = model.IsVisible,
            CreatedAt = DateTime.UtcNow
        };

        _context.CollectionLinks.Add(link);

        // Update collection timestamp
        collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return link;
    }

    public async Task<bool> UpdateLinkAsync(int linkId, string userId, CollectionLinkViewModel model)
    {
        var link = await _context.CollectionLinks
            .Include(l => l.Collection)
            .FirstOrDefaultAsync(l => l.Id == linkId && l.Collection.UserId == userId);

        if (link == null) return false;

        link.Title = model.Title;
        link.Url = model.Url;
        link.Description = model.Description;
        link.LinkType = model.LinkType;
        link.IsVisible = model.IsVisible;
        link.UpdatedAt = DateTime.UtcNow;

        // Update collection timestamp
        link.Collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteLinkAsync(int linkId, string userId)
    {
        var link = await _context.CollectionLinks
            .Include(l => l.Collection)
            .FirstOrDefaultAsync(l => l.Id == linkId && l.Collection.UserId == userId);

        if (link == null) return false;

        _context.CollectionLinks.Remove(link);

        // Update collection timestamp
        link.Collection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Public Collections

    public async Task<IEnumerable<UserCollection>> GetPublicCollectionsAsync(int page = 1, int pageSize = 20)
    {
        return await _context.Collections
            .Include(c => c.User)
            .Where(c => c.IsPublic)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserCollection>> GetPublicCollectionsAsync(int page, int pageSize, string? search, string? tag)
    {
        var query = _context.Collections
            .Include(c => c.User)
            .Where(c => c.IsPublic);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowercaseSearch = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(lowercaseSearch) ||
                c.Description!.ToLower().Contains(lowercaseSearch) ||
                (c.Tags != null && c.Tags.ToLower().Contains(lowercaseSearch)));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var lowercaseTag = tag.ToLower();
            query = query.Where(c => c.Tags != null && c.Tags.ToLower().Contains(lowercaseTag));
        }

        return await query
            .OrderByDescending(c => c.ViewCount)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserCollection>> GetFeaturedCollectionsAsync()
    {
        return await _context.Collections
            .Include(c => c.User)
            .Where(c => c.IsPublic && c.IsFeatured && (c.FeaturedUntil == null || c.FeaturedUntil > DateTime.UtcNow))
            .OrderByDescending(c => c.ViewCount)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserCollection>> GetFeaturedCollectionsAsync(int limit)
    {
        return await _context.Collections
            .Include(c => c.User)
            .Where(c => c.IsPublic && c.IsFeatured && (c.FeaturedUntil == null || c.FeaturedUntil > DateTime.UtcNow)).OrderByDescending(c => c.ViewCount)
            .ThenByDescending(c => c.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserCollection>> SearchCollectionsAsync(string searchTerm, int page = 1, int pageSize = 20)
    {
        var lowercaseSearch = searchTerm.ToLower();
        return await _context.Collections
            .Include(c => c.User)
            .Where(c => c.IsPublic &&
                (c.Name.ToLower().Contains(lowercaseSearch) ||
                 c.Description!.ToLower().Contains(lowercaseSearch) ||
                 c.Tags!.ToLower().Contains(lowercaseSearch)))
            .OrderByDescending(c => c.ViewCount)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    #endregion

    #region SEO and Analytics

    public async Task<bool> IncrementViewCountAsync(int collectionId)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId);

        if (collection == null) return false;

        collection.ViewCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncrementViewCountAsync(string slug)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Slug == slug);

        if (collection == null) return false;

        collection.ViewCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateUniqueSlugAsync(string name, int? existingCollectionId = null)
    {
        var baseSlug = SlugGenerator.GenerateSlug(name);
        var slug = baseSlug;
        var counter = 1;

        while (await _context.Collections
            .AnyAsync(c => c.Slug == slug && (existingCollectionId == null || c.Id != existingCollectionId)))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    #endregion
}
