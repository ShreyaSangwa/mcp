// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaborations;
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
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var root = new CommandGroup(Name,
            "Azure Managed Cleanroom operations - Commands for interacting with the Azure Cleanroom Analytics Frontend, including listing collaborations a user participates in.", Title);

        var collaborations = new CommandGroup("collaborations", "Cleanroom collaboration operations - Commands for listing and inspecting cleanroom collaborations.");
        root.AddSubGroup(collaborations);

        collaborations.AddCommand<CollaborationsListCommand>(serviceProvider);

        return root;
    }
}
