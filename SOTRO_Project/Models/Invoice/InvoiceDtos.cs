namespace SOTRO_Project.Models.Invoice
{
    public class InvoiceResponse
    {
        public int InvoiceId { get; set; }
        public string InvoiceCode { get; set; } = string.Empty;
        public int? RoomId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string TenantPhone { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal RoomPrice { get; set; }
        public decimal ElectricCost { get; set; }
        public decimal WaterCost { get; set; }
        public decimal ServiceCost { get; set; }
        public decimal OtherCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Status { get; set; } = "Unpaid";
    }

    public class GenerateMonthlyInvoicesRequest
    {
        public int BuildingId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TrashFee { get; set; }
        public decimal InternetFee { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal ManagementFee { get; set; }
    }

    public class MarkInvoicePaidRequest
    {
        public decimal? Amount { get; set; }
        public string PaymentMethod { get; set; } = "BankTransfer";
        public string? Note { get; set; }
    }
}
