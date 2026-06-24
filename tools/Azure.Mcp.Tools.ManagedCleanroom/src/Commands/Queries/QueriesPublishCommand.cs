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
    Id = "c7e9f1b3-6d48-4c14-f3d1-8e9f0a1b2c34",
    Name = "publish",
    Title = "Publish Cleanroom Query",
    Description = "Publishes a query document for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the updated query record from the service.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class QueriesPublishCommand(ILogger<QueriesPublishCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<QueriesPublishOptions, QueriesPublishCommand.QueriesPublishCommandResult>
{
    private readonly ILogger<QueriesPublishCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, QueriesPublishOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.PublishQueryAsync(
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
                "Error publishing cleanroom query. Endpoint: {Endpoint} CollaborationId: {CollaborationId} DocumentId: {DocumentId}",
                options.Endpoint, options.CollaborationId, options.DocumentId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record QueriesPublishCommandResult;
}


