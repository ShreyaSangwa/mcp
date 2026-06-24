// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Options.Queries;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Queries;

[CommandMetadata(
    Id = "f0a2b4c6-9b71-4f47-c6a4-1b2c3d4e5f67",
    Name = "vote",
    Title = "Vote on Cleanroom Query",
    Description = "Casts a vote (Approve or Reject) on a query document for an Azure Cleanroom collaboration via the Cleanroom Analytics Frontend service. Returns the updated query vote record from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class QueriesVoteCommand(ILogger<QueriesVoteCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<QueriesVoteOptions, QueriesVoteCommand.QueriesVoteCommandResult>
{
    private readonly ILogger<QueriesVoteCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, QueriesVoteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(options.Body) && string.IsNullOrWhiteSpace(options.Vote))
            {
                throw new ArgumentException("Either --body or --vote must be provided for query vote.");
            }

            var result = await _service.VoteOnQueryAsync(
                options.Endpoint,
                options.CollaborationId,
                options.DocumentId,
                options.Body,
                options.Vote,
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
                "Error voting on cleanroom query. Endpoint: {Endpoint} CollaborationId: {CollaborationId} DocumentId: {DocumentId}",
                options.Endpoint, options.CollaborationId, options.DocumentId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record QueriesVoteCommandResult;
}


