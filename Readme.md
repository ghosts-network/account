# GhostNetwork - Account

Account is a part of GhostNetwork education project and provides OAuth support

## Installation

copy provided dev-compose.yml and customize for your needs

### Parameters

| Environment             | Description                                                                       |
|-------------------------|-----------------------------------------------------------------------------------|
| EMAIL_SENDER            | Type of notifications sender. Options: smtp, service. By default logs messages    |
| NOTIFICATIONS_ADDRESS   | Address of notifications service instance. Required for FILE_STORAGE_TYPE=service |

## Development

To run dependent environment use

```bash
docker-compose -f dev-compose.yml pull
docker-compose -f dev-compose.yml up --force-recreate
```
