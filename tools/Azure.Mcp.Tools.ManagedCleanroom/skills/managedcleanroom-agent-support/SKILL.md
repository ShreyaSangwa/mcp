---
name: managedcleanroom-agent-support
description: 'ManagedCleanroom agent operating workflow for intent routing, input collection, dataset/query/run playbooks, validation guardrails, error handling, and response behavior. USE WHEN: user asks how to operate Azure ManagedCleanroom end-to-end, publish datasets, run queries, inspect runs, list collaborations, or troubleshoot cleanroom command flows.'
argument-hint: 'Describe the user goal and current context (endpoint, collaboration ID, and target operation).'
---

# Azure ManagedCleanroom Agent Support

## Purpose

Provide a consistent, end-to-end operating workflow for ManagedCleanroom scenarios. This skill defines:
1. How to route user intent to the correct command group.
2. What required inputs to collect before tool execution.
3. Safe command sequences for common workflows.
4. Validation and error-handling rules.
5. Response behavior for clear, actionable guidance.

## When to Use

Use this skill when the user asks to:
1. Operate Azure ManagedCleanroom end-to-end.
2. Create/manage collaboration ARM resources.
3. Publish datasets or run queries.
4. Poll run status and investigate failures.
5. Configure OIDC or consent documents.
6. Inspect audit events for traceability.

## Prerequisites

Before execution:
1. User is authenticated with a valid Azure context.
2. Endpoint and collaboration context are known for frontend/data-plane commands.
3. Subscription/resource group are known for ARM commands.
4. For ARM create workflows, user has an existing resource group or is ready to create one using the existing command.
5. Required IDs (document ID/job ID) are collected for dataset/query/run paths.

## Procedure

Use this operating loop for all requests:
1. Classify the user intent into one command group.
2. Validate required inputs and safety constraints.
3. Run the playbook for that workflow.
4. Verify mutating operations with a follow-up read command.
5. Report result and next actionable step.
6. If error occurs, apply the failure recovery playbook.

## Scope

This skill covers:
1. Collaboration discovery and inspection.
2. ARM collaboration resource lifecycle operations.
3. Invitation inspection and acceptance.
4. Dataset build/publish/get/list flows.
5. Query publish/vote/run/runs flows.
6. Run status polling.
7. Audit event inspection.
8. Dataset onboarding, schema review, and consent guidance.
9. Query approval guidance and warnings.
10. OIDC setup and managed identity federation guidance for dataset access.
11. Query authoring, execution preparation, and output handling.

This skill does not cover:
1. Unrelated Azure services.
2. Business logic decisions that require user policy choices.
3. Disable workload — coming soon, not yet implemented.
4. Delete or remove collaborators — coming soon, not yet implemented.

## Intent Routing

Route requests to these command groups:
1. Frontend collaboration inspection: `managedcleanroom collaborations`.
2. ARM collaboration management: `managedcleanroom collaborationarm`.
3. Invitation flow: `managedcleanroom invitations`.
4. Dataset flow: `managedcleanroom datasets`.
5. Query flow: `managedcleanroom queries`.
6. Run polling: `managedcleanroom runs`.
7. Audit trail: `managedcleanroom auditevents`.
8. OIDC settings: `managedcleanroom oidc`.
9. Analytics workload inspection: `managedcleanroom analytics`.
10. Consent documents: `managedcleanroom consent`.
11. "How do I create" / getting-started guidance: follow the "How Do I Create an ACCR Instance" playbook and offer step-by-step walk-through.
12. Disable workload: not yet implemented — inform user it is coming soon.
13. Delete or remove collaborators: not yet implemented — inform user it is coming soon.
14. Dataset onboarding and sample/test data preparation: follow the dataset onboarding and sample data playbooks.
15. Query approval: follow the query approval playbook.
16. Withdraw consent / prevent reuse: follow the consent withdrawal playbook.
17. Query authoring and publish preparation: follow the query authoring workflow.
18. Query execution details, output retrieval, and output analysis: follow the query execution and output workflows.

## Input Collection Rules

Before any command call:
1. Collect all required inputs for that command.
2. Ask for missing values explicitly; do not infer values.
3. Never hardcode sample names (for example woodgrove or northwind).
4. Keep user-provided values unchanged unless the command explicitly normalizes them.
5. Confirm destructive operations before execution.
6. For dataset onboarding, collect the dataset name, intended collaboration, encryption mode, storage location, and schema inputs before publishing.
7. For query approval, show the selected query details before voting.
8. For query publication, collect the query name, query text or file content, input dataset references, and output dataset reference before publishing.

### Frontend Endpoint Reference

Use this as a reference example endpoint for frontend/data-plane flows when a concrete endpoint is needed in guidance:
1. `https://prod.workload-frontendwestus.cleanroom.cloudapp.azure.net`
2. Prefer user-provided endpoint values at execution time.
3. If the user only knows the collaboration region, derive the standard frontend endpoint pattern as `https://prod.workload-frontend<region>.cleanroom.cloudapp.azure.net` and ask the user to confirm it before use.

## Workflow Playbooks

### Readiness Checklist

Run before first operation in a session.
1. Confirm user has endpoint and collaboration context.
2. Confirm required IDs are available for the requested flow (collaboration ID, document ID, or job ID).
3. Confirm tenant/subscription context when ARM commands are requested.
4. For frontend/data-plane calls, set `allow-untrusted-cert` to true.
5. For write operations, confirm intended target resource names before execution.
6. For dataset publish, confirm whether the user is using sample data or existing data.
7. For external or personal Microsoft accounts, guide the user through acquiring a frontend token before frontend/data-plane commands.

### Collaboration Discovery

Use when user asks what collaborations exist or wants details.
1. Call `managedcleanroom collaborations list`.
2. If user asks for one item, call `managedcleanroom collaborations get` with collaboration ID.
3. Return concise summary and key identifiers for next actions.

### How Do I Use ACCR

Use when the user asks how to use ACCR broadly or is not yet sure whether they want to add data, approve queries, execute queries, or inspect results.
1. Briefly explain the main user journeys:
	- Add dataset information to a collaboration.
	- Publish or approve query definitions.
	- Execute an approved query.
	- Inspect runs, results, consent, and audit history.
2. Ask what the user wants to do first and route them to the corresponding playbook.
3. If the user wants to work with data, continue with the dataset onboarding or sample data workflow.
4. If the user wants to work with queries, continue with the query approval or query execution workflow.

### Data Readiness Workflow

Use when the user asks how to make their data ready for ACCR collaboration.
1. Explain the supported storage and data shapes used by this toolset:
	- Storage: Azure Blob Storage dataset documents.
	- File formats: `csv`, `parquet`, `json`.
	- Encryption modes: `SSE` and customer-provided key mode (`CPK`).
2. Ask whether the user already has data uploaded or needs help preparing sample/test data.
3. If the user chooses `CPK`, explain that they will also need managed identity access, Key Vault key material, OIDC setup, federation, and SKR-related steps.
4. Offer to continue with either the sample data workflow or the dataset onboarding workflow.

### Sample/Test Data Workflow

Use when the user wants help generating or preparing test/sample data before dataset publication.
1. Ask the user to choose an encryption mode: `SSE` or `CPK`.
2. If the user has local sample data already, help them prepare the target storage account, container, and optional Key Vault inputs.
3. If the user does not have data yet, offer to help them generate sample data first and then continue with dataset onboarding.
4. Explain that for `CPK`, the agent should also help gather or create the Key Vault key inputs needed later for secure key release.
5. After the data is ready, continue with the dataset onboarding workflow.

### Frontend Dataset Onboarding Workflow

Use when the user wants to add dataset information to a clean room or publish dataset metadata.
1. Confirm the identity and token path needed for frontend calls:
	- Microsoft work or school account: use Azure-authenticated token flow.
	- Personal Microsoft account: use the frontend token flow supported by the data-plane commands.
2. Call `managedcleanroom collaborations list` to show all collaborations the user can access.
3. Ask the user to select the target collaboration.
4. If the invitation has not been accepted yet, call `managedcleanroom invitations list` and then `managedcleanroom invitations accept` before continuing.
5. Resolve or confirm the frontend endpoint for the selected collaboration region.
6. Collect dataset metadata inputs:
	- dataset name / document ID
	- encryption mode
	- storage account URL and container name
	- storage account type
	- managed identity details if applicable
	- Key Vault / key details if using `CPK`
7. If the user needs help selecting resources, assist them by gathering the available storage account, container, managed identity, Key Vault, and key details from their Azure context.
8. Explain that the user should share the final dataset name with query providers so queries can reference that dataset correctly.

### Query Authoring and Publish Preparation Workflow

Use when the user wants to publish a query, add a query to a clean room, or asks for help composing query content.
1. Ask for the user's email or identity context when needed and guide them through the correct Azure or frontend token flow before frontend commands.
2. Call `managedcleanroom collaborations list` to show accessible collaborations.
3. Ask the user to select the target collaboration.
4. If the invitation has not been accepted yet, call `managedcleanroom invitations list` and then `managedcleanroom invitations accept` before continuing.
5. Resolve or confirm the frontend endpoint for the selected collaboration region.
6. Call `managedcleanroom datasets list` for the selected collaboration so the user can choose the input datasets and confirm the available dataset names.
7. Ask whether the user already has query content prepared:
   - If yes, collect the query text or file content.
   - If not, help the user compose a syntactically correct SQL query based on the business goal and the selected collaborators' datasets.
8. Help the user review and refine the query text, and allow them to edit or approve the final version.
9. Check the query syntax before publish and warn the user if the query appears malformed.
	- There is no dedicated standalone query validation command in the current ManagedCleanroom toolset.
	- Perform a pre-publish review by comparing the query text against the selected dataset names, schema, and allowed fields already gathered in the session.
	- If uncertainty remains, tell the user that final validation will happen at publish or execution time and that service validation errors should be surfaced back exactly.
10. Validate the query against the selected datasets' schema and allowed fields. Warn if the query references fields that are not present or not allowed.
11. Ask the user which dataset should receive the query results.
	- If they do not yet have a suitable output dataset, route them through the dataset publication flow first.
12. Ask whether the query should be segmented into multiple views or execution steps. If the query is large or logically split, recommend segmentation; if one segment is enough, state that clearly.
13. Explain preconditions and postfilters, and help the user decide whether to set them before publication.
14. Ask the user to choose the final query name and confirm it.
15. Publish the query with `managedcleanroom queries publish` using the selected input datasets, output dataset, and composed query body.
16. If the publishing user is also expected to approve the query, continue with the query approval workflow.
17. Tell the user to share the final query name with the relevant dataset owners so they can review and approve it.

### Dataset Validation and Publication Workflow

Use when the user is ready to validate and publish dataset information.
1. Ask the user for the schema inputs needed for validation.
	- For structured schema input, collect field names and field types.
	- For existing schema artifacts, ask the user to provide the CSV or JSON schema content they want validated.
2. Check that the selected data types are compatible with ACCR Spark analytics and warn about unsupported or risky field types.
3. Help the user choose the `allowedFields` set and explain that only those fields are exposed to query processing.
4. If the dataset uses `CPK`, confirm the managed identity, Key Vault, OIDC setup, federation inputs, and secure key release prerequisites before publish.
5. Call `managedcleanroom datasets build-body` first.
6. If validation passes, call `managedcleanroom datasets publish` using the returned `body`.
7. Verify publication with `managedcleanroom datasets get` or `managedcleanroom datasets list`.
8. Explain that the user can withdraw consent for the dataset later if they need to prevent reuse.

### OIDC, Access, and Federation Workflow

Use when the user needs help making encrypted datasets accessible to the clean room runtime.
1. Run the OIDC inspection/setup flow using `managedcleanroom oidc issuer-info`, `managedcleanroom oidc keys`, and `managedcleanroom oidc set-issuer-url` as needed.
2. Confirm the managed identity that will access the dataset.
3. Confirm the identity has the right access on the storage account and, for `CPK`, on the Key Vault.
4. For `CPK`, guide the user through the additional secure key release prerequisites before dataset publication.
5. Continue back into dataset publication after OIDC and access prerequisites are ready.

### Collaboration ARM Resource Management

Use when user asks to create/manage collaboration ARM resources.
1. Use an existing resource group.
2. If the user does not have one, ask them to create a resource group first using the existing command (`arm create-or-update-resource-group`) before continuing.
3. **Region Check**: Confirm the chosen region supports VN2 on C-ACI. Supported regions: `centralindia`, `eastasia`, `eastus`, `eastus2`, `germanywestcentral`, `italynorth`, `japaneast`, `northeurope`, `southcentralus`, `southeastasia`, `switzerlandnorth`, `uaenorth`, `westeurope`, `westus`, `westus2`. If the user's region is not in this list, suggest the nearest supported region and ask them to confirm.
4. **Capacity Check**: Inform the user that the following minimum quota is required in the chosen region before proceeding:
	- AKS node pool: 8 vCPUs (Standard_D4ds_v5 / Ddsv5 family).
	- Confidential ACI: 6 vCPUs (confidential container groups).
	Ask the user to verify quota via the Azure portal or `az vm list-usage` before creating. Do not proceed if the user confirms quota is insufficient.
5. Create: `managedcleanroom collaborationarm create`.
	- Mention that collaborators can be added at creation time via the `--collaborators` parameter, or later using `add-collaborator`.
6. Inspect status/details: `managedcleanroom collaborationarm get`.
7. Add collaborator: `managedcleanroom collaborationarm add-collaborator`.
	- **Users**: identified by email address (`--user-identifier <email>`). After adding, instruct the user to share the collaboration name with the invited person — they must use that name to list and accept their invitation via `managedcleanroom invitations list/accept`.
	- **SPNs** (for automation/CI): require `--user-identifier <appId>`, `--object-id <enterpriseAppObjectId>` (from the Enterprise Application, not the App Registration), and `--tenant-id <tenantId>`. SPNs auto-activate without needing to accept an invitation.
8. Enable workload: `managedcleanroom collaborationarm enable-workload`.
9. Delete only after explicit confirmation: `managedcleanroom collaborationarm delete`.

### How Do I Create an ACCR Instance

Use when user asks how to create a collaboration or ACCR instance for the first time.
1. Explain the three high-level phases:
	- **Phase 1 — Create collaboration ARM resource** (`collaborationarm create`): provisions an AKS-backed confidential clean room. Takes ~25 minutes.
	- **Phase 2 — Enable analytics workload** (`collaborationarm enable-workload`): activates the Spark analytics endpoint. Takes ~7 minutes.
	- **Phase 3 — Add collaborators** (`collaborationarm add-collaborator`): invite users (by email) or SPNs. Can be done at create time or later.
2. Ask: "Would you like me to walk you through this step-by-step, or should I proceed autonomously and make reasonable assumptions?"
	- If step-by-step: follow the "Collaboration ARM Resource Management" playbook, confirming inputs at each step.
	- If autonomous: follow the "Autonomous Clean Room Creation" playbook.

### Autonomous Clean Room Creation

Use when user explicitly says to make all assumptions and create a clean room without being prompted for each value.
1. Derive a collaboration name from the user's stated purpose, or use a descriptive default like `analytics-cleanroom`.
2. Select `westus` as the default region (first in the supported list; valid for VN2 on C-ACI).
3. Use or create a resource group named `<collabName>-rg`.
4. Before executing, state all assumptions clearly: collaboration name, region, resource group, and that collaborators can be added later.
5. Create the collaboration: `managedcleanroom collaborationarm create`.
6. Enable analytics workload: `managedcleanroom collaborationarm enable-workload`.
7. Share all created resource details back to the user: collaboration name, resource group, region, and the workload endpoint once available.

### Disable Workload (Coming Soon)

Use when user asks to disable a workload.
1. Inform the user: "Disabling workloads is not yet supported. This capability is coming soon."
2. Offer `managedcleanroom collaborationarm get` to inspect current workload state if needed.

### Delete/Remove Collaborators (Coming Soon)

Use when user asks to delete or remove a collaborator from a collaboration.
1. Inform the user: "Removing collaborators is not yet supported. This capability is coming soon."
2. Offer `managedcleanroom collaborationarm get` to view current collaborator list.

### OIDC Configuration Workflow

Use when user asks to inspect or set OIDC issuer settings.
1. Inspect issuer metadata: `managedcleanroom oidc issuer-info`.
2. Inspect keys: `managedcleanroom oidc keys`.
3. If user requests change, set issuer URL: `managedcleanroom oidc set-issuer-url`.
4. Re-read issuer metadata to verify update.

### Consent Lifecycle Workflow

Use when user asks to create or update consent documents.
1. Collect endpoint, collaboration ID, document ID, and consent body requirements.
2. Call `managedcleanroom consent put`.
3. Verify outcome by checking returned document state/details.
4. If body validation fails, return exact service error and request corrected JSON.

### Consent Withdrawal Workflow

Use when the user wants to ensure that a dataset or query they approved cannot be used again.
1. Ask whether they want to withdraw consent for a dataset or a query document.
2. Help them identify the candidate document by listing datasets or queries in the selected collaboration.
3. Ask the user to choose the exact document to update.
4. Call `managedcleanroom consent put` with the withdraw or disable action required by the current document type and payload.
5. Verify the updated consent state and explain the effect back to the user.

### Dataset Deletion Guidance

Use when the user asks to delete a dataset.
1. Explain that dataset documents are treated as immutable collaboration documents and are not deleted through the current workflow.
2. Explain that the supported way to prevent reuse is to withdraw or disable consent for that dataset.
3. Offer to continue with the consent withdrawal workflow.

### Dataset Publish Workflow

Use when user wants to publish dataset documents safely.
1. Collect dataset payload inputs.
2. Call `managedcleanroom datasets build-body` first.
3. If validation passes, take returned `body`.
4. Call `managedcleanroom datasets publish` using that `body`.
5. Verify with `managedcleanroom datasets get` or `managedcleanroom datasets list`.

### Query Execution Workflow

Use when user wants to publish or execute queries.
1. If collaboration context is not already known, call `managedcleanroom collaborations list` and ask the user to select the target collaboration.
2. Confirm the frontend endpoint and user token path for the selected collaboration.
3. Call `managedcleanroom queries list` and ask the user to select the query to run.
4. If the user already provided a query name, confirm that it exists by matching it against `managedcleanroom queries list`, then show its details with `managedcleanroom queries get`.
5. Check that the selected query is in an accepted state before running it.
6. Warn the user that inbound network connectivity or tenant networking restrictions can block execution, especially if previous runs stayed submitted or failed to start.
7. Ask whether they want to execute the query for a specific date range. If yes, collect the date range inputs before execution.
8. Start run: `managedcleanroom queries run`.
9. Return and highlight the resulting job ID.
10. Inspect query run history: `managedcleanroom queries runs`.
11. Poll run status by job ID with `managedcleanroom runs get`.

### Query Approval Workflow

Use when the user wants to approve a query.
1. Call `managedcleanroom queries list` to show the available query documents.
2. Ask the user to select the target query.
3. Call `managedcleanroom queries get` to display the query details before approval.
4. Warn the user explicitly if preconditions or postfilters appear to be missing or empty.
5. Ask the user to confirm approval after showing the query details and warnings.
6. Call `managedcleanroom queries vote` for the selected query.
7. Verify the updated state with `managedcleanroom queries get`.

### Query Run Details and Statistics Workflow

Use when the user asks for execution details of a query or wants run statistics.
1. Confirm the collaboration and query name.
2. Call `managedcleanroom queries list` if needed to confirm the query exists.
3. Call `managedcleanroom queries runs` to retrieve the query's run history.
4. Summarize the latest or selected runs, including status, duration, and available run statistics.
5. If the user asked about a particular execution, point them to the relevant job ID and continue with `managedcleanroom runs get` when detailed status is needed.

### Run Monitoring Workflow

Use when user asks for run state/progress.
1. Call `managedcleanroom runs get` with job ID.
2. Surface terminal status or latest progress.
3. If failed, include error context and next diagnostic step.
4. Default to checking progress every 30 seconds for active runs unless the user asks for a different cadence.
5. If the user wants more or less frequent updates, adjust the polling cadence accordingly.

### Query Output Retrieval Workflow

Use when the user wants to see where query output is written or wants help retrieving it.
1. Explain that query results are written to the user-owned output dataset location, not stored inside the clean room itself.
2. Help the user identify the output dataset configured for the query.
3. Call `managedcleanroom queries get` and `managedcleanroom datasets get` as needed to confirm which output dataset document and storage location are associated with the query.
4. For `SSE` mode, guide the user to the customer-owned output storage location so they can inspect or download the results using their normal storage tooling.
5. For `CPK` mode, explain that they must use the customer-controlled decryption and download path for that storage account and key setup to retrieve the results locally.
6. There is no dedicated output-download command in the current ManagedCleanroom MCP command surface.
	- Be explicit that the agent should help the user locate the output dataset and storage path first.
	- Then guide the user to use their existing storage download workflow outside the ManagedCleanroom command set when local file retrieval is needed.

### Output Analysis Workflow

Use when the user wants to analyze the produced query output.
1. Confirm that the query output has been downloaded or is accessible in the output storage location.
2. Help the user inspect the result structure and summarize the key findings.
3. If the user wants further analysis, help derive insights from the output or suggest a simple dashboard-oriented summary.
4. If the user has not yet retrieved the result data, route them back to the query output retrieval workflow first rather than speculating about unseen output.
5. Keep the analysis grounded in the actual output the user provides or confirms.

### Can I View Output Data In The Clean Room

Use when the user asks whether the clean room stores or displays the output data directly.
1. Explain that the clean room does not act as a long-term data store for customer datasets or output results.
2. Explain that output data must be retrieved from the customer-owned output storage location.
3. Offer to continue with the query output retrieval workflow.

### Audit and Compliance Workflow

Use when user asks for traceability.
1. Call `managedcleanroom auditevents list`.
2. Apply scope/fromSeqno/toSeqno filters when provided.
3. Summarize event type, actor, and sequence range.
4. If the user asks for a time window such as the last 24 hours and direct timeframe filtering is not available, explain the limitation and guide them to the closest supported filtering approach.

## Verification Matrix

After mutating commands, run a read-back verification command.
1. `collaborationarm create` -> verify with `collaborationarm get`.
2. `collaborationarm add-collaborator` -> verify with collaboration read/list path available to user context.
3. `collaborationarm enable-workload` -> verify with `collaborationarm get` and `analytics get` when applicable.
4. `datasets publish` -> verify with `datasets get` or `datasets list`.
5. `queries publish` -> verify with `queries get` or `queries list`.
6. `queries vote` -> verify with `queries get`.
7. `queries run` -> verify with `runs get` and `queries runs`.
8. `consent put` -> verify from returned object and follow-up read if available in current flow.

## Decision Tree: collaborationarm vs collaborations

Use this routing rule in mixed scenarios.
1. If user asks about Azure ARM resource lifecycle (create/delete/recover/workload enable/kubeconfig), use `collaborationarm`.
2. If user asks about frontend collaboration business objects, list/get details, use `collaborations`.
3. If request mentions both and order is unclear:
4. Start with `collaborationarm get` to confirm resource existence.
5. Then use `collaborations get` for frontend state/details.

## Validation and Guardrails

1. Always validate required options before execution.
2. Do not fabricate command output.
3. If service returns a validation error, surface it directly.
4. Use retries only for transient failures.
5. For frontend/data-plane calls, always pass `allow-untrusted-cert=true`.
6. Do not expose secrets or sensitive tokens in responses.

## Error Handling Policy

1. Missing required input: ask a single precise follow-up question.
2. Validation failure: return exact reason and required correction.
3. Not found: confirm identifiers and suggest list/get verification.
4. Auth/RBAC failure: request correct tenant/subscription/context.
5. Service errors: report key message and safest next command.

## Failure Recovery Playbooks

### Workload Enable Failure

1. Re-check target using `collaborationarm get`.
2. Inspect analytics state with `analytics get` when available.
3. Report exact error category (validation/auth/service/transient).
4. Retry once only for transient network/service failures.
5. If still failing, provide minimal repro inputs and stop automatic retries.

### Dataset Publish Failure

1. Rebuild payload with `datasets build-body` to catch structural issues.
2. Re-run `datasets publish` with verified body.
3. If failure persists, call `datasets get`/`list` to check partial state.
4. Return exact error and field-level correction guidance.

### Query Run Failure

1. Check query definition with `queries get`.
2. Check run history with `queries runs`.
3. Poll latest run with `runs get`.
4. Distinguish validation error vs runtime/service error and guide next action.
5. If the failure appears to happen before execution starts, remind the user to verify query accepted status, networking prerequisites, and output dataset configuration.

## Response Behavior

1. Explain what command was run and why.
2. Provide outcome first, then next actionable step.
3. Keep output concise and operational.
4. For multi-step workflows, show current step and next step only.

## Command Sequencing Principles

1. Prefer read/validate actions before write actions.
2. For datasets, use build-before-publish sequence.
3. For queries, publish then run then poll.
4. Verify writes with a read command where possible.

## Output Contract

For each completed step, return:
1. Command group and operation executed.
2. Outcome summary (success/failure + key identifiers).
3. Verification status for write operations.
4. Next recommended command.

## Reference Commands

Common command chains used by this skill:
1. Dataset publish chain: `datasets build-body` -> `datasets publish` -> `datasets get|list`.
2. Query run chain: `queries publish` -> `queries run` -> `runs get`.
3. Collaboration ARM chain: `collaborationarm create` -> `collaborationarm get` -> optional `add-collaborator`/`enable-workload`.

## References

Primary references for this skill:
1. Azure MCP command reference in `servers/Azure.Mcp.Server/docs/azmcp-commands.md`.
2. ManagedCleanroom command and service implementation under `tools/Azure.Mcp.Tools.ManagedCleanroom/src/`.
3. ManagedCleanroom tests under `tools/Azure.Mcp.Tools.ManagedCleanroom/tests/`.
