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
        public IActionResult ProcessPayment(float amount, int? bookId = null)
        {
            if (amount <= 0)
            {
                TempData["PaymentMessage"] = "Invalid payment amount.";
                return RedirectToAction("Index", bookId.HasValue ? "Home" : "Cart");
            }

            var paymentViewModel = new PaymentViewModel
            {
                TotalAmount = amount,
                BookId = bookId
            };

            return View(paymentViewModel);
        }

        [HttpPost]
        public IActionResult CompletePayment(string paymentMethod, float amount, int? bookId = null)
        {
            if (string.IsNullOrEmpty(paymentMethod))
            {
                TempData["PaymentMessage"] = "Please select a payment method.";
                return RedirectToAction("ProcessPayment", new { amount, bookId });
            }

            return paymentMethod switch
            {
                "CreditCard" => RedirectToAction("CreditCardPayment", new { amount, bookId }),
                "PayPal" => RedirectToAction("PayPalPayment", new { amount, bookId }),
                _ => HandleInvalidPaymentMethod(amount, bookId)
            };
        }

        private IActionResult HandleInvalidPaymentMethod(float amount, int? bookId)
        {
            TempData["PaymentMessage"] = "Invalid payment method selected.";
            return RedirectToAction("ProcessPayment", new { amount, bookId });
        }

        [HttpGet]
        public IActionResult CreditCardPayment(float amount, int? bookId = null)
        {
            if (amount <= 0)
            {
                TempData["PaymentMessage"] = "Invalid payment amount.";
                return RedirectToAction("ProcessPayment", new { bookId });
            }

            var creditCardPaymentViewModel = new CreditCardPaymentViewModel
            {
                TotalAmount = amount,
                BookId = bookId
            };

            return View(creditCardPaymentViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitCreditCardPayment(CreditCardPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["PaymentMessage"] = "Invalid credit card details. Please try again.";
                return View("CreditCardPayment", model);
            }

            // Mock payment processing logic
            bool paymentSuccess = true;

            if (paymentSuccess)
            {
                string userEmail = User.Identity.Name ?? "user@example.com";
                string formattedAmount = model.TotalAmount.ToString("F2", CultureInfo.InvariantCulture);
                string emailContent = $"Your payment of ${formattedAmount} has been successfully processed.";

                if (model.BookId.HasValue)
                {
                    var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == model.BookId.Value);
                    if (book == null)
                    {
                        TempData["PaymentMessage"] = "Book not found.";
                        return RedirectToAction("Index", "Home");
                    }

                    emailContent += $"\nYou purchased the book: {book.Title}.";
                }
                else
                {
                    // Clear cart for the current user after payment
                    var userCart = await _context.Carts
                        .Include(c => c.Items)
                        .FirstOrDefaultAsync(c => c.UserEmail == userEmail);

                    if (userCart != null)
                    {
                        _context.CartItems.RemoveRange(userCart.Items);
                        await _context.SaveChangesAsync();
                    }
                }

                // Send confirmation email
                await _emailService.SendEmailAsync(userEmail, "Payment Confirmation", emailContent);

                TempData["PaymentMessage"] = "Payment successful! A confirmation email has been sent.";
                return RedirectToAction("Index", "Home");
            }

            TempData["PaymentMessage"] = "Payment failed. Please try again.";
            return View("CreditCardPayment", model);
        }

        [HttpGet]
        public IActionResult PayPalPayment(float amount, int? bookId = null)
        {
            // Assume conversion rate to USD is 3.66
            float amountInUSD = amount / 3.66f;
            if (amountInUSD <= 0)
            {
                TempData["PaymentMessage"] = "Invalid payment amount.";
                return RedirectToAction("ProcessPayment", new { bookId });
            }

            string formattedAmount = amountInUSD.ToString("F2", CultureInfo.InvariantCulture);
            string paypalLink = $"https://paypal.me/ebookstore22/{formattedAmount}";

            return Redirect(paypalLink);
        }
    }
}