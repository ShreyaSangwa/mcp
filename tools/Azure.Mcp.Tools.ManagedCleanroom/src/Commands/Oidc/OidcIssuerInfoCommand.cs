// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Tools.ManagedCleanroom.Options;
using Azure.Mcp.Tools.ManagedCleanroom.Options.Oidc;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Oidc;

[CommandMetadata(
    Id = "2e5f3b74-9d1a-4c8e-cf3f-5a4b6c7d8e9b",
    Name = "issuer-info",
    Title = "Get Cleanroom OIDC Issuer Info",
    Description = "Gets the OIDC issuer information for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the full OIDC issuer record from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class OidcIssuerInfoCommand(IManagedCleanroomService service, ILogger<OidcIssuerInfoCommand> logger)
    : BaseManagedCleanroomCommand<OidcIssuerInfoOptions>
{
    private readonly IManagedCleanroomService _service = service;
    private readonly ILogger<OidcIssuerInfoCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ManagedCleanroomOptionDefinitions.CollaborationId);
        command.Options.Add(ManagedCleanroomOptionDefinitions.AllowUntrustedCert);
    }

    protected override OidcIssuerInfoOptions BindOptions(ParseResult parseResult)
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
            var result = await _service.GetOidcIssuerInfoAsync(
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
                "Error getting cleanroom OIDC issuer info. Endpoint: {Endpoint} CollaborationId: {CollaborationId}",
                options.Endpoint, options.CollaborationId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
