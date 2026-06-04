// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using AnalyticsFrontendAPI;
using Azure;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ManagedCleanroom.Models;

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

public class ManagedCleanroomService(ITenantService tenantService)
    : BaseAzureService(tenantService), IManagedCleanroomService
{
    public async Task<List<Collaboration>> ListCollaborationsAsync(
        string endpoint,
        bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint));

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new ArgumentException($"Endpoint '{endpoint}' is not a valid absolute URI.", nameof(endpoint));
        }

        var client = new CollaborationClient(endpointUri);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.GetGetsAsync(activeOnly, requestContext).ConfigureAwait(false);

        var collaborations = new List<Collaboration>();
        if (response.Content is null)
        {
            return collaborations;
        }

        using var document = JsonDocument.Parse(response.Content.ToMemory());
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return collaborations;
        }

        foreach (var element in document.RootElement.EnumerateArray())
        {
            collaborations.Add(new Collaboration
            {
                CollaborationId = ReadString(element, "collaborationId"),
                CollaborationName = ReadString(element, "collaborationName"),
                UserStatus = ReadString(element, "userStatus"),
            });
        }

        return collaborations;
    }

    private static string? ReadString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
