// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Options.Collaborations;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaborations;

[CommandMetadata(
    Id = "5b8f2a4c-1e7d-4f3b-9a06-2c0d2cfb91a2",
    Name = "get",
    Title = "Get Cleanroom Collaboration",
    Description = "Gets details for a single Azure Cleanroom collaboration by its identifier via the Cleanroom Analytics Frontend service. Returns the full collaboration record from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CollaborationsGetCommand(ILogger<CollaborationsGetCommand> logger, IManagedCleanroomServiceDataPlane service)
    : AuthenticatedCommand<CollaborationsGetOptions, CollaborationsGetCommand.CollaborationsGetCommandResult>
{
    private readonly ILogger<CollaborationsGetCommand> _logger = logger;
    private readonly IManagedCleanroomServiceDataPlane _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, CollaborationsGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetCollaborationAsync(
                options.Endpoint,
                options.CollaborationId,
                options.IncludeDeleted,
                options.TokenScope,
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
                "Error getting cleanroom collaboration. Endpoint: {Endpoint} CollaborationId: {CollaborationId}",
                options.Endpoint, options.CollaborationId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record CollaborationsGetCommandResult;
}