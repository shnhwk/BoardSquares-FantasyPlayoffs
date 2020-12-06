using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using BoardSquares.Models;
using BoardSquares.ViewModels;

namespace BoardSquares.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public BoardSquaresRepository BoardSquaresRepository { get; set; }

        public AccountController()
        {
            BoardSquaresRepository = new BoardSquaresRepository();
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            User applicationUser = new User();
            var loginResult = BoardSquaresRepository.AttemptLogin(model.Email, model.Password);
            switch (loginResult)
            {
                case -1:
                    ViewBag.ErrorMessage = "No Accounts found with specified Email";
                    return View(model);
                case 0: 
                    ViewBag.ErrorMessage = "Incorrect Password";
                    return View(model);
                default:
                    applicationUser = BoardSquaresRepository.GetUserByID(loginResult);
                    break;
            }
            
            var claims = new[] {
                new Claim(ClaimTypes.Email, applicationUser.Email),
                new Claim(ClaimTypes.NameIdentifier, applicationUser.Email),
                new Claim(ClaimTypes.Name, applicationUser.UserName )
                // can add more claims
            };
            
            var identity = new ClaimsIdentity(claims, "ApplicationCookie");
                var roleClaims =  new Claim(ClaimTypes.Role, applicationUser.AdminRole ? "Admin" : "User");
                identity.AddClaim(roleClaims);
                var context = Request.GetOwinContext();
                var authManager = context.Authentication;
                authManager.SignIn(new AuthenticationProperties { IsPersistent = true,  }, identity);
                return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = BoardSquaresRepository.AttemptRegister(model);
                switch (result)
                {
                    case -9:
                        ViewBag.ErrorMessage = "Error when creating user. Please try again later.";
                        return View(model);
                    case -8:
                        ViewBag.ErrorMessage = "An account with specified Email already exists.";
                        return View(model);
                    case -7:
                        ViewBag.ErrorMessage = "Desired User Name already exists. Please try a different one.";
                        return View(model);
                    case -6:
                        ViewBag.ErrorMessage = "Error validating user.";
                        return View(model);
                    case -5:
                        ViewBag.ErrorMessage = "Account Created, but Team Name already exists in game. Please Log in to create a team";
                        return View(model);
                    case -4:
                        ViewBag.ErrorMessage = "Game exists, but is currently full.";
                        return View(model);
                    case -3:
                        ViewBag.ErrorMessage = "Game is Closed.";
                        return View(model);
                    case -2:
                        ViewBag.ErrorMessage = "Game is Inactive.";
                        return View(model);
                    case -1:
                        ViewBag.ErrorMessage = "Game does not exist.";
                        return View(model);
                    default:
                        var user = BoardSquaresRepository.GetAllUsers().FirstOrDefault(u => u.UserName == model.UserName && u.Email == model.Email);
                        BoardSquaresRepository.CreateNewTeam(user.UserID, model.TeamName, model.GameNumber);
                        return RedirectToAction("Games", "Home");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = BoardSquaresRepository.Context.Users.FirstOrDefault(r => r.Email == model.Email);//await UserManager.FindByNameAsync(model.Email);
                if (user == null )//|| !(await UserManager.IsEmailConfirmedAsync(user.UserID.ToString())))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = Membership.GeneratePassword(25, 8);//await UserManager.GeneratePasswordResetTokenAsync(user.UserID.ToString());
                BoardSquaresRepository.AssignPasswordResetCode(user.UserID, code);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.UserID, code = code }, protocol: Request.Url.Scheme);
                var subject = "Password Reset";
                var message = "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a><br /><br />";
                 var mailClient = new SmtpClient
                 {
                     Host = "relay-hosting.secureserver.net"

                 };
                 var mailMessage = new MailMessage
                 {
                     From = new MailAddress("admin@boardsquares.com"),
                     Subject = subject,
                     Body = message,
                     IsBodyHtml = true
                 };
                 mailMessage.To.Add(new MailAddress(model.Email));
                 mailClient.Send(mailMessage);
                 mailClient.Dispose();
                 return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = BoardSquaresRepository.Context.Users.FirstOrDefault(r => r.Email == model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = BoardSquaresRepository.AttemptPasswordReset(user.UserID, model.Code, model.Password);
            
            if (result == 1)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        public ActionResult LogOff()
        {
            var ctx = Request.GetOwinContext();
            var authManager = ctx.Authentication;

            authManager.SignOut("ApplicationCookie");
            return RedirectToAction("Index", "Home");
            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        [Authorize(Roles = "User, Admin")]
        public ActionResult UpdateAccountInfo()
        {
            var viewmodel = new UpdateAccountInfoViewModel();
            var user = BoardSquaresRepository.Context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            viewmodel.WeeklyUpdates = user.WeeklyEmail;
            viewmodel.UserID = user.UserID;
            return View(viewmodel);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public ActionResult UpdateAccountInfo(UpdateAccountInfoViewModel viewmodel)
        {
            //var viewmodel = new RegisterViewModel();
            if (viewmodel.Password != null && viewmodel.ConfirmPassword != null)
            {
                BoardSquaresRepository.AttemptPasswordChange(viewmodel.UserID, viewmodel.Password);
            }
            BoardSquaresRepository.AttemptUpdateWeeklyEmails(viewmodel.UserID, viewmodel.WeeklyUpdates);
            //var test = true;
            return View(viewmodel);
        }
    }
}