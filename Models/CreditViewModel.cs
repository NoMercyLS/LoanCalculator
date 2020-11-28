using System;
using System.ComponentModel.DataAnnotations;

namespace Loan_Calculator.Models
{
    public class CreditViewModel
    {
        [Required]
        public double Amount { get; set; }
        [Required]
        public ushort Term { get; set; }
        [Required]
        public double InterestRate { get; set; }
        [Required]
        public bool[] Conditions { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
