// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ManagedCleanroom.Options.Analytics;
using Azure.Mcp.Tools.ManagedCleanroom.Options.Collaborations;
using Azure.Mcp.Tools.ManagedCleanroom.Options.Oidc;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands;

[JsonSerializable(typeof(CollaborationsListOptions))]
[JsonSerializable(typeof(CollaborationsGetOptions))]
[JsonSerializable(typeof(AnalyticsGetOptions))]
[JsonSerializable(typeof(AnalyticsSkrPolicyOptions))]
[JsonSerializable(typeof(OidcIssuerInfoOptions))]
[JsonSerializable(typeof(JsonElement))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ManagedCleanroomJsonContext : JsonSerializerContext;
