using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Loan_Calculator.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Loan_Calculator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Calculation(CreditViewModel credit)
        {
            int period = 1;
            double periodMainPayment = 0;
            double periodInterestPayment = 0;
            
            double periodAnnuityPayment = 0;
            if (!ModelState.IsValid)
            {
                return Error();
            }
            if (credit.Conditions[0])
            {
                period = credit.Conditions[1] ? 15 : 10;
            }
            double interestRatePart = period == 1 ? (credit.InterestRate / 100 / 12) : (credit.InterestRate / 100);
            double allPercents = Convert.ToDouble(Math.Pow(1 + interestRatePart, Convert.ToDouble(credit.Term)));
            double annuityPayment = Convert.ToDouble(credit.Amount) * (interestRatePart * allPercents) / (allPercents - 1);
            double mainDebt = credit.Amount;
            List<PaymentViewModel> paymentSchedule = new List<PaymentViewModel>();
            for (int j = 1; j <= credit.Term; j++)
            {
                if (j % period == 0 || j == credit.Term)
                {
                    PaymentViewModel payment = new PaymentViewModel
                    {
                        PaymentDate = period == 1 ? credit.CreatedAt.AddMonths(j) : credit.CreatedAt.AddDays(j),
                        PaymentAmount = period == 1? annuityPayment : periodAnnuityPayment + annuityPayment
                    };
                    payment.InterestPayment = period == 1 ? mainDebt * interestRatePart : periodInterestPayment + mainDebt * interestRatePart;
                    payment.MainPayment = period == 1 ? annuityPayment - payment.InterestPayment : periodMainPayment + annuityPayment - mainDebt * interestRatePart;
                    payment.RemainingDebt = Math.Abs(mainDebt - payment.MainPayment);
                    mainDebt -= annuityPayment - mainDebt * interestRatePart;
                    if (period != 1)
                    {
                        periodAnnuityPayment = 0;
                        periodInterestPayment = 0;
                        periodMainPayment = 0;
                    }
                    paymentSchedule.Add(payment);
                }
                else
                {
                    periodAnnuityPayment += annuityPayment;
                    periodInterestPayment += mainDebt * interestRatePart;
                    periodMainPayment += annuityPayment - mainDebt * interestRatePart;
                    mainDebt = Math.Abs(mainDebt - (annuityPayment - mainDebt * interestRatePart));
                }
            }
            ViewBag.Schedule = paymentSchedule;
            ViewBag.AllPayment = paymentSchedule.Sum(x => x.PaymentAmount);
            ViewBag.AllMainPayment = paymentSchedule.Sum(x => x.MainPayment);
            ViewBag.AllInterestPayment = paymentSchedule.Sum(x => x.InterestPayment);
            return View();
        }

        [HttpPost]
        public IActionResult PostData(double amount, float rate, ushort term, bool conditions=false, bool option1=false, bool option2=false)
        {
            return RedirectToAction("Calculation", new CreditViewModel()
            {
                Amount = Convert.ToDouble(amount),
                InterestRate = Convert.ToDouble(rate),
                Term = term,
                Conditions = new bool[3] { conditions, option1, option2 },
                CreatedAt = DateTime.Now
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
