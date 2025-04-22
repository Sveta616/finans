CREATE TABLE AutoDeposits (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    ToAccountId INT NOT NULL,  -- Куда поступают средства
    Source NVARCHAR(100),      -- Откуда (например, 'Зарплата', 'Инвестиции')
    NextDate DATETIME NOT NULL,
    Frequency NVARCHAR(20) NOT NULL CHECK (Frequency IN ('Daily', 'Weekly', 'Monthly', 'Quarterly', 'Yearly')),
    LastProcessed DATETIME NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (ToAccountId) REFERENCES Accounts(Id)
);

CREATE TABLE AutoPayments (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    FromAccountId INT NOT NULL,
    ToAccountId INT NOT NULL,
    NextDate DATETIME NOT NULL,
    Frequency NVARCHAR(20) NOT NULL CHECK (Frequency IN ('Daily', 'Weekly', 'Monthly', 'Quarterly', 'Yearly')),
    LastProcessed DATETIME NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (FromAccountId) REFERENCES Accounts(Id),
    FOREIGN KEY (ToAccountId) REFERENCES Accounts(Id)
);

CREATE TABLE Transactions (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    AccountId INT NOT NULL,
    Type NVARCHAR(10) NOT NULL CHECK (Type IN ('Income', 'Expense', 'Transfer')),
    CategoryId INT,
    Amount DECIMAL(18,2) NOT NULL,
    Date DATETIME NOT NULL,
    Description NVARCHAR(255),
    TargetAccountId INT NULL, -- Для переводов между счетами
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (AccountId) REFERENCES Accounts(Id),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    FOREIGN KEY (TargetAccountId) REFERENCES Accounts(Id)
);

CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    Type NVARCHAR(10) NOT NULL CHECK (Type IN ('Income', 'Expense')),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT UC_Category UNIQUE (UserId, Name, Type)
);

CREATE TABLE Accounts (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    Balance DECIMAL(18,2) DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT UC_Account UNIQUE (UserId, Name)
);

CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(64) NOT NULL,
    Salt NVARCHAR(64) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);