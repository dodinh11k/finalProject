namespace test_2.Models
{
    public class PasswordResetEmailModel
    {
        public string UserName { get; set; } = string.Empty;
        public string ResetCode { get; set; } = string.Empty;
    }
} 