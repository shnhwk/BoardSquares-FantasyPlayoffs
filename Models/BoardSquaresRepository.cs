using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using BoardSquares.ViewModels;

namespace BoardSquares.Models
{
    public class BoardSquaresRepository
    {
        public BoardSquaresRepository()
        {
            Context = new BoardSquaresContext();
        }

        public BoardSquaresContext Context { get; set; }

        public List<Player> GetAllPlayers()
        {
            return Context.Players.Where(t => t.Active).AsNoTracking().ToList();
        }

        public List<Player> GetPlayersByTeam(string team)
        {
            if (team == "ALL")
                return GetAllPlayers();
            return Context.Players.Where(t => t.Active && (t.PlayerTeam == team)).AsNoTracking().ToList();
        }

        public List<string> GetGamesByUser(int user)
        {
            return
                Context.UserTeams.Where(r => (r.UserID == user) && r.Active)
                    .Select(r => r.GameNumber)
                    .Distinct()
                    .ToList();
        }

        public List<string> GetClosedGamesByUser(int user)
        {
            var closedGames = Context.Games.Where(r => r.Active && r.CloseDate < DateTime.Now).Select(r => r.GameNumber).ToList();
                var userGames = Context.UserTeams.Where(r => (r.UserID == user) && r.Active)
                    .Select(r => r.GameNumber)
                    .Distinct()
                    .ToList();
            return userGames.Where(r => closedGames.Contains(r)).ToList();
        }

        public List<Player> GetPlayersByTeamAndPosition(string team, string position)
        {
            if (position == "ALL")
                return GetPlayersByTeam(team);

            return
                Context.Players.Where(t => t.Active && (t.PlayerTeam == team) && (t.PlayerPosition == position))
                    .AsNoTracking().ToList();
        }

        public Dictionary<string, string> GetPlayerDictionaryByTeamAndPosition(string team, string position)
        {
            var teams = new Dictionary<string, string>();
            if (team == "ALL")
                Context.Players.Where(c => c.Active && ((c.PlayerPosition == position) || (position == "ALL")))
                    .GroupBy(c => new {c.PlayerID, c.PlayerFirstName, c.PlayerLastName})
                    .OrderBy(c => c.Key.PlayerLastName)
                    .AsNoTracking()
                    .ToList()
                    .ForEach(
                        c => teams.Add(c.Key.PlayerID.ToString(), c.Key.PlayerFirstName + " " + c.Key.PlayerLastName));
            else
                Context.Players.Where(
                        c =>
                            c.Active && (c.PlayerTeam == team) &&
                            ((c.PlayerPosition == position) || (position == "ALL")))
                    .GroupBy(c => new {c.PlayerID, c.PlayerFirstName, c.PlayerLastName})
                    .OrderBy(c => c.Key.PlayerLastName)
                    .AsNoTracking()
                    .ToList()
                    .ForEach(
                        c => teams.Add(c.Key.PlayerID.ToString(), c.Key.PlayerFirstName + " " + c.Key.PlayerLastName));
            return teams;
        }

        public List<Player> GetPlayersByTeam(List<string> teamsList)
        {
            return Context.Players.Where(t => t.Active && teamsList.Contains(t.PlayerTeam)).AsNoTracking().ToList();
        }

        public List<Player> GetTieBreakerPlayers(int team)
        {
            var players = Context.TieBreakerPlayers.Where(r => r.UserTeamID == team).Select(r => r.PlayerID).ToList();
            if (!players.Any())
                return new List<Player>();
            return Context.Players.Where(r => players.Contains(r.PlayerID)).AsNoTracking().OrderBy(r => r.PlayerLastName).ToList();
        }

        public UserTeam GetUserTeamById(int userTeamId)
        {
            return Context.UserTeams.AsNoTracking().FirstOrDefault(t => t.UserTeamID == userTeamId);
        }

        public List<UserTeamPlayer> GetUserTeamPlayersByGame(int userTeamId)
        {
            return Context.UserTeamPlayers.Where(t => t.UserTeamID == userTeamId).AsNoTracking().ToList();
        }

        public List<Player> GetUsedPlayers(string team)
        {
            var playerlist = Context.UserTeamPlayers
                .Select(o => o.PlayerID)
                .Union(Context.TieBreakerPlayers.Select(t => t.PlayerID))
                .Distinct()
                .ToList();
            return Context.Players.Where(p => (p.PlayerTeam == team) && playerlist.Contains(p.PlayerID)).AsNoTracking().ToList();
        }

        public Dictionary<string, string> GetAllTeams()
        {
            var teams = new Dictionary<string, string> {{"ALL", "ALL"}};
            //var allTeams = Context.Teams.Select(t => t).ToList();
            //var test = true;
            Context.Teams.Where(c => c.Active)
                .GroupBy(c => new {c.TeamAbbr, c.TeamName})
                .OrderBy(c => c.Key.TeamName)
                .ToList()
                .ForEach(c => teams.Add(c.Key.TeamAbbr, c.Key.TeamName));
            return teams;
        }

        public Dictionary<string, string> GetAllNFLTeams()
        {
            //return Context.Teams.Where(t => t.Active).ToList();
            var teams = new Dictionary<string, string>();
            Context.Teams.Where(c => c.Active)
                .GroupBy(c => new {c.TeamAbbr, c.TeamName})
                .OrderBy(c => c.Key.TeamName)
                .AsNoTracking()
                .ToList()
                .ForEach(c => teams.Add(c.Key.TeamAbbr, c.Key.TeamName));
            return teams;
        }

        public List<User> GetAllUsers()
        {
            return Context.Users.Select(u => u).AsNoTracking().AsNoTracking().ToList();
        } //FP17Test


        public int AttemptRegister(RegisterViewModel model)
        {
            // define a new output parameter
            var returnCode = new SqlParameter();
            returnCode.ParameterName = "@ReturnCode";
            returnCode.SqlDbType = SqlDbType.Int;
            returnCode.Direction = ParameterDirection.Output;

            var registerUserResult =
                Context.Database.ExecuteSqlCommand(
                    "EXEC FP_CreateNewUser @UserName, @Password, @FirstName, @LastName, @Email, @GameNumber, @WeeklyEmail, @ReturnCode OUT",
                    new SqlParameter("UserName", model.UserName),
                    new SqlParameter("Password", model.Password),
                    new SqlParameter("FirstName", model.FirstName),
                    new SqlParameter("LastName", model.LastName),
                    new SqlParameter("Email", model.Email),
                    new SqlParameter("GameNumber", model.GameNumber),
                    new SqlParameter("WeeklyEmail", model.WeeklyUpdates),
                    returnCode);
            return (int) returnCode.Value;
        }

        public int AttemptLogin(string email, string password)
        {
            var returnCode = new SqlParameter();
            returnCode.ParameterName = "@ReturnCode";
            returnCode.SqlDbType = SqlDbType.Int;
            returnCode.Direction = ParameterDirection.Output;

            Context.Database.ExecuteSqlCommand(
                "EXEC FP_VerifyAccount @UserEmail, @UserPwd, @ReturnCode OUT",
                new SqlParameter("UserEmail", email),
                new SqlParameter("UserPwd", password),
                returnCode
            );
            return (int) returnCode.Value;
        }

        public int AttemptPasswordReset(int user, string code, string password)
        {
            var result =
                Context.Database.ExecuteSqlCommand(string.Format(
                    "DECLARE @Salt VARCHAR(25) " +
                    "DECLARE @Code VARCHAR(25) " +
                    "SELECT @Salt = Salt, @Code = VerifyCode FROM FP_UserAccounts WHERE UserID = {0} " +
                    "IF @Code = '{1}' " +
                    "BEGIN " +
                    "UPDATE squaresadmin.FP_UserAccounts " +
                    "SET UserPwd =  HASHBYTES('SHA1', @Salt + '{2}'), VerifyCode = NULL " +
                    "WHERE UserID = {0} " +
                    "END"
                    , user, code, password));
            return result;
        }

        public int AttemptPasswordChange(int user, string password)
        {
            var result =
                Context.Database.ExecuteSqlCommand(string.Format(
                    "EXEC [FP_UpdateUserPassword] @UserID = {0}, @NewPassword = '{1}'"
                    , user, password));
            return result;
        }

        public int AttemptUpdateWeeklyEmails(int user, bool emails)
        {
            var result =
                Context.Database.ExecuteSqlCommand(string.Format(
                        "UPDATE FP_Users SET WeeklyEmail = @WeeklyEmail WHERE UserID = {0}", user),
                    new SqlParameter("WeeklyEmail", emails));
            return result;
        }

        public int AssignPasswordResetCode(int user, string code)
        {
            var result =
                Context.Database.ExecuteSqlCommand(
                    string.Format("UPDATE FP_UserAccounts SET VerifyCode = '{0}' WHERE UserID = {1}", code, user)
                );
            return result;
        }

        public User GetUserByID(int userID)
        {
            return Context.Users.AsNoTracking().FirstOrDefault(s => s.UserID == userID);
        }

        public List<UserTeam> GetTeamsByUser(int userId, bool onlyActive)
        {
            return Context.UserTeams.Where(g => g.UserID == userId && (!onlyActive || g.Active )).AsNoTracking().ToList();
        }

        public List<UserTeam> GetAllUserTeams()
        {
            return Context.UserTeams.Where(g => g.Active).AsNoTracking().ToList();
        }

        public int CreateNewTeam(int userId, string teamName, string gameCode)
        {
            //TODO: change the proc to return the results we want... then go back to the controller
            var returnCode = new SqlParameter();
            returnCode.ParameterName = "@ReturnCode";
            returnCode.SqlDbType = SqlDbType.Int;
            returnCode.Direction = ParameterDirection.Output;
            Context.Database.ExecuteSqlCommand(
                    "EXEC FP_CreateNewTeam @UserID, @UserTeamName, @GameNum, @ReturnCode OUT",
                    new SqlParameter("UserID", userId),
                    new SqlParameter("UserTeamName", teamName),
                    new SqlParameter("GameNum", gameCode),
                    returnCode
                );
            return (int) returnCode.Value;
        }

        public void CreateNewScoringEvent(int playerId, int scoringEventId, int round, int val)
        {
            //TODO: change the proc to return the results we want... then go back to the controller
            //var returnCode = new SqlParameter();
            //returnCode.ParameterName = "@ReturnCode";
            //returnCode.SqlDbType = SqlDbType.Int;
            //returnCode.Direction = ParameterDirection.Output;
            var registerUserResult =
                Context.Database.ExecuteSqlCommand(
                    "EXEC FP_AddWorkSheetScoring @PlayerID, @RoundID, @WorksheetEventID, @Value",
                    new SqlParameter("PlayerID", playerId),
                    new SqlParameter("RoundID", round),
                    new SqlParameter("WorksheetEventID", scoringEventId),
                    new SqlParameter("Value", val)
                );
        }

        public List<Message> AttemptGetEmails(int MessageType)
        {
            var returnList = new List<Message>();
            returnList = Context.Database.SqlQuery(typeof(Message),
                    string.Format("EXEC [FP_GetMessageDetailsByType] @MessageType = {0}", MessageType))
                .ToListAsync()
                .Result
                .Select(r => r as Message)
                .ToList();
            return returnList;
        }

        public void AttemptMarkEmailsSent(int batch)
        {
            Context.Database.ExecuteSqlCommand(
                string.Format("UPDATE FP_Messages SET MessageSent = 1 WHERE BatchID = {0}", batch)
            );
        }
    }
}