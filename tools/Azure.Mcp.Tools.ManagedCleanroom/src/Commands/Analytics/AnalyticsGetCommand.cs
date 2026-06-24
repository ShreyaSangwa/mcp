// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Options.Analytics;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Analytics;

[CommandMetadata(
    Id = "9c3b1a52-7d8e-4f6c-bc1d-3e2a4b5d6f7a",
    Name = "get",
    Title = "Get Cleanroom Analytics",
    Description = "Gets the analytics workload configuration for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the full analytics record from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AnalyticsGetCommand(ILogger<AnalyticsGetCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<AnalyticsGetOptions, AnalyticsGetCommand.AnalyticsGetCommandResult>
{
    private readonly ILogger<AnalyticsGetCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, AnalyticsGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetAnalyticsAsync(
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
                "Error getting cleanroom analytics. Endpoint: {Endpoint} CollaborationId: {CollaborationId}",
                options.Endpoint, options.CollaborationId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record AnalyticsGetCommandResult;
}



