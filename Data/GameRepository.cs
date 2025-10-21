using System;
using System.Collections.Generic;
using System.Linq;
using GameShelfWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace GameShelfWeb.Data
{
    public class GameRepository
    {
        private readonly GameDbContext _db;

        public GameRepository(GameDbContext db)
        {
            _db = db;
            _db.Database.EnsureCreated();
        }

        public List<Game> GetAll() => _db.Games.OrderBy(g => g.Title).ToList();

        public Game? Get(Guid id) => _db.Games.FirstOrDefault(g => g.Id == id);

        public void Add(Game game)
        {
            _db.Games.Add(game);
            _db.SaveChanges();
        }

        public void Update(Game game)
        {
            _db.Games.Update(game);
            _db.SaveChanges();
        }

        public void Remove(Guid id)
        {
            var g = _db.Games.FirstOrDefault(x => x.Id == id);
            if (g != null)
            {
                _db.Games.Remove(g);
                _db.SaveChanges();
            }
        }

        // Helpers para cambiar estado rápido
        public void SetStatus(Guid id, GameStatus status)
        {
            var g = Get(id);
            if (g == null) return;
            g.Status = status;
            if (status == GameStatus.Completed) g.DateCompleted = DateTime.UtcNow;
            else g.DateCompleted = null;
            Update(g);
        }

        // --- Nuevos métodos ---

        // Rellena la base con datos de ejemplo si está vacía
        public void SeedIfEmpty()
        {
            if (_db.Games.Any()) return;

            var now = DateTime.UtcNow;
            var seed = new List<Game>
            {
                new Game { Title = "The Legend of Example", Platform = "Switch", Status = GameStatus.Completed, DateCompleted = now.AddDays(-40), Description = "A classic adventure to learn the basics." },
                new Game { Title = "Space Adventures", Platform = "PC", Status = GameStatus.Playing, Description = "Exploration + roguelite." },
                new Game { Title = "Puzzle Time", Platform = "Mobile", Status = GameStatus.Paused, Description = "Casual puzzles for short sessions." },
                new Game { Title = "Future Wish", Platform = "PS5", Status = GameStatus.Wishlist, Description = "AAA open-world on my wishlist." },
                new Game { Title = "Retro Racer", Platform = "PC", Status = GameStatus.Completed, DateCompleted = now.AddMonths(-2), Description = "Arcade racing with pixel art." },
                new Game { Title = "Mystery Manor", Platform = "PC", Status = GameStatus.Wishlist, Description = "Narrative mystery game." },
                new Game { Title = "Skybound", Platform = "Xbox", Status = GameStatus.Playing, Description = "Action RPG in flight." },
                new Game { Title = "Island Builder", Platform = "Switch", Status = GameStatus.Paused, Description = "Relaxing base-building game." }
            };

            _db.Games.AddRange(seed);
            _db.SaveChanges();
        }

        // Obtener plataformas distintas (para filtros)
        // Obtener plataformas distintas (para filtros) - versión segura para EF Core
        public List<string> GetPlatforms()
        {
            // Traer desde la base sólo la columna Platform (evitamos Trim e IsNullOrEmpty en la parte que EF debe traducir)
            var raw = _db.Games
                .Where(g => g.Platform != null)   // esta condición se traduce a SQL correctamente
                .Select(g => g.Platform!)
                .ToList(); // materializamos: a partir de aquí trabajamos en memoria

            // Ahora limpiamos/trimeamos y hacemos Distinct/Order en memoria
            var cleaned = raw
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p)
                .ToList();

            return cleaned;
        }

    }
}
