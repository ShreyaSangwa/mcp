// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Options.Oidc;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
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
public sealed class OidcIssuerInfoCommand(ILogger<OidcIssuerInfoCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<OidcIssuerInfoOptions, OidcIssuerInfoCommand.OidcIssuerInfoCommandResult>
{
    private readonly ILogger<OidcIssuerInfoCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, OidcIssuerInfoOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetOidcIssuerInfoAsync(
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
                "Error getting cleanroom OIDC issuer info. Endpoint: {Endpoint} CollaborationId: {CollaborationId}",
                options.Endpoint, options.CollaborationId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record OidcIssuerInfoCommandResult;
}



