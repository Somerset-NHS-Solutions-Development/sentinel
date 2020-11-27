using Sentinel.Identity;
using Authentication.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
//using Sentinel.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Sentinel.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly LdapUserManager _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        //private ApplicationOptions _options;

        public LoginModel(SignInManager<User> signInManager,
            ILogger<LoginModel> logger,
            LdapUserManager userManager/*,
            IOptionsMonitor<ApplicationOptions> options*/)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            //_options = options.CurrentValue;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "User name")]
            public string UserName { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            // Don't allow logins on public facing instances of the application
            //if (_options.PublicFacing) return new RedirectToPageResult("/Error");

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                _logger.LogInformation($"Login attempt from {Input.UserName}");
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User logged in: {Input.UserName}");
                    return LocalRedirect(returnUrl);
                }
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                //}
                // Check if user is locked out
                using PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, "YDH");
                var userPrincipal = UserPrincipal.FindByIdentity(
                   principalContext,
                   IdentityType.SamAccountName,
                   Input.UserName);
                int invalidAttempts = userPrincipal.BadLogonCount;
                bool lockedOut = userPrincipal.IsAccountLockedOut();

                if (lockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    _logger.LogInformation($"Login failure from {Input.UserName}");
                    string message = (invalidAttempts <= 1) ? "Login failed. Are your username and password correct?"
                                                            : "Login failed. You have one more attempt left.";
                    ModelState.AddModelError(string.Empty, message);
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
