// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaborations;
using Azure.Mcp.Tools.ManagedCleanroom.Models;
using Azure.Mcp.Tools.ManagedCleanroom.Options.Collaborations;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands;

[JsonSerializable(typeof(CollaborationsListOptions))]
[JsonSerializable(typeof(CollaborationsListCommand.CollaborationsListResult))]
[JsonSerializable(typeof(Collaboration))]
[JsonSerializable(typeof(List<Collaboration>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ManagedCleanroomJsonContext : JsonSerializerContext;
