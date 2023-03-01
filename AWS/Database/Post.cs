using System.Text.Json.Serialization;

namespace AWS.Users;

public class Post
{

    public int Id { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; }
    public string? ImageKey { get; set; }
    
    [JsonIgnore]
    public User User { get; set; }
}