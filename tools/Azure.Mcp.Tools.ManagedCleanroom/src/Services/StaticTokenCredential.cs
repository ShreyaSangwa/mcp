// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

/// <summary>
/// A <see cref="TokenCredential"/> that returns a pre-acquired token string verbatim.
/// Used when a caller supplies a raw bearer token (e.g. an MSAL ID token for MSA accounts)
/// via the <c>MANAGEDCLEANROOM_ACCESS_TOKEN</c> environment variable instead of going through
/// the normal Azure credential chain.
/// </summary>
internal sealed class StaticTokenCredential(string token) : TokenCredential
{
    // Use MaxValue so the SDK never tries to refresh — the token is static by design.
    private readonly AccessToken _accessToken = new(token, DateTimeOffset.MaxValue);

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        => _accessToken;

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        => ValueTask.FromResult(_accessToken);
}
