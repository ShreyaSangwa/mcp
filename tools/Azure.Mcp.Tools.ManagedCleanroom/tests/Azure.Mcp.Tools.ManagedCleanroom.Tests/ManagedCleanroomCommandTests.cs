// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests;

public class ManagedCleanroomCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task Should_list_collaborations()
    {
        var endpoint = Settings.DeploymentOutputs["CLEANROOM_ENDPOINT"];

        var result = await CallToolAsync(
            "managedcleanroom_collaborations_list",
            new()
            {
                { "endpoint", endpoint }
            });

        Assert.NotNull(result);
        var collaborations = result.Value.AssertProperty("collaborations");
        Output.WriteLine($"Collaborations payload: {collaborations}");
    }

    [Fact]
    public async Task Should_get_collaboration()
    {
        var endpoint = Settings.DeploymentOutputs["CLEANROOM_ENDPOINT"];
        var collaborationId = Settings.DeploymentOutputs["CLEANROOM_COLLABORATION_ID"];

        var result = await CallToolAsync(
            "managedcleanroom_collaborations_get",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId }
            });

        Assert.NotNull(result);
        result.Value.AssertProperty("collaborationId");
        Output.WriteLine($"Collaboration payload: {result.Value}");
    }

    [Fact]
    public async Task Should_get_analytics()
    {
        var endpoint = Settings.DeploymentOutputs["CLEANROOM_ENDPOINT"];
        var collaborationId = Settings.DeploymentOutputs["CLEANROOM_COLLABORATION_ID"];

        var result = await CallToolAsync(
            "managedcleanroom_analytics_get",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId }
            });

        Assert.NotNull(result);
        Output.WriteLine($"Analytics payload: {result.Value}");
    }

    [Fact]
    public async Task Should_get_oidc_issuer_info()
    {
        var endpoint = Settings.DeploymentOutputs["CLEANROOM_ENDPOINT"];
        var collaborationId = Settings.DeploymentOutputs["CLEANROOM_COLLABORATION_ID"];

        var result = await CallToolAsync(
            "managedcleanroom_oidc_issuer-info",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId }
            });

        Assert.NotNull(result);
        Output.WriteLine($"OIDC issuer info payload: {result.Value}");
    }
}

