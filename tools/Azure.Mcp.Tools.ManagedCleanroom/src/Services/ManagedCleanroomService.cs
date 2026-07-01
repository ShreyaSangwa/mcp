// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using AnalyticsFrontendAPI;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager;
using Azure.ResourceManager.CleanRoom;
using Azure.ResourceManager.CleanRoom.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Options;
namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

public class ManagedCleanroomService(ISubscriptionService subscriptionService, ITenantService tenantService, IHttpClientFactory httpClientFactory)
    : BaseAzureResourceService(subscriptionService, tenantService), IManagedCleanroomService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private static readonly TimeSpan WorkloadEndpointTimeout = TimeSpan.FromMinutes(15);

    // A stable empty-string JsonElement to represent an empty raw response body.
    private static readonly JsonElement EmptyStringElement =
        JsonDocument.Parse("\"\"").RootElement.Clone();
    private static readonly TimeSpan WorkloadHealthTimeout = TimeSpan.FromMinutes(10);

    public async Task<JsonElement> ListCollaborationsAsync(
        string endpoint,
        bool? activeOnly = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.GetGetsAsync(activeOnly, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetCollaborationAsync(
        string endpoint,
        string collaborationId,
        bool? includeDeleted = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.IdGetAsync(collaborationId, includeDeleted, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetAnalyticsAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsGetAsync(collaborationId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetAnalyticsSkrPolicyAsync(
        string endpoint,
        string collaborationId,
        string kid,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(kid), kid));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsSkrPolicyGetAsync(collaborationId, kid, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> ListInvitationsAsync(
        string endpoint,
        string collaborationId,
        bool? pendingOnly = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.InvitationsGetAsync(collaborationId, pendingOnly, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> AcceptInvitationAsync(
        string endpoint,
        string collaborationId,
        string invitationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(invitationId), invitationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.InvitationIdAcceptPostAsync(collaborationId, invitationId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetOidcIssuerInfoAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.OidcIssuerInfoGetAsync(collaborationId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetOidcKeysAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.OidcKeysGetAsync(collaborationId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> SetOidcIssuerUrlAsync(
        string endpoint,
        string collaborationId,
        string issuerUrl,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(issuerUrl), issuerUrl));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        // Build the request body expected by the frontend API: {"url": "<value>"}
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("url", issuerUrl);
            writer.WriteEndObject();
            writer.Flush();
        }

        var content = RequestContent.Create(buffer.WrittenMemory);
        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.OidcSetIssuerUrlPostAsync(collaborationId, content, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> PublishDatasetAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        string? body = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestBody = await ResolveBodyContentAsync(body, cancellationToken).ConfigureAwait(false);
        var content = RequestContent.Create(BinaryData.FromString(requestBody));
        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsDatasetsDocumentIdPublishPostAsync(
            collaborationId, documentId, content, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    private static async Task<string> ResolveBodyContentAsync(string? body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body))
            return "{}";

        // Match CLI behavior where @path reads content from a file.
        if (body.StartsWith("@@", StringComparison.Ordinal))
            return body[1..];

        if (!body.StartsWith("@", StringComparison.Ordinal))
            return body;

        var filePath = body[1..].Trim();
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Body file path cannot be empty when using @file syntax.", nameof(body));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Body file '{filePath}' was not found.", filePath);

        return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
    }

    public async Task<JsonElement> GetDatasetAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsDatasetsDocumentIdGetAsync(
            collaborationId, documentId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> PutConsentAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        string? body = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestBody = await ResolveBodyContentAsync(body, cancellationToken).ConfigureAwait(false);
        var content = RequestContent.Create(BinaryData.FromString(requestBody));
        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.ConsentDocumentIdPutAsync(
            collaborationId, documentId, content, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> ListDatasetsAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsDatasetsListGetAsync(collaborationId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

        public Task<JsonElement> BuildDatasetBodyAsync(
        string datasetName,
        string containerName,
        string storageAccountUrl,
        string[] schemaFields,
        string[] allowedFields,
        string? format = null,
        string? accessMode = null,
        string? encryptionMode = null,
        string? storageAccountType = null,
        string? subdirectory = null,
        string? cpkKeyVaultUrl = null,
        string? cpkKeyName = null,
        string? cpkKeyVersion = null,
        string? additionalStoreJson = null,
        string? identityName = null,
        string? identityClientId = null,
        string? identityTenantId = null,
        string? identityIssuerUrl = null,
        string? dekKeyVaultUrl = null,
        string? dekSecretId = null,
        string? kekKeyVaultUrl = null,
        string? kekSecretId = null,
        string? maaUrl = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ValidateRequiredParameters(
            (nameof(datasetName), datasetName),
            (nameof(containerName), containerName),
            (nameof(storageAccountUrl), storageAccountUrl));

        if (!Uri.TryCreate(storageAccountUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("storageAccountUrl must be a valid absolute URI.", nameof(storageAccountUrl));
        }

        if (schemaFields is null || schemaFields.Length == 0)
        {
            throw new ArgumentException("At least one schema field is required.", nameof(schemaFields));
        }

        if (allowedFields is null || allowedFields.Length == 0)
        {
            throw new ArgumentException("At least one allowed field is required.", nameof(allowedFields));
        }

        var hasAnyIdentity =
            !string.IsNullOrWhiteSpace(identityName) ||
            !string.IsNullOrWhiteSpace(identityClientId) ||
            !string.IsNullOrWhiteSpace(identityTenantId) ||
            !string.IsNullOrWhiteSpace(identityIssuerUrl);

        if (hasAnyIdentity &&
            (string.IsNullOrWhiteSpace(identityName) ||
             string.IsNullOrWhiteSpace(identityClientId) ||
             string.IsNullOrWhiteSpace(identityTenantId) ||
             string.IsNullOrWhiteSpace(identityIssuerUrl)))
        {
            throw new ArgumentException(
                "Identity settings must include name, clientId, tenantId, and issuerUrl when any identity value is provided.",
                nameof(identityName));
        }

        var normalizedFormat = string.IsNullOrWhiteSpace(format) ? "csv" : format.Trim().ToLowerInvariant();
        var normalizedAccessMode = string.IsNullOrWhiteSpace(accessMode) ? "read" : accessMode.Trim().ToLowerInvariant();
        var normalizedStorageAccountType = string.IsNullOrWhiteSpace(storageAccountType)
            ? "Azure_BlobStorage"
            : storageAccountType.Trim();

        var normalizedEncryptionMode = string.IsNullOrWhiteSpace(encryptionMode) ? "SSE" : encryptionMode.Trim().ToUpperInvariant();
        if (normalizedEncryptionMode == "CPK")
        {
            // The frontend payload commonly uses CSE for customer-provided keys.
            normalizedEncryptionMode = "CSE";
        }

        var hasAnyKeyBlocks =
            !string.IsNullOrWhiteSpace(dekKeyVaultUrl) ||
            !string.IsNullOrWhiteSpace(dekSecretId) ||
            !string.IsNullOrWhiteSpace(kekKeyVaultUrl) ||
            !string.IsNullOrWhiteSpace(kekSecretId) ||
            !string.IsNullOrWhiteSpace(maaUrl);

        if (hasAnyKeyBlocks &&
            (string.IsNullOrWhiteSpace(dekKeyVaultUrl) ||
             string.IsNullOrWhiteSpace(dekSecretId) ||
             string.IsNullOrWhiteSpace(kekKeyVaultUrl) ||
             string.IsNullOrWhiteSpace(kekSecretId) ||
             string.IsNullOrWhiteSpace(maaUrl)))
        {
            throw new ArgumentException(
                "Key blocks require dekKeyVaultUrl, dekSecretId, kekKeyVaultUrl, kekSecretId, and maaUrl when any key value is provided.",
                nameof(dekKeyVaultUrl));
        }

        if (hasAnyKeyBlocks && normalizedEncryptionMode != "CSE")
        {
            throw new ArgumentException(
                "dek/kek key blocks are only valid when encryptionMode is CPK/CSE.",
                nameof(encryptionMode));
        }

        var schemaFieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var schemaNodes = new JsonArray();
        foreach (var schemaField in schemaFields)
        {
            var fieldParts = (schemaField ?? string.Empty).Split(':', 2, StringSplitOptions.TrimEntries);
            if (fieldParts.Length != 2 || string.IsNullOrWhiteSpace(fieldParts[0]) || string.IsNullOrWhiteSpace(fieldParts[1]))
            {
                throw new ArgumentException(
                    $"Schema field '{schemaField}' is invalid. Use the format '<fieldName>:<fieldType>'.",
                    nameof(schemaFields));
            }

            if (!schemaFieldNames.Add(fieldParts[0]))
            {
                throw new ArgumentException($"Duplicate schema field '{fieldParts[0]}' is not allowed.", nameof(schemaFields));
            }

            schemaNodes.Add((JsonNode)new JsonObject
            {
                ["fieldName"] = fieldParts[0],
                ["fieldType"] = fieldParts[1]
            });
        }

        var allowedNodes = new JsonArray();
        foreach (var allowedField in allowedFields)
        {
            if (string.IsNullOrWhiteSpace(allowedField))
            {
                throw new ArgumentException("allowedFields cannot contain empty values.", nameof(allowedFields));
            }

            if (!schemaFieldNames.Contains(allowedField))
            {
                throw new ArgumentException(
                    $"Allowed field '{allowedField}' is not present in schemaFields.",
                    nameof(allowedFields));
            }

            allowedNodes.Add((JsonNode?)JsonValue.Create(allowedField));
        }

        var store = new JsonObject
        {
            ["containerName"] = containerName,
            ["storageAccountUrl"] = storageAccountUrl,
            ["storageAccountType"] = normalizedStorageAccountType,
            ["encryptionMode"] = normalizedEncryptionMode
        };

        if (!string.IsNullOrWhiteSpace(subdirectory))
        {
            store["subdirectory"] = subdirectory.Trim();
        }

        if (!string.IsNullOrWhiteSpace(cpkKeyVaultUrl))
        {
            store["cpkKeyVaultUrl"] = cpkKeyVaultUrl.Trim();
        }

        if (!string.IsNullOrWhiteSpace(cpkKeyName))
        {
            store["cpkKeyName"] = cpkKeyName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(cpkKeyVersion))
        {
            store["cpkKeyVersion"] = cpkKeyVersion.Trim();
        }

        if (!string.IsNullOrWhiteSpace(additionalStoreJson))
        {
            JsonObject? additionalStore;
            try
            {
                additionalStore = JsonNode.Parse(additionalStoreJson) as JsonObject;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("additionalStoreJson must be a valid JSON object string.", nameof(additionalStoreJson), ex);
            }

            if (additionalStore is null)
            {
                throw new ArgumentException("additionalStoreJson must be a JSON object.", nameof(additionalStoreJson));
            }

            foreach (var kvp in additionalStore)
            {
                store[kvp.Key] = kvp.Value?.DeepClone();
            }
        }

        var dataset = new JsonObject
        {
            ["name"] = datasetName,
            ["store"] = store,
            ["datasetSchema"] = new JsonObject
            {
                ["format"] = normalizedFormat,
                ["fields"] = schemaNodes
            },
            ["datasetAccessPolicy"] = new JsonObject
            {
                ["accessMode"] = normalizedAccessMode,
                ["allowedFields"] = allowedNodes
            }
        };

        if (hasAnyIdentity)
        {
            dataset["identity"] = new JsonObject
            {
                ["name"] = identityName!.Trim(),
                ["clientId"] = identityClientId!.Trim(),
                ["tenantId"] = identityTenantId!.Trim(),
                ["issuerUrl"] = identityIssuerUrl!.Trim()
            };
        }

        if (hasAnyKeyBlocks)
        {
            dataset["dek"] = new JsonObject
            {
                ["keyVaultUrl"] = dekKeyVaultUrl!.Trim(),
                ["secretId"] = dekSecretId!.Trim()
            };

            dataset["kek"] = new JsonObject
            {
                ["keyVaultUrl"] = kekKeyVaultUrl!.Trim(),
                ["secretId"] = kekSecretId!.Trim(),
                ["maaUrl"] = maaUrl!.Trim()
            };
        }

        var compactBody = dataset.ToJsonString(new JsonSerializerOptions { WriteIndented = false });

        var result = new JsonObject
        {
            ["body"] = compactBody,
            ["dataset"] = dataset,
            ["normalization"] = new JsonObject
            {
                ["format"] = normalizedFormat,
                ["accessMode"] = normalizedAccessMode,
                ["storageAccountType"] = normalizedStorageAccountType,
                ["encryptionMode"] = normalizedEncryptionMode
            }
        };

        return Task.FromResult(result.Deserialize(ManagedCleanroomSerializerContext.Default.JsonElement));
    }

    public async Task<JsonElement> PublishQueryAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        string? body = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestBody = await ResolveBodyContentAsync(body, cancellationToken).ConfigureAwait(false);
        var content = RequestContent.Create(BinaryData.FromString(requestBody));
        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsQueriesDocumentIdPublishPostAsync(
            collaborationId, documentId, content, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> BuildQueryBodyAsync(
        string queryName,
        string queryDirectory,
        string outputDataset,
        string? publisherInputDataset = null,
        string? consumerInputDataset = null,
        string? inputDatasetMappings = null,
        string? outputDatasetAlias = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ValidateRequiredParameters(
            (nameof(queryName), queryName),
            (nameof(queryDirectory), queryDirectory),
            (nameof(outputDataset), outputDataset));

        if (!Directory.Exists(queryDirectory))
        {
            throw new ArgumentException($"Query directory '{queryDirectory}' was not found.", nameof(queryDirectory));
        }

        // Resolve input dataset mappings: use custom JSON if provided, else fall back to publisher/consumer pair.
        var inputDatasetMappingsDict = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(inputDatasetMappings))
        {
            // Parse custom mappings from JSON.
            try
            {
                var mappingsJson = JsonDocument.Parse(inputDatasetMappings).RootElement;
                if (mappingsJson.ValueKind != JsonValueKind.Object)
                {
                    throw new ArgumentException("Input dataset mappings must be a JSON object.");
                }

                foreach (var property in mappingsJson.EnumerateObject())
                {
                    var datasetId = property.Name;
                    if (property.Value.ValueKind != JsonValueKind.String)
                    {
                        throw new ArgumentException($"Alias for dataset '{datasetId}' must be a string.");
                    }

                    var alias = property.Value.GetString();
                    if (string.IsNullOrWhiteSpace(alias))
                    {
                        throw new ArgumentException($"Alias for dataset '{datasetId}' cannot be empty.");
                    }

                    inputDatasetMappingsDict[datasetId.Trim()] = alias.Trim();
                }
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Failed to parse input dataset mappings JSON: {ex.Message}", ex);
            }

            if (inputDatasetMappingsDict.Count == 0)
            {
                throw new ArgumentException("Input dataset mappings JSON must contain at least one dataset.");
            }
        }
        else
        {
            // Fall back to publisher/consumer pattern.
            if (string.IsNullOrWhiteSpace(publisherInputDataset) || string.IsNullOrWhiteSpace(consumerInputDataset))
            {
                throw new ArgumentException(
                    "Either provide --input-dataset-mappings or both --publisher-input-dataset and --consumer-input-dataset.");
            }

            inputDatasetMappingsDict[publisherInputDataset.Trim()] = "publisher_data";
            inputDatasetMappingsDict[consumerInputDataset.Trim()] = "consumer_data";
        }

        var normalizedOutputAlias = string.IsNullOrWhiteSpace(outputDatasetAlias) ? "output" : outputDatasetAlias.Trim();

        var jsonSegments = Directory.GetFiles(queryDirectory, "segment*.json").OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        var txtSegments = Directory.GetFiles(queryDirectory, "segment*.txt").OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();

        if (jsonSegments.Length == 0 && txtSegments.Length == 0)
        {
            throw new ArgumentException(
                $"No segment*.json or segment*.txt files found in '{queryDirectory}'.",
                nameof(queryDirectory));
        }

        var queryDataNodes = new JsonArray();
        var sourceFormat = jsonSegments.Length > 0 ? "json" : "txt";

        if (jsonSegments.Length > 0)
        {
            foreach (var segmentPath in jsonSegments)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var rawSegment = await File.ReadAllTextAsync(segmentPath, cancellationToken).ConfigureAwait(false);

                JsonElement segment;
                try
                {
                    segment = JsonDocument.Parse(rawSegment).RootElement;
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException($"Segment file '{segmentPath}' does not contain valid JSON.", nameof(queryDirectory), ex);
                }

                if (!segment.TryGetProperty("data", out var dataElement) || dataElement.ValueKind != JsonValueKind.String)
                {
                    throw new ArgumentException($"Segment file '{segmentPath}' must include a string 'data' property.", nameof(queryDirectory));
                }

                if (!segment.TryGetProperty("executionSequence", out var executionSequenceElement) || !executionSequenceElement.TryGetInt32(out var executionSequence))
                {
                    throw new ArgumentException($"Segment file '{segmentPath}' must include an integer 'executionSequence' property.", nameof(queryDirectory));
                }

                var data = dataElement.GetString();
                if (string.IsNullOrWhiteSpace(data))
                {
                    throw new ArgumentException($"Segment file '{segmentPath}' has an empty 'data' value.", nameof(queryDirectory));
                }

                var preConditions = TryGetStringProperty(segment, "preConditions") ?? string.Empty;
                var postFilters = TryGetStringProperty(segment, "postFilters") ?? string.Empty;

                queryDataNodes.Add((JsonNode)new JsonObject
                {
                    ["data"] = data,
                    ["executionSequence"] = executionSequence,
                    ["preConditions"] = preConditions,
                    ["postFilters"] = postFilters
                });
            }
        }
        else
        {
            // Keep parity with the sample script behavior: sequence defaults to 1 unless overridden by '-- seq=N' per file.
            var sequence = 1;
            foreach (var segmentPath in txtSegments)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var sql = (await File.ReadAllTextAsync(segmentPath, cancellationToken).ConfigureAwait(false)).Trim();
                if (string.IsNullOrWhiteSpace(sql))
                {
                    throw new ArgumentException($"Segment file '{segmentPath}' is empty.", nameof(queryDirectory));
                }

                var match = Regex.Match(sql, @"^--\s*seq\s*=\s*(\d+)", RegexOptions.CultureInvariant);
                if (match.Success)
                {
                    sequence = int.Parse(match.Groups[1].Value);
                    sql = Regex.Replace(sql, @"^--\s*seq\s*=\s*\d+\s*\r?\n?", string.Empty, RegexOptions.CultureInvariant).Trim();
                }

                queryDataNodes.Add((JsonNode)new JsonObject
                {
                    ["executionSequence"] = sequence,
                    ["data"] = sql,
                    ["preConditions"] = string.Empty,
                    ["postFilters"] = string.Empty
                });
            }
        }

        // Build inputDatasets string in the format "datasetId1:alias1,datasetId2:alias2".
        var inputDatasetsPairs = inputDatasetMappingsDict.Select(kvp => $"{kvp.Key}:{kvp.Value}");
        var inputDatasetsString = string.Join(",", inputDatasetsPairs);

        var query = new JsonObject
        {
            ["inputDatasets"] = inputDatasetsString,
            ["outputDataset"] = $"{outputDataset}:{normalizedOutputAlias}",
            ["queryData"] = queryDataNodes
        };

        var compactBody = query.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        var result = new JsonObject
        {
            ["body"] = compactBody,
            ["query"] = query,
            ["recommendedDocumentId"] = queryName,
            ["normalization"] = new JsonObject
            {
                ["queryName"] = queryName,
                ["segmentSource"] = sourceFormat,
                ["segmentCount"] = queryDataNodes.Count,
                ["inputDatasetCount"] = inputDatasetMappingsDict.Count,
                ["outputDatasetAlias"] = normalizedOutputAlias
            }
        };

        return result.Deserialize(ManagedCleanroomSerializerContext.Default.JsonElement);
    }

    private static string? TryGetStringProperty(JsonElement node, string name)
    {
        if (!node.TryGetProperty(name, out var element))
        {
            return null;
        }

        return element.ValueKind == JsonValueKind.String ? element.GetString() : null;
    }

    public async Task<JsonElement> GetQueryAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsQueriesDocumentIdGetAsync(
            collaborationId, documentId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> ListQueriesAsync(
        string endpoint,
        string collaborationId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsQueriesListGetAsync(collaborationId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> VoteOnQueryAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        string? body = null,
        string? vote = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        if (string.IsNullOrWhiteSpace(body) && string.IsNullOrWhiteSpace(vote))
        {
            throw new ArgumentException("Either body or vote must be provided for query vote.");
        }

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        string requestBody;
        if (!string.IsNullOrWhiteSpace(body))
        {
            requestBody = await ResolveBodyContentAsync(body, cancellationToken).ConfigureAwait(false);
        }
        else if (!string.IsNullOrWhiteSpace(vote))
        {
            // Backward compatibility for legacy --vote input.
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                writer.WriteStartObject();
                writer.WriteString("vote", vote);
                writer.WriteEndObject();
                writer.Flush();
            }

            requestBody = BinaryData.FromBytes(buffer.WrittenMemory.ToArray()).ToString();
        }
        else
        {
            requestBody = "{}";
        }

        var content = RequestContent.Create(BinaryData.FromString(requestBody));
        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsQueriesDocumentIdVotePostAsync(
            collaborationId, documentId, content, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> RunQueryAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        string? body = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestBody = await ResolveBodyContentAsync(body, cancellationToken).ConfigureAwait(false);
        var content = RequestContent.Create(BinaryData.FromString(requestBody));
        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsQueriesDocumentIdRunPostAsync(
            collaborationId, documentId, content, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetQueryRunsAsync(
        string endpoint,
        string collaborationId,
        string documentId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(documentId), documentId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsQueriesDocumentIdRunsGetAsync(
            collaborationId, documentId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> GetRunStatusAsync(
        string endpoint,
        string collaborationId,
        string jobId,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(collaborationId), collaborationId),
            (nameof(jobId), jobId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsRunsJobIdGetAsync(
            collaborationId, jobId, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    public async Task<JsonElement> ListAuditEventsAsync(
        string endpoint,
        string collaborationId,
        string? scope = null,
        string? fromSeqno = null,
        string? toSeqno = null,
        bool allowUntrustedCert = false,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(collaborationId), collaborationId));

        var client = await BuildClientAsync(endpoint, allowUntrustedCert, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.AnalyticsAuditeventsGetAsync(
            collaborationId, scope, fromSeqno, toSeqno, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }


    public async Task<JsonElement> AddCollaboratorAsync(
        string name,
        string resourceGroup,
        string subscription,
        string collaboratorUserIdentifier,
        string? collaboratorObjectId = null,
        string? collaboratorTenantId = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(collaboratorUserIdentifier), collaboratorUserIdentifier));

        var collaborationResource = await GetCollaborationResourceAsync(
            name, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var content = new Azure.ResourceManager.CleanRoom.Models.AddCollaboratorContent
        {
            UserIdentifier = collaboratorUserIdentifier,
            ObjectId = collaboratorObjectId,
            TenantId = collaboratorTenantId
        };

        var operation = await collaborationResource
            .AddCollaboratorAsync(WaitUntil.Started, content, cancellationToken)
            .ConfigureAwait(false);

        return ParseResponse(operation.GetRawResponse());
    }

    public async Task<JsonElement> EnableWorkloadAsync(
        string name,
        string resourceGroup,
        string subscription,
        string workloadType,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(workloadType), workloadType));

        var collaborationResource = await GetCollaborationResourceAsync(
            name, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        if (string.Equals(workloadType, "Analytics", StringComparison.OrdinalIgnoreCase))
        {
            workloadType = "AnalyticsStrict";
        }

        var wlType = (Azure.ResourceManager.CleanRoom.Models.WorkloadType)Enum.Parse(
            typeof(Azure.ResourceManager.CleanRoom.Models.WorkloadType), workloadType, ignoreCase: true);

        var content = new Azure.ResourceManager.CleanRoom.Models.EnableWorkloadContent(wlType);

        // WaitUntil.Started — return immediately after the request is accepted.
        var operation = await collaborationResource
            .EnableWorkloadAsync(WaitUntil.Started, content, cancellationToken)
            .ConfigureAwait(false);

        return ParseResponse(operation.GetRawResponse());
    }

    private async Task<CollaborationResource> GetCollaborationResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var subscriptionResource = await _subscriptionService
            .GetSubscription(subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var resourceId = CollaborationResource.CreateResourceIdentifier(
            subscriptionResource.Id.SubscriptionId!, resourceGroup, name);

        return armClient.GetCollaborationResource(resourceId);
    }

    /// <summary>
    /// Extracts the calling user's email/UPN from the preferred_username or upn claim in the ARM access token.
    /// </summary>
    private async Task<string> GetCallerEmailAsync(string? tenant, CancellationToken cancellationToken)
    {
        var token = await GetArmAccessTokenAsync(tenant, cancellationToken).ConfigureAwait(false);

        // JWT is three base64url-encoded segments separated by dots. Decode the payload (second segment).
        var parts = token.Token.Split('.');
        if (parts.Length < 2)
            return string.Empty;

        // Base64url decode: replace URL-safe chars and pad to a multiple of 4.
        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
        var bytes = Convert.FromBase64String(payload);

        using var doc = JsonDocument.Parse(bytes);
        var root = doc.RootElement;

        if (root.TryGetProperty("preferred_username", out var pref) && pref.ValueKind == JsonValueKind.String)
            return pref.GetString() ?? string.Empty;

        if (root.TryGetProperty("upn", out var upn) && upn.ValueKind == JsonValueKind.String)
            return upn.GetString() ?? string.Empty;

        return string.Empty;
    }

    public async Task<JsonElement> GetCollaborationArmResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var collaborationResource = await GetCollaborationResourceAsync(
            name, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var response = await collaborationResource.GetAsync(cancellationToken).ConfigureAwait(false);
        return ParseResponse(response.GetRawResponse());
    }

    public async Task<JsonElement> GetCollaborationReadonlyKubeconfigAsync(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var collaborationResource = await GetCollaborationResourceAsync(
            name, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var response = await collaborationResource
            .GetReadonlyKubeConfigAsync(cancellationToken)
            .ConfigureAwait(false);

        return ParseResponse(response.GetRawResponse());
    }

    public async Task<JsonElement> RecoverCollaborationArmResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var collaborationResource = await GetCollaborationResourceAsync(
            name, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var operation = await collaborationResource
            .RecoverAsync(WaitUntil.Started, new RecoverCollaborationContent(forceRecover: true), cancellationToken)
            .ConfigureAwait(false);

        return ParseResponse(operation.GetRawResponse());
    }

    public async Task<JsonElement> DeleteCollaborationArmResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var collaborationResource = await GetCollaborationResourceAsync(
            name, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var operation = await collaborationResource
            .DeleteAsync(WaitUntil.Started, cancellationToken)
            .ConfigureAwait(false);

        return ParseResponse(operation.GetRawResponse());
    }

    public async Task<CollaborationCreateResult> CreateCollaborationArmResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string location,
        string? resourceLocation = null,
        string[]? collaborators = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(location), location));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var subscriptionResource = await _subscriptionService
            .GetSubscription(subscription, tenant, retryPolicy, cancellationToken)
            .ConfigureAwait(false);

        var resourceGroupId = ResourceGroupResource.CreateResourceIdentifier(
            subscriptionResource.Id.SubscriptionId!,
            resourceGroup);
        var resourceGroupResource = armClient.GetResourceGroupResource(resourceGroupId);

        // Resolve the calling user's email from the ARM token and always include them as a collaborator.
        var callerEmail = await GetCallerEmailAsync(tenant, cancellationToken).ConfigureAwait(false);

        // Merge caller with any explicitly provided collaborators, deduplicating by email (case-insensitive).
        var allCollaborators = (collaborators ?? [])
            .Append(callerEmail)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var collaborationData = new CollaborationData(new Azure.Core.AzureLocation(location))
        {
            ResourceLocation = new Azure.Core.AzureLocation(resourceLocation ?? location)
        };

        foreach (var collaborator in allCollaborators)
        {
            collaborationData.Collaborators.Add(new Collaborator
            {
                UserIdentifier = collaborator
            });
        }

        // Fire the ARM PUT without blocking — provisioning takes ~25 minutes.
        var operation = await resourceGroupResource.GetCollaborations()
            .CreateOrUpdateAsync(
                WaitUntil.Started,
                name,
                collaborationData,
                cancellationToken)
            .ConfigureAwait(false);

        var acceptedResult = ParseResponse(operation.GetRawResponse());

        const string acceptedMessage = "Collaboration create request accepted. Provisioning typically takes about 25 minutes.";
        return new CollaborationCreateResult(acceptedResult, acceptedMessage);
    }

    private async Task<CollaborationClient> BuildClientAsync(
        string endpoint,
        bool allowUntrustedCert,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint));

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new ArgumentException($"Endpoint '{endpoint}' is not a valid absolute URI.", nameof(endpoint));
        }

        var credential = await GetCredential(tenant, cancellationToken).ConfigureAwait(false);

        var options = new CollaborationClientOptions();
        options.AddPolicy(
            new BearerTokenAuthenticationPolicy(credential, TenantService.CloudConfiguration.ArmEnvironment.DefaultScope),
            HttpPipelinePosition.PerCall);

        var testProxyUrl = Environment.GetEnvironmentVariable("TEST_PROXY_URL");
        if (!string.IsNullOrWhiteSpace(testProxyUrl))
        {
            // Keep proxy transport active in record/playback so requests are captured and replayed.
            options.Transport = new HttpClientTransport(_httpClientFactory.CreateClient());
        }
        else if (allowUntrustedCert)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            options.Transport = new HttpClientTransport(handler);
        }
        else
        {
            options.Transport = new HttpClientTransport(_httpClientFactory.CreateClient());
        }

        return new CollaborationClient(endpointUri, options);
    }

    internal static JsonElement ParseResponse(Response response)
    {
        if (response.Content is null)
        {
            return EmptyStringElement;
        }

        var content = response.Content.ToMemory();
        if (content.IsEmpty)
        {
            return EmptyStringElement;
        }

        try
        {
            var element = JsonSerializer.Deserialize(
                content.Span,
                ManagedCleanroomSerializerContext.Default.JsonElement);
            // Preserve JSON payload as received when content is valid JSON.
            return element;
        }
        catch (JsonException)
        {
            // Some endpoints may return non-JSON payloads; return the raw payload as a JSON string value.
            var rawResponse = response.Content.ToString();
            if (string.IsNullOrWhiteSpace(rawResponse))
            {
                return EmptyStringElement;
            }

            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                writer.WriteStringValue(rawResponse);
                writer.Flush();
            }

            return JsonSerializer.Deserialize(
                buffer.WrittenSpan,
                ManagedCleanroomSerializerContext.Default.JsonElement);
        }
    }

    /// <summary>
    /// Parses the response bytes from an ARM action POST, throwing if the service returned
    /// an error payload despite a 2xx HTTP status (e.g. HTTP 200 with ErrorCode: InternalError).
    /// </summary>
    private static JsonElement ParseArmActionResponse(byte[] responseBytes, int httpStatusCode, string actionName)
    {
        if (responseBytes.Length == 0)
        {
            return default;
        }

        var element = JsonSerializer.Deserialize(responseBytes, ManagedCleanroomSerializerContext.Default.JsonElement);

        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty("error", out var errorProp) &&
            errorProp.TryGetProperty("code", out var codeProp))
        {
            var code = codeProp.GetString() ?? "UnknownError";
            var message = errorProp.TryGetProperty("message", out var msgProp)
                ? msgProp.GetString() ?? string.Empty
                : string.Empty;
            throw new Azure.RequestFailedException(
                httpStatusCode,
                $"ARM action '{actionName}' returned an error: [{code}] {message}",
                code,
                null);
        }

        return element;
    }
}


