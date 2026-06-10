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
│   │   ├── Collaboration/
│   │   │   └── CollaborationCreateCommand.cs
│   │   ├── Collaborations/
│   │   │   ├── CollaborationsListCommand.cs
│   │   │   └── CollaborationsGetCommand.cs
│   │   └── Oidc/
│   │       └── OidcIssuerInfoCommand.cs
│   ├── Models/                            # Empty - responses use raw JsonElement
│   ├── Options/
│   │   ├── ManagedCleanroomOptionDescriptions.cs
│   │   ├── Analytics/
│   │   │   ├── AnalyticsGetOptions.cs
│   │   │   └── AnalyticsSkrPolicyOptions.cs
│   │   ├── Collaboration/
│   │   │   └── CollaborationCreateOptions.cs
│   │   ├── Collaborations/
│   │   │   ├── CollaborationsListOptions.cs
│   │   │   └── CollaborationsGetOptions.cs
│   │   └── Oidc/
│   │       └── OidcIssuerInfoOptions.cs
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
        ├── Collaboration/
        │   └── CollaborationCreateCommandTests.cs
        ├── Collaborations/
        │   ├── CollaborationsListCommandTests.cs
        │   └── CollaborationsGetCommandTests.cs
        └── Oidc/
            └── OidcIssuerInfoCommandTests.cs
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
- The ARM `collaboration create` command extends `SubscriptionCommand<TOptions, TResult>`.
- Responses are returned as **raw `JsonElement`** (no typed DTOs) - the generated client is protocol-method-only, so this keeps the toolset faithful to the upstream contract.
- AOT safety: source-generated `JsonSerializerContext`, no reflection, manual JSON string for the ARM create payload.
- Stateless: a new client is built per call; safe for multi-user remote HTTP mode.
- Authentication: uses `BaseAzureResourceService.GetCredential` so stdio, hosting identity, and OBO all work transparently. Data-plane bearer token uses the **ARM default scope**.

---

## Commands Implemented Today (6)

| Group | Command | Tool Name | What it does |
|-------|---------|-----------|--------------|
| `collaborations` | `list` | `managedcleanroom_collaborations_list` | List cleanrooms the user participates in |
| `collaborations` | `get` | `managedcleanroom_collaborations_get` | Get a collaboration by UUID |
| `analytics` | `get` | `managedcleanroom_analytics_get` | Get analytics workload config |
| `analytics` | `skr-policy` | `managedcleanroom_analytics_skr-policy` | Get SKR policy for a key (`--kid`) |
| `oidc` | `issuer-info` | `managedcleanroom_oidc_issuer-info` | Get OIDC issuer info |
| `collaboration` | `create` | `managedcleanroom_collaboration_create` | Create the ARM resource, poll provisioningState every 30s, return result + elapsed time (~25 min) |

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
| 10 Poll run state | `runs get` | `runs get` | Completed |
| 11 Run history | `queries runs` | `queries runs` | Completed |
| 11 Audit events | `audit-events list` | `audit-events list` | Pending |
| 12 Get readonly kubeconfig | POST `/getReadonlyKubeConfig` | `collaboration get-readonly-kubeconfig` | Pending |
| App G Force recover | POST `/recover` | `collaboration recover` | Pending |
| App G Delete collaboration | ARM DELETE | `collaboration delete` | Pending |

---

## Missing Commands (Roadmap)

**Total: 18 implemented out of ~34 surface-area commands.**

### Data plane (frontend) - 11 missing

| Group | Commands |
|-------|----------|
| `datasets` (extend) | `queries` (list, get, publish, vote, run, runs) |
| `invitations` (extend) | `get` |
| `consent` (extend) | `get` |
| `audit-events` | `list` (with `--from`, `--to`, `--type`) |
| `analytics secrets` | `set` (mark `Secret = true`) |
| `collaborations` (extend) | `report` (attestation report) |

### Control plane (ARM) - 5 missing

`collaboration` group needs: `list`, `update`, `delete`, `get-readonly-kubeconfig`, `recover`.

### Minimum viable end-to-end flow

A minimum viable end-to-end agent flow needs at least these additions on top of what exists today:

- **ARM:** `collaboration delete` / `recover` (for cleanup).
- **Frontend:** `queries list/get/publish/vote/run/runs`, `runs get`, `audit-events list`.

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
