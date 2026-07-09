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
    Id = "d5c3a1e7-9f24-4b68-bc35-6e7f8a9b0c1d",
    Name = "get",
    Title = "Get Cleanroom Collaboration",
    Description = """
        Gets the ARM resource details for an Azure Cleanroom collaboration.
        Returns the collaboration's provisioningState, collaborationState, health, and workload endpoints.
        Required options:
        - --name: the collaboration ARM resource name
        - --resource-group: resource group containing the collaboration
        - --subscription: Azure subscription
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CollaborationGetCommand(
    ILogger<CollaborationGetCommand> logger,
    IManagedCleanroomServiceControlPlane service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CollaborationGetOptions, CollaborationGetCommand.CollaborationGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<CollaborationGetCommand> _logger = logger;
    private readonly IManagedCleanroomServiceControlPlane _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, CollaborationGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetCollaborationArmResourceAsync(
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
                "Error getting cleanroom collaboration. Name: {Name}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
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
            $"Authorization failed accessing the collaboration. Details: {reqEx.Message}",
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

    public record CollaborationGetCommandResult;
}


