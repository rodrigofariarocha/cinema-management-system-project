using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace CinemaRocha.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ConfirmEmailModel> _logger;

        public ConfirmEmailModel(
            UserManager<IdentityUser> userManager,
            ILogger<ConfirmEmailModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public string StatusMessage { get; set; }
        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                StatusMessage = "Link de confirmação inválido.";
                IsSuccess = false;
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Unable to find user with ID '{UserId}'.", userId);
                StatusMessage = "Utilizador não encontrado.";
                IsSuccess = false;
                return Page();
            }

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                var result = await _userManager.ConfirmEmailAsync(user, code);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} confirmed their email successfully.", userId);
                    StatusMessage = "Obrigado por confirmar o seu email.";
                    IsSuccess = true;
                }
                else
                {
                    _logger.LogWarning("Error confirming email for user {UserId}: {Errors}", 
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    StatusMessage = "Erro ao confirmar o seu email. Por favor, tente novamente.";
                    IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while confirming email for user {UserId}", userId);
                StatusMessage = "Ocorreu um erro inesperado. Por favor, tente novamente.";
                IsSuccess = false;
            }

            return Page();
        }
    }
}
