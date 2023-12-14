using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BoardSquares.Models;
using BoardSquares.ViewModels;
using WebGrease.Css.Extensions;

namespace BoardSquares.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
            BoardSquaresRepository = new BoardSquaresRepository();
        }

        public BoardSquaresRepository BoardSquaresRepository { get; set; }

        [Authorize(Roles = "User, Admin")]
        public ActionResult Index()
        {
            var vm = new ScoringSummaryViewModel
            {
                User = BoardSquaresRepository.GetAllUsers().FirstOrDefault(u => u.UserName == User.Identity.Name)
            };

            vm.GetGamesByUser();
            vm.GetTeamDetailsByGame();

            var team = vm.TeamSummariesList.SelectMany(r => r).FirstOrDefault(r => r.UserID == vm.User.UserID);
            if (team == null)
                return RedirectToAction("Games", "Home");

            vm.GetFilteredTeamDetails(team.UserTeamID, team.UserTeamName);
            ViewBag.ScoreTotal = vm.PlayerScoringSummaries.Sum(r => r.PlayerTotal);
            //ViewBag.TieBreakerScoreTotal = vm.TieBreakerScoringSummaries.Sum(r => r.PlayerTotal);
             
            return View(vm);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public ActionResult Index(ScoringSummaryViewModel vm)
        {
            if (vm.EventCommand.Contains("breakdown"))
            {
                var round = vm.EventCommand.Substring(vm.EventCommand.Length - 1);
                var viewModel = new PlayerScoringViewModel
                {
                    SelectedPlayer = Convert.ToInt32(vm.EventArgument),
                    SelectedRound = round,
                    PlayerSelectionVisible = false
                };
                viewModel.GetPlayerScoring();
                return RedirectToAction("PlayerScoring", "Home",
                    new { SelectedPlayer = Convert.ToInt32(vm.EventArgument), SelectedRound = Convert.ToInt32(round) });
            }
            vm.GetGamesByUser();
            vm.GetTeamDetailsByGame();
            var team = vm.TeamSummariesList.SelectMany(r => r)
                .FirstOrDefault(p => p.UserTeamID == Convert.ToInt32(vm.EventArgument));
            vm.GetFilteredTeamDetails(team.UserTeamID, team.UserTeamName);
            ViewBag.ScoreTotal = vm.PlayerScoringSummaries.Sum(r => r.PlayerTotal);
            //ViewBag.TieBreakerScoreTotal = vm.TieBreakerScoringSummaries.Sum(r => r.PlayerTotal);
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Administration()
        {
            ViewBag.Message = "Admin Tools";
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult EditPlayers()
        {
            var viewModel = new PlayerSearchViewModel { Teams = BoardSquaresRepository.GetAllTeams() };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult EditPlayers(PlayerSearchViewModel viewModel)
        {
            viewModel.Teams = BoardSquaresRepository.GetAllTeams();

            viewModel.Players = viewModel.SelectedTeam == "All"
                ? BoardSquaresRepository.GetAllPlayers()
                : BoardSquaresRepository.GetPlayersByTeam(viewModel.SelectedTeam);

            return View(viewModel);
        }

        [Authorize(Roles = "User, Admin")]
        public ActionResult Games()
        {
            var viewModel = new UserGamesViewModel
            {
                User = BoardSquaresRepository.GetAllUsers().FirstOrDefault(u => u.UserName == User.Identity.Name)
            };

            if (viewModel.User == null)
            {
                ViewBag.ErrorMessage = "Unable to Find User";
                viewModel.UserTeams = new List<UserTeam>();
                return View(viewModel);
            }
            viewModel.GetUserTeams();
            return View(viewModel);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public ActionResult Games(UserGamesViewModel viewModel)
        {
            ViewBag.Errors = new[] { "", "", "" };
            if (viewModel.EventCommand == "save")
            {

                //populate teams

                var allPlayers = BoardSquaresRepository.GetAllPlayers();
                if (allPlayers?.Any() == true)
                {
                    viewModel.DEFPlayer.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.DEFPlayer.PlayerID)?.PlayerTeam;
                    viewModel.QB1Player.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.QB1Player.PlayerID)?.PlayerTeam;
                    viewModel.QB2Player.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.QB2Player.PlayerID)?.PlayerTeam;
                    viewModel.RB1Player.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.RB1Player.PlayerID)?.PlayerTeam;
                    viewModel.RB2Player.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.RB2Player.PlayerID)?.PlayerTeam;
                    viewModel.RB3Player.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.RB3Player.PlayerID)?.PlayerTeam;
                    viewModel.RB4Player.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.RB4Player.PlayerID)?.PlayerTeam;
                    viewModel.WR1Player.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.WR1Player.PlayerID)?.PlayerTeam;
                    viewModel.WR2Player.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.WR2Player.PlayerID)?.PlayerTeam;
                    viewModel.WR3Player.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.WR3Player.PlayerID)?.PlayerTeam;
                    viewModel.WR4Player.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.WR4Player.PlayerID)?.PlayerTeam;
                    viewModel.WR5Player.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.WR5Player.PlayerID)?.PlayerTeam;
                    viewModel.TEPlayer.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.TEPlayer.PlayerID)?.PlayerTeam;
                    viewModel.KPlayer.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.KPlayer.PlayerID)?.PlayerTeam;
                    viewModel.TieBreakerPlayer1.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.TieBreakerPlayer1.PlayerID)?.PlayerTeam;
                    viewModel.TieBreakerPlayer2.PlayerTeam = allPlayers.FirstOrDefault(t => t.PlayerID == viewModel.TieBreakerPlayer2.PlayerID)?.PlayerTeam;
                }

                //validation
                var playerList = viewModel.GetCombinedPlayers();
                var duplicateList =
                    playerList.Where(g => g.PlayerTeam != null)
                        .GroupBy(p => p.PlayerTeam)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .ToList();
                var isInvalidTieBreaker =
                    playerList.Any(
                        r =>
                            r.PlayerID == viewModel.TieBreakerPlayer1.PlayerID ||
                            r.PlayerID == viewModel.TieBreakerPlayer2.PlayerID ||
                            viewModel.TieBreakerPlayer1.PlayerID == viewModel.TieBreakerPlayer2.PlayerID);
                //playerList.Select(r => r.PlayerID).ToList().Contains(viewModel.TieBreakerPlayer1.PlayerID || viewModel.TieBreakerPlayer2.PlayerID);
                playerList.Add(viewModel.TieBreakerPlayer1);
                playerList.Add(viewModel.TieBreakerPlayer2);
                var nullList = playerList.Where(p => (p == null) || (p.PlayerID == 0)).ToList();
                if (duplicateList.Any() || nullList.Any() || isInvalidTieBreaker)
                {

                    if (nullList.Any())
                    {
                        viewModel.ErrorMessage = "Must select a player for all positions";
                    }
                    else if (duplicateList.Any())
                    {
                        viewModel.ErrorMessage = "Cannot have more than one player from team(s): " +
                                                 duplicateList.Aggregate((current, next) => current + ", " + next);
                    }
                    else if (isInvalidTieBreaker)
                    {
                        viewModel.ErrorMessage = "Tie Breaker player cannot already be a member of your team";
                    }

                    viewModel.IsValid = false;
                    ModelState.Clear();


                    return View(viewModel);
                }
            }

            if (viewModel.EventCommand == "viewGame")
            {
                return RedirectToAction("GameSummary", "Home", new { gameNumber = viewModel.EventArgument });
            }

            viewModel.HandleRequest();
            ModelState.Clear();
            ViewBag.ScoreTotal = viewModel.PlayerScoringSummaries == null || !viewModel.PlayerScoringSummaries.Any()
                ? 0
                : viewModel.PlayerScoringSummaries.Sum(r => r.PlayerTotal);
            //ViewBag.TieBreakerTotal = viewModel.TieBreakerScoringSummaries == null || !viewModel.TieBreakerScoringSummaries.Any()
            //    ? 0
            //    : viewModel.TieBreakerScoringSummaries.Sum(r => r.PlayerTotal);
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ScoringEvents()
        {
            var viewModel = new ScoringEventsViewModel();
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult ScoringEvents(ScoringEventsViewModel viewModel)
        {
            viewModel.HandleRequest();
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Payments()
        {
            var viewModel = new PaymentsViewModel();
            viewModel.GetUsers();
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Payments(PaymentsViewModel viewModel)
        {
            viewModel.HandleRequest();
            ModelState.Clear();
            return View(viewModel);
        }

        [Authorize(Roles = "User, Admin")]
        public ActionResult PlayerScoring(int? SelectedPlayer, int? SelectedRound)
        {
            var vm = new PlayerScoringViewModel();
            if ((SelectedPlayer != null) && (SelectedRound != null))
            {
                vm.SelectedPlayer = SelectedPlayer.Value;
                vm.SelectedRound = SelectedRound.Value.ToString();
                vm.GetPlayerScoring();
            }

            return View(vm);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public ActionResult PlayerScoring(PlayerScoringViewModel viewModel)
        {
            if (viewModel.EventCommand == "back")
                return RedirectToAction("Index", "Home");
            if (viewModel.EventCommand == "cancel")
                return View(new PlayerScoringViewModel());
            viewModel.GetPlayerScoring();
            return View(viewModel);
        }

        [Authorize(Roles = "User, Admin")]
        public ActionResult Rules()
        {
            return View();
        }


        [Authorize(Roles = "User, Admin")]
        public ActionResult GameSummary(string gameNumber = "")
        {
            var vm = new GameSummaryViewModel()
            {
                User = BoardSquaresRepository.GetAllUsers().FirstOrDefault(u => u.UserName == User.Identity.Name),
            };

            if (User == null)
            {
                vm.ErrorMessage =
                    "Please login.";
                return View(vm);
            }


            //get the games list regardless
            vm.GamesList = BoardSquaresRepository.GetClosedGamesByUser(vm.User.UserID);

            if (vm.GamesList.Count == 0)
            {
                vm.ErrorMessage =
                    "Other teams are not available to view until drafting is closed. Please check back later.";
                return View(vm);
            }


            if (!string.IsNullOrEmpty(gameNumber))
            {
                  //if a game number is supplied, get that one
                vm.GetAllGames(gameNumber);
                return View(vm);
            }




            //otherwise just show the first one we have. 
            vm.GetAllGames(vm.GamesList.First());
            return View(vm);

            //var closedGames = BoardSquaresRepository.Context.Games.Where(r => r.Active & r.CloseDate < DateTime.Now).Select(r => r.GameNumber).ToList();
            //vm.GamesList.Where(r => closedGames.Contains(r)).ForEach(c => vm.GamesDropDown.Add(c, c));
            //if (vm.GamesList.Count == 0)
            //{
            //    vm.ErrorMessage =
            //        "Other teams are not available to view until drafting is closed. Please check back later.";
            //}
            //else
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public ActionResult GameSummary(GameSummaryViewModel vm)
        {

            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult SendEmails()
        {
            var model = new SendEmailsViewModel();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult SendEmails(SendEmailsViewModel model)
        {
            model.HandleRequest();
            return View(model);
        }

        [Authorize(Roles = "User, Admin")]
        public ActionResult News()
        {
            var model = new NewsViewModel();
            model.NewsMessages = BoardSquaresRepository.Context.News.OrderByDescending(n => n.CreatedDate).ToList();
            foreach (var newsMessage in model.NewsMessages)
            {
                ViewBag.HtmlStr += "<div class=\"bordered\">"
                    //+ "<p style=\"padding-left: 1cm; padding-top: 5px; font-size:16px;\"><b>" + newsMessage.CreatedDate.ToShortDateString() + "</b> </p> <hr style=\"margin-top:0px; margin-bottom:5px; \">" 
                    + newsMessage.Note + "</div>";
            }
            return View();
        }


        [Authorize(Roles = "Admin")]
        public ActionResult ExactTeams()
        {
            var vm = new ExactTeamsViewModel();
            vm.GetExactTeams();
            return View(vm);
        }


        #region CRUD 

        [HttpPost]
        public ActionResult GetPlayersByTeamAndPosition(string team, string position)
        {
            var playerList = new List<Player>();
            playerList = BoardSquaresRepository.GetPlayersByTeamAndPosition(team, position);
            var dropdownPlayers = new SelectList(playerList, "PlayerID", "FirstLastPosition", 0);
            return Json(dropdownPlayers);
        }

        [HttpPost]
        public ActionResult GetTeams(string[] teams)
        {
            var teamList = BoardSquaresRepository.GetAllNFLTeams().Where(r => !teams.Contains(r.Key));
            return Json(new SelectList(teamList, "Key", "Value", 0));
        }

        [HttpPost]
        public ActionResult GetScoringEvents(string playerId, string roundId)
        {
            var model = new PlayerScoringViewModel
            {
                SelectedPlayer = Convert.ToInt32(playerId),
                SelectedRound = roundId
            };
            model.GetPlayerScoring();
            return Json(model.ScoringEvents);
        }

        [HttpPost]
        public ActionResult GetAllTeams()
        {
            var teamList = BoardSquaresRepository.GetAllNFLTeams();
            var list = new List<string>();
            foreach (var team in teamList)
            {
                list.Add(team.Key);
                list.Add(team.Value);
            }
            return Json(list.ToArray());
        }

        [HttpPost]
        public ActionResult GetPlayersByTeam(string team)
        {
            var playerList = new List<Player>();
            playerList = BoardSquaresRepository.GetPlayersByTeam(team);
            var dropdownPlayers = new SelectList(playerList, "PlayerID", "FirstLastPosition", 0);
            return Json(dropdownPlayers);
        }

        [HttpPost]
        public ActionResult GetUsedPlayersByTeam(string team)
        {
            var playerList = new List<Player>();
            playerList = BoardSquaresRepository.GetUsedPlayers(team);
            var dropdownPlayers = new SelectList(playerList, "PlayerID", "FirstLastPosition", 0);
            return Json(dropdownPlayers);
        }

        #endregion
    }
}