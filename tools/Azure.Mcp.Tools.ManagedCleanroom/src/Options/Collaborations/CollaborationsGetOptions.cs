// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Collaborations;

public class CollaborationsGetOptions
{
    [Option(Description = "The Azure Cleanroom Analytics Frontend service endpoint URL (e.g., 'https://my-cleanroom.cloudapp.azure.net').")]
    public required string Endpoint { get; set; }

    [Option(Description = "The unique identifier (UUID) of the cleanroom collaboration.")]
    public required string CollaborationId { get; set; }

    [Option(Description = "When true, includes soft-deleted records in the response.")]
    public bool? IncludeDeleted { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool AllowUntrustedCert { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}

