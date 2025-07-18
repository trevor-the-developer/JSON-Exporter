using System.Text.Json;
using FluentAssertions;
using JsonExtractor.Services;
using JsonExtractor.Tests.TestData;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JsonExtractor.Tests.Integration;

public class FacebookMessagesIntegrationTests
{
    private readonly JsonExtractorService _extractorService;
    private readonly JsonQueryService _queryService;

    public FacebookMessagesIntegrationTests()
    {
        var extractorLogger = new Mock<ILogger<JsonExtractorService>>();
        var queryLogger = new Mock<ILogger<JsonQueryService>>();

        _extractorService = new JsonExtractorService(extractorLogger.Object);
        _queryService = new JsonQueryService(queryLogger.Object);
    }

    [Fact]
    public async Task ExtractBusinessInquiries_FromFacebookMessages_ShouldFindServiceRequests()
    {
        // Arrange
        var parseResult = await _extractorService.ParseJsonAsync(SampleData.FacebookMessagesJson);
        parseResult.Success.Should().BeTrue();

        using var document = JsonDocument.Parse(parseResult.Data!.ToString()!);
        var rootElement = document.RootElement;

        // Act - Find messages containing keywords related to service requests
        var serviceKeywords = new[] { "carpet cleaning", "quote", "price", "schedule" };
        var results = new List<JsonElement>();

        foreach (var keyword in serviceKeywords)
        {
            var messagesResult = _queryService.ExecuteJsonPath(rootElement, "$.messages[*]");

            if (!messagesResult.Success) continue;
            var filteredMessages = messagesResult.Results.Where(msg =>
                msg.TryGetProperty("content", out var content) &&
                content.GetString()?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true);

            results.AddRange(filteredMessages);
        }

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCountGreaterThan(2); // Should find multiple relevant messages

        // Verify we found the initial inquiry
        var inquiryMessage = results.FirstOrDefault(r =>
            r.TryGetProperty("content", out var content) &&
            content.GetString()?.Contains("carpet cleaning") == true);

        inquiryMessage.ValueKind.Should().NotBe(JsonValueKind.Undefined);
    }

    [Fact]
    public async Task ExtractPricingInformation_FromFacebookMessages_ShouldFindQuotes()
    {
        // Arrange
        var parseResult = await _extractorService.ParseJsonAsync(SampleData.FacebookMessagesJson);
        using var document = JsonDocument.Parse(parseResult.Data!.ToString()!);
        var rootElement = document.RootElement;

        // Act - Find messages with pricing information
        var messagesResult = _queryService.ExecuteJsonPath(rootElement, "$.messages[*]");
        var pricingMessages = messagesResult.Results.Where(msg =>
        {
            if (!msg.TryGetProperty("content", out var content))
                return false;

            var text = content.GetString() ?? "";
            return text.Contains('$') || text.Contains("charge") || text.Contains("total");
        }).ToList();

        // Assert
        pricingMessages.Should().NotBeEmpty();
        pricingMessages.Should().HaveCount(2); // Two pricing-related messages in sample data

        // Verify we found the quote
        var quoteMessage = pricingMessages.FirstOrDefault(msg =>
            msg.GetProperty("content").GetString()?.Contains("$2 per square foot") == true);

        quoteMessage.ValueKind.Should().NotBe(JsonValueKind.Undefined);
    }

    [Fact]
    public async Task ExtractConversationTimeline_FromFacebookMessages_ShouldOrderByTimestamp()
    {
        // Arrange
        var parseResult = await _extractorService.ParseJsonAsync(SampleData.FacebookMessagesJson);
        using var document = JsonDocument.Parse(parseResult.Data!.ToString()!);
        var rootElement = document.RootElement;

        // Act - Get all messages ordered by timestamp
        var messagesResult = _queryService.ExecuteJsonPath(rootElement, "$.messages[*]");
        var orderedMessages = messagesResult.Results
            .OrderBy(msg => msg.GetProperty("timestamp_ms").GetInt64())
            .ToList();

        // Assert
        orderedMessages.Should().HaveCount(4);

        // Verify chronological order
        for (var i = 1; i < orderedMessages.Count; i++)
        {
            var currentTimestamp = orderedMessages[i].GetProperty("timestamp_ms").GetInt64();
            var previousTimestamp = orderedMessages[i - 1].GetProperty("timestamp_ms").GetInt64();
            currentTimestamp.Should().BeGreaterThan(previousTimestamp);
        }
    }

    [Fact]
    public async Task ExtractContactInformation_FromFacebookMessages_ShouldFindSenders()
    {
        // Arrange
        var parseResult = await _extractorService.ParseJsonAsync(SampleData.FacebookMessagesJson);
        using var document = JsonDocument.Parse(parseResult.Data!.ToString()!);
        var rootElement = document.RootElement;

        // Act - Get unique senders
        var sendersResult = _queryService.ExecuteJsonPath(rootElement, "$.messages[*].sender_name");
        var uniqueSenders = sendersResult.Results
            .Select(s => s.GetString())
            .Distinct()
            .ToList();

        // Assert
        uniqueSenders.Should().HaveCount(2);
        uniqueSenders.Should().Contain("John Smith");
        uniqueSenders.Should().Contain("CleanPro Services");
    }
}