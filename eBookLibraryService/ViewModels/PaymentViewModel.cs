namespace eBookLibraryService.ViewModels;

public class PaymentViewModel
{
    public float TotalAmount { get; set; }
    public int? BookId { get; set; }
    public string FormattedAmount => TotalAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
}
