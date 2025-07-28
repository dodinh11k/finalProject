# Chức năng In Hóa đơn và Download PDF

## Tóm tắt
Đã thêm chức năng tạo và download hóa đơn PDF cho hệ thống MY GARAGE, bao gồm:
- Hóa đơn dịch vụ (Appointment Invoice)
- Biên lai thanh toán (Payment Receipt)

## Các thành phần đã tạo

### 1. Thư viện PDF
- **QuestPDF**: Thư viện hiện đại để tạo PDF với fluent API
- **Ưu điểm**: Không có xung đột namespace, dễ sử dụng, hiệu suất cao

### 2. Services
#### InvoiceService.cs
- **GenerateInvoicePdf()**: Tạo hóa đơn dịch vụ
- **GeneratePaymentReceiptPdf()**: Tạo biên lai thanh toán
- **Các method hỗ trợ**: AddHeader, AddCustomerInfo, AddServiceDetails, AddTotal, AddFooter

### 3. Controllers
#### InvoiceController.cs
- **DownloadAppointmentInvoice()**: Download hóa đơn dịch vụ
- **DownloadPaymentReceipt()**: Download biên lai thanh toán
- **GetCurrentUserId()**: Lấy ID người dùng hiện tại

### 4. Views (Cập nhật)
- **Details.cshtml**: Thêm nút "Tải hóa đơn PDF"
- **Success.cshtml**: Thêm nút "Tải biên lai PDF"
- **History.cshtml**: Thêm nút "Hóa đơn" cho lịch hẹn đã hoàn thành

## Tính năng của PDF

### Hóa đơn Dịch vụ
- **Header**: Logo, tên công ty, thông tin liên hệ
- **Thông tin khách hàng**: Họ tên, email, điện thoại, địa chỉ garage
- **Chi tiết dịch vụ**: Bảng dịch vụ với xe, số lượng, đơn giá
- **Sản phẩm thay thế**: Bảng sản phẩm (nếu có)
- **Tổng tiền**: Chi tiết giảm giá và tổng cộng
- **Footer**: Thông tin liên hệ

### Biên lai Thanh toán
- **Header**: Logo, tên công ty, thông tin liên hệ
- **Thông tin thanh toán**: Mã thanh toán, lịch hẹn, khách hàng, số tiền
- **Chi tiết giao dịch**: Phương thức, trạng thái, ngày thanh toán
- **Footer**: Thông tin liên hệ

## Cách sử dụng

### 1. Tải hóa đơn dịch vụ
- Vào trang "Chi tiết lịch hẹn"
- Click nút "Tải hóa đơn PDF"
- Hoặc vào trang "Lịch sử đặt lịch" → Click "Hóa đơn"

### 2. Tải biên lai thanh toán
- Sau khi thanh toán thành công
- Click nút "Tải biên lai PDF"

## URL Endpoints

### Hóa đơn dịch vụ
```
GET /invoice/appointment/{appointmentId}
```

### Biên lai thanh toán
```
GET /invoice/payment/{paymentId}
```

## Cấu hình

### Program.cs
```csharp
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
```

### Dependencies
```xml
<PackageReference Include="QuestPDF" Version="2023.12.6" />
```

## Tính năng bảo mật
- **Authentication**: Yêu cầu đăng nhập
- **Authorization**: Chỉ cho phép tải PDF của chính mình
- **Validation**: Kiểm tra quyền truy cập dữ liệu

## Định dạng file
- **Tên file**: `HoaDon_LichHen_{ID}_{Date}.pdf` hoặc `BienLai_ThanhToan_{ID}_{Date}.pdf`
- **Content-Type**: `application/pdf`
- **Encoding**: UTF-8

## Lợi ích
1. **Chuyên nghiệp**: Hóa đơn PDF đẹp, đầy đủ thông tin
2. **Tiện lợi**: Có thể tải về và in ra
3. **Lưu trữ**: Dễ dàng lưu trữ và chia sẻ
4. **Pháp lý**: Đáp ứng yêu cầu pháp lý về hóa đơn
5. **Tùy chỉnh**: Dễ dàng thay đổi mẫu hóa đơn

## Tương lai
- Thêm logo công ty vào PDF
- Tùy chỉnh template hóa đơn
- Thêm chữ ký số
- Tích hợp email tự động gửi PDF
- Thêm QR code cho thanh toán 