// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ManagedCleanroom.Options.Dashboard;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Dashboard;

[CommandMetadata(
    Id = "a1b2c3d4-e5f6-47g8-h9i0-j1k2l3m4n5o6",
    Name = "open-grafana",
    Title = "Open Cleanroom Grafana Dashboard",
    Description = """
        Opens the Grafana dashboard for an Azure Cleanroom collaboration by retrieving admin credentials
        from the AKS cluster and providing port-forwarding instructions. Returns the Grafana admin username,
        password, and the local URL to access the dashboard. Note: This command requires kubectl to be
        installed and accessible on the system PATH.
        Required options:
        - --endpoint: the Analytics Frontend API endpoint URL
        - --collaboration-id: the collaboration ID (UUID)
        - --kubeconfig-path: path to the kubeconfig file for accessing the AKS cluster
        Optional options:
        - --local-port: local port for port-forwarding (default: 3000)
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = true,
    LocalRequired = true)]
public sealed class DashboardOpenGrafanaCommand(ILogger<DashboardOpenGrafanaCommand> logger, IManagedCleanroomServiceDataPlane service)
    : AuthenticatedCommand<DashboardOpenGrafanaOptions, DashboardOpenGrafanaCommand.DashboardOpenGrafanaCommandResult>
{
    private readonly ILogger<DashboardOpenGrafanaCommand> _logger = logger;
    private readonly IManagedCleanroomServiceDataPlane _service = service;

    private const string GrafanaSecretName = "cleanroom-spark-grafana";
    private const string GrafanaNamespace = "telemetry";
    private const string GrafanaService = "cleanroom-spark-grafana";

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, DashboardOpenGrafanaOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Validate kubeconfig file exists
            if (!File.Exists(options.KubeconfigPath))
            {
                throw new FileNotFoundException($"Kubeconfig file not found: {options.KubeconfigPath}");
            }

            // Retrieve Grafana admin credentials using kubectl
            var (adminPassword, error) = await GetGrafanaPasswordAsync(options.KubeconfigPath, cancellationToken);

            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException($"Failed to retrieve Grafana credentials: {error}");
            }

            var grafanaUrl = $"http://localhost:{options.LocalPort}";
            var portForwardCommand = BuildPortForwardCommand(options.KubeconfigPath, options.LocalPort);

            var result = new DashboardOpenGrafanaCommandResult(
                GrafanaUrl: grafanaUrl,
                AdminUsername: "admin",
                AdminPassword: adminPassword,
                PortForwardCommand: portForwardCommand,
                Instructions: BuildInstructions(grafanaUrl, adminPassword));

            context.Response.Results = ResponseResult.Create(
                result,
                ManagedCleanroomJsonContext.Default.DashboardOpenGrafanaCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error opening cleanroom Grafana dashboard. Endpoint: {Endpoint} CollaborationId: {CollaborationId}",
                options.Endpoint, options.CollaborationId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private async Task<(string password, string error)> GetGrafanaPasswordAsync(string kubeconfigPath, CancellationToken cancellationToken)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "kubectl",
                Arguments = $"--kubeconfig \"{kubeconfigPath}\" get secret {GrafanaSecretName} -n {GrafanaNamespace} -o jsonpath=\"{{.data.admin-password}}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processInfo })
            {
                process.Start();

                var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
                var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

                await Task.WhenAll(outputTask, errorTask).ConfigureAwait(false);
                await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

                if (process.ExitCode != 0)
                {
                    return (string.Empty, errorTask.Result ?? $"kubectl exited with code {process.ExitCode}");
                }

                var encodedPassword = outputTask.Result.Trim().Trim('"');
                if (string.IsNullOrEmpty(encodedPassword))
                {
                    return (string.Empty, "Grafana admin secret not found or empty");
                }

                try
                {
                    var decodedBytes = Convert.FromBase64String(encodedPassword);
                    var password = Encoding.UTF8.GetString(decodedBytes);
                    return (password, string.Empty);
                }
                catch (FormatException)
                {
                    return (string.Empty, "Failed to decode Grafana admin password from base64");
                }
            }
        }
        catch (Exception ex)
        {
            return (string.Empty, $"Exception retrieving Grafana password: {ex.Message}");
        }
    }

    private string BuildPortForwardCommand(string kubeconfigPath, int localPort) =>
        $"kubectl --kubeconfig \"{kubeconfigPath}\" port-forward svc/{GrafanaService} {localPort}:80 -n {GrafanaNamespace}";

    private string BuildInstructions(string grafanaUrl, string password) =>
        $"""
        Grafana Dashboard Access Instructions:
        
        1. Credentials:
           - URL: {grafanaUrl}
           - Username: admin
           - Password: {password}
        
        2. To access the dashboard:
           a. In your terminal, run the port-forward command provided in 'PortForwardCommand'
           b. Open {grafanaUrl} in your web browser
           c. Sign in with the credentials above
           d. Press Ctrl+C in the terminal to stop port-forwarding when done
        
        Note: Keep the port-forward command running while accessing the dashboard.
        """;

    public record DashboardOpenGrafanaCommandResult(
        [property: JsonPropertyName("grafanaUrl")]
        string GrafanaUrl,
        
        [property: JsonPropertyName("adminUsername")]
        string AdminUsername,
        
        [property: JsonPropertyName("adminPassword")]
        string AdminPassword,
        
        [property: JsonPropertyName("portForwardCommand")]
        string PortForwardCommand,
        
        [property: JsonPropertyName("instructions")]
        string Instructions);
}
