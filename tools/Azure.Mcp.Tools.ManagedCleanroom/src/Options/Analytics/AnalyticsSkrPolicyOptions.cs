// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Analytics;

public sealed class AnalyticsSkrPolicyOptions : BaseManagedCleanroomOptions
{
    [JsonPropertyName(ManagedCleanroomOptionDefinitions.CollaborationIdName)]
    public string? CollaborationId { get; set; }

    [JsonPropertyName(ManagedCleanroomOptionDefinitions.KidName)]
    public string? Kid { get; set; }

    [JsonPropertyName(ManagedCleanroomOptionDefinitions.AllowUntrustedCertName)]
    public bool AllowUntrustedCert { get; set; }
}
