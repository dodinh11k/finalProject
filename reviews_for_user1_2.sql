-- Script thêm dữ liệu đánh giá cho User 1 và 2
-- Tạo bởi: AI Assistant
-- Ngày: 2024

USE MyGarageFinal;
GO

PRINT N'=== THÊM DỮ LIỆU ĐÁNH GIÁ CHO USER 1 VÀ 2 ===';

-- 1. Kiểm tra và tạo appointments cho user 1 và 2 nếu chưa có
PRINT N'1. Kiểm tra appointments hiện có cho User 1 và 2...';

-- Kiểm tra xem user 1 và 2 có appointments chưa
IF NOT EXISTS (SELECT 1 FROM Appointments WHERE UserID = 1)
BEGIN
    PRINT N'   - Tạo appointment cho User 1...';
    INSERT INTO Appointments (UserID, GarageID, AppointmentTime, Status, Notes, TotalAmount, CreatedAt)
    VALUES 
    (1, 1, '2024-01-10 09:00:00', 'Completed', N'Thay dầu nhớt và lọc dầu', 200000, '2024-01-05 14:30:00'),
    (1, 2, '2024-01-20 14:00:00', 'Completed', N'Kiểm tra và bảo dưỡng phanh', 350000, '2024-01-15 10:15:00'),
    (1, 1, '2024-02-05 10:30:00', 'Completed', N'Thay lốp xe', 800000, '2024-01-30 16:45:00');
END

IF NOT EXISTS (SELECT 1 FROM Appointments WHERE UserID = 2)
BEGIN
    PRINT N'   - Tạo appointment cho User 2...';
    INSERT INTO Appointments (UserID, GarageID, AppointmentTime, Status, Notes, TotalAmount, CreatedAt)
    VALUES 
    (2, 1, '2024-01-12 11:00:00', 'Completed', N'Bảo dưỡng định kỳ', 450000, '2024-01-08 09:20:00'),
    (2, 3, '2024-01-25 16:00:00', 'Completed', N'Sửa chữa hệ thống điện', 600000, '2024-01-20 13:10:00'),
    (2, 2, '2024-02-08 08:30:00', 'Completed', N'Thay dầu phanh và kiểm tra hệ thống làm mát', 280000, '2024-02-03 11:30:00');
END

-- 2. Thêm dữ liệu đánh giá cho User 1
PRINT N'2. Thêm đánh giá cho User 1...';

-- Lấy appointment IDs của user 1
DECLARE @User1Appointment1 INT = (SELECT TOP 1 AppointmentID FROM Appointments WHERE UserID = 1 ORDER BY AppointmentTime);
DECLARE @User1Appointment2 INT = (SELECT TOP 1 AppointmentID FROM Appointments WHERE UserID = 1 ORDER BY AppointmentTime DESC);
DECLARE @User1Appointment3 INT = (SELECT TOP 1 AppointmentID FROM Appointments WHERE UserID = 1 AND AppointmentID NOT IN (@User1Appointment1, @User1Appointment2));

INSERT INTO Reviews (UserID, GarageID, AppointmentId, Rating, Comment, CreatedAt)
VALUES 
-- Đánh giá cho garage 1 (appointment đầu tiên)
(@User1Appointment1, 1, @User1Appointment1, 5, N'Dịch vụ rất tốt! Nhân viên chuyên nghiệp, thao tác nhanh chóng. Giá cả hợp lý và minh bạch. Sẽ quay lại lần sau.', DATEADD(day, 1, (SELECT AppointmentTime FROM Appointments WHERE AppointmentID = @User1Appointment1))),

-- Đánh giá cho garage 2 (appointment thứ hai)
(@User1Appointment2, 2, @User1Appointment2, 4, N'Garage sạch sẽ, thợ kỹ thuật có tay nghề cao. Hệ thống phanh được kiểm tra kỹ lưỡng. Chỉ trừ 1 sao vì chờ hơi lâu.', DATEADD(day, 1, (SELECT AppointmentTime FROM Appointments WHERE AppointmentID = @User1Appointment2))),

-- Đánh giá cho garage 1 (appointment thứ ba)
(@User1Appointment3, 1, @User1Appointment3, 5, N'Thay lốp xe nhanh chóng, chất lượng lốp tốt. Nhân viên tư vấn nhiệt tình về các loại lốp phù hợp. Rất hài lòng!', DATEADD(day, 1, (SELECT AppointmentTime FROM Appointments WHERE AppointmentID = @User1Appointment3)));

-- 3. Thêm dữ liệu đánh giá cho User 2
PRINT N'3. Thêm đánh giá cho User 2...';

-- Lấy appointment IDs của user 2
DECLARE @User2Appointment1 INT = (SELECT TOP 1 AppointmentID FROM Appointments WHERE UserID = 2 ORDER BY AppointmentTime);
DECLARE @User2Appointment2 INT = (SELECT TOP 1 AppointmentID FROM Appointments WHERE UserID = 2 ORDER BY AppointmentTime DESC);
DECLARE @User2Appointment3 INT = (SELECT TOP 1 AppointmentID FROM Appointments WHERE UserID = 2 AND AppointmentID NOT IN (@User2Appointment1, @User2Appointment2));

INSERT INTO Reviews (UserID, GarageID, AppointmentId, Rating, Comment, CreatedAt)
VALUES 
-- Đánh giá cho garage 1 (appointment đầu tiên)
(@User2Appointment1, 1, @User2Appointment1, 4, N'Bảo dưỡng định kỳ được thực hiện đầy đủ các bước. Thợ kỹ thuật giải thích rõ ràng những gì đã làm. Giá hơi cao nhưng chất lượng tốt.', DATEADD(day, 1, (SELECT AppointmentTime FROM Appointments WHERE AppointmentID = @User2Appointment1))),

-- Đánh giá cho garage 3 (appointment thứ hai)
(@User2Appointment2, 3, @User2Appointment2, 3, N'Sửa chữa hệ thống điện thành công nhưng mất nhiều thời gian hơn dự kiến. Thợ kỹ thuật có kinh nghiệm nhưng cần cải thiện về thời gian.', DATEADD(day, 1, (SELECT AppointmentTime FROM Appointments WHERE AppointmentID = @User2Appointment2))),

-- Đánh giá cho garage 2 (appointment thứ ba)
(@User2Appointment3, 2, @User2Appointment3, 5, N'Dịch vụ xuất sắc! Thay dầu phanh và kiểm tra hệ thống làm mát rất kỹ lưỡng. Nhân viên thân thiện, giá cả phải chăng. Chắc chắn sẽ giới thiệu cho bạn bè.', DATEADD(day, 1, (SELECT AppointmentTime FROM Appointments WHERE AppointmentID = @User2Appointment3)));

-- 4. Hiển thị kết quả
PRINT N'4. Kiểm tra dữ liệu đã thêm...';

SELECT 
    'Tổng số đánh giá đã thêm' AS ThongKe,
    COUNT(*) AS SoLuong
FROM Reviews r
INNER JOIN Appointments a ON r.AppointmentId = a.AppointmentID
WHERE a.UserID IN (1, 2);

PRINT N'=== CHI TIẾT ĐÁNH GIÁ CỦA USER 1 VÀ 2 ===';
SELECT 
    r.ReviewID,
    u.FullName AS CustomerName,
    g.Name AS GarageName,
    r.Rating,
    r.Comment,
    r.CreatedAt,
    a.AppointmentTime,
    a.Status
FROM Reviews r
INNER JOIN Users u ON r.UserID = u.UserID
INNER JOIN Garages g ON r.GarageID = g.GarageID
INNER JOIN Appointments a ON r.AppointmentId = a.AppointmentID
WHERE u.UserID IN (1, 2)
ORDER BY r.CreatedAt DESC;

PRINT N'=== HOÀN THÀNH THÊM DỮ LIỆU ĐÁNH GIÁ ===';
PRINT N'Đã thêm thành công dữ liệu đánh giá cho User 1 và 2!'; 