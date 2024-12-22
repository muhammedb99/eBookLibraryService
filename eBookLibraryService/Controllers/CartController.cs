using eBookLibraryService.Data;
using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
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

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cart = HttpContext.Session.GetObject<Cart>("Cart") ?? new Cart();
            ViewBag.CartItemCount = cart.Items.Count;
            base.OnActionExecuting(context);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int id, bool isBorrow)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book != null)
            {
                var cart = GetCart();

                // Calculate the price based on borrow or buy
                float price = isBorrow ?
                    (book.BorrowPrice ?? 0) :
                    (book.DiscountPrice.HasValue && book.DiscountUntil.HasValue && book.DiscountUntil.Value >= DateTime.Now
                        ? book.DiscountPrice.Value
                        : book.BuyingPrice);

                var cartItem = new CartItem
                {
                    Id = new Random().Next(1, 100000), // Generate unique ID
                    Book = book,
                    IsBorrow = isBorrow,
                    Price = price
                };

                cart.AddToCart(cartItem);
                SaveCart(cart);
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(Dictionary<int, string> cartItems)
        {
            var cart = GetCart();

            foreach (var cartItem in cart.Items)
            {
                if (cartItems.TryGetValue(cartItem.Id, out var isBorrowValue))
                {
                    // Ensure the incoming value is parsed correctly
                    var isBorrow = bool.TryParse(isBorrowValue, out var result) && result;

                    cartItem.IsBorrow = isBorrow;

                    // Update the price based on the updated option
                    cartItem.Price = cartItem.IsBorrow
                        ? (cartItem.Book.BorrowPrice ?? 0)
                        : (cartItem.Book.DiscountPrice.HasValue &&
                           cartItem.Book.DiscountUntil.HasValue &&
                           cartItem.Book.DiscountUntil.Value >= DateTime.Now
                            ? cartItem.Book.DiscountPrice.Value
                            : cartItem.Book.BuyingPrice);
                }
            }

            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int itemId)
        {
            var cart = GetCart();
            cart.RemoveFromCart(itemId);
            SaveCart(cart);

            return RedirectToAction(nameof(Index));
        }

        private Cart GetCart()
        {
            return HttpContext.Session.GetObject<Cart>("Cart") ?? new Cart();
        }

        private void SaveCart(Cart cart)
        {
            HttpContext.Session.SetObject("Cart", cart);
        }
    }
}
