targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention.')
param environmentName string

@minLength(1)
@maxLength(90)
@description('Name of the resource group to use or create')
param resourceGroupName string = 'rg-${environmentName}'

@description('The location to deploy resources')
param location string = deployment().location

var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))

// Tags that should be applied to all resources.
// 
// Note that 'azd-service-name' tags should be applied separately to service host resources.
// Example usage:
//   tags: union(tags, { 'azd-service-name': <service name in azure.yaml> })
var tags = {
  'azd-env-name': environmentName
}

@description('(Optional) Principal identifier of the identity that is deploying the template.')
param deploymentIdentityPrincipalId string = deployer().objectId

// Redis Cache Contributor role definition ID
var redisCacheContributorRoleId = 'e0f68234-74aa-48ed-b826-c38b57376e17'

// Create resource group
resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
}

// Create a managed identity for Redis Cache access
module managedIdentity 'br/public:avm/res/managed-identity/user-assigned-identity:0.4.0' = {
  scope: rg
  name: 'managed-identity-deployment'
  params: {
    name: '${resourceToken}-identity'
    location: location
  }
}

// Deploy Redis Cache using Azure Verified Module
module redisEnterprise  'br/public:avm/res/cache/redis-enterprise:0.5.0' = {
  scope: rg
  name: 'redis-deployment'
  params: {
    name: '${resourceToken}-redis'
    location: location
    skuName: 'Balanced_B0' // Using Balanced_B0 for Basic SKU
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Disabled'
    managedIdentities: {
      userAssignedResourceIds: [managedIdentity.outputs.resourceId]
    }
    roleAssignments: [
      // Role assignment for user account
      {
        principalId: deploymentIdentityPrincipalId
        roleDefinitionIdOrName: redisCacheContributorRoleId
        principalType: 'User'
        description: 'Grants Redis Cache Contributor role to the user account'
      }
      // Role assignment for managed identity
      {
        principalId: managedIdentity.outputs.principalId
        roleDefinitionIdOrName: redisCacheContributorRoleId
        principalType: 'ServicePrincipal'
        description: 'Grants Redis Cache Contributor role to the managed identity'
      }
    ]
    tags: tags
  }
}

@description('The name of the Redis Cache')
output AZURE_REDIS_RESOURCE_NAME string = redisEnterprise .outputs.name

@description('Redis connection information')
output AZURE_REDIS_HOST_NAME string = redisEnterprise .outputs.hostName
output AZURE_REDIS_PORT int = redisEnterprise .outputs.port
output AZURE_REDIS_ENDPOINT string = redisEnterprise .outputs.endpoint
