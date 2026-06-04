// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Collaborations;

public sealed class CollaborationsListOptions : BaseManagedCleanroomOptions
{
    [JsonPropertyName(ManagedCleanroomOptionDefinitions.ActiveOnlyName)]
    public bool? ActiveOnly { get; set; }

    [JsonPropertyName(ManagedCleanroomOptionDefinitions.AllowUntrustedCertName)]
    public bool AllowUntrustedCert { get; set; }
}
