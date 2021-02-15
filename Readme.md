# Azure DevOps Metrics collection Sample

This code collects KPIs from Azure DevOps, like
- Number of projects
- Number of repositories
- number of users

## Prerequisite
- Azure Function (.net Core stack) - linux based recommended, with System Identity (MSI)
- Application Insights
- Azure KeyVault (grant read access to function app by MSI)
- Storage Account 
- Personal Access token (Azure DevOps) with read permissions

## add the following secrets to KeyVault
- metricStorageAccount, sample "DefaultEndpointsProtocol=https;AccountName=metricstrg;AccountKey=YOUR SA KEY HERE"
- AIInstrumentationKey, sample "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
- OrgaPAT, sample "xxxxxx7w2ysseuupufon2lhuy3xxkcxxxxxxxx6zvt3tdxxxxxxxx"

## Update Repo/ Pipeline
- update deployment variables in azure-pipelines,yml with your values (functionapp name, keyvault references, Service Connection)
- Create new Azure Pipeline from existing (azure-pipelines,yml)

## Run pipeline
This should deploy your function app to your Azure environment

## Trigger KPI scan 
To trigger the crawling of Azure DevOps KPIs, drop a message with any content to the Queue "metric-crawler-trigger"
