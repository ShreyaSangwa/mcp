// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ManagedCleanroom.Options.Queries;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Queries;

[CommandMetadata(
    Id = "b8bc9d10-5f80-4cf9-9f0a-bba81e7d2af1",
    Name = "build-body",
    Title = "Build Cleanroom Query Body",
    Description = "Builds a query publish body from segment*.json or segment*.txt files in a query directory. Returns a wrapper containing a compact JSON 'body' string that can be passed directly to queries publish.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = true)]
public sealed class QueriesBuildBodyCommand(ILogger<QueriesBuildBodyCommand> logger, IManagedCleanroomServiceDataPlane service)
    : AuthenticatedCommand<QueriesBuildBodyOptions, QueriesBuildBodyCommand.QueriesBuildBodyCommandResult>
{
    private readonly ILogger<QueriesBuildBodyCommand> _logger = logger;
    private readonly IManagedCleanroomServiceDataPlane _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, QueriesBuildBodyOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.BuildQueryBodyAsync(
                options.QueryName,
                options.QueryDirectory,
                options.OutputDataset,
                options.PublisherInputDataset,
                options.ConsumerInputDataset,
                options.InputDatasetMappings,
                options.OutputDatasetAlias,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                result,
                ManagedCleanroomJsonContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error building cleanroom query body. QueryName: {QueryName}, QueryDirectory: {QueryDirectory}",
                options.QueryName, options.QueryDirectory);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    public record QueriesBuildBodyCommandResult;
}
