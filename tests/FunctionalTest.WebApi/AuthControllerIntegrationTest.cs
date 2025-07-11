// File: AuthControllerIntegrationTest.cs
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Core.Application.Contracts.DataTransferObjects;
using FunctionalTest.WebApi.Factory;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace FunctionalTest.WebApi
{
    public class AuthControllerIntegrationTest : IClassFixture<AppFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerIntegrationTest(AppFactory factory)
        {
            _client = factory.CreateClient();
        }

        // [Fact]
        // public async Task Login_WithValidCredentials_ReturnsJwtToken()
        // {
        //     // Arrange
        //     var loginDto = new LoginUserDtoForApi
        //     {
        //         UserName = "superadmin",
        //         Password = "1234"
        //     };

        //     var json = JsonConvert.SerializeObject(loginDto);
        //     var content = new StringContent(json, Encoding.UTF8, "application/json");

        //     // Act
        //     var response = await _client.PostAsync("/api/account/login", content);

        //     // Assert
        //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //     var token = await response.Content.ReadAsStringAsync();
        //     Assert.False(string.IsNullOrWhiteSpace(token));
        // }
    }
}
