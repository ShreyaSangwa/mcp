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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Services;

public sealed class ManagedCleanroomServiceDatasetPublishHttpTests
{
    [Fact]
    public async Task PublishDatasetAsync_SendsSdkCompatibleJsonBodyToFrontend()
    {
        var handler = new CapturingHandler();
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
        var service = new ManagedCleanroomService(subscriptionService, tenantService, httpClientFactory);

        const string flatDatasetBody = """
            {
              "name": "demo",
              "datasetSchema": { "format": "csv", "fields": [] },
              "datasetAccessPolicy": { "accessMode": "read", "allowedFields": [] },
              "store": {
                "storageAccountUrl": "https://demo.blob.core.windows.net",
                "containerName": "input",
                "storageAccountType": "Azure_BlobStorage",
                "encryptionMode": "SSE"
              }
            }
            """;

        await Assert.ThrowsAsync<RequestFailedException>(async () =>
            await service.PublishDatasetAsync(
                endpoint: "https://demo.cleanroom.cloudapp.azure.net",
                collaborationId: "collab-123",
                documentId: "dataset-123",
                body: flatDatasetBody,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpMethod.Post, handler.RequestMethod);
        Assert.NotNull(handler.RequestUri);
        Assert.Contains("/analytics/datasets/dataset-123/publish", handler.RequestUri!.AbsolutePath, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("application/json", handler.ContentType);

        using var document = JsonDocument.Parse(handler.RequestBody ?? throw new Xunit.Sdk.XunitException("Expected request body to be captured."));
        Assert.Equal("demo", document.RootElement.GetProperty("name").GetString());
        Assert.Equal("csv", document.RootElement.GetProperty("datasetSchema").GetProperty("format").GetString());
        Assert.Equal("input", document.RootElement.GetProperty("store").GetProperty("containerName").GetString());
    }

        [Fact]
        public async Task PublishDatasetAsync_WithDatasetDetailsWrapper_UnwrapsBodyBeforeSend()
        {
                var handler = new CapturingHandler();
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
                var service = new ManagedCleanroomService(subscriptionService, tenantService, httpClientFactory);

                const string wrappedBody = """
                        {
                            "datasetDetails": {
                                "name": "demo",
                                "datasetSchema": { "format": "csv", "fields": [] },
                                "datasetAccessPolicy": { "accessMode": "read", "allowedFields": [] },
                                "store": {
                                    "storageAccountUrl": "https://demo.blob.core.windows.net",
                                    "containerName": "input",
                                    "storageAccountType": "Azure_BlobStorage",
                                    "encryptionMode": "SSE"
                                }
                            }
                        }
                        """;

                await Assert.ThrowsAsync<RequestFailedException>(async () =>
                        await service.PublishDatasetAsync(
                                endpoint: "https://demo.cleanroom.cloudapp.azure.net",
                                collaborationId: "collab-123",
                                documentId: "dataset-123",
                                body: wrappedBody,
                                cancellationToken: TestContext.Current.CancellationToken));

                using var document = JsonDocument.Parse(handler.RequestBody ?? throw new Xunit.Sdk.XunitException("Expected request body to be captured."));
                Assert.Equal("demo", document.RootElement.GetProperty("name").GetString());
                Assert.False(document.RootElement.TryGetProperty("datasetDetails", out _));
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
        public string? RequestBody { get; private set; }
        public string? ContentType { get; private set; }
        public Uri? RequestUri { get; private set; }
        public HttpMethod? RequestMethod { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            RequestMethod = request.Method;
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"))
            };
        }
    }
}