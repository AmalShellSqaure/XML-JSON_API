namespace XML_JSON_API.Models
{
    public class FileStatus
    {
        public Guid Id { get; set; }
        public bool IsProcessed { get; set; }
        public string? JsonData { get; set; }
        public string? Error { get; set; }
    }
}
