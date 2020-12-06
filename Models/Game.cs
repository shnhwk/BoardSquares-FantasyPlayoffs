using System;
using System.ComponentModel.DataAnnotations;


namespace BoardSquares.Models
{
    public class Game
    {
        [Key]
        public int GameID { get; set; }

        public string GameNumber { get; set; }
        public string GameName { get; set; }
        public bool Active { get; set; }
        public int Year { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime CloseDate { get; set; }

    }
}
