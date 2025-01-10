using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using eBookLibraryService.Data;

public class DeleteExpiredBorrowingsService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeleteExpiredBorrowingsService> _logger;

    public DeleteExpiredBorrowingsService(IServiceProvider serviceProvider, ILogger<DeleteExpiredBorrowingsService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<eBookLibraryServiceContext>();

                var today = DateTime.UtcNow.Date;

                // Find all expired borrowed books
                var expiredBooks = await context.OwnedBooks
                    .Where(b => b.BorrowDueDate.Date <= today && b.IsBorrowed)
                    .Include(b => b.Book)
                    .ToListAsync();

                foreach (var expiredBook in expiredBooks)
                {
                    // Remove the expired borrowing
                    context.OwnedBooks.Remove(expiredBook);

                    // Check waiting list for the book
                    var waitingListEntry = await context.WaitingListEntries
                        .Where(w => w.BookId == expiredBook.BookId)
                        .OrderBy(w => w.DateAdded) // Get the earliest request
                        .FirstOrDefaultAsync();

                    if (waitingListEntry != null)
                    {
                        // Notify the user that they can borrow the book
                        await NotifyUserAsync(waitingListEntry.UserId, expiredBook.Book.Title);

                        // Remove the user from the waiting list
                        context.WaitingListEntries.Remove(waitingListEntry);
                    }
                }

                // Save changes to the database
                if (expiredBooks.Any())
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("{Count} expired borrowings removed at {Time}.", expiredBooks.Count, DateTime.UtcNow);
                }
            }

            // Run the task once every 24 hours
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task NotifyUserAsync(string email, string bookTitle)
    {
        // Replace with your email sending logic
        _logger.LogInformation("Sending email to {Email} about availability of {BookTitle}.", email, bookTitle);

        // Example logic for sending email
        await Task.Run(() =>
        {
            // Email sending logic here
            Console.WriteLine($"Email sent to {email}: '{bookTitle} is now available for borrowing.'");
        });
    }
}
