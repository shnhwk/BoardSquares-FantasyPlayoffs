using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardSquares.Models
{
    public class Player
    {
        [Key]
        public int PlayerID { get; set; }
        
        public string PlayerPosition { get; set; }
        public string PlayerTeam { get; set; }
        public string PlayerFirstName { get; set; }
        public string PlayerLastName { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
        [NotMapped]
        public string FirstLastPosition { get { return PlayerFirstName + " " + PlayerLastName + " - " + PlayerPosition;} }
        [NotMapped]
        public string FirstLastTeam { get { return PlayerFirstName + " " + PlayerLastName + " - " + PlayerTeam; } }
    }
}