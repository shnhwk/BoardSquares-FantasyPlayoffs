using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BoardSquares.Models;

namespace BoardSquares.ViewModels
{
    public class GameSummaryViewModel
    {
        public List<GameSummaryTeam> TeamList { get; set; }
        public User User { get; set; }
        public string GameNumber { get; set; }
        public List<string> GamesList { get; set; }
        public Dictionary<string, string> GamesDropDown { get; set; }
        public string SelectedGame { get; set; }
        public string ErrorMessage { get; set; }

        public GameSummaryViewModel()
        {
            Init();
        }

        private void Init()
        {
            TeamList = new List<GameSummaryTeam>();
            GamesList = new List<string>();
            User = new User();
            GameNumber = string.Empty;
            GamesDropDown = new Dictionary<string, string>();
            ErrorMessage = string.Empty;
        }

        public void GetAllGames(string gameNumber)
        {
            var db = new BoardSquaresRepository();
            var teams = db.Context.Database.SqlQuery(typeof(TeamScoringByPlayer), String.Format("SELECT a.*, CASE WHEN t.ID IS NULL THEN -1 ELSE t.ID END as TiebreakerSort FROM FP_TeamScoringSummaryByPlayerView a join FP_GameNumbers g on g.GameNumber = a.GameNumber left join FP_UserTeamTieBreakerPlayers t on a.PlayerID = t.PlayerID AND a.UserTeamID = t.UserTeamID WHERE g.Active = 1 AND GETDATE() > g.CloseDate AND a.GameNumber = '{0}' ", gameNumber)).ToListAsync().Result.Select(r => r as TeamScoringByPlayer).GroupBy(r => r.UserTeamID);
            foreach (var team in teams)
            {
                TeamList.Add(new GameSummaryTeam
                {
                    TeamName = team.First().UserTeamName,
                    TeamTotal = team.Where(r => !r.TieBreakerPlayer).Sum(r => r.TotalPoints),
                    //TieBreakerTotal = team.Where(r => r.TieBreakerPlayer).Sum(r => r.TotalPoints),
                    TeamPlayers = team.Where(r => !r.TieBreakerPlayer).OrderByDescending(r => r.TotalPoints).ToList(),
                    TieBreakerPlayers = team.Where(r => r.TieBreakerPlayer).OrderBy(r => r.TiebreakerSort).ToList(),
                    TieBreaker1Score = team.Where(r => r.TieBreakerPlayer).OrderBy(r => r.TiebreakerSort).ToList().First().TotalPoints,
                    TieBreaker2Score = team.Where(r => r.TieBreakerPlayer).OrderBy(r => r.TiebreakerSort).ToList().Last().TotalPoints,
                });
            }
            TeamList = TeamList.OrderByDescending(r => r.TeamTotal).ThenByDescending(r => r.TieBreaker1Score).ThenByDescending(r => r.TieBreaker2Score).ToList();
            SelectedGame = gameNumber;
        }

        public void BuidDropdowns()
        {
            var db = new BoardSquaresRepository();
            db.GetGamesByUser(User.UserID).ForEach(c => GamesDropDown.Add(c, c)); 

        }
    }

    public class GameSummaryTeam
    {
        public string TeamName { get; set; }
        public int TeamTotal { get; set; }
        //public int TieBreakerTotal { get; set; }
        public List<TeamScoringByPlayer> TeamPlayers { get; set; }
        public List<TeamScoringByPlayer> TieBreakerPlayers { get; set; }
        //public int TieBreaker1 { get; set; }
        //public int TieBreaker2 { get; set; }
        public int TieBreaker1Score { get; set; }
        public int TieBreaker2Score { get; set; }
    }
}