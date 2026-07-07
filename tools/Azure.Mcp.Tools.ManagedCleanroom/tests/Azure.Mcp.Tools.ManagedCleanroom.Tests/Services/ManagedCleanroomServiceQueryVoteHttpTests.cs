// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Services;

public sealed class ManagedCleanroomServiceQueryVoteHttpTests
{
    [Fact]
    public async Task VoteOnQueryAsync_WithBodyVoteActionApprove_NormalizesVoteActionToAccept()
    {
        var handler = new CapturingHandler();
        var service = CreateService(handler);

        await Assert.ThrowsAsync<RequestFailedException>(async () =>
            await service.VoteOnQueryAsync(
                endpoint: "https://demo.cleanroom.cloudapp.azure.net",
                collaborationId: "collab-123",
                documentId: "query-123",
                body: "{\"voteAction\":\"Approve\",\"proposalId\":\"p-001\"}",
                cancellationToken: TestContext.Current.CancellationToken));

        using var document = JsonDocument.Parse(handler.RequestBody ?? throw new Xunit.Sdk.XunitException("Expected request body to be captured."));
        Assert.Equal("accept", document.RootElement.GetProperty("voteAction").GetString());
        Assert.Equal("p-001", document.RootElement.GetProperty("proposalId").GetString());
    }

    [Fact]
    public async Task VoteOnQueryAsync_WithBodyVoteApprove_AddsNormalizedVoteAction()
    {
        var handler = new CapturingHandler();
        var service = CreateService(handler);

        await Assert.ThrowsAsync<RequestFailedException>(async () =>
            await service.VoteOnQueryAsync(
                endpoint: "https://demo.cleanroom.cloudapp.azure.net",
                collaborationId: "collab-123",
                documentId: "query-123",
                body: "{\"vote\":\"Approve\",\"proposalId\":\"p-001\"}",
                cancellationToken: TestContext.Current.CancellationToken));

        using var document = JsonDocument.Parse(handler.RequestBody ?? throw new Xunit.Sdk.XunitException("Expected request body to be captured."));
        Assert.Equal("Approve", document.RootElement.GetProperty("vote").GetString());
        Assert.Equal("accept", document.RootElement.GetProperty("voteAction").GetString());
        Assert.Equal("p-001", document.RootElement.GetProperty("proposalId").GetString());
    }

    [Fact]
    public async Task VoteOnQueryAsync_WithProposalId_SendsProposalId()
    {
        var handler = new CapturingHandler();
        var service = CreateService(handler);

        await Assert.ThrowsAsync<RequestFailedException>(async () =>
            await service.VoteOnQueryAsync(
                endpoint: "https://demo.cleanroom.cloudapp.azure.net",
                collaborationId: "collab-123",
                documentId: "query-123",
                vote: "Approve",
                proposalId: "p-001",
                cancellationToken: TestContext.Current.CancellationToken));

        using var document = JsonDocument.Parse(handler.RequestBody ?? throw new Xunit.Sdk.XunitException("Expected request body to be captured."));
        Assert.Equal("accept", document.RootElement.GetProperty("voteAction").GetString());
        Assert.Equal("p-001", document.RootElement.GetProperty("proposalId").GetString());
        Assert.False(document.RootElement.TryGetProperty("vote", out _));
    }

    [Fact]
    public async Task VoteOnQueryAsync_WithApproveVote_SendsAcceptVoteAction()
    {
        var handler = new CapturingHandler();
        var service = CreateService(handler);

        await Assert.ThrowsAsync<RequestFailedException>(async () =>
            await service.VoteOnQueryAsync(
                endpoint: "https://demo.cleanroom.cloudapp.azure.net",
                collaborationId: "collab-123",
                documentId: "query-123",
                vote: "Approve",
                proposalId: "p-approve",
                cancellationToken: TestContext.Current.CancellationToken));

        using var document = JsonDocument.Parse(handler.RequestBody ?? throw new Xunit.Sdk.XunitException("Expected request body to be captured."));
        Assert.Equal("accept", document.RootElement.GetProperty("voteAction").GetString());
        Assert.Equal("p-approve", document.RootElement.GetProperty("proposalId").GetString());
        Assert.False(document.RootElement.TryGetProperty("vote", out _));
    }

    [Fact]
    public async Task VoteOnQueryAsync_WithApproveVoteAndNoProposalId_ThrowsArgumentException()
    {
        var handler = new CapturingHandler("{\"proposalId\":\"p-123\"}");
        var service = CreateService(handler);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.VoteOnQueryAsync(
                endpoint: "https://demo.cleanroom.cloudapp.azure.net",
                collaborationId: "collab-123",
                documentId: "query-123",
                vote: "Approve",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("proposalId is required", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(handler.RequestBody);
    }

    [Fact]
    public async Task VoteOnQueryAsync_WithProposalId_DoesNotDependOnQueryGetResponse()
    {
        var handler = new CapturingHandler("{\"proposalId\":\"p-top\",\"history\":{\"proposalId\":\"p-old\"}}");
        var service = CreateService(handler);

        await Assert.ThrowsAsync<RequestFailedException>(async () =>
            await service.VoteOnQueryAsync(
                endpoint: "https://demo.cleanroom.cloudapp.azure.net",
                collaborationId: "collab-123",
                documentId: "query-123",
                vote: "Approve",
                proposalId: "p-explicit",
                cancellationToken: TestContext.Current.CancellationToken));

        using var document = JsonDocument.Parse(handler.RequestBody ?? throw new Xunit.Sdk.XunitException("Expected request body to be captured."));
        Assert.Equal("p-explicit", document.RootElement.GetProperty("proposalId").GetString());
    }

    [Fact]
    public async Task VoteOnQueryAsync_WithRejectVote_SendsRejectVoteAction()
    {
        var handler = new CapturingHandler();
        var service = CreateService(handler);

        await Assert.ThrowsAsync<RequestFailedException>(async () =>
            await service.VoteOnQueryAsync(
                endpoint: "https://demo.cleanroom.cloudapp.azure.net",
                collaborationId: "collab-123",
                documentId: "query-123",
                vote: "Reject",
                proposalId: "p-reject",
                cancellationToken: TestContext.Current.CancellationToken));

        using var document = JsonDocument.Parse(handler.RequestBody ?? throw new Xunit.Sdk.XunitException("Expected request body to be captured."));
        Assert.Equal("reject", document.RootElement.GetProperty("voteAction").GetString());
        Assert.Equal("p-reject", document.RootElement.GetProperty("proposalId").GetString());
        Assert.False(document.RootElement.TryGetProperty("vote", out _));
    }

    [Fact]
    public async Task VoteOnQueryAsync_WithUnknownVote_ThrowsArgumentException()
    {
        var handler = new CapturingHandler();
        var service = CreateService(handler);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.VoteOnQueryAsync(
                endpoint: "https://demo.cleanroom.cloudapp.azure.net",
                collaborationId: "collab-123",
                documentId: "query-123",
                vote: "CustomVote",
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Unsupported vote value", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(handler.RequestBody);
    }

    [Fact]
    public async Task VoteOnQueryAsync_WithoutProposalIdAndWithoutResolvableProposalId_ThrowsArgumentException()
    {
        var handler = new CapturingHandler("{}");
        var service = CreateService(handler);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.VoteOnQueryAsync(
                endpoint: "https://demo.cleanroom.cloudapp.azure.net",
                collaborationId: "collab-123",
                documentId: "query-123",
                vote: "Approve",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("proposalId is required", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(handler.RequestBody);
    }

    private static ManagedCleanroomService CreateService(CapturingHandler handler)
    {
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));

        var tenantService = Substitute.For<ITenantService>();
        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        cloudConfig.AuthorityHost.Returns(new Uri("https://login.microsoftonline.com"));
        tenantService.CloudConfiguration.Returns(cloudConfig);
        tenantService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new TestTokenCredential());

        var subscriptionService = Substitute.For<ISubscriptionService>();
        return new ManagedCleanroomService(subscriptionService, tenantService, httpClientFactory);
    }

    private sealed class TestTokenCredential : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => new("test-token", DateTimeOffset.UtcNow.AddHours(1));

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => ValueTask.FromResult(GetToken(requestContext, cancellationToken));
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly string _queryResponseBody;

        public CapturingHandler(string queryResponseBody = "{}")
        {
            _queryResponseBody = queryResponseBody;
        }

        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_queryResponseBody, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"))
                };
            }

            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{}", Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"))
            };
        }
    }
}
