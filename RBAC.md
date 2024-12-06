| ObjectType       |  Scope                 | DisplayName                          | RoleDefinitionName                  |
| ---------------- | ---------------------- | ------------------------------------ | ----------------------------------- |
| ServicePrincipal | SunstealerFunctionApp1 | sunstealerkv                         | Key Vault Administrator             |
| ServicePrincipal | SunstealerFunctionApp1 | sunstealeracs                        | App Configuration Data Owner        |
| User             | Adam Mauger            | \<id\>                               | Owner                               |
| User             | Adam Mauger            | sunstealeracs                        | App Configuration Data Owner        |
| User             | Adam Mauger            | sunstealerkv                         | Key Vault Administrator             |
| ServicePrincipal | sunstealeracs          | sunstealerkv                         | Key Vault Administrator             |
| ServicePrincipal | sunstealeracs          | sunstealerkv                         | Key Vault Data Access Administrator |
