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
        public async Task<IActionResult> AddToCart(int id, bool isBorrow)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["CartMessage"] = "You must be logged in to add items to your cart.";
                return RedirectToAction("Login", "Account");
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                TempData["CartMessage"] = "The selected book does not exist.";
                return RedirectToAction("Index", "Home");
            }

            var userEmail = User.Identity.Name;
            var cart = await GetOrCreateCartAsync(userEmail);

            if (cart.Items.Any(i => i.Book.Id == id))
            {
                TempData["CartMessage"] = "This book is already in your cart.";
                return RedirectToAction("Index", "Home");
            }

            var price = isBorrow
                ? book.BorrowPrice.GetValueOrDefault()
                : (book.DiscountPrice > 0 && book.DiscountUntil.HasValue && book.DiscountUntil.Value >= DateTime.Now
                    ? book.DiscountPrice.GetValueOrDefault() : book.BuyingPrice);

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