# KK.PlayerWallet API Documentation

## Overview
The **KK.PlayerWallet** system provides API endpoints for managing player wallets, transactions, and game-related financial operations. This documentation describes available endpoints, request/response formats, and system behaviors.

## Services in the System
1. **KK.PlayerWalletApi** (Main API for player wallet operations)
2. **KK.WalletService** (Handles transactions and maintains consistency)
3. **KK.TransactionService** (Manages transaction validation and processing)
4. **KK.Common** (Shared resources, models, and utilities)
5. **RabbitMQ Message Bus** (Handles event-driven transaction processing)

## Endpoints

### 1. Wallet Service API (`/wallet`)
Manages wallet creation, deposits, withdrawals, and balance retrieval.

#### 1.1 Register a New Wallet
**Endpoint:** `POST /wallet/register`

**Request Body:**
```json
{
  "playerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response:**
- **200 OK** → `{ "message": "Wallet created" }`
- **400 Bad Request** → `{ "message": "Wallet already exists" }`

#### 1.2 Get Wallet Balance
**Endpoint:** `GET /wallet/balance/{playerId}`

**Response:**
- **200 OK** → `{ "balance": 100.50 }`
- **404 Not Found** → `{ "message": "Wallet not found" }`

#### 1.3 Deposit Funds
**Endpoint:** `POST /wallet/deposit`

**Request Body:**
```json
{
  "playerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "txRemoteId": "devKKenvTX123456",
  "amount": 50.00
}
```

**Response:**
```json
{
  "isSucces": true,
  "statusCode": 1,
  "playerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "txRemoteId": "devKKenvTX123456",
  "amount": 50.00,
  "type": "Deposit"
}
```
- **400 Bad Request** → `{ "statusCode": 2, "message": "Duplicate transaction" }`

#### 1.4 Withdraw Funds
**Endpoint:** `POST /wallet/withdraw`

**Request Body:**
```json
{
  "playerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "txRemoteId": "devKKenvTX654321",
  "amount": 20.00
}
```

**Response:**
```json
{
  "isSucces": true,
  "statusCode": 1,
  "playerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "txRemoteId": "devKKenvTX654321",
  "amount": 20.00,
  "type": "Withdraw"
}
```
- **400 Bad Request** → `{ "statusCode": 3, "message": "Insufficient funds" }`

#### 1.5 Get Transactions for a Player
**Endpoint:** `GET /wallet/transactions/{playerId}`

**Response:**
```json
[
  {
    "id": "d2811b92-3e2d-4565-8476-4a5c7a43e0ab",
    "txRemoteId": "devKKenvTX123456",
    "amount": 50.00,
    "type": "Deposit",
    "timestamp": "2024-06-10T12:00:00Z"
  },
  {
    "id": "e1a5f748-6bdc-45c3-a3bb-2a463f87d4b2",
    "txRemoteId": "devKKenvTX654321",
    "amount": 20.00,
    "type": "Withdraw",
    "timestamp": "2024-06-11T15:30:00Z"
  }
]
```

## Transaction Types Enum
```csharp
public enum TransactionType
{
    Deposit,
    Withdraw,
    Stake,
    Win,
    EndGameRound,
    Freespin,
    FreespinWin
}
```

## Status Codes Enum
```csharp
public enum StatusCode
{
    Success = 1,
    DuplicateTransaction = 2,
    InsufficientFunds = 3,
    WalletNotFound = 4,
    GeneralError = 5
}
```

## Requirements Fulfilled
- ✅ Secure transactions with GUID-based unique identifiers.
- ✅ Transaction rollback mechanism for consistency.
- ✅ Wallet balance validation before withdrawals.
- ✅ Database schema with referential integrity.
- ✅ Structured error handling for API endpoints.

## Non-Functional Requirements
- 🟢 Single-node deployment, directly communicating with the database.
- 🟢 Balance updates happen **only after transaction validation** to prevent race conditions.

## RabbitMQ Message Bus
- RabbitMQ is used for **event-driven processing** to ensure transactions are not impacting balance updates directly.
- Messages are queued to handle high loads asynchronously.

## Docker Setup

### Running with Docker Compose
1. Make sure Docker and Docker Compose are installed.
2. Run the following command:
   ```sh
   docker-compose up -d --build
    or
    docker-compose build --no-cache
    docker compose up -d
    
   ```
3. The API should be accessible at `http://localhost:5003` and `http://localhost:8080` (or your configured port).

### Detailed Description of Docker Containers
- **playerwalletapi** → Main API service (exposed on port `5003`).
- **walletservice** → Handles direct wallet operations (exposed on port `8080` ).
- **transactionservice** → Handles transaction validation and persistence.
- **rabbitmq** → Message broker for handling event-driven transactions.
- **mssql** → Microsoft SQL Server database (exposed on `1433`).

### Manual Database Initialization
If you need to manually initialize the database, run the following SQL script:

```sh   */opt/mssql-tools/bin/sqlcmd* (or your path)
docker exec -i mssql /opt/mssql-tools/bin/sqlcmd    -S localhost -U sa -P "<YourStrong!Password>" -d master -i /scripts/init.sql
```

Alternatively, if running without Docker, execute:
```sh
sqlcmd -S localhost -U sa -P "<YourStrong!Password>" -d master -i init.sql
```
