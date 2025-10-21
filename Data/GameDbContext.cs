using Microsoft.EntityFrameworkCore;
using GameShelfWeb.Models;

namespace GameShelfWeb.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        public DbSet<Game> Games { get; set; } = null!;
    }
}
