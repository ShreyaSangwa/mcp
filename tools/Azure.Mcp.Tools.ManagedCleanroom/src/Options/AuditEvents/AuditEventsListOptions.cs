// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedCleanroom.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.AuditEvents;

public class AuditEventsListOptions
{
    [Option(ManagedCleanroomOptionDescriptions.Endpoint)]
    public required string Endpoint { get; set; }

    [Option(ManagedCleanroomOptionDescriptions.CollaborationId)]
    public required string CollaborationId { get; set; }

    [Option("Optional scope filter for audit events (e.g., 'analytics'). When omitted, all audit events are returned.")]
    public string? Scope { get; set; }

    [Option("Optional starting sequence number (inclusive) for filtering audit events. When omitted, events are returned from the beginning of the log.")]
    public string? FromSeqno { get; set; }

    [Option("Optional ending sequence number (inclusive) for filtering audit events. When omitted, events are returned up to the latest.")]
    public string? ToSeqno { get; set; }

    [Option(ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool? AllowUntrustedCert { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
