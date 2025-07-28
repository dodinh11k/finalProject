-- Script kiểm tra dữ liệu đánh giá của User 1 và 2
-- Tạo bởi: AI Assistant
-- Ngày: 2024

USE MyGarageFinal;
GO

PRINT N'=== KIỂM TRA DỮ LIỆU ĐÁNH GIÁ USER 1 VÀ 2 ===';

-- 1. Kiểm tra thông tin user 1 và 2
PRINT N'1. THÔNG TIN USER 1 VÀ 2:';
SELECT 
    UserID,
    Username,
    FullName,
    Email,
    Phone,
    Role,
    IsActive
FROM Users 
WHERE UserID IN (1, 2)
ORDER BY UserID;

-- 2. Kiểm tra appointments của user 1 và 2
PRINT N'2. APPOINTMENTS CỦA USER 1 VÀ 2:';
SELECT 
    a.AppointmentID,
    a.UserID,
    u.FullName AS CustomerName,
    g.Name AS GarageName,
    a.AppointmentTime,
    a.Status,
    a.Notes,
    a.TotalAmount,
    a.CreatedAt
FROM Appointments a
INNER JOIN Users u ON a.UserID = u.UserID
INNER JOIN Garages g ON a.GarageID = g.GarageID
WHERE a.UserID IN (1, 2)
ORDER BY a.UserID, a.AppointmentTime;

-- 3. Kiểm tra đánh giá của user 1 và 2
PRINT N'3. ĐÁNH GIÁ CỦA USER 1 VÀ 2:';
SELECT 
    r.ReviewID,
    r.UserID,
    u.FullName AS CustomerName,
    g.Name AS GarageName,
    r.Rating,
    r.Comment,
    r.CreatedAt,
    a.AppointmentTime,
    a.Status AS AppointmentStatus
FROM Reviews r
INNER JOIN Users u ON r.UserID = u.UserID
INNER JOIN Garages g ON r.GarageID = g.GarageID
INNER JOIN Appointments a ON r.AppointmentId = a.AppointmentID
WHERE r.UserID IN (1, 2)
ORDER BY r.UserID, r.CreatedAt DESC;

-- 4. Thống kê đánh giá theo user
PRINT N'4. THỐNG KÊ ĐÁNH GIÁ THEO USER:';
SELECT 
    r.UserID,
    u.FullName AS CustomerName,
    COUNT(*) AS TotalReviews,
    AVG(CAST(r.Rating AS FLOAT)) AS AverageRating,
    MIN(r.Rating) AS MinRating,
    MAX(r.Rating) AS MaxRating
FROM Reviews r
INNER JOIN Users u ON r.UserID = u.UserID
WHERE r.UserID IN (1, 2)
GROUP BY r.UserID, u.FullName
ORDER BY r.UserID;

-- 5. Thống kê đánh giá theo garage
PRINT N'5. THỐNG KÊ ĐÁNH GIÁ THEO GARAGE:';
SELECT 
    g.GarageID,
    g.Name AS GarageName,
    g.Address AS GarageAddress,
    COUNT(*) AS TotalReviews,
    AVG(CAST(r.Rating AS FLOAT)) AS AverageRating,
    MIN(r.Rating) AS MinRating,
    MAX(r.Rating) AS MaxRating
FROM Reviews r
INNER JOIN Garages g ON r.GarageID = g.GarageID
INNER JOIN Appointments a ON r.AppointmentId = a.AppointmentID
WHERE a.UserID IN (1, 2)
GROUP BY g.GarageID, g.Name, g.Address
ORDER BY g.GarageID;

-- 6. Chi tiết đánh giá với thông tin đầy đủ
PRINT N'6. CHI TIẾT ĐÁNH GIÁ ĐẦY ĐỦ:';
SELECT 
    'Review #' + CAST(r.ReviewID AS VARCHAR(10)) AS ReviewInfo,
    u.FullName AS CustomerName,
    g.Name AS GarageName,
    '★' + CAST(r.Rating AS VARCHAR(1)) + '/5' AS RatingDisplay,
    r.Comment,
    FORMAT(r.CreatedAt, 'dd/MM/yyyy HH:mm') AS ReviewDate,
    FORMAT(a.AppointmentTime, 'dd/MM/yyyy HH:mm') AS AppointmentDate,
    a.Notes AS AppointmentNotes,
    FORMAT(a.TotalAmount, 'N0') + ' VNĐ' AS TotalAmount
FROM Reviews r
INNER JOIN Users u ON r.UserID = u.UserID
INNER JOIN Garages g ON r.GarageID = g.GarageID
INNER JOIN Appointments a ON r.AppointmentId = a.AppointmentID
WHERE r.UserID IN (1, 2)
ORDER BY r.CreatedAt DESC;

-- 7. Kiểm tra xem có appointment nào chưa có đánh giá
PRINT N'7. APPOINTMENTS CHƯA CÓ ĐÁNH GIÁ:';
SELECT 
    a.AppointmentID,
    a.UserID,
    u.FullName AS CustomerName,
    g.Name AS GarageName,
    a.AppointmentTime,
    a.Status,
    a.Notes
FROM Appointments a
INNER JOIN Users u ON a.UserID = u.UserID
INNER JOIN Garages g ON a.GarageID = g.GarageID
WHERE a.UserID IN (1, 2)
    AND a.Status = 'Completed'
    AND NOT EXISTS (
        SELECT 1 FROM Reviews r 
        WHERE r.AppointmentId = a.AppointmentID
    )
ORDER BY a.UserID, a.AppointmentTime;

PRINT N'=== HOÀN THÀNH KIỂM TRA ===';
PRINT N'Script kiểm tra đã chạy xong!'; 