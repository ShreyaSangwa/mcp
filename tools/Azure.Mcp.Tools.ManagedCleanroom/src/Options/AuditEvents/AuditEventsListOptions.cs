// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedCleanroom.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.AuditEvents;

public class AuditEventsListOptions
{
    [Option(Description = ManagedCleanroomOptionDescriptions.Endpoint)]
    public required string Endpoint { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.CollaborationId)]
    public required string CollaborationId { get; set; }

    [Option(Description = "Optional scope filter for audit events (e.g., 'analytics'). When omitted, all audit events are returned.")]
    public string? Scope { get; set; }

    [Option(Description = "Optional starting sequence number (inclusive) for filtering audit events. When omitted, events are returned from the beginning of the log.")]
    public string? FromSeqno { get; set; }

    [Option(Description = "Optional ending sequence number (inclusive) for filtering audit events. When omitted, events are returned up to the latest.")]
    public string? ToSeqno { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool? AllowUntrustedCert { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
