using System.ComponentModel.DataAnnotations;

namespace SoTro_BE.DTOs.Invoice
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
        [Required]
        public int BuildingId { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }

        [Range(2000, 9999)]
        public int Year { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TrashFee { get; set; }

        [Range(0, double.MaxValue)]
        public decimal InternetFee { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CleaningFee { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ManagementFee { get; set; }
    }

    public class MarkInvoicePaidRequest
    {
        [Range(0, double.MaxValue)]
        public decimal? Amount { get; set; }

        public string PaymentMethod { get; set; } = "BankTransfer";
        public string? Note { get; set; }
    }
}
