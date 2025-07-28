# Thay đổi về nút thanh toán

## Tóm tắt thay đổi
Đã di chuyển nút "Thanh toán" từ trang "Tất cả lịch hẹn" sang trang "Lịch sử đặt lịch" và chỉ hiển thị khi technician cập nhật status thành "Completed".

## Các file đã thay đổi

### 1. Views/Appointment/AllAppointments.cshtml
- **Xóa nút thanh toán** khỏi trang "Tất cả lịch hẹn"
- **Thêm thông báo hướng dẫn** cho người dùng biết nút thanh toán sẽ xuất hiện trong trang "Lịch sử đặt lịch"
- **Cập nhật tên nút** từ "Lịch hẹn đã xác nhận" thành "Lịch sử đặt lịch"

### 2. Views/Appointment/History.cshtml
- **Thêm nút thanh toán** chỉ hiển thị khi status là "Completed" (khi technician đã hoàn thành công việc)
- **Cập nhật tiêu đề trang** để phản ánh đúng chức năng
- **Thêm thông báo hướng dẫn** về điều kiện hiển thị nút thanh toán
- **Cải thiện hiển thị trạng thái** với các status khác nhau
- **Thêm CSS** cho nút thanh toán

### 3. Views/Appointment/Details.cshtml
- **Thêm thông báo hướng dẫn** cho người dùng biết có thể thanh toán từ cả trang Details và trang History
- **Giữ nguyên nút thanh toán** trong trang Details để đảm bảo tính linh hoạt

### 4. Controllers/AppointmentController.cs
- **Cập nhật comment** trong method History để phản ánh đúng logic hiển thị

## Logic mới

### Trang "Tất cả lịch hẹn"
- Hiển thị tất cả lịch hẹn (bao gồm Pending, Confirmed, In Progress, Completed)
- **KHÔNG có nút thanh toán**
- Chỉ hiển thị trạng thái "Đã thanh toán" cho những lịch hẹn Pending đã thanh toán

### Trang "Lịch sử đặt lịch"
- Hiển thị lịch hẹn đã xác nhận, đang thực hiện, hoàn thành và những lịch hẹn Pending đã thanh toán
- **CÓ nút thanh toán** chỉ khi status là "Completed" và chưa thanh toán
- Hiển thị nút "Đánh giá" sau khi thanh toán

### Trang "Chi tiết lịch hẹn"
- **CÓ nút thanh toán** khi status là "Completed" và chưa thanh toán
- Thêm thông báo hướng dẫn về các cách thanh toán

## Lợi ích của thay đổi
1. **Tập trung hóa**: Nút thanh toán chính được đặt ở trang "Lịch sử đặt lịch"
2. **Logic rõ ràng**: Chỉ thanh toán khi technician đã hoàn thành công việc
3. **Trải nghiệm người dùng tốt hơn**: Hướng dẫn rõ ràng về khi nào và ở đâu có thể thanh toán
4. **Tính linh hoạt**: Vẫn có thể thanh toán từ trang Details nếu cần

## Trạng thái thanh toán
- **Pending**: Chờ xác nhận (có thể đã thanh toán hoặc chưa)
- **Confirmed**: Đã xác nhận
- **In Progress/In_Progress**: Đang thực hiện
- **Completed**: Hoàn thành (có thể thanh toán)
- **Cancelled**: Đã hủy 