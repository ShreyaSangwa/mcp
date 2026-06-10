// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Options.Consent;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Consent;

[CommandMetadata(
    Id = "a5c7e9b1-4d36-4a92-d1bf-6c7d8e9f0a12",
    Name = "put",
    Title = "Put Cleanroom Consent",
    Description = "Creates or updates a consent document for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the updated consent record from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ConsentPutCommand(ILogger<ConsentPutCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<ConsentPutOptions, ConsentPutCommand.ConsentPutCommandResult>
{
    private readonly ILogger<ConsentPutCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, ConsentPutOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.PutConsentAsync(
                options.Endpoint,
                options.CollaborationId,
                options.DocumentId,
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
                "Error putting cleanroom consent. Endpoint: {Endpoint} CollaborationId: {CollaborationId} DocumentId: {DocumentId}",
                options.Endpoint, options.CollaborationId, options.DocumentId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ConsentPutCommandResult;
}
