-- Script kiểm tra dữ liệu mẫu đã thêm
USE MyGarageFinal;
GO

PRINT N'=== KIỂM TRA DỮ LIỆU MẪU ===';
PRINT N'';

-- 1. Kiểm tra thông tin khách hàng
PRINT N'1. THÔNG TIN KHÁCH HÀNG:';
SELECT 
    UserID,
    Username,
    FullName,
    Email,
    Phone,
    Role
FROM Users 
WHERE Role = 'Customer'
ORDER BY UserID;
PRINT N'';

-- 2. Kiểm tra xe của khách hàng
PRINT N'2. XE CỦA KHÁCH HÀNG:';
SELECT 
    v.VehicleID,
    u.FullName AS CustomerName,
    v.LicensePlate,
    v.Make,
    v.Model,
    v.Notes
FROM Vehicles v
INNER JOIN Users u ON v.UserID = u.UserID
ORDER BY v.UserID, v.VehicleID;
PRINT N'';

-- 3. Kiểm tra lịch hẹn
PRINT N'3. LỊCH HẸN CỦA KHÁCH HÀNG:';
SELECT 
    a.AppointmentID,
    u.FullName AS CustomerName,
    g.Name AS GarageName,
    a.AppointmentTime,
    a.Status,
    a.TotalAmount,
    a.DiscountAmount,
    a.Notes
FROM Appointments a
INNER JOIN Users u ON a.UserID = u.UserID
INNER JOIN Garages g ON a.GarageID = g.GarageID
WHERE u.Role = 'Customer'
ORDER BY a.AppointmentTime;
PRINT N'';

-- 4. Kiểm tra chi tiết lịch hẹn
PRINT N'4. CHI TIẾT LỊCH HẸN:';
SELECT 
    avd.AppointmentVehicleDetailID,
    a.AppointmentID,
    u.FullName AS CustomerName,
    v.LicensePlate,
    s.ServiceName,
    tech.FullName AS TechnicianName,
    avd.Quantity,
    avd.Note
FROM AppointmentVehicleDetails avd
INNER JOIN Appointments a ON avd.AppointmentID = a.AppointmentID
INNER JOIN Users u ON a.UserID = u.UserID
INNER JOIN Vehicles v ON avd.VehicleID = v.VehicleID
INNER JOIN Services s ON avd.ServiceID = s.ServiceID
LEFT JOIN Users tech ON avd.TechnicianID = tech.UserID
ORDER BY a.AppointmentID;
PRINT N'';

-- 5. Kiểm tra sản phẩm trong lịch hẹn
PRINT N'5. SẢN PHẨM TRONG LỊCH HẸN:';
SELECT 
    apd.AppointmentProductDetailID,
    a.AppointmentID,
    u.FullName AS CustomerName,
    p.ProductName,
    apd.Quantity,
    apd.UnitPrice,
    (apd.Quantity * apd.UnitPrice) AS TotalPrice
FROM AppointmentProductDetails apd
INNER JOIN Appointments a ON apd.AppointmentID = a.AppointmentID
INNER JOIN Users u ON a.UserID = u.UserID
INNER JOIN Products p ON apd.ProductID = p.ProductID
ORDER BY a.AppointmentID;
PRINT N'';

-- 6. Kiểm tra đánh giá
PRINT N'6. ĐÁNH GIÁ CỦA KHÁCH HÀNG:';
SELECT 
    r.ReviewID,
    u.FullName AS CustomerName,
    g.Name AS GarageName,
    r.Rating,
    r.Comment,
    r.CreatedAt
FROM Reviews r
INNER JOIN Users u ON r.UserID = u.UserID
INNER JOIN Garages g ON r.GarageID = g.GarageID
ORDER BY r.CreatedAt DESC;
PRINT N'';

-- 7. Kiểm tra trạng thái sửa chữa
PRINT N'7. TRẠNG THÁI SỬA CHỮA:';
SELECT 
    rs.StatusID,
    a.AppointmentID,
    u.FullName AS CustomerName,
    rs.StatusStep,
    rs.Notes,
    rs.CreatedAt
FROM RepairStatus rs
INNER JOIN Appointments a ON rs.AppointmentID = a.AppointmentID
INNER JOIN Users u ON a.UserID = u.UserID
ORDER BY a.AppointmentID, rs.CreatedAt;
PRINT N'';

-- 8. Kiểm tra báo cáo kỹ thuật
PRINT N'8. BÁO CÁO KỸ THUẬT:';
SELECT 
    tr.ReportID,
    a.AppointmentID,
    u.FullName AS CustomerName,
    tech.FullName AS TechnicianName,
    tr.VehicleStatus,
    tr.PerformedItems,
    tr.Recommendations,
    tr.CreatedAt
FROM TechnicalReports tr
INNER JOIN Appointments a ON tr.AppointmentID = a.AppointmentID
INNER JOIN Users u ON a.UserID = u.UserID
INNER JOIN Users tech ON tr.TechnicianID = tech.UserID
ORDER BY tr.CreatedAt DESC;
PRINT N'';

-- 9. Kiểm tra thông báo
PRINT N'9. THÔNG BÁO CỦA KHÁCH HÀNG:';
SELECT 
    n.NotificationID,
    u.FullName AS CustomerName,
    n.Title,
    n.Message,
    n.IsRead,
    n.CreatedAt
FROM Notifications n
INNER JOIN Users u ON n.UserID = u.UserID
WHERE u.Role = 'Customer'
ORDER BY n.CreatedAt DESC;
PRINT N'';

-- 10. Kiểm tra lịch sử thanh toán
PRINT N'10. LỊCH SỬ THANH TOÁN:';
SELECT 
    ph.PaymentID,
    u.FullName AS CustomerName,
    g.Name AS GarageName,
    ph.Amount,
    ph.PaymentMethod,
    ph.Status,
    ph.TransactionId,
    ph.Description,
    ph.CreatedAt
FROM PaymentHistory ph
INNER JOIN Users u ON ph.UserID = u.UserID
INNER JOIN Appointments a ON ph.AppointmentID = a.AppointmentID
INNER JOIN Garages g ON a.GarageID = g.GarageID
ORDER BY ph.CreatedAt DESC;
PRINT N'';

-- 11. Kiểm tra giỏ hàng
PRINT N'11. GIỎ HÀNG CỦA KHÁCH HÀNG:';
SELECT 
    ci.CartItemID,
    u.FullName AS CustomerName,
    p.ProductName,
    p.Price,
    ci.AddedAt
FROM CartItems ci
INNER JOIN Users u ON ci.UserID = u.UserID
INNER JOIN Products p ON ci.ProductID = p.ProductID
ORDER BY ci.UserID, ci.AddedAt;
PRINT N'';

-- 12. Kiểm tra sản phẩm yêu thích
PRINT N'12. SẢN PHẨM YÊU THÍCH:';
SELECT 
    fp.FavoriteID,
    u.FullName AS CustomerName,
    p.ProductName,
    p.Price,
    fp.CreatedAt
FROM FavoriteProducts fp
INNER JOIN Users u ON fp.UserID = u.UserID
INNER JOIN Products p ON fp.ProductID = p.ProductID
ORDER BY fp.UserID, fp.CreatedAt;
PRINT N'';

-- 13. Thống kê tổng quan
PRINT N'13. THỐNG KÊ TỔNG QUAN:';
SELECT 
    'Tổng số khách hàng' AS Metric,
    COUNT(*) AS Value
FROM Users 
WHERE Role = 'Customer'

UNION ALL

SELECT 
    'Tổng số xe',
    COUNT(*)
FROM Vehicles

UNION ALL

SELECT 
    'Tổng số lịch hẹn',
    COUNT(*)
FROM Appointments

UNION ALL

SELECT 
    'Lịch hẹn đã hoàn thành',
    COUNT(*)
FROM Appointments
WHERE Status = 'Completed'

UNION ALL

SELECT 
    'Lịch hẹn đang xử lý',
    COUNT(*)
FROM Appointments
WHERE Status = 'In Progress'

UNION ALL

SELECT 
    'Tổng số đánh giá',
    COUNT(*)
FROM Reviews

UNION ALL

SELECT 
    'Tổng doanh thu (VNĐ)',
    FORMAT(SUM(TotalAmount), 'N0')
FROM Appointments
WHERE Status = 'Completed'

UNION ALL

SELECT 
    'Tổng giao dịch thanh toán',
    COUNT(*)
FROM PaymentHistory
WHERE Status = 'Completed';
PRINT N'';

-- 14. Đánh giá trung bình theo garage
PRINT N'14. ĐÁNH GIÁ TRUNG BÌNH THEO GARAGE:';
SELECT 
    g.Name AS GarageName,
    AVG(CAST(r.Rating AS FLOAT)) AS AverageRating,
    COUNT(r.ReviewID) AS TotalReviews,
    MIN(r.Rating) AS MinRating,
    MAX(r.Rating) AS MaxRating
FROM Garages g
LEFT JOIN Reviews r ON g.GarageID = r.GarageID
GROUP BY g.GarageID, g.Name
ORDER BY AverageRating DESC;
PRINT N'';

-- 15. Top khách hàng theo số lịch hẹn
PRINT N'15. TOP KHÁCH HÀNG THEO SỐ LỊCH HẸN:';
SELECT 
    u.FullName AS CustomerName,
    COUNT(a.AppointmentID) AS TotalAppointments,
    SUM(ISNULL(a.TotalAmount, 0)) AS TotalSpent,
    AVG(CAST(r.Rating AS FLOAT)) AS AverageRating
FROM Users u
LEFT JOIN Appointments a ON u.UserID = a.UserID
LEFT JOIN Reviews r ON u.UserID = r.UserID
WHERE u.Role = 'Customer'
GROUP BY u.UserID, u.FullName
ORDER BY TotalAppointments DESC, TotalSpent DESC;
PRINT N'';

PRINT N'=== HOÀN THÀNH KIỂM TRA DỮ LIỆU ==='; 