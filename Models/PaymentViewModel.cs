using System;

namespace Loan_Calculator.Models
{
    public class PaymentViewModel
    {
        public DateTime PaymentDate { get; set; }
        public double PaymentAmount { get; set; }
        public double MainPayment { get; set; }
        public double InterestPayment { get; set; }
        public double RemainingDebt { get; set; }
    }
}
