using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using eBookLibraryService.Services;
using eBookLibraryService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace eBookLibraryService.Controllers
{
    public class PaymentController : Controller
    {
        private readonly NotificationService _emailService; // Assuming you have an email service

        public PaymentController(NotificationService emailService)
        {
            _emailService = emailService;
        }

        [RequireHttps]
        public IActionResult ProcessPayment(float amount)
        {
            if (amount <= 0)
            {
                TempData["PaymentMessage"] = "Invalid payment amount.";
                return RedirectToAction("Index", "Cart");
            }

            var paymentViewModel = new PaymentViewModel
            {
                TotalAmount = amount
            };

            return View(paymentViewModel);
        }

        [HttpPost]
        public IActionResult CompletePayment(string paymentMethod, float amount)
        {
            if (string.IsNullOrEmpty(paymentMethod))
            {
                TempData["PaymentMessage"] = "Please select a payment method.";
                return RedirectToAction("ProcessPayment", new { amount });
            }

            if (paymentMethod == "CreditCard")
            {
                return RedirectToAction("CreditCardPayment", new { amount });
            }
            else if (paymentMethod == "PayPal")
            {
                return RedirectToAction("PayPalPayment", new { amount });
            }

            TempData["PaymentMessage"] = "Invalid payment method selected.";
            return RedirectToAction("ProcessPayment", new { amount });
        }

        [HttpGet]
        public IActionResult CreditCardPayment(float amount)
        {
            if (amount <= 0)
            {
                TempData["PaymentMessage"] = "Invalid payment amount.";
                return RedirectToAction("ProcessPayment");
            }

            var creditCardPaymentViewModel = new CreditCardPaymentViewModel
            {
                TotalAmount = amount
            };

            return View(creditCardPaymentViewModel);
        }



        [HttpPost]
        public async Task<IActionResult> SubmitCreditCardPayment(CreditCardPaymentViewModel model, float amount)
        {
            if (!ModelState.IsValid)
            {
                TempData["PaymentMessage"] = "Invalid credit card details. Please try again.";
                return RedirectToAction("CreditCardPayment", new { amount });
            }

            bool paymentSuccess = true; 

            if (paymentSuccess)
            {
                var userEmail = User.Identity.Name;
                var emailContent = $"Your payment of ${amount} has been successfully processed. Thank you for shopping with us!";
                await _emailService.SendEmailAsync(userEmail, "Payment Confirmation", emailContent);

                var cart = HttpContext.Session.GetObject<Cart>("Cart");
                cart?.Items.Clear();
                HttpContext.Session.SetObject("Cart", cart);

                TempData["PaymentMessage"] = "Payment successful! A confirmation email has been sent.";
                return RedirectToAction("Index", "Home");
            }

            TempData["PaymentMessage"] = "Payment failed. Please try again.";
            return RedirectToAction("CreditCardPayment", new { amount });
        }

        [HttpGet]
        public IActionResult PayPalPayment(float amount)
        {
            string formattedAmount = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            string paypalLink = $"https://paypal.me/ebookstore22/{formattedAmount}";

            return Redirect(paypalLink);
        }
    }
}
