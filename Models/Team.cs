using System.ComponentModel.DataAnnotations;

namespace BoardSquares.Models
{
    public class Team
    {
        [Key]
        public int TeamID { get; set; }

        public string TeamName { get; set; }
        public string TeamAbbr { get; set; }
        public bool Active { get; set; }
    }
}