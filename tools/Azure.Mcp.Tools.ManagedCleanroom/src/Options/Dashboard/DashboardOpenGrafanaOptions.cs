// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedCleanroom.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Dashboard;

public class DashboardOpenGrafanaOptions
{
    [Option(Description = ManagedCleanroomOptionDescriptions.Endpoint)]
    public required string Endpoint { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.CollaborationId)]
    public required string CollaborationId { get; set; }

    [Option(Name = "kubeconfig-path", Description = "Path to the kubeconfig file for accessing the AKS cluster (e.g., /path/to/kubeconfig.yaml). Required for kubectl access to retrieve Grafana credentials.")]
    public required string KubeconfigPath { get; set; }

    [Option(Name = "local-port", Description = "Local port for port-forwarding to Grafana (default: 3000).")]
    public int LocalPort { get; set; } = 3000;

    [Option(Description = ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool? AllowUntrustedCert { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
