targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

// NOTE: Azure Cleanroom Analytics Frontend is not a first-class ARM resource type.
// Live tests expect the CLEANROOM_ENDPOINT output to be supplied externally
// (e.g., via the post-deployment script reading an existing service endpoint).
@description('The Azure Cleanroom Analytics Frontend endpoint URL to test against.')
param cleanroomEndpoint string = ''

@description('A known collaboration ID to use in live tests (collaborations get, analytics get, oidc issuer-info).')
param cleanroomCollaborationId string = ''

output CLEANROOM_ENDPOINT string = cleanroomEndpoint
output CLEANROOM_COLLABORATION_ID string = cleanroomCollaborationId
