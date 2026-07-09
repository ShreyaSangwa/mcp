// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedCleanroom.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Queries;

public class QueriesVoteOptions
{
    [Option(Description = ManagedCleanroomOptionDescriptions.Endpoint)]
    public required string Endpoint { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.CollaborationId)]
    public required string CollaborationId { get; set; }

    [Option(Description = "The unique identifier (UUID) of the query document to vote on.")]
    public required string DocumentId { get; set; }

    [Option(Description = "JSON request body for query vote. Supports CLI-style @file input to load JSON from disk. Use @@ to send a literal value starting with '@'. If omitted and --vote is provided, a compatibility body is synthesized.")]
    public string? Body { get; set; }

    [Option(Description = "Optional compatibility vote value. Used only when --body is not provided.")]
    public string? Vote { get; set; }

    [Option(Description = "Optional proposal identifier for approval workflows that require proposal scoping. If omitted and --vote is used, the service attempts to resolve proposalId from the query document.")]
    public string? ProposalId { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool AllowUntrustedCert { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
