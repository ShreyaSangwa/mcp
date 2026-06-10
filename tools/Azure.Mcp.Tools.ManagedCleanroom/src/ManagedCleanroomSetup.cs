// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Analytics;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaboration;
using Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaborations;
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
        services.AddSingleton<CollaborationCreateCommand>();
        services.AddSingleton<CollaborationAddCollaboratorCommand>();
        services.AddSingleton<CollaborationEnableWorkloadCommand>();
        services.AddSingleton<InvitationsListCommand>();
        services.AddSingleton<InvitationsAcceptCommand>();
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

        var collaboration = new CommandGroup("collaboration", "Cleanroom ARM management operations - Commands for creating and managing Azure Cleanroom collaboration ARM resources.");
        root.AddSubGroup(collaboration);

        collaboration.AddCommand<CollaborationCreateCommand>(serviceProvider);
        collaboration.AddCommand<CollaborationAddCollaboratorCommand>(serviceProvider);
        collaboration.AddCommand<CollaborationEnableWorkloadCommand>(serviceProvider);

        var invitations = new CommandGroup("invitations", "Cleanroom invitation operations - Commands for listing and inspecting cleanroom collaboration invitations.");
        root.AddSubGroup(invitations);

        invitations.AddCommand<InvitationsListCommand>(serviceProvider);
        invitations.AddCommand<InvitationsAcceptCommand>(serviceProvider);

        return root;
    }
}
