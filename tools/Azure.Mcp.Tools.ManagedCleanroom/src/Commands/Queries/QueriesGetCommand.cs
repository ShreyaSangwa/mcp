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
    Id = "d8f0a2b4-7e59-4d25-a4e2-9f0a1b2c3d45",
    Name = "get",
    Title = "Get Cleanroom Query",
    Description = "Gets a query document for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the full query record from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class QueriesGetCommand(ILogger<QueriesGetCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<QueriesGetOptions, QueriesGetCommand.QueriesGetCommandResult>
{
    private readonly ILogger<QueriesGetCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, QueriesGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetQueryAsync(
                options.Endpoint,
                options.CollaborationId,
                options.DocumentId,
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
                "Error getting cleanroom query. Endpoint: {Endpoint} CollaborationId: {CollaborationId} DocumentId: {DocumentId}",
                options.Endpoint, options.CollaborationId, options.DocumentId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record QueriesGetCommandResult;
}


