// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedCleanroom.Options.Collaboration;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaboration;

[CommandMetadata(
    Id = "b3d7e914-5f2c-4a1e-9c6d-7a8b0c1d2e3f",
    Name = "enable-workload",
    Title = "Enable Cleanroom Workload",
    Description = """
        Enable a workload on your Azure Cleanroom collaboration. The request returns immediately after Azure accepts it; endpoint readiness typically takes about 7 minutes. 
        You'll need to provide: collaboration name, workload type (Analytics or AnalyticsStrict), resource group, and subscription.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CollaborationEnableWorkloadCommand(
    ILogger<CollaborationEnableWorkloadCommand> logger,
    IManagedCleanroomService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CollaborationEnableWorkloadOptions, CollaborationEnableWorkloadCommand.CollaborationEnableWorkloadCommandResult>(subscriptionResolver)
{
    private readonly ILogger<CollaborationEnableWorkloadCommand> _logger = logger;
    private readonly IManagedCleanroomService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, CollaborationEnableWorkloadOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.EnableWorkloadAsync(
                options.Name,
                options.ResourceGroup,
                options.Subscription!,
                options.WorkloadType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                result,
                ManagedCleanroomJsonContext.Default.JsonElement);

            context.Response.Message =
                $"Enable workload request accepted for '{options.WorkloadType}' on collaboration '{options.Name}'. " +
                "The workload endpoint typically takes about 7 minutes to become available. " +
                $"To check provisioning status, run: managedcleanroom collaboration get --name {options.Name} --resource-group {options.ResourceGroup} --subscription {options.Subscription}. " +
                "If provisioning is delayed or fails, check for quota limits or network issues.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error enabling workload on cleanroom collaboration. Name: {Name}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, WorkloadType: {WorkloadType}",
                options.Name, options.ResourceGroup, options.Subscription, options.WorkloadType);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Collaboration not found. Verify the collaboration name, resource group, and subscription.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed enabling the workload. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Workload is already enabled or the collaboration is in a conflicting state.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound => HttpStatusCode.NotFound,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden => HttpStatusCode.Forbidden,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict => HttpStatusCode.Conflict,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public record CollaborationEnableWorkloadCommandResult;
}



