// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace CinemaRocha.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirme o seu Email - RochaCinema",
                        $@"
<!DOCTYPE html>
<html lang='pt'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Confirme o seu Email - RochaCinema</title>
</head>
<body style='margin: 0; padding: 0; font-family: ""Outfit"", -apple-system, BlinkMacSystemFont, ""Segoe UI"", sans-serif; background-color: #0B0B0B;'>
    <table role='presentation' style='width: 100%; border-collapse: collapse;'>
        <tr>
            <td style='padding: 40px 20px;'>
                <table role='presentation' style='max-width: 600px; margin: 0 auto; background: linear-gradient(145deg, rgba(22, 22, 22, 0.95), rgba(11, 11, 11, 0.98)); border-radius: 24px; overflow: hidden; box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3); border: 1px solid rgba(255, 255, 255, 0.1);'>
                    <!-- Header -->
                    <tr>
                        <td style='padding: 40px 40px 30px; text-align: center; background: linear-gradient(135deg, rgba(255, 59, 48, 0.1), transparent);'>
                            <h1 style='margin: 0; font-size: 32px; font-weight: 900; color: #FFFFFF; letter-spacing: -0.5px;'>
                                ROCHA<span style='background: linear-gradient(135deg, #FF3B30, #FF6B5E); -webkit-background-clip: text; -webkit-text-fill-color: transparent;'>CINEMA</span>
                            </h1>
                        </td>
                    </tr>
                    
                    <!-- Icon -->
                    <tr>
                        <td style='padding: 0 40px 30px; text-align: center;'>
                            <div style='width: 80px; height: 80px; margin: 0 auto; background: linear-gradient(135deg, #FF3B30, #FF6B5E); border-radius: 50%; display: flex; align-items: center; justify-content: center; box-shadow: 0 10px 40px rgba(255, 59, 48, 0.3);'>
                                <span style='font-size: 40px; color: white;'>✉️</span>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style='padding: 0 40px 40px;'>
                            <h2 style='margin: 0 0 20px; font-size: 28px; font-weight: 800; color: #FFFFFF; text-align: center;'>
                                Bem-vindo ao RochaCinema!
                            </h2>
                            <p style='margin: 0 0 16px; color: #9CA3AF; font-size: 16px; line-height: 1.6; text-align: center;'>
                                Obrigado por se registar! Estamos entusiasmados por tê-lo connosco.
                            </p>
                            <p style='margin: 0 0 32px; color: #9CA3AF; font-size: 16px; line-height: 1.6; text-align: center;'>
                                Para começar a explorar os nossos filmes e fazer reservas, por favor confirme o seu endereço de email clicando no botão abaixo:
                            </p>
                            
                            <!-- CTA Button -->
                            <table role='presentation' style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='text-align: center; padding: 0 0 32px;'>
                                        <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                                           style='display: inline-block; background: linear-gradient(135deg, #FF3B30, #FF6B5E); color: white; padding: 16px 48px; border-radius: 16px; text-decoration: none; font-weight: 600; font-size: 16px; box-shadow: 0 4px 20px rgba(255, 59, 48, 0.3); transition: all 0.3s ease;'>
                                            ✓ Confirmar Email
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style='margin: 0 0 16px; color: #6B7280; font-size: 14px; line-height: 1.6; text-align: center;'>
                                Se não criou esta conta, pode ignorar este email em segurança.
                            </p>
                            <p style='margin: 0; color: #6B7280; font-size: 14px; line-height: 1.6; text-align: center;'>
                                Este link é válido por 24 horas.
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='padding: 32px 40px; background: rgba(11, 11, 11, 0.5); border-top: 1px solid rgba(255, 255, 255, 0.05);'>
                            <p style='margin: 0 0 8px; color: #6B7280; font-size: 13px; text-align: center;'>
                                A melhor experiência de cinema. Salas premium, som imersivo e os últimos lançamentos.
                            </p>
                            <p style='margin: 0; color: #4B5563; font-size: 12px; text-align: center;'>
                                © 2024 RochaCinema. Todos os direitos reservados.
                            </p>
                        </td>
                    </tr>
                </table>
                
                <!-- Alternative Link -->
                <table role='presentation' style='max-width: 600px; margin: 20px auto 0;'>
                    <tr>
                        <td style='padding: 0 20px;'>
                            <p style='margin: 0; color: #4B5563; font-size: 12px; line-height: 1.6; word-break: break-all;'>
                                Se o botão não funcionar, copie e cole este link no seu navegador:<br>
                                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='color: #FF3B30; text-decoration: none;'>{HtmlEncoder.Default.Encode(callbackUrl)}</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
