using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BoardSquares.Models;

namespace BoardSquares.ViewModels
{
    public class ExactTeamsViewModel
    {
        public List<ExactTeam> ExactTeamList { get; set; }

        public ExactTeamsViewModel()
        {
            ExactTeamList = new List<ExactTeam>();
        }

        public void GetExactTeams()
        {
            var db = new BoardSquaresRepository();
            ExactTeamList = db.Context.Database.SqlQuery(typeof(ExactTeam), "EXEC FP_ExactTeamsByGame").ToListAsync().Result.Select(r => r as ExactTeam).ToList();
        }
    }

    public class ExactTeam
    {
        public string GameNumber { get; set; }
        public string TeamName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string MatchedTeamName { get; set; }
        public DateTime MatchedCreatedDate { get; set; }
        public string MatchedName { get; set; }
        public string MatchedEmail { get; set; }
    }
}