// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.CollaborationArm;

public class CollaborationAddCollaboratorOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure Cleanroom collaboration ARM resource.")]
    public required string Name { get; set; }

    [Option(Description = "The user identifier of the collaborator to add. For a user, provide an email address. For a service principal, provide the SPN application (client) ID and also specify --collaborator-object-id and --collaborator-tenant-id.")]
    public required string CollaboratorUserIdentifier { get; set; }

    [Option(Description = "The object ID of the service principal collaborator. Required when adding a service principal (along with --collaborator-tenant-id).")]
    public string? CollaboratorObjectId { get; set; }

    [Option(Description = "The tenant ID of the service principal collaborator. Required when adding a service principal (along with --collaborator-object-id).")]
    public string? CollaboratorTenantId { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}