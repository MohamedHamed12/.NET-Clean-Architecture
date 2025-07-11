using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Newtonsoft.Json;

namespace FunctionalTest.WebApi.Controllers
{
    public class PostControllerIntegrationTest : IClassFixture<Factory.AppFactory>
    {
        private readonly HttpClient _client;

        public PostControllerIntegrationTest(Factory.AppFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_Should_Return_Ok()
        {
            var response = await _client.GetAsync("/api/v1/Post");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AddPost_Should_Return_Created()
        {
            var payload = new
            {
                Title = "Test Post",
                Content = "Integration test content",
                Tags = new[] { "dotnet", "test" }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/v1/Post", content);

            // Read and log the response body for debugging
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response body:");
            Console.WriteLine(responseBody);
            Console.WriteLine("Response body:");

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

    }
}
