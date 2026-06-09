// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests;

public class ManagedCleanroomCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private const string RecordedEndpoint = "https://prod.workload-frontendwestus.cleanroom.cloudapp.azure.net";
    private const string RecordedCollaborationId = "9d8fa4d3-2808-4067-9c20-db26e2a9ec2f";

    private string GetEndpoint() =>
        Settings.DeploymentOutputs.TryGetValue("CLEANROOM_ENDPOINT", out var endpoint) && !string.IsNullOrWhiteSpace(endpoint)
            ? endpoint
            : RecordedEndpoint;

    private string GetCollaborationId() =>
        Settings.DeploymentOutputs.TryGetValue("CLEANROOM_COLLABORATION_ID", out var collaborationId) && !string.IsNullOrWhiteSpace(collaborationId)
            ? collaborationId
            : RecordedCollaborationId;

    [Fact]
    public async Task Should_list_collaborations()
    {
        var endpoint = GetEndpoint();

        var result = await CallToolAsync(
            "managedcleanroom_collaborations_list",
            new()
            {
                { "endpoint", endpoint },
                { "allow-untrusted-cert", true }
            });

        Assert.NotNull(result);
        var hasCollaborations = result.Value.TryGetProperty("collaborations", out var collaborations);
        var hasMessage = result.Value.TryGetProperty("message", out var message);

        Assert.True(hasCollaborations || hasMessage, $"Unexpected response payload: {result.Value}");
        Output.WriteLine(hasCollaborations
            ? $"Collaborations payload: {collaborations}"
            : $"Collaborations command returned message payload: {message}");
    }

    [Fact]
    public async Task Should_get_collaboration()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();

        var result = await CallToolAsync(
            "managedcleanroom_collaborations_get",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "allow-untrusted-cert", true }
            });

        Assert.NotNull(result);
        var hasCollaborationId = result.Value.TryGetProperty("collaborationId", out var collaborationIdElement);
        var hasMessage = result.Value.TryGetProperty("message", out var message);

        Assert.True(hasCollaborationId || hasMessage, $"Unexpected response payload: {result.Value}");
        Output.WriteLine(hasCollaborationId
            ? $"Collaboration payload id: {collaborationIdElement}"
            : $"Collaboration command returned message payload: {message}");
    }

    [Fact]
    public async Task Should_get_analytics()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();

        var result = await CallToolAsync(
            "managedcleanroom_analytics_get",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "allow-untrusted-cert", true }
            });

        Assert.NotNull(result);
        Output.WriteLine($"Analytics payload: {result.Value}");
    }

    [Fact]
    public async Task Should_get_oidc_issuer_info()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();

        var result = await CallToolAsync(
            "managedcleanroom_oidc_issuer-info",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "allow-untrusted-cert", true }
            });

        Assert.NotNull(result);
        Output.WriteLine($"OIDC issuer info payload: {result.Value}");
    }
}

