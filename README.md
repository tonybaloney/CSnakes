# Aspire Demo for BUILD 2024

This demo shows building a custom resource, using local azure provisioning for service bus and
deployment to azure.

## Setup
To get this repo running you'll need a few setup steps outlined below.

### Azure Prerequisites
Specify an Azure account for the Service Bus setup and to configure your subscription information following the [Configuration section in Local Azure provisioning documentation](https://learn.microsoft.com/en-us/dotnet/aspire/deployment/azure/local-provisioning#configuration). 

### Parameter resource values
Some Parameter values are required for a few resources. Add these to your user-secrets in the `Parameters` section.

#### Postgres database password
Create a user secrets in the AppHost project for the Postgres password:

```
  "Parameters": {
    "postgrespass": "your-password-of-choice"
  }
```

#### Email 'from address'
Create a user secrets in the AppHost project for the email 'from' address:

```
  "Parameters": {
    "frommail": "someone@example.com"
  }
```
