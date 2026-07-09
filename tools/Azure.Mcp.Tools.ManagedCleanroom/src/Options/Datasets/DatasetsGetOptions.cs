// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedCleanroom.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Datasets;

public class DatasetsGetOptions
{
    [Option(Description = ManagedCleanroomOptionDescriptions.Endpoint)]
    public required string Endpoint { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.CollaborationId)]
    public required string CollaborationId { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.DocumentId)]
    public required string DocumentId { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool? AllowUntrustedCert { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
