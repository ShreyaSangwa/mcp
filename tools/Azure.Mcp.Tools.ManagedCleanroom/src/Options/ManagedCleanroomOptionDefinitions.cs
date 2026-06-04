// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options;

public static class ManagedCleanroomOptionDefinitions
{
    public const string EndpointName = "endpoint";
    public const string ActiveOnlyName = "active-only";
    public const string AllowUntrustedCertName = "allow-untrusted-cert";

    public static readonly Option<string> Endpoint = new($"--{EndpointName}")
    {
        Description = "The Azure Cleanroom Analytics Frontend service endpoint URL (e.g., 'https://my-cleanroom.azure.com').",
        Required = true
    };

    public static readonly Option<bool?> ActiveOnly = new($"--{ActiveOnlyName}")
    {
        Description = "When true, returns only active collaborations (email-only lookup). When false or omitted, returns all collaborations.",
        Required = false
    };

    public static readonly Option<bool> AllowUntrustedCert = new($"--{AllowUntrustedCertName}")
    {
        Description = "When true, skips TLS certificate validation for the cleanroom endpoint. Use only for dev/test endpoints with self-signed certificates.",
        Required = false
    };
}
