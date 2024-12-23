using eBookLibraryService.Data;
using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace eBookLibraryService.Controllers
{
    public class CartController : Controller
    {
        private readonly eBookLibraryServiceContext _context;

        public CartController(eBookLibraryServiceContext context)
        {
            _context = context;
        }

        // Updates the cart item count on every action
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cart = HttpContext.Session.GetObject<Cart>("Cart") ?? new Cart();
            ViewBag.CartItemCount = cart.Items.Count;
            base.OnActionExecuting(context);
        }

        // Add item to cart and calculate price based on borrow or buy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int id, bool isBorrow)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book != null)
            {
                var cart = GetCart();

                // Calculate price based on borrow or buy
                var price = isBorrow ? (book.BorrowPrice ?? 0) : book.BuyingPrice;

                var cartItem = new CartItem
                {
                    Book = book,
                    IsBorrow = isBorrow,
                    Price = price
                };

                cart.AddToCart(cartItem);
                SaveCart(cart);
            }

            return RedirectToAction("Index", "Home");
        }

        // Display the cart
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // Remove item from cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int itemId)
        {
            var cart = GetCart();
            cart.RemoveFromCart(itemId);
            SaveCart(cart);

            return RedirectToAction(nameof(Index));
        }

        // Update cart when options are changed (Buy to Borrow or Borrow to Buy)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(Dictionary<int, string> cartItems)
        {
            var cart = GetCart();

            // Loop through each cart item and update its IsBorrow value
            foreach (var cartItem in cart.Items)
            {
                if (cartItems.TryGetValue(cartItem.Id, out var isBorrowValue))
                {
                    // Ensure the value is parsed correctly
                    var isBorrow = bool.TryParse(isBorrowValue, out var result) && result;

                    // Update IsBorrow flag
                    cartItem.IsBorrow = isBorrow;

                    // Update price based on the IsBorrow value
                    cartItem.Price = cartItem.IsBorrow
                        ? (cartItem.Book.BorrowPrice ?? 0)
                        : cartItem.Book.BuyingPrice;
                }
            }

            SaveCart(cart);  // Save updated cart
            return RedirectToAction(nameof(Index));  // Redirect to cart view
        }


        // Helper method to get the cart from session
        private Cart GetCart()
        {
            var cart = HttpContext.Session.GetObject<Cart>("Cart") ?? new Cart();
            return cart;
        }

        // Helper method to save the cart back into session
        private void SaveCart(Cart cart)
        {
            HttpContext.Session.SetObject("Cart", cart);
        }
    }
}
