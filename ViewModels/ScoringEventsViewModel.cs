using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BoardSquares.Models;

namespace BoardSquares.ViewModels
{
    public class ScoringEventsViewModel
    {
        [Display(Name = "Team")]
        public Team Team { get; set; }

        [Display(Name = "Player")]
        public Player Player { get; set; }

        public Dictionary<string, string> NFLTeams { get; set; }
        public List<ScoringPoints> ScoringPoints { get; set; }
        public List<ScoringEvent> ScoringEventsList { get; set; }

        [Display(Name = "Scoring Event")]
        public Dictionary<string, string> Scorings { get; set; }

        [Display(Name = "Scoring Event")]
        public string NewScoringEvent { get; set; }

        [Display(Name = "Value")]
        public int NewScoringEventValue { get; set; }
        public Dictionary<string, string> RoundsDictionary { get; set; }
        public bool IsPlayerSelectionAreaVisible { get; set; }
        public bool IsScoringDetailVisible { get; set; }
        public bool IsNewScoringEventAreaVisible { get; set; }
        public string EventCommand { get; set; }
        public string EventArgument { get; set; }
        public int Round { get; set; }
        public int Year { get; set; }
        public int Entity { get; set; }
        public int RoundTotal { get; set; }

        public ScoringEventsViewModel()
        {
            Init();
        }

        public void Init()
        {
            IsPlayerSelectionAreaVisible = true;
            IsScoringDetailVisible = false;
            IsNewScoringEventAreaVisible = false;
            EventCommand = string.Empty;
            EventArgument = string.Empty;
            Scorings = new Dictionary<string, string>();
            Team = new Team();
            ScoringPoints = new List<ScoringPoints>();
            NewScoringEvent = string.Empty;
            NFLTeams = GetAllNFLTeams();
            Player = new Player();
            Entity = -1;
            Round = 1;
            Year = GetYear();
            RoundTotal = 0;
            NewScoringEventValue = 0;
            ScoringEventsList = new List<ScoringEvent>();
            GetRounds();
        }

        private Dictionary<string, string> GetAllNFLTeams()
        {
            var db = new BoardSquaresRepository();
            return db.GetAllNFLTeams();
        }

        private void GetRounds()
        {
            var rounds = new Dictionary<string, string>();
            rounds.Add("1", "Wild Card");
            rounds.Add("2", "Divisional");
            rounds.Add("3", "Conference");
            rounds.Add("4", "Superbowl");
            RoundsDictionary = rounds;
        }

        public int GetYear()
        {
            var db = new BoardSquaresRepository();
            var games = db.Context.Games.Where(r => r.Active).ToList();
            return games.Any() ? games.Max(r => r.Year) : DateTime.Now.Year;
        }

        public List<ScoringPoints> GetScoringEventsForPlayer(int playerId)
        {
            var db = new BoardSquaresRepository();
            var list =
                db.Context.ScoringPoints.Where(s => s.PlayerID == playerId && s.RoundID == Round && s.Year == Year)
                    .OrderByDescending(s => s.CreatedDate)
                    .ToList();
            var total = 0;
            foreach (var scoringPoint in list)
            {
                scoringPoint.RoundName =
                    RoundsDictionary.First(r => r.Key == scoringPoint.RoundID.ToString()).Value;
                        
                scoringPoint.ScoringEventName =
                    Scorings.FirstOrDefault(k => k.Key == scoringPoint.ScoringEventID.ToString()).Value;
                total += scoringPoint.Points;
            }
            RoundTotal = total;
            return list;
        }

        public void GetScoringsForPlayerPosition()
        {
            var db = new BoardSquaresRepository();
            var returnlist = new Dictionary<string, string>();
            Player = db.Context.Players.Find(Entity);

            if (Player.PlayerPosition == "DEF")
            {
                db.Context.Scorings.Where(s => s.OnlyDEF)
                    .GroupBy(s => new {s.ID, s.EventName})
                    .OrderBy(s => s.Key.ID)
                    .ToList()
                    .ForEach(s => returnlist.Add(s.Key.ID.ToString(), s.Key.EventName.ToString()));
            }
            if (Player.PlayerPosition == "K")
            {
                db.Context.Scorings.Where(
                        s => s.EventCategory == "Passing" || s.EventCategory == "Kicking" || s.EventCategory == "Other" || s.EventCategory == "Rushing")
                    .GroupBy(s => new {s.ID, s.EventName})
                    .OrderBy(s => s.Key.ID)
                    .ToList()
                    .ForEach(s => returnlist.Add(s.Key.ID.ToString(), s.Key.EventName.ToString()));
            }
            if (Player.PlayerPosition == "QB")
            {
                db.Context.Scorings.Where(
                        s => s.EventCategory == "Passing" || s.EventCategory == "Other" || s.EventCategory == "Rushing")
                    .GroupBy(s => new {s.ID, s.EventName})
                    .OrderBy(s => s.Key.ID)
                    .ToList()
                    .ForEach(s => returnlist.Add(s.Key.ID.ToString(), s.Key.EventName.ToString()));
            }
            if (Player.PlayerPosition == "RB")
            {
                db.Context.Scorings.Where(
                        s => s.EventCategory == "Receiving" || s.EventCategory == "Rushing" || s.EventCategory == "Other" || s.EventCategory == "Passing")
                    .GroupBy(s => new {s.ID, s.EventName})
                    .OrderBy(s => s.Key.ID)
                    .ToList()
                    .ForEach(s => returnlist.Add(s.Key.ID.ToString(), s.Key.EventName.ToString()));
            }
            if (Player.PlayerPosition == "WR")
            {
                db.Context.Scorings.Where(
                        s => s.EventCategory == "Receiving" || s.EventCategory == "Other" || s.EventCategory == "Rushing" || s.EventCategory == "Passing")
                    .GroupBy(s => new {s.ID, s.EventName})
                    .OrderBy(s => s.Key.ID)
                    .ToList()
                    .ForEach(s => returnlist.Add(s.Key.ID.ToString(), s.Key.EventName.ToString()));
            }
            if (Player.PlayerPosition == "TE")
            {
                db.Context.Scorings.Where(
                        s => s.EventCategory == "Receiving" || s.EventCategory == "Other" || s.EventCategory == "Rushing" || s.EventCategory == "Passing")
                    .GroupBy(s => new {s.ID, s.EventName})
                    .OrderBy(s => s.Key.ID)
                    .ToList()
                    .ForEach(s => returnlist.Add(s.Key.ID.ToString(), s.Key.EventName.ToString()));
            }

            IsScoringDetailVisible = true;
            IsPlayerSelectionAreaVisible = false;
            Scorings = returnlist;
        }

        public void HandleRequest()
        {
           switch (EventCommand.ToLower())
            {
                case "":
                    break;
                case "cancel":
                    Clear();
                    break;
                case "add":
                    AddPlayerScoring(Convert.ToInt32(Entity));
                    break;
                case "delete":
                    Delete(Convert.ToInt32(EventArgument));
                    break;
                case "view":
                    ViewPlayerScoring(Convert.ToInt32(Entity));
                    break;
            }
            EventCommand = string.Empty;
            EventArgument = string.Empty;
        }

        private void Delete(int id)
        {
            var db = new BoardSquaresRepository();
            var entity = db.Context.ScoringPoints.Find(id);
            db.Context.ScoringPoints.Remove(entity);
            db.Context.SaveChanges();
            ViewPlayerScoring(Entity);
        }

        private void ViewPlayerScoring(int player)
        {
            GetScoringsForPlayerPosition();
            ScoringPoints = GetScoringEventsForPlayer(player);
            GetScoringNewEventGrid();
            IsScoringDetailVisible = true;
            IsPlayerSelectionAreaVisible = false;
            IsNewScoringEventAreaVisible = true;
        }

        private void Clear()
        {
            EventCommand = string.Empty;
            EventArgument = string.Empty;
            IsScoringDetailVisible = false;
            IsPlayerSelectionAreaVisible = true;
            IsNewScoringEventAreaVisible = false;
        }

        private void AddPlayerScoring(int playerId)
        {
            var db = new BoardSquaresRepository();
            foreach (var score in ScoringEventsList.Where(s => s.Value > -1))
            {
                db.CreateNewScoringEvent(playerId, Convert.ToInt32(score.ScoringEventID), Round, score.Value);
            }
            GetScoringsForPlayerPosition();
            ScoringPoints = GetScoringEventsForPlayer(playerId);
            IsScoringDetailVisible = true;
            IsPlayerSelectionAreaVisible = false;
            IsNewScoringEventAreaVisible = true;
        }

        private void GetScoringNewEventGrid()
        {
            var scoringEvents = new List<ScoringEvent>();
            foreach (var scoring in Scorings)
            {
                var newEvent = new ScoringEvent();
                newEvent.ScoringType = scoring.Value;
                newEvent.ScoringEventID = scoring.Key;
                newEvent.Value = -1;
                scoringEvents.Add(newEvent);
            }
            ScoringEventsList = scoringEvents;
        }
    }
}