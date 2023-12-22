using System;
using System.Collections.Generic;
using System.Linq;
using BoardSquares.Models;

namespace BoardSquares.ViewModels
{
    public class ScoringSummaryViewModel
    {
        public List<PlayerScoringSummary> PlayerScoringSummaries { get; set; }
        public string Entity { get; set; }
        public string EventArgument { get; set; }
        public string EventCommand { get; set; }
        public string SelectedTeam { get; set; }
        public int SelectedTeamID { get; set; }
        public User User { get; set; }
        public List<string> UserGames { get; set; }
        public List<List<TeamScoringSummary>> TeamSummariesList { get; set; }
        public List<PlayerScoringSummary> TieBreakerScoringSummaries { get; set; }
        public ScoringSummaryViewModel()
        {
            Init();
        }

        private void Init()
        {
            PlayerScoringSummaries = new List<PlayerScoringSummary>();
            TieBreakerScoringSummaries = new List<PlayerScoringSummary>();
            User = new User();
            Entity = string.Empty;
            EventCommand = string.Empty;
            EventArgument = string.Empty;
            SelectedTeam = string.Empty;
            SelectedTeamID = -1;
            TeamSummariesList = new List<List<TeamScoringSummary>>();
        }

        public void GetPlayerScoringTotalsByRound(List<int> players)
        {
            var db = new BoardSquaresRepository();
            var inString = string.Join(", ", players);

            PlayerScoringSummaries = !players.Any()
                ? new List<PlayerScoringSummary>()
                : db.Context.Database
                    .SqlQuery(typeof(PlayerScoringSummary),
                        $"SELECT * FROM FP_PlayerTotalsByRoundView WHERE PlayerID IN ({inString}) ")
                    .ToListAsync().Result.Select(r => r as PlayerScoringSummary).OrderByDescending(r => r.PlayerTotal)
                    .ToList();
        }

        public void GetTieBreakerScoringTotalsByRound(List<int> players, int teamId)
        {
            var db = new BoardSquaresRepository();
 
            var inString = string.Join(", ", players);

            TieBreakerScoringSummaries = !players.Any()
                ? new List<PlayerScoringSummary>()
                : db.Context.Database.SqlQuery(typeof(PlayerScoringSummary),
                        $"select p.*,t.ID AS TieBreakerSort from FP_PlayerTotalsByRoundView p left join FP_UserTeamTieBreakerPlayers t on p.PlayerID = t.PlayerID WHERE p.PlayerID IN ({inString}) AND t.UserTeamID = {teamId} ").ToListAsync().Result.Select(r => r as PlayerScoringSummary)
                    .OrderBy(r => r.TieBreakerSort).ToList();
        }
         

        public void GetGamesByUser()
        {
            var db = new BoardSquaresRepository();
            UserGames = db.GetGamesByUser(User.UserID);
        }

        public void GetTeamDetailsByGame()
        {
            var db = new BoardSquaresRepository();
            foreach (var userGame in UserGames)
            {
                var temp =
                    db.Context.Database.SqlQuery(typeof(TeamScoringSummary),
                            String.Format("EXEC FP_GetRankings_Ties @GameNumber = '{0}'", userGame))
                        .ToListAsync()
                        .Result
                        .Select(r => r as TeamScoringSummary)
                        .OrderBy(r => r.tempID)
                        .ThenByDescending(r => r.TieBreakerPoints)
                        .ToList();
                    
                temp.All(p =>
                {
                    p.Amount = Decimal.Round(p.Amount, 2);
                    return true;
                });
                TeamSummariesList.Add(temp);
            }
        }

        public void GetFilteredTeamDetails(int entity, string teamName)
        {
            var db = new BoardSquaresRepository();
            var playerList = db.GetUserTeamPlayersByGame(entity).Select(r => r.PlayerID).ToList();
            var tieBreaker = db.GetTieBreakerPlayers(entity);//db.Context.TieBreakerPlayers.Where(r => r.UserTeamID == entity).Select(r => r.PlayerID).ToList();
            SelectedTeam = teamName;
            SelectedTeamID = entity;
            //var playerList = temp.Select(r => r.PlayerID).ToList();
            var tiebreakerList = tieBreaker.Select(r => r.PlayerID).ToList();
            //var regularPlayerList = playerList.Where(r => !tiebreakerList.Contains(r)).ToList();
            GetPlayerScoringTotalsByRound(playerList);
            GetTieBreakerScoringTotalsByRound(tiebreakerList, SelectedTeamID);
        }
    }
}