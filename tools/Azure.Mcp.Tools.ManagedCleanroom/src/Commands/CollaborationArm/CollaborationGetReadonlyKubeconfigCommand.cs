// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedCleanroom.Options.CollaborationArm;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.CollaborationArm;

[CommandMetadata(
    Id = "32edf1c8-3511-48b6-9c1b-29e2af4c45cb",
    Name = "get-readonly-kubeconfig",
    Title = "Get Cleanroom Collaboration Read-only Kubeconfig",
    Description = """
        Gets a read-only kubeconfig for the AKS cluster backing an Azure Cleanroom collaboration ARM resource.
        Returns the kubeconfig YAML payload (as a JSON string) that can be used to inspect the workload cluster
        without granting write permissions.
        Required options:
        - --name: the collaboration ARM resource name
        - --resource-group: resource group containing the collaboration
        - --subscription: Azure subscription
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = true,
    LocalRequired = false)]
public sealed class CollaborationGetReadonlyKubeconfigCommand(
    ILogger<CollaborationGetReadonlyKubeconfigCommand> logger,
    IManagedCleanroomService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CollaborationGetReadonlyKubeconfigOptions, CollaborationGetReadonlyKubeconfigCommand.CollaborationGetReadonlyKubeconfigCommandResult>(subscriptionResolver)
{
    private readonly ILogger<CollaborationGetReadonlyKubeconfigCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, CollaborationGetReadonlyKubeconfigOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetCollaborationReadonlyKubeconfigAsync(
                options.Name,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                result,
                ManagedCleanroomJsonContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting cleanroom collaboration readonly kubeconfig. Name: {Name}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Name, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Collaboration not found. Verify the collaboration name, resource group, and subscription.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed retrieving the readonly kubeconfig. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound => HttpStatusCode.NotFound,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden => HttpStatusCode.Forbidden,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public record CollaborationGetReadonlyKubeconfigCommandResult;
}


