// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.CollaborationArm;

public class CollaborationEnableWorkloadOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure Cleanroom collaboration ARM resource.")]
    public required string Name { get; set; }

    [Option(Description = "The type of workload to enable on the collaboration. Allowed values: Analytics, AnalyticsStrict.")]
    public required string WorkloadType { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public required string Subscription { get; set; }

    string? ISubscriptionOption.Subscription { get => Subscription; set => Subscription = value ?? string.Empty; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
