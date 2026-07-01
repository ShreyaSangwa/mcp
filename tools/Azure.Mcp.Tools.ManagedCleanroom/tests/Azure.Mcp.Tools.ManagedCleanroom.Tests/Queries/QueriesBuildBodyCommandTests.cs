// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ManagedCleanroom.Commands;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Queries;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Queries;

public sealed class QueriesBuildBodyCommandTests : CommandUnitTestsBase<QueriesBuildBodyCommand, IManagedCleanroomService>
{
    private const string QueryName = "query1-v1";
    private const string QueryDirectory = "./demos/query/woodgrove/query1";
    private const string PublisherInputDataset = "northwind-input-csv-v1";
    private const string ConsumerInputDataset = "woodgrove-input-csv-v1";
    private const string OutputDataset = "woodgrove-output-csv-v1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("build-body", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--query-name query1-v1 --query-directory ./query --publisher-input-dataset northwind-input-csv-v1 --consumer-input-dataset woodgrove-input-csv-v1 --output-dataset woodgrove-output-csv-v1", true)]
    [InlineData("--query-name query1-v1 --query-directory ./query --input-dataset-mappings '{\"northwind\":\"nw\",\"woodgrove\":\"wg\"}' --output-dataset woodgrove-output-csv-v1", true)]
    [InlineData("--query-directory ./query --publisher-input-dataset northwind-input-csv-v1 --consumer-input-dataset woodgrove-input-csv-v1 --output-dataset woodgrove-output-csv-v1", false)]
    [InlineData("--query-name query1-v1 --publisher-input-dataset northwind-input-csv-v1 --consumer-input-dataset woodgrove-input-csv-v1 --output-dataset woodgrove-output-csv-v1", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.BuildQueryBodyAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(JsonDocument.Parse("""{"body":"{}","query":{},"normalization":{}}""").RootElement);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServiceResponse()
    {
        var expected = JsonDocument.Parse(
            """{"body":"{\"inputDatasets\":\"northwind-input-csv-v1:publisher_data,woodgrove-input-csv-v1:consumer_data\"}","query":{"outputDataset":"woodgrove-output-csv-v1:output"},"recommendedDocumentId":"query1-v1","normalization":{"segmentSource":"json","segmentCount":2}}""").RootElement;

        Service.BuildQueryBodyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--query-name", QueryName,
            "--query-directory", QueryDirectory,
            "--publisher-input-dataset", PublisherInputDataset,
            "--consumer-input-dataset", ConsumerInputDataset,
            "--output-dataset", OutputDataset);

        Assert.Equal(HttpStatusCode.OK, response.Status);

        var result = ValidateAndDeserializeResponse(response, ManagedCleanroomJsonContext.Default.JsonElement);
        result.AssertProperty("body");
        result.AssertProperty("query");
        result.AssertProperty("recommendedDocumentId");
        result.AssertProperty("normalization");

        await Service.Received(1).BuildQueryBodyAsync(
            QueryName,
            QueryDirectory,
            OutputDataset,
            PublisherInputDataset,
            ConsumerInputDataset,
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomInputMappings_PassesCustomMappingsToService()
    {
        var customMappings = @"{""dataset-a"":""view_a"",""dataset-b"":""view_b""}";
        Service.BuildQueryBodyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(JsonDocument.Parse("""{"body":"{}","query":{},"recommendedDocumentId":"query1","normalization":{}}""").RootElement);

        var response = await ExecuteCommandAsync(
            "--query-name", QueryName,
            "--query-directory", QueryDirectory,
            "--input-dataset-mappings", customMappings,
            "--output-dataset", OutputDataset,
            "--output-dataset-alias", "result");

        Assert.Equal(HttpStatusCode.OK, response.Status);

        await Service.Received(1).BuildQueryBodyAsync(
            QueryName,
            QueryDirectory,
            OutputDataset,
            null,
            null,
            customMappings,
            "result",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesArgumentErrorsAsBadRequest()
    {
        Service.BuildQueryBodyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("No segment files found."));

        var response = await ExecuteCommandAsync(
            "--query-name", QueryName,
            "--query-directory", QueryDirectory,
            "--publisher-input-dataset", PublisherInputDataset,
            "--consumer-input-dataset", ConsumerInputDataset,
            "--output-dataset", OutputDataset);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("No segment files found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.BuildQueryBodyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--query-name", QueryName,
            "--query-directory", QueryDirectory,
            "--publisher-input-dataset", PublisherInputDataset,
            "--consumer-input-dataset", ConsumerInputDataset,
            "--output-dataset", OutputDataset);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
