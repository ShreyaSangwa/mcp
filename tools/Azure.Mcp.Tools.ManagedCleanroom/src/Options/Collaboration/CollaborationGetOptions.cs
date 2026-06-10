// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Collaboration;

public class CollaborationGetOptions : ISubscriptionOption
{
    [Option("The name of the Azure Cleanroom collaboration ARM resource to retrieve.")]
    public required string Name { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
