using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebSpark.ArtSpark.Demo.Models;

namespace WebSpark.ArtSpark.Demo.Data;

public class ArtSparkDbContext : IdentityDbContext<ApplicationUser>
{
    public ArtSparkDbContext(DbContextOptions<ArtSparkDbContext> options) : base(options)
    {
    }
    public override DbSet<ApplicationUser> Users { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ArtworkReview> Reviews { get; set; }
    public DbSet<UserFavorite> Favorites { get; set; }
    public DbSet<UserCollection> Collections { get; set; }
    public DbSet<CollectionArtwork> CollectionArtworks { get; set; }
    public DbSet<CollectionContentSection> CollectionContentSections { get; set; }
    public DbSet<CollectionMedia> CollectionMedia { get; set; }
    public DbSet<CollectionLink> CollectionLinks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ApplicationUser
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();
        });
        
        // Configure AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AdminUserId, e.CreatedAtUtc });
            entity.HasIndex(e => new { e.TargetUserId, e.CreatedAtUtc });
            
            entity.HasOne(e => e.AdminUser)
                  .WithMany()
                  .HasForeignKey(e => e.AdminUserId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.TargetUser)
                  .WithMany()
                  .HasForeignKey(e => e.TargetUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ArtworkReview
        modelBuilder.Entity<ArtworkReview>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ArtworkId }).IsUnique();

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Reviews)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserFavorite
        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ArtworkId }).IsUnique();

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Favorites)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });        // Configure UserCollection
        modelBuilder.Entity<UserCollection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MetaTitle).HasMaxLength(60);
            entity.Property(e => e.MetaDescription).HasMaxLength(160);
            entity.Property(e => e.MetaKeywords).HasMaxLength(255);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Collections)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure CollectionArtwork
        modelBuilder.Entity<CollectionArtwork>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CollectionId, e.ArtworkId }).IsUnique();
            entity.HasIndex(e => new { e.CollectionId, e.Slug }).IsUnique();
            entity.Property(e => e.Slug).HasMaxLength(100);
            entity.Property(e => e.CustomTitle).HasMaxLength(200);
            entity.Property(e => e.MetaTitle).HasMaxLength(60);
            entity.Property(e => e.MetaDescription).HasMaxLength(160);

            entity.HasOne(e => e.Collection)
                  .WithMany(c => c.Artworks)
                  .HasForeignKey(e => e.CollectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure CollectionContentSection
        modelBuilder.Entity<CollectionContentSection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.SectionType).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Collection)
                  .WithMany(c => c.ContentSections)
                  .HasForeignKey(e => e.CollectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure CollectionMedia
        modelBuilder.Entity<CollectionMedia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.MediaType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.AltText).HasMaxLength(255);

            entity.HasOne(e => e.Collection)
                  .WithMany(c => c.MediaItems)
                  .HasForeignKey(e => e.CollectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure CollectionLink
        modelBuilder.Entity<CollectionLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
            entity.Property(e => e.LinkType).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Collection)
                  .WithMany(c => c.Links)
                  .HasForeignKey(e => e.CollectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
