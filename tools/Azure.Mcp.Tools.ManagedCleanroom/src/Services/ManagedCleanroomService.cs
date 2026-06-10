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
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

public class ManagedCleanroomService(ISubscriptionService subscriptionService, ITenantService tenantService, IHttpClientFactory httpClientFactory)
    : BaseAzureResourceService(subscriptionService, tenantService), IManagedCleanroomService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private const string CleanRoomApiVersion = "2026-04-30-preview";
    private const string CleanRoomResourceType = "Microsoft.CleanRoom/Collaborations";
    private static readonly TimeSpan ProvisioningPollInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ProvisioningTimeout = TimeSpan.FromMinutes(40);

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

        var subscriptionResource = await _subscriptionService
            .GetSubscription(subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var token = await GetArmAccessTokenAsync(tenant, cancellationToken).ConfigureAwait(false);

        var managementEndpoint = TenantService.CloudConfiguration.ArmEnvironment.Endpoint.ToString().TrimEnd('/');
        var collaborationId = $"{subscriptionResource.Id}/resourceGroups/{resourceGroup}/providers/{CleanRoomResourceType}/{name}";
        var actionUrl = $"{managementEndpoint}{collaborationId}/addCollaborator?api-version={CleanRoomApiVersion}";

        var bodyBuffer = new ArrayBufferWriter<byte>();
        using (var jsonWriter = new Utf8JsonWriter(bodyBuffer))
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("collaborator");
            jsonWriter.WriteStartObject();
            jsonWriter.WriteString("userIdentifier", collaboratorUserIdentifier);
            if (!string.IsNullOrWhiteSpace(collaboratorObjectId))
            {
                jsonWriter.WriteString("objectId", collaboratorObjectId);
            }
            if (!string.IsNullOrWhiteSpace(collaboratorTenantId))
            {
                jsonWriter.WriteString("tenantId", collaboratorTenantId);
            }
            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
            jsonWriter.Flush();
        }

        var httpClient = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, actionUrl);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
        request.Content = new ByteArrayContent(bodyBuffer.WrittenSpan.ToArray());
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = responseBytes.Length > 0
                ? System.Text.Encoding.UTF8.GetString(responseBytes)
                : $"ARM action 'addCollaborator' failed with status {(int)response.StatusCode}.";
            throw new Azure.RequestFailedException((int)response.StatusCode, errorMessage, null, null);
        }

        return ParseArmActionResponse(responseBytes, (int)response.StatusCode, "addCollaborator");
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

        var subscriptionResource = await _subscriptionService
            .GetSubscription(subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var token = await GetArmAccessTokenAsync(tenant, cancellationToken).ConfigureAwait(false);

        var managementEndpoint = TenantService.CloudConfiguration.ArmEnvironment.Endpoint.ToString().TrimEnd('/');
        var collaborationId = $"{subscriptionResource.Id}/resourceGroups/{resourceGroup}/providers/{CleanRoomResourceType}/{name}";
        var actionUrl = $"{managementEndpoint}{collaborationId}/enableWorkload?api-version={CleanRoomApiVersion}";

        var bodyBuffer = new ArrayBufferWriter<byte>();
        using (var jsonWriter = new Utf8JsonWriter(bodyBuffer))
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WriteString("workloadType", workloadType);
            jsonWriter.WriteEndObject();
            jsonWriter.Flush();
        }

        var httpClient = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, actionUrl);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
        request.Content = new ByteArrayContent(bodyBuffer.WrittenSpan.ToArray());
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = responseBytes.Length > 0
                ? System.Text.Encoding.UTF8.GetString(responseBytes)
                : $"ARM action 'enableWorkload' failed with status {(int)response.StatusCode}.";
            throw new Azure.RequestFailedException((int)response.StatusCode, errorMessage, null, null);
        }

        return ParseArmActionResponse(responseBytes, (int)response.StatusCode, "enableWorkload");
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

        var armClient = await CreateArmClientWithApiVersionAsync(
            CleanRoomResourceType, CleanRoomApiVersion, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var subscriptionResource = await _subscriptionService
            .GetSubscription(subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var resourceId = new ResourceIdentifier(
            $"{subscriptionResource.Id}/resourceGroups/{resourceGroup}/providers/{CleanRoomResourceType}/{name}");

        // Build properties JSON with Utf8JsonWriter for AOT safety and proper escaping.
        var payloadBuffer = new ArrayBufferWriter<byte>();
        using (var jsonWriter = new Utf8JsonWriter(payloadBuffer))
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("collaborators");
            jsonWriter.WriteStartArray();

            foreach (var collaborator in collaborators ?? [])
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("userIdentifier", collaborator);
                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
            jsonWriter.WriteString("resourceLocation", resourceLocation ?? location);
            jsonWriter.WriteEndObject();
            jsonWriter.Flush();
        }

        var resourceData = new GenericResourceData(new Azure.Core.AzureLocation(location))
        {
            Properties = BinaryData.FromBytes(payloadBuffer.WrittenSpan.ToArray())
        };

        // Fire the ARM PUT without blocking — provisioning takes ~25 minutes.
        await armClient.GetGenericResources()
            .CreateOrUpdateAsync(
                Azure.WaitUntil.Started,
                resourceId,
                resourceData,
                cancellationToken)
            .ConfigureAwait(false);

        // Poll provisioningState until terminal state, but stop waiting after timeout.
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var timeoutAt = DateTimeOffset.UtcNow + ProvisioningTimeout;
        var resource = armClient.GetGenericResource(resourceId);
        var provisioningState = "Accepted";
        JsonElement properties = default;

        while (provisioningState is not ("Succeeded" or "Failed" or "Canceled"))
        {
            if (DateTimeOffset.UtcNow >= timeoutAt)
            {
                throw new TimeoutException(
                    $"Timed out waiting for collaboration provisioning to reach a terminal state. Last known state: '{provisioningState}'.");
            }

            await Task.Delay(ProvisioningPollInterval, cancellationToken).ConfigureAwait(false);

            var getResponse = await resource.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var propsBytes = getResponse.Value.Data.Properties?.ToArray() ?? [];

            if (propsBytes.Length > 0)
            {
                properties = JsonSerializer.Deserialize(propsBytes, ManagedCleanroomSerializerContext.Default.JsonElement);
                provisioningState = properties.TryGetProperty("provisioningState", out var ps)
                    ? ps.GetString() ?? "Unknown"
                    : "Unknown";
            }
        }

        stopwatch.Stop();
        var elapsed = stopwatch.Elapsed;
        var message = $"Collaboration provisioning {provisioningState.ToLower()} after " +
            $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s " +
            $"(expected ~25 minutes).";

        return new CollaborationCreateResult(properties, message);
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
