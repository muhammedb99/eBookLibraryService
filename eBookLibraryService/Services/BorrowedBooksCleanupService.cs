using eBookLibraryService.Data;
using Microsoft.EntityFrameworkCore;

public class BorrowedBooksCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public BorrowedBooksCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<eBookLibraryServiceContext>();

                // Find expired borrowed books
                var expiredBooks = await context.BorrowedBooks
                    .Where(b => b.ReturnDate <= DateTime.Now)
                    .ToListAsync();

                foreach (var borrowedBook in expiredBooks)
                {
                    // Decrement BorrowCount
                    var book = await context.Books.FindAsync(borrowedBook.BookId);
                    if (book != null)
                    {
                        book.BorrowCount = Math.Max(0, book.BorrowCount - 1);
                    }

                    // Remove the expired borrowed book
                    context.BorrowedBooks.Remove(borrowedBook);

                    // Notify the next user in the waiting list
                    var nextUser = await context.WaitingListEntries
                        .Where(w => w.BookId == borrowedBook.BookId)
                        .OrderBy(w => w.DateAdded) // Get the earliest entry
                        .FirstOrDefaultAsync();

                    if (nextUser != null)
                    {
                        // Example email notification logic
                        await SendEmailNotification(nextUser.UserId, book.Title);

                        // Remove the user from the waiting list
                        context.WaitingListEntries.Remove(nextUser);
                    }
                }

                await context.SaveChangesAsync();
            }

            // Run every hour (or adjust as needed)
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task SendEmailNotification(string userEmail, string bookTitle)
    {
        // Implement email sending logic here
        Console.WriteLine($"Email sent to {userEmail}: '{bookTitle}' is now available to borrow.");
        await Task.CompletedTask;
    }
}
