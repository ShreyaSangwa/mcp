// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Analytics;

public class AnalyticsSkrPolicyOptions
{
    [Option("The Azure Cleanroom Analytics Frontend service endpoint URL (e.g., 'https://my-cleanroom.cloudapp.azure.net').")]
    public required string Endpoint { get; set; }

    [Option("The unique identifier (UUID) of the cleanroom collaboration.")]
    public required string CollaborationId { get; set; }

    [Option("The key identifier (kid) of the SKR (Secure Key Release) policy to retrieve.")]
    public required string Kid { get; set; }

    [Option(ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool AllowUntrustedCert { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}

