// File: Factory/MockSchemeProvider.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace FunctionalTest.WebApi.Factory
{
    public class MockSchemeProvider : AuthenticationSchemeProvider
    {
        public MockSchemeProvider(IOptions<AuthenticationOptions> options)
            : base(options) { }

        public override Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync()
        {
            return Task.FromResult(new AuthenticationScheme("Test", "Test", typeof(LocalAuthenticationHandler)));
        }

        public override Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync()
        {
            return Task.FromResult(new AuthenticationScheme("Test", "Test", typeof(LocalAuthenticationHandler)));
        }

        public override Task<AuthenticationScheme> GetSchemeAsync(string name)
        {
            return Task.FromResult(new AuthenticationScheme(name, name, typeof(LocalAuthenticationHandler)));
        }

        public override Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
        {
            var schemes = new[]
            {
                new AuthenticationScheme("Test", "Test", typeof(LocalAuthenticationHandler))
            };
            return Task.FromResult<IEnumerable<AuthenticationScheme>>(schemes);
        }
    }
}
