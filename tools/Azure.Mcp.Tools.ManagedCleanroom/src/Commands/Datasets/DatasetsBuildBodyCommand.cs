// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ManagedCleanroom.Options.Datasets;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Datasets;

[CommandMetadata(
    Id = "33cce6e4-b8fb-4f70-b1af-d25227cd1ca1",
    Name = "build-body",
    Title = "Build Cleanroom Dataset Body",
    Description = "Builds and validates a dataset publish body for Azure Cleanroom. Returns a wrapper with 'body' (compact JSON string), 'dataset' (object), and normalization metadata. Datasets publish accepts the returned body value, the dataset object, or the full wrapper JSON directly.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DatasetsBuildBodyCommand(ILogger<DatasetsBuildBodyCommand> logger, IManagedCleanroomService service)
    : AuthenticatedCommand<DatasetsBuildBodyOptions, DatasetsBuildBodyCommand.DatasetsBuildBodyCommandResult>
{
    private readonly ILogger<DatasetsBuildBodyCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, DatasetsBuildBodyOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.BuildDatasetBodyAsync(
                options.DatasetName,
                options.ContainerName,
                options.StorageAccountUrl,
                options.SchemaField,
                options.AllowedField,
                options.Format,
                options.AccessMode,
                options.EncryptionMode,
                options.StorageAccountType,
                options.Subdirectory,
                options.CpkKeyVaultUrl,
                options.CpkKeyName,
                options.CpkKeyVersion,
                options.AdditionalStoreJson,
                options.IdentityName,
                options.IdentityClientId,
                options.IdentityTenantId,
                options.IdentityIssuerUrl,
                options.DekKeyVaultUrl,
                options.DekSecretId,
                options.KekKeyVaultUrl,
                options.KekSecretId,
                options.MaaUrl,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                result,
                ManagedCleanroomJsonContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error building cleanroom dataset body. DatasetName: {DatasetName}, ContainerName: {ContainerName}",
                options.DatasetName, options.ContainerName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    public record DatasetsBuildBodyCommandResult;
}
