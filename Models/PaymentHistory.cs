using System;
using System.ComponentModel.DataAnnotations;

namespace test_2.Models
{
    public class PaymentHistory
    {
        public int PaymentId { get; set; }
        
        public int? AppointmentId { get; set; }
        
        public int? UserId { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        public string? PaymentMethod { get; set; } = "PayOS";
        
        public string? TransactionId { get; set; }
        
        public string? Status { get; set; } = "Pending";
        
        public string? PayOSOrderCode { get; set; }
        
        public string? PayOSTransactionId { get; set; }
        
        public string? Description { get; set; }
        
        public DateTime? CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Appointment? Appointment { get; set; }
        public virtual User? User { get; set; }
    }
} 