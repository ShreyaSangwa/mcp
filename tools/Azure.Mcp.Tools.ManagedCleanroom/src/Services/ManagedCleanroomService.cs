// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Text.Json;
using AnalyticsFrontendAPI;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager;
using Azure.ResourceManager.CleanRoom;
using Azure.ResourceManager.CleanRoom.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Options;
namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

public class ManagedCleanroomService(ISubscriptionService subscriptionService, ITenantService tenantService, IHttpClientFactory httpClientFactory)
    : BaseAzureResourceService(subscriptionService, tenantService), IManagedCleanroomService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private static readonly TimeSpan WorkloadEndpointTimeout = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan WorkloadHealthTimeout = TimeSpan.FromMinutes(10);

    public async Task<JsonElement> ListCollaborationsAsync(
        string endpoint,
        bool? activeOnly = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.GetGetsAsync(activeOnly, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetCollaborationAsync(
        string endpoint,
        string collaborationId,
        bool? includeDeleted = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.IdGetAsync(collaborationId, includeDeleted, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetAnalyticsAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsGetAsync(collaborationId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetAnalyticsSkrPolicyAsync(
        string endpoint,
        string collaborationId,
        string kid,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(kid), kid));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsSkrPolicyGetAsync(collaborationId, kid, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> ListInvitationsAsync(
        string endpoint,
        string collaborationId,
        bool? pendingOnly = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.InvitationsGetAsync(collaborationId, pendingOnly, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> AcceptInvitationAsync(
        string endpoint,
        string collaborationId,
        string invitationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(invitationId), invitationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.InvitationIdAcceptPostAsync(collaborationId, invitationId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetOidcIssuerInfoAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.OidcIssuerInfoGetAsync(collaborationId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetOidcKeysAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.OidcKeysGetAsync(collaborationId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> SetOidcIssuerUrlAsync(
        string endpoint,
        string collaborationId,
        string issuerUrl,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(issuerUrl), issuerUrl));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        // Build the request body expected by the frontend API: {"url": "<value>"}
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("url", issuerUrl);
            writer.WriteEndObject();
            writer.Flush();
        }

        var content = RequestContent.Create(buffer.WrittenMemory);
        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.OidcSetIssuerUrlPostAsync(collaborationId, content, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> PublishDatasetAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        // The publish POST body is empty — the document ID in the URL path identifies the dataset.
        var content = RequestContent.Create(BinaryData.FromBytes("{}"u8.ToArray()));
        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsDatasetsDocumentIdPublishPostAsync(
            collaborationId, documentId, content, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetDatasetAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsDatasetsDocumentIdGetAsync(
            collaborationId, documentId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> PutConsentAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        // The PUT body is empty — the document ID in the URL path identifies the consent document.
        var content = RequestContent.Create(BinaryData.FromBytes("{}"u8.ToArray()));
        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.ConsentDocumentIdPutAsync(
            collaborationId, documentId, content, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> ListDatasetsAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsDatasetsListGetAsync(collaborationId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> PublishQueryAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var content = RequestContent.Create(BinaryData.FromBytes("{}"u8.ToArray()));
        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsQueriesDocumentIdPublishPostAsync(
            collaborationId, documentId, content, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetQueryAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsQueriesDocumentIdGetAsync(
            collaborationId, documentId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> ListQueriesAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsQueriesListGetAsync(collaborationId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> VoteOnQueryAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        string vote,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId),
            (nameof(vote), vote));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        // Build the request body: {"vote": "<value>"}
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("vote", vote);
            writer.WriteEndObject();
            writer.Flush();
        }

        var content = RequestContent.Create(buffer.WrittenMemory);
        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsQueriesDocumentIdVotePostAsync(
            collaborationId, documentId, content, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> RunQueryAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var content = RequestContent.Create(BinaryData.FromBytes("{}"u8.ToArray()));
        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsQueriesDocumentIdRunPostAsync(
            collaborationId, documentId, content, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetQueryRunsAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsQueriesDocumentIdRunsGetAsync(
            collaborationId, documentId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> ListAuditEventsAsync(
        string endpoint,
        string collaborationId,
        string? scope = null,
        string? fromSeqno = null,
        string? toSeqno = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsAuditeventsGetAsync(
            collaborationId, scope, fromSeqno, toSeqno, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> AddCollaboratorAsync(
        string name,
        string resourceGroup,
        string subscription,
        string collaboratorUserIdentifier,
        string? collaboratorObjectId = null,
        string? collaboratorTenantId = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(collaboratorUserIdentifier), collaboratorUserIdentifier));

        var collaborationResource = await GetCollaborationResourceAsync(
            name, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var content = new Azure.ResourceManager.CleanRoom.Models.AddCollaboratorContent
        {
            UserIdentifier = collaboratorUserIdentifier,
            ObjectId = collaboratorObjectId,
            TenantId = collaboratorTenantId
        };

        await collaborationResource
            .AddCollaboratorAsync(WaitUntil.Completed, content, cancellationToken)
            .ConfigureAwait(false);

        // Re-fetch and return latest collaboration state as JSON.
        var refreshed = await collaborationResource.GetAsync(cancellationToken).ConfigureAwait(false);
        return SerializeCollaborationData(refreshed.Value.Data);
    }

    public async Task<JsonElement> EnableWorkloadAsync(
        string name,
        string resourceGroup,
        string subscription,
        string workloadType,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(workloadType), workloadType));

        var collaborationResource = await GetCollaborationResourceAsync(
            name, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var wlType = (Azure.ResourceManager.CleanRoom.Models.WorkloadType)Enum.Parse(
            typeof(Azure.ResourceManager.CleanRoom.Models.WorkloadType), workloadType, ignoreCase: true);

        var content = new Azure.ResourceManager.CleanRoom.Models.EnableWorkloadContent(wlType);

        // WaitUntil.Started — return immediately after the request is accepted.
        await collaborationResource
            .EnableWorkloadAsync(WaitUntil.Started, content, cancellationToken)
            .ConfigureAwait(false);

        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        writer.WriteStartObject();
        writer.WriteString("name", name);
        writer.WriteString("resourceGroup", resourceGroup);
        writer.WriteString("subscription", subscription);
        writer.WriteString("workloadType", workloadType);
        writer.WriteString("provisioningState", "Accepted");
        writer.WriteEndObject();
        writer.Flush();

        return JsonSerializer.Deserialize(buffer.WrittenSpan, ManagedCleanroomSerializerContext.Default.JsonElement);
    }

    private async Task<CollaborationResource> GetCollaborationResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var subscriptionResource = await _subscriptionService
            .GetSubscription(subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var resourceId = CollaborationResource.CreateResourceIdentifier(
            subscriptionResource.Id.SubscriptionId!, resourceGroup, name);

        return armClient.GetCollaborationResource(resourceId);
    }

    private static JsonElement SerializeCollaborationData(CollaborationData data)
    {
        // Serialize via Utf8JsonWriter for AOT safety — no reflection.
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        writer.WriteStartObject();
        writer.WriteString("collaborationState", data.CollaborationState?.ToString());
        writer.WriteString("provisioningState", data.ProvisioningState?.ToString());
        if (data.Health is not null)
        {
            writer.WritePropertyName("health");
            writer.WriteStartObject();
            writer.WriteString("healthState", data.Health.HealthState.ToString());
            writer.WriteEndObject();
        }
        writer.WritePropertyName("workloads");
        writer.WriteStartArray();
        foreach (var wl in data.Workloads ?? [])
        {
            writer.WriteStartObject();
            writer.WriteString("workloadType", wl.WorkloadType.ToString());
            writer.WriteString("endpoint", wl.Endpoint?.ToString());
            writer.WriteString("namespace", wl.Namespace);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();

        return JsonSerializer.Deserialize(buffer.WrittenSpan, ManagedCleanroomSerializerContext.Default.JsonElement);
    }

    /// <summary>
    /// Extracts the calling user's email/UPN from the preferred_username or upn claim in the ARM access token.
    /// </summary>
    private async Task<string> GetCallerEmailAsync(string? tenant, CancellationToken cancellationToken)
    {
        var token = await GetArmAccessTokenAsync(tenant, cancellationToken).ConfigureAwait(false);

        // JWT is three base64url-encoded segments separated by dots. Decode the payload (second segment).
        var parts = token.Token.Split('.');
        if (parts.Length < 2)
            return string.Empty;

        // Base64url decode: replace URL-safe chars and pad to a multiple of 4.
        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
        var bytes = Convert.FromBase64String(payload);

        using var doc = JsonDocument.Parse(bytes);
        var root = doc.RootElement;

        if (root.TryGetProperty("preferred_username", out var pref) && pref.ValueKind == JsonValueKind.String)
            return pref.GetString() ?? string.Empty;

        if (root.TryGetProperty("upn", out var upn) && upn.ValueKind == JsonValueKind.String)
            return upn.GetString() ?? string.Empty;

        return string.Empty;
    }

    private static JsonElement BuildAcceptedCreateResult(
        string name,
        string resourceGroup,
        string subscription,
        string location,
        string? resourceLocation,
        string[]? collaborators)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        writer.WriteStartObject();
        writer.WriteString("name", name);
        writer.WriteString("resourceGroup", resourceGroup);
        writer.WriteString("subscription", subscription);
        writer.WriteString("location", location);
        writer.WriteString("resourceLocation", resourceLocation ?? location);
        writer.WriteString("provisioningState", "Accepted");
        writer.WritePropertyName("collaborators");
        writer.WriteStartArray();
        foreach (var collaborator in collaborators ?? [])
        {
            writer.WriteStartObject();
            writer.WriteString("userIdentifier", collaborator);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();

        return JsonSerializer.Deserialize(buffer.WrittenSpan, ManagedCleanroomSerializerContext.Default.JsonElement);
    }

    public async Task<JsonElement> GetCollaborationArmResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var collaborationResource = await GetCollaborationResourceAsync(
            name, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var response = await collaborationResource.GetAsync(cancellationToken).ConfigureAwait(false);
        return SerializeCollaborationData(response.Value.Data);
    }

    public async Task<JsonElement> GetCollaborationReadonlyKubeconfigAsync(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var collaborationResource = await GetCollaborationResourceAsync(
            name, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var response = await collaborationResource
            .GetReadonlyKubeConfigAsync(cancellationToken)
            .ConfigureAwait(false);

        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("kubeconfig", response.Value.Kubeconfig);
            writer.WriteEndObject();
            writer.Flush();
        }

        return JsonSerializer.Deserialize(buffer.WrittenSpan, ManagedCleanroomSerializerContext.Default.JsonElement);
    }

    public async Task<CollaborationCreateResult> CreateCollaborationArmResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string location,
        string? resourceLocation = null,
        string[]? collaborators = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(location), location));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var subscriptionResource = await _subscriptionService
            .GetSubscription(subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var resourceGroupId = ResourceGroupResource.CreateResourceIdentifier(
            subscriptionResource.Id.SubscriptionId!,
            resourceGroup);
        var resourceGroupResource = armClient.GetResourceGroupResource(resourceGroupId);

        // Resolve the calling user's email from the ARM token and always include them as a collaborator.
        var callerEmail = await GetCallerEmailAsync(tenant, cancellationToken).ConfigureAwait(false);

        // Merge caller with any explicitly provided collaborators, deduplicating by email (case-insensitive).
        var allCollaborators = (collaborators ?? [])
            .Append(callerEmail)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var collaborationData = new CollaborationData(new Azure.Core.AzureLocation(location))
        {
            ResourceLocation = new Azure.Core.AzureLocation(resourceLocation ?? location)
        };

        foreach (var collaborator in allCollaborators)
        {
            collaborationData.Collaborators.Add(new Collaborator
            {
                UserIdentifier = collaborator
            });
        }

        // Fire the ARM PUT without blocking — provisioning takes ~25 minutes.
        await resourceGroupResource.GetCollaborations()
            .CreateOrUpdateAsync(
                WaitUntil.Started,
                name,
                collaborationData,
                cancellationToken)
            .ConfigureAwait(false);

        var acceptedResult = BuildAcceptedCreateResult(
            name,
            resourceGroup,
            subscription,
            location,
            resourceLocation,
            allCollaborators);

        const string acceptedMessage = "Collaboration create request accepted. Provisioning typically takes about 25 minutes.";
        return new CollaborationCreateResult(acceptedResult, acceptedMessage);
    }

    private async Task<CollaborationClient> BuildClientAsync(
        string endpoint,
        bool allowUntrustedCert,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint));

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new ArgumentException($"Endpoint '{endpoint}' is not a valid absolute URI.", nameof(endpoint));
        }

        var credential = await GetCredential(tenant, cancellationToken).ConfigureAwait(false);

        var options = new CollaborationClientOptions();
        options.AddPolicy(
            new BearerTokenAuthenticationPolicy(credential, TenantService.CloudConfiguration.ArmEnvironment.DefaultScope),
            HttpPipelinePosition.PerCall);

        var testProxyUrl = Environment.GetEnvironmentVariable("TEST_PROXY_URL");
        if (!string.IsNullOrWhiteSpace(testProxyUrl))
        {
            // Keep proxy transport active in record/playback so requests are captured and replayed.
            options.Transport = new HttpClientTransport(_httpClientFactory.CreateClient());
        }
        else if (allowUntrustedCert)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            options.Transport = new HttpClientTransport(handler);
        }
        else
        {
            options.Transport = new HttpClientTransport(_httpClientFactory.CreateClient());
        }

        return new CollaborationClient(endpointUri, options);
    }

    private static JsonElement ParseResponse(Response response)
    {
        if (response.Content is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize(
            response.Content.ToMemory().Span,
            ManagedCleanroomSerializerContext.Default.JsonElement);
    }

    /// <summary>
    /// Parses the response bytes from an ARM action POST, throwing if the service returned
    /// an error payload despite a 2xx HTTP status (e.g. HTTP 200 with ErrorCode: InternalError).
    /// </summary>
    private static JsonElement ParseArmActionResponse(byte[] responseBytes, int httpStatusCode, string actionName)
    {
        if (responseBytes.Length == 0)
        {
            return default;
        }

        var element = JsonSerializer.Deserialize(responseBytes, ManagedCleanroomSerializerContext.Default.JsonElement);

        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty("error", out var errorProp) &&
            errorProp.TryGetProperty("code", out var codeProp))
        {
            var code = codeProp.GetString() ?? "UnknownError";
            var message = errorProp.TryGetProperty("message", out var msgProp)
                ? msgProp.GetString() ?? string.Empty
                : string.Empty;
            throw new Azure.RequestFailedException(
                httpStatusCode,
                $"ARM action '{actionName}' returned an error: [{code}] {message}",
                code,
                null);
        }

        return element;
    }
}
