using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using test_2.Models;
using System.IO;

namespace test_2.Services
{
    public interface IInvoiceService
    {
        byte[] GenerateInvoicePdf(AppointmentDetailsViewModel appointmentDetails);
        byte[] GeneratePaymentReceiptPdf(PaymentHistory payment, Appointment appointment);
        byte[] GenerateTestPdf();
    }

    public class InvoiceService : IInvoiceService
    {
        public byte[] GenerateInvoicePdf(AppointmentDetailsViewModel appointmentDetails)
        {
            try
            {
                // Kiểm tra null
                if (appointmentDetails == null)
                    throw new ArgumentNullException(nameof(appointmentDetails));
                
                if (appointmentDetails.Appointment == null)
                    throw new ArgumentException("Appointment cannot be null");

                // Tạo PDF với PdfSharpCore
                using (var document = new PdfDocument())
                {
                    var page = document.AddPage();
                    var graphics = XGraphics.FromPdfPage(page);
                    var font = new XFont("Arial", 10);
                    var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
                    var headerFont = new XFont("Arial", 14, XFontStyle.Bold);
                    var smallFont = new XFont("Arial", 8);

                    int y = 30;
                    int leftMargin = 30;
                    int rightMargin = 550;

                    // Header - Tiêu đề
                    graphics.DrawString("MY GARAGE - HÓA ĐƠN DỊCH VỤ", titleFont, XBrushes.Black, leftMargin, y);
                    y += 25;
                    graphics.DrawString("Dịch vụ bảo dưỡng ô tô chuyên nghiệp", font, XBrushes.Black, leftMargin, y);
                    y += 15;
                    graphics.DrawString("Địa chỉ: 123 Đường Lớn, Quận 1, TP. HCM", smallFont, XBrushes.Black, leftMargin, y);
                    y += 12;
                    graphics.DrawString("Điện thoại: 1900 1234 | Email: support@mygarage.com", smallFont, XBrushes.Black, leftMargin, y);
                    y += 20;

                    // Vẽ đường kẻ
                    graphics.DrawLine(new XPen(XColors.Gray, 1), leftMargin, y, rightMargin, y);
                    y += 20;

                    // Thông tin khách hàng
                    graphics.DrawString("THÔNG TIN KHÁCH HÀNG", headerFont, XBrushes.Black, leftMargin, y);
                    y += 20;
                    graphics.DrawString($"Họ tên: {appointmentDetails.Appointment?.User?.FullName ?? "N/A"}", font, XBrushes.Black, leftMargin, y);
                    y += 15;
                    graphics.DrawString($"Email: {appointmentDetails.Appointment?.User?.Email ?? "N/A"}", font, XBrushes.Black, leftMargin, y);
                    y += 15;
                    graphics.DrawString($"Điện thoại: {appointmentDetails.Appointment?.User?.Phone ?? "N/A"}", font, XBrushes.Black, leftMargin, y);
                    y += 15;
                    graphics.DrawString($"Ngày hẹn: {appointmentDetails.Appointment?.AppointmentTime:dd/MM/yyyy HH:mm}", font, XBrushes.Black, leftMargin, y);
                    y += 15;
                    graphics.DrawString($"Mã lịch hẹn: #{appointmentDetails.Appointment?.AppointmentId ?? 0}", font, XBrushes.Black, leftMargin, y);
                    y += 25;

                    // Chi tiết dịch vụ
                    if (appointmentDetails.VehicleDetails?.Count > 0)
                    {
                        graphics.DrawString("CHI TIẾT DỊCH VỤ", headerFont, XBrushes.Black, leftMargin, y);
                        y += 15;

                        // Vẽ bảng header
                        DrawTableHeader(graphics, leftMargin, y, rightMargin);
                        y += 20;

                        // Vẽ dữ liệu dịch vụ
                        foreach (var detail in appointmentDetails.VehicleDetails)
                        {
                            if (y > 700) // Nếu hết trang thì tạo trang mới
                            {
                                page = document.AddPage();
                                graphics = XGraphics.FromPdfPage(page);
                                y = 30;
                            }

                            DrawServiceRow(graphics, detail, leftMargin, y, rightMargin);
                            y += 20;
                        }
                        
                        // Vẽ đường kẻ dưới bảng dịch vụ
                        graphics.DrawLine(new XPen(XColors.Gray, 1), leftMargin, y, rightMargin, y);
                        y += 20;
                    }

                    // Sản phẩm thay thế
                    if (appointmentDetails.ReplacementProducts?.Count > 0)
                    {
                        graphics.DrawString("SẢN PHẨM THAY THẾ", headerFont, XBrushes.Black, leftMargin, y);
                        y += 15;

                        // Vẽ bảng header sản phẩm
                        DrawProductTableHeader(graphics, leftMargin, y, rightMargin);
                        y += 20;

                        foreach (var product in appointmentDetails.ReplacementProducts)
                        {
                            if (y > 700)
                            {
                                page = document.AddPage();
                                graphics = XGraphics.FromPdfPage(page);
                                y = 30;
                            }

                            DrawProductRow(graphics, product, leftMargin, y, rightMargin);
                            y += 20;
                        }
                        
                        // Vẽ đường kẻ dưới bảng sản phẩm
                        graphics.DrawLine(new XPen(XColors.Gray, 1), leftMargin, y, rightMargin, y);
                        y += 20;
                    }

                    // Tổng tiền
                    graphics.DrawString("TỔNG TIỀN", headerFont, XBrushes.Black, leftMargin, y);
                    y += 20;

                    decimal totalServicePrice = appointmentDetails.TotalServicePrice;
                    graphics.DrawString($"Tổng tiền dịch vụ: {totalServicePrice:N0} VNĐ", font, XBrushes.Black, leftMargin, y);
                    y += 15;

                    if (appointmentDetails.ReplacementProducts?.Count > 0)
                    {
                        decimal totalProductPrice = appointmentDetails.ReplacementProducts.Sum(p => p.UnitPrice * p.Quantity);
                        graphics.DrawString($"Tổng tiền sản phẩm: {totalProductPrice:N0} VNĐ", font, XBrushes.Black, leftMargin, y);
                        y += 15;
                    }

                    if (appointmentDetails.DiscountAmount > 0)
                    {
                        graphics.DrawString($"Giảm giá: -{appointmentDetails.DiscountAmount:N0} VNĐ", font, XBrushes.Red, leftMargin, y);
                        y += 15;
                    }

                    // Vẽ đường kẻ cho tổng cộng
                    graphics.DrawLine(new XPen(XColors.Black, 2), leftMargin, y, rightMargin, y);
                    y += 15;

                    graphics.DrawString($"TỔNG CỘNG: {appointmentDetails.TotalAmount:N0} VNĐ", headerFont, XBrushes.Black, leftMargin, y);
                    y += 30;

                    // Footer
                    graphics.DrawLine(new XPen(XColors.Gray, 1), leftMargin, y, rightMargin, y);
                    y += 15;
                    graphics.DrawString("Cảm ơn quý khách đã sử dụng dịch vụ của MY GARAGE!", font, XBrushes.Black, leftMargin, y);
                    y += 15;
                    graphics.DrawString("Mọi thắc mắc vui lòng liên hệ: 1900 1234 hoặc support@mygarage.com", smallFont, XBrushes.Black, leftMargin, y);

                    // Lưu PDF vào memory stream
                    using (var stream = new MemoryStream())
                    {
                        document.Save(stream);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating invoice PDF: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return GenerateSimplePdf("HÓA ĐƠN DỊCH VỤ", appointmentDetails?.Appointment?.AppointmentId.ToString() ?? "N/A");
            }
        }

        public byte[] GeneratePaymentReceiptPdf(PaymentHistory payment, Appointment appointment)
        {
            try
            {
                // Kiểm tra null
                if (payment == null)
                    throw new ArgumentNullException(nameof(payment));
                
                if (appointment == null)
                    throw new ArgumentNullException(nameof(appointment));

                // Tạo PDF với PdfSharpCore
                using (var document = new PdfDocument())
                {
                    var page = document.AddPage();
                    var graphics = XGraphics.FromPdfPage(page);
                    var font = new XFont("Arial", 12);
                    var titleFont = new XFont("Arial", 16, XFontStyle.Bold);

                    int y = 50;

                    // Tiêu đề
                    graphics.DrawString("MY GARAGE - BIÊN LAI THANH TOÁN", titleFont, XBrushes.Black, 50, y);
                    y += 30;

                    // Thông tin thanh toán
                    graphics.DrawString($"Mã thanh toán: {payment?.PaymentId ?? 0}", font, XBrushes.Black, 50, y);
                    y += 20;

                    graphics.DrawString($"Mã lịch hẹn: {appointment?.AppointmentId ?? 0}", font, XBrushes.Black, 50, y);
                    y += 20;

                    graphics.DrawString($"Khách hàng: {appointment?.User?.FullName ?? "N/A"}", font, XBrushes.Black, 50, y);
                    y += 20;

                    graphics.DrawString($"Số tiền: {payment?.Amount:N0} VNĐ", font, XBrushes.Black, 50, y);
                    y += 20;

                    graphics.DrawString($"Ngày thanh toán: {payment?.CreatedAt:dd/MM/yyyy HH:mm}", font, XBrushes.Black, 50, y);

                    // Lưu PDF vào memory stream
                    using (var stream = new MemoryStream())
                    {
                        document.Save(stream);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating payment receipt PDF: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return GenerateSimplePdf("BIÊN LAI THANH TOÁN", payment?.PaymentId.ToString() ?? "N/A");
            }
        }

        public byte[] GenerateTestPdf()
        {
            try
            {
                using (var document = new PdfDocument())
                {
                    var page = document.AddPage();
                    var graphics = XGraphics.FromPdfPage(page);
                    var font = new XFont("Arial", 12);

                    graphics.DrawString("Hello World!", font, XBrushes.Black, 50, 50);

                    using (var stream = new MemoryStream())
                    {
                        document.Save(stream);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating test PDF: {ex.Message}");
                return new byte[0];
            }
        }

        private byte[] GenerateSimplePdf(string title, string id)
        {
            try
            {
                using (var document = new PdfDocument())
                {
                    var page = document.AddPage();
                    var graphics = XGraphics.FromPdfPage(page);
                    var font = new XFont("Arial", 12);
                    var titleFont = new XFont("Arial", 16, XFontStyle.Bold);

                    int y = 50;

                    graphics.DrawString("MY GARAGE", titleFont, XBrushes.Black, 50, y);
                    y += 30;

                    graphics.DrawString(title, font, XBrushes.Black, 50, y);
                    y += 20;

                    graphics.DrawString($"Mã: {id}", font, XBrushes.Black, 50, y);
                    y += 20;

                    graphics.DrawString($"Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}", font, XBrushes.Black, 50, y);

                    using (var stream = new MemoryStream())
                    {
                        document.Save(stream);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating simple PDF: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new byte[0];
            }
        }

        // Helper methods để vẽ bảng
        private void DrawTableHeader(XGraphics graphics, int leftMargin, int y, int rightMargin)
        {
            var headerFont = new XFont("Arial", 10, XFontStyle.Bold);

            // Vẽ header
            graphics.DrawString("Dịch vụ", headerFont, XBrushes.Black, leftMargin, y);
            graphics.DrawString("Xe", headerFont, XBrushes.Black, leftMargin + 200, y);
            graphics.DrawString("SL", headerFont, XBrushes.Black, leftMargin + 350, y);
            graphics.DrawString("Đơn giá (VNĐ)", headerFont, XBrushes.Black, leftMargin + 400, y);

            // Không vẽ đường kẻ dưới header
        }

        private void DrawServiceRow(XGraphics graphics, AppointmentVehicleDetail detail, int leftMargin, int y, int rightMargin)
        {
            var font = new XFont("Arial", 9);

            graphics.DrawString(detail.Service?.ServiceName ?? "N/A", font, XBrushes.Black, leftMargin, y);
            graphics.DrawString($"{detail.Vehicle?.Make} {detail.Vehicle?.Model} ({detail.Vehicle?.LicensePlate})", font, XBrushes.Black, leftMargin + 200, y);
            graphics.DrawString((detail.Quantity ?? 0).ToString(), font, XBrushes.Black, leftMargin + 350, y);
            graphics.DrawString((detail.Service?.Price ?? 0).ToString("N0"), font, XBrushes.Black, leftMargin + 400, y);
        }

        private void DrawProductTableHeader(XGraphics graphics, int leftMargin, int y, int rightMargin)
        {
            var headerFont = new XFont("Arial", 10, XFontStyle.Bold);

            // Vẽ header
            graphics.DrawString("Sản phẩm", headerFont, XBrushes.Black, leftMargin, y);
            graphics.DrawString("SL", headerFont, XBrushes.Black, leftMargin + 300, y);
            graphics.DrawString("Đơn giá (VNĐ)", headerFont, XBrushes.Black, leftMargin + 350, y);
            graphics.DrawString("Thành tiền (VNĐ)", headerFont, XBrushes.Black, leftMargin + 450, y);

            // Không vẽ đường kẻ dưới header
        }

        private void DrawProductRow(XGraphics graphics, AppointmentProductDetail product, int leftMargin, int y, int rightMargin)
        {
            var font = new XFont("Arial", 9);

            graphics.DrawString(product.Product?.ProductName ?? "N/A", font, XBrushes.Black, leftMargin, y);
            graphics.DrawString(product.Quantity.ToString(), font, XBrushes.Black, leftMargin + 300, y);
            graphics.DrawString(product.UnitPrice.ToString("N0"), font, XBrushes.Black, leftMargin + 350, y);
            graphics.DrawString((product.UnitPrice * product.Quantity).ToString("N0"), font, XBrushes.Black, leftMargin + 450, y);
        }
    }
} 