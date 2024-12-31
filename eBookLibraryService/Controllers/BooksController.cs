using eBookLibraryService.Data;
using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using eBookLibraryService.Services;
using eBookLibraryService.ViewModels;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,Publisher,BorrowPrice,BuyingPrice,YearOfPublishing,AgeLimitation,Genre,Popularity,DiscountPrice,DiscountUntil,PublicationYears,Publishers,ImageUrl")] Book book)
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
        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .Include(b => b.Reviews) // Include reviews
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
                Quantity = book.Quantity,
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



        // Borrow book functionality
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Borrow(int id)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                TempData["Error"] = "You must be logged in to borrow a book.";
                return RedirectToAction("Index");
            }

            var book = await _context.Books.Include(b => b.WaitingList).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null || book.Quantity <= 0)
            {
                TempData["Error"] = "This book is not available for borrowing.";
                return RedirectToAction("Index");
            }

            if (_context.BorrowedBooks.Count(bb => bb.UserEmail == user.FullName && !bb.IsReturned) >= 3)
            {
                TempData["Error"] = "You cannot borrow more than 3 books at the same time.";
                return RedirectToAction("Index");
            }

            book.Quantity--;
            _context.BorrowedBooks.Add(new BorrowedBook
            {
                BookId = book.Id,
                UserEmail = user.Email,
                BorrowedDate = DateTime.Now,
                ReturnDate = DateTime.Now.AddDays(30),
                IsReturned = false
            });

            await _context.SaveChangesAsync();
            TempData["Message"] = "Book borrowed successfully.";
            return RedirectToAction("Index");
        }

        public async Task NotifyWaitingUsersAsync()
        {
            var booksWithWaitingList = _context.Books.Include(b => b.WaitingList)
                                                     .Where(b => b.WaitingList.Any())
                                                     .ToList();

            foreach (var book in booksWithWaitingList)
            {
                if (book.BorrowedCopies < book.Quantity && book.WaitingList.Any())
                {
                    var firstInLine = book.WaitingList.OrderBy(w => w.DateAdded).First();
                    var user = _appDbContext.Users.FirstOrDefault(u => u.UserName == firstInLine.UserId);
                    if (user != null)
                    {
                        await _notificationService.SendEmailAsync(
                            recipientEmail: user.Email,
                            subject: "Book Available for Borrowing",
                            message: $"The book '{book.Title}' is now available for borrowing. Please act quickly to secure your copy."
                        );

                        book.WaitingList.Remove(firstInLine);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }


        // Buy book functionality
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Buy(int id)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user == null)
            {
                TempData["Error"] = "You must be logged in to buy a book.";
                return RedirectToAction("Index");
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                TempData["Error"] = "The selected book does not exist.";
                return RedirectToAction("Index");
            }

            _context.BorrowedBooks.Add(new BorrowedBook
            {
                BookId = book.Id,
                UserEmail = user.Email,
                BorrowedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["Message"] = "Book purchased successfully.";
            return RedirectToAction("Index");
        }
    }
}
