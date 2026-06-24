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
    Id = "a1b3c5d7-0c82-4a58-d7b5-2c3d4e5f6a78",
    Name = "run",
    Title = "Run Cleanroom Query",
    Description = "Runs a query document for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the run initiation result from the service.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class QueriesRunCommand(ILogger<QueriesRunCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<QueriesRunOptions, QueriesRunCommand.QueriesRunCommandResult>
{
    private readonly ILogger<QueriesRunCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, QueriesRunOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RunQueryAsync(
                options.Endpoint,
                options.CollaborationId,
                options.DocumentId,
                options.Body,
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
                "Error running cleanroom query. Endpoint: {Endpoint} CollaborationId: {CollaborationId} DocumentId: {DocumentId}",
                options.Endpoint, options.CollaborationId, options.DocumentId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record QueriesRunCommandResult;
}


