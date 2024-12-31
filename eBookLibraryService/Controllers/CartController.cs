using eBookLibraryService.Data;
using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
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
            if (User.Identity.IsAuthenticated)
            {
                var userEmail = User.Identity.Name;
                var cartItemCount = _context.Carts
                    .Include(c => c.Items)
                    .Where(c => c.UserEmail == userEmail)
                    .SelectMany(c => c.Items)
                    .Count();

                ViewBag.CartItemCount = cartItemCount;
            }
            else
            {
                ViewBag.CartItemCount = 0;
            }

            base.OnActionExecuting(context);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyNow(int id, float amount)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["CartMessage"] = "You must be logged in to purchase a book.";
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.Identity.Name;

            // Check if the book already exists in the user's library
            var bookInLibrary = await _context.OwnedBooks.AnyAsync(o => o.UserEmail == userEmail && o.BookId == id);
            if (bookInLibrary)
            {
                TempData["CartMessage"] = "This book is already in your library.";
                return RedirectToAction("Index", "Home");
            }

            // Fetch the book
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                TempData["CartMessage"] = "The selected book does not exist.";
                return RedirectToAction("Index", "Home");
            }

            // Add the book directly to the OwnedBooks table
            var ownedBook = new OwnedBook
            {
                UserEmail = userEmail,
                BookId = book.Id,
                Title = book.Title,
                Author = book.Author,
                IsBorrowed = false, // Purchased books are not borrowed
                Price = amount,
                PurchaseDate = DateTime.Now,
                BorrowEndDate = null // Not applicable for purchased books
            };

            _context.OwnedBooks.Add(ownedBook);

            // Save changes to the database
            await _context.SaveChangesAsync();

            TempData["CartMessage"] = "Book purchased successfully and added to your library.";
            return RedirectToAction("Index", "Library");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int id, bool isBorrow)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["CartMessage"] = "You must be logged in to add items to your cart.";
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.Identity.Name;

            // Check if the book already exists in the user's library (owned or borrowed)
            var bookInLibrary = await _context.OwnedBooks.AnyAsync(o => o.UserEmail == userEmail && o.BookId == id) ||
                                await _context.BorrowedBooks.AnyAsync(b => b.UserEmail == userEmail && b.BookId == id);

            if (bookInLibrary)
            {
                TempData["CartMessage"] = "This book is already in your library and cannot be added to the cart.";
                return RedirectToAction("Index", "Home");
            }

            // Fetch the book
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                TempData["CartMessage"] = "The selected book does not exist.";
                return RedirectToAction("Index", "Home");
            }

            // Get or create the user's cart
            var cart = await GetOrCreateCartAsync(userEmail);

            // Check if the book is already in the cart
            if (cart.Items.Any(i => i.Book.Id == id))
            {
                TempData["CartMessage"] = "This book is already in your cart.";
                return RedirectToAction("Index", "Home");
            }

            // Determine the price
            var price = isBorrow
                ? book.BorrowPrice.GetValueOrDefault()
                : (book.DiscountPrice > 0 && book.DiscountUntil.HasValue && book.DiscountUntil.Value >= DateTime.Now
                    ? book.DiscountPrice.GetValueOrDefault() : book.BuyingPrice);

            // Add the book to the cart
            var cartItem = new CartItem
            {
                Book = book,
                IsBorrow = isBorrow,
                Price = price
            };

            cart.Items.Add(cartItem);
            _context.Update(cart);
            await _context.SaveChangesAsync();

            TempData["CartMessage"] = isBorrow ? "Book added to your cart for borrowing." : "Book added to your cart for buying.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["CartMessage"] = "You must be logged in to view your cart.";
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.Identity.Name;
            var cart = await GetOrCreateCartAsync(userEmail);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int itemId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["CartMessage"] = "You must be logged in to modify your cart.";
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.Identity.Name;
            var cart = await GetOrCreateCartAsync(userEmail);

            var itemToRemove = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
                _context.Update(cart);
                await _context.SaveChangesAsync();
            }

            TempData["CartMessage"] = "Book removed from your cart.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["CartMessage"] = "You must be logged in to clear your cart.";
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.Identity.Name;
            var cart = await GetOrCreateCartAsync(userEmail);

            cart.Items.Clear();
            _context.Update(cart);
            await _context.SaveChangesAsync();

            TempData["CartMessage"] = "Cart cleared successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<Cart> GetOrCreateCartAsync(string userEmail)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(c => c.UserEmail == userEmail);

            if (cart == null)
            {
                cart = new Cart { UserEmail = userEmail };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }
    }
}
