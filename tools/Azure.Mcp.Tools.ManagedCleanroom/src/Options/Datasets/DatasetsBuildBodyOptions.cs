// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Datasets;

public class DatasetsBuildBodyOptions
{
    [Option("The dataset name to include in the dataset body payload.")]
    public required string DatasetName { get; set; }

    [Option("The storage container name that hosts the dataset files.")]
    public required string ContainerName { get; set; }

    [Option("The storage account URL for the dataset store (for example: https://myacct.blob.core.windows.net).")]
    public required string StorageAccountUrl { get; set; }

    [Option("Dataset schema fields in '<fieldName>:<fieldType>' format. Repeat this option for each field.")]
    public required string[] SchemaField { get; set; }

    [Option("Allowed fields for query access. Repeat this option for each allowed field name.")]
    public required string[] AllowedField { get; set; }

    [Option("Dataset format. Defaults to 'csv'.")]
    public string? Format { get; set; }

    [Option("Dataset access mode. Defaults to 'read'.")]
    public string? AccessMode { get; set; }

    [Option("Dataset encryption mode. Defaults to 'SSE'. Values like 'CPK' are normalized to 'CSE' for frontend payload compatibility.")]
    public string? EncryptionMode { get; set; }

    [Option("Storage account type for the dataset store. Defaults to 'Azure_BlobStorage'.")]
    public string? StorageAccountType { get; set; }

    [Option("Optional subdirectory prefix in the container to scope dataset files.")]
    public string? Subdirectory { get; set; }

    [Option("Optional Key Vault URL for CPK/CSE dataset configurations.")]
    public string? CpkKeyVaultUrl { get; set; }

    [Option("Optional key name for CPK/CSE dataset configurations.")]
    public string? CpkKeyName { get; set; }

    [Option("Optional key version for CPK/CSE dataset configurations.")]
    public string? CpkKeyVersion { get; set; }

    [Option("Optional JSON object string with additional store properties to merge into the dataset 'store' object.")]
    public string? AdditionalStoreJson { get; set; }
}
