// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedCleanroom.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Datasets;

public class DatasetsPublishOptions
{
    [Option(Description = ManagedCleanroomOptionDescriptions.Endpoint)]
    public required string Endpoint { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.CollaborationId)]
    public required string CollaborationId { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.DocumentId)]
    public required string DocumentId { get; set; }

    [Option(Description = "JSON request body for dataset publish. Supports CLI-style @file input to load JSON from disk. Use @@ to send a literal value starting with '@'. If omitted, an empty JSON object '{}' is sent. You can provide a flat dataset object, a wrapped object with 'datasetDetails', or the full wrapper returned by datasets build-body; publish unwraps and normalizes these automatically before sending. The effective dataset payload must include: name, store, datasetSchema, and datasetAccessPolicy. If identity is required by your workflow/environment, include identity.name, identity.clientId, identity.tenantId, and identity.issuerUrl together. Note: datasetSchema.fields[].fieldType must match service DataFieldType values; values like 'double' may be rejected by the service.")]
    public string? Body { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool AllowUntrustedCert { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
