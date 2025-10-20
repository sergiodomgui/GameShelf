using System.Collections.Generic;
using System.Linq;
using GameShelfWeb.Models;
using System.Text.Json;
using System.IO;

namespace GameShelfWeb.Data
{
    public class GameService
    {
        private readonly string _filePath = "games.json";
        private List<Game> _games;

        public GameService()
        {
            _games = Load();
        }

        public List<Game> GetAll() => _games;

        public void Add(Game game)
        {
            _games.Add(game);
            Save();
        }

        public void Update(Game game)
        {
            var index = _games.FindIndex(g => g.Id == game.Id);
            if (index >= 0) _games[index] = game;
            Save();
        }

        public void Remove(Guid id)
        {
            _games.RemoveAll(g => g.Id == id);
            Save();
        }

        private List<Game> Load()
        {
            if (!File.Exists(_filePath)) return new List<Game>();
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Game>>(json) ?? new List<Game>();
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_games, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}
