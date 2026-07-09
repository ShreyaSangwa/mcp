// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedCleanroom.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Queries;

public class QueriesRunOptions
{
    [Option(Description = ManagedCleanroomOptionDescriptions.Endpoint)]
    public required string Endpoint { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.CollaborationId)]
    public required string CollaborationId { get; set; }

    [Option(Description = "The unique identifier (UUID) of the query document to run.")]
    public required string DocumentId { get; set; }

    [Option(Description = "JSON request body for query run. Supports CLI-style @file input to load JSON from disk. Use @@ to send a literal value starting with '@'. If omitted, an empty JSON object '{}' is sent.")]
    public string? Body { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool AllowUntrustedCert { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
