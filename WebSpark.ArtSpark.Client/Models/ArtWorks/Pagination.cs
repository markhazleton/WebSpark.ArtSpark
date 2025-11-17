namespace WebSpark.ArtSpark.Client.Models.ArtWorks
{
    public class Pagination
    {
        public int total { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
        public int total_pages { get; set; }
        public int current_page { get; set; }
        public string? next_url { get; set; }
    }
}
