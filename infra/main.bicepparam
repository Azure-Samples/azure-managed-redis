using './main.bicep'

// Required parameters
param environmentName = readEnvironmentVariable('AZURE_ENV_NAME', 'redis-sample')

// Optional parameters - uncomment and modify as needed
param location = 'eastus'
