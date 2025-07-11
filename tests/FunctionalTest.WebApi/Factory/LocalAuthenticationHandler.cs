// File: Factory/LocalAuthenticationHandler.cs
using Core.Domain.Identity.Constants;
using Core.Domain.Identity.Entities;
using Core.Domain.Identity.Permissions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FunctionalTest.WebApi.Factory
{
    public class LocalAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ApplicationUser _appUser = DefaultApplicationUsers.GetSuperUser();

        public LocalAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder) 
            : base(options, logger, encoder)
        {
        }

        private List<Claim> UserClaims()
        {
            return new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, _appUser.Id ?? string.Empty),
                new(ClaimTypes.Name, _appUser.UserName ?? string.Empty)
            };
        }

        private List<Claim> AllRolesClaims() => DefaultApplicationRoles.GetDefaultRoleClaims();

        private List<Claim> AllPermissionsClaims()
        {
            IPermissionHelper permissions = new Web.Framework.Permissions.PermissionHelper();
            return permissions.GetAllPermissions();
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim>();
            claims.AddRange(UserClaims());
            claims.AddRange(AllRolesClaims());
            claims.AddRange(AllPermissionsClaims());

            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme)),
                new AuthenticationProperties(),
                IdentityConstants.ApplicationScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}