using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using eBookLibraryService.Services;
using eBookLibraryService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Threading.Tasks;

namespace eBookLibraryService.Controllers
{
    public class PaymentController : Controller
    {
        private readonly NotificationService _emailService;

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

            return paymentMethod switch
            {
                "CreditCard" => RedirectToAction("CreditCardPayment", new { amount }),
                "PayPal" => RedirectToAction("PayPalPayment", new { amount }),
                _ => HandleInvalidPaymentMethod(amount)
            };
        }

        private IActionResult HandleInvalidPaymentMethod(float amount)
        {
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
        public async Task<IActionResult> SubmitCreditCardPayment(CreditCardPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["PaymentMessage"] = "Invalid credit card details. Please try again.";
                return RedirectToAction("CreditCardPayment", new { model.TotalAmount });
            }

            // Mock payment processing logic (replace with real payment gateway integration)
            bool paymentSuccess = true;

            if (paymentSuccess)
            {
                // Send confirmation email
                string userEmail = User.Identity.Name ?? "user@example.com";
                string formattedAmount = model.TotalAmount.ToString("F2", CultureInfo.InvariantCulture);
                string emailContent = $"Your payment of ${formattedAmount} has been successfully processed. Thank you for shopping with us!";
                await _emailService.SendEmailAsync(userEmail, "Payment Confirmation", emailContent);

                // Clear the cart after successful payment
                var cart = HttpContext.Session.GetObject<Cart>("Cart");
                cart?.Items.Clear();
                HttpContext.Session.SetObject("Cart", cart);

                TempData["PaymentMessage"] = "Payment successful! A confirmation email has been sent.";
                return RedirectToAction("Index", "Home");
            }

            TempData["PaymentMessage"] = "Payment failed. Please try again.";
            return RedirectToAction("CreditCardPayment", new { model.TotalAmount });
        }

        [HttpGet]
        public IActionResult PayPalPayment(float amount)
        {
            if (amount <= 0)
            {
                TempData["PaymentMessage"] = "Invalid payment amount.";
                return RedirectToAction("ProcessPayment");
            }

            string formattedAmount = amount.ToString("F2", CultureInfo.InvariantCulture);
            string paypalLink = $"https://paypal.me/ebookstore22/{formattedAmount}";

            return Redirect(paypalLink);
        }
    }
}
