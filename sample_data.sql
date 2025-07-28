-- Script thêm dữ liệu mẫu cho MyGarageFinal
USE MyGarageFinal;
GO

-- Thêm dữ liệu mẫu cho Users (khách hàng)
INSERT INTO Users (Username, PasswordHash, FullName, Email, Phone, Address, Role, IsActive)
VALUES 
('customer1', 'customer123', N'Nguyễn Văn An', 'an.nguyen@email.com', '0901234567', N'123 Đường ABC, Quận 1, TP.HCM', 'Customer', 1),
('customer2', 'customer123', N'Trần Thị Bình', 'binh.tran@email.com', '0902345678', N'456 Đường XYZ, Quận 3, TP.HCM', 'Customer', 1),
('customer3', 'customer123', N'Lê Văn Cường', 'cuong.le@email.com', '0903456789', N'789 Đường DEF, Quận 7, TP.HCM', 'Customer', 1);

-- Thêm xe cho các khách hàng
INSERT INTO Vehicles (UserID, LicensePlate, Make, Model, Notes)
VALUES 
-- Xe của customer1 (UserID = 4)
(4, '51A-12345', 'Toyota', 'Vios', N'Xe gia đình, đã sử dụng 3 năm'),
(4, '51B-67890', 'Honda', 'City', N'Xe công ty, mới mua 1 năm'),

-- Xe của customer2 (UserID = 5)
(5, '51C-11111', 'Ford', 'Ranger', N'Xe bán tải, dùng để chở hàng'),
(5, '51D-22222', 'Mazda', 'CX-5', N'Xe SUV gia đình'),

-- Xe của customer3 (UserID = 6)
(6, '51E-33333', 'BMW', 'X3', N'Xe cao cấp, cần bảo dưỡng định kỳ'),
(6, '51F-44444', 'Mercedes', 'C-Class', N'Xe sang trọng, đã sử dụng 2 năm');

-- Thêm lịch hẹn cho các khách hàng
INSERT INTO Appointments (UserID, GarageID, AppointmentTime, Status, Notes, PromoCode, DiscountAmount, TotalAmount, CreatedAt)
VALUES 
-- Lịch hẹn của customer1
(4, 1, '2024-01-15 09:00:00', 'Completed', N'Thay dầu nhớt và lọc dầu', 'SAVE10', 20000, 180000, '2024-01-10 14:30:00'),
(4, 1, '2024-01-20 14:00:00', 'Completed', N'Bảo dưỡng định kỳ', NULL, 0, 800000, '2024-01-15 10:15:00'),
(4, 2, '2024-01-25 10:30:00', 'In Progress', N'Thay lốp xe', 'TIRE20', 100000, 400000, '2024-01-20 16:45:00'),

-- Lịch hẹn của customer2
(5, 1, '2024-01-16 11:00:00', 'Completed', N'Sửa chữa động cơ', NULL, 0, 1500000, '2024-01-12 09:20:00'),
(5, 2, '2024-01-22 08:00:00', 'Completed', N'Thay dầu nhớt', 'OIL15', 30000, 170000, '2024-01-18 15:30:00'),
(5, 1, '2024-01-28 13:30:00', 'Pending', N'Bảo dưỡng định kỳ', NULL, 0, 800000, '2024-01-25 11:00:00'),

-- Lịch hẹn của customer3
(6, 1, '2024-01-17 15:00:00', 'Completed', N'Bảo dưỡng cao cấp', 'VIP20', 160000, 640000, '2024-01-14 12:00:00'),
(6, 2, '2024-01-24 16:00:00', 'Completed', N'Thay phanh xe', NULL, 0, 300000, '2024-01-21 14:20:00'),
(6, 1, '2024-01-30 10:00:00', 'Scheduled', N'Kiểm tra tổng thể', NULL, 0, 500000, '2024-01-27 16:30:00');

-- Thêm chi tiết lịch hẹn (AppointmentVehicleDetails)
INSERT INTO AppointmentVehicleDetails (AppointmentID, VehicleID, ServiceID, TechnicianID, Quantity, Note, CreatedAt)
VALUES 
-- Chi tiết lịch hẹn 1 (customer1 - thay dầu)
(1, 1, 1, 2, 1, N'Thay dầu nhớt 5W-30', '2024-01-10 14:30:00'),

-- Chi tiết lịch hẹn 2 (customer1 - bảo dưỡng)
(2, 1, 3, 3, 1, N'Bảo dưỡng toàn bộ xe', '2024-01-15 10:15:00'),

-- Chi tiết lịch hẹn 3 (customer1 - thay lốp)
(3, 2, 2, 2, 4, N'Thay 4 lốp xe', '2024-01-20 16:45:00'),

-- Chi tiết lịch hẹn 4 (customer2 - sửa động cơ)
(4, 3, 4, 3, 1, N'Sửa chữa động cơ diesel', '2024-01-12 09:20:00'),

-- Chi tiết lịch hẹn 5 (customer2 - thay dầu)
(5, 4, 1, 2, 1, N'Thay dầu nhớt động cơ', '2024-01-18 15:30:00'),

-- Chi tiết lịch hẹn 6 (customer2 - bảo dưỡng)
(6, 3, 3, 3, 1, N'Bảo dưỡng xe bán tải', '2024-01-25 11:00:00'),

-- Chi tiết lịch hẹn 7 (customer3 - bảo dưỡng cao cấp)
(7, 5, 3, 3, 1, N'Bảo dưỡng xe BMW', '2024-01-14 12:00:00'),

-- Chi tiết lịch hẹn 8 (customer3 - thay phanh)
(8, 6, 1, 2, 1, N'Thay phanh xe Mercedes', '2024-01-21 14:20:00'),

-- Chi tiết lịch hẹn 9 (customer3 - kiểm tra)
(9, 5, 3, 3, 1, N'Kiểm tra tổng thể BMW', '2024-01-27 16:30:00');

-- Thêm sản phẩm vào lịch hẹn (AppointmentProductDetails)
INSERT INTO AppointmentProductDetails (AppointmentID, ProductID, Quantity, UnitPrice)
VALUES 
-- Sản phẩm cho lịch hẹn 1 (thay dầu)
(1, 1, 1, 150000), -- Dầu nhớt 5W-30
(1, 2, 1, 50000),  -- Lọc dầu

-- Sản phẩm cho lịch hẹn 2 (bảo dưỡng)
(2, 1, 1, 150000), -- Dầu nhớt
(2, 2, 1, 50000),  -- Lọc dầu
(2, 4, 1, 300000), -- Phanh xe

-- Sản phẩm cho lịch hẹn 3 (thay lốp)
(3, 3, 4, 800000), -- 4 lốp xe Michelin

-- Sản phẩm cho lịch hẹn 4 (sửa động cơ)
(4, 1, 2, 150000), -- 2 lít dầu nhớt
(4, 2, 2, 50000),  -- 2 lọc dầu

-- Sản phẩm cho lịch hẹn 5 (thay dầu)
(5, 1, 1, 150000), -- Dầu nhớt
(5, 2, 1, 50000),  -- Lọc dầu

-- Sản phẩm cho lịch hẹn 7 (bảo dưỡng BMW)
(7, 1, 1, 150000), -- Dầu nhớt
(7, 2, 1, 50000),  -- Lọc dầu
(7, 4, 1, 300000), -- Phanh xe

-- Sản phẩm cho lịch hẹn 8 (thay phanh Mercedes)
(8, 4, 1, 300000); -- Phanh xe

-- Thêm đánh giá (Reviews)
INSERT INTO Reviews (UserID, GarageID, Rating, Comment, CreatedAt)
VALUES 
-- Đánh giá của customer1 cho Garage Trung Tâm
(4, 1, 5, N'Dịch vụ rất tốt, nhân viên chuyên nghiệp, giá cả hợp lý. Sẽ quay lại!', '2024-01-15 18:00:00'),
(4, 1, 4, N'Bảo dưỡng xe rất kỹ lưỡng, nhưng hơi chậm một chút.', '2024-01-20 19:30:00'),

-- Đánh giá của customer1 cho Garage Củ Chi
(4, 2, 3, N'Thay lốp xe nhanh, nhưng giá hơi cao so với nơi khác.', '2024-01-25 16:45:00'),

-- Đánh giá của customer2 cho Garage Trung Tâm
(5, 1, 5, N'Sửa chữa động cơ rất giỏi, xe chạy mượt hơn hẳn sau khi sửa.', '2024-01-16 20:15:00'),
(5, 1, 4, N'Dịch vụ tốt, nhưng cần cải thiện thời gian chờ.', '2024-01-28 17:00:00'),

-- Đánh giá của customer2 cho Garage Củ Chi
(5, 2, 4, N'Thay dầu nhớt nhanh gọn, giá cả phải chăng.', '2024-01-22 12:30:00'),

-- Đánh giá của customer3 cho Garage Trung Tâm
(6, 1, 5, N'Bảo dưỡng xe BMW rất chuyên nghiệp, đúng tiêu chuẩn hãng.', '2024-01-17 21:00:00'),
(6, 1, 5, N'Kiểm tra tổng thể rất kỹ lưỡng, tư vấn nhiệt tình.', '2024-01-30 15:30:00'),

-- Đánh giá của customer3 cho Garage Củ Chi
(6, 2, 4, N'Thay phanh xe Mercedes chất lượng tốt, nhưng cần đặt lịch trước.', '2024-01-24 18:45:00');

-- Thêm trạng thái sửa chữa (RepairStatus)
INSERT INTO RepairStatus (AppointmentID, StatusStep, Notes, CreatedAt)
VALUES 
-- Trạng thái cho lịch hẹn 1 (customer1 - thay dầu)
(1, 'Confirmed', N'Xác nhận lịch hẹn thay dầu nhớt', '2024-01-10 14:30:00'),
(1, 'In Progress', N'Đang thay dầu nhớt và lọc dầu', '2024-01-15 09:15:00'),
(1, 'Completed', N'Hoàn thành thay dầu nhớt', '2024-01-15 10:30:00'),

-- Trạng thái cho lịch hẹn 2 (customer1 - bảo dưỡng)
(2, 'Confirmed', N'Xác nhận lịch hẹn bảo dưỡng', '2024-01-15 10:15:00'),
(2, 'In Progress', N'Đang bảo dưỡng toàn bộ xe', '2024-01-20 14:15:00'),
(2, 'Completed', N'Hoàn thành bảo dưỡng', '2024-01-20 16:45:00'),

-- Trạng thái cho lịch hẹn 3 (customer1 - thay lốp)
(3, 'Confirmed', N'Xác nhận lịch hẹn thay lốp', '2024-01-20 16:45:00'),
(3, 'In Progress', N'Đang thay 4 lốp xe', '2024-01-25 10:45:00'),

-- Trạng thái cho lịch hẹn 4 (customer2 - sửa động cơ)
(4, 'Confirmed', N'Xác nhận lịch hẹn sửa động cơ', '2024-01-12 09:20:00'),
(4, 'In Progress', N'Đang chẩn đoán và sửa động cơ', '2024-01-16 11:15:00'),
(4, 'Completed', N'Hoàn thành sửa chữa động cơ', '2024-01-16 17:30:00'),

-- Trạng thái cho lịch hẹn 5 (customer2 - thay dầu)
(5, 'Confirmed', N'Xác nhận lịch hẹn thay dầu', '2024-01-18 15:30:00'),
(5, 'In Progress', N'Đang thay dầu nhớt', '2024-01-22 08:15:00'),
(5, 'Completed', N'Hoàn thành thay dầu nhớt', '2024-01-22 09:30:00'),

-- Trạng thái cho lịch hẹn 6 (customer2 - bảo dưỡng)
(6, 'Confirmed', N'Xác nhận lịch hẹn bảo dưỡng', '2024-01-25 11:00:00'),

-- Trạng thái cho lịch hẹn 7 (customer3 - bảo dưỡng BMW)
(7, 'Confirmed', N'Xác nhận lịch hẹn bảo dưỡng BMW', '2024-01-14 12:00:00'),
(7, 'In Progress', N'Đang bảo dưỡng xe BMW', '2024-01-17 15:15:00'),
(7, 'Completed', N'Hoàn thành bảo dưỡng BMW', '2024-01-17 18:45:00'),

-- Trạng thái cho lịch hẹn 8 (customer3 - thay phanh)
(8, 'Confirmed', N'Xác nhận lịch hẹn thay phanh', '2024-01-21 14:20:00'),
(8, 'In Progress', N'Đang thay phanh xe Mercedes', '2024-01-24 16:15:00'),
(8, 'Completed', N'Hoàn thành thay phanh', '2024-01-24 17:30:00'),

-- Trạng thái cho lịch hẹn 9 (customer3 - kiểm tra)
(9, 'Confirmed', N'Xác nhận lịch hẹn kiểm tra', '2024-01-27 16:30:00');

-- Thêm báo cáo kỹ thuật (TechnicalReports)
INSERT INTO TechnicalReports (AppointmentID, TechnicianID, VehicleStatus, PerformedItems, Recommendations, CreatedAt)
VALUES 
-- Báo cáo cho lịch hẹn 1 (thay dầu)
(1, 2, N'Tốt', N'Thay dầu nhớt 5W-30, thay lọc dầu mới', N'Định kỳ thay dầu sau 5000km', '2024-01-15 10:30:00'),

-- Báo cáo cho lịch hẹn 2 (bảo dưỡng)
(2, 3, N'Tốt', N'Bảo dưỡng toàn bộ xe, thay dầu nhớt, kiểm tra phanh, lốp xe', N'Kiểm tra lại sau 10000km', '2024-01-20 16:45:00'),

-- Báo cáo cho lịch hẹn 4 (sửa động cơ)
(4, 3, N'Đã sửa', N'Sửa chữa động cơ diesel, thay phụ tùng hỏng', N'Chạy nhẹ nhàng trong 1000km đầu', '2024-01-16 17:30:00'),

-- Báo cáo cho lịch hẹn 5 (thay dầu)
(5, 2, N'Tốt', N'Thay dầu nhớt động cơ, kiểm tra tổng thể', N'Định kỳ bảo dưỡng sau 5000km', '2024-01-22 09:30:00'),

-- Báo cáo cho lịch hẹn 7 (bảo dưỡng BMW)
(7, 3, N'Tốt', N'Bảo dưỡng xe BMW theo tiêu chuẩn hãng', N'Sử dụng dầu nhớt cao cấp, bảo dưỡng định kỳ', '2024-01-17 18:45:00'),

-- Báo cáo cho lịch hẹn 8 (thay phanh)
(8, 2, N'Tốt', N'Thay phanh xe Mercedes, kiểm tra hệ thống phanh', N'Kiểm tra phanh sau 20000km', '2024-01-24 17:30:00');

-- Thêm thông báo (Notifications)
INSERT INTO Notifications (UserID, Title, Message, IsRead, CreatedAt)
VALUES 
-- Thông báo cho customer1
(4, N'Xác nhận lịch hẹn', N'Lịch hẹn thay dầu nhớt ngày 15/01/2024 đã được xác nhận', 1, '2024-01-10 14:30:00'),
(4, N'Hoàn thành dịch vụ', N'Dịch vụ thay dầu nhớt đã hoàn thành. Vui lòng đến nhận xe', 1, '2024-01-15 10:30:00'),
(4, N'Xác nhận lịch hẹn', N'Lịch hẹn bảo dưỡng ngày 20/01/2024 đã được xác nhận', 1, '2024-01-15 10:15:00'),
(4, N'Hoàn thành dịch vụ', N'Dịch vụ bảo dưỡng đã hoàn thành. Vui lòng đến nhận xe', 1, '2024-01-20 16:45:00'),
(4, N'Xác nhận lịch hẹn', N'Lịch hẹn thay lốp xe ngày 25/01/2024 đã được xác nhận', 0, '2024-01-20 16:45:00'),

-- Thông báo cho customer2
(5, N'Xác nhận lịch hẹn', N'Lịch hẹn sửa động cơ ngày 16/01/2024 đã được xác nhận', 1, '2024-01-12 09:20:00'),
(5, N'Hoàn thành dịch vụ', N'Dịch vụ sửa động cơ đã hoàn thành. Vui lòng đến nhận xe', 1, '2024-01-16 17:30:00'),
(5, N'Xác nhận lịch hẹn', N'Lịch hẹn thay dầu ngày 22/01/2024 đã được xác nhận', 1, '2024-01-18 15:30:00'),
(5, N'Hoàn thành dịch vụ', N'Dịch vụ thay dầu đã hoàn thành. Vui lòng đến nhận xe', 1, '2024-01-22 09:30:00'),
(5, N'Xác nhận lịch hẹn', N'Lịch hẹn bảo dưỡng ngày 28/01/2024 đã được xác nhận', 0, '2024-01-25 11:00:00'),

-- Thông báo cho customer3
(6, N'Xác nhận lịch hẹn', N'Lịch hẹn bảo dưỡng BMW ngày 17/01/2024 đã được xác nhận', 1, '2024-01-14 12:00:00'),
(6, N'Hoàn thành dịch vụ', N'Dịch vụ bảo dưỡng BMW đã hoàn thành. Vui lòng đến nhận xe', 1, '2024-01-17 18:45:00'),
(6, N'Xác nhận lịch hẹn', N'Lịch hẹn thay phanh ngày 24/01/2024 đã được xác nhận', 1, '2024-01-21 14:20:00'),
(6, N'Hoàn thành dịch vụ', N'Dịch vụ thay phanh đã hoàn thành. Vui lòng đến nhận xe', 1, '2024-01-24 17:30:00'),
(6, N'Xác nhận lịch hẹn', N'Lịch hẹn kiểm tra tổng thể ngày 30/01/2024 đã được xác nhận', 0, '2024-01-27 16:30:00');

-- Thêm lịch sử thanh toán (PaymentHistory)
INSERT INTO PaymentHistory (AppointmentID, UserID, Amount, PaymentMethod, Status, TransactionId, Description, CreatedAt)
VALUES 
-- Thanh toán cho lịch hẹn 1
(1, 4, 180000, 'PayOS', 'Completed', 'TXN001', N'Thanh toán dịch vụ thay dầu nhớt', '2024-01-15 10:30:00'),

-- Thanh toán cho lịch hẹn 2
(2, 4, 800000, 'PayOS', 'Completed', 'TXN002', N'Thanh toán dịch vụ bảo dưỡng', '2024-01-20 16:45:00'),

-- Thanh toán cho lịch hẹn 4
(4, 5, 1500000, 'PayOS', 'Completed', 'TXN003', N'Thanh toán dịch vụ sửa động cơ', '2024-01-16 17:30:00'),

-- Thanh toán cho lịch hẹn 5
(5, 5, 170000, 'PayOS', 'Completed', 'TXN004', N'Thanh toán dịch vụ thay dầu', '2024-01-22 09:30:00'),

-- Thanh toán cho lịch hẹn 7
(7, 6, 640000, 'PayOS', 'Completed', 'TXN005', N'Thanh toán dịch vụ bảo dưỡng BMW', '2024-01-17 18:45:00'),

-- Thanh toán cho lịch hẹn 8
(8, 6, 300000, 'PayOS', 'Completed', 'TXN006', N'Thanh toán dịch vụ thay phanh', '2024-01-24 17:30:00');

-- Thêm sản phẩm vào giỏ hàng (CartItems)
INSERT INTO CartItems (UserID, ProductID, AddedAt)
VALUES 
(4, 1, '2024-01-26 10:00:00'), -- customer1 thêm dầu nhớt vào giỏ
(4, 2, '2024-01-26 10:05:00'), -- customer1 thêm lọc dầu vào giỏ
(5, 3, '2024-01-26 14:30:00'), -- customer2 thêm lốp xe vào giỏ
(6, 4, '2024-01-26 16:45:00'); -- customer3 thêm phanh xe vào giỏ

-- Thêm sản phẩm yêu thích (FavoriteProducts)
INSERT INTO FavoriteProducts (UserID, ProductID, CreatedAt)
VALUES 
(4, 1, '2024-01-15 18:00:00'), -- customer1 yêu thích dầu nhớt
(4, 3, '2024-01-25 16:45:00'), -- customer1 yêu thích lốp xe
(5, 2, '2024-01-16 20:15:00'), -- customer2 yêu thích lọc dầu
(6, 4, '2024-01-17 21:00:00'); -- customer3 yêu thích phanh xe

PRINT N'Đã thêm thành công dữ liệu mẫu cho các user 1, 2, 3!';
PRINT N'- 3 khách hàng mới với thông tin đầy đủ';
PRINT N'- 6 xe của các khách hàng';
PRINT N'- 9 lịch hẹn với trạng thái khác nhau';
PRINT N'- 9 chi tiết lịch hẹn với dịch vụ cụ thể';
PRINT N'- 8 sản phẩm được thêm vào lịch hẹn';
PRINT N'- 9 đánh giá từ các khách hàng';
PRINT N'- 15 trạng thái sửa chữa';
PRINT N'- 6 báo cáo kỹ thuật';
PRINT N'- 15 thông báo';
PRINT N'- 6 giao dịch thanh toán';
PRINT N'- 4 sản phẩm trong giỏ hàng';
PRINT N'- 4 sản phẩm yêu thích'; 