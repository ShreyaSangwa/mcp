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
}

