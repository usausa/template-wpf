CREATE TABLE Product (
    Code TEXT NOT NULL,
    Name TEXT NOT NULL,
    Category TEXT NOT NULL,
    Supplier TEXT NOT NULL,
    Warehouse TEXT NOT NULL,
    Price REAL NOT NULL,
    Stock INTEGER,
    Updated TEXT NOT NULL,
    IsActive INTEGER NOT NULL,
    PRIMARY KEY (Code)
);

CREATE TABLE Customer (
    Id INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Company TEXT NOT NULL,
    Email TEXT NOT NULL,
    Phone TEXT NOT NULL,
    City TEXT NOT NULL,
    PRIMARY KEY (Id AUTOINCREMENT)
);

CREATE TABLE OrderLine (
    LineNo INTEGER NOT NULL,
    Code TEXT NOT NULL,
    Description TEXT NOT NULL,
    Quantity INTEGER,
    UnitPrice REAL,
    Discount INTEGER,
    Delivery TEXT,
    Status INTEGER NOT NULL,
    Note TEXT NOT NULL,
    PRIMARY KEY (LineNo)
);

CREATE TABLE Data (
    Id INTEGER NOT NULL,
    Name TEXT NOT NULL,
    CreateAt INTEGER NOT NULL,
    PRIMARY KEY (Id)
);

CREATE TABLE BulkData (
    Key1 TEXT NOT NULL,
    Key2 TEXT NOT NULL,
    Key3 TEXT NOT NULL,
    Value1 INTEGER NOT NULL,
    Value2 INTEGER NOT NULL,
    Value3 INTEGER NOT NULL,
    Value4 INTEGER NOT NULL,
    Value5 INTEGER NOT NULL,
    PRIMARY KEY (Key1, Key2, Key3)
)
