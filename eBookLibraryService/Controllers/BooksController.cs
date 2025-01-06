using eBookLibraryService.Data;
using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using eBookLibraryService.Services;
using eBookLibraryService.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eBookLibraryService.Controllers
{
    public class BooksController : Controller
    {
        private readonly eBookLibraryServiceContext _context;
        private readonly AppDbContext _appDbContext;
        private readonly NotificationService _notificationService;

        public BooksController(eBookLibraryServiceContext context, AppDbContext appDbContext, NotificationService notificationService)
        {
            _context = context;
            _appDbContext = appDbContext;
            _notificationService = notificationService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cart = HttpContext.Session.GetObject<Cart>("Cart");
            ViewBag.CartItemCount = cart?.Items.Count ?? 0;
            base.OnActionExecuting(context);
        }

        // View all books
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.Include(b => b.WaitingList).ToListAsync();

            var updatedBooks = new List<Book>();

            // Handle discount expiration
            foreach (var book in books)
            {
                if (book.DiscountPrice.HasValue && book.DiscountPrice > 0)
                {
                    var discountStartDate = book.CreatedDate;
                    var discountEndDate = discountStartDate.AddDays(7);

                    if (DateTime.Now > discountEndDate)
                    {
                        book.DiscountPrice = null;
                        updatedBooks.Add(book);
                    }
                }
            }

            if (updatedBooks.Any())
            {
                _context.UpdateRange(updatedBooks);
                await _context.SaveChangesAsync();
            }

            return View(books);
        }

        // Admin-only: Manage books
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageBooks()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        // Admin-only: Create books
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [Bind("Title,Author,Publisher,BorrowPrice,BuyingPrice,YearOfPublishing,AgeLimitation,Genre,Popularity,DiscountPrice,DiscountUntil,ImageUrl,PdfLink,EpubLink,F2bLink,MobiLink")] Book book)
        {
            if (ModelState.IsValid)
            {
                if (book.BorrowPrice > book.BuyingPrice)
                {
                    ModelState.AddModelError("BorrowPrice", "The Borrow Price cannot be greater than the Buying Price.");
                    return View(book);
                }

                if (!ValidateFileLinks(book))
                {
                    return View(book);
                }

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // Admin-only: Edit books
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
        public async Task<IActionResult> Edit(
    int id,
    [Bind("Id,Title,Author,Publisher,BorrowPrice,BuyingPrice,YearOfPublishing,AgeLimitation,Genre,Popularity,DiscountPrice,DiscountUntil,ImageUrl,PdfLink,EpubLink,F2bLink,MobiLink")] Book book)
        {
            if (id != book.Id) return NotFound();

            // Ensure optional fields are not NULL
            book.EpubLink = book.EpubLink ?? string.Empty;
            book.F2bLink = book.F2bLink ?? string.Empty;
            book.MobiLink = book.MobiLink ?? string.Empty;

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


        // Admin-only: Delete books
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
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var model = new BookDetailsWithReviewViewModel
            {
                Title = book.Title,
                Author = book.Author,
                Publisher = book.Publisher,
                BorrowPrice = book.BorrowPrice,
                BuyingPrice = book.BuyingPrice,
                YearOfPublishing = book.YearOfPublishing,
                Genre = book.Genre,
                ImageUrl = book.ImageUrl,
                Reviews = book.Reviews.Select(r => new ReviewViewModel
                {
                    UserEmail = r.UserEmail,
                    Feedback = r.Feedback,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };

            return View(model);
        }

        private bool ValidateFileLinks(Book book)
        {
            bool isValid = true;

            if (!string.IsNullOrEmpty(book.PdfLink) && !Uri.IsWellFormedUriString(book.PdfLink, UriKind.Absolute))
            {
                ModelState.AddModelError("PdfLink", "Invalid PDF link.");
                isValid = false;
            }

            if (!string.IsNullOrEmpty(book.EpubLink) && !Uri.IsWellFormedUriString(book.EpubLink, UriKind.Absolute))
            {
                ModelState.AddModelError("EpubLink", "Invalid EPUB link.");
                isValid = false;
            }

            if (!string.IsNullOrEmpty(book.F2bLink) && !Uri.IsWellFormedUriString(book.F2bLink, UriKind.Absolute))
            {
                ModelState.AddModelError("F2bLink", "Invalid F2B link.");
                isValid = false;
            }

            if (!string.IsNullOrEmpty(book.MobiLink) && !Uri.IsWellFormedUriString(book.MobiLink, UriKind.Absolute))
            {
                ModelState.AddModelError("MobiLink", "Invalid MOBI link.");
                isValid = false;
            }

            return isValid;
        }
    }
}
