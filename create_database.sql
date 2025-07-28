-- Script tạo database MyGarageFinal
-- Dựa trên Entity Framework models

-- Tạo database
CREATE DATABASE MyGarageFinal;
GO

USE MyGarageFinal;
GO

-- Tạo bảng Users
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100),
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    Role NVARCHAR(20),
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);

-- Tạo bảng Garages
CREATE TABLE Garages (
    GarageID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100),
    Address NVARCHAR(255),
    OperatingArea NVARCHAR(100),
    Description NVARCHAR(255)
);

-- Tạo bảng Services
CREATE TABLE Services (
    ServiceID INT IDENTITY(1,1) PRIMARY KEY,
    ServiceName NVARCHAR(100),
    Description NVARCHAR(255),
    Price DECIMAL(18,2),
    image_url NVARCHAR(255)
);

-- Tạo bảng GarageServices (Many-to-Many)
CREATE TABLE GarageServices (
    GarageID INT,
    ServiceID INT,
    PRIMARY KEY (GarageID, ServiceID),
    FOREIGN KEY (GarageID) REFERENCES Garages(GarageID),
    FOREIGN KEY (ServiceID) REFERENCES Services(ServiceID)
);

-- Tạo bảng Vehicles
CREATE TABLE Vehicles (
    VehicleID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT,
    LicensePlate NVARCHAR(20),
    Make NVARCHAR(50),
    Model NVARCHAR(50),
    Notes NVARCHAR(255),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Tạo bảng Appointments
CREATE TABLE Appointments (
    AppointmentID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT,
    GarageID INT,
    AppointmentTime DATETIME,
    Status NVARCHAR(50),
    Notes NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    PromoCode NVARCHAR(50),
    DiscountAmount DECIMAL(18,2),
    TotalAmount DECIMAL(18,2),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (GarageID) REFERENCES Garages(GarageID)
);

-- Tạo bảng AppointmentVehicleDetails
CREATE TABLE AppointmentVehicleDetails (
    AppointmentVehicleDetailID INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentID INT,
    VehicleID INT,
    ServiceID INT,
    TechnicianID INT,
    Quantity INT DEFAULT 1,
    Note NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID),
    FOREIGN KEY (VehicleID) REFERENCES Vehicles(VehicleID),
    FOREIGN KEY (ServiceID) REFERENCES Services(ServiceID),
    FOREIGN KEY (TechnicianID) REFERENCES Users(UserID)
);

-- Tạo bảng Products
CREATE TABLE Products (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(100),
    Description NVARCHAR(255),
    Price DECIMAL(18,2),
    ImageUrl NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Tạo bảng AppointmentProductDetails
CREATE TABLE AppointmentProductDetails (
    AppointmentProductDetailID INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentID INT,
    ProductID INT,
    Quantity INT,
    UnitPrice DECIMAL(18,2),
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- Tạo bảng CartItems
CREATE TABLE CartItems (
    CartItemID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT,
    ProductID INT,
    AddedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- Tạo bảng FavoriteProducts
CREATE TABLE FavoriteProducts (
    FavoriteID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT,
    ProductID INT,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- Tạo bảng Orders
CREATE TABLE Orders (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT,
    OrderDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Pending',
    TotalAmount DECIMAL(18,2),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Tạo bảng OrderItems
CREATE TABLE OrderItems (
    OrderItemID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT,
    ProductID INT,
    Quantity INT,
    UnitPrice DECIMAL(18,2),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- Tạo bảng Reviews
CREATE TABLE Reviews (
    ReviewID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT,
    GarageID INT,
    Rating INT,
    Comment NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (GarageID) REFERENCES Garages(GarageID)
);

-- Tạo bảng GarageSchedules
CREATE TABLE GarageSchedules (
    ScheduleID INT IDENTITY(1,1) PRIMARY KEY,
    GarageID INT,
    DayOfWeek NVARCHAR(20),
    OpenTime TIME,
    CloseTime TIME,
    FOREIGN KEY (GarageID) REFERENCES Garages(GarageID)
);

-- Tạo bảng Notifications
CREATE TABLE Notifications (
    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT,
    Title NVARCHAR(100),
    Message NVARCHAR(255),
    IsRead BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Tạo bảng RepairStatus
CREATE TABLE RepairStatus (
    StatusID INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentID INT,
    StatusStep NVARCHAR(100),
    Notes NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID)
);

-- Tạo bảng TechnicalReports
CREATE TABLE TechnicalReports (
    ReportID INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentID INT,
    TechnicianID INT,
    VehicleStatus NVARCHAR(255),
    PerformedItems NVARCHAR(500),
    Recommendations NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID),
    FOREIGN KEY (TechnicianID) REFERENCES Users(UserID)
);

-- Tạo bảng AdminActivity
CREATE TABLE AdminActivities (
    ActivityID INT IDENTITY(1,1) PRIMARY KEY,
    AdminID INT,
    ActionType NVARCHAR(100),
    Description NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AdminID) REFERENCES Users(UserID)
);

-- Tạo bảng PromoCode
CREATE TABLE PromoCode (
    PromoCodeId INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50),
    DiscountAmount DECIMAL(18,2),
    DiscountPercent INT,
    ExpiryDate DATETIME,
    Description NVARCHAR(255)
);

-- Tạo bảng PaymentHistory
CREATE TABLE PaymentHistory (
    PaymentID INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentID INT,
    UserID INT,
    Amount DECIMAL(18,2),
    PaymentMethod NVARCHAR(50) DEFAULT 'PayOS',
    TransactionId NVARCHAR(100),
    Status NVARCHAR(50) DEFAULT 'Pending',
    PayOSOrderCode NVARCHAR(100),
    PayOSTransactionId NVARCHAR(100),
    Description NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Tạo bảng Messages
CREATE TABLE Messages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SenderId INT,
    ReceiverId INT,
    Content NVARCHAR(MAX) NOT NULL,
    Timestamp DATETIME,
    FOREIGN KEY (SenderId) REFERENCES Users(UserID),
    FOREIGN KEY (ReceiverId) REFERENCES Users(UserID)
);

-- Tạo indexes để tối ưu hiệu suất
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Appointments_UserId ON Appointments(UserID);
CREATE INDEX IX_Appointments_GarageId ON Appointments(GarageID);
CREATE INDEX IX_Appointments_Status ON Appointments(Status);
CREATE INDEX IX_Vehicles_UserId ON Vehicles(UserID);
CREATE INDEX IX_Reviews_GarageId ON Reviews(GarageID);
CREATE INDEX IX_Notifications_UserId ON Notifications(UserID);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);

-- Thêm dữ liệu mẫu cho bảng Users (Admin)
INSERT INTO Users (Username, PasswordHash, FullName, Email, Role, IsActive)
VALUES 
('admin', 'admin123', N'Administrator', 'admin@mygarage.com', 'Admin', 1),
('technician1', 'tech123', N'Kỹ thuật viên 1', 'tech1@mygarage.com', 'Technician', 1),
('technician2', 'tech123', N'Kỹ thuật viên 2', 'tech2@mygarage.com', 'Technician', 1);

-- Thêm dữ liệu mẫu cho bảng Garages
INSERT INTO Garages (Name, Address, OperatingArea, Description)
VALUES 
(N'Garage Trung Tâm', N'123 Đường ABC, Quận 1, TP.HCM', N'Quận 1, Quận 3', N'Garage chuyên nghiệp tại trung tâm thành phố'),
(N'Garage Củ Chi', N'456 Đường XYZ, Huyện Củ Chi, TP.HCM', N'Huyện Củ Chi', N'Garage phục vụ khu vực ngoại thành');

-- Thêm dữ liệu mẫu cho bảng Services
INSERT INTO Services (ServiceName, Description, Price, image_url)
VALUES 
(N'Thay dầu nhớt', N'Thay dầu nhớt động cơ và lọc dầu', 200000, '/images/oil-change.jpg'),
(N'Thay lốp xe', N'Thay lốp xe và cân bằng bánh', 500000, '/images/tire-change.jpg'),
(N'Bảo dưỡng định kỳ', N'Kiểm tra và bảo dưỡng toàn bộ xe', 800000, '/images/maintenance.jpg'),
(N'Sửa chữa động cơ', N'Chẩn đoán và sửa chữa động cơ', 1500000, '/images/engine-repair.jpg');

-- Thêm dữ liệu mẫu cho bảng Products
INSERT INTO Products (ProductName, Description, Price, ImageUrl)
VALUES 
(N'Dầu nhớt 5W-30', N'Dầu nhớt động cơ chất lượng cao', 150000, '/images/oil-5w30.jpg'),
(N'Lọc dầu', N'Lọc dầu động cơ chính hãng', 50000, '/images/oil-filter.jpg'),
(N'Lốp xe Michelin', N'Lốp xe cao cấp Michelin', 800000, '/images/michelin-tire.jpg'),
(N'Phanh xe', N'Bộ phanh xe chất lượng', 300000, '/images/brake-pads.jpg');

-- Liên kết Garage với Services
INSERT INTO GarageServices (GarageID, ServiceID)
VALUES 
(1, 1), (1, 2), (1, 3), (1, 4),
(2, 1), (2, 2), (2, 3);

-- Thêm lịch làm việc cho Garage
INSERT INTO GarageSchedules (GarageID, DayOfWeek, OpenTime, CloseTime)
VALUES 
(1, N'Thứ 2', '08:00', '18:00'),
(1, N'Thứ 3', '08:00', '18:00'),
(1, N'Thứ 4', '08:00', '18:00'),
(1, N'Thứ 5', '08:00', '18:00'),
(1, N'Thứ 6', '08:00', '18:00'),
(1, N'Thứ 7', '08:00', '17:00'),
(2, N'Thứ 2', '07:00', '19:00'),
(2, N'Thứ 3', '07:00', '19:00'),
(2, N'Thứ 4', '07:00', '19:00'),
(2, N'Thứ 5', '07:00', '19:00'),
(2, N'Thứ 6', '07:00', '19:00'),
(2, N'Thứ 7', '07:00', '18:00');

PRINT N'Database MyGarageFinal đã được tạo thành công với tất cả các bảng và dữ liệu mẫu!'; 