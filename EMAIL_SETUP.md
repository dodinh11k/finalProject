# Hướng dẫn cấu hình Email cho MyGarage

## Tổng quan
Hệ thống MyGarage đã được tích hợp chức năng gửi email tự động khi user đặt lịch hẹn thành công.

## Cấu hình Email

### 1. Cấu hình Gmail (Khuyến nghị)

#### Bước 1: Tạo App Password
1. Đăng nhập vào tài khoản Google
2. Vào Settings > Security
3. Bật 2-Step Verification nếu chưa bật
4. Tạo App Password:
   - Vào Security > App passwords
   - Chọn "Mail" và "Other (Custom name)"
   - Đặt tên: "MyGarage"
   - Copy password được tạo

#### Bước 2: Cập nhật appsettings.json
```json
{
  "Email": {
    "From": "your-email@gmail.com",
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "Username": "taithptqt@gmail.com",
    "Password": "sbnk acqi wgih mcuo"
  }
}
```

### 2. Cấu hình Email khác

#### Outlook/Hotmail
```json
{
  "Email": {
    "From": "your-email@outlook.com",
    "SmtpServer": "smtp-mail.outlook.com",
    "Port": 587,
    "Username": "your-email@outlook.com",
    "Password": "your-password"
  }
}
```

#### Yahoo
```json
{
  "Email": {
    "From": "your-email@yahoo.com",
    "SmtpServer": "smtp.mail.yahoo.com",
    "Port": 587,
    "Username": "your-email@yahoo.com",
    "Password": "your-app-password"
  }
}
```

## Tính năng Email

### Email xác nhận đặt lịch
- **Khi nào gửi**: Khi user đặt lịch hẹn thành công
- **Nội dung**: 
  - Thông tin lịch hẹn (mã, thời gian, dịch vụ)
  - Thông tin garage và kỹ thuật viên
  - Hướng dẫn và lưu ý quan trọng

### Template Email
Email sử dụng HTML template với:
- Header màu xanh với icon
- Thông tin chi tiết lịch hẹn
- Hướng dẫn và lưu ý
- Footer với thông tin công ty

## Troubleshooting

### Lỗi thường gặp

1. **"Authentication failed"**
   - Kiểm tra username/password
   - Đảm bảo đã tạo App Password cho Gmail
   - Kiểm tra 2FA đã được bật

2. **"Connection timeout"**
   - Kiểm tra kết nối internet
   - Kiểm tra firewall
   - Thử port khác (465 cho SSL)

3. **"Email not sent"**
   - Kiểm tra log trong console
   - Đảm bảo user có email hợp lệ
   - Kiểm tra cấu hình SMTP

### Debug
- Xem log trong console khi có lỗi email
- Kiểm tra cấu hình trong appsettings.json
- Test kết nối SMTP

## Bảo mật

### Lưu ý quan trọng
- **KHÔNG** commit password email vào source code
- Sử dụng App Password thay vì password chính
- Cấu hình email trong User Secrets hoặc Environment Variables cho production

### User Secrets (Development)
```bash
dotnet user-secrets set "Email:Password" "your-app-password"
```

### Environment Variables (Production)
```bash
set Email__Password=your-app-password
```

## Cấu trúc Code

### Files chính
- `Services/IEmailService.cs` - Interface email service
- `Services/EmailService.cs` - Implementation email service
- `Controllers/AppointmentController.cs` - Gọi email service
- `appsettings.json` - Cấu hình email

### Dependencies
- MailKit (đã có trong project)
- MimeKit (đã có trong project)

## Testing

### Test Email Service
1. Cấu hình email đúng
2. Đặt lịch hẹn với user có email
3. Kiểm tra email được gửi
4. Kiểm tra nội dung email

### Test Error Handling
1. Cấu hình email sai
2. Đặt lịch hẹn
3. Kiểm tra appointment vẫn được tạo
4. Kiểm tra log lỗi email 