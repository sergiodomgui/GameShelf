using System;

namespace GameShelfWeb.Models
{
    public enum GameStatus
    {
        Wishlist,
        Playing,
        Paused,
        Completed
    }

    public class Game
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public GameStatus Status { get; set; } = GameStatus.Wishlist;
        public DateTime? DateCompleted { get; set; }
    }
}
