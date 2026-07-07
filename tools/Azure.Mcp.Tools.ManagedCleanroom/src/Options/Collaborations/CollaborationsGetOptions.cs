// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Collaborations;

public class CollaborationsGetOptions : BaseManagedCleanroomDataPlaneOptions
{
    [Option(Description = ManagedCleanroomOptionDescriptions.CollaborationId)]
    public required string CollaborationId { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.IncludeDeleted)]
    public bool? IncludeDeleted { get; set; }
}