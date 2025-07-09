public class CampBillingSummaryDto
{
    public string Patient { get; set; } = default!;
    public string InvoiceNo { get; set; } = default!;
    public string BilledOn { get; set; } = default!;
    public int PaymentStatus { get; set; }
    public decimal Amount { get; set; }
    public string Billing { get; set; } = default!;
}
