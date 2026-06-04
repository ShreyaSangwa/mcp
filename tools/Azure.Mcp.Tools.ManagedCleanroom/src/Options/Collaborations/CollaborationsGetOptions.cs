// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Collaborations;

public sealed class CollaborationsGetOptions : BaseManagedCleanroomOptions
{
    [JsonPropertyName(ManagedCleanroomOptionDefinitions.CollaborationIdName)]
    public string? CollaborationId { get; set; }

    [JsonPropertyName(ManagedCleanroomOptionDefinitions.IncludeDeletedName)]
    public bool? IncludeDeleted { get; set; }

    [JsonPropertyName(ManagedCleanroomOptionDefinitions.AllowUntrustedCertName)]
    public bool AllowUntrustedCert { get; set; }
}
