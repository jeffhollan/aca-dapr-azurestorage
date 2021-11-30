az extension add `
  --source https://workerappscliextension.blob.core.windows.net/azure-cli-extension/containerapp-0.2.0-py2.py3-none-any.whl

$RESOURCE_GROUP="my-dapr-app"
$LOCATION="canadacentral"
$LOG_ANALYTICS_WORKSPACE="aca-logs"
$CONTAINERAPPS_ENVIRONMENT="aca-env"
$STORAGE_ACCOUNT="daprstor$(Get-Random -Minimum 100 -Maximum 1000)"

az group create `
  --name $RESOURCE_GROUP `
  --location "$LOCATION"

az monitor log-analytics workspace create `
--resource-group $RESOURCE_GROUP `
--workspace-name $LOG_ANALYTICS_WORKSPACE

$LOG_ANALYTICS_WORKSPACE_CLIENT_ID=(az monitor log-analytics workspace show --query customerId -g $RESOURCE_GROUP -n $LOG_ANALYTICS_WORKSPACE --out tsv)
$LOG_ANALYTICS_WORKSPACE_CLIENT_SECRET=(az monitor log-analytics workspace get-shared-keys --query primarySharedKey -g $RESOURCE_GROUP -n $LOG_ANALYTICS_WORKSPACE --out tsv)

az storage account create -n $STORAGE_ACCOUNT -g $RESOURCE_GROUP
$ACCOUNT_KEY=(az storage account keys list -n $STORAGE_ACCOUNT --query '[0].value' -otsv)

az containerapp create `
  --name 'my-container' `
  --resource-group $RESOURCE_GROUP `
  --environment $CONTAINERAPPS_ENVIRONMENT `
  --image ghcr.io/jeffhollan/aca-dapr-azurestorage/app:main `
  --secrets storage-account-key=$ACCOUNT_KEY `
  --target-port 80 `
  --ingress 'external' `
  --min-replicas 1 `
  --max-replicas 1 `
  --enable-dapr `
  --dapr-app-port 80 `
  --dapr-app-id 'my-container' `
  --dapr-components ./dapr-components.yaml 