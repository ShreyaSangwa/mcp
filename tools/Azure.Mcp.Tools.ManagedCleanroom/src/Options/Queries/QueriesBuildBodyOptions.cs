// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Queries;

public class QueriesBuildBodyOptions
{
    [Option("The query document name. This is used as the document-id when you publish the query.")]
    public required string QueryName { get; set; }

    [Option("Path to a directory containing query segment files named segment*.json or segment*.txt.")]
    public required string QueryDirectory { get; set; }

    [Option("Document ID of the publisher input dataset. Used with --consumer-input-dataset for the simple two-dataset case. Ignored if --input-dataset-mappings is provided.")]
    public string? PublisherInputDataset { get; set; }

    [Option("Document ID of the consumer input dataset. Used with --publisher-input-dataset for the simple two-dataset case. Ignored if --input-dataset-mappings is provided.")]
    public string? ConsumerInputDataset { get; set; }

    [Option("JSON mapping of input dataset IDs to their view aliases for the query, e.g., '{\"dataset1-id\":\"view1\",\"dataset2-id\":\"view2\"}'. When provided, overrides publisher/consumer input dataset options.")]
    public string? InputDatasetMappings { get; set; }

    [Option("Document ID of the output dataset.")]
    public required string OutputDataset { get; set; }

    [Option("The view alias for the output dataset in the query. Defaults to 'output'.")]
    public string? OutputDatasetAlias { get; set; }
}
