using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebSpark.ArtSpark.Agent.Extensions;
using WebSpark.ArtSpark.Client.Clients;
using WebSpark.ArtSpark.Client.Interfaces;
using WebSpark.ArtSpark.Demo.Data;
using WebSpark.ArtSpark.Demo.HttpClientUtility.MemoryCache;
using WebSpark.ArtSpark.Demo.Models;
using WebSpark.ArtSpark.Demo.Options;
using WebSpark.ArtSpark.Demo.Services;
using WebSpark.ArtSpark.Demo.Utilities;
using WebSpark.Bootswatch;
using WebSpark.HttpClientUtility.ClientService;
using WebSpark.HttpClientUtility.RequestResult;
using WebSpark.HttpClientUtility.StringConverter;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog logging
builder.ConfigureLogging();

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Add HttpContextAccessor for Tag Helper support
builder.Services.AddHttpContextAccessor();

// Add Database Context
builder.Services.AddDbContext<ArtSparkDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Use SQLite for development
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
    else
    {
        // Use SQLite for production as well
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

// Add Identity Services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = builder.Configuration.GetValue<bool>("Identity:RequireEmailVerification");
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ArtSparkDbContext>()
.AddDefaultTokenProviders();

// Configure options from appsettings
builder.Services.Configure<FileUploadOptions>(builder.Configuration.GetSection(FileUploadOptions.SectionName));
builder.Services.Configure<AuditLogOptions>(builder.Configuration.GetSection(AuditLogOptions.SectionName));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));

// Configure form file upload limits
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    var maxSize = builder.Configuration.GetValue<long>("FileUpload:MaxProfilePhotoSize");
    options.MultipartBodyLengthLimit = maxSize > 0 ? maxSize : 5242880;
});

// Add Profile Management Services
builder.Services.AddScoped<IProfilePhotoService, ProfilePhotoService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IPasswordStrengthService, PasswordStrengthService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, SmtpEmailSender>();

// Add Background Services
builder.Services.AddHostedService<AuditLogCleanupService>();
builder.Services.AddHostedService<ProfilePhotoStorageMonitor>();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<ProfilePhotoStorageHealthCheck>("profile_storage");

// Add Review Services
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
builder.Services.AddScoped<IPublicCollectionService, PublicCollectionService>();
builder.Services.AddSingleton<IBuildInfoService, BuildInfoService>();

// Add SEO Optimization Service
builder.Services.AddScoped<ISeoOptimizationService, SeoOptimizationService>();

// Add HttpClient factory (required for WebSpark.HttpClientUtility)
builder.Services.AddHttpClient();

// Add ArtSpark Agent services
builder.Services.AddArtSparkAgent(builder.Configuration, config =>
{
    // Override default settings if needed
    config.OpenAI.ModelId = "gpt-4o";
    config.OpenAI.Temperature = 0.7;
    config.Cache.Enabled = true;
});

RegisterHttpClientUtilities(builder);

builder.Services.AddSingleton<IStringConverter, SystemJsonStringConverter>();
builder.Services.AddScoped<IHttpClientService, HttpClientService>();
builder.Services.AddScoped<HttpRequestResultService>();
builder.Services.AddScoped<IArtInstituteClient, ArtInstituteClient>();

// Register IHttpRequestResultService with telemetry decorator
builder.Services.AddScoped<IHttpRequestResultService>(provider =>
{
    IHttpRequestResultService service = provider.GetRequiredService<HttpRequestResultService>();

    // Add Telemetry (basic decorator for logging and monitoring)
    service = new HttpRequestResultServiceTelemetry(
        provider.GetRequiredService<ILogger<HttpRequestResultServiceTelemetry>>(),
        service
    );

    return service;
});

// Add WebSpark.Bootswatch theme switcher
builder.Services.AddBootswatchThemeSwitcher();

var app = builder.Build();

// Seed Identity roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        await IdentitySeeder.SeedRolesAndAdminAsync(services, builder.Configuration, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Configure WebSpark.Bootswatch (includes static files and style cache)
app.UseBootswatchAll();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapHealthChecks("/health");

// SEO-friendly collection routes
app.MapControllerRoute(
    name: "collectionBySlug",
    pattern: "collection/{slug}",
    defaults: new { controller = "PublicCollections", action = "Details" });

app.MapControllerRoute(
    name: "collectionItemBySlug",
    pattern: "collection/{collectionSlug}/item/{itemSlug}",
    defaults: new { controller = "Account", action = "CollectionItemDetails" });

app.MapControllerRoute(
    name: "collections",
    pattern: "collections",
    defaults: new { controller = "Account", action = "Collections" });

app.MapControllerRoute(
    name: "publicCollections",
    pattern: "explore/collections",
    defaults: new { controller = "PublicCollections", action = "Index" });

app.MapControllerRoute(
    name: "featuredCollections",
    pattern: "explore/featured",
    defaults: new { controller = "PublicCollections", action = "Featured" });

app.MapControllerRoute(
    name: "recentCollections",
    pattern: "explore/recent",
    defaults: new { controller = "PublicCollections", action = "Recent" });

app.MapControllerRoute(
    name: "popularCollections",
    pattern: "explore/popular",
    defaults: new { controller = "PublicCollections", action = "Popular" });

app.MapControllerRoute(
    name: "collectionsByTag",
    pattern: "explore/collections/tag/{tag}",
    defaults: new { controller = "PublicCollections", action = "ByTag" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();


static void RegisterHttpClientUtilities(WebApplicationBuilder builder)
{
    builder.Services.AddHttpClient("HttpClientService", client =>
    {
        client.Timeout = TimeSpan.FromMilliseconds(90000);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "HttpClientService");
        client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add("X-Request-Source", "HttpClientService");
    });

    // Register IHttpClientMemoryCache implementation
    builder.Services.AddSingleton<IHttpClientMemoryCache, HttpClientMemoryCache>();

    // HTTP Send Service with Decorator Pattern
    builder.Services.AddSingleton(serviceProvider =>
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var retryOptions = configuration.GetSection("HttpRequestResultPollyOptions").Get<HttpRequestResultPollyOptions>();

        IHttpRequestResultService baseService = new HttpRequestResultService(
            serviceProvider.GetRequiredService<ILogger<HttpRequestResultService>>(),
            configuration,
            serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("HttpClientDecorator"));

        IHttpRequestResultService pollyService = new HttpRequestResultServicePolly(
            serviceProvider.GetRequiredService<ILogger<HttpRequestResultServicePolly>>(),
            baseService,
            retryOptions);

        IHttpRequestResultService telemetryService = new HttpRequestResultServiceTelemetry(
            serviceProvider.GetRequiredService<ILogger<HttpRequestResultServiceTelemetry>>(),
            pollyService);

        IHttpRequestResultService cacheService = new HttpRequestResultServiceCache(
            telemetryService,
            serviceProvider.GetRequiredService<ILogger<HttpRequestResultServiceCache>>(),
            serviceProvider.GetRequiredService<IMemoryCache>()); return cacheService;
    });
}

// Make the Program class accessible for testing
public partial class Program { }
