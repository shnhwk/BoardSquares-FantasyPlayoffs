using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardSquares.Models
{
    public class UserTeam
    {
        [Key]
        public int UserTeamID { get; set; }
        public int UserID { get; set; }
        public string UserTeamName { get; set; }
        public int Year { get; set; }
        public string GameNumber { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Complete { get; set; }
        public decimal Balance { get; set; }

        [NotMapped]
        public bool IsGameClosed { get; set; }
    }
}
