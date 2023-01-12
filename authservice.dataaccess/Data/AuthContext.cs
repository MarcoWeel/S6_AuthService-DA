using Microsoft.EntityFrameworkCore;
using authservice.dataaccess.Models;

namespace authservice.dataaccess.Data;

public class AuthContext : DbContext
{
    public AuthContext(DbContextOptions<AuthContext> options)
    : base(options)
    {
    }

    public DbSet<User> User { get; set; } = default!;
}