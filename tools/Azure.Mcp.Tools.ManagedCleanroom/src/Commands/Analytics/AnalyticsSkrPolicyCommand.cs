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
public sealed class AnalyticsSkrPolicyCommand(IManagedCleanroomService service, ILogger<AnalyticsSkrPolicyCommand> logger)
    : BaseManagedCleanroomCommand<AnalyticsSkrPolicyOptions>
{
    private readonly IManagedCleanroomService _service = service;
    private readonly ILogger<AnalyticsSkrPolicyCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ManagedCleanroomOptionDefinitions.CollaborationId);
        command.Options.Add(ManagedCleanroomOptionDefinitions.Kid);
        command.Options.Add(ManagedCleanroomOptionDefinitions.AllowUntrustedCert);
    }

    protected override AnalyticsSkrPolicyOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.CollaborationId = parseResult.GetValueOrDefault<string>(ManagedCleanroomOptionDefinitions.CollaborationId.Name);
        options.Kid = parseResult.GetValueOrDefault<string>(ManagedCleanroomOptionDefinitions.Kid.Name);
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
            var result = await _service.GetAnalyticsSkrPolicyAsync(
                options.Endpoint!,
                options.CollaborationId!,
                options.Kid!,
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
}
