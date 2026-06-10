// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedCleanroom.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Datasets;

public class DatasetsPublishOptions
{
    [Option(ManagedCleanroomOptionDescriptions.Endpoint)]
    public required string Endpoint { get; set; }

    [Option(ManagedCleanroomOptionDescriptions.CollaborationId)]
    public required string CollaborationId { get; set; }

    [Option(ManagedCleanroomOptionDescriptions.DocumentId)]
    public required string DocumentId { get; set; }

    [Option(ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool AllowUntrustedCert { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
