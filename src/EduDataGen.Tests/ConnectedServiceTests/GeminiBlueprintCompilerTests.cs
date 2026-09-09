using System.Net;
using System.Text;
using System.Text.Json;
using EduDataGen.ConnectedService.Gemini;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace EduDataGen.Tests.ConnectedServiceTests;

public class GeminiBlueprintCompilerTests
{
    [Fact]
    public async Task CompileBlueprintAsync_MissingApiKey_ThrowsInvalidOperationException()
    {
        var inMemoryConfig = new Dictionary<string, string?>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();
        var httpClient = new HttpClient();
        var compiler = new GeminiBlueprintCompiler(httpClient, config);

        var act = async () => await compiler.CompileBlueprintAsync("test prompt");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Gemini API Key is missing*");
    }

    [Fact]
    public async Task CompileBlueprintAsync_ValidResponse_DeserializesAndAppliesOverrides()
    {
        var mockResponseBody = new
        {
            candidates = new[]
            {
                new
                {
                    content = new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = JsonSerializer.Serialize(new
                                {
                                    Scenario = "ai_triage",
                                    Description = "AI Generated Triage",
                                    Seed = 100,
                                    TotalRecords = 50,
                                    CohortSettings = new { HealthyRatio = 0.5 },
                                    BaselineMetrics = new { },
                                    Conditions = new { },
                                    Anomalies = new { MissingValueRate = 0.02 }
                                })
                            }
                        }
                    }
                }
            }
        };

        var handler = new MockHttpMessageHandler(JsonSerializer.Serialize(mockResponseBody), HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);

        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "GEMINI_API_KEY", "test_mock_key" }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        var compiler = new GeminiBlueprintCompiler(httpClient, config);

        var blueprint = await compiler.CompileBlueprintAsync(
            prompt: "Test Triage Scenario",
            seed: 9999,
            totalRecords: 200,
            healthyRatio: 0.3,
            missingValueRate: 0.1);

        blueprint.Should().NotBeNull();
        blueprint.Scenario.Should().Be("ai_triage");
        blueprint.Seed.Should().Be(9999);
        blueprint.TotalRecords.Should().Be(200);
        blueprint.CohortSettings.HealthyRatio.Should().Be(0.3);
        blueprint.Anomalies.MissingValueRate.Should().Be(0.1);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;
        private readonly HttpStatusCode _statusCode;

        public MockHttpMessageHandler(string responseContent, HttpStatusCode statusCode)
        {
            _responseContent = responseContent;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
