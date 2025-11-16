namespace WebSpark.ArtSpark.Agent.Models
{
    public class ArtworkData
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ArtistDisplay { get; set; } = string.Empty;
        public string DateDisplay { get; set; } = string.Empty;
        public string PlaceOfOrigin { get; set; } = string.Empty;
        public string Medium { get; set; } = string.Empty;
        public string Dimensions { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CulturalContext { get; set; } = string.Empty;
        public string StyleTitle { get; set; } = string.Empty;
        public string Classification { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public Dictionary<string, object> FullApiData { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
