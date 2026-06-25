// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedCleanroom.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Runs;

public class RunsGetOptions
{
    [Option(ManagedCleanroomOptionDescriptions.Endpoint)]
    public required string Endpoint { get; set; }

    [Option(ManagedCleanroomOptionDescriptions.CollaborationId)]
    public required string CollaborationId { get; set; }

    [Option("The unique identifier (UUID) of the query run job to retrieve status for.")]
    public string? JobId { get; set; }

    [Option("Legacy compatibility alias for --job-id. If provided, this value is treated as the run job ID.")]
    public string? DocumentId { get; set; }

    [Option(ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool? AllowUntrustedCert { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
