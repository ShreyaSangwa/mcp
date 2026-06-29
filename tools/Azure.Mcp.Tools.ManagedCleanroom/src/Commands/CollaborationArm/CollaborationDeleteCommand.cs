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
    Id = "b02b8f2f-5ef3-4122-a821-f4b4049554ef",
    Name = "delete",
    Title = "Delete Cleanroom Collaboration",
    Description = """
        Deletes an Azure Cleanroom collaboration ARM resource.
        This operation is irreversible. The request returns immediately after Azure accepts it,
        and deletion continues in the background.
        Required options:
        - --name: the collaboration ARM resource name
        - --resource-group: resource group containing the collaboration
        - --subscription: Azure subscription
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CollaborationDeleteCommand(
    ILogger<CollaborationDeleteCommand> logger,
    IManagedCleanroomService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CollaborationDeleteOptions, CollaborationDeleteCommand.CollaborationDeleteCommandResult>(subscriptionResolver)
{
    private readonly ILogger<CollaborationDeleteCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, CollaborationDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.DeleteCollaborationArmResourceAsync(
                options.Name,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                result,
                ManagedCleanroomJsonContext.Default.JsonElement);
            context.Response.Message = $"Delete request accepted for cleanroom collaboration '{options.Name}'.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting cleanroom collaboration. Name: {Name}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
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
            $"Authorization failed deleting the collaboration. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Collaboration cannot be deleted in its current state.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound => HttpStatusCode.NotFound,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden => HttpStatusCode.Forbidden,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict => HttpStatusCode.Conflict,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public record CollaborationDeleteCommandResult;
}
