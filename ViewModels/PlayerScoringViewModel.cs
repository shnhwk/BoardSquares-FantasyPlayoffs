using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BoardSquares.Models;

namespace BoardSquares.ViewModels
{
    public class PlayerScoringViewModel
    {
        public List<PlayerScoring> ScoringEvents { get; set; }
        [Display(Name = "Round")]
        public string SelectedRound { get; set; }
        [Display(Name = "Player Team")]
        public string SelectedTeam { get; set; }
        [Display(Name = "Player")]
        public int SelectedPlayer { get; set; }
        [Display(Name = "Year")]
        public int SelectedYear { get; set; }
        public Dictionary<string, string> TeamsDictionary { get; set; }
        public Dictionary<string, string> PlayersDictionary { get; set; }
        public Dictionary<string, string> RoundsDictionary { get; set; }
        public bool PlayerSelectionVisible { get; set; }
        public string EventCommand { get; set; }

        public PlayerScoringViewModel()
        {
            ScoringEvents = new List<PlayerScoring>();
            PlayerSelectionVisible = true;
            SelectedYear = GetYear();
            GetTeams();
            GetRounds();
        }

        private void GetRounds()
        {
            var rounds = new Dictionary<string,string>();
            rounds.Add("0", "All");
            rounds.Add("1", "Wild Card");
            rounds.Add("2", "Divisional");
            rounds.Add("3", "Conference");
            rounds.Add("4", "Superbowl");
            RoundsDictionary = rounds;
        }

        public void GetPlayerScoring()
        {
            var db = new BoardSquaresRepository();
            ScoringEvents = db.Context.Database.SqlQuery(typeof(PlayerScoring),
                String.Format("SELECT * FROM FP_PlayerPointsByRoundView WHERE (PlayerID IN ({0}) OR {0} = 0)AND (RoundID = {1} OR {1} = 0) ", SelectedPlayer, SelectedRound))
                .ToListAsync()
                .Result
                .Select(r => r as PlayerScoring)
                .OrderBy(r => r.RoundID)
                .ThenBy(r => r.ScoringEventID)
                .ToList();
            PlayerSelectionVisible = false;
        }

        public void GetTeams()
        {
            var db = new BoardSquaresRepository();
            TeamsDictionary = db.GetAllNFLTeams();
        }
        public void GetPlayers()
        {
            var db = new BoardSquaresRepository();
            PlayersDictionary = db.GetPlayerDictionaryByTeamAndPosition(SelectedTeam,"All");
        }
        public int GetYear()
        {
            var db = new BoardSquaresRepository();
            var games = db.Context.Games.Where(r => r.Active).ToList();
            return games.Any() ? games.Max(r => r.Year) : DateTime.Now.Year;
        }
    }
}