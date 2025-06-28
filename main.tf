terraform {
  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
    }
  }
}

provider "azurerm" {
  features {}
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
  name     = "example-rg"
  location = "example-location"
}

resource "azurerm_app_configuration" "acs" {
  name                = "app-config-name"
  location            = "example-location"
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

resource "azurerm_storage_account" "storage" {
  name                     = "examplestorageaccount"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "example"
  account_replication_type = "example"
  shared_access_key_enabled = false
}

resource "azurerm_service_plan" "plan" {
  name                = "example-serviceplan"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "example"
  sku_name            = "example"
}

resource "azurerm_function_app" "function" {
  name                       = "example-functionapp"
  resource_group_name        = azurerm_resource_group.rg.name
  location                   = azurerm_resource_group.rg.location
  storage_account_name       = azurerm_storage_account.storage.name
  app_service_plan_id        = azurerm_service_plan.plan.id
  storage_account_access_key = null

  auth_settings {
    enabled                      = true
    issuer                        = "https://sts.windows.net/{tenant_id}/"
    unauthenticated_client_action = "RedirectToLoginPage"
    default_provider              = "AzureActiveDirectory"
  }

  site_config {
    linux_fx_version = "Python|3.9"
  }
}

resource "azurerm_role_assignment" "function_storage_access" {
  scope                = azurerm_storage_account.storage.id
  role_definition_name = "example"
  principal_id         = azurerm_function_app.function.identity.0.principal_id
}

resource "azurerm_api_management" "apim" {
  name                = "example-apim"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  publisher_name      = "example"
  publisher_email     = "example@example.com"
  sku_name            = "example"
}

resource "azurerm_api_management_api" "api" {
  name                = "example-api"
  resource_group_name = azurerm_resource_group.rg.name
  api_management_name = azurerm_api_management.apim.name
  revision            = "1"
  display_name        = "Example Hello API"
  path                = "hello"
  protocols           = ["https"]
}

resource "azurerm_api_management_api_policy" "policy" {
  resource_group_name   = azurerm_resource_group.rg.name
  api_management_name   = azurerm_api_management.apim.name
  api_name              = azurerm_api_management_api.api.name
  xml_content           = <<XML
<policies>
  <inbound>
    <validate-jwt header-name="Authorization" failed-validation-httpcode="401" require-expiry="true">
      <openid-config url="https://login.microsoftonline.com/{tenant_id}/v2.0/.well-known/openid-configuration"/>
      <issuer-signing-keys/>
    </validate-jwt>
  </inbound>
</policies>
XML
}

resource "azuread_application" "app" {
  display_name = "example-apim"
}

resource "azuread_service_principal" "app_sp" {
  client_id = azuread_application.app.client_id
}

resource "azuread_application_password" "app_password" {
  application_id = azuread_application.app.object_id
}
