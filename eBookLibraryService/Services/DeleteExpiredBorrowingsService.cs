using eBookLibraryService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

                var booksToRemind = await context.OwnedBooks
                    .Where(b => b.IsBorrowed && b.BorrowDueDate.Date == today.AddDays(5))
                    .Include(b => b.Book)
                    .ToListAsync();

                foreach (var book in booksToRemind)
                {
                    await SendReminderAsync(book.UserEmail, book.Book.Title, book.BorrowDueDate);

                    _logger.LogInformation("Reminder sent to {Email} for book '{BookTitle}' due on {DueDate}.",
                        book.UserEmail, book.Book.Title, book.BorrowDueDate);
                }

                var expiredBooks = await context.OwnedBooks
                    .Where(b => b.BorrowDueDate.Date <= today && b.IsBorrowed)
                    .Include(b => b.Book)
                    .ToListAsync();

                foreach (var expiredBook in expiredBooks)
                {
                    context.OwnedBooks.Remove(expiredBook);

                    var waitingListEntry = await context.WaitingListEntries
                        .Where(w => w.BookId == expiredBook.BookId)
                        .OrderBy(w => w.DateAdded) 
                        .FirstOrDefaultAsync();

                    if (waitingListEntry != null)
                    {
                        await NotifyUserAsync(waitingListEntry.UserId, expiredBook.Book.Title);

                        context.WaitingListEntries.Remove(waitingListEntry);
                    }
                }

                if (expiredBooks.Any() || booksToRemind.Any())
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("{ExpiredCount} expired borrowings removed and {ReminderCount} reminders sent at {Time}.",
                        expiredBooks.Count, booksToRemind.Count, DateTime.UtcNow);
                }
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task SendReminderAsync(string email, string bookTitle, DateTime dueDate)
    {
        _logger.LogInformation("Sending reminder to {Email} about book '{BookTitle}' due on {DueDate}.",
            email, bookTitle, dueDate);

        await Task.Run(() =>
        {
            Console.WriteLine($"Reminder sent to {email}: '{bookTitle}' is due on {dueDate:dd MMM yyyy}'.");
        });
    }

    private async Task NotifyUserAsync(string email, string bookTitle)
    {
        _logger.LogInformation("Sending email to {Email} about availability of {BookTitle}.", email, bookTitle);

        await Task.Run(() =>
        {
            Console.WriteLine($"Email sent to {email}: '{bookTitle} is now available for borrowing.'");
        });
    }
}
