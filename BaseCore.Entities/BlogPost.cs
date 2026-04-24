namespace BaseCore.Entities
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? Summary { get; set; }
        public string Content { get; set; } = "";
        public string? ThumbnailUrl { get; set; }
        public string? Category { get; set; }
        public int? AuthorUserId { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User? AuthorUser { get; set; }
    }
}
