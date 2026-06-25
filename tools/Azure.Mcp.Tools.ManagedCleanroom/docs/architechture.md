# Azure Managed Cleanroom MCP Toolset - Architecture

## Overview

`Azure.Mcp.Tools.ManagedCleanroom` gives AI agents access to **two surfaces** of Azure Cleanroom:

| Surface | What it does | Client |
|---------|--------------|--------|
| **Analytics Frontend (data plane)** | Inspect collaborations, datasets, queries, runs, OIDC, audit, etc. | Generated `CollaborationClient` from `Azure.Cleanroom.Analytics.Frontend.Client` |
| **Cleanroom ARM (control plane)** | Create / manage `Microsoft.CleanRoom/Collaborations` ARM resources | `Azure.ResourceManager` (generic resources, API `2026-04-30-preview`) |

All commands live under the `azmcp managedcleanroom ...` namespace.

---

## Project Structure

```
Azure.Mcp.Tools.ManagedCleanroom/
├── docs/
│   └── architechture.md                  # This file
├── src/
│   ├── Azure.Mcp.Tools.ManagedCleanroom.csproj
│   ├── AssemblyInfo.cs
│   ├── ManagedCleanroomSetup.cs          # DI + command tree
│   ├── Commands/
│   │   ├── ManagedCleanroomJsonContext.cs # AOT JsonElement context
│   │   ├── Analytics/
│   │   │   ├── AnalyticsGetCommand.cs
│   │   │   └── AnalyticsSkrPolicyCommand.cs
│   │   ├── AuditEvents/
│   │   │   └── AuditEventsListCommand.cs
│   │   ├── Collaboration/
│   │   │   ├── CollaborationAddCollaboratorCommand.cs
│   │   │   ├── CollaborationCreateCommand.cs
│   │   │   ├── CollaborationEnableWorkloadCommand.cs
│   │   │   ├── CollaborationGetCommand.cs
│   │   │   └── CollaborationGetReadonlyKubeconfigCommand.cs
│   │   ├── Collaborations/
│   │   │   ├── CollaborationsListCommand.cs
│   │   │   └── CollaborationsGetCommand.cs
│   │   ├── Consent/
│   │   │   └── ConsentPutCommand.cs
│   │   ├── Datasets/
│   │   │   ├── DatasetsGetCommand.cs
│   │   │   ├── DatasetsListCommand.cs
│   │   │   └── DatasetsPublishCommand.cs
│   │   ├── Invitations/
│   │   │   ├── InvitationsAcceptCommand.cs
│   │   │   └── InvitationsListCommand.cs
│   │   ├── Oidc/
│   │   │   ├── OidcIssuerInfoCommand.cs
│   │   │   ├── OidcKeysCommand.cs
│   │   │   └── OidcSetIssuerUrlCommand.cs
│   │   ├── Queries/
│   │   │   ├── QueriesGetCommand.cs
│   │   │   ├── QueriesListCommand.cs
│   │   │   ├── QueriesPublishCommand.cs
│   │   │   ├── QueriesRunCommand.cs
│   │   │   ├── QueriesRunsCommand.cs
│   │   │   └── QueriesVoteCommand.cs
│   │   └── Runs/
│   │       └── RunsGetCommand.cs
│   ├── Models/                            # Empty - responses use raw JsonElement
│   ├── Options/
│   │   ├── ManagedCleanroomOptionDescriptions.cs
│   │   ├── Analytics/
│   │   │   ├── AnalyticsGetOptions.cs
│   │   │   └── AnalyticsSkrPolicyOptions.cs
│   │   ├── AuditEvents/
│   │   │   └── AuditEventsListOptions.cs
│   │   ├── Collaboration/
│   │   │   ├── CollaborationAddCollaboratorOptions.cs
│   │   │   ├── CollaborationCreateOptions.cs
│   │   │   ├── CollaborationEnableWorkloadOptions.cs
│   │   │   ├── CollaborationGetOptions.cs
│   │   │   └── CollaborationGetReadonlyKubeconfigOptions.cs
│   │   ├── Collaborations/
│   │   │   ├── CollaborationsListOptions.cs
│   │   │   └── CollaborationsGetOptions.cs
│   │   ├── Consent/
│   │   │   └── ConsentPutOptions.cs
│   │   ├── Datasets/
│   │   │   ├── DatasetsGetOptions.cs
│   │   │   ├── DatasetsListOptions.cs
│   │   │   └── DatasetsPublishOptions.cs
│   │   ├── Invitations/
│   │   │   ├── InvitationsAcceptOptions.cs
│   │   │   └── InvitationsListOptions.cs
│   │   ├── Oidc/
│   │   │   ├── OidcIssuerInfoOptions.cs
│   │   │   ├── OidcKeysOptions.cs
│   │   │   └── OidcSetIssuerUrlOptions.cs
│   │   ├── Queries/
│   │   │   ├── QueriesGetOptions.cs
│   │   │   ├── QueriesListOptions.cs
│   │   │   ├── QueriesPublishOptions.cs
│   │   │   ├── QueriesRunOptions.cs
│   │   │   ├── QueriesRunsOptions.cs
│   │   │   └── QueriesVoteOptions.cs
│   │   └── Runs/
│   │       └── RunsGetOptions.cs
│   └── Services/
│       ├── IManagedCleanroomService.cs
│       ├── ManagedCleanroomService.cs     # Frontend client + ARM glue
│       └── ManagedCleanroomSerializerContext.cs
└── tests/
    ├── test-resources.bicep
    ├── test-resources-post.ps1
    └── Azure.Mcp.Tools.ManagedCleanroom.Tests/
        ├── Azure.Mcp.Tools.ManagedCleanroom.Tests.csproj
        ├── assets.json
        ├── AssemblyAttributes.cs
        ├── Usings.cs
        ├── ManagedCleanroomCommandTests.cs # Recorded integration tests
        ├── Analytics/
        │   ├── AnalyticsGetCommandTests.cs
        │   └── AnalyticsSkrPolicyCommandTests.cs
        ├── AuditEvents/
        │   └── AuditEventsListCommandTests.cs
        ├── Collaboration/
        │   ├── CollaborationAddCollaboratorCommandTests.cs
        │   ├── CollaborationCreateCommandTests.cs
        │   ├── CollaborationEnableWorkloadCommandTests.cs
        │   ├── CollaborationGetCommandTests.cs
        │   └── CollaborationGetReadonlyKubeconfigCommandTests.cs
        ├── Collaborations/
        │   ├── CollaborationsListCommandTests.cs
        │   └── CollaborationsGetCommandTests.cs
        ├── Consent/
        │   └── ConsentPutCommandTests.cs
        ├── Datasets/
        │   ├── DatasetsGetCommandTests.cs
        │   ├── DatasetsListCommandTests.cs
        │   └── DatasetsPublishCommandTests.cs
        ├── Invitations/
        │   ├── InvitationsAcceptCommandTests.cs
        │   └── InvitationsListCommandTests.cs
        ├── Oidc/
        │   ├── OidcIssuerInfoCommandTests.cs
        │   ├── OidcKeysCommandTests.cs
        │   └── OidcSetIssuerUrlCommandTests.cs
        ├── Queries/
        │   ├── QueriesGetCommandTests.cs
        │   ├── QueriesListCommandTests.cs
        │   ├── QueriesPublishCommandTests.cs
        │   ├── QueriesRunCommandTests.cs
        │   ├── QueriesRunsCommandTests.cs
        │   └── QueriesVoteCommandTests.cs
        └── Runs/
            └── RunsGetCommandTests.cs
```

---

## How It Works

```
Command (sealed, [CommandMetadata])
    │
    ▼
IManagedCleanroomService (ManagedCleanroomService)
    │
    ├─► CollaborationClient   (HTTPS to --endpoint, bearer token w/ ARM scope)
    │       returns Azure.Response -> parsed to JsonElement
    │
    └─► ArmClient             (Microsoft.CleanRoom/Collaborations, 2026-04-30-preview)
            CreateOrUpdate at WaitUntil.Started (~25 min provisioning)
```

- Data-plane commands extend `AuthenticatedCommand<TOptions, TResult>`.
- ARM commands (control plane) extend `SubscriptionCommand<TOptions, TResult>` and inject `ISubscriptionResolver`.
- Responses are returned as **raw `JsonElement`** (no typed DTOs) - the generated client is protocol-method-only, so this keeps the toolset faithful to the upstream contract.
- AOT safety: source-generated `JsonSerializerContext`, no reflection, manual `Utf8JsonWriter` for ARM payloads.
- Stateless: a new client is built per call; safe for multi-user remote HTTP mode.
- Authentication: uses `BaseAzureResourceService.GetCredential` so stdio, hosting identity, and OBO all work transparently. Data-plane bearer token uses the **ARM default scope**.

---

## Commands Implemented (26)

| Group | Command | Tool Name | What it does |
|-------|---------|-----------|--------------|
| `collaborations` | `list` | `managedcleanroom_collaborations_list` | List cleanrooms the user participates in |
| `collaborations` | `get` | `managedcleanroom_collaborations_get` | Get a collaboration by UUID |
| `analytics` | `get` | `managedcleanroom_analytics_get` | Get analytics workload config |
| `analytics` | `skr-policy` | `managedcleanroom_analytics_skr-policy` | Get SKR policy for a key (`--kid`) |
| `oidc` | `issuer-info` | `managedcleanroom_oidc_issuer-info` | Get OIDC issuer info |
| `oidc` | `keys` | `managedcleanroom_oidc_keys` | Get OIDC JWKS public keys |
| `oidc` | `set-issuer-url` | `managedcleanroom_oidc_set-issuer-url` | Register an OIDC issuer URL |
| `collaboration` | `create` | `managedcleanroom_collaboration_create` | Create the ARM resource, poll provisioningState every 30s, return result + elapsed time (~25 min) |
| `collaboration` | `get` | `managedcleanroom_collaboration_get` | Get ARM resource details (provisioningState, health, workloads) |
| `collaboration` | `add-collaborator` | `managedcleanroom_collaboration_add-collaborator` | Add a user/SPN as a collaborator via ARM `addCollaborator` action |
| `collaboration` | `enable-workload` | `managedcleanroom_collaboration_enable-workload` | Enable a workload type (e.g. Analytics) via ARM `enableWorkload` action |
| `collaboration` | `get-readonly-kubeconfig` | `managedcleanroom_collaboration_get-readonly-kubeconfig` | Get a read-only kubeconfig for the backing AKS cluster via ARM `getReadonlyKubeConfig` action (`Secret = true`) |
| `collaboration` | `recover` | `managedcleanroom_collaboration_recover` | Recover a collaboration via ARM `recover` action |
| `invitations` | `list` | `managedcleanroom_invitations_list` | List collaboration invitations |
| `invitations` | `accept` | `managedcleanroom_invitations_accept` | Accept a collaboration invitation |
| `datasets` | `publish` | `managedcleanroom_datasets_publish` | Publish an input/output dataset document |
| `datasets` | `get` | `managedcleanroom_datasets_get` | Get a dataset document |
| `datasets` | `list` | `managedcleanroom_datasets_list` | List all dataset documents |
| `consent` | `put` | `managedcleanroom_consent_put` | Toggle execution consent for a document |
| `queries` | `publish` | `managedcleanroom_queries_publish` | Publish a query document |
| `queries` | `get` | `managedcleanroom_queries_get` | Get a query document |
| `queries` | `list` | `managedcleanroom_queries_list` | List published queries |
| `queries` | `vote` | `managedcleanroom_queries_vote` | Approve or reject a query |
| `queries` | `run` | `managedcleanroom_queries_run` | Execute a query |
| `queries` | `runs` | `managedcleanroom_queries_runs` | Get run history for a query |
| `runs` | `get` | `managedcleanroom_runs_get` | Poll run state |
| `auditevents` | `list` | `managedcleanroom_auditevents_list` | List audit events |

> `collaborations` (plural) = data plane. `collaboration` (singular) = ARM control plane.

### Shared Options

| Option | Used by | Notes |
|--------|---------|-------|
| `--endpoint` | All data-plane | Analytics Frontend URL |
| `--collaboration-id` | Most data-plane | UUID |
| `--allow-untrusted-cert` | All data-plane | Dev/test only - skips TLS validation |
| `--tenant` | All | Forwarded to credential |
| `--name`, `--location`, `--resource-location`, `--collaborator`, `--resource-group`, `--subscription`, `--retry` | `collaboration create` | Standard ARM options |

---

## End-to-End Workflow Coverage

Mapped against the [Cleanroom Analytics SDK sample workflow](https://github.com/Azure-Samples/azure-cleanroom-samples). Only operations covered by the `Azure.Cleanroom.Analytics.Frontend.Client` SDK (data plane) or `Microsoft.CleanRoom/Collaborations` ARM API (control plane) are listed.

Legend: ✅ implemented | ❌ not yet implemented

| Step | Operation | Command | Status |
|------|-----------|---------|:------:|
| 02 Create collaboration (ARM) | PUT (waits for completion) | `collaboration create` | Completed |
| 02 Enable Analytics workload | POST `/enableWorkload` | `collaboration enable-workload` | Completed |
| 02 Add more collaborators | POST `/addCollaborator` | `collaboration add-collaborator` | Completed |
| 03 Discover collaboration UUID | `collaborations list` | `collaborations list` | Completed |
| 03 Inspect a single collaboration | `collaborations get` | `collaborations get` | Completed |
| 03 List invitations | `invitations list` | `invitations list` | Completed |
| 03 Accept invitation | `invitations accept` | `invitations accept` | Completed |
| 05 Fetch JWKS | `oidc keys` | `oidc keys` | Completed |
| 05 Verify OIDC issuer | `oidc issuer-info` | `oidc issuer-info` | Completed |
| 05 Register issuer URL | `oidc set-issuer-url` | `oidc set-issuer-url` | Completed |
| 06 Publish input/output dataset | `datasets publish` | `datasets publish` | Completed |
| 06 Verify dataset | `datasets get` | `datasets get` | Completed |
| 06 Toggle execution consent | `consent put` | `consent put` | Completed |
| 06 Fetch SKR policy (CPK) | `analytics skr-policy` | `analytics skr-policy` | Completed |
| 06 Inspect analytics workload config | `analytics get` | `analytics get` | Completed |
| 07 Look up partner dataset id | `datasets list` | `datasets list` | Completed |
| 07 Publish query | `queries publish` | `queries publish` | Completed |
| 08 Inspect query + proposal ID | `queries get` | `queries get` | Completed |
| 08 List published queries | `queries list` | `queries list` | Completed |
| 08 Vote on query | `queries vote` | `queries vote` | Completed |
| 09 Run query | `queries run` | `queries run` | Completed |
| 09 Check collaboration health | ARM GET | `collaboration get` | Completed |
| 09 Get readonly kubeconfig | POST `/getReadonlyKubeConfig` | `collaboration get-readonly-kubeconfig` | Completed |
| 10 Poll run state | `runs get` | `runs get` | Completed |
| 11 Run history | `queries runs` | `queries runs` | Completed |
| 11 Audit events | `auditevents list` | `auditevents list` | Completed |
| App G Force recover | POST `/recover` | `collaboration recover` | Completed |
| App G Delete collaboration | ARM DELETE | `collaboration delete` | Pending |

---

## Missing Commands (Roadmap)

**Total: 27 implemented out of ~28 surface-area commands.**

### Control plane (ARM) - 1 remaining

`collaboration` group needs: `delete`.

### Minimum viable end-to-end flow

The toolset now covers the complete happy-path agent workflow end-to-end. Remaining gap is cleanup:

- **ARM:** `collaboration delete` (clean up test/dev resources).

---

## Adding a New Command (Checklist)

1. Add the method to `IManagedCleanroomService` + `ManagedCleanroomService` (reuse `BuildClientAsync` / `ParseResponse` for data plane; `CreateArmClientWithApiVersionAsync` + `GenericResources` for ARM).
2. Add `Options/<Group>/<Command>Options.cs` (reuse `ManagedCleanroomOptionDescriptions` and `OptionDescriptions`).
3. Add `Commands/<Group>/<Command>Command.cs` (sealed, with `[CommandMetadata]`, inheriting `AuthenticatedCommand<,>` or `SubscriptionCommand<,>`).
4. Register the command as a singleton in `ManagedCleanroomSetup` and attach to its `CommandGroup` (create a new group if needed).
5. Add unit + recorded tests under `tests/Azure.Mcp.Tools.ManagedCleanroom.Tests/<Group>/`.
6. Keep returning raw `JsonElement` to stay AOT-safe and faithful to the upstream contract.

---

## Tests

- `tests/test-resources.bicep` - lightweight; takes `cleanroomEndpoint` + `cleanroomCollaborationId` as inputs (the frontend is not a first-class ARM resource).
- `tests/test-resources-post.ps1` - generates the test settings file.
- `tests/Azure.Mcp.Tools.ManagedCleanroom.Tests/` - recorded integration tests + per-command unit tests (constructor, happy path, argument forwarding, validation, error mapping for 401/403/404/409).
