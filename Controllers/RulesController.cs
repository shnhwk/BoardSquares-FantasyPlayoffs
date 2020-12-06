using System.Web.Mvc;

namespace BoardSquares.Controllers
{
    [Authorize(Roles = "User, Admin")]
    public class RulesController : Controller
    {
        // GET: Rules
        public ActionResult Index()
        {
            return View();
        }
    }
}