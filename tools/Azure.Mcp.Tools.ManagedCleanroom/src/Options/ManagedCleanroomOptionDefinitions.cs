// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options;

public static class ManagedCleanroomOptionDefinitions
{
    public const string EndpointName = "endpoint";
    public const string ActiveOnlyName = "active-only";
    public const string AllowUntrustedCertName = "allow-untrusted-cert";
    public const string CollaborationIdName = "collaboration-id";
    public const string IncludeDeletedName = "include-deleted";
    public const string KidName = "kid";

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

    public static readonly Option<string> CollaborationId = new($"--{CollaborationIdName}")
    {
        Description = "The unique identifier (UUID) of the cleanroom collaboration.",
        Required = true
    };

    public static readonly Option<bool?> IncludeDeleted = new($"--{IncludeDeletedName}")
    {
        Description = "When true, includes soft-deleted records in the response.",
        Required = false
    };

    public static readonly Option<string> Kid = new($"--{KidName}")
    {
        Description = "The key identifier (kid) of the SKR (Secure Key Release) policy to retrieve.",
        Required = true
    };
}
