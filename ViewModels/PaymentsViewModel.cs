using System;
using System.Collections.Generic;
using System.Linq;
using BoardSquares.Models;

namespace BoardSquares.ViewModels
{
    public class PaymentsViewModel
    {
        public Dictionary<string, string> UsersDropdown { get; set; }
        public List<UserTeam> UserTeamList { get; set; }
        public List<User> UserList { get; set; }
        public string Entity { get; set; }
        public string EventArgument { get; set; }
        public string EventCommand { get; set; }
        public bool IsUsersSectionVisible { get; set; }
        public bool IsUserTeamSectionVisible { get; set; }

        public PaymentsViewModel()
        {
            Init();
        }

        private void Init()
        {
            UserTeamList = new List<UserTeam>();
            UserList = new List<User>();
            UsersDropdown = new Dictionary<string, string>();
            Entity = string.Empty;
            EventCommand = string.Empty;
            EventArgument = string.Empty;
            IsUsersSectionVisible = true;
            IsUserTeamSectionVisible = false;
        }

        public void GetUsers()
        {
            BoardSquaresRepository db = new BoardSquaresRepository();
            UserList = db.GetAllUsers();
            var userTeamList = db.Context.UserTeams.Where(g => g.Balance > 0).ToList();
            foreach (var user in UserList)
            {
                user.BalDue = 0;
                var teams = userTeamList.Where(g => g.UserID == user.UserID).Select(r => r.Balance).ToList();
                if (teams.Any())
                {
                    user.BalDue = Decimal.Round(teams.Sum(r => r),2);
                }
                
            }
            UserList = UserList.OrderByDescending(r => r.BalDue).ThenBy(r => r.UserName).ToList();
        }

        public void GetUserTeams(int p)
        {
            BoardSquaresRepository db = new BoardSquaresRepository();
            UserTeamList = db.GetTeamsByUser(p, true);
            IsUserTeamSectionVisible = true;
            IsUsersSectionVisible = false;
        }

        public void UpdateBalanceDue(int p)
        {
            BoardSquaresRepository db = new BoardSquaresRepository();
            UserTeam team = db.Context.UserTeams.Find(p);
            team.Balance = 0;
            db.Context.SaveChanges();
            GetUserTeams(team.UserID);
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

                case "markpaid":
                    UpdateBalanceDue(Convert.ToInt32(EventArgument));
                    break;

                case "deleteuser":
                    DeleteUser(Convert.ToInt32(EventArgument));
                    break;

                case "deleteteam":
                    DeleteUserTeam(Convert.ToInt32(EventArgument));
                    break;

                case "view":
                    GetUserTeams(Convert.ToInt32(EventArgument));
                    break;
            }
            EventCommand = string.Empty;
            EventArgument = string.Empty;
        }

        private void Clear()
        {
            IsUsersSectionVisible = true;
            IsUserTeamSectionVisible = false;
            GetUsers();
        }

        private void DeleteUser(int p)
        {
            BoardSquaresRepository db = new BoardSquaresRepository();
            User userToDelete = db.Context.Users.Find(p);
            db.Context.Users.Remove(userToDelete);
            db.Context.SaveChanges();
            GetUsers();
        }

        private void DeleteUserTeam(int p)
        {
            BoardSquaresRepository db = new BoardSquaresRepository();
            UserTeam teamToDelete = db.Context.UserTeams.Find(p);
            db.Context.UserTeams.Remove(teamToDelete);
            db.Context.SaveChanges();
            GetUserTeams(teamToDelete.UserID);
        }
    }
}