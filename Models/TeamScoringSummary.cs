using System.ComponentModel.DataAnnotations.Schema;
namespace BoardSquares.Models
{
    public class TeamScoringSummary
    {
        public int tempID { get; set; }
        public string GameNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }        
        public int UserID { get; set; }        
        public int UserTeamID { get; set; }
        public string UserTeamName { get; set; }
        public int TeamTotal { get; set; }        
        public int TieBreakerPoints { get; set; }
        public decimal Amount { get; set; }
        public bool IsOpen { get; set; }
        public int TiebreakerOnePoints { get; set; }
        public int TiebreakerTwoPoints { get; set; }

        [NotMapped]
        public int PlayerID { get; set; }
        [NotMapped]
        public int TotalPoints { get; set; }
        [NotMapped]
        public string PlayerTeam { get; set; }
        [NotMapped]
        public string PlayerTeamName { get; set; }
        [NotMapped]
        public string PlayerPosition { get; set; }
        [NotMapped]
        public string PlayerFirstName { get; set; }
        [NotMapped]
        public string PlayerLastName { get; set; }
    }
}