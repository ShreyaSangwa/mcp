// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Options.Invitations;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Invitations;

[CommandMetadata(
    Id = "3f7a9b2e-4c51-4d8f-ae12-7d1c3e5f9a04",
    Name = "list",
    Title = "List Cleanroom Invitations",
    Description = "Lists invitations for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns all invitations (or only pending ones when --pending-only is set).",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class InvitationsListCommand(ILogger<InvitationsListCommand> logger, IManagedCleanroomServiceDataPlane service)
    : AuthenticatedCommand<InvitationsListOptions, InvitationsListCommand.InvitationsListCommandResult>
{
    private readonly ILogger<InvitationsListCommand> _logger = logger;
    private readonly IManagedCleanroomServiceDataPlane _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, InvitationsListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ListInvitationsAsync(
                options.Endpoint,
                options.CollaborationId,
                options.PendingOnly,
                options.AllowUntrustedCert ?? false,
                options.Tenant,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                result,
                ManagedCleanroomJsonContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing cleanroom invitations. Endpoint: {Endpoint} CollaborationId: {CollaborationId}",
                options.Endpoint, options.CollaborationId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record InvitationsListCommandResult;
}


