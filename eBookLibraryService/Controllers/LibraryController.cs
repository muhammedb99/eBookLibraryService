using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eBookLibraryService.Data;
using eBookLibraryService.ViewModels;
using eBookLibraryService.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace eBookLibraryService.Controllers
{
    public class LibraryController : Controller
    {
        private readonly eBookLibraryServiceContext _context;

        public LibraryController(eBookLibraryServiceContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "You must be logged in to view your library.";
                return RedirectToAction("Login", "Account");
            }

            var ownedBooks = await _context.OwnedBooks
                .Include(o => o.Book)
                .Where(o => o.UserEmail == userEmail && !o.IsBorrowed)
                .Select(o => new BookDetailsViewModel
                {
                    Id = o.Book.Id,
                    Title = o.Book.Title,
                    Author = o.Book.Author,
                    Publisher = o.Book.Publisher,
                    BorrowPrice = o.Book.BorrowPrice,
                    BuyingPrice = o.Book.BuyingPrice,
                    YearOfPublishing = o.Book.YearOfPublishing,
                    Genre = o.Book.Genre,
                    ImageUrl = o.Book.ImageUrl,
                    PdfLink = o.Book.PdfLink,
                    EpubLink = o.Book.EpubLink,
                    F2bLink = o.Book.F2bLink,
                    MobiLink = o.Book.MobiLink,
                    Reviews = o.Book.Reviews.Select(r => new ReviewViewModel
                    {
                        UserEmail = r.UserEmail,
                        Feedback = r.Feedback,
                        Rating = r.Rating
                    }).ToList()
                })
                .ToListAsync();

            var borrowedBooks = await _context.OwnedBooks
                .Include(b => b.Book)
                .Where(b => b.UserEmail == userEmail && b.IsBorrowed)
                .Select(b => new BookDetailsViewModel
                {
                    Id = b.Book.Id,
                    Title = b.Book.Title,
                    Author = b.Book.Author,
                    Publisher = b.Book.Publisher,
                    BorrowPrice = b.Book.BorrowPrice,
                    BuyingPrice = b.Book.BuyingPrice,
                    YearOfPublishing = b.Book.YearOfPublishing,
                    Genre = b.Book.Genre,
                    ImageUrl = b.Book.ImageUrl,
                    PdfLink = b.Book.PdfLink,
                    EpubLink = b.Book.EpubLink,
                    F2bLink = b.Book.F2bLink,
                    MobiLink = b.Book.MobiLink,
                    Reviews = b.Book.Reviews.Select(r => new ReviewViewModel
                    {
                        UserEmail = r.UserEmail,
                        Feedback = r.Feedback,
                        Rating = r.Rating
                    }).ToList(),
                    BorrowDueDate = b.BorrowDueDate 
                })
                .ToListAsync();

            var model = new LibraryViewModel
            {
                OwnedBooks = ownedBooks,
                BorrowedBooks = borrowedBooks
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> AddReview(int bookId, string feedback, int rating)
        {
            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Rating must be between 1 and 5.";
                return RedirectToAction("Index");
            }

            var userEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "You must be logged in to add a review.";
                return RedirectToAction("Login", "Account");
            }

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                TempData["ErrorMessage"] = "The book you are reviewing does not exist.";
                return RedirectToAction("Index");
            }

            var existingReview = await _context.Reviews.FirstOrDefaultAsync(r => r.BookId == bookId && r.UserEmail == userEmail);
            if (existingReview != null)
            {
                TempData["ErrorMessage"] = "You have already reviewed this book.";
                return RedirectToAction("Index");
            }

            var review = new Review
            {
                BookId = bookId,
                Feedback = feedback,
                Rating = rating,
                UserEmail = userEmail,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your review has been submitted successfully.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DownloadFile(string fileType, int bookId)
        {
            if (string.IsNullOrEmpty(fileType))
            {
                TempData["ErrorMessage"] = "Please select a valid file type.";
                return RedirectToAction("Index");
            }

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Book not found.";
                return RedirectToAction("Index");
            }

            string fileUrl = fileType switch
            {
                "PdfLink" => book.PdfLink,
                "EpubLink" => book.EpubLink,
                "F2bLink" => book.F2bLink,
                "MobiLink" => book.MobiLink,
                _ => null
            };

            if (string.IsNullOrEmpty(fileUrl))
            {
                TempData["ErrorMessage"] = "The selected file type is unavailable.";
                return RedirectToAction("Index");
            }

            return Redirect(fileUrl);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOwnedBook(int bookId)
        {
            var userEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "You must be logged in to delete a book.";
                return RedirectToAction("Login", "Account");
            }

            var ownedBook = await _context.OwnedBooks
                .FirstOrDefaultAsync(o => o.BookId == bookId && o.UserEmail == userEmail && !o.IsBorrowed);

            if (ownedBook == null)
            {
                TempData["ErrorMessage"] = "The book you are trying to delete does not exist in your library.";
                return RedirectToAction("Index");
            }

            _context.OwnedBooks.Remove(ownedBook);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "The book has been successfully removed from your library.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> BorrowedBooks()
        {
            var userEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "You must be logged in to view borrowed books.";
                return RedirectToAction("Login", "Account");
            }

            var borrowedBooks = await _context.OwnedBooks
                .Include(b => b.Book)
                .Where(b => b.UserEmail == userEmail && b.IsBorrowed)
                .ToListAsync();

            ViewBag.BorrowedBooksCount = borrowedBooks.Count;

            return View(borrowedBooks); 
        }

    }
}
