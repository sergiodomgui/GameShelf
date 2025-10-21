using System;
using System.ComponentModel.DataAnnotations;

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
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Platform { get; set; } = string.Empty;

        public GameStatus Status { get; set; } = GameStatus.Wishlist;

        public DateTime? DateCompleted { get; set; }

        public string? Description { get; set; }   // campo extra para ficha
    }
}
