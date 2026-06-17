using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Invoice;

namespace SOTRO_Project.Services
{
    public interface IInvoiceApiService
    {
        Task<ApiResponse<List<InvoiceResponse>>> GetInvoicesAsync(int buildingId, int month, int year);
        Task<ApiResponse<List<InvoiceResponse>>> GenerateMonthlyInvoicesAsync(GenerateMonthlyInvoicesRequest request);
        Task<ApiResponse<InvoiceResponse>> MarkPaidAsync(int invoiceId, MarkInvoicePaidRequest request);
    }
}
