// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Options.Queries;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Queries;

[CommandMetadata(
    Id = "e9f1b3c5-8a60-4e36-b5f3-0a1b2c3d4e56",
    Name = "list",
    Title = "List Cleanroom Queries",
    Description = "Lists query documents for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns all query records from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class QueriesListCommand(ILogger<QueriesListCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<QueriesListOptions, QueriesListCommand.QueriesListCommandResult>
{
    private readonly ILogger<QueriesListCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, QueriesListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ListQueriesAsync(
                options.Endpoint,
                options.CollaborationId,
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
                "Error listing cleanroom queries. Endpoint: {Endpoint} CollaborationId: {CollaborationId}",
                options.Endpoint, options.CollaborationId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record QueriesListCommandResult;
}


