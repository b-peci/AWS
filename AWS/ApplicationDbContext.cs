using AWS.Users;
using Microsoft.EntityFrameworkCore;

namespace AWS;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
            
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    
}