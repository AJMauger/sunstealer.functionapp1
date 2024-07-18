terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
    }
  }
}

data "azurerm_key_vault" "kv" {
  name                       = "key-vault-name"
  resource_group_name        = azurerm_resource_group.rg.name
}

data "azurerm_key_vault_secret" "kvs" {
  name         = "kvs"
  key_vault_id = data.azurerm_key_vault.kv.id
}

data "azurerm_client_config" "current" {}

resource "azurerm_resource_group" "rg" {
  name = "resource-group-name"
  location = "US-West"
}

resource "azurerm_app_configuration" "acs" {
  name                = "app-config-name"
  location            = "East US"
  resource_group_name = azurerm_resource_group.rg.name

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_role_assignment" "akv_sp" {
  scope                = data.azurerm_key_vault.kv.id
  principal_id         = azurerm_app_configuration.acs.identity
  role_definition_name = "Key Vault Secrets Officer"
}

resource "azurerm_app_configuration_key" "acs_key" {
  configuration_store_id = azurerm_app_configuration.acs.id
  content_type = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
  key          = "secret-key"
  type         = "vault"
  # value      = "{\"uri\": \"https://key-vault-name.vault.azure.net/secrets/secret-key\"}"

  vault_key_reference    = data.azurerm_key_vault_secret.kvs.id
}
 