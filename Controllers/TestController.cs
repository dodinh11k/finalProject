using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace finalProject_1607.Controllers
{
    [Route("test")]
    public class TestController : Controller
    {
        [HttpGet("pdf")]
        public IActionResult TestPdf()
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
                        byte[] pdfBytes = stream.ToArray();
                        return File(pdfBytes, "application/pdf", "test.pdf");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi tạo PDF: {ex.Message}");
            }
        }

        [HttpGet("simple")]
        public IActionResult Simple()
        {
            return Content("Test endpoint hoạt động!");
        }
    }
}