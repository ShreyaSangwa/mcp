// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Tools.ManagedCleanroom.Commands;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Dashboard;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;

namespace Azure.Mcp.Tools.ManagedCleanroom.Tests.Dashboard;

public sealed class DashboardOpenGrafanaCommandTests : CommandUnitTestsBase<DashboardOpenGrafanaCommand, IManagedCleanroomServiceDataPlane>
{
    private const string TestEndpoint = "https://my-cleanroom.cloudapp.azure.net";
    private const string TestCollaborationId = "9d8fa4d3-2808-4067-9c20-db26e2a9ec2f";
    private const string TestKubeconfigPath = "/tmp/kubeconfig.yaml";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("open-grafana", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net --collaboration-id 9d8fa4d3-2808-4067-9c20-db26e2a9ec2f --kubeconfig-path /tmp/kubeconfig.yaml", true)]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net --collaboration-id 9d8fa4d3-2808-4067-9c20-db26e2a9ec2f --kubeconfig-path /tmp/kubeconfig.yaml --local-port 3001", true)]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net --collaboration-id 9d8fa4d3-2808-4067-9c20-db26e2a9ec2f", false)]
    [InlineData("--endpoint https://my-cleanroom.cloudapp.azure.net", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        var response = await ExecuteCommandAsync(args);

        // Command will fail because kubeconfig file doesn't exist, but we're testing argument validation
        if (!shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Parse_BindsLocalPortOptionCorrectly()
    {
        var args = $"--endpoint {TestEndpoint} --collaboration-id {TestCollaborationId} --kubeconfig-path {TestKubeconfigPath} --local-port 3001";
        var parseResult = CommandDefinition.Parse(args);
        var localPortOption = CommandDefinition.Options
            .OfType<Option<int>>()
            .Single(option => option.Name == "--local-port");
        var localPort = parseResult.GetValue(localPortOption);

        Assert.Equal(3001, localPort);
    }

    [Fact]
    public void Options_DefaultsLocalPortTo3000()
    {
        var options = new Azure.Mcp.Tools.ManagedCleanroom.Options.Dashboard.DashboardOpenGrafanaOptions
        {
            Endpoint = TestEndpoint,
            CollaborationId = TestCollaborationId,
            KubeconfigPath = TestKubeconfigPath,
        };

        Assert.Equal(3000, options.LocalPort);
    }

    [Fact]
    public async Task ExecuteAsync_FailsWhenKubeconfigPathDoesNotExist()
    {
        var args = $"--endpoint {TestEndpoint} --collaboration-id {TestCollaborationId} --kubeconfig-path /nonexistent/path.yaml";
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Kubeconfig file not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
