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

resource "azurerm_resource_group" "rg" {
  name     = "example-rg"
  location = "East US"
}

resource "azurerm_service_plan" "plan" {
  name                = "example-plan"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  os_type             = "Linux"
  sku_name            = "B1"
}

resource "azurerm_storage_account" "storage" {
  name                     = "examplestorageacct"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_function_app" "function" {
  storage_account_access_key = false
  app_service_plan_id       = azurerm_service_plan.plan.id
  name                      = "example-function-app"
  resource_group_name       = azurerm_resource_group.rg.name
  location                  = azurerm_resource_group.rg.location
  storage_account_name      = azurerm_storage_account.storage.name
  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_user_assigned_identity" "api_identity" {
  name                = "api-managed-identity"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
}

resource "azurerm_role_assignment" "function_identity_role" {
  principal_id         = azurerm_function_app.function.identity[0].principal_id
  role_definition_name = "Reader"
  scope               = "/subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/${azurerm_resource_group.rg.name}"
}
