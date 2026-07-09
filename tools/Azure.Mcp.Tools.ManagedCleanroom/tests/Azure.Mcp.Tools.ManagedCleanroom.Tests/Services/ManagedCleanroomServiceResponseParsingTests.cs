// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using NSubstitute;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Services;

public sealed class ManagedCleanroomServiceResponseParsingTests
{
    [Fact]
    public void ParseResponse_WithJsonContent_ReturnsJsonElement()
    {
        var response = Substitute.For<Response>();
        response.Content.Returns(BinaryData.FromString("{\"documentId\":\"123\",\"status\":\"Published\"}"));

        var result = ManagedCleanroomService.ParseResponse(response);

        Assert.Equal(JsonValueKind.Object, result.ValueKind);
        Assert.Equal("123", result.GetProperty("documentId").GetString());
        Assert.Equal("Published", result.GetProperty("status").GetString());
    }

    [Fact]
    public void ParseResponse_WithEmptyContent_ReturnsDefault()
    {
        var response = Substitute.For<Response>();
        response.Content.Returns(BinaryData.FromBytes([]));

        var result = ManagedCleanroomService.ParseResponse(response);

        Assert.Equal(JsonValueKind.String, result.ValueKind);
        Assert.Equal(string.Empty, result.GetString());
    }

    [Fact]
    public void ParseResponse_WithNonJsonContent_ReturnsRawJsonString()
    {
        var response = Substitute.For<Response>();
        response.Content.Returns(BinaryData.FromString("publish completed"));

        var result = ManagedCleanroomService.ParseResponse(response);

        Assert.Equal(JsonValueKind.String, result.ValueKind);
        Assert.Equal("publish completed", result.GetString());
    }
}