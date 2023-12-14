using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using BoardSquares.Models;

namespace BoardSquares.ViewModels
{
    public class UserGamesViewModel
    {
        public User User { get; set; }
        public List<UserTeam> UserTeams { get; set; }
        [Display(Name = "Game Code")]
        public string NewGameCode { get; set; }
        [Display(Name = "New Team Name")]
        public string NewTeamName { get; set; }
        public string ConfirmationMessage { get; set; }
        public string ErrorMessage { get; set; }
        public string EventCommand { get; set; }
        public string EventArgument { get; set; }
        public bool IsValid { get; set; }
        public string PageMode { get; set; }
        public bool IsNewTeamAreaVisible { get; set; }
        public bool IsTeamListAreaVisible { get; set; }
        public bool IsPlayerDetailVisible { get; set; }
        public bool IsTeamDetailVisible { get; set; }
        public List<UserTeamPlayer> UserTeamPlayers { get; set; }
        public UserTeam Entity { get; set; }
        public ModelStateDictionary Messages { get; set; }
        public Player DEFPlayer { get; set; }
        public Player KPlayer { get; set; }
        public Player QB1Player { get; set; }
        public Player QB2Player { get; set; }
        public Player WR1Player { get; set; }
        public Player WR2Player { get; set; }
        public Player WR3Player { get; set; }
        public Player WR4Player { get; set; }
        public Player WR5Player { get; set; }
        public Player RB1Player { get; set; }
        public Player RB2Player { get; set; }
        public Player RB3Player { get; set; }
        public Player RB4Player { get; set; }
        public Player TEPlayer { get; set; }
        public Player TieBreakerPlayer1 { get; set; }
        public Player TieBreakerPlayer2 { get; set; }
        public List<Team> UsedTeams { get; set; }
        public Dictionary<string, string> AvailableTeams { get; set; }

        public UserGamesViewModel()
        {
            Init();
        }

        #region Init Method

        private void Init()
        {
            User = new User();
            UserTeams = new List<UserTeam>();
            UserTeamPlayers = new List<UserTeamPlayer>();
            NewGameCode = string.Empty;
            ErrorMessage = string.Empty;
            ConfirmationMessage = string.Empty;
            NewTeamName = string.Empty;
            EventCommand = string.Empty;
            EventArgument = string.Empty;
            IsValid = true;
            PageMode = string.Empty;
            Entity = new UserTeam();
            Messages = new ModelStateDictionary();
            UsedTeams = new List<Team>();
            AvailableTeams = GetAvailableTeams();
            IsNewTeamAreaVisible = true;
            IsTeamDetailVisible = false;
            IsTeamListAreaVisible = true;
            IsPlayerDetailVisible = false;
            DEFPlayer = new Player();
            KPlayer = new Player();
            QB1Player = new Player();
            QB2Player = new Player();
            WR1Player = new Player();
            WR2Player = new Player();
            WR3Player = new Player();
            WR4Player = new Player();
            WR5Player = new Player();
            RB1Player = new Player();
            RB2Player = new Player();
            RB3Player = new Player();
            RB4Player = new Player();
            TEPlayer = new Player();
            TieBreakerPlayer1 = new Player();
            TieBreakerPlayer2 = new Player();
        }


        private Dictionary<string, string> GetAvailableTeams()
        {
            BoardSquaresRepository db = new BoardSquaresRepository();
            return db.GetAllNFLTeams();
        }

        #endregion

        #region HandleRequest Method

        public void HandleRequest()
        {
            //LoadSearchCategories();
            //LoadCategories();

            switch (EventCommand.ToLower())
            {
                case "":
                    break;
                case "cancel":
                    Clear();
                    GetUserTeams();
                    break;

                case "edit":
                    EditMode(Convert.ToInt32(EventArgument));
                    break;

                case "save":
                    Save();
                    Clear();
                    GetUserTeams();
                    break;

                case "delete":
                    Delete(Convert.ToInt32(EventArgument));
                    break;

                case "create":
                    CreateNew();
                    GetUserTeams();
                    break;
                case "view":
                    //ViewPlayers(Convert.ToInt32(EventArgument));
                    GetFilteredTeamDetails(Convert.ToInt32(EventArgument));
                    break;
            }
            EventCommand = string.Empty;
            EventArgument = string.Empty;
        }

        public void GetFilteredTeamDetails(int entity)
        {
            var db = new BoardSquaresRepository();
            var temp = db.GetUserTeamPlayersByGame(entity);
            var tieBreaker = db.Context.TieBreakerPlayers.Where(r => r.UserTeamID == entity).Select(r => r.PlayerID).ToList();

            var playerList = temp.Select(r => r.PlayerID).ToList();

            GetPlayerScoringTotalsByRound(playerList);
            GetTieBreakerScoringTotalsByRound(tieBreaker);
            IsNewTeamAreaVisible = false;
            IsTeamDetailVisible = false;
            IsTeamListAreaVisible = false;
            IsPlayerDetailVisible = true;
        }

        public void GetPlayerScoringTotalsByRound(List<int> players)
        {
            var db = new BoardSquaresRepository();
            var instring = string.Join(", ", players);
            PlayerScoringSummaries = db.Context.Database.SqlQuery(typeof(PlayerScoringSummary), String.Format("SELECT * FROM FP_PlayerTotalsByRoundView WHERE PlayerID IN ({0}) ", instring)).ToListAsync().Result.Select(r => r as PlayerScoringSummary).OrderByDescending(r => r.PlayerTotal).ToList();
        }
        public void GetTieBreakerScoringTotalsByRound(List<int> players)
        {
            var db = new BoardSquaresRepository();
            var instring = string.Join(",", players);
            TieBreakerScoringSummaries = db.Context.Database.SqlQuery(typeof(PlayerScoringSummary), String.Format("SELECT * FROM FP_PlayerTotalsByRoundView WHERE PlayerID IN ({0}) ", instring)).ToListAsync().Result.Select(r => r as PlayerScoringSummary).OrderByDescending(r => r.PlayerTotal).ToList();
        }

        public List<PlayerScoringSummary> PlayerScoringSummaries { get; set; }
        public List<PlayerScoringSummary> TieBreakerScoringSummaries { get; set; }

        private void ViewPlayers(int p)
        {
            BoardSquaresRepository db = new BoardSquaresRepository();
            Entity = Get(p);
            UserTeamPlayers = db.GetUserTeamPlayersByGame(Entity.UserTeamID);
            var players = db.Context.Players.Where(r => r.Active).ToList();
            if (UserTeamPlayers.Any())
            {
                foreach (var userTeamPlayer in UserTeamPlayers)
                {
                    var player = players.FirstOrDefault(r => r.PlayerID == userTeamPlayer.PlayerID);
                    switch (player.PlayerPosition)
                    {
                        case "QB":
                            if (QB1Player.PlayerID == 0)
                                QB1Player = player;
                            QB2Player = player;
                            break;
                        case "K":
                            KPlayer = player;
                            break;
                        case "DEF":
                            DEFPlayer = player;
                            break;
                        case "TE":
                            TEPlayer = player;
                            break;
                        case "RB":
                            if (RB1Player.PlayerID == 0)
                            {
                                RB1Player = player;
                            }
                            else
                            {
                                if (RB2Player.PlayerID == 0)
                                {
                                    RB2Player = player;
                                }
                                else
                                {
                                    if (RB3Player.PlayerID == 0)
                                        RB3Player = player;
                                    RB4Player = player;
                                }
                            }
                            break;
                        case "WR":
                            if (WR1Player.PlayerID == 0)
                            {
                                WR1Player = player;
                            }
                            else
                            {
                                if (WR2Player.PlayerID == 0)
                                {
                                    WR2Player = player;
                                }
                                else
                                {
                                    if (WR3Player.PlayerID == 0)
                                    {
                                        WR3Player = player;
                                    }
                                    else
                                    {
                                        if (WR4Player.PlayerID == 0)
                                            WR4Player = player;
                                        WR5Player = player;
                                    }

                                }
                            }
                            break;
                    }
                }
                var tiebreakers = db.GetTieBreakerPlayers(Entity.UserTeamID);
                TieBreakerPlayer1 = tiebreakers.First();
                TieBreakerPlayer2 = tiebreakers.Last();

            }
            IsNewTeamAreaVisible = false;
            IsTeamDetailVisible = false;
            IsTeamListAreaVisible = false;
            IsPlayerDetailVisible = true;
        }

        private void CreateNew()
        {
            if (string.IsNullOrEmpty(NewGameCode) || string.IsNullOrEmpty(NewTeamName))
            {
                ErrorMessage = "Must supply Game Code and New Team Name";
                IsValid = false;
                return;
            }
            BoardSquaresRepository db = new BoardSquaresRepository();
            int result = db.CreateNewTeam(User.UserID, NewTeamName, NewGameCode);
            switch (result)
            {
                case -9:
                    ErrorMessage = "Error when creating user. Please try again later.";
                    IsValid = false;
                    break;
                case -8:
                    ErrorMessage = "An account with specified Email already exists.";
                    IsValid = false;
                    break;
                case -7:
                    ErrorMessage = "Desired User Name already exists. Please try a different one.";
                    IsValid = false;
                    break;
                case -6:
                    ErrorMessage = "Error validating user.";
                    IsValid = false;
                    break;
                case -5:
                    ErrorMessage = "Team Name already exists in game.";
                    IsValid = false;
                    break;
                case -4:
                    ErrorMessage = "Game exists, but is currently full.";
                    IsValid = false;
                    break;
                case -3:
                    ErrorMessage = "Game is Closed.";
                    IsValid = false;
                    break;
                case -2:
                    ErrorMessage = "Game is Inactive.";
                    IsValid = false;
                    break;
                case -1:
                    ErrorMessage = "Game does not exist.";
                    IsValid = false;
                    break;
                case 1:
                    ConfirmationMessage = "Successfully Created " + NewTeamName;
                    NewGameCode = "";
                    NewTeamName = "";
                    break;
                default:
                    ErrorMessage = "Something went wrong. Contact an Admin";
                    IsValid = false;
                    break;
            }
        }

        public void GetUserTeams()
        {
            BoardSquaresRepository db = new BoardSquaresRepository();
            UserTeams = db.GetTeamsByUser(User.UserID, false).OrderByDescending(r => r.Year).ThenByDescending(r => r.CreatedDate).ToList();

            if (!UserTeams.Any()) 
                return;
            


            var userGames = UserTeams.Select(t => t.GameNumber).ToList();
            var games = db.Context.Games.Where(g => userGames.Contains(g.GameNumber));

            foreach (var viewModelUserTeam in UserTeams)
            {
                viewModelUserTeam.Balance = decimal.Round(viewModelUserTeam.Balance, 2);

                var game = games.FirstOrDefault(g => g.GameNumber.Equals(viewModelUserTeam.GameNumber, StringComparison.OrdinalIgnoreCase));


                viewModelUserTeam.IsGameClosed = game?.CloseDate < DateTime.Now;
            }
        }

        private void Clear()
        {
            TieBreakerPlayer1 = new Player();
            TieBreakerPlayer2 = new Player();
            DEFPlayer = new Player();
            KPlayer = new Player();
            QB1Player = new Player();
            QB2Player = new Player();
            WR1Player = new Player();
            WR2Player = new Player();
            WR3Player = new Player();
            WR4Player = new Player();
            WR5Player = new Player();
            RB1Player = new Player();
            RB2Player = new Player();
            RB3Player = new Player();
            RB4Player = new Player();
            TEPlayer = new Player();
            IsNewTeamAreaVisible = true;
            IsTeamDetailVisible = false;
            IsTeamListAreaVisible = true;
            IsPlayerDetailVisible = false;
            NewGameCode = string.Empty;
            NewTeamName = string.Empty;
            EventCommand = string.Empty;
            EventArgument = string.Empty;
            IsValid = true;
            PageMode = string.Empty;
        }

        private void Save()
        {
            BoardSquaresRepository db = new BoardSquaresRepository();

            var hasPlayers = db.Context.UserTeamPlayers.Where(t => t.UserTeamID == Entity.UserTeamID).ToList();

            if (hasPlayers.Any())
            {
                //this means the team was already saved, so we're editing and need to wipe them before re saving the new lineup. 

                db.Context.UserTeamPlayers.RemoveRange(hasPlayers);
                db.Context.SaveChanges();

            }

            var existingTieBreakers = db.Context.TieBreakerPlayers.Where(tb=>tb.UserTeamID == Entity.UserTeamID).ToList();
            if (existingTieBreakers.Any())
            {
                db.Context.TieBreakerPlayers.RemoveRange(existingTieBreakers);
                db.Context.SaveChanges();
            }


            foreach (var player in GetCombinedPlayers())
            {
                var userTeamPlayer = new UserTeamPlayer();
                userTeamPlayer.PlayerID = player.PlayerID;
                userTeamPlayer.UserTeamID = Entity.UserTeamID;
                db.Context.UserTeamPlayers.Add(userTeamPlayer);
            }
            var tieBreaker1 = new UserTeamTieBreakerPlayers();
            tieBreaker1.PlayerID = TieBreakerPlayer1.PlayerID;
            tieBreaker1.UserTeamID = Entity.UserTeamID;
            db.Context.TieBreakerPlayers.Add(tieBreaker1);

            var tieBreaker2 = new UserTeamTieBreakerPlayers();
            tieBreaker2.PlayerID = TieBreakerPlayer2.PlayerID;
            tieBreaker2.UserTeamID = Entity.UserTeamID;
            db.Context.TieBreakerPlayers.Add(tieBreaker2);

            var ut = db.Context.UserTeams.Find(Entity.UserTeamID);
            ut.Complete = true;
            db.Context.SaveChanges();
        }

        private void Delete(int p)
        {
            BoardSquaresRepository db = new BoardSquaresRepository();
            UserTeam teamToDelete = db.Context.UserTeams.Find(p);
            db.Context.UserTeams.Remove(teamToDelete);
            db.Context.SaveChanges();
            GetUserTeams();
        }

        #endregion

        private void EditMode(int p)
        {
            BoardSquaresRepository db = new BoardSquaresRepository();
            Entity = Get(p);
            UserTeamPlayers = db.GetUserTeamPlayersByGame(Entity.UserTeamID);
            if (UserTeamPlayers.Any())
            {
                foreach (var userTeamPlayer in UserTeamPlayers)
                {
                    var player = db.Context.Players.Find(userTeamPlayer.PlayerID);
                    switch (player.PlayerPosition)
                    {
                        case "QB":
                            if (QB1Player.PlayerID == 0)
                                QB1Player = player;
                            QB2Player = player;
                            break;
                        case "K":
                            KPlayer = player;
                            break;
                        case "DEF":
                            DEFPlayer = player;
                            break;
                        case "TE":
                            TEPlayer = player;
                            break;
                        case "RB":
                            if (RB1Player.PlayerID == 0)
                            {
                                RB1Player = player;
                            }
                            else
                            {
                                if (RB2Player.PlayerID == 0)
                                {
                                    RB2Player = player;
                                }
                                else
                                {
                                    if (RB3Player.PlayerID == 0)
                                        RB3Player = player;
                                    RB4Player = player;
                                }
                            }
                            break;
                        case "WR":
                            if (WR1Player.PlayerID == 0)
                            {
                                WR1Player = player;
                            }
                            else
                            {
                                if (WR2Player.PlayerID == 0)
                                {
                                    WR2Player = player;
                                }
                                else
                                {
                                    if (WR3Player.PlayerID == 0)
                                    {
                                        WR3Player = player;
                                    }
                                    else
                                    {
                                        if (WR4Player.PlayerID == 0)
                                            WR4Player = player;
                                        WR5Player = player;
                                    }

                                }
                            }
                            break;
                    }
                }
                var tb = db.GetTieBreakerPlayers(Entity.UserTeamID);
                TieBreakerPlayer1 = tb.First();
                TieBreakerPlayer2 = tb.Last();
            }
            IsTeamDetailVisible = true;
            IsNewTeamAreaVisible = false;
            IsTeamListAreaVisible = false;
            IsPlayerDetailVisible = false;
        }

        public List<Player> GetCombinedPlayers()
        {
            var returnList = new List<Player>();
            returnList.Add(DEFPlayer);
            returnList.Add(RB1Player);
            returnList.Add(RB2Player);
            returnList.Add(RB3Player);
            returnList.Add(RB4Player);
            returnList.Add(KPlayer);
            returnList.Add(TEPlayer);
            returnList.Add(WR1Player);
            returnList.Add(WR2Player);
            returnList.Add(WR3Player);
            returnList.Add(WR4Player);
            returnList.Add(WR5Player);
            returnList.Add(QB1Player);
            returnList.Add(QB2Player);
            return returnList;
        }

        private UserTeam Get(int p)
        {
            BoardSquaresRepository db = new BoardSquaresRepository();
            return db.GetUserTeamById(p);
        }

        public Dictionary<string, string> GetAvailablePlayers(string playerTeam, string playerPosition)
        {
            if (string.IsNullOrEmpty(playerTeam))
            {
                return new Dictionary<string, string>();
            }
            BoardSquaresRepository db = new BoardSquaresRepository();
            return db.GetPlayerDictionaryByTeamAndPosition(playerTeam, playerPosition);
        }
    }
}