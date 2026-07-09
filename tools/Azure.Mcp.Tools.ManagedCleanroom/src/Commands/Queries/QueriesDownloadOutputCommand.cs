// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ManagedCleanroom.Options.Queries;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Queries;

[CommandMetadata(
    Id = "f49ef322-0589-4cde-9a4e-3fa22f4be0aa",
    Name = "download-output",
    Title = "Download Cleanroom Query Output",
    Description = "Downloads query output CSV files for a published query into a local directory. The output dataset is resolved from the query document to keep this operation output-specific.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = true)]
public sealed class QueriesDownloadOutputCommand(ILogger<QueriesDownloadOutputCommand> logger, IManagedCleanroomServiceDataPlane service)
    : AuthenticatedCommand<QueriesDownloadOutputOptions, QueriesDownloadOutputCommand.QueriesDownloadOutputCommandResult>
{
    private readonly ILogger<QueriesDownloadOutputCommand> _logger = logger;
    private readonly IManagedCleanroomServiceDataPlane _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, QueriesDownloadOutputOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.DownloadQueryOutputAsync(
                options.Endpoint,
                options.CollaborationId,
                options.DocumentId,
                options.OutputDirectory,
                options.JobId,
                options.CpkEncryptionKeyBase64,
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
                "Error downloading cleanroom query output. Endpoint: {Endpoint} CollaborationId: {CollaborationId} DocumentId: {DocumentId}",
                options.Endpoint, options.CollaborationId, options.DocumentId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        FormatException formatEx => formatEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        FormatException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    public record QueriesDownloadOutputCommandResult;
}
