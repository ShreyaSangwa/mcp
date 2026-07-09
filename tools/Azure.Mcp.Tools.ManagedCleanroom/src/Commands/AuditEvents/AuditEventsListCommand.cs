// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Options.AuditEvents;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.AuditEvents;

[CommandMetadata(
    Id = "a1b3c5d7-9e20-4f58-b6c4-2d3e4f5a6b7c",
    Name = "list",
    Title = "List Cleanroom Audit Events",
    Description = "Lists audit events for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Supports optional scope, fromSeqno, and toSeqno filters to narrow the returned events.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AuditEventsListCommand(ILogger<AuditEventsListCommand> logger, IManagedCleanroomServiceDataPlane service)
    : AuthenticatedCommand<AuditEventsListOptions, AuditEventsListCommand.AuditEventsListCommandResult>
{
    private readonly ILogger<AuditEventsListCommand> _logger = logger;
    private readonly IManagedCleanroomServiceDataPlane _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, AuditEventsListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ListAuditEventsAsync(
                options.Endpoint,
                options.CollaborationId,
                options.Scope,
                options.FromSeqno,
                options.ToSeqno,
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
                "Error listing cleanroom audit events. Endpoint: {Endpoint} CollaborationId: {CollaborationId}",
                options.Endpoint, options.CollaborationId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record AuditEventsListCommandResult;
}


