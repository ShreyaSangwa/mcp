// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Options.Datasets;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Datasets;

[CommandMetadata(
    Id = "b6d8f0a2-5e47-4b03-e2c0-7d8e9f0a1b23",
    Name = "list",
    Title = "List Cleanroom Datasets",
    Description = "Lists dataset documents for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns all dataset records from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DatasetsListCommand(ILogger<DatasetsListCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<DatasetsListOptions, DatasetsListCommand.DatasetsListCommandResult>
{
    private readonly ILogger<DatasetsListCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, DatasetsListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ListDatasetsAsync(
                options.Endpoint,
                options.CollaborationId,
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
                "Error listing cleanroom datasets. Endpoint: {Endpoint} CollaborationId: {CollaborationId}",
                options.Endpoint, options.CollaborationId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record DatasetsListCommandResult;
}
