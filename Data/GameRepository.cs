using System;
using System.Collections.Generic;
using System.Globalization; // Necesario para CsvHelper
using System.IO;            // Necesario para leer archivos
using System.Linq;
using CsvHelper;            // Librería para leer CSV
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
        
        // Rellena la base con datos del CSV si está vacía (VERSIÓN CON DEPURACIÓN)
        public void SeedIfEmpty()
        {
            Console.WriteLine("--- [DEBUG] Ejecutando SeedIfEmpty... ---");

            if (_db.Games.Any())
            {
                Console.WriteLine("[DEBUG] La base de datos ya tiene datos. No se hará nada.");
                return;
            }
            
            Console.WriteLine("[DEBUG] La base de datos está vacía. Intentando cargar desde CSV.");

            // Ruta al archivo CSV.
            var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games_data.csv");
            Console.WriteLine($"[DEBUG] Buscando archivo CSV en la ruta: {csvPath}");

            if (!File.Exists(csvPath))
            {
                Console.WriteLine("[DEBUG] ¡ERROR! No se encontró el archivo CSV en esa ruta.");
                return;
            }
            
            Console.WriteLine("[DEBUG] Archivo CSV encontrado. Procediendo a leerlo.");
            var gamesToAdd = new List<Game>();

            try
            {
                using (var reader = new StreamReader(csvPath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<dynamic>().ToList();
                    Console.WriteLine($"[DEBUG] Se encontraron {records.Count} filas en el archivo CSV.");

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
                    Console.WriteLine($"[DEBUG] Se van a añadir {gamesToAdd.Count} juegos a la base de datos.");
                    _db.Games.AddRange(gamesToAdd);
                    _db.SaveChanges();
                    Console.WriteLine("[DEBUG] ¡ÉXITO! Los datos se guardaron en la base de datos.");
                }
                else
                {
                    Console.WriteLine("[DEBUG] Aunque se leyó el CSV, no se procesó ningún juego para añadir.");
                }
            }
            catch (Exception ex)
            {
                // Esta línea es crucial. Si hay un error al leer el CSV, lo mostrará.
                Console.WriteLine($"[DEBUG] ¡ERROR CATASTRÓFICO! Falló la lectura del CSV: {ex.Message}");
            }
            
            Console.WriteLine("--- [DEBUG] SeedIfEmpty ha finalizado. ---");
        }

        // Obtener plataformas distintas (para filtros)
        public List<string> GetPlatforms()
        {
            var raw = _db.Games
                .Where(g => g.Platform != null)
                .Select(g => g.Platform!)
                .ToList();

            var cleaned = raw
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p)
                .ToList();

            return cleaned;
        }

        // --- Añade estos dos nuevos métodos DENTRO de la clase GameRepository ---

        // Método para contar el total de juegos en la base de datos
        public int Count() => _db.Games.Count();

        // Método para obtener una página específica de juegos
        public List<Game> GetPaged(int pageNumber, int pageSize)
        {
            // pageNumber = 1 significa la primera página.
            // Skip omite los juegos de las páginas anteriores.
            // Take coge solo el número de juegos para la página actual.
            return _db.Games
                .OrderBy(g => g.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}