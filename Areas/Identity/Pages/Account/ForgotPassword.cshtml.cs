using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace CinemaRocha.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "O email é obrigatório")]
            [EmailAddress(ErrorMessage = "Email inválido")]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                
                // Always redirect to confirmation page for security (don't reveal if user exists)
                if (user == null)
                {
                    _logger.LogWarning("Password reset attempted for non-existent email: {Email}", Input.Email);
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Generate password reset token
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code, email = Input.Email },
                    protocol: Request.Scheme);

                try
                {
                    _logger.LogInformation("Attempting to send password reset email to {Email}", Input.Email);
                    
                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Recuperar Password - RochaCinema",
                        $@"
<!DOCTYPE html>
<html lang='pt'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Recuperar Password</title>
</head>
<body style='margin: 0; padding: 0; font-family: ""Outfit"", -apple-system, BlinkMacSystemFont, ""Segoe UI"", sans-serif; background-color: #0B0B0B;'>
    <table role='presentation' style='width: 100%; border-collapse: collapse; background-color: #0B0B0B;'>
        <tr>
            <td style='padding: 48px 20px;'>
                <table role='presentation' style='max-width: 540px; margin: 0 auto; background: linear-gradient(180deg, #1A1A1A 0%, #161616 100%); border-radius: 12px; overflow: hidden; box-shadow: 0 4px 24px rgba(0, 0, 0, 0.5); border: 1px solid rgba(255, 255, 255, 0.08);'>
                    <!-- Header -->
                    <tr>
                        <td style='padding: 36px 36px 28px; text-align: center; border-bottom: 1px solid rgba(255, 255, 255, 0.06);'>
                            <h1 style='margin: 0; font-size: 24px; font-weight: 700; color: #FFFFFF; letter-spacing: -0.4px;'>
                                ROCHA<span style='color: #FF3B30;'>CINEMA</span>
                            </h1>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style='padding: 36px 36px 32px;'>
                            <h2 style='margin: 0 0 18px; font-size: 19px; font-weight: 600; color: #FFFFFF; text-align: left; letter-spacing: -0.2px;'>
                                Recuperar Password
                            </h2>
                            <p style='margin: 0 0 22px; color: #B3B3B3; font-size: 14.5px; line-height: 1.5; text-align: left;'>
                                Recebemos um pedido para redefinir a password da sua conta RochaCinema. Clique no botão abaixo para criar uma nova password.
                            </p>
                            
                            <!-- CTA Button -->
                            <table role='presentation' style='width: 100%; border-collapse: collapse; margin: 28px 0;'>
                                <tr>
                                    <td style='text-align: left;'>
                                        <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                                           style='display: inline-block; background: linear-gradient(135deg, #FF3B30, #FF5547); color: #FFFFFF; padding: 13px 36px; border-radius: 8px; text-decoration: none; font-weight: 600; font-size: 14px; letter-spacing: 0.2px; box-shadow: 0 4px 16px rgba(255, 59, 48, 0.25);'>
                                            Redefinir Password
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            
                            <div style='margin: 28px 0 0; padding: 16px; background: rgba(255, 255, 255, 0.02); border-left: 3px solid rgba(255, 255, 255, 0.1); border-radius: 4px;'>
                                <p style='margin: 0 0 8px; color: #8E8E93; font-size: 13px; line-height: 1.4; text-align: left;'>
                                    Se não solicitou esta alteração, pode ignorar este email em segurança. A sua password permanecerá inalterada.
                                </p>
                                <p style='margin: 0; color: #8E8E93; font-size: 13px; line-height: 1.4; text-align: left;'>
                                    Este link expira em 24 horas.
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='padding: 24px 36px; background: rgba(0, 0, 0, 0.2); border-top: 1px solid rgba(255, 255, 255, 0.06);'>
                            <p style='margin: 0; color: #6E6E73; font-size: 12px; text-align: center; line-height: 1.3;'>
                                © 2024 RochaCinema. Todos os direitos reservados.
                            </p>
                        </td>
                    </tr>
                </table>
                
                <!-- Alternative Link -->
                <table role='presentation' style='max-width: 540px; margin: 20px auto 0;'>
                    <tr>
                        <td style='padding: 0 20px;'>
                            <p style='margin: 0; color: #6E6E73; font-size: 11px; line-height: 1.4; text-align: center;'>
                                Se o botão não funcionar, copie este link:<br>
                                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='color: #FF3B30; text-decoration: none; word-break: break-all;'>{HtmlEncoder.Default.Encode(callbackUrl)}</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>");
                    
                    _logger.LogInformation("Password reset email successfully sent to {Email}", Input.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending password reset email to {Email}", Input.Email);
                    // Still redirect to confirmation page for security
                }

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
