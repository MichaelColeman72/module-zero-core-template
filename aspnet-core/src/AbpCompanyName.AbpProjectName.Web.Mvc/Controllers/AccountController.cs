using Abp;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.MultiTenancy;
using Abp.Notifications;
using Abp.Threading;
using Abp.Timing;
using Abp.UI;
using Abp.Web.Models;
using Abp.Zero.Configuration;
using AbpCompanyName.AbpProjectName.Authorization;
using AbpCompanyName.AbpProjectName.Authorization.Users;
using AbpCompanyName.AbpProjectName.Controllers;
using AbpCompanyName.AbpProjectName.Identity;
using AbpCompanyName.AbpProjectName.MultiTenancy;
using AbpCompanyName.AbpProjectName.Sessions;
using AbpCompanyName.AbpProjectName.Web.Models.Account;
using AbpCompanyName.AbpProjectName.Web.Views.Shared.Components.TenantChange;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AbpCompanyName.AbpProjectName.Web.Controllers
{
    public class AccountController : AbpProjectNameControllerBase
    {
        private readonly UserManager _userManager;
        private readonly TenantManager _tenantManager;
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly LogInManager _logInManager;
        private readonly SignInManager _signInManager;
        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly ISessionAppService _sessionAppService;
        private readonly ITenantCache _tenantCache;
        private readonly INotificationPublisher _notificationPublisher;

        public AccountController(
            UserManager userManager,
            IMultiTenancyConfig multiTenancyConfig,
            TenantManager tenantManager,
            IUnitOfWorkManager unitOfWorkManager,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            LogInManager logInManager,
            SignInManager signInManager,
            UserRegistrationManager userRegistrationManager,
            ISessionAppService sessionAppService,
            ITenantCache tenantCache,
            INotificationPublisher notificationPublisher)
        {
            _userManager = userManager;
            _multiTenancyConfig = multiTenancyConfig;
            _tenantManager = tenantManager;
            _unitOfWorkManager = unitOfWorkManager;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _logInManager = logInManager;
            _signInManager = signInManager;
            _userRegistrationManager = userRegistrationManager;
            _sessionAppService = sessionAppService;
            _tenantCache = tenantCache;
            _notificationPublisher = notificationPublisher;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "By design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "By design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5395:Miss HttpVerb attribute for action methods", Justification = "By design")]
        public ActionResult Login(string userNameOrEmailAddress = "", string returnUrl = "", string successMessage = "")
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = GetAppHomeUrl();
            }

            return View(new LoginFormViewModel
            {
                ReturnUrl = returnUrl,
                IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled,
                IsSelfRegistrationAllowed = IsSelfRegistrationEnabled(),
                MultiTenancySide = AbpSession.MultiTenancySide
            });
        }

        [HttpPost]
        [UnitOfWork]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "By design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5391:Use antiforgery tokens in ASP.NET Core MVC controllers", Justification = "By design")]
        public virtual async Task<JsonResult> Login(LoginViewModel loginModel, string returnUrl = "", string returnUrlHash = "")
        {
            returnUrl = NormalizeReturnUrl(returnUrl);
            if (!string.IsNullOrWhiteSpace(returnUrlHash))
            {
                returnUrl += returnUrlHash;
            }

            var loginResult = await GetLoginResultAsync(loginModel.UsernameOrEmailAddress, loginModel.Password, GetTenancyNameOrNull()).ConfigureAwait(false);

            await _signInManager.SignInAsync(loginResult.Identity, loginModel.RememberMe).ConfigureAwait(false);
            await UnitOfWorkManager.Current.SaveChangesAsync().ConfigureAwait(false);

            return Json(new AjaxResponse { TargetUrl = returnUrl });
        }

        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync().ConfigureAwait(false);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Register()
        {
            return RegisterView(new RegisterViewModel());
        }

        [HttpPost]
        [UnitOfWork]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "By design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1506: Avoid excessive class coupling", Justification = "By design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5391:Use antiforgery tokens in ASP.NET Core MVC controllers", Justification = "By design")]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            {
                ExternalLoginInfo externalLoginInfo = null;
                if (model.IsExternalLogin)
                {
                    externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync().ConfigureAwait(false);
                    if (externalLoginInfo == null)
                    {
                        throw new Exception("Can not external login!");
                    }

                    model.UserName = model.EmailAddress;
                    model.Password = Authorization.Users.User.CreateRandomPassword();
                }
                else
                {
                    if (model.UserName.IsNullOrEmpty() || model.Password.IsNullOrEmpty())
                    {
                        throw new UserFriendlyException(L("FormIsNotValidMessage"));
                    }
                }

                var user = await _userRegistrationManager.RegisterAsync(
                    model.Name,
                    model.Surname,
                    model.EmailAddress,
                    model.UserName,
                    model.Password,
                    true).ConfigureAwait(false); // Assumed email address is always confirmed. Change this if you want to implement email confirmation.

                // Getting tenant-specific settings
                var isEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin).ConfigureAwait(false);

                if (model.IsExternalLogin)
                {
                    Debug.Assert(externalLoginInfo != null, "externalLoginInfo != null");

                    if (string.Equals(externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email), model.EmailAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        user.IsEmailConfirmed = true;
                    }

                    user.Logins = new List<UserLogin>
                    {
                        new UserLogin
                        {
                            LoginProvider = externalLoginInfo.LoginProvider,
                            ProviderKey = externalLoginInfo.ProviderKey,
                            TenantId = user.TenantId
                        }
                    };
                }

                await _unitOfWorkManager.Current.SaveChangesAsync().ConfigureAwait(false);

                Debug.Assert(user.TenantId != null, "user.TenantId != null");

                var tenant = await _tenantManager.GetByIdAsync(user.TenantId.Value).ConfigureAwait(false);

                // Directly login if possible
                if (user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin))
                {
                    var loginResult = externalLoginInfo != null
                        ? await _logInManager.LoginAsync(externalLoginInfo, tenant.TenancyName).ConfigureAwait(false)
                        : await GetLoginResultAsync(user.UserName, model.Password, tenant.TenancyName).ConfigureAwait(false);
                    if (loginResult.Result == AbpLoginResultType.Success)
                    {
                        await _signInManager.SignInAsync(loginResult.Identity, false).ConfigureAwait(false);
                        return Redirect(GetAppHomeUrl());
                    }

                    Logger.Warn("New registered user could not be login. This should not be normally. login result: " + loginResult.Result);
                }

                return View("RegisterResult", new RegisterResultViewModel
                {
                    TenancyName = tenant.TenancyName,
                    NameAndSurname = user.Name + " " + user.Surname,
                    UserName = user.UserName,
                    EmailAddress = user.EmailAddress,
                    IsEmailConfirmed = user.IsEmailConfirmed,
                    IsActive = user.IsActive,
                    IsEmailConfirmationRequiredForLogin = isEmailConfirmationRequiredForLogin
                });
            }
            catch (UserFriendlyException ex)
            {
                ViewBag.ErrorMessage = ex.Message;

                return View("Register", model);
            }
        }

        /// <summary>
        /// External login hook.
        /// </summary>
        /// <param name="provider">Login provider.</param>
        /// <param name="returnUrl">Post login callback.</param>
        /// <remarks>
        /// <code>
        /// _ = Url.Action(
        ///        "ExternalLoginCallback",
        ///        "Account",
        ///        new
        ///        {
        ///            ReturnUrl = returnUrl
        /// });
        ///
        /// return Challenge(
        ///
        ///     // TODO: ...?
        ///     // new Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties
        ///   // {
        ///   //     Items = { { "LoginProvider", provider } },
        ///   //     RedirectUri = redirectUrl
        ///   // },
        ///   provider);
        /// </code>
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "By design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1615:Element return value should be documented", Justification = "By design")]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            _ = Url.Action(
                "ExternalLoginCallback",
                "Account",
                new
                {
                    ReturnUrl = returnUrl
                });

            return Challenge(provider);
        }

        [HttpGet]
        [UnitOfWork]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "By design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "By design")]
        public virtual async Task<ActionResult> ExternalLoginCallback(string returnUrl, string remoteError = null)
        {
            returnUrl = NormalizeReturnUrl(returnUrl);

            if (remoteError != null)
            {
                Logger.Error("Remote Error in ExternalLoginCallback: " + remoteError);
                throw new UserFriendlyException(L("CouldNotCompleteLoginOperation"));
            }

            var externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync().ConfigureAwait(false);
            if (externalLoginInfo == null)
            {
                Logger.Warn("Could not get information from external login.");
                return RedirectToAction(nameof(Login));
            }

            await _signInManager.SignOutAsync().ConfigureAwait(false);

            var tenancyName = GetTenancyNameOrNull();

            var loginResult = await _logInManager.LoginAsync(externalLoginInfo, tenancyName).ConfigureAwait(false);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    await _signInManager.SignInAsync(loginResult.Identity, false).ConfigureAwait(false);
                    return Redirect(returnUrl);
                case AbpLoginResultType.UnknownExternalLogin:
                    return await RegisterForExternalLogin(externalLoginInfo).ConfigureAwait(false);
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                        loginResult.Result,
                        externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email) ?? externalLoginInfo.ProviderKey,
                        tenancyName);
            }
        }

        [HttpGet]
        public ActionResult RedirectToAppHome() => RedirectToAction("Index", "Home");

        [HttpGet]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1055:URI-like return values should not be strings", Justification = "By design")]
        public string GetAppHomeUrl() => Url.Action("Index", "About");

        [HttpGet]
        public async Task<ActionResult> TenantChangeModal()
        {
            var loginInfo = await _sessionAppService.GetCurrentLoginInformations().ConfigureAwait(false);
            return View("/Views/Shared/Components/TenantChange/_ChangeModal.cshtml", new ChangeModalViewModel
            {
                TenancyName = loginInfo.Tenant?.TenancyName
            });
        }

        /// <summary>
        /// This is a demo code to demonstrate sending notification to default tenant admin and host admin uers.
        /// Don't use this code in production !!!.
        /// </summary>
        /// <param name="message">A message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet]
        [AbpMvcAuthorize]
        public async Task<ActionResult> TestNotification(string message = "")
        {
            if (message.IsNullOrEmpty())
            {
                message = "This is a test notification, created at " + Clock.Now;
            }

            var defaultTenantAdmin = new UserIdentifier(1, 2);
            var hostAdmin = new UserIdentifier(null, 1);

            await _notificationPublisher.PublishAsync(
                    "App.SimpleMessage",
                    new MessageNotificationData(message),
                    severity: NotificationSeverity.Info,
                    userIds: new[] { defaultTenantAdmin, hostAdmin }).ConfigureAwait(false);

            return Content("Sent notification: " + message);
        }

        [UnitOfWork]
        protected virtual async Task<List<Tenant>> FindPossibleTenantsOfUserAsync(UserLoginInfo login)
        {
            List<User> allUsers;
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                allUsers = await _userManager.FindAllAsync(login).ConfigureAwait(false);
            }

            return allUsers
                .Where(u => u.TenantId != null)
                .Select(u => AsyncHelper.RunSync(() => _tenantManager.FindByIdAsync(u.TenantId.Value)))
                .ToList();
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName).ConfigureAwait(false);

            return loginResult.Result switch
            {
                AbpLoginResultType.Success => loginResult,
                _ => throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName),
            };
        }

        private ActionResult RegisterView(RegisterViewModel model)
        {
            ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;

            return View("Register", model);
        }

        private bool IsSelfRegistrationEnabled()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return false; // No registration enabled for host users!
            }

            return true;
        }

        private async Task<ActionResult> RegisterForExternalLogin(ExternalLoginInfo externalLoginInfo)
        {
            var email = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email);
            var (name, surname) = ExternalLoginInfoHelper.GetNameAndSurnameFromClaims(externalLoginInfo.Principal.Claims.ToList());

            var viewModel = new RegisterViewModel
            {
                EmailAddress = email,
                Name = name,
                Surname = surname,
                IsExternalLogin = true,
                ExternalLoginAuthSchema = externalLoginInfo.LoginProvider
            };

            return name != null &&
                surname != null &&
                email != null
                ? await Register(viewModel).ConfigureAwait(false)
                : RegisterView(viewModel);
        }

        private string GetTenancyNameOrNull()
        {
            return !AbpSession.TenantId.HasValue ? null : _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        private string NormalizeReturnUrl(string returnUrl, Func<string> defaultValueBuilder = null)
        {
            defaultValueBuilder ??= GetAppHomeUrl;

            return returnUrl.IsNullOrEmpty() ? defaultValueBuilder() : Url.IsLocalUrl(returnUrl) ? returnUrl : defaultValueBuilder();
        }
    }
}
