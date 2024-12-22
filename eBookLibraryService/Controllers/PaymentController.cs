using eBookLibraryService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace eBookLibraryService.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult ProcessPayment()
        {
            // Display payment options
            return View();
        }

        [HttpPost]
        public IActionResult CompletePayment(string paymentMethod)
        {
            if (string.IsNullOrEmpty(paymentMethod))
            {
                TempData["Message"] = "Payment method is required.";
                return RedirectToAction("ProcessPayment");
            }

            try
            {
                if (paymentMethod == "PayPal")
                {
                    // Redirect to PayPal API (example implementation)
                    string payPalUrl = "https://www.paypal.com/checkout";
                    return Redirect(payPalUrl);
                }

                // Simulate payment success for other methods
                TempData["Message"] = "Payment was successful!";
                return RedirectToAction("PaymentSuccess");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Payment failed: {ex.Message}");
                TempData["Message"] = "Payment failed. Please try again.";
                return RedirectToAction("PaymentFailed");
            }
        }

        public IActionResult PaymentSuccess()
        {
            // Display success message and redirect to home
            TempData["Message"] = "Your payment was successful!";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult PaymentFailed()
        {
            // Display failure message and redirect to home
            TempData["Message"] = "Your payment failed. Please try again.";
            return RedirectToAction("Index", "Home");
        }
    }
}
