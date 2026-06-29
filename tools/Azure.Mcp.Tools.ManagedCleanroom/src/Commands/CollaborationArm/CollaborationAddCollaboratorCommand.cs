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
    Id = "c4e8f025-6a3d-4b2f-ad7e-8b9c0d1e2f3a",
    Name = "add-collaborator",
    Title = "Add Cleanroom Collaborator",
    Description = """
        Adds a collaborator to an Azure Cleanroom collaboration ARM resource via the Microsoft.CleanRoom ARM API.
        Returns immediately after the add-collaborator request is accepted.
        For a user collaborator, provide --collaborator-user-identifier as an email address.
        For a service principal collaborator, provide --collaborator-user-identifier as the SPN application (client) ID
        and also specify --collaborator-object-id and --collaborator-tenant-id.
        Required options:
        - --name: the collaboration ARM resource name
        - --collaborator-user-identifier: email or SPN client ID of the collaborator to add
        - --resource-group: resource group containing the collaboration
        - --subscription: Azure subscription
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CollaborationAddCollaboratorCommand(
    ILogger<CollaborationAddCollaboratorCommand> logger,
    IManagedCleanroomService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CollaborationAddCollaboratorOptions, CollaborationAddCollaboratorCommand.CollaborationAddCollaboratorCommandResult>(subscriptionResolver)
{
    private readonly ILogger<CollaborationAddCollaboratorCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, CollaborationAddCollaboratorOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.AddCollaboratorAsync(
                options.Name,
                options.ResourceGroup,
                options.Subscription!,
                options.CollaboratorUserIdentifier,
                options.CollaboratorObjectId,
                options.CollaboratorTenantId,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                result,
                ManagedCleanroomJsonContext.Default.JsonElement);
            context.Response.Message = $"Add collaborator request accepted for '{options.CollaboratorUserIdentifier}' on collaboration '{options.Name}'.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error adding collaborator to cleanroom collaboration. Name: {Name}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
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
            $"Authorization failed adding collaborator. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Collaborator is already a member of this collaboration.",
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

    public record CollaborationAddCollaboratorCommandResult;
}


