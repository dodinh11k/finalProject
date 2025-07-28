-- Script để reset Payment ID sequence và xóa dữ liệu cũ
-- Chạy script này để bắt đầu lại Payment ID từ 1

-- 1. Xóa tất cả dữ liệu trong bảng PaymentHistory
DELETE FROM PaymentHistory;

-- 2. Reset identity seed về 1
DBCC CHECKIDENT ('PaymentHistory', RESEED, 0);

-- 3. Kiểm tra lại identity seed
DBCC CHECKIDENT ('PaymentHistory');

-- 4. Xác nhận bảng đã được reset
SELECT 'PaymentHistory table has been reset. Next Payment ID will be 1.' AS Status;

-- 5. Kiểm tra cấu trúc bảng
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PaymentHistory' 
AND COLUMN_NAME = 'PaymentID'; 