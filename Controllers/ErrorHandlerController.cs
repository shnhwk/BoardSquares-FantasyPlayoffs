using System.Web.Mvc;

namespace BoardSquares.Controllers
{
    public class ErrorHandlerController : Controller
    {
        // GET: ErrorHandler
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult NotFound()
        {
            return View();
        }

    }
}