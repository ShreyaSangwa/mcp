// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Analytics;

public sealed class AnalyticsGetOptions : BaseManagedCleanroomOptions
{
    [JsonPropertyName(ManagedCleanroomOptionDefinitions.CollaborationIdName)]
    public string? CollaborationId { get; set; }

    [JsonPropertyName(ManagedCleanroomOptionDefinitions.AllowUntrustedCertName)]
    public bool AllowUntrustedCert { get; set; }
}
