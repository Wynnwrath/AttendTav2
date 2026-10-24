using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AccountController : ControllerBase<AccountController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public AccountController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
        }

        /// <summary>
        /// Login Method
        /// </summary>
        /// <returns>Created response view</returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            TempData["returnUrl"] = System.Net.WebUtility.UrlDecode(HttpContext.Request.Query["ReturnUrl"]);
            this._sessionManager.Clear();
            this._session.SetString("SessionId", System.Guid.NewGuid().ToString());
            return this.View();
        }

        /// <summary>
        /// Authenticate user and signs the user in when successful.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns> Created response view </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            this._session.SetString("HasSession", "Exist");

            if (_userService.IsPendingApproval(model.UserId))
            {
                TempData["ErrorMessage"] = "Your account registration is currently pending officer review and approval.";
                return View(model);
            }

            User user = null;
            var loginResult = _userService.AuthenticateUser(model.UserId, model.Password, ref user);
            if (loginResult == LoginResult.Success)
            {
                await this._signInManager.SignInAsync(user);
                this._session.SetString("UserName", user.Name);
                this._session.SetString("UserId", user.UserId);

                if (user.UserId.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (user.UserId.StartsWith("20", StringComparison.OrdinalIgnoreCase) || user.UserId.Contains("-"))
                {
                    return RedirectToAction("Index", "Student");
                }
                else
                {
                    return RedirectToAction("Index", "Member");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Incorrect UserId or Password";
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register(UserViewModel model)
        {
            try
            {
                _userService.RegisterApplicant(model);
                TempData["SuccessMessage"] = "Membership request submitted successfully! An officer will review your application.";
                return RedirectToAction("Login", "Account");
            }
            catch(InvalidDataException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch(Exception)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
            }
            return View(model);
        }

        /// <summary>
        /// Officer Workstation: Membership Approvals
        /// </summary>
        [HttpGet]
        public IActionResult Approvals()
        {
            var pending = _userService.GetPendingApplicants();
            return View(pending);
        }

        [HttpPost]
        public IActionResult Approve(string studentNumber)
        {
            if (!string.IsNullOrWhiteSpace(studentNumber))
            {
                _userService.ApproveApplicant(studentNumber);
                TempData["SuccessMessage"] = $"Applicant {studentNumber} has been approved and activated in the Member directory!";
            }
            return RedirectToAction("Approvals");
        }

        [HttpPost]
        public IActionResult Reject(string studentNumber)
        {
            if (!string.IsNullOrWhiteSpace(studentNumber))
            {
                _userService.RejectApplicant(studentNumber);
                TempData["SuccessMessage"] = $"Application for {studentNumber} has been rejected.";
            }
            return RedirectToAction("Approvals");
        }

        /// <summary>
        /// Sign Out current account and return login view.
        /// </summary>
        /// <returns>Created response view</returns>
        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
