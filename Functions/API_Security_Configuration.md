# API Key Configuration Guide

## Overview
The smart bulb functions now include secure API key authentication. The API key is required for all requests to the bulb control API.

## Security Implementation
- ? API key is stored in configuration (not hardcoded)
- ? Different settings for development and production
- ? Secure header injection (`X-API-Key`)
- ? Warning logs if API key is missing

## Configuration

### Development (Local)
The API key is configured in `local.settings.json`:
```json
{
  "Values": {
    "BulbApiBaseUrl": "https://deidra-nonconverging-farah.ngrok-free.dev/api",
    "BulbApiKey": "MySecretDemo2025"
  }
}
```

### Production (Azure Functions)
For production deployment, configure these Application Settings in Azure:

| Setting Name | Value |
|-------------|-------|
| `BulbApiBaseUrl` | `https://your-production-api.com/api` |
| `BulbApiKey` | `YourSecretProductionKey` |

### Azure CLI Configuration
```bash
# Set the API base URL
az functionapp config appsettings set \
  --name YourFunctionAppName \
  --resource-group YourResourceGroup \
  --settings "BulbApiBaseUrl=https://your-production-api.com/api"

# Set the API key (keep this secret!)
az functionapp config appsettings set \
  --name YourFunctionAppName \
  --resource-group YourResourceGroup \
  --settings "BulbApiKey=YourSecretProductionKey"
```

### Azure Portal Configuration
1. Navigate to your Function App in Azure Portal
2. Go to **Configuration** > **Application settings**
3. Add new settings:
   - **Name**: `BulbApiBaseUrl`, **Value**: `https://your-production-api.com/api`
   - **Name**: `BulbApiKey`, **Value**: `YourSecretProductionKey`
4. Click **Save**

## Security Best Practices

### ? DO:
- Store API keys in Azure Key Vault for production
- Use different API keys for development and production
- Rotate API keys regularly
- Monitor API key usage

### ? DON'T:
- Commit API keys to source control
- Share API keys in plain text
- Use the same key across environments
- Log API keys in application logs

## Advanced Security (Recommended for Production)

### Using Azure Key Vault
1. Store the API key in Azure Key Vault
2. Grant your Function App access to the Key Vault
3. Reference the secret in configuration:
   ```
   BulbApi:ApiKey=@Microsoft.KeyVault(SecretUri=https://your-vault.vault.azure.net/secrets/bulb-api-key/)
   ```

### Environment-Specific Configuration
```json
// Development
{
  "BulbApiKey": "MySecretDemo2025"
}

// Staging  
{
  "BulbApiKey": "@Microsoft.KeyVault(SecretUri=https://staging-vault.vault.azure.net/secrets/bulb-api-key/)"
}

// Production
{
  "BulbApiKey": "@Microsoft.KeyVault(SecretUri=https://prod-vault.vault.azure.net/secrets/bulb-api-key/)"
}
```

## Troubleshooting

### Missing API Key Warning
If you see this log message:
```
API key not configured. Some API calls may fail.
```

**Solution**: Configure the `BulbApiKey` setting in your environment.

### API Authentication Errors
If API calls return 401/403 errors:
1. Verify the API key is correct
2. Check the key hasn't expired
3. Ensure the key has proper permissions
4. Verify the `X-API-Key` header is being sent

### Configuration Not Loading
If settings aren't loading:
1. Restart the Function App
2. Check setting names match exactly (case-sensitive)
3. Verify JSON formatting in local.settings.json
4. Check Azure Portal configuration is saved

## Current API Key
**Development**: `MySecretDemo2025`
**Note**: This key is for demo purposes only. Use a secure key in production.