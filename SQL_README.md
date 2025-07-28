# Hướng dẫn sử dụng SQL Scripts cho MyGarageFinal

## Tổng quan
Dự án này bao gồm 6 file SQL chính để thiết lập database cho hệ thống quản lý garage:

1. `create_database.sql` - Tạo database và các bảng
2. `stored_procedures_and_views.sql` - Tạo stored procedures và views
3. `sample_data.sql` - Thêm dữ liệu mẫu cho khách hàng mới (UserID 4, 5, 6)
4. `check_sample_data.sql` - Kiểm tra dữ liệu mẫu đã thêm
5. `reviews_for_user1_2.sql` - Thêm dữ liệu đánh giá cho User 1 và 2
6. `check_reviews_user1_2.sql` - Kiểm tra dữ liệu đánh giá của User 1 và 2

## Cấu trúc Database

### Các bảng chính:

#### 1. Users (Người dùng)
- Quản lý thông tin người dùng (khách hàng, admin, kỹ thuật viên)
- Các trường: UserID, Username, PasswordHash, FullName, Email, Phone, Address, Role, CreatedAt, IsActive

#### 2. Garages (Garage)
- Thông tin các garage
- Các trường: GarageID, Name, Address, OperatingArea, Description

#### 3. Services (Dịch vụ)
- Danh sách các dịch vụ sửa chữa
- Các trường: ServiceID, ServiceName, Description, Price, image_url

#### 4. Vehicles (Xe)
- Thông tin xe của khách hàng
- Các trường: VehicleID, UserID, LicensePlate, Make, Model, Notes

#### 5. Appointments (Lịch hẹn)
- Quản lý lịch hẹn sửa chữa
- Các trường: AppointmentID, UserID, GarageID, AppointmentTime, Status, Notes, PromoCode, TotalAmount

#### 6. Products (Sản phẩm)
- Danh sách sản phẩm bán
- Các trường: ProductID, ProductName, Description, Price, ImageUrl

#### 7. PaymentHistory (Lịch sử thanh toán)
- Quản lý thanh toán qua PayOS
- Các trường: PaymentID, AppointmentID, UserID, Amount, PaymentMethod, Status, TransactionId

## Cách sử dụng

### Bước 1: Tạo Database
```sql
-- Chạy file create_database.sql trong SQL Server Management Studio
-- Hoặc sử dụng lệnh:
sqlcmd -S LAPTOP-QUAIVAT -i create_database.sql
```

### Bước 2: Tạo Stored Procedures và Views
```sql
-- Chạy file stored_procedures_and_views.sql
-- Hoặc sử dụng lệnh:
sqlcmd -S LAPTOP-QUAIVAT -d MyGarageFinal -i stored_procedures_and_views.sql
```

### Bước 3: Thêm dữ liệu mẫu
```sql
-- Chạy file sample_data.sql để thêm dữ liệu mẫu
-- Hoặc sử dụng lệnh:
sqlcmd -S LAPTOP-QUAIVAT -d MyGarageFinal -i sample_data.sql
```

### Bước 4: Kiểm tra dữ liệu
```sql
-- Chạy file check_sample_data.sql để kiểm tra dữ liệu
-- Hoặc sử dụng lệnh:
sqlcmd -S LAPTOP-QUAIVAT -d MyGarageFinal -i check_sample_data.sql
```

### Bước 5: Thêm đánh giá cho User 1 và 2
```sql
-- Chạy file reviews_for_user1_2.sql để thêm đánh giá
-- Hoặc sử dụng lệnh:
sqlcmd -S LAPTOP-QUAIVAT -d MyGarageFinal -i reviews_for_user1_2.sql
```

### Bước 6: Kiểm tra đánh giá User 1 và 2
```sql
-- Chạy file check_reviews_user1_2.sql để kiểm tra đánh giá
-- Hoặc sử dụng lệnh:
sqlcmd -S LAPTOP-QUAIVAT -d MyGarageFinal -i check_reviews_user1_2.sql
```

## Stored Procedures chính

### 1. sp_GetAppointmentDetails
```sql
-- Lấy thông tin chi tiết lịch hẹn
EXEC sp_GetAppointmentDetails @AppointmentID = 1;
```

### 2. sp_CreateAppointment
```sql
-- Tạo lịch hẹn mới
EXEC sp_CreateAppointment 
    @UserID = 1, 
    @GarageID = 1, 
    @AppointmentTime = '2024-01-15 10:00:00',
    @Notes = 'Thay dầu nhớt',
    @PromoCode = 'SAVE10';
```

### 3. sp_UpdateAppointmentStatus
```sql
-- Cập nhật trạng thái lịch hẹn
EXEC sp_UpdateAppointmentStatus 
    @AppointmentID = 1, 
    @Status = 'In Progress',
    @Notes = 'Đang sửa chữa';
```

### 4. sp_GetMonthlyRevenue
```sql
-- Báo cáo doanh thu theo tháng
EXEC sp_GetMonthlyRevenue @Year = 2024, @Month = 1;
```

### 5. sp_GetUserVehicles
```sql
-- Lấy danh sách xe của user
EXEC sp_GetUserVehicles @UserID = 1;
```

## Views chính

### 1. vw_AppointmentOverview
```sql
-- Xem tổng quan lịch hẹn
SELECT * FROM vw_AppointmentOverview;
```

### 2. vw_GarageStatistics
```sql
-- Thống kê garage
SELECT * FROM vw_GarageStatistics;
```

### 3. vw_PopularProducts
```sql
-- Sản phẩm bán chạy
SELECT * FROM vw_PopularProducts;
```

### 4. vw_CustomerInfo
```sql
-- Thông tin khách hàng
SELECT * FROM vw_CustomerInfo;
```

## Queries hữu ích

### 1. Lấy lịch hẹn theo ngày
```sql
SELECT 
    a.AppointmentID,
    a.AppointmentTime,
    u.FullName AS CustomerName,
    g.Name AS GarageName,
    a.Status
FROM Appointments a
INNER JOIN Users u ON a.UserID = u.UserID
INNER JOIN Garages g ON a.GarageID = g.GarageID
WHERE CAST(a.AppointmentTime AS DATE) = '2024-01-15'
ORDER BY a.AppointmentTime;
```

### 2. Thống kê doanh thu theo garage
```sql
SELECT 
    g.Name AS GarageName,
    COUNT(a.AppointmentID) AS TotalAppointments,
    SUM(a.TotalAmount) AS TotalRevenue,
    AVG(a.TotalAmount) AS AverageRevenue
FROM Garages g
LEFT JOIN Appointments a ON g.GarageID = a.GarageID
WHERE a.Status = 'Completed'
GROUP BY g.GarageID, g.Name
ORDER BY TotalRevenue DESC;
```

### 3. Lấy đánh giá garage
```sql
SELECT 
    g.Name AS GarageName,
    AVG(CAST(r.Rating AS FLOAT)) AS AverageRating,
    COUNT(r.ReviewID) AS TotalReviews
FROM Garages g
LEFT JOIN Reviews r ON g.GarageID = r.GarageID
GROUP BY g.GarageID, g.Name
ORDER BY AverageRating DESC;
```

### 4. Lấy sản phẩm trong giỏ hàng
```sql
SELECT 
    p.ProductName,
    p.Price,
    ci.AddedAt
FROM CartItems ci
INNER JOIN Products p ON ci.ProductID = p.ProductID
WHERE ci.UserID = 1;
```

### 5. Lấy lịch hẹn của khách hàng cụ thể
```sql
SELECT 
    a.AppointmentID,
    a.AppointmentTime,
    a.Status,
    a.TotalAmount,
    g.Name AS GarageName,
    v.LicensePlate
FROM Appointments a
INNER JOIN Garages g ON a.GarageID = g.GarageID
INNER JOIN AppointmentVehicleDetails avd ON a.AppointmentID = avd.AppointmentID
INNER JOIN Vehicles v ON avd.VehicleID = v.VehicleID
WHERE a.UserID = 4
ORDER BY a.AppointmentTime DESC;
```

### 6. Lấy đánh giá của khách hàng
```sql
SELECT 
    r.Rating,
    r.Comment,
    r.CreatedAt,
    g.Name AS GarageName
FROM Reviews r
INNER JOIN Garages g ON r.GarageID = g.GarageID
WHERE r.UserID = 4
ORDER BY r.CreatedAt DESC;
```

## Lưu ý quan trọng

1. **Connection String**: Đảm bảo connection string trong `appsettings.json` khớp với server SQL của bạn
2. **Permissions**: User SQL cần có quyền tạo database và bảng
3. **Backup**: Luôn backup database trước khi chạy script
4. **Indexes**: Các indexes đã được tạo để tối ưu hiệu suất truy vấn

## Troubleshooting

### Lỗi thường gặp:

1. **Database đã tồn tại**: Xóa database cũ hoặc thay đổi tên
2. **Permission denied**: Kiểm tra quyền của user SQL
3. **Foreign key constraint**: Đảm bảo thứ tự tạo bảng đúng

### Kiểm tra database:
```sql
-- Kiểm tra các bảng đã tạo
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';

-- Kiểm tra stored procedures
SELECT ROUTINE_NAME 
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_TYPE = 'PROCEDURE';

-- Kiểm tra views
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.VIEWS;
```

## Dữ liệu mẫu đã thêm

### Dữ liệu từ sample_data.sql:
Sau khi chạy `sample_data.sql`, database sẽ có:

#### Khách hàng mẫu:
- **Nguyễn Văn An** (UserID: 4) - 2 xe: Toyota Vios, Honda City
- **Trần Thị Bình** (UserID: 5) - 2 xe: Ford Ranger, Mazda CX-5  
- **Lê Văn Cường** (UserID: 6) - 2 xe: BMW X3, Mercedes C-Class

### Dữ liệu từ reviews_for_user1_2.sql:
Sau khi chạy `reviews_for_user1_2.sql`, database sẽ có:

#### Appointments cho User 1 và 2:
- **User 1**: 3 appointments (thay dầu nhớt, bảo dưỡng phanh, thay lốp xe)
- **User 2**: 3 appointments (bảo dưỡng định kỳ, sửa điện, thay dầu phanh)

#### Đánh giá cho User 1 và 2:
- **User 1**: 3 đánh giá (5★, 4★, 5★) cho các garage khác nhau
- **User 2**: 3 đánh giá (4★, 3★, 5★) cho các garage khác nhau

### Lịch hẹn mẫu:
- **9 lịch hẹn** với các trạng thái khác nhau (Completed, In Progress, Pending, Scheduled)
- **Chi tiết dịch vụ** cho từng lịch hẹn
- **Sản phẩm** được sử dụng trong lịch hẹn
- **Trạng thái sửa chữa** theo thời gian thực
- **Báo cáo kỹ thuật** chi tiết

### Đánh giá mẫu:
- **9 đánh giá** từ các khách hàng
- **Điểm đánh giá** từ 3-5 sao
- **Nhận xét chi tiết** về chất lượng dịch vụ

### Dữ liệu khác:
- **15 thông báo** cho khách hàng
- **6 giao dịch thanh toán** qua PayOS
- **4 sản phẩm** trong giỏ hàng
- **4 sản phẩm** yêu thích

## Liên hệ
Nếu có vấn đề gì, vui lòng kiểm tra:
1. Connection string trong `appsettings.json`
2. Quyền của user SQL
3. Version SQL Server (khuyến nghị SQL Server 2019 trở lên) 