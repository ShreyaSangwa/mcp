// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Options.Runs;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Runs;

[CommandMetadata(
    Id = "b2c4d6e8-1d93-4b69-e8c6-3d4e5f6a7b89",
    Name = "get",
    Title = "Get Cleanroom Query Runs",
    Description = "Gets the run history for a query document in an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the run records from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RunsGetCommand(ILogger<RunsGetCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<RunsGetOptions, RunsGetCommand.RunsGetCommandResult>
{
    private readonly ILogger<RunsGetCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, RunsGetOptions options, CancellationToken cancellationToken)
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
                "Error getting cleanroom query runs. Endpoint: {Endpoint} CollaborationId: {CollaborationId} DocumentId: {DocumentId}",
                options.Endpoint, options.CollaborationId, options.DocumentId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record RunsGetCommandResult;
}
