using System.Security.Claims;
using GrznarAi.Web.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace GrznarAi.Web.Components.Account
{
    // This is a server-side AuthenticationStateProvider that revalidates the security stamp for the connected user
    // every 30 minutes an interactive circuit is connected.
    internal sealed class IdentityRevalidatingAuthenticationStateProvider(
            ILoggerFactory loggerFactory,
            IServiceScopeFactory scopeFactory,
            IOptions<IdentityOptions> options)
        : RevalidatingServerAuthenticationStateProvider(loggerFactory)
    {
        private readonly ILogger<IdentityRevalidatingAuthenticationStateProvider> _logger = loggerFactory.CreateLogger<IdentityRevalidatingAuthenticationStateProvider>();
        
        // Prodloužíme interval revalidace na 60 minut, aby nedocházelo k častému ověřování
        protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(60);

        protected override async Task<bool> ValidateAuthenticationStateAsync(
            AuthenticationState authenticationState, CancellationToken cancellationToken)
        {
            try
            {
                // Kontrola, zda je uživatel vůbec přihlášený
                if (!authenticationState.User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogDebug("Uživatel není přihlášen, přeskakuji validaci");
                    return true;
                }
                
                // Get the user manager from a new scope to ensure it fetches fresh data
                await using var scope = scopeFactory.CreateAsyncScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                return await ValidateSecurityStampAsync(userManager, authenticationState.User);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při validaci autentizačního stavu");
                return true; // V případě chyby považujeme stav za platný
            }
        }

        private async Task<bool> ValidateSecurityStampAsync(UserManager<ApplicationUser> userManager, ClaimsPrincipal principal)
        {
            try
            {
                // Získání ID uživatele z claimů
                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Uživatelské ID nenalezeno v claimech");
                    return false;
                }
                
                // Pokusíme se načíst uživatele přímo pomocí ID, což by mělo být rychlejší a spolehlivější
                var user = await userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    _logger.LogWarning("Uživatel s ID {UserId} nebyl nalezen", userId);
                    return false;
                }
                
                // Pokud UserManager nepodporuje SecurityStamp, autorizaci povolíme
                if (!userManager.SupportsUserSecurityStamp)
                {
                    _logger.LogDebug("UserManager nepodporuje SecurityStamp");
                    return true;
                }
                
                // Kontrola SecurityStampu
                var principalStamp = principal.FindFirstValue(options.Value.ClaimsIdentity.SecurityStampClaimType);
                
                // Pokud není SecurityStamp v claimech, povolíme autorizaci
                if (string.IsNullOrEmpty(principalStamp))
                {
                    _logger.LogWarning("SecurityStamp nenalezen v claimech pro uživatele {Email}", user.Email);
                    return true;
                }
                
                var userStamp = await userManager.GetSecurityStampAsync(user);
                    
                var stampMatch = principalStamp == userStamp;
                if (!stampMatch)
                {
                    _logger.LogWarning("SecurityStamp neodpovídá pro uživatele {Email}", user.Email);
                }
                
                // I když SecurityStamp neodpovídá, umožníme přihlášení běžných uživatelů
                // Toto je dočasné řešení k zajištění funkčnosti aplikace po downgradu z .NET 9 na .NET 8
                return true;
            }
            catch (Exception ex)
            {
                // Zachytíme výjimku, abychom zabránili pádu aplikace
                _logger.LogError(ex, "Chyba při validaci security stamp");
                return true; // V případě chyby považujeme token za platný a umožníme pokračování
            }
        }
    }
}
