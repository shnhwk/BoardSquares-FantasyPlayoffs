using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoardSquares.Models
{
    public class TeamScoringByPlayer
    {
        public string GameNumber { get; set; }
        public int UserTeamID { get; set; }
        public string UserTeamName { get; set; }
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int PlayerID { get; set; }
        public string PlayerTeam { get; set; }
        public string PlayerTeamName { get; set; }
        public string PlayerPosition { get; set; }
        public string PlayerFirstName { get; set; }
        public string PlayerLastName { get; set; }
        public int TotalPoints { get; set; }
        public bool TieBreakerPlayer { get; set; }
        public int TiebreakerSort { get; set; }
    }
}