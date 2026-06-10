// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ManagedCleanroom.Commands;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Invitations;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Invitations;

public sealed class InvitationsAcceptCommandTests : CommandUnitTestsBase<InvitationsAcceptCommand, IManagedCleanroomService>
{
    private const string TestEndpoint = "https://my-cleanroom.cloudapp.azure.net";
    private const string TestCollaborationId = "9d8fa4d3-2808-4067-9c20-db26e2a9ec2f";
    private const string TestInvitationId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("accept", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net --collaboration-id 9d8fa4d3-2808-4067-9c20-db26e2a9ec2f --invitation-id a1b2c3d4-e5f6-7890-abcd-ef1234567890", true)]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net --collaboration-id 9d8fa4d3-2808-4067-9c20-db26e2a9ec2f", false)]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net --invitation-id a1b2c3d4-e5f6-7890-abcd-ef1234567890", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.AcceptInvitationAsync(
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
        var expected = JsonDocument.Parse("""{"invitationId":"a1b2c3d4-e5f6-7890-abcd-ef1234567890","status":"Accepted"}""").RootElement;
        Service.AcceptInvitationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--endpoint", TestEndpoint,
            "--collaboration-id", TestCollaborationId,
            "--invitation-id", TestInvitationId);

        var result = ValidateAndDeserializeResponse(response, ManagedCleanroomJsonContext.Default.JsonElement);
        Assert.Equal(JsonValueKind.Object, result.ValueKind);
        result.AssertProperty("invitationId");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServiceResponse()
    {
        Service.AcceptInvitationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(default(JsonElement));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestEndpoint,
            "--collaboration-id", TestCollaborationId,
            "--invitation-id", TestInvitationId);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).AcceptInvitationAsync(
            TestEndpoint, TestCollaborationId, TestInvitationId, false, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.AcceptInvitationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestEndpoint,
            "--collaboration-id", TestCollaborationId,
            "--invitation-id", TestInvitationId);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
