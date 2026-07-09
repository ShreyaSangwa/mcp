// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Datasets;

public class DatasetsBuildBodyOptions
{
    [Option(Description = "The dataset name to include in the dataset body payload.")]
    public required string DatasetName { get; set; }

    [Option(Description = "The storage container name that hosts the dataset files.")]
    public required string ContainerName { get; set; }

    [Option(Description = "The storage account URL for the dataset store (for example: https://myacct.blob.core.windows.net).")]
    public required string StorageAccountUrl { get; set; }

    [Option(Description = "Dataset schema fields in '<fieldName>:<fieldType>' format. Repeat this option for each field.")]
    public required string[] SchemaField { get; set; }

    [Option(Description = "Allowed fields for query access. Repeat this option for each allowed field name.")]
    public required string[] AllowedField { get; set; }

    [Option(Description = "Dataset format. Defaults to 'csv'.")]
    public string? Format { get; set; }

    [Option(Description = "Dataset access mode. Defaults to 'read'.")]
    public string? AccessMode { get; set; }

    [Option(Description = "Dataset encryption mode. Defaults to 'SSE'. Use 'CPK' for customer-provided keys to match the sample dataset payloads; 'CSE' is also accepted for compatibility.")]
    public string? EncryptionMode { get; set; }

    [Option(Description = "Storage account type for the dataset store. Defaults to 'Azure_BlobStorage'.")]
    public string? StorageAccountType { get; set; }

    [Option(Description = "Optional subdirectory prefix in the container to scope dataset files.")]
    public string? Subdirectory { get; set; }

    [Option(Description = "Optional Key Vault URL for CPK/CSE dataset configurations.")]
    public string? CpkKeyVaultUrl { get; set; }

    [Option(Description = "Optional key name for CPK/CSE dataset configurations.")]
    public string? CpkKeyName { get; set; }

    [Option(Description = "Optional key version for CPK/CSE dataset configurations.")]
    public string? CpkKeyVersion { get; set; }

    [Option(Description = "Optional JSON object string with additional store properties to merge into the dataset 'store' object.")]
    public string? AdditionalStoreJson { get; set; }

    [Option(Description = "Optional managed identity name for the dataset identity block.")]
    public string? IdentityName { get; set; }

    [Option(Description = "Optional managed identity clientId for the dataset identity block.")]
    public string? IdentityClientId { get; set; }

    [Option(Description = "Optional managed identity tenantId for the dataset identity block.")]
    public string? IdentityTenantId { get; set; }

    [Option(Description = "Optional OIDC issuer URL for the dataset identity block.")]
    public string? IdentityIssuerUrl { get; set; }

    [Option(Description = "Optional DEK Key Vault URL for CPK/CSE payloads. Requires --dek-secret-id, --kek-key-vault-url, --kek-secret-id, and --maa-url.")]
    public string? DekKeyVaultUrl { get; set; }

    [Option(Description = "Optional DEK secret identifier for CPK/CSE payloads. Requires --dek-key-vault-url, --kek-key-vault-url, --kek-secret-id, and --maa-url.")]
    public string? DekSecretId { get; set; }

    [Option(Description = "Optional KEK Key Vault URL for CPK/CSE payloads. Requires --dek-key-vault-url, --dek-secret-id, --kek-secret-id, and --maa-url.")]
    public string? KekKeyVaultUrl { get; set; }

    [Option(Description = "Optional KEK secret identifier for CPK/CSE payloads. Requires --dek-key-vault-url, --dek-secret-id, --kek-key-vault-url, and --maa-url.")]
    public string? KekSecretId { get; set; }

    [Option(Description = "Optional MAA URL for KEK attestation in CPK/CSE payloads. Requires --dek-key-vault-url, --dek-secret-id, --kek-key-vault-url, and --kek-secret-id.")]
    public string? MaaUrl { get; set; }
}
