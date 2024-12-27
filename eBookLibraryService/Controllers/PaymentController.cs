using eBookLibraryService.Data;
using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using eBookLibraryService.Services;
using eBookLibraryService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading.Tasks;

namespace eBookLibraryService.Controllers
{
    public class PaymentController : Controller
    {
        private readonly NotificationService _emailService;
        private readonly eBookLibraryServiceContext _context;

        public PaymentController(NotificationService emailService, eBookLibraryServiceContext context)
        {
            _emailService = emailService;
            _context = context;
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
                string userEmail = User.Identity.Name ?? "user@example.com";
                string formattedAmount = model.TotalAmount.ToString("F2", CultureInfo.InvariantCulture);
                string emailContent = $"Your payment of ${formattedAmount} has been successfully processed. Thank you for shopping with us!";
                await _emailService.SendEmailAsync(userEmail, "Payment Confirmation", emailContent);

                // Clear the cart stored in the database
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserEmail == userEmail);

                if (cart != null)
                {
                    cart.Items.Clear();
                    _context.Carts.Update(cart);
                    await _context.SaveChangesAsync();
                }

                TempData["PaymentMessage"] = "Payment successful! A confirmation email has been sent.";
                return RedirectToAction("Index", "Home");
            }

            TempData["PaymentMessage"] = "Payment failed. Please try again.";
            return RedirectToAction("CreditCardPayment", new { model.TotalAmount });
        }

        [HttpGet]
        public IActionResult PayPalPayment(float amount)
        {
            // Assume conversion rate to USD is 3.66
            float amountInUSD = amount / 3.66f;
            if (amountInUSD <= 0)
            {
                TempData["PaymentMessage"] = "Invalid payment amount.";
                return RedirectToAction("ProcessPayment");
            }

            string formattedAmount = amountInUSD.ToString("F2", CultureInfo.InvariantCulture);
            string paypalLink = $"https://paypal.me/ebookstore22/{formattedAmount}";

            return Redirect(paypalLink);
        }
    }
}
