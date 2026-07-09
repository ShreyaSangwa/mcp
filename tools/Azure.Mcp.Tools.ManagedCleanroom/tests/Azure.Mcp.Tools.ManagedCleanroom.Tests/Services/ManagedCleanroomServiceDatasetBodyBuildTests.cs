// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using NSubstitute;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Services;

public sealed class ManagedCleanroomServiceDatasetBodyBuildTests
{
    private static ManagedCleanroomService CreateService()
    {
        var subscriptionService = Substitute.For<ISubscriptionService>();
        var tenantService = Substitute.For<ITenantService>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        return new ManagedCleanroomService(subscriptionService, tenantService, httpClientFactory);
    }

    [Fact]
    public async Task BuildDatasetBodyAsync_WithSampleStyleCpkInputs_ReturnsSampleCompatiblePayload()
    {
        var service = CreateService();
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await service.BuildDatasetBodyAsync(
            datasetName: "woodgrove-input-csv-v1",
            containerName: "woodgrove-input",
            storageAccountUrl: "https://woodgrovestorage.blob.core.windows.net",
            schemaFields: ["user_id:string", "hashed_email:string", "purchase_history:string"],
            allowedFields: ["user_id", "hashed_email", "purchase_history"],
            accessMode: "read",
            encryptionMode: "CPK",
            subdirectory: "2025-09-01",
            identityName: "woodgrove-mi",
            identityClientId: "00000000-0000-0000-0000-000000000001",
            identityTenantId: "00000000-0000-0000-0000-000000000002",
            identityIssuerUrl: "https://issuer.example.com",
            dekKeyVaultUrl: "https://woodgrove-kv.vault.azure.net/",
            dekSecretId: "woodgrove-dek",
            kekKeyVaultUrl: "https://woodgrove-kv.vault.azure.net/",
            kekSecretId: "woodgrove-kek",
            maaUrl: "https://sharedeus.eus.attest.azure.net",
            cancellationToken: cancellationToken);

        var dataset = result.GetProperty("dataset");
        Assert.Equal("woodgrove-input-csv-v1", dataset.GetProperty("name").GetString());
        Assert.Equal("CPK", dataset.GetProperty("store").GetProperty("encryptionMode").GetString());
        Assert.Equal("2025-09-01", dataset.GetProperty("store").GetProperty("subdirectory").GetString());
        Assert.Equal("woodgrove-mi", dataset.GetProperty("identity").GetProperty("name").GetString());
        Assert.Equal("woodgrove-dek", dataset.GetProperty("dek").GetProperty("secretId").GetString());
        Assert.Equal("woodgrove-kek", dataset.GetProperty("kek").GetProperty("secretId").GetString());

        var bodyJson = result.GetProperty("body").GetString();
        Assert.False(string.IsNullOrWhiteSpace(bodyJson));

        using var bodyDoc = JsonDocument.Parse(bodyJson!);
        Assert.Equal("CPK", bodyDoc.RootElement.GetProperty("store").GetProperty("encryptionMode").GetString());
        Assert.Equal("2025-09-01", bodyDoc.RootElement.GetProperty("store").GetProperty("subdirectory").GetString());

        var normalization = result.GetProperty("normalization");
        Assert.Equal("CPK", normalization.GetProperty("encryptionMode").GetString());
        Assert.Equal("read", normalization.GetProperty("accessMode").GetString());
    }

    [Fact]
    public async Task BuildDatasetBodyAsync_OutputCanBeChainedDirectlyIntoPublishNormalization()
    {
        var service = CreateService();
        var cancellationToken = TestContext.Current.CancellationToken;

        var built = await service.BuildDatasetBodyAsync(
            datasetName: "demo-dataset",
            containerName: "input",
            storageAccountUrl: "https://demo.blob.core.windows.net",
            schemaFields: ["id:string"],
            allowedFields: ["id"],
            cancellationToken: cancellationToken);

        var wrapperJson = built.GetRawText();
        var bodyJson = built.GetProperty("body").GetString();

        Assert.False(string.IsNullOrWhiteSpace(bodyJson));

        var normalizedFromWrapper = ManagedCleanroomService.NormalizeDatasetPublishPayload(wrapperJson);
        var normalizedFromBody = ManagedCleanroomService.NormalizeDatasetPublishPayload(bodyJson!);

        using var wrapperDoc = JsonDocument.Parse(normalizedFromWrapper);
        using var bodyDoc = JsonDocument.Parse(normalizedFromBody);

        Assert.Equal("demo-dataset", wrapperDoc.RootElement.GetProperty("name").GetString());
        Assert.Equal("input", wrapperDoc.RootElement.GetProperty("store").GetProperty("containerName").GetString());
        Assert.Equal("demo-dataset", bodyDoc.RootElement.GetProperty("name").GetString());
        Assert.Equal("input", bodyDoc.RootElement.GetProperty("store").GetProperty("containerName").GetString());
    }
}