using eBookLibraryService.Data;
using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;

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

                // Determine applicable price
                float price;
                if (!isBorrow && book.DiscountPrice.HasValue && book.DiscountUntil.HasValue && book.DiscountUntil.Value >= DateTime.Now)
                {
                    price = book.DiscountPrice.Value; // Apply discounted price
                }
                else if (!isBorrow)
                {
                    price = book.BuyingPrice; // Regular buying price
                }
                else
                {
                    price = book.BorrowPrice ?? 0; // Borrow price
                }

                // Add item to the cart with the correct price
                var cartItem = new CartItem
                {
                    Book = book,
                    IsBorrow = isBorrow,
                    Price = price // Assign the determined price
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
        public IActionResult RemoveFromCart(int itemId)
        {
            var cart = GetCart();
            cart.RemoveFromCart(itemId);
            SaveCart(cart);

            return RedirectToAction(nameof(Index));
        }

        private Cart GetCart()
        {
            var cart = HttpContext.Session.GetObject<Cart>("Cart") ?? new Cart();
            return cart;
        }

        private void SaveCart(Cart cart)
        {
            HttpContext.Session.SetObject("Cart", cart);
        }
    }
}
