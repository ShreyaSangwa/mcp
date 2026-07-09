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
    Id = "b2d4e6f8-3a15-4c90-8e7b-1f2d3e4c5a6b",
    Name = "accept",
    Title = "Accept Cleanroom Invitation",
    Description = "Accepts an invitation to join an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the updated invitation record from the service.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class InvitationsAcceptCommand(ILogger<InvitationsAcceptCommand> logger, IManagedCleanroomServiceDataPlane service)
    : AuthenticatedCommand<InvitationsAcceptOptions, InvitationsAcceptCommand.InvitationsAcceptCommandResult>
{
    private readonly ILogger<InvitationsAcceptCommand> _logger = logger;
    private readonly IManagedCleanroomServiceDataPlane _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, InvitationsAcceptOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.AcceptInvitationAsync(
                options.Endpoint,
                options.CollaborationId,
                options.InvitationId,
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
                "Error accepting cleanroom invitation. Endpoint: {Endpoint} CollaborationId: {CollaborationId} InvitationId: {InvitationId}",
                options.Endpoint, options.CollaborationId, options.InvitationId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record InvitationsAcceptCommandResult;
}


