using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks; // <-- Importante: Añadir para usar async/await
using CsvHelper;
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
            _db.Database.EnsureCreated(); // Esto puede seguir siendo síncrono, solo se ejecuta una vez.
        }

        // --- MÉTODOS ASÍNCRONOS ACTUALIZADOS ---

        public async Task<List<Game>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _db.Games
                .OrderBy(g => g.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); // <-- Cambia a ToListAsync
        }

        public async Task<int> CountAsync()
        {
            return await _db.Games.CountAsync(); // <-- Cambia a CountAsync
        }
        
        public async Task<Game?> GetAsync(Guid id)
        {
            return await _db.Games.FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task AddAsync(Game game)
        {
            await _db.Games.AddAsync(game);
            await _db.SaveChangesAsync(); // <-- Cambia a SaveChangesAsync
        }

        public async Task UpdateAsync(Game game)
        {
            _db.Games.Update(game);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAsync(Guid id)
        {
            var g = await _db.Games.FirstOrDefaultAsync(x => x.Id == id); // <-- Cambia a FirstOrDefaultAsync
            if (g != null)
            {
                _db.Games.Remove(g);
                await _db.SaveChangesAsync();
            }
        }

        public async Task SetStatusAsync(Guid id, GameStatus status)
        {
            var g = await _db.Games.FirstOrDefaultAsync(x => x.Id == id);
            if (g == null) return;
            
            g.Status = status;
            if (status == GameStatus.Completed)
            {
                g.DateCompleted = DateTime.UtcNow;
            }
            else
            {
                g.DateCompleted = null;
            }
            await UpdateAsync(g);
        }

        public async Task<List<string>> GetPlatformsAsync()
        {
            return await _db.Games
                .Where(g => g.Platform != null && g.Platform != "")
                .Select(g => g.Platform)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        // El método SeedIfEmpty no necesita ser asíncrono porque solo se ejecuta una vez al inicio
        // y es aceptable que bloquee el hilo principal durante ese breve momento.
        public void SeedIfEmpty()
        {
            if (_db.Games.Any())
            {
                return;
            }
            
            // Tu lógica de lectura de CSV va aquí...
            var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games_data.csv");
            if (!File.Exists(csvPath))
            {
                return;
            }
            
            var gamesToAdd = new List<Game>();
            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<dynamic>().ToList();
                foreach (var record in records)
                {
                    var game = new Game();
                    game.Title = record.name;
                    string platformsString = record.platforms ?? "";
                    game.Platform = platformsString.Split(',').FirstOrDefault()?.Trim() ?? "N/A";
                    int.TryParse(record.added_status_playing, out int playingCount);
                    int.TryParse(record.added_status_beaten, out int beatenCount);
                    int.TryParse(record.added_status_toplay, out int toplayCount);

                    if (playingCount > 0) game.Status = GameStatus.Playing;
                    else if (beatenCount > 0) game.Status = GameStatus.Completed;
                    else if (toplayCount > 0) game.Status = GameStatus.Wishlist;
                    else game.Status = GameStatus.Wishlist;

                    if (game.Status == GameStatus.Completed)
                    {
                        if (DateTime.TryParse(record.updated, out DateTime updatedDate))
                        {
                            game.DateCompleted = updatedDate.ToUniversalTime();
                        }
                    }
                    game.Description = $"Genres: {record.genres}";
                    gamesToAdd.Add(game);
                }
            }
            
            if (gamesToAdd.Any())
            {
                _db.Games.AddRange(gamesToAdd);
                _db.SaveChanges();
            }
        }
    }
}