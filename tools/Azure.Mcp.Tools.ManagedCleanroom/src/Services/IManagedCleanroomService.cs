// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Models;

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

public interface IManagedCleanroomService
{
    Task<List<Collaboration>> ListCollaborationsAsync(
        string endpoint,
        bool? activeOnly = null,
        CancellationToken cancellationToken = default);
}
