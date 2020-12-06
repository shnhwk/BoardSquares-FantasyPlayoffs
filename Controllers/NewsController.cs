using BoardSquares.Models;
using BoardSquares.ViewModels;
using System.Linq;
using System.Web.Mvc;

namespace BoardSquares.Controllers
{
    [Authorize(Roles = "User, Admin")]
    public class NewsController : Controller
    {
        public BoardSquaresRepository BoardSquaresRepository { get; set; }

        public NewsController()
        {
            BoardSquaresRepository = new BoardSquaresRepository();
        }

        // GET: News
        public ActionResult Index()
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
    }
}