// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests;

public class ManagedCleanroomCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private const string RecordedEndpoint = "https://prod.workload-frontendwestus.cleanroom.cloudapp.azure.net";
    private const string RecordedCollaborationId = "9d8fa4d3-2808-4067-9c20-db26e2a9ec2f";
    private const string RecordedSkrPolicyKid = "my-key-id";
    private const string RecordedKubeconfigPath = "recorded-kubeconfig.yaml";

    private string GetEndpoint() =>
        Settings.DeploymentOutputs.TryGetValue("CLEANROOM_ENDPOINT", out var endpoint) && !string.IsNullOrWhiteSpace(endpoint)
            ? endpoint
            : RecordedEndpoint;

    private string GetCollaborationId() =>
        Settings.DeploymentOutputs.TryGetValue("CLEANROOM_COLLABORATION_ID", out var collaborationId) && !string.IsNullOrWhiteSpace(collaborationId)
            ? collaborationId
            : RecordedCollaborationId;

    private string GetOutputOrDefault(string key, string defaultValue) =>
        Settings.DeploymentOutputs.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : defaultValue;

    private static bool IsEnabled(string? value) =>
        string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);

    [Fact]
    public async Task Should_list_collaborations()
    {
        var endpoint = GetEndpoint();

        var result = await CallToolAsync(
            "managedcleanroom_collaborations_list",
            new()
            {
                { "endpoint", endpoint },
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
            });

        Assert.NotNull(result);
        Output.WriteLine($"Analytics payload: {result.Value}");
    }

    [Fact]
    public async Task Should_get_analytics_skr_policy()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var kid = GetOutputOrDefault("CLEANROOM_SKR_POLICY_KID", RecordedSkrPolicyKid);

        if (TestMode != TestMode.Playback)
        {
            Assert.SkipWhen(string.IsNullOrWhiteSpace(kid), "CLEANROOM_SKR_POLICY_KID must be set for live SKR policy testing.");
        }

        var result = await CallToolAsync(
            "managedcleanroom_analytics_skr-policy",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "kid", kid },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Analytics SKR policy payload: {result.Value}");
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
            });

        Assert.NotNull(result);
        Output.WriteLine($"OIDC issuer info payload: {result.Value}");
    }

    [Fact]
    public async Task Should_set_oidc_issuer_url()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var issuerUrl = Settings.DeploymentOutputs.TryGetValue("CLEANROOM_OIDC_ISSUER_URL", out var url) && !string.IsNullOrWhiteSpace(url)
            ? url
            : "https://issuer.example.com";

        var result = await CallToolAsync(
            "managedcleanroom_oidc_set-issuer-url",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "issuer-url", issuerUrl },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Set OIDC issuer URL payload: {result.Value}");
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
    public async Task Should_create_collaboration_arm_resource()
    {
        Settings.DeploymentOutputs.TryGetValue("CLEANROOM_ENABLE_ARM_MUTATION_TESTS", out var mutationTestsEnabled);
        if (TestMode != TestMode.Playback)
        {
            Assert.SkipWhen(!IsEnabled(mutationTestsEnabled),
                "Set CLEANROOM_ENABLE_ARM_MUTATION_TESTS=true to run collaboration create/delete live tests.");
        }

        var location = GetOutputOrDefault("CLEANROOM_LOCATION", GetOutputOrDefault("LOCATION", "westus"));
        var name = GetOutputOrDefault("CLEANROOM_MUTATION_NAME", $"{Settings.ResourceBaseName}-lt");

        var result = await CallToolAsync(
            "managedcleanroom_collaboration_create",
            new()
            {
                { "name", name },
                { "location", location },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Collaboration create payload: {result.Value}");
    }

    [Fact]
    public async Task Should_get_collaboration_readonly_kubeconfig()
    {
        var result = await CallToolAsync(
            "managedcleanroom_collaboration_get-readonly-kubeconfig",
            new()
            {
                { "name", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId }
            });

        Assert.NotNull(result);
        var hasKubeconfig = result.Value.TryGetProperty("kubeconfig", out var kubeconfig);
        var hasMessage = result.Value.TryGetProperty("message", out var message);

        Assert.True(hasKubeconfig || hasMessage, $"Unexpected response payload: {result.Value}");
        Output.WriteLine(hasKubeconfig
            ? $"Collaboration readonly kubeconfig length: {kubeconfig.GetString()?.Length ?? 0}"
            : $"Collaboration get-readonly-kubeconfig command returned message payload: {message}");
    }

    [Fact]
    public async Task Should_recover_collaboration_arm_resource()
    {
        var result = await CallToolAsync(
            "managedcleanroom_collaboration_recover",
            new()
            {
                { "name", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId }
            });

        Assert.NotNull(result);
        Output.WriteLine($"Collaboration recover payload: {result.Value}");
    }

    [Fact]
    public async Task Should_delete_collaboration_arm_resource()
    {
        Settings.DeploymentOutputs.TryGetValue("CLEANROOM_ENABLE_ARM_MUTATION_TESTS", out var mutationTestsEnabled);
        if (TestMode != TestMode.Playback)
        {
            Assert.SkipWhen(!IsEnabled(mutationTestsEnabled),
                "Set CLEANROOM_ENABLE_ARM_MUTATION_TESTS=true to run collaboration create/delete live tests.");
        }

        var name = GetOutputOrDefault("CLEANROOM_MUTATION_NAME", $"{Settings.ResourceBaseName}-lt");

        var result = await CallToolAsync(
            "managedcleanroom_collaboration_delete",
            new()
            {
                { "name", name },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Collaboration delete payload: {result.Value}");
    }

    [Fact]
    public async Task Should_open_grafana_dashboard()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var kubeconfigPath = GetOutputOrDefault("CLEANROOM_KUBECONFIG_PATH", RecordedKubeconfigPath);

        if (TestMode != TestMode.Playback)
        {
            Assert.SkipWhen(string.IsNullOrWhiteSpace(kubeconfigPath), "CLEANROOM_KUBECONFIG_PATH must be set for dashboard live testing.");
            Assert.SkipWhen(!File.Exists(kubeconfigPath), $"Kubeconfig path does not exist: {kubeconfigPath}");
        }

        var result = await CallToolAsync(
            "managedcleanroom_dashboard_open-grafana",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "kubeconfig-path", kubeconfigPath },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Dashboard open-grafana payload: {result.Value}");
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
            });

        Assert.NotNull(result);
        Output.WriteLine($"OIDC keys payload: {result.Value}");
    }

    [Fact]
    public async Task Should_publish_dataset()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var documentId = Settings.DeploymentOutputs.TryGetValue("CLEANROOM_DATASET_DOCUMENT_ID", out var docId) && !string.IsNullOrWhiteSpace(docId)
            ? docId
            : "00000000-0000-0000-0000-000000000000";

        var result = await CallToolAsync(
            "managedcleanroom_datasets_publish",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "document-id", documentId },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Dataset publish payload: {result.Value}");
    }

    [Fact]
    public async Task Should_get_dataset()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var documentId = Settings.DeploymentOutputs.TryGetValue("CLEANROOM_DATASET_DOCUMENT_ID", out var docId) && !string.IsNullOrWhiteSpace(docId)
            ? docId
            : "00000000-0000-0000-0000-000000000000";

        var result = await CallToolAsync(
            "managedcleanroom_datasets_get",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "document-id", documentId },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Dataset get payload: {result.Value}");
    }

    [Fact]
    public async Task Should_put_consent()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var documentId = Settings.DeploymentOutputs.TryGetValue("CLEANROOM_CONSENT_DOCUMENT_ID", out var docId) && !string.IsNullOrWhiteSpace(docId)
            ? docId
            : "00000000-0000-0000-0000-000000000000";
        var consentBody = "{\"consentAction\":\"enable\"}";

        var result = await CallToolAsync(
            "managedcleanroom_consent_put",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "document-id", documentId },
                { "body", consentBody },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Consent put payload: {result.Value}");
    }

    [Fact]
    public async Task Should_list_datasets()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();

        var result = await CallToolAsync(
            "managedcleanroom_datasets_list",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Datasets list payload: {result.Value}");
    }

    [Fact]
    public async Task Should_build_dataset_body()
    {
        var result = await CallToolAsync(
            "managedcleanroom_datasets_build-body",
            new()
            {
                { "dataset-name", "mcp-dataset-v1" },
                { "container-name", "dataset-input" },
                { "storage-account-url", "https://contosodata.blob.core.windows.net" },
                { "schema-field", new[] { "id:string", "value:string" } },
                { "allowed-field", new[] { "id", "value" } },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Dataset build-body payload: {result.Value}");
    }

    [Fact]
    public async Task Should_publish_query()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var documentId = Settings.DeploymentOutputs.TryGetValue("CLEANROOM_QUERY_DOCUMENT_ID", out var docId) && !string.IsNullOrWhiteSpace(docId)
            ? docId
            : "00000000-0000-0000-0000-000000000000";

        var result = await CallToolAsync(
            "managedcleanroom_queries_publish",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "document-id", documentId },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Query publish payload: {result.Value}");
    }

    [Fact]
    public async Task Should_build_query_body()
    {
        var outputDataset = GetOutputOrDefault("CLEANROOM_QUERY_OUTPUT_DATASET_ID", "woodgrove-output-csv-v1");
        var publisherInputDataset = GetOutputOrDefault("CLEANROOM_QUERY_PUBLISHER_DATASET_ID", "northwind-input-csv-v1");
        var consumerInputDataset = GetOutputOrDefault("CLEANROOM_QUERY_CONSUMER_DATASET_ID", "woodgrove-input-csv-v1");

        var queryDirectory = Path.Combine(Path.GetTempPath(), "mcp-managedcleanroom-query-body");
        Directory.CreateDirectory(queryDirectory);
        var segmentPath = Path.Combine(queryDirectory, "segment001.txt");
        await File.WriteAllTextAsync(segmentPath, "SELECT 1 AS id", CancellationToken.None);

        try
        {
            var result = await CallToolAsync(
                "managedcleanroom_queries_build-body",
                new()
                {
                    { "query-name", "mcp-query-v1" },
                    { "query-directory", queryDirectory },
                    { "publisher-input-dataset", publisherInputDataset },
                    { "consumer-input-dataset", consumerInputDataset },
                    { "output-dataset", outputDataset },
                });

            Assert.NotNull(result);
            Output.WriteLine($"Query build-body payload: {result.Value}");
        }
        finally
        {
            if (Directory.Exists(queryDirectory))
            {
                Directory.Delete(queryDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Should_get_query()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var documentId = Settings.DeploymentOutputs.TryGetValue("CLEANROOM_QUERY_DOCUMENT_ID", out var docId) && !string.IsNullOrWhiteSpace(docId)
            ? docId
            : "00000000-0000-0000-0000-000000000000";

        var result = await CallToolAsync(
            "managedcleanroom_queries_get",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "document-id", documentId },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Query get payload: {result.Value}");
    }

    [Fact]
    public async Task Should_list_queries()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();

        var result = await CallToolAsync(
            "managedcleanroom_queries_list",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Queries list payload: {result.Value}");
    }

    [Fact]
    public async Task Should_vote_on_query()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var documentId = Settings.DeploymentOutputs.TryGetValue("CLEANROOM_QUERY_DOCUMENT_ID", out var docId) && !string.IsNullOrWhiteSpace(docId)
            ? docId
            : "00000000-0000-0000-0000-000000000000";

        var result = await CallToolAsync(
            "managedcleanroom_queries_vote",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "document-id", documentId },
                { "vote", "Approve" },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Query vote payload: {result.Value}");
    }

    [Fact]
    public async Task Should_run_query()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var documentId = Settings.DeploymentOutputs.TryGetValue("CLEANROOM_QUERY_DOCUMENT_ID", out var docId) && !string.IsNullOrWhiteSpace(docId)
            ? docId
            : "00000000-0000-0000-0000-000000000000";

        var result = await CallToolAsync(
            "managedcleanroom_queries_run",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "document-id", documentId },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Query run payload: {result.Value}");
    }

    [Fact]
    public async Task Should_get_query_runs()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var documentId = Settings.DeploymentOutputs.TryGetValue("CLEANROOM_QUERY_DOCUMENT_ID", out var docId) && !string.IsNullOrWhiteSpace(docId)
            ? docId
            : "00000000-0000-0000-0000-000000000000";

        var result = await CallToolAsync(
            "managedcleanroom_runs_get",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "document-id", documentId },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Runs get payload: {result.Value}");
    }

    [Fact]
    public async Task Should_download_query_output()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var documentId = GetOutputOrDefault("CLEANROOM_QUERY_DOCUMENT_ID", "00000000-0000-0000-0000-000000000000");
        var outputDirectory = Path.Combine(Path.GetTempPath(), "mcp-managedcleanroom-output");
        Directory.CreateDirectory(outputDirectory);

        try
        {
            var result = await CallToolAsync(
                "managedcleanroom_queries_download-output",
                new()
                {
                    { "endpoint", endpoint },
                    { "collaboration-id", collaborationId },
                    { "document-id", documentId },
                    { "output-directory", outputDirectory },
                });

            Assert.NotNull(result);
            Output.WriteLine($"Queries download-output payload: {result.Value}");
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Should_get_query_run_history()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();
        var documentId = Settings.DeploymentOutputs.TryGetValue("CLEANROOM_QUERY_DOCUMENT_ID", out var docId) && !string.IsNullOrWhiteSpace(docId)
            ? docId
            : "00000000-0000-0000-0000-000000000000";

        var result = await CallToolAsync(
            "managedcleanroom_queries_runs",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
                { "document-id", documentId },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Query runs payload: {result.Value}");
    }

    [Fact]
    public async Task Should_list_audit_events()
    {
        var endpoint = GetEndpoint();
        var collaborationId = GetCollaborationId();

        var result = await CallToolAsync(
            "managedcleanroom_auditevents_list",
            new()
            {
                { "endpoint", endpoint },
                { "collaboration-id", collaborationId },
            });

        Assert.NotNull(result);
        Output.WriteLine($"Audit events payload: {result.Value}");
    }
}
