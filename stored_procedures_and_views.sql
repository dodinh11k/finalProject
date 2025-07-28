-- Stored Procedures và Views cho hệ thống MyGarageFinal
USE MyGarageFinal;
GO

-- =============================================
-- STORED PROCEDURES
-- =============================================

-- 1. Stored Procedure để lấy thông tin lịch hẹn chi tiết
CREATE PROCEDURE sp_GetAppointmentDetails
    @AppointmentID INT
AS
BEGIN
    SELECT 
        a.AppointmentID,
        a.AppointmentTime,
        a.Status,
        a.Notes,
        a.TotalAmount,
        a.DiscountAmount,
        u.FullName AS CustomerName,
        u.Phone AS CustomerPhone,
        g.Name AS GarageName,
        g.Address AS GarageAddress
    FROM Appointments a
    INNER JOIN Users u ON a.UserID = u.UserID
    INNER JOIN Garages g ON a.GarageID = g.GarageID
    WHERE a.AppointmentID = @AppointmentID;
END;
GO

-- 2. Stored Procedure để lấy danh sách dịch vụ của garage
CREATE PROCEDURE sp_GetGarageServices
    @GarageID INT
AS
BEGIN
    SELECT 
        s.ServiceID,
        s.ServiceName,
        s.Description,
        s.Price,
        s.image_url
    FROM Services s
    INNER JOIN GarageServices gs ON s.ServiceID = gs.ServiceID
    WHERE gs.GarageID = @GarageID;
END;
GO

-- 3. Stored Procedure để tạo lịch hẹn mới
CREATE PROCEDURE sp_CreateAppointment
    @UserID INT,
    @GarageID INT,
    @AppointmentTime DATETIME,
    @Notes NVARCHAR(500) = NULL,
    @PromoCode NVARCHAR(50) = NULL
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
        
        INSERT INTO Appointments (UserID, GarageID, AppointmentTime, Notes, PromoCode, Status, CreatedAt)
        VALUES (@UserID, @GarageID, @AppointmentTime, @Notes, @PromoCode, 'Pending', GETDATE());
        
        DECLARE @AppointmentID INT = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        
        SELECT @AppointmentID AS AppointmentID;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 4. Stored Procedure để cập nhật trạng thái lịch hẹn
CREATE PROCEDURE sp_UpdateAppointmentStatus
    @AppointmentID INT,
    @Status NVARCHAR(50),
    @Notes NVARCHAR(500) = NULL
AS
BEGIN
    UPDATE Appointments 
    SET Status = @Status,
        Notes = ISNULL(@Notes, Notes)
    WHERE AppointmentID = @AppointmentID;
    
    -- Thêm vào bảng RepairStatus
    INSERT INTO RepairStatus (AppointmentID, StatusStep, Notes, CreatedAt)
    VALUES (@AppointmentID, @Status, @Notes, GETDATE());
END;
GO

-- 5. Stored Procedure để lấy báo cáo doanh thu theo tháng
CREATE PROCEDURE sp_GetMonthlyRevenue
    @Year INT,
    @Month INT
AS
BEGIN
    SELECT 
        g.Name AS GarageName,
        COUNT(a.AppointmentID) AS TotalAppointments,
        SUM(ISNULL(a.TotalAmount, 0)) AS TotalRevenue,
        AVG(ISNULL(a.TotalAmount, 0)) AS AverageRevenue
    FROM Garages g
    LEFT JOIN Appointments a ON g.GarageID = a.GarageID
        AND YEAR(a.AppointmentTime) = @Year
        AND MONTH(a.AppointmentTime) = @Month
        AND a.Status = 'Completed'
    GROUP BY g.GarageID, g.Name
    ORDER BY TotalRevenue DESC;
END;
GO

-- 6. Stored Procedure để lấy danh sách xe của user
CREATE PROCEDURE sp_GetUserVehicles
    @UserID INT
AS
BEGIN
    SELECT 
        VehicleID,
        LicensePlate,
        Make,
        Model,
        Notes
    FROM Vehicles
    WHERE UserID = @UserID;
END;
GO

-- 7. Stored Procedure để thêm xe mới
CREATE PROCEDURE sp_AddVehicle
    @UserID INT,
    @LicensePlate NVARCHAR(20),
    @Make NVARCHAR(50),
    @Model NVARCHAR(50),
    @Notes NVARCHAR(255) = NULL
AS
BEGIN
    INSERT INTO Vehicles (UserID, LicensePlate, Make, Model, Notes)
    VALUES (@UserID, @LicensePlate, @Make, @Model, @Notes);
    
    SELECT SCOPE_IDENTITY() AS VehicleID;
END;
GO

-- 8. Stored Procedure để lấy thông báo chưa đọc
CREATE PROCEDURE sp_GetUnreadNotifications
    @UserID INT
AS
BEGIN
    SELECT 
        NotificationID,
        Title,
        Message,
        CreatedAt
    FROM Notifications
    WHERE UserID = @UserID AND IsRead = 0
    ORDER BY CreatedAt DESC;
END;
GO

-- 9. Stored Procedure để đánh dấu thông báo đã đọc
CREATE PROCEDURE sp_MarkNotificationAsRead
    @NotificationID INT
AS
BEGIN
    UPDATE Notifications
    SET IsRead = 1
    WHERE NotificationID = @NotificationID;
END;
GO

-- 10. Stored Procedure để lấy lịch làm việc của garage
CREATE PROCEDURE sp_GetGarageSchedule
    @GarageID INT
AS
BEGIN
    SELECT 
        DayOfWeek,
        OpenTime,
        CloseTime
    FROM GarageSchedules
    WHERE GarageID = @GarageID
    ORDER BY 
        CASE DayOfWeek
            WHEN N'Thứ 2' THEN 1
            WHEN N'Thứ 3' THEN 2
            WHEN N'Thứ 4' THEN 3
            WHEN N'Thứ 5' THEN 4
            WHEN N'Thứ 6' THEN 5
            WHEN N'Thứ 7' THEN 6
            WHEN N'Chủ nhật' THEN 7
            ELSE 8
        END;
END;
GO

-- =============================================
-- VIEWS
-- =============================================

-- 1. View tổng quan lịch hẹn
CREATE VIEW vw_AppointmentOverview AS
SELECT 
    a.AppointmentID,
    a.AppointmentTime,
    a.Status,
    a.TotalAmount,
    u.FullName AS CustomerName,
    u.Phone AS CustomerPhone,
    g.Name AS GarageName,
    COUNT(avd.AppointmentVehicleDetailID) AS ServiceCount
FROM Appointments a
INNER JOIN Users u ON a.UserID = u.UserID
INNER JOIN Garages g ON a.GarageID = g.GarageID
LEFT JOIN AppointmentVehicleDetails avd ON a.AppointmentID = avd.AppointmentID
GROUP BY a.AppointmentID, a.AppointmentTime, a.Status, a.TotalAmount, u.FullName, u.Phone, g.Name;
GO

-- 2. View thống kê garage
CREATE VIEW vw_GarageStatistics AS
SELECT 
    g.GarageID,
    g.Name AS GarageName,
    g.Address,
    COUNT(DISTINCT a.AppointmentID) AS TotalAppointments,
    COUNT(DISTINCT CASE WHEN a.Status = 'Completed' THEN a.AppointmentID END) AS CompletedAppointments,
    AVG(CAST(r.Rating AS FLOAT)) AS AverageRating,
    COUNT(DISTINCT r.ReviewID) AS TotalReviews
FROM Garages g
LEFT JOIN Appointments a ON g.GarageID = a.GarageID
LEFT JOIN Reviews r ON g.GarageID = r.GarageID
GROUP BY g.GarageID, g.Name, g.Address;
GO

-- 3. View sản phẩm bán chạy
CREATE VIEW vw_PopularProducts AS
SELECT 
    p.ProductID,
    p.ProductName,
    p.Price,
    COUNT(oi.OrderItemID) AS OrderCount,
    SUM(oi.Quantity) AS TotalQuantity,
    SUM(oi.Quantity * oi.UnitPrice) AS TotalRevenue
FROM Products p
LEFT JOIN OrderItems oi ON p.ProductID = oi.ProductID
LEFT JOIN Orders o ON oi.OrderID = o.OrderID
WHERE o.Status = 'Completed' OR o.Status IS NULL
GROUP BY p.ProductID, p.ProductName, p.Price
ORDER BY TotalQuantity DESC;
GO

-- 4. View dịch vụ phổ biến
CREATE VIEW vw_PopularServices AS
SELECT 
    s.ServiceID,
    s.ServiceName,
    s.Price,
    COUNT(avd.AppointmentVehicleDetailID) AS AppointmentCount,
    SUM(avd.Quantity) AS TotalQuantity
FROM Services s
LEFT JOIN AppointmentVehicleDetails avd ON s.ServiceID = avd.ServiceID
LEFT JOIN Appointments a ON avd.AppointmentID = a.AppointmentID
WHERE a.Status = 'Completed' OR a.Status IS NULL
GROUP BY s.ServiceID, s.ServiceName, s.Price
ORDER BY AppointmentCount DESC;
GO

-- 5. View thông tin khách hàng
CREATE VIEW vw_CustomerInfo AS
SELECT 
    u.UserID,
    u.FullName,
    u.Email,
    u.Phone,
    u.Address,
    u.CreatedAt AS RegistrationDate,
    COUNT(DISTINCT a.AppointmentID) AS TotalAppointments,
    COUNT(DISTINCT v.VehicleID) AS TotalVehicles,
    SUM(ISNULL(a.TotalAmount, 0)) AS TotalSpent
FROM Users u
LEFT JOIN Appointments a ON u.UserID = a.UserID
LEFT JOIN Vehicles v ON u.UserID = v.UserID
WHERE u.Role = 'Customer' OR u.Role IS NULL
GROUP BY u.UserID, u.FullName, u.Email, u.Phone, u.Address, u.CreatedAt;
GO

-- 6. View lịch hẹn chi tiết với dịch vụ
CREATE VIEW vw_AppointmentDetails AS
SELECT 
    a.AppointmentID,
    a.AppointmentTime,
    a.Status,
    a.TotalAmount,
    u.FullName AS CustomerName,
    g.Name AS GarageName,
    v.LicensePlate,
    v.Make,
    v.Model,
    s.ServiceName,
    avd.Quantity,
    tech.FullName AS TechnicianName
FROM Appointments a
INNER JOIN Users u ON a.UserID = u.UserID
INNER JOIN Garages g ON a.GarageID = g.GarageID
INNER JOIN AppointmentVehicleDetails avd ON a.AppointmentID = avd.AppointmentID
INNER JOIN Vehicles v ON avd.VehicleID = v.VehicleID
INNER JOIN Services s ON avd.ServiceID = s.ServiceID
LEFT JOIN Users tech ON avd.TechnicianID = tech.UserID;
GO

-- 7. View báo cáo thanh toán
CREATE VIEW vw_PaymentReport AS
SELECT 
    ph.PaymentID,
    ph.Amount,
    ph.PaymentMethod,
    ph.Status,
    ph.CreatedAt,
    u.FullName AS CustomerName,
    g.Name AS GarageName,
    a.AppointmentTime
FROM PaymentHistory ph
INNER JOIN Users u ON ph.UserID = u.UserID
INNER JOIN Appointments a ON ph.AppointmentID = a.AppointmentID
INNER JOIN Garages g ON a.GarageID = g.GarageID;
GO

-- 8. View đánh giá garage
CREATE VIEW vw_GarageReviews AS
SELECT 
    r.ReviewID,
    r.Rating,
    r.Comment,
    r.CreatedAt,
    u.FullName AS CustomerName,
    g.Name AS GarageName,
    g.Address AS GarageAddress
FROM Reviews r
INNER JOIN Users u ON r.UserID = u.UserID
INNER JOIN Garages g ON r.GarageID = g.GarageID
ORDER BY r.CreatedAt DESC;
GO

PRINT N'Đã tạo thành công tất cả Stored Procedures và Views!'; 