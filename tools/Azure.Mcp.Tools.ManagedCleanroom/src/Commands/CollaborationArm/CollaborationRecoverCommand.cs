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
    Id = "1f01c93e-0f26-4949-9c43-693981df8d89",
    Name = "recover",
    Title = "Recover Cleanroom Collaboration",
    Description = """
        Recovers an Azure Cleanroom collaboration ARM resource by invoking the ARM recover action.
        This operation is intended for collaborations in unhealthy or failed states and returns immediately
        after the recover request is accepted.
        Required options:
        - --name: the collaboration ARM resource name
        - --resource-group: resource group containing the collaboration
        - --subscription: Azure subscription
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CollaborationRecoverCommand(
    ILogger<CollaborationRecoverCommand> logger,
    IManagedCleanroomServiceControlPlane service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CollaborationRecoverOptions, CollaborationRecoverCommand.CollaborationRecoverCommandResult>(subscriptionResolver)
{
    private readonly ILogger<CollaborationRecoverCommand> _logger = logger;
    private readonly IManagedCleanroomServiceControlPlane _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, CollaborationRecoverOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RecoverCollaborationArmResourceAsync(
                options.Name,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                result,
                ManagedCleanroomJsonContext.Default.JsonElement);
            context.Response.Message = $"Recover request accepted for cleanroom collaboration '{options.Name}'.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error recovering cleanroom collaboration. Name: {Name}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Name, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Collaboration not found. Verify the collaboration name, resource group, and subscription.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Collaboration cannot be recovered in its current state.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed recovering the collaboration. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound => HttpStatusCode.NotFound,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict => HttpStatusCode.Conflict,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden => HttpStatusCode.Forbidden,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public record CollaborationRecoverCommandResult;
}
