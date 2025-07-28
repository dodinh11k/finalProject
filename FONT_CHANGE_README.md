# Thay đổi Font Chữ Website

## Tổng quan
Đã thay đổi font chữ trên toàn bộ website từ font Cairo sang font Roboto để đảm bảo tính ổn định và dễ đọc hơn.

## Font được sử dụng
- **Font chính**: Roboto (từ Google Fonts)
- **Font phụ**: Roboto Mono (cho các phần code)

## Các file đã được cập nhật

### 1. File CSS mới
- `wwwroot/css/font-fix.css` - File CSS duy nhất để thiết lập font Roboto và bảo vệ icon

### 2. Layout files
- `Views/Shared/_Layout.cshtml` - Layout chính
- `Views/Shared/_Homelayout.cshtml` - Layout trang chủ
- `Views/Shared/_AdminLayout.cshtml` - Layout admin

## Cách hoạt động

### Font Stack
```css
font-family: 'Roboto', -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Oxygen', 'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue', sans-serif;
```

### Font cho code
```css
font-family: 'Roboto Mono', 'Courier New', monospace;
```

## Lợi ích của việc thay đổi

1. **Tính ổn định**: Roboto là font được hỗ trợ rộng rãi trên nhiều hệ điều hành
2. **Dễ đọc**: Font Roboto được thiết kế tối ưu cho việc đọc trên màn hình
3. **Tương thích**: Hoạt động tốt trên tất cả các trình duyệt hiện đại
4. **Hiệu suất**: Font được tải từ Google Fonts với CDN nhanh

## Cách khôi phục (nếu cần)

Nếu muốn quay lại font Cairo, chỉ cần:
1. Thay đổi link Google Fonts trong các layout files
2. Cập nhật font-family trong các file CSS
3. Xóa hoặc comment các file `global-font.css` và `font-override.css`

## Lưu ý
- Font Roboto được tải từ Google Fonts, đảm bảo kết nối internet ổn định
- Các font fallback được thiết lập để đảm bảo hiển thị ngay cả khi không tải được font chính
- Font rendering được tối ưu với `-webkit-font-smoothing` và `text-rendering`
- Các icon font (Font Awesome, Material Design Icons, etc.) được bảo vệ khỏi bị ảnh hưởng bởi font override

## Giải quyết vấn đề icon

File `font-fix.css` đã được tối ưu để:
- Áp dụng font Roboto cho text elements
- Bảo vệ tất cả các loại icon font (Font Awesome, Material Design Icons, etc.)
- Đặc biệt bảo vệ các icon sao đánh giá (star rating icons)
- Đảm bảo icon hiển thị đúng với font gốc
- Không ảnh hưởng đến font chữ của website 