param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs,
    [hashtable] $AdditionalParameters
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

$testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

if (-not $testSettings.Contains('DeploymentOutputs') -or $null -eq $testSettings.DeploymentOutputs) {
    $testSettings.DeploymentOutputs = @{}
}

function Set-DeploymentOutputValue {
    param(
        [string] $Key,
        [string] $DefaultValue = ""
    )

    $value = $DefaultValue
    if ($AdditionalParameters -and $AdditionalParameters.ContainsKey($Key) -and -not [string]::IsNullOrWhiteSpace($AdditionalParameters[$Key])) {
        $value = [string]$AdditionalParameters[$Key]
    }
    elseif ($DeploymentOutputs -and $DeploymentOutputs.ContainsKey($Key) -and -not [string]::IsNullOrWhiteSpace($DeploymentOutputs[$Key])) {
        $value = [string]$DeploymentOutputs[$Key]
    }

    $testSettings.DeploymentOutputs[$Key] = $value
}

# Required/optional values for recording all Managed Cleanroom command tests.
Set-DeploymentOutputValue -Key "CLEANROOM_SKR_POLICY_KID"
Set-DeploymentOutputValue -Key "CLEANROOM_KUBECONFIG_PATH"
Set-DeploymentOutputValue -Key "CLEANROOM_ENABLE_ARM_MUTATION_TESTS" -DefaultValue "true"
Set-DeploymentOutputValue -Key "CLEANROOM_MUTATION_NAME" -DefaultValue "$BaseName-lt"
Set-DeploymentOutputValue -Key "CLEANROOM_QUERY_DOCUMENT_ID"
Set-DeploymentOutputValue -Key "CLEANROOM_DATASET_DOCUMENT_ID"
Set-DeploymentOutputValue -Key "CLEANROOM_CONSENT_DOCUMENT_ID"
Set-DeploymentOutputValue -Key "CLEANROOM_OIDC_ISSUER_URL"
Set-DeploymentOutputValue -Key "CLEANROOM_INVITATION_ID"
Set-DeploymentOutputValue -Key "CLEANROOM_QUERY_PUBLISHER_DATASET_ID"
Set-DeploymentOutputValue -Key "CLEANROOM_QUERY_CONSUMER_DATASET_ID"
Set-DeploymentOutputValue -Key "CLEANROOM_QUERY_OUTPUT_DATASET_ID"

$settingsPath = Join-Path -Path $PSScriptRoot -ChildPath ".testsettings.json"
($testSettings | ConvertTo-Json -Depth 20) | Set-Content -Path $settingsPath -Force -NoNewLine

$cleanroomEndpoint = $DeploymentOutputs['CLEANROOM_ENDPOINT']

if ([string]::IsNullOrWhiteSpace($cleanroomEndpoint)) {
    Write-Warning "CLEANROOM_ENDPOINT was not set. Live tests will be skipped until a Cleanroom Analytics Frontend endpoint is provisioned and provided."
} else {
    Write-Host "Cleanroom Analytics Frontend endpoint: $cleanroomEndpoint" -ForegroundColor Gray
}

Write-Host "Managed Cleanroom test settings saved to: $PSScriptRoot\.testsettings.json" -ForegroundColor Green
