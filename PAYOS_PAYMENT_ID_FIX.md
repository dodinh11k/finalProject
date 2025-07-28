# Sửa vấn đề Payment ID không match với PayOS

## Vấn đề
- Payment ID trong database đã lên đến 48
- PayOS không hiển thị trang QR thanh toán
- Hệ thống chuyển sang Mock Payment Gateway

## Nguyên nhân
PayOS yêu cầu orderCode phải là duy nhất và không được trùng lặp. Payment ID hiện tại (48) có thể đã được sử dụng trước đó trong PayOS.

## Giải pháp

### 1. Reset Payment ID Sequence
Chạy script SQL để reset Payment ID về 1:

```sql
-- Xóa tất cả dữ liệu trong bảng PaymentHistory
DELETE FROM PaymentHistory;

-- Reset identity seed về 1
DBCC CHECKIDENT ('PaymentHistory', RESEED, 0);

-- Kiểm tra lại identity seed
DBCC CHECKIDENT ('PaymentHistory');
```

### 2. Sử dụng Payment ID làm OrderCode
Đã cập nhật logic để sử dụng Payment ID làm orderCode:

```csharp
// Sử dụng Payment ID làm orderCode để đảm bảo match với PayOS
// PayOS yêu cầu orderCode phải là số nguyên dương và duy nhất
long orderCode = payment.PaymentId;
```

### 3. Sử dụng Production Environment
Đã chuyển từ sandbox sang production environment:

```csharp
// Sử dụng PayOS Production environment
_httpClient.BaseAddress = new Uri("https://api-merchant.payos.vn/");
```

## Các thay đổi đã thực hiện

### Controllers/PaymentController.cs
- **Sử dụng Payment ID làm orderCode**: `long orderCode = payment.PaymentId;`
- **Cập nhật logging**: Thông báo sử dụng Production Environment

### Services/PayOSService.cs
- **Chuyển sang Production domain**: `https://api-merchant.payos.vn/`
- **Cập nhật BaseAddress**: Sử dụng production environment

### reset_payment_id.sql
- **Script reset Payment ID**: Xóa dữ liệu cũ và reset sequence về 1

## Các bước thực hiện

### Bước 1: Reset Database
1. Chạy script `reset_payment_id.sql` trong SQL Server Management Studio
2. Xác nhận Payment ID đã được reset về 1

### Bước 2: Test Thanh toán
1. Tạo một lịch hẹn mới với status "Completed"
2. Thử thanh toán từ trang "Lịch sử đặt lịch"
3. Kiểm tra xem PayOS có hiển thị trang QR không

### Bước 3: Kiểm tra Logs
1. Theo dõi console logs để xem:
   - Order Code được tạo
   - Response từ PayOS
   - URL redirect

## Kết quả mong đợi
- Payment ID sẽ bắt đầu từ 1
- PayOS sẽ hiển thị trang QR thanh toán
- OrderCode sẽ match với Payment ID
- Không còn chuyển sang Mock Gateway

## Lưu ý quan trọng
- **Backup dữ liệu**: Trước khi chạy script reset, hãy backup dữ liệu PaymentHistory
- **Production credentials**: Đảm bảo PayOS production credentials được cấu hình đúng
- **Test kỹ**: Test thanh toán nhiều lần để đảm bảo hoạt động ổn định 