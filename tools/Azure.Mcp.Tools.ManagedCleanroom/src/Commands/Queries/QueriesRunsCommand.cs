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
    Id = "c3d5e7f9-2ea4-4c7a-f9d7-4e5f6a7b8c90",
    Name = "runs",
    Title = "Get Cleanroom Query Run History",
    Description = "Gets the run history for a query document in an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the run records from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class QueriesRunsCommand(ILogger<QueriesRunsCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<QueriesRunsOptions, QueriesRunsCommand.QueriesRunsCommandResult>
{
    private readonly ILogger<QueriesRunsCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, QueriesRunsOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetQueryRunsAsync(
                options.Endpoint,
                options.CollaborationId,
                options.DocumentId,
                options.AllowUntrustedCert,
                options.Tenant,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                result,
                ManagedCleanroomJsonContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting cleanroom query run history. Endpoint: {Endpoint} CollaborationId: {CollaborationId} DocumentId: {DocumentId}",
                options.Endpoint, options.CollaborationId, options.DocumentId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record QueriesRunsCommandResult;
}
