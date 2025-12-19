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

// Redis Enterprise data access roles
var redisEnterpriseDataOwnerRoleId = '7334ffa7-bda9-4f59-abec-e738babf4049' // Redis Enterprise Cache Data Owner
var redisEnterpriseDataContributorRoleId = '0526d86d-59b5-4ce5-a5db-2f6b802b0dc6' // Redis Enterprise Cache Data Contributor

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
    publicNetworkAccess: 'Enabled'
    managedIdentities: {
      userAssignedResourceIds: [managedIdentity.outputs.resourceId]
    }
    roleAssignments: [
      // Data Owner role for deployment user account
      {
        principalId: deploymentIdentityPrincipalId
        roleDefinitionIdOrName: redisEnterpriseDataOwnerRoleId
        principalType: 'User'
        description: 'Grants Redis Enterprise Cache Data Owner role to the deployment user account'
      }
      // Data Contributor role for deployment user account
      {
        principalId: deploymentIdentityPrincipalId
        roleDefinitionIdOrName: redisEnterpriseDataContributorRoleId
        principalType: 'User'
        description: 'Grants Redis Enterprise Cache Data Contributor role to the deployment user account'
      }
      // Data Owner role for managed identity
      {
        principalId: managedIdentity.outputs.principalId
        roleDefinitionIdOrName: redisEnterpriseDataOwnerRoleId
        principalType: 'ServicePrincipal'
        description: 'Grants Redis Enterprise Cache Data Owner role to the managed identity'
      }
      // Data Contributor role for managed identity
      {
        principalId: managedIdentity.outputs.principalId
        roleDefinitionIdOrName: redisEnterpriseDataContributorRoleId
        principalType: 'ServicePrincipal'
        description: 'Grants Redis Enterprise Cache Data Contributor role to the managed identity'
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
