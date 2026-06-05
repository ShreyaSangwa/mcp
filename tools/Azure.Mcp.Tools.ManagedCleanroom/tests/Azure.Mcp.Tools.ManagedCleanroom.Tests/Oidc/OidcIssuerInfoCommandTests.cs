// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Oidc;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Oidc;

public sealed class OidcIssuerInfoCommandTests : CommandUnitTestsBase<OidcIssuerInfoCommand, IManagedCleanroomService>
{
    private const string TestEndpoint = "https://my-cleanroom.cloudapp.azure.net";
    private const string TestCollaborationId = "fdab3134-931f-4e82-acea-5d224061df80";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("issuer-info", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net --collaboration-id fdab3134-931f-4e82-acea-5d224061df80", true)]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net", false)]
    [InlineData("--collaboration-id fdab3134-931f-4e82-acea-5d224061df80", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetOidcIssuerInfoAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_ReturnsServiceResponse()
    {
        Service.GetOidcIssuerInfoAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(default(JsonElement));

        var response = await ExecuteCommandAsync("--endpoint", TestEndpoint, "--collaboration-id", TestCollaborationId);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetOidcIssuerInfoAsync(
            TestEndpoint, TestCollaborationId, false, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetOidcIssuerInfoAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--endpoint", TestEndpoint, "--collaboration-id", TestCollaborationId);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
