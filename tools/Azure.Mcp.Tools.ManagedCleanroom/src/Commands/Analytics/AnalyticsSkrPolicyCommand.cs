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
    Id = "1f4d2e63-8c9a-4b7d-be2e-4f3b5c6d7e8a",
    Name = "skr-policy",
    Title = "Get Cleanroom Analytics SKR Policy",
    Description = "Gets the Secure Key Release (SKR) policy for a given key identifier on an Azure Cleanroom collaboration's analytics workload via the Cleanroom Analytics Frontend service. Returns the full SKR policy record from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AnalyticsSkrPolicyCommand(ILogger<AnalyticsSkrPolicyCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<AnalyticsSkrPolicyOptions, AnalyticsSkrPolicyCommand.AnalyticsSkrPolicyCommandResult>
{
    private readonly ILogger<AnalyticsSkrPolicyCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, AnalyticsSkrPolicyOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetAnalyticsSkrPolicyAsync(
                options.Endpoint,
                options.CollaborationId,
                options.Kid,
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
                "Error getting cleanroom analytics SKR policy. Endpoint: {Endpoint} CollaborationId: {CollaborationId} Kid: {Kid}",
                options.Endpoint, options.CollaborationId, options.Kid);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record AnalyticsSkrPolicyCommandResult;
}



