
DELETE FROM OrderProduct;
DELETE FROM ComputerEmployee;
DELETE FROM EmployeeTraining;
DELETE FROM Employee;
DELETE FROM TrainingProgram;
DELETE FROM Computer;
DELETE FROM Department;
DELETE FROM [Order];
DELETE FROM PaymentType;
DELETE FROM Product;
DELETE FROM ProductType;
DELETE FROM Customer;


ALTER TABLE Employee DROP CONSTRAINT [FK_EmployeeDepartment];
ALTER TABLE ComputerEmployee DROP CONSTRAINT [FK_ComputerEmployee_Employee];
ALTER TABLE ComputerEmployee DROP CONSTRAINT [FK_ComputerEmployee_Computer];
ALTER TABLE EmployeeTraining DROP CONSTRAINT [FK_EmployeeTraining_Employee];
ALTER TABLE EmployeeTraining DROP CONSTRAINT [FK_EmployeeTraining_Training];
ALTER TABLE Product DROP CONSTRAINT [FK_Product_ProductType];
ALTER TABLE Product DROP CONSTRAINT [FK_Product_Customer];
ALTER TABLE PaymentType DROP CONSTRAINT [FK_PaymentType_Customer];
ALTER TABLE [Order] DROP CONSTRAINT [FK_Order_Customer];
ALTER TABLE [Order] DROP CONSTRAINT [FK_Order_Payment];
ALTER TABLE OrderProduct DROP CONSTRAINT [FK_OrderProduct_Product];
ALTER TABLE OrderProduct DROP CONSTRAINT [FK_OrderProduct_Order];


DROP TABLE IF EXISTS OrderProduct;
DROP TABLE IF EXISTS ComputerEmployee;
DROP TABLE IF EXISTS EmployeeTraining;
DROP TABLE IF EXISTS Employee;
DROP TABLE IF EXISTS TrainingProgram;
DROP TABLE IF EXISTS Computer;
DROP TABLE IF EXISTS Department;
DROP TABLE IF EXISTS [Order];
DROP TABLE IF EXISTS PaymentType;
DROP TABLE IF EXISTS Product;
DROP TABLE IF EXISTS ProductType;
DROP TABLE IF EXISTS Customer;



CREATE TABLE Department (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(55) NOT NULL,
	Budget 	INTEGER NOT NULL
);

CREATE TABLE Employee (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	FirstName VARCHAR(55) NOT NULL,
	LastName VARCHAR(55) NOT NULL,
	DepartmentId INTEGER NOT NULL,
	IsSuperVisor BIT NOT NULL DEFAULT(0),
    CONSTRAINT FK_EmployeeDepartment FOREIGN KEY(DepartmentId) REFERENCES Department(Id)
);

CREATE TABLE Computer (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	PurchaseDate DATETIME NOT NULL,
	DecomissionDate DATETIME
);

CREATE TABLE ComputerEmployee (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	EmployeeId INTEGER NOT NULL,
	ComputerId INTEGER NOT NULL,
	AssignDate DATETIME NOT NULL,
	UnassignDate DATETIME,
    CONSTRAINT FK_ComputerEmployee_Employee FOREIGN KEY(EmployeeId) REFERENCES Employee(Id),
    CONSTRAINT FK_ComputerEmployee_Computer FOREIGN KEY(ComputerId) REFERENCES Computer(Id)
);


CREATE TABLE TrainingProgram (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	StartDate DATETIME NOT NULL,
	EndDate DATETIME NOT NULL,
	MaxAttendees INTEGER NOT NULL
);

CREATE TABLE EmployeeTraining (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	EmployeeId INTEGER NOT NULL,
	TrainingProgramId INTEGER NOT NULL,
    CONSTRAINT FK_EmployeeTraining_Employee FOREIGN KEY(EmployeeId) REFERENCES Employee(Id),
    CONSTRAINT FK_EmployeeTraining_Training FOREIGN KEY(TrainingProgramId) REFERENCES TrainingProgram(Id)
);

CREATE TABLE ProductType (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(55) NOT NULL
);

CREATE TABLE Customer (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	FirstName VARCHAR(55) NOT NULL,
	LastName VARCHAR(55) NOT NULL
);

CREATE TABLE Product (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	ProductTypeId INTEGER NOT NULL,
	CustomerId INTEGER NOT NULL,
	Price INTEGER NOT NULL,
	Title VARCHAR(255) NOT NULL,
	[Description] VARCHAR(255) NOT NULL,
	Quantity INTEGER NOT NULL,
    CONSTRAINT FK_Product_ProductType FOREIGN KEY(ProductTypeId) REFERENCES ProductType(Id),
    CONSTRAINT FK_Product_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id)
);


CREATE TABLE PaymentType (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	AcctNumber INTEGER NOT NULL,
	[Name] VARCHAR(55) NOT NULL,
	CustomerId INTEGER NOT NULL,
    CONSTRAINT FK_PaymentType_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id)
);

CREATE TABLE [Order] (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	CustomerId INTEGER NOT NULL,
	PaymentTypeId INTEGER,
    CONSTRAINT FK_Order_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id),
    CONSTRAINT FK_Order_Payment FOREIGN KEY(PaymentTypeId) REFERENCES PaymentType(Id)
);

CREATE TABLE OrderProduct (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	OrderId INTEGER NOT NULL,
	ProductId INTEGER NOT NULL,
    CONSTRAINT FK_OrderProduct_Product FOREIGN KEY(ProductId) REFERENCES Product(Id),
    CONSTRAINT FK_OrderProduct_Order FOREIGN KEY(OrderId) REFERENCES [Order](Id)
);
INSERT INTO Department ([Name], Budget)
VALUES('Development', 200000);
INSERT INTO Department ([Name], Budget)
VALUES('Custodial', 150);
INSERT INTO Employee (FirstName,LastName,DepartmentId,IsSuperVisor)
VALUES ('Johnathan', 'PraiseBe', 1, 1);
INSERT INTO Employee (FirstName,LastName,DepartmentId,IsSuperVisor)
VALUES ('Alejandro', 'ComicSans', 2, 0);
INSERT INTO Customer (FirstName, LastName)
VALUES ('Jose', 'Cuervo');
INSERT INTO Customer (FirstName, LastName)
VALUES ('Don', 'Juan');
INSERT INTO Computer (PurchaseDate, DecomissionDate)
VALUES ('01-24-2018 00:00:01', '01-24-2020 23:59:59');
INSERT INTO Computer (PurchaseDate, DecomissionDate)
VALUES ('02-24-2018 00:00:01', '02-24-2020 23:59:59');
INSERT INTO ComputerEmployee (EmployeeId, ComputerId, AssignDate, UnassignDate)
VALUES (1, 1, '01-24-2018 00:00:01', '01-24-2020 23:59:59');
INSERT INTO ComputerEmployee (EmployeeId, ComputerId, AssignDate, UnassignDate)
VALUES (2, 2, '02-24-2018 00:00:01', '02-24-2020 23:59:59');
INSERT INTO TrainingProgram (StartDate,EndDate, MaxAttendees)
VALUES ('09-24-2018 00:00:01', '09-26-2018 23:59:59', 30);
INSERT INTO TrainingProgram (StartDate,EndDate, MaxAttendees)
VALUES ('10-31-2018 00:00:01', '10-31-2018 23:59:59', 500);
INSERT INTO EmployeeTraining (EmployeeId,TrainingProgramId)
VALUES (1, 1);
INSERT INTO EmployeeTraining (EmployeeId,TrainingProgramId)
VALUES (1, 2);
INSERT INTO EmployeeTraining (EmployeeId,TrainingProgramId)
VALUES (2, 2);
INSERT INTO ProductType ([Name])
VALUES ('McGuffins');
INSERT INTO ProductType ([Name])
VALUES ('Deus Ex Machinas');
INSERT INTO ProductType ([Name])
VALUES ('Widgets');
INSERT INTO Customer (FirstName,LastName)
VALUES('Steve', 'Jobs');
INSERT INTO Customer (FirstName,LastName)
VALUES('George', 'Washington');
INSERT INTO Product (ProductTypeId, CustomerId, Price, Title, [Description], Quantity)
VALUES (1, 1, 500, 'Instant Csharp Learning', 'Matrix like jack-in to learn csharp', 1);
INSERT INTO Product (ProductTypeId, CustomerId, Price, Title, [Description], Quantity)
VALUES (3, 2, 500, 'GUN', 'I don''t know what it does, I just know the sound it makes when it takes a life', 1);
INSERT INTO PaymentType (AcctNumber,[Name],CustomerId)
VALUES (12345, 'Veesa', 1);
INSERT INTO PaymentType (AcctNumber,[Name],CustomerId)
VALUES (123456, 'Master Shard', 2);
INSERT INTO PaymentType (AcctNumber,[Name],CustomerId)
VALUES (123456, 'Soviet Express', 2);
INSERT INTO [Order] (CustomerId, PaymentTypeId)
VALUES (1, 1);
INSERT INTO [Order] (CustomerId, PaymentTypeId)
VALUES (2, 3);
INSERT INTO OrderProduct (OrderId, ProductId)
VALUES (1, 1);
INSERT INTO OrderProduct (OrderId, ProductId)
VALUES (2, 2);

SELECT
c.Id,
c.FirstName,
o.CustomerId
FROM Customer c
LEFT JOIN [Order] o ON c.Id = o.CustomerId
WHERE o.CustomerId IS NULL


