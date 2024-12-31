using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eBookLibraryService.Data;
using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using eBookLibraryService.Services;
using eBookLibraryService.ViewModels;
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
            if (!IsValidAmount(amount))
            {
                SetPaymentMessage("Invalid payment amount.");
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
                SetPaymentMessage("Please select a payment method.");
                return RedirectToAction("ProcessPayment", new { amount, bookId });
            }

            return paymentMethod switch
            {
                "CreditCard" => RedirectToAction("CreditCardPayment", new { amount, bookId }),
                "PayPal" => RedirectToAction("PayPalPayment", new { amount, bookId }),
                _ => HandleInvalidPaymentMethod(amount, bookId)
            };
        }

        [HttpGet]
        public IActionResult CreditCardPayment(float amount, int? bookId = null)
        {
            if (!IsValidAmount(amount))
            {
                SetPaymentMessage("Invalid payment amount.");
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
                SetPaymentMessage("Invalid credit card details. Please try again.");
                return View("CreditCardPayment", model);
            }

            // Mock payment processing logic
            bool paymentSuccess = true;

            if (paymentSuccess)
            {
                try
                {
                    string userEmail = User.Identity.Name ?? "user@example.com";

                    if (model.BookId.HasValue)
                    {
                        // Fetch the purchased book
                        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == model.BookId.Value);
                        if (book == null)
                        {
                            SetPaymentMessage("The selected book does not exist.");
                            return RedirectToAction("Index", "Home");
                        }

                        // Add the book to the user's library
                        var ownedBook = new OwnedBook
                        {
                            UserEmail = userEmail,
                            BookId = book.Id,
                            Title = book.Title,
                            Author = book.Author,
                            IsBorrowed = false, // Since this is a purchase
                            Price = model.TotalAmount,
                            PurchaseDate = DateTime.Now,
                            BorrowEndDate = null // Not applicable for purchased books
                        };

                        // Avoid duplicates in My Library
                        var alreadyInLibrary = await _context.OwnedBooks.AnyAsync(ob => ob.UserEmail == userEmail && ob.BookId == book.Id);
                        if (!alreadyInLibrary)
                        {
                            _context.OwnedBooks.Add(ownedBook);
                        }
                    }
                    else
                    {
                        // If no specific book, handle cart purchase
                        var userCart = await _context.Carts.Include(c => c.Items).ThenInclude(ci => ci.Book).FirstOrDefaultAsync(c => c.UserEmail == userEmail);
                        if (userCart != null && userCart.Items.Any())
                        {
                            foreach (var cartItem in userCart.Items)
                            {
                                // Add each book in the cart to the user's library
                                var ownedBook = new OwnedBook
                                {
                                    UserEmail = userEmail,
                                    BookId = cartItem.Book.Id,
                                    Title = cartItem.Book.Title,
                                    Author = cartItem.Book.Author,
                                    IsBorrowed = cartItem.IsBorrow,
                                    Price = cartItem.Price,
                                    PurchaseDate = DateTime.Now,
                                    BorrowEndDate = cartItem.IsBorrow ? DateTime.Now.AddDays(14) : (DateTime?)null // 14 days for borrowed books
                                };

                                // Avoid duplicates in My Library
                                var alreadyInLibrary = await _context.OwnedBooks.AnyAsync(ob => ob.UserEmail == userEmail && ob.BookId == cartItem.Book.Id);
                                if (!alreadyInLibrary)
                                {
                                    _context.OwnedBooks.Add(ownedBook);
                                }
                            }
                        }
                    }

                    await _context.SaveChangesAsync(); // Save changes for adding to My Library

                    // Existing email confirmation and cart clearing logic
                    await ProcessPaymentSuccess(model);

                    SetPaymentMessage("Payment successful! A confirmation email has been sent.");
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Log exception (optional)
                    SetPaymentMessage($"An error occurred while processing your payment: {ex.Message}");
                    return RedirectToAction("Index", "Home");
                }
            }

            SetPaymentMessage("Payment failed. Please try again.");
            return View("CreditCardPayment", model);
        }


        [HttpGet]
        public IActionResult PayPalPayment(float amount, int? bookId = null)
        {
            float amountInUSD = ConvertToUSD(amount);
            if (!IsValidAmount(amountInUSD))
            {
                SetPaymentMessage("Invalid payment amount.");
                return RedirectToAction("ProcessPayment", new { bookId });
            }

            string paypalLink = GeneratePayPalLink(amountInUSD);
            return Redirect(paypalLink);
        }

        private IActionResult HandleInvalidPaymentMethod(float amount, int? bookId)
        {
            SetPaymentMessage("Invalid payment method selected.");
            return RedirectToAction("ProcessPayment", new { amount, bookId });
        }

        // Helper Methods
        private async Task ProcessPaymentSuccess(CreditCardPaymentViewModel model)
        {
            string userEmail = User.Identity.Name ?? "user@example.com";
            string formattedAmount = model.TotalAmount.ToString("F2", CultureInfo.InvariantCulture);
            string emailContent = $"Your payment of ${formattedAmount} has been successfully processed.";

            if (model.BookId.HasValue)
            {
                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == model.BookId.Value);
                if (book == null) throw new Exception("Book not found.");

                emailContent += $"\nYou purchased the book: {book.Title}.";
            }
            else
            {
                var userCart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserEmail == userEmail);
                if (userCart != null)
                {
                    _context.CartItems.RemoveRange(userCart.Items);
                    await _context.SaveChangesAsync();
                }
            }

            await _emailService.SendEmailAsync(userEmail, "Payment Confirmation", emailContent);
        }

        private float ConvertToUSD(float amount) => amount / 3.66f;

        private bool IsValidAmount(float amount) => amount > 0;

        private void SetPaymentMessage(string message) => TempData["PaymentMessage"] = message;

        private string GeneratePayPalLink(float amount) =>
            $"https://paypal.me/ebookstore22/{amount.ToString("F2", CultureInfo.InvariantCulture)}";
    }
}
