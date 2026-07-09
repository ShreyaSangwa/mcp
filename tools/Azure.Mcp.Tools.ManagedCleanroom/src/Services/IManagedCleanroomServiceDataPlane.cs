// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

/// <summary>
/// Data-plane operations against the Cleanroom Analytics Frontend service.
/// Authentication uses a bearer token scoped to the frontend endpoint.
/// </summary>
public interface IManagedCleanroomServiceDataPlane : IManagedCleanroomService;
