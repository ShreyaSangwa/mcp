// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ManagedCleanroom.Commands;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Datasets;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Datasets;

public sealed class DatasetsBuildBodyCommandTests : CommandUnitTestsBase<DatasetsBuildBodyCommand, IManagedCleanroomService>
{
    private const string DatasetName = "woodgrove-input-csv-v1";
    private const string ContainerName = "woodgrove-input";
    private const string StorageAccountUrl = "https://woodgrovestorage.blob.core.windows.net";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("build-body", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--dataset-name demo --container-name input --storage-account-url https://a.blob.core.windows.net --schema-field id:string --allowed-field id", true)]
    [InlineData("--container-name input --storage-account-url https://a.blob.core.windows.net --schema-field id:string --allowed-field id", false)]
    [InlineData("--dataset-name demo --storage-account-url https://a.blob.core.windows.net --schema-field id:string --allowed-field id", false)]
    [InlineData("--dataset-name demo --container-name input --schema-field id:string --allowed-field id", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.BuildDatasetBodyAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(JsonDocument.Parse("""{"body":"{}","dataset":{}}""").RootElement);
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
            """{"body":"{\"name\":\"woodgrove-input-csv-v1\"}","dataset":{"name":"woodgrove-input-csv-v1"},"normalization":{"encryptionMode":"SSE"}}""").RootElement;

        Service.BuildDatasetBodyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--dataset-name", DatasetName,
            "--container-name", ContainerName,
            "--storage-account-url", StorageAccountUrl,
            "--schema-field", "id:string",
            "--allowed-field", "id");

        Assert.Equal(HttpStatusCode.OK, response.Status);

        var result = ValidateAndDeserializeResponse(response, ManagedCleanroomJsonContext.Default.JsonElement);
        result.AssertProperty("body");
        result.AssertProperty("dataset");
        result.AssertProperty("normalization");

        await Service.Received(1).BuildDatasetBodyAsync(
            DatasetName,
            ContainerName,
            StorageAccountUrl,
            Arg.Is<string[]>(fields => fields.Length == 1 && fields[0] == "id:string"),
            Arg.Is<string[]>(fields => fields.Length == 1 && fields[0] == "id"),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesArgumentErrorsAsBadRequest()
    {
        Service.BuildDatasetBodyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Allowed field 'email' is not present in schemaFields."));

        var response = await ExecuteCommandAsync(
            "--dataset-name", DatasetName,
            "--container-name", ContainerName,
            "--storage-account-url", StorageAccountUrl,
            "--schema-field", "id:string",
            "--allowed-field", "email");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not present in schemaFields", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.BuildDatasetBodyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--dataset-name", DatasetName,
            "--container-name", ContainerName,
            "--storage-account-url", StorageAccountUrl,
            "--schema-field", "id:string",
            "--allowed-field", "id");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
