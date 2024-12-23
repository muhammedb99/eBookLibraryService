using eBookLibraryService.Data;
using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace eBookLibraryService.Controllers
{
    public class BooksController : Controller
    {
        private readonly eBookLibraryServiceContext _context;

        public BooksController(eBookLibraryServiceContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cart = HttpContext.Session.GetObject<Cart>("Cart");
            ViewBag.CartItemCount = cart?.Items.Count ?? 0;
            base.OnActionExecuting(context);
        }

        // Available to all users: View books
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.ToListAsync();

            var updatedBooks = new List<Book>();  // To track updated books

            // Handle discount expiration
            foreach (var book in books)
            {
                if (book.DiscountPrice.HasValue && book.DiscountPrice > 0)
                {
                    var discountStartDate = book.CreatedDate;  // Ensure you have this field in your model
                    var discountEndDate = discountStartDate.AddDays(7);

                    if (DateTime.Now > discountEndDate)
                    {
                        // Remove the discount if it has expired
                        book.DiscountPrice = null;
                        updatedBooks.Add(book);  // Track updated book
                    }
                }
            }

            if (updatedBooks.Any())
            {
                _context.UpdateRange(updatedBooks);  // Update all books in one go
                await _context.SaveChangesAsync();  // Commit the changes
            }

            return View(books);
        }



        // Available to admins only: Manage books
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageBooks()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        // Available to admins only: Create books
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Title,Author,Publisher,BorrowPrice,BuyingPrice,YearOfPublishing,AgeLimitation,Quantity,Genre,Popularity,DiscountPrice,DiscountUntil,PublicationYears,Publishers,ImageUrl")] Book book)
        {
            if (ModelState.IsValid)
            {
                if (book.BorrowPrice > book.BuyingPrice)
                {
                    ModelState.AddModelError("BorrowPrice", "The Borrow Price cannot be greater than the Buying Price.");
                    return View(book);
                }

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // Available to admins only: Edit books
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title, Author, Publisher, BorrowPrice, BuyingPrice, YearOfPublishing, AgeLimitation, Genre, Popularity, DiscountPrice,DiscountUntil, PublicationYears, Publishers, ImageUrl")] Book book)
        {
            if (id != book.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (book.BorrowPrice > book.BuyingPrice)
                {
                    ModelState.AddModelError("BorrowPrice", "The Borrow Price cannot be greater than the Buying Price.");
                    return View(book);
                }

                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Books.Any(e => e.Id == book.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // Available to admins only: Delete books
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
            if (book == null) return NotFound();

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageBooks));
        }

        // View details of a book (Available to all users)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
            if (book == null) return NotFound();

            return View(book);
        }

        // Borrow book functionality
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null || book.Quantity <= 0)
            {
                return BadRequest("This book is not available for borrowing.");
            }

            book.Quantity -= 1;
            book.BorrowCount += 1;

            _context.Update(book);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Buy book functionality
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            book.PurchaseCount += 1;

            _context.Update(book);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
