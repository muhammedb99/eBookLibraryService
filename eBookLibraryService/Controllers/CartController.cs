using eBookLibraryService.Data;
using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> AddToCart(int id, bool isBorrow)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                TempData["CartMessage"] = "The selected book does not exist.";
                return RedirectToAction("Index", "Home");
            }

            var cart = GetCart();

            // Check for duplicate items in the cart (borrow/buy distinction)
            var existingItem = cart.Items.FirstOrDefault(i => i.Book.Id == id && i.IsBorrow == isBorrow);
            if (existingItem != null)
            {
                TempData["CartMessage"] = "This book is already in your cart with the selected option.";
                return RedirectToAction("Index", "Home");
            }

            if (isBorrow)
            {
                // Borrow-specific logic
                var currentUserEmail = User.Identity.Name;
                if (string.IsNullOrEmpty(currentUserEmail))
                {
                    TempData["CartMessage"] = "You must be logged in to borrow a book.";
                    return RedirectToAction("Index", "Home");
                }

                // Check if the user has already borrowed 3 books
                var currentBorrowedCount = await _context.BorrowedBooks
                    .CountAsync(bb => bb.Email == currentUserEmail && !bb.IsReturned);
                if (currentBorrowedCount >= 3)
                {
                    TempData["CartMessage"] = "You have reached the borrowing limit of 3 books.";
                    return RedirectToAction("Index", "Home");
                }

                // Check if the book is available for borrowing
                if (book.BorrowCount >= book.Quantity)
                {
                    TempData["CartMessage"] = "This book is currently unavailable for borrowing.";
                    return RedirectToAction("Index", "Home");
                }

                // Increment the borrow count
                book.BorrowCount++;
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }

            // Calculate price based on borrow or buy
            var price = isBorrow ? (book.BorrowPrice ?? 0) : book.BuyingPrice;

            // Add the book to the cart
            var cartItem = new CartItem
            {
                Book = book,
                IsBorrow = isBorrow,
                Price = price
            };

            cart.AddToCart(cartItem);
            SaveCart(cart);

            TempData["CartMessage"] = isBorrow ? "Book added to your cart for borrowing." : "Book added to your cart for buying.";
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

            TempData["CartMessage"] = "Book removed from your cart.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(Dictionary<int, bool> cartItems)
        {
            var cart = GetCart();

            foreach (var item in cart.Items)
            {
                if (cartItems.TryGetValue(item.Id, out var isBorrow))
                {
                    item.IsBorrow = isBorrow;
                    item.Price = isBorrow ? (item.Book.BorrowPrice ?? 0) : item.Book.BuyingPrice;
                }
            }

            SaveCart(cart);
            TempData["CartMessage"] = "Cart updated successfully.";
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
