// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options;

public class BaseManagedCleanroomOptions : GlobalOptions
{
    [JsonPropertyName(ManagedCleanroomOptionDefinitions.EndpointName)]
    public string? Endpoint { get; set; }
}
