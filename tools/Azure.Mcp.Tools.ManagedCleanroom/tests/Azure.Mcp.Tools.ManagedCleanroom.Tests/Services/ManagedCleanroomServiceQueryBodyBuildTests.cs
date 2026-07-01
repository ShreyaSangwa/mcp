// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using NSubstitute;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Services;

public sealed class ManagedCleanroomServiceQueryBodyBuildTests
{
    private static ManagedCleanroomService CreateService()
    {
        var subscriptionService = Substitute.For<ISubscriptionService>();
        var tenantService = Substitute.For<ITenantService>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        return new ManagedCleanroomService(subscriptionService, tenantService, httpClientFactory);
    }

    [Fact]
    public async Task BuildQueryBodyAsync_WithJsonSegments_ReturnsExpectedPayload()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = CreateService();
        var queryDir = Path.Combine(Path.GetTempPath(), $"mc-query-json-{Guid.NewGuid():N}");
        Directory.CreateDirectory(queryDir);

        try
        {
            var segmentPath = Path.Combine(queryDir, "segment1.json");
                        await File.WriteAllTextAsync(segmentPath,
                                "{\n  \"executionSequence\": 1,\n  \"data\": \"select * from publisher_data\",\n  \"preConditions\": \"\",\n  \"postFilters\": \"\"\n}",
                                cancellationToken);

            var result = await service.BuildQueryBodyAsync(
                "query1-v1",
                queryDir,
                "woodgrove-output-csv-v1",
                "northwind-input-csv-v1",
                "woodgrove-input-csv-v1",
                null,
                null,
                cancellationToken);

            Assert.Equal("json", result.GetProperty("normalization").GetProperty("segmentSource").GetString());
            Assert.Equal(1, result.GetProperty("normalization").GetProperty("segmentCount").GetInt32());
            Assert.Equal("query1-v1", result.GetProperty("recommendedDocumentId").GetString());
            Assert.Contains("northwind-input-csv-v1:publisher_data", result.GetProperty("body").GetString());
        }
        finally
        {
            if (Directory.Exists(queryDir))
            {
                Directory.Delete(queryDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task BuildQueryBodyAsync_WithTxtSegments_ParsesSequenceMetadata()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = CreateService();
        var queryDir = Path.Combine(Path.GetTempPath(), $"mc-query-txt-{Guid.NewGuid():N}");
        Directory.CreateDirectory(queryDir);

        try
        {
            var segmentPath = Path.Combine(queryDir, "segment1.txt");
            await File.WriteAllTextAsync(segmentPath,
                "-- seq=3\nselect * from consumer_data", cancellationToken);

            var result = await service.BuildQueryBodyAsync(
                "query1-v1",
                queryDir,
                "woodgrove-output-csv-v1",
                "northwind-input-csv-v1",
                "woodgrove-input-csv-v1",
                null,
                null,
                cancellationToken);

            var segment = result.GetProperty("query").GetProperty("queryData")[0];
            Assert.Equal(3, segment.GetProperty("executionSequence").GetInt32());
            Assert.Equal("txt", result.GetProperty("normalization").GetProperty("segmentSource").GetString());
        }
        finally
        {
            if (Directory.Exists(queryDir))
            {
                Directory.Delete(queryDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task BuildQueryBodyAsync_WithoutSegments_ThrowsArgumentException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = CreateService();
        var queryDir = Path.Combine(Path.GetTempPath(), $"mc-query-empty-{Guid.NewGuid():N}");
        Directory.CreateDirectory(queryDir);

        try
        {
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.BuildQueryBodyAsync(
                    "query1-v1",
                    queryDir,
                    "woodgrove-output-csv-v1",
                    "northwind-input-csv-v1",
                    "woodgrove-input-csv-v1",
                    null,
                    null,
                    cancellationToken));
        }
        finally
        {
            if (Directory.Exists(queryDir))
            {
                Directory.Delete(queryDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task BuildQueryBodyAsync_WithCustomInputMappings_UsesCustomAliases()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = CreateService();
        var queryDir = Path.Combine(Path.GetTempPath(), $"mc-query-custom-{Guid.NewGuid():N}");
        Directory.CreateDirectory(queryDir);

        try
        {
            var segmentPath = Path.Combine(queryDir, "segment1.json");
            await File.WriteAllTextAsync(segmentPath,
                """{"executionSequence": 1, "data": "select * from view_a", "preConditions": "", "postFilters": ""}""",
                cancellationToken);

            var customMappings = """{"dataset-a":"view_a","dataset-b":"view_b"}""";
            var result = await service.BuildQueryBodyAsync(
                "query1-v1",
                queryDir,
                "output-dataset",
                null,
                null,
                customMappings,
                "result",
                cancellationToken);

            var body = result.GetProperty("body").GetString()!;
            Assert.Contains("dataset-a:view_a", body);
            Assert.Contains("dataset-b:view_b", body);
            Assert.Contains("output-dataset:result", body);
            Assert.Equal("result", result.GetProperty("normalization").GetProperty("outputDatasetAlias").GetString());
        }
        finally
        {
            if (Directory.Exists(queryDir))
            {
                Directory.Delete(queryDir, recursive: true);
            }
        }
    }
}
