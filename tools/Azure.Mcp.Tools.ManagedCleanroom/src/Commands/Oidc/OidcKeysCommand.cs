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
    Id = "a7c2e4f6-1b38-4d59-9e0c-2f3a4b5c6d7e",
    Name = "keys",
    Title = "Get Cleanroom OIDC Keys",
    Description = "Gets the OIDC JSON Web Key Set (JWKS) for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the public keys used to verify tokens issued by the collaboration's OIDC issuer.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class OidcKeysCommand(ILogger<OidcKeysCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<OidcKeysOptions, OidcKeysCommand.OidcKeysCommandResult>
{
    private readonly ILogger<OidcKeysCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, OidcKeysOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetOidcKeysAsync(
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
                "Error getting cleanroom OIDC keys. Endpoint: {Endpoint} CollaborationId: {CollaborationId}",
                options.Endpoint, options.CollaborationId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record OidcKeysCommandResult;
}
