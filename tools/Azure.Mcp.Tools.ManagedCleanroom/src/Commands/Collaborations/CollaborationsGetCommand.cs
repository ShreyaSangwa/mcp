// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
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
    Id = "5b8f2a4c-1e7d-4f3b-9a06-2c0d2cfb91a2",
    Name = "get",
    Title = "Get Cleanroom Collaboration",
    Description = "Gets details for a single Azure Cleanroom collaboration by its identifier via the Cleanroom Analytics Frontend service. Returns the full collaboration record from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CollaborationsGetCommand(IManagedCleanroomService service, ILogger<CollaborationsGetCommand> logger)
    : BaseManagedCleanroomCommand<CollaborationsGetOptions>
{
    private readonly IManagedCleanroomService _service = service;
    private readonly ILogger<CollaborationsGetCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ManagedCleanroomOptionDefinitions.CollaborationId);
        command.Options.Add(ManagedCleanroomOptionDefinitions.IncludeDeleted);
        command.Options.Add(ManagedCleanroomOptionDefinitions.AllowUntrustedCert);
    }

    protected override CollaborationsGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.CollaborationId = parseResult.GetValueOrDefault<string>(ManagedCleanroomOptionDefinitions.CollaborationId.Name);
        options.IncludeDeleted = parseResult.GetValueOrDefault<bool?>(ManagedCleanroomOptionDefinitions.IncludeDeleted.Name);
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
            var result = await _service.GetCollaborationAsync(
                options.Endpoint!,
                options.CollaborationId!,
                options.IncludeDeleted,
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
                "Error getting cleanroom collaboration. Endpoint: {Endpoint} CollaborationId: {CollaborationId}",
                options.Endpoint, options.CollaborationId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
