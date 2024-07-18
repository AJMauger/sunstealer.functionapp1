terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
    }
  }
}

resource "azurerm_app_configuration" "app_config" {
  name                = "app-config-name"
  location            = "East US"
  resource_group_name = azurerm_resource_group.rg.name

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_key_vault" "vault" {
  name                       = "key-vault-name"
  location                   = "East US"
  resource_group_name        = azurerm_resource_group.rg.name
  sku_name                   = "standard"
  tenant_id                  = data.azurerm_client_config.current.tenant_id
}

data "azurerm_client_config" "current" {}

resource "azurerm_role_assignment" "akv_sp" {
  scope                = azurerm_key_vault.vault.id
  principal_id         = azurerm_app_configuration.app_config.identity
  role_definition_name = "Key Vault Secrets Officer"
}

resource "azurerm_app_configuration_kv" "my_key" {
  name         = "my-secret-key"
  value        = "{\"uri\": \"https://key-vault-name.vault.azure.net/secrets/my-secret\"}"
  content_type = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
  config_store_id = azurerm_app_configuration.app_config.id
}