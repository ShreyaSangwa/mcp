// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedCleanroom.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Queries;

public class QueriesDownloadOutputOptions
{
    [Option(Description = ManagedCleanroomOptionDescriptions.Endpoint)]
    public required string Endpoint { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.CollaborationId)]
    public required string CollaborationId { get; set; }

    [Option(Description = "The unique identifier (UUID) of the published query document. Output dataset is resolved from this query.")]
    public required string DocumentId { get; set; }

    [Option(Description = "Local directory path where output CSV files will be downloaded.")]
    public required string OutputDirectory { get; set; }

    [Option(Description = "Optional run job ID filter. When provided, only output files associated with this run are downloaded.")]
    public string? JobId { get; set; }

    [Option(Description = "Optional base64-encoded customer-provided key for CPK/CSE output datasets. Required only when output dataset encryption mode is CPK/CSE.")]
    public string? CpkEncryptionKeyBase64 { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool AllowUntrustedCert { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
