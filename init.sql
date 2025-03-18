IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'WalletDb')
BEGIN
    CREATE DATABASE WalletDb;
END;
GO

USE WalletDb;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Wallets')
BEGIN
    CREATE TABLE Wallets (
        PlayerId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Balance DECIMAL(18,2) NOT NULL CHECK (Balance >= 0),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
BEGIN
    CREATE TABLE Transactions (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        PlayerId UNIQUEIDENTIFIER NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT GETDATE(),
        Type INT NOT NULL,
        TxRemoteId NVARCHAR(100) NOT NULL, 
        UNIQUE (TxRemoteId), 
        FOREIGN KEY (PlayerId) REFERENCES Wallets(PlayerId)
    );
END;
GO
