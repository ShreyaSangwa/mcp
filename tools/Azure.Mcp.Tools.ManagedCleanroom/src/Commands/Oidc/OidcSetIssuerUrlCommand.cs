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
    Id = "c9e1f3a5-7b42-4d60-ae8d-3f4c5b6a7e1f",
    Name = "set-issuer-url",
    Title = "Set Cleanroom OIDC Issuer URL",
    Description = "Registers an OIDC issuer URL for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the updated OIDC issuer record from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class OidcSetIssuerUrlCommand(ILogger<OidcSetIssuerUrlCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<OidcSetIssuerUrlOptions, OidcSetIssuerUrlCommand.OidcSetIssuerUrlCommandResult>
{
    private readonly ILogger<OidcSetIssuerUrlCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, OidcSetIssuerUrlOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.SetOidcIssuerUrlAsync(
                options.Endpoint,
                options.CollaborationId,
                options.IssuerUrl,
                options.AllowUntrustedCert ?? false,
                options.Tenant,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                result,
                ManagedCleanroomJsonContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error setting cleanroom OIDC issuer URL. Endpoint: {Endpoint} CollaborationId: {CollaborationId}",
                options.Endpoint, options.CollaborationId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record OidcSetIssuerUrlCommandResult;
}


