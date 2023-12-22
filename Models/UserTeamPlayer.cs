using System.ComponentModel.DataAnnotations;

namespace BoardSquares.Models
{
    public class UserTeamPlayer
    {
        [Key]
        public int ID { get; set; }
        public int UserTeamID { get; set; }
        public int PlayerID { get; set; }

        public virtual Player Player { get; set; }


    }
}
