using System.ComponentModel.DataAnnotations;

namespace test_2.Models
{
    public class RegisterViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public string Phone { get; set; }     // ✅ THÊM DÒNG NÀY
        public string Address { get; set; }
    }
}
