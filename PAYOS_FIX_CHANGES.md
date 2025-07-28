# Sửa lỗi PayOS không hiển thị trang QR

## Vấn đề
- Payment ID trong hệ thống đã lên đến 48
- PayOS không hiển thị trang QR thanh toán
- Hệ thống chuyển sang Mock Payment Gateway

## Nguyên nhân
1. **OrderCode xung đột**: Sử dụng `payment.PaymentId` (48) làm orderCode cho PayOS
2. **Domain không đúng**: Sử dụng production domain thay vì sandbox
3. **Thiếu logging**: Không có đủ thông tin debug

## Các thay đổi đã thực hiện

### 1. Controllers/PaymentController.cs
- **Thay đổi logic tạo OrderCode**:
  ```csharp
  // Trước:
  long orderCode = payment.PaymentId;
  
  // Sau:
  long orderCode = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss")) + appointmentId.Value;
  ```
- **Cải thiện logging**: Thêm JsonSerializer để log chi tiết paymentRequest
- **Thêm thông báo sandbox**: Log để xác nhận đang sử dụng sandbox environment

### 2. Services/PayOSService.cs
- **Sửa domain**: Chuyển từ production sang sandbox
  ```csharp
  // Trước:
  var domain = "https://api-merchant.payos.vn/";
  
  // Sau:
  var domain = "https://sandbox.payos.vn/";
  ```

### 3. Cập nhật Mock Payment Gateway
- **Cải thiện PayOSOrderCode**: Thêm appointmentId vào để tránh trùng lặp
  ```csharp
  // Trước:
  PayOSOrderCode = $"MOCK_ORDER_{DateTime.Now:yyyyMMddHHmmss}";
  
  // Sau:
  PayOSOrderCode = $"MOCK_ORDER_{DateTime.Now:yyyyMMddHHmmss}_{appointmentId}";
  ```

## Lợi ích của thay đổi

### 1. **OrderCode độc lập**
- Không phụ thuộc vào Payment ID của database
- Sử dụng timestamp + appointmentId để tạo orderCode duy nhất
- Tránh xung đột với PayOS

### 2. **Sandbox Environment**
- Đảm bảo sử dụng môi trường test của PayOS
- Tránh xung đột với production data
- An toàn cho việc testing

### 3. **Logging tốt hơn**
- Có thể debug chi tiết payment request
- Theo dõi được response từ PayOS
- Dễ dàng xác định lỗi

## Kết quả mong đợi
- PayOS sẽ hiển thị trang QR thanh toán thay vì chuyển sang Mock Gateway
- OrderCode sẽ không bị xung đột với Payment ID
- Có thể theo dõi và debug quá trình thanh toán dễ dàng hơn

## Lưu ý
- Cần đảm bảo PayOS sandbox credentials được cấu hình đúng
- OrderCode mới sẽ có format: `yyyyMMddHHmmss + appointmentId`
- Vẫn giữ nguyên logic fallback sang Mock Gateway nếu có lỗi 