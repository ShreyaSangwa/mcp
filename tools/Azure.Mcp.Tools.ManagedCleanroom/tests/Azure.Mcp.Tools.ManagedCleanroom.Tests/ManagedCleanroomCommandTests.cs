// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests;

public class ManagedCleanroomCommandTests(ITestOutputHelper output, LiveServerFixture liveServerFixture) : CommandTestsBase(output, liveServerFixture)
{
    [LiveTestOnly]
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
