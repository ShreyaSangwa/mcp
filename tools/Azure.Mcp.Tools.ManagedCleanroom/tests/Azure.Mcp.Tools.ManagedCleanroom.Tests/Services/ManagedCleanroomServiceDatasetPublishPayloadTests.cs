// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.ManagedCleanroom.Services;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Services;

public sealed class ManagedCleanroomServiceDatasetPublishPayloadTests
{
    [Fact]
    public void NormalizeDatasetPublishPayload_WithBuildBodyWrapper_UsesDatasetObject()
    {
        const string payload = """
            {
              "body": "{\"name\":\"demo\",\"store\":{\"containerName\":\"input\"}}",
              "dataset": {
                "name": "demo",
                "store": {
                  "containerName": "input"
                }
              },
              "normalization": {
                "encryptionMode": "SSE"
              }
            }
            """;

        var normalized = ManagedCleanroomService.NormalizeDatasetPublishPayload(payload);
        using var doc = JsonDocument.Parse(normalized);

        Assert.Equal("demo", doc.RootElement.GetProperty("name").GetString());
        Assert.Equal("input", doc.RootElement.GetProperty("store").GetProperty("containerName").GetString());
        Assert.False(doc.RootElement.TryGetProperty("body", out _));
        Assert.False(doc.RootElement.TryGetProperty("normalization", out _));
    }

    [Fact]
    public void NormalizeDatasetPublishPayload_WithBuildBodyBodyString_UsesNestedBody()
    {
        const string payload = """
            {
              "body": "{\"name\":\"demo\",\"datasetSchema\":{\"format\":\"csv\"}}"
            }
            """;

        var normalized = ManagedCleanroomService.NormalizeDatasetPublishPayload(payload);
        using var doc = JsonDocument.Parse(normalized);

        Assert.Equal("demo", doc.RootElement.GetProperty("name").GetString());
        Assert.Equal("csv", doc.RootElement.GetProperty("datasetSchema").GetProperty("format").GetString());
    }

    [Fact]
    public void NormalizeDatasetPublishPayload_WithWrappedPayload_UnwrapsDatasetDetails()
    {
        const string payload = """
            {
              "datasetDetails": {
                "name": "demo"
              }
            }
            """;

        var normalized = ManagedCleanroomService.NormalizeDatasetPublishPayload(payload);
        using var doc = JsonDocument.Parse(normalized);

        Assert.Equal("demo", doc.RootElement.GetProperty("name").GetString());
        Assert.False(doc.RootElement.TryGetProperty("datasetDetails", out _));
    }

    [Fact]
    public void NormalizeDatasetPublishPayload_WithCommandResponseEnvelope_UsesResultsPayload()
    {
        const string payload = """
            {
              "status": 200,
              "message": "ok",
              "results": {
                "body": "{\"name\":\"demo\",\"datasetSchema\":{\"format\":\"csv\"},\"datasetAccessPolicy\":{\"accessMode\":\"read\",\"allowedFields\":[]},\"store\":{\"containerName\":\"input\"}}",
                "dataset": {
                  "name": "demo",
                  "datasetSchema": { "format": "csv" },
                  "datasetAccessPolicy": { "accessMode": "read", "allowedFields": [] },
                  "store": { "containerName": "input" }
                }
              }
            }
            """;

        var normalized = ManagedCleanroomService.NormalizeDatasetPublishPayload(payload);
        using var doc = JsonDocument.Parse(normalized);

        Assert.Equal("demo", doc.RootElement.GetProperty("name").GetString());
        Assert.Equal("csv", doc.RootElement.GetProperty("datasetSchema").GetProperty("format").GetString());
        Assert.Equal("input", doc.RootElement.GetProperty("store").GetProperty("containerName").GetString());
    }

    [Fact]
    public void NormalizeDatasetPublishPayload_WithMcpCallResultEnvelope_UsesEmbeddedTextPayload()
    {
        const string payload = """
            {
              "content": [
                {
                  "type": "text",
                  "text": "{\"status\":200,\"message\":\"ok\",\"results\":{\"dataset\":{\"name\":\"demo\",\"datasetSchema\":{\"format\":\"csv\"},\"datasetAccessPolicy\":{\"accessMode\":\"read\",\"allowedFields\":[]},\"store\":{\"containerName\":\"input\"}}}}"
                }
              ],
              "isError": false
            }
            """;

        var normalized = ManagedCleanroomService.NormalizeDatasetPublishPayload(payload);
        using var doc = JsonDocument.Parse(normalized);

        Assert.Equal("demo", doc.RootElement.GetProperty("name").GetString());
        Assert.Equal("csv", doc.RootElement.GetProperty("datasetSchema").GetProperty("format").GetString());
        Assert.Equal("input", doc.RootElement.GetProperty("store").GetProperty("containerName").GetString());
    }
}