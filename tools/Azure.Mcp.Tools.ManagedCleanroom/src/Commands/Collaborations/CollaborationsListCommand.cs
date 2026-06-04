// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Tools.ManagedCleanroom.Options;
using Azure.Mcp.Tools.ManagedCleanroom.Options.Collaborations;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaborations;

[CommandMetadata(
    Id = "0d6a0a0e-7a3a-4a7c-8e3f-2c0d2cfb91a1",
    Name = "list",
    Title = "List Cleanroom Collaborations",
    Description = "Lists Azure Cleanroom collaborations the calling user participates in via the Cleanroom Analytics Frontend service. Returns the full collaboration details from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CollaborationsListCommand(IManagedCleanroomService service, ILogger<CollaborationsListCommand> logger)
    : BaseManagedCleanroomCommand<CollaborationsListOptions>
{
    private readonly IManagedCleanroomService _service = service;
    private readonly ILogger<CollaborationsListCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ManagedCleanroomOptionDefinitions.ActiveOnly);
        command.Options.Add(ManagedCleanroomOptionDefinitions.AllowUntrustedCert);
    }

    protected override CollaborationsListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ActiveOnly = parseResult.GetValueOrDefault<bool?>(ManagedCleanroomOptionDefinitions.ActiveOnly.Name);
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
            var result = await _service.ListCollaborationsAsync(
                options.Endpoint!,
                options.ActiveOnly,
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
                "Error listing cleanroom collaborations. Endpoint: {Endpoint} ActiveOnly: {ActiveOnly}",
                options.Endpoint, options.ActiveOnly);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
