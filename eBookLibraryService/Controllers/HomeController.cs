using eBookLibraryService.Data;
using eBookLibraryService.Helpers;
using eBookLibraryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace eBookLibraryService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly eBookLibraryServiceContext _context;

        public HomeController(ILogger<HomeController> logger, eBookLibraryServiceContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Executed before every action
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userEmail = User.Identity.Name;
                    var cart = _context.Carts
                        .Include(c => c.Items)
                        .FirstOrDefault(c => c.UserEmail == userEmail);

                    ViewBag.CartItemCount = cart?.Items.Count ?? 0;
                }
                else
                {
                    var cart = HttpContext.Session.GetObject<Cart>("Cart") ?? new Cart();
                    ViewBag.CartItemCount = cart.Items.Count;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing the cart in session or database.");
                ViewBag.CartItemCount = 0;
            }

            base.OnActionExecuting(context);
        }

        [HttpPost]
        public async Task<IActionResult> BorrowBook(int bookId)
        {
            try
            {
                var userId = User.Identity.Name; // Assuming User.Identity.Name is the user's email
                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);

                if (book == null)
                {
                    _logger.LogWarning($"Book with ID {bookId} not found.");
                    return NotFound("Book not found.");
                }

                // Check if the book is already fully borrowed
                if (book.BorrowedCopies >= 3)
                {
                    TempData["Error"] = "The book is currently unavailable. You can join the waiting list.";
                    _logger.LogInformation($"Book {bookId} is fully borrowed. Redirecting user to the waiting list.");
                    return RedirectToAction("JoinWaitingList", new { bookId });
                }

                // Increment borrowed copies and BorrowCount
                book.BorrowedCopies++;
                book.BorrowCount++;

                var borrowDate = DateTime.Now; // Borrowing date
                var borrowEndDate = borrowDate.AddDays(30); // Example: 30-day borrow period

                var borrowedBook = new BorrowedBook
                {
                    BookId = bookId,
                    UserEmail = userId,
                    BorrowedDate = borrowDate,
                    ReturnDate = borrowEndDate
                };

                _logger.LogInformation($"Before Save: BookId={bookId}, BorrowedCopies={book.BorrowedCopies}, BorrowCount={book.BorrowCount}");

                // Explicitly attach and update the book
                _context.Books.Attach(book);
                _context.Entry(book).Property(b => b.BorrowedCopies).IsModified = true;
                _context.Entry(book).Property(b => b.BorrowCount).IsModified = true;

                _context.BorrowedBooks.Add(borrowedBook);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"After Save: BorrowedCopies={book.BorrowedCopies}, BorrowCount={book.BorrowCount}");

                TempData["Success"] = "You have successfully borrowed the book!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while borrowing the book.");
                TempData["Error"] = "An error occurred while processing your request. Please try again.";
                return RedirectToAction("Index");
            }
        }


        // Join Waiting List Action
        [HttpGet]
        public IActionResult JoinWaitingList(int bookId)
        {
            var waitingListEntry = new WaitingListEntry
            {
                BookId = bookId,
                UserId = User.Identity.Name,
                DateAdded = DateTime.Now
            };

            _context.WaitingListEntries.Add(waitingListEntry);
            _context.SaveChanges();

            TempData["Success"] = "You have been added to the waiting list.";
            return RedirectToAction("Index");
        }

        // Main Index action
        public async Task<IActionResult> Index(
    string query = null,
    string author = null,
    string genre = null,
    string method = null,
    float? minPrice = null,
    float? maxPrice = null,
    bool? isOnSale = null,
    int? year = null,
    string publisher = null,
    string sortOrder = null)
        {
            try
            {
                // Fetch books with reviews included
                var booksQuery = _context.Books
                    .Include(b => b.Reviews)
                    .AsQueryable();

                // Apply filters
                booksQuery = ApplyFilters(booksQuery, query, author, genre, method, minPrice, maxPrice, isOnSale, year, publisher);

                // Apply sorting
                booksQuery = ApplySorting(booksQuery, sortOrder);

                // Fetch all books and group by genre
                var allBooks = await booksQuery.ToListAsync();
                var genres = allBooks
                    .GroupBy(b => b.Genre)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.ToList());

                // Set genres to ViewBag
                ViewBag.GenreBooks = genres.Any() ? genres : new Dictionary<string, List<Book>>();

                // Fetch distinct genres for dropdown
                var genreList = await _context.Books
                    .Where(b => !string.IsNullOrEmpty(b.Genre))
                    .Select(b => b.Genre.Trim())
                    .Distinct()
                    .OrderBy(g => g)
                    .ToListAsync();

                // Set genres list in ViewBag
                ViewBag.Genres = genreList;

                // Retrieve the latest 10 feedbacks
                var feedbacks = await _context.ServiceFeedbacks
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(10) // Show the latest 10 feedback entries
                    .ToListAsync();

                // Set feedbacks in ViewBag
                ViewBag.Feedbacks = feedbacks;

                return View(allBooks);
            }
            catch (Exception ex)
            {
                // Log error and set default values in ViewBag
                _logger.LogError(ex, "Error retrieving books or feedbacks for the Index page.");
                ViewBag.GenreBooks = new Dictionary<string, List<Book>>();
                ViewBag.Genres = new List<string>();
                ViewBag.Feedbacks = new List<ServiceFeedback>();
                return View(new List<Book>());
            }
        }


        private IQueryable<Book> ApplyFilters(IQueryable<Book> books, string query, string author, string genre, string method, float? minPrice, float? maxPrice, bool? isOnSale, int? year, string publisher)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                books = books.Where(b => EF.Functions.Like(b.Title, $"%{query}%") || EF.Functions.Like(b.Author, $"%{query}%"));
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                books = books.Where(b => EF.Functions.Like(b.Author, $"%{author}%"));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                books = books.Where(b => b.Genre == genre);
            }

            if (!string.IsNullOrWhiteSpace(method))
            {
                books = method.ToLower() switch
                {
                    "buy" => books.Where(b => b.BuyingPrice > 0),
                    "borrow" => books.Where(b => b.BorrowPrice > 0),
                    _ => books
                };
            }

            if (minPrice.HasValue)
            {
                books = books.Where(b => (b.DiscountPrice ?? b.BuyingPrice) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                books = books.Where(b => (b.DiscountPrice ?? b.BuyingPrice) <= maxPrice.Value);
            }

            if (isOnSale.HasValue && isOnSale.Value)
            {
                books = books.Where(b =>
                    b.DiscountPrice.HasValue &&
                    b.DiscountPrice < b.BuyingPrice &&
                    b.DiscountUntil.HasValue &&
                    b.DiscountUntil.Value >= DateTime.Now
                );
            }

            if (year.HasValue)
            {
                books = books.Where(b => b.YearOfPublishing == year.Value);
            }

            if (!string.IsNullOrWhiteSpace(publisher))
            {
                books = books.Where(b => EF.Functions.Like(b.Publisher, $"%{publisher}%"));
            }

            return books;
        }

        private IQueryable<Book> ApplySorting(IQueryable<Book> books, string sortOrder)
        {
            return sortOrder switch
            {
                "price_asc" => books.OrderBy(b => b.DiscountPrice ?? b.BuyingPrice),
                "price_desc" => books.OrderByDescending(b => b.DiscountPrice ?? b.BuyingPrice),
                "popular" => books.OrderByDescending(b => b.Popularity),
                "genre" => books.OrderBy(b => b.Genre),
                "year" => books.OrderByDescending(b => b.YearOfPublishing),
                _ => books
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(int rating, string feedback)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.Name; // Assuming User.Identity.Name stores user email or username
                var newFeedback = new ServiceFeedback
                {
                    UserId = userId,
                    Rating = rating,
                    Feedback = feedback,
                    CreatedAt = DateTime.Now
                };

                _context.ServiceFeedbacks.Add(newFeedback);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thank you for your feedback!";
            }
            else
            {
                TempData["Error"] = "You must be logged in to submit feedback.";
            }

            return RedirectToAction("Index");
        }

    }
}