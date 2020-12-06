using System.ComponentModel.DataAnnotations;

namespace BoardSquares.Models
{
    public class UserTeamTieBreakerPlayers
    {
        [Key]
        public int ID { get; set; }
        public int UserTeamID { get; set; }
        public int PlayerID { get; set; }
    }
}