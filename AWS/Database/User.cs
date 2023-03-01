using System.Text.Json.Serialization;

namespace AWS.Users;

public class User
{

    public User()
    {
        Posts = new List<Post>();
    }
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    
    [JsonIgnore]
    public ICollection<Post> Posts { get; set; }
}