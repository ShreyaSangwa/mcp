// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ManagedCleanroom.Commands;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Oidc;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Oidc;

public sealed class OidcSetIssuerUrlCommandTests : CommandUnitTestsBase<OidcSetIssuerUrlCommand, IManagedCleanroomService>
{
    private const string TestEndpoint = "https://my-cleanroom.cloudapp.azure.net";
    private const string TestCollaborationId = "9d8fa4d3-2808-4067-9c20-db26e2a9ec2f";
    private const string TestIssuerUrl = "https://issuer.example.com";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("set-issuer-url", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net --collaboration-id 9d8fa4d3-2808-4067-9c20-db26e2a9ec2f --issuer-url https://issuer.example.com", true)]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net --collaboration-id 9d8fa4d3-2808-4067-9c20-db26e2a9ec2f", false)]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net --issuer-url https://issuer.example.com", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.SetOidcIssuerUrlAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(default(JsonElement));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var expected = JsonDocument.Parse("""{"issuerUrl":"https://issuer.example.com","status":"Registered"}""").RootElement;
        Service.SetOidcIssuerUrlAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--endpoint", TestEndpoint,
            "--collaboration-id", TestCollaborationId,
            "--issuer-url", TestIssuerUrl);

        var result = ValidateAndDeserializeResponse(response, ManagedCleanroomJsonContext.Default.JsonElement);
        Assert.Equal(JsonValueKind.Object, result.ValueKind);
        result.AssertProperty("issuerUrl");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServiceResponse()
    {
        Service.SetOidcIssuerUrlAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(default(JsonElement));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestEndpoint,
            "--collaboration-id", TestCollaborationId,
            "--issuer-url", TestIssuerUrl);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SetOidcIssuerUrlAsync(
            TestEndpoint, TestCollaborationId, TestIssuerUrl, false, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.SetOidcIssuerUrlAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestEndpoint,
            "--collaboration-id", TestCollaborationId,
            "--issuer-url", TestIssuerUrl);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
