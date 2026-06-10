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

    [Fact]
    public async Task Should_add_collaborator()
    {
        var result = await CallToolAsync(
            "managedcleanroom_collaboration_add-collaborator",
            new()
            {
                { "name", Settings.ResourceBaseName },
                { "collaborator-user-identifier", "alice@contoso.com" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId }
            });

        Assert.NotNull(result);
        Output.WriteLine($"Add collaborator payload: {result.Value}");
    }

    [Fact]
    public async Task Should_enable_workload()
    {
        var result = await CallToolAsync(
            "managedcleanroom_collaboration_enable-workload",
            new()
            {
                { "name", Settings.ResourceBaseName },
                { "workload-type", "Analytics" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId }
            });

        Assert.NotNull(result);
        Output.WriteLine($"Enable workload payload: {result.Value}");
    }

    [Fact]
    public async Task Should_list_invitations()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();

        var result = await CallToolAsync(
            "managedcleanroom_invitations_list",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "allow-untrusted-cert", true }
            });

        Assert.NotNull(result);
        var hasInvitations = result.Value.TryGetProperty("invitations", out var invitations);
        var hasMessage = result.Value.TryGetProperty("message", out var message);

        Assert.True(hasInvitations || hasMessage, $"Unexpected response payload: {result.Value}");
        Output.WriteLine(hasInvitations
            ? $"Invitations payload: {invitations}"
            : $"Invitations command returned message payload: {message}");
    }

    [Fact]
    public async Task Should_accept_invitation()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var invitationId = Settings.DeploymentOutputs.TryGetValue("CLEANROOM_INVITATION_ID", out var id) && !string.IsNullOrWhiteSpace(id)
            ? id
            : "00000000-0000-0000-0000-000000000000";

        var result = await CallToolAsync(
            "managedcleanroom_invitations_accept",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "invitation-id", invitationId },
                { "allow-untrusted-cert", true }
            });

        Assert.NotNull(result);
        Output.WriteLine($"Accept invitation payload: {result.Value}");
    }

    [Fact]
    public async Task Should_get_collaboration_arm_resource()
    {
        var result = await CallToolAsync(
            "managedcleanroom_collaboration_get",
            new()
            {
                { "name", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId }
            });

        Assert.NotNull(result);
        var hasProvisioningState = result.Value.TryGetProperty("provisioningState", out var provisioningState);
        var hasMessage = result.Value.TryGetProperty("message", out var message);

        Assert.True(hasProvisioningState || hasMessage, $"Unexpected response payload: {result.Value}");
        Output.WriteLine(hasProvisioningState
            ? $"Collaboration ARM provisioningState: {provisioningState}"
            : $"Collaboration get command returned message payload: {message}");
    }

    [Fact]
    public async Task Should_get_oidc_keys()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();

        var result = await CallToolAsync(
            "managedcleanroom_oidc_keys",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "allow-untrusted-cert", true }
            });

        Assert.NotNull(result);
        Output.WriteLine($"OIDC keys payload: {result.Value}");
    }
}

