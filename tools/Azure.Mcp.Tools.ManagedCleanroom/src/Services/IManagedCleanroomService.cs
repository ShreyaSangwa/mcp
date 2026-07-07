// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

public interface IManagedCleanroomService
{
    Task<JsonElement> ListCollaborationsAsync(
        string endpoint,
        bool? activeOnly = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetCollaborationAsync(
        string endpoint,
        string collaborationId,
        bool? includeDeleted = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetAnalyticsAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetAnalyticsSkrPolicyAsync(
        string endpoint,
        string collaborationId,
        string kid,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> ListInvitationsAsync(
        string endpoint,
        string collaborationId,
        bool? pendingOnly = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> AcceptInvitationAsync(
        string endpoint,
        string collaborationId,
        string invitationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetOidcIssuerInfoAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> SetOidcIssuerUrlAsync(
        string endpoint,
        string collaborationId,
        string issuerUrl,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> PublishDatasetAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        string? body = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetDatasetAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> ListDatasetsAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> BuildDatasetBodyAsync(
        string datasetName,
        string containerName,
        string storageAccountUrl,
        string[] schemaFields,
        string[] allowedFields,
        string? format = null,
        string? accessMode = null,
        string? encryptionMode = null,
        string? storageAccountType = null,
        string? subdirectory = null,
        string? cpkKeyVaultUrl = null,
        string? cpkKeyName = null,
        string? cpkKeyVersion = null,
        string? additionalStoreJson = null,
        string? identityName = null,
        string? identityClientId = null,
        string? identityTenantId = null,
        string? identityIssuerUrl = null,
        string? dekKeyVaultUrl = null,
        string? dekSecretId = null,
        string? kekKeyVaultUrl = null,
        string? kekSecretId = null,
        string? maaUrl = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> PublishQueryAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        string? body = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> BuildQueryBodyAsync(
        string queryName,
        string queryDirectory,
        string outputDataset,
        string? publisherInputDataset = null,
        string? consumerInputDataset = null,
        string? inputDatasetMappings = null,
        string? outputDatasetAlias = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetQueryAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> ListQueriesAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> VoteOnQueryAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        string? body = null,
        string? vote = null,
        string? proposalId = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> RunQueryAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        string? body = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetQueryRunsAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetRunStatusAsync(
        string endpoint,
        string collaborationId,
        string jobId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> DownloadQueryOutputAsync(
        string endpoint,
        string collaborationId,
        string queryDocumentId,
        string outputDirectory,
        string? jobId = null,
        string? cpkEncryptionKeyBase64 = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> ListAuditEventsAsync(
        string endpoint,
        string collaborationId,
        string? scope = null,
        string? fromSeqno = null,
        string? toSeqno = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> PutConsentAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        string? body = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetOidcKeysAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> AddCollaboratorAsync(
        string name,
        string resourceGroup,
        string subscription,
        string collaboratorUserIdentifier,
        string? collaboratorObjectId = null,
        string? collaboratorTenantId = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> EnableWorkloadAsync(
        string name,
        string resourceGroup,
        string subscription,
        string workloadType,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<CollaborationCreateResult> CreateCollaborationArmResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string location,
        string? resourceLocation = null,
        string[]? collaborators = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetCollaborationArmResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetCollaborationReadonlyKubeconfigAsync(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> RecoverCollaborationArmResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<JsonElement> DeleteCollaborationArmResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}

/// <summary>Result returned by <see cref="IManagedCleanroomService.CreateCollaborationArmResourceAsync"/>.</summary>
/// <param name="Properties">ARM resource properties as a raw <see cref="System.Text.Json.JsonElement"/>.</param>
/// <param name="Message">Human-readable summary of the provisioning outcome including elapsed time.</param>
public sealed record CollaborationCreateResult(System.Text.Json.JsonElement Properties, string Message);


