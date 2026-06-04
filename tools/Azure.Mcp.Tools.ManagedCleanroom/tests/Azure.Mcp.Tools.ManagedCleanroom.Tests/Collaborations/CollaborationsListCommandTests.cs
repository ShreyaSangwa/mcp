// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Commands;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaborations;
using Azure.Mcp.Tools.ManagedCleanroom.Models;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Collaborations;

public sealed class CollaborationsListCommandTests : CommandUnitTestsBase<CollaborationsListCommand, IManagedCleanroomService>
{
    private const string TestEndpoint = "https://my-cleanroom.azure.com";

    [Fact]
    public async Task Execute_WithEndpoint_ReturnsCollaborations()
    {
        var collaborations = new List<Collaboration>
        {
            new() { CollaborationId = "c1", CollaborationName = "Alpha", UserStatus = "Active" },
            new() { CollaborationId = "c2", CollaborationName = "Beta", UserStatus = "Pending" },
        };

        Service.ListCollaborationsAsync(TestEndpoint, null, Arg.Any<CancellationToken>())
            .Returns(collaborations);

        var response = await ExecuteCommandAsync("--endpoint", TestEndpoint);

        var result = ValidateAndDeserializeResponse(response, ManagedCleanroomJsonContext.Default.CollaborationsListResult);
        Assert.NotNull(result);
        Assert.Equal(2, result!.Collaborations.Count);
        Assert.Equal("c1", result.Collaborations[0].CollaborationId);

        await Service.Received(1).ListCollaborationsAsync(TestEndpoint, null, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Execute_WithActiveOnly_PassesFlagThrough(bool activeOnly)
    {
        Service.ListCollaborationsAsync(TestEndpoint, activeOnly, Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync(
            "--endpoint", TestEndpoint,
            "--active-only", activeOnly.ToString().ToLowerInvariant());

        var result = ValidateAndDeserializeResponse(response, ManagedCleanroomJsonContext.Default.CollaborationsListResult);
        Assert.NotNull(result);
        Assert.Empty(result!.Collaborations);

        await Service.Received(1).ListCollaborationsAsync(TestEndpoint, activeOnly, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WithoutEndpoint_FailsValidation()
    {
        var response = await ExecuteCommandAsync();
        Assert.NotEqual(System.Net.HttpStatusCode.OK, response.Status);
    }
}
