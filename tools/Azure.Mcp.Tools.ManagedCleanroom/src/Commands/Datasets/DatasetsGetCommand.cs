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
    Id = "f4a6b8d0-3c25-4f81-c0ae-5b6c7d8e9f10",
    Name = "get",
    Title = "Get Cleanroom Dataset",
    Description = "Gets a dataset document for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the full dataset record from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DatasetsGetCommand(ILogger<DatasetsGetCommand> logger, IManagedCleanroomServiceDataPlane service)
    : AuthenticatedCommand<DatasetsGetOptions, DatasetsGetCommand.DatasetsGetCommandResult>
{
    private readonly ILogger<DatasetsGetCommand> _logger = logger;
    private readonly IManagedCleanroomServiceDataPlane _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, DatasetsGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetDatasetAsync(
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
                "Error getting cleanroom dataset. Endpoint: {Endpoint} CollaborationId: {CollaborationId} DocumentId: {DocumentId}",
                options.Endpoint, options.CollaborationId, options.DocumentId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record DatasetsGetCommandResult;
}


