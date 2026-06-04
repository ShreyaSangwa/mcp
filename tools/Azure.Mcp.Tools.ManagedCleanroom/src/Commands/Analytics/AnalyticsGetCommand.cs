// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Tools.ManagedCleanroom.Options;
using Azure.Mcp.Tools.ManagedCleanroom.Options.Analytics;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
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
public sealed class AnalyticsGetCommand(IManagedCleanroomService service, ILogger<AnalyticsGetCommand> logger)
    : BaseManagedCleanroomCommand<AnalyticsGetOptions>
{
    private readonly IManagedCleanroomService _service = service;
    private readonly ILogger<AnalyticsGetCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ManagedCleanroomOptionDefinitions.CollaborationId);
        command.Options.Add(ManagedCleanroomOptionDefinitions.AllowUntrustedCert);
    }

    protected override AnalyticsGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.CollaborationId = parseResult.GetValueOrDefault<string>(ManagedCleanroomOptionDefinitions.CollaborationId.Name);
        options.AllowUntrustedCert = parseResult.GetValueOrDefault<bool>(ManagedCleanroomOptionDefinitions.AllowUntrustedCert.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var result = await _service.GetAnalyticsAsync(
                options.Endpoint!,
                options.CollaborationId!,
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
}
