// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ManagedCleanroom.Commands;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.CollaborationArm;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Collaboration;

public sealed class CollaborationGetReadonlyKubeconfigCommandTests
    : SubscriptionCommandUnitTestsBase<CollaborationGetReadonlyKubeconfigCommand, IManagedCleanroomServiceControlPlane>
{
    private const string TestName = "my-collab";
    private const string TestResourceGroup = "my-rg";
    private const string TestSubscription = "test-sub";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get-readonly-kubeconfig", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--name my-collab --resource-group my-rg --subscription test-sub", true)]
    [InlineData("--resource-group my-rg --subscription test-sub", false)]
    [InlineData("--name my-collab --subscription test-sub", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetCollaborationReadonlyKubeconfigAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
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
        var expected = JsonDocument.Parse("""{"kubeconfig":"apiVersion: v1\nkind: Config\n"}""").RootElement;
        Service.GetCollaborationReadonlyKubeconfigAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--name", TestName, "--resource-group", TestResourceGroup, "--subscription", TestSubscription);

        var result = ValidateAndDeserializeResponse(response, ManagedCleanroomJsonContext.Default.JsonElement);
        Assert.Equal(JsonValueKind.Object, result.ValueKind);
        result.AssertProperty("kubeconfig");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServiceResponse()
    {
        Service.GetCollaborationReadonlyKubeconfigAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(default(JsonElement));

        var response = await ExecuteCommandAsync(
            "--name", TestName, "--resource-group", TestResourceGroup, "--subscription", TestSubscription);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetCollaborationReadonlyKubeconfigAsync(
            TestName, TestResourceGroup, TestSubscription,
            null, Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        Service.GetCollaborationReadonlyKubeconfigAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Azure.RequestFailedException(404, "Not Found"));

        var response = await ExecuteCommandAsync(
            "--name", TestName, "--resource-group", TestResourceGroup, "--subscription", TestSubscription);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetCollaborationReadonlyKubeconfigAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--name", TestName, "--resource-group", TestResourceGroup, "--subscription", TestSubscription);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
