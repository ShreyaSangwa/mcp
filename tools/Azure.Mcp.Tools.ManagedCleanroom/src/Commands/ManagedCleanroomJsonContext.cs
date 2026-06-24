// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands;

// Only JsonElement is registered here — no typed *CommandResult models — because
// CollaborationClient is a protocol-method-only generated client that returns raw
// Azure.Response with no typed deserialization. Commands pass the raw JsonElement
// response directly to ResponseResult.Create without wrapping it in a result record.
[JsonSerializable(typeof(JsonElement))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ManagedCleanroomJsonContext : JsonSerializerContext;



