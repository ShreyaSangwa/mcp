// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedCleanroom.Options.Collaboration;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaboration;

[CommandMetadata(
    Id = "e247b9e0-2d87-43a7-8e5d-57eea22237a3",
    Name = "create",
    Title = "Create Cleanroom Collaboration",
    Description = """
        Create a new Azure Cleanroom collaboration. The request returns immediately after Azure accepts it; provisioning continues in the background and typically takes about 25 minutes. 
        You'll need to provide: collaboration name, Azure region, resource group, and subscription.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CollaborationCreateCommand(
    ILogger<CollaborationCreateCommand> logger,
    IManagedCleanroomService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CollaborationCreateOptions, CollaborationCreateCommand.CollaborationCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<CollaborationCreateCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, CollaborationCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateCollaborationArmResourceAsync(
                options.Name,
                options.ResourceGroup,
                options.Subscription!,
                options.Location,
                options.ResourceLocation,
                options.Collaborator,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken).ConfigureAwait(false);

            context.Response.Message = result.Message;
            context.Response.Results = ResponseResult.Create(
                result.Properties,
                ManagedCleanroomJsonContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating cleanroom collaboration. Name: {Name}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Name, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A collaboration with this name already exists in the resource group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the collaboration. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource group not found. Verify the resource group exists and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            HttpStatusCode.Conflict,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            HttpStatusCode.Forbidden,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            HttpStatusCode.NotFound,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public record CollaborationCreateCommandResult;
}


