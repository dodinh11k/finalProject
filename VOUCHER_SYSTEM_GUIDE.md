# Voucher System Integration Guide

## Overview
The voucher system has been successfully integrated into the existing payment system. Users can now apply discount codes when creating appointments, and the discounted amount will be used for payment processing.

## Features Added

### 1. Voucher Management (Admin)
- **Location**: `/Admin/Event` - Manage all vouchers
- **Add Voucher**: `/Admin/AddVoucher` - Create new vouchers
- **Edit Voucher**: `/Admin/EditVoucher/{id}` - Modify existing vouchers
- **Delete Voucher**: Delete vouchers from the management page

### 2. Voucher Types Supported
- **Percentage Discount**: Reduces total by a percentage (e.g., 10%, 20%)
- **Fixed Amount Discount**: Reduces total by a fixed amount (e.g., 50,000 VNĐ)

### 3. Voucher Validation
- **Real-time validation** when users enter voucher codes
- **Expiry date checking** - expired vouchers are rejected
- **Case-insensitive** voucher code matching
- **Automatic discount calculation** based on service prices

### 4. Payment Integration
- **Discounted amounts** are automatically calculated and used for payment
- **Payment descriptions** include discount information
- **All existing payment methods** (PayOS, Mock Gateway) work with discounts

## How It Works

### 1. Creating Appointments with Vouchers
1. User fills out appointment form (single or multi-vehicle)
2. User enters voucher code in the "Mã khuyến mãi" field
3. System validates voucher in real-time (AJAX call)
4. If valid, discount is applied and saved to appointment
5. Payment amount reflects the discounted total

### 2. Voucher Validation Process
```csharp
// VoucherService validates:
- Voucher exists in database
- Voucher is not expired
- Calculates discount amount
- Returns validation result with message
```

### 3. Payment Flow
```csharp
// Payment amount calculation:
Total Service Price - Discount Amount = Final Payment Amount
```

## Database Changes

### New Fields in Appointment Table
- `PromoCode` (string) - Stores the applied voucher code
- `DiscountAmount` (decimal) - Stores the calculated discount amount
- `TotalAmount` (decimal) - Stores the final amount after discount

### PromoCode Table Structure
- `PromoCodeId` (int) - Primary key
- `Code` (string) - Voucher code (max 50 chars)
- `DiscountAmount` (decimal?) - Fixed discount amount
- `DiscountPercent` (double?) - Percentage discount
- `ExpiryDate` (DateTime?) - Expiration date
- `Description` (string) - Voucher description

## API Endpoints

### Voucher Validation
```
POST /Appointment/ValidateVoucher
Parameters: promoCode (string), totalAmount (decimal)
Returns: JSON with validation result and discount details
```

### Test Endpoints
```
GET /test/add-vouchers - Add sample vouchers for testing
GET /test/list-vouchers - List all available vouchers
```

## Sample Vouchers for Testing

After running `/test/add-vouchers`, these vouchers will be available:

1. **SAVE10** - 10% discount (expires in 3 months)
2. **SAVE50K** - 50,000 VNĐ discount (expires in 2 months)
3. **WELCOME20** - 20% discount (expires in 1 month)
4. **FIXED30K** - 30,000 VNĐ discount (expires in 30 days)

## User Experience

### 1. Appointment Creation
- Users see voucher input field on both single and multi-vehicle forms
- Real-time validation shows success/error messages
- Discount amount is displayed immediately

### 2. Appointment Details
- Shows original service price
- Shows applied discount
- Shows final amount to pay
- Payment button uses discounted amount

### 3. Payment History
- AllAppointments view shows discounted prices
- Payment descriptions include discount information
- Payment amounts reflect final discounted totals

## Technical Implementation

### Services Added
- `IVoucherService` - Interface for voucher operations
- `VoucherService` - Implementation of voucher validation and calculation

### Controllers Modified
- `AppointmentController` - Added voucher validation and discount calculation
- `AdminController` - Already had voucher management (from your code)

### Views Updated
- `Create.cshtml` - Added voucher input with real-time validation
- `CreateMulti.cshtml` - Added voucher input with real-time validation
- `Details.cshtml` - Shows discount breakdown and payment button
- `AllAppointments.cshtml` - Shows discounted prices

## Testing the System

1. **Add test vouchers**: Visit `/test/add-vouchers`
2. **Create appointment**: Go to appointment creation page
3. **Enter voucher code**: Try "SAVE10" or "SAVE50K"
4. **Verify discount**: Check that discount is applied
5. **Complete payment**: Verify payment uses discounted amount

## Security Considerations

- Voucher codes are case-insensitive but exact match
- Expired vouchers are automatically rejected
- Discount amounts cannot exceed original service price
- All voucher operations are logged in the database

## Future Enhancements

1. **Usage Limits**: Limit how many times a voucher can be used
2. **User-specific Vouchers**: Vouchers for specific users only
3. **Minimum Order Value**: Require minimum order value for voucher use
4. **Voucher Categories**: Different voucher types for different services
5. **Analytics**: Track voucher usage and effectiveness

## Troubleshooting

### Common Issues
1. **Voucher not found**: Check if voucher exists in database
2. **Voucher expired**: Check expiry date in admin panel
3. **Discount not applied**: Verify voucher validation is working
4. **Payment amount incorrect**: Check discount calculation logic

### Debug Endpoints
- `/test/list-vouchers` - View all vouchers in database
- Check browser console for AJAX validation errors
- Check server logs for validation errors 