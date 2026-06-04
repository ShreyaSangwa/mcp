// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using AnalyticsFrontendAPI;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

public class ManagedCleanroomService(ITenantService tenantService)
    : BaseAzureService(tenantService), IManagedCleanroomService
{
    private const string DefaultScope = "https://management.azure.com/.default";

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

        try
        {
            var tokenResult = await credential.GetTokenAsync(
                new TokenRequestContext(new[] { DefaultScope }, tenantId: tenant),
                cancellationToken).ConfigureAwait(false);
            LogTokenIdentity(tokenResult.Token);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[cleanroom-debug] Token fetch failed: {ex.GetType().Name}: {ex.Message}");
        }

        var options = new CollaborationClientOptions();
        options.AddPolicy(
            new BearerTokenAuthenticationPolicy(credential, DefaultScope),
            HttpPipelinePosition.PerCall);

        if (allowUntrustedCert)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            options.Transport = new HttpClientTransport(handler);
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

    private static void LogTokenIdentity(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return;
            var payload = parts[1];
            var padded = payload + new string('=', (4 - payload.Length % 4) % 4);
            var bytes = Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
            var json = JsonSerializer.Deserialize(bytes, ManagedCleanroomSerializerContext.Default.JsonElement);

            string Get(string n) => json.TryGetProperty(n, out var p) ? p.GetString() ?? "" : "";

            var upn = Get("upn");
            var unique = Get("unique_name");
            var pref = Get("preferred_username");
            var name = Get("name");
            var tid = Get("tid");
            var appid = Get("appid");
            var idtyp = Get("idtyp");

            Console.Error.WriteLine($"[cleanroom-debug] Token identity: upn='{upn}' unique_name='{unique}' preferred_username='{pref}' name='{name}' tid='{tid}' appid='{appid}' idtyp='{idtyp}'");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[cleanroom-debug] Token decode failed: {ex.Message}");
        }
    }
}
