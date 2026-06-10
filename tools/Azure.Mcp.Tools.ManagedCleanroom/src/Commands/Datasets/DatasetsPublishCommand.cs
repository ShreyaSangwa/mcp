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
    Id = "e3f5a7c9-2b14-4e70-bf9d-4a5b6c7d8e0f",
    Name = "publish",
    Title = "Publish Cleanroom Dataset",
    Description = "Publishes a dataset document for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the updated dataset record from the service.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DatasetsPublishCommand(ILogger<DatasetsPublishCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<DatasetsPublishOptions, DatasetsPublishCommand.DatasetsPublishCommandResult>
{
    private readonly ILogger<DatasetsPublishCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, DatasetsPublishOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.PublishDatasetAsync(
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
                "Error publishing cleanroom dataset. Endpoint: {Endpoint} CollaborationId: {CollaborationId} DocumentId: {DocumentId}",
                options.Endpoint, options.CollaborationId, options.DocumentId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record DatasetsPublishCommandResult;
}
