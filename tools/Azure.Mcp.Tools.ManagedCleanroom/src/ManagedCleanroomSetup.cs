// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Commands.Analytics;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.AuditEvents;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.CollaborationArm;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaborations;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Consent;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Datasets;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Queries;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Runs;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Invitations;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Oidc;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ManagedCleanroom;

public class ManagedCleanroomSetup : IAreaSetup
{
    public string Name => "managedcleanroom";

    public string Title => "Azure Managed Cleanroom";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IManagedCleanroomService, ManagedCleanroomService>();
        services.AddSingleton<CollaborationsListCommand>();
        services.AddSingleton<CollaborationsGetCommand>();
        services.AddSingleton<AnalyticsGetCommand>();
        services.AddSingleton<AnalyticsSkrPolicyCommand>();
        services.AddSingleton<OidcIssuerInfoCommand>();
        services.AddSingleton<OidcKeysCommand>();
        services.AddSingleton<OidcSetIssuerUrlCommand>();
        services.AddSingleton<CollaborationCreateCommand>();
        services.AddSingleton<CollaborationGetCommand>();
        services.AddSingleton<CollaborationDeleteCommand>();
        services.AddSingleton<CollaborationAddCollaboratorCommand>();
        services.AddSingleton<CollaborationEnableWorkloadCommand>();
        services.AddSingleton<CollaborationGetReadonlyKubeconfigCommand>();
        services.AddSingleton<CollaborationRecoverCommand>();
        services.AddSingleton<InvitationsListCommand>();
        services.AddSingleton<InvitationsAcceptCommand>();
        services.AddSingleton<DatasetsPublishCommand>();
        services.AddSingleton<DatasetsBuildBodyCommand>();
        services.AddSingleton<DatasetsGetCommand>();
        services.AddSingleton<DatasetsListCommand>();
        services.AddSingleton<ConsentPutCommand>();
        services.AddSingleton<QueriesPublishCommand>();
        services.AddSingleton<QueriesGetCommand>();
        services.AddSingleton<QueriesListCommand>();
        services.AddSingleton<QueriesVoteCommand>();
        services.AddSingleton<QueriesRunCommand>();
        services.AddSingleton<QueriesRunsCommand>();
        services.AddSingleton<RunsGetCommand>();
        services.AddSingleton<AuditEventsListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var root = new CommandGroup(Name,
            "Azure Managed Cleanroom operations - Commands for interacting with the Azure Cleanroom Analytics Frontend, including listing and inspecting collaborations and analytics workloads.", Title);

        var collaborations = new CommandGroup("collaborations", "Cleanroom collaboration operations - Commands for listing and inspecting cleanroom collaborations.");
        root.AddSubGroup(collaborations);

        collaborations.AddCommand<CollaborationsListCommand>(serviceProvider);
        collaborations.AddCommand<CollaborationsGetCommand>(serviceProvider);

        var analytics = new CommandGroup("analytics", "Cleanroom analytics operations - Commands for inspecting analytics workload configuration on a cleanroom collaboration.");
        root.AddSubGroup(analytics);

        analytics.AddCommand<AnalyticsGetCommand>(serviceProvider);
        analytics.AddCommand<AnalyticsSkrPolicyCommand>(serviceProvider);

        var oidc = new CommandGroup("oidc", "Cleanroom OIDC operations - Commands for inspecting OIDC issuer configuration on a cleanroom collaboration.");
        root.AddSubGroup(oidc);

        oidc.AddCommand<OidcIssuerInfoCommand>(serviceProvider);
        oidc.AddCommand<OidcKeysCommand>(serviceProvider);
        oidc.AddCommand<OidcSetIssuerUrlCommand>(serviceProvider);

        var collaborationArm = new CommandGroup("collaborationarm", "Cleanroom ARM management operations - Commands for creating and managing Azure Cleanroom collaboration ARM resources.");
        root.AddSubGroup(collaborationArm);

        collaborationArm.AddCommand<CollaborationCreateCommand>(serviceProvider);
        collaborationArm.AddCommand<CollaborationGetCommand>(serviceProvider);
        collaborationArm.AddCommand<CollaborationDeleteCommand>(serviceProvider);
        collaborationArm.AddCommand<CollaborationAddCollaboratorCommand>(serviceProvider);
        collaborationArm.AddCommand<CollaborationEnableWorkloadCommand>(serviceProvider);
        collaborationArm.AddCommand<CollaborationGetReadonlyKubeconfigCommand>(serviceProvider);
        collaborationArm.AddCommand<CollaborationRecoverCommand>(serviceProvider);

        var invitations = new CommandGroup("invitations", "Cleanroom invitation operations - Commands for listing and inspecting cleanroom collaboration invitations.");
        root.AddSubGroup(invitations);

        invitations.AddCommand<InvitationsListCommand>(serviceProvider);
        invitations.AddCommand<InvitationsAcceptCommand>(serviceProvider);

        var datasets = new CommandGroup("datasets", "Cleanroom dataset operations - Commands for publishing and inspecting datasets in a cleanroom collaboration.");
        root.AddSubGroup(datasets);

        datasets.AddCommand<DatasetsPublishCommand>(serviceProvider);
        datasets.AddCommand<DatasetsBuildBodyCommand>(serviceProvider);
        datasets.AddCommand<DatasetsGetCommand>(serviceProvider);
        datasets.AddCommand<DatasetsListCommand>(serviceProvider);

        var consent = new CommandGroup("consent", "Cleanroom consent operations - Commands for creating and managing consent documents in a cleanroom collaboration.");
        root.AddSubGroup(consent);

        consent.AddCommand<ConsentPutCommand>(serviceProvider);

        var queries = new CommandGroup("queries", "Cleanroom query operations - Commands for publishing, inspecting, and running analytics queries on a cleanroom collaboration.");
        root.AddSubGroup(queries);

        queries.AddCommand<QueriesPublishCommand>(serviceProvider);
        queries.AddCommand<QueriesGetCommand>(serviceProvider);
        queries.AddCommand<QueriesListCommand>(serviceProvider);
        queries.AddCommand<QueriesVoteCommand>(serviceProvider);
        queries.AddCommand<QueriesRunCommand>(serviceProvider);
        queries.AddCommand<QueriesRunsCommand>(serviceProvider);

        var runs = new CommandGroup("runs", "Cleanroom run operations - Commands for polling and inspecting query run state in a cleanroom collaboration.");
        root.AddSubGroup(runs);

        runs.AddCommand<RunsGetCommand>(serviceProvider);

        var auditevents = new CommandGroup("auditevents", "Cleanroom audit event operations - Commands for listing and inspecting audit events in a cleanroom collaboration.");
        root.AddSubGroup(auditevents);

        auditevents.AddCommand<AuditEventsListCommand>(serviceProvider);

        return root;
    }
}
