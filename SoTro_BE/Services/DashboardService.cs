using Microsoft.EntityFrameworkCore;
using SoTro_BE.Data;
using SoTro_BE.DTOs.Auth;
using SoTro_BE.DTOs.Dashboard;

namespace SoTro_BE.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly SoTroDbContext _context;

        public DashboardService(SoTroDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<DashboardSummaryResponse>> GetSummaryAsync(int landlordId)
        {
            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            var currentYear = now.Year;
            var previousMonth = now.AddMonths(-1);

            var rooms = await _context.Rooms
                .Include(r => r.Building)
                .Where(r => r.IsDeleted != true && r.Building != null && r.Building.LandlordId == landlordId && r.Building.IsDeleted != true)
                .AsNoTracking()
                .ToListAsync();

            var invoices = await _context.Invoices
                .Include(i => i.Room)
                .Where(i => i.IsDeleted != true && i.LandlordId == landlordId)
                .AsNoTracking()
                .ToListAsync();

            var monthlyRevenue = invoices
                .Where(i => i.Month == currentMonth && i.Year == currentYear)
                .Sum(i => i.PaidAmount ?? (i.Status == "Paid" ? i.TotalAmount ?? 0 : 0));

            var previousRevenue = invoices
                .Where(i => i.Month == previousMonth.Month && i.Year == previousMonth.Year)
                .Sum(i => i.PaidAmount ?? (i.Status == "Paid" ? i.TotalAmount ?? 0 : 0));

            var revenueGrowth = previousRevenue > 0
                ? Math.Round((monthlyRevenue - previousRevenue) / previousRevenue * 100, 1)
                : monthlyRevenue > 0 ? 100 : 0;

            var lastSixMonths = Enumerable.Range(0, 6)
                .Select(offset => now.AddMonths(-5 + offset))
                .Select(date => new DashboardRevenuePoint
                {
                    Month = date.Month,
                    Year = date.Year,
                    Revenue = invoices
                        .Where(i => i.Month == date.Month && i.Year == date.Year)
                        .Sum(i => i.PaidAmount ?? (i.Status == "Paid" ? i.TotalAmount ?? 0 : 0))
                })
                .ToList();

            var totalRooms = rooms.Count;
            var occupiedRooms = rooms.Count(r => r.Status == "Occupied");

            var recentInvoices = invoices
                .OrderByDescending(i => i.CreatedAt ?? i.IssueDate ?? DateTime.MinValue)
                .Take(5)
                .Select(i => new DashboardActivityItem
                {
                    TimeLabel = FormatTimeLabel(i.CreatedAt ?? i.IssueDate),
                    Content = $"Hóa đơn tháng {i.Month}/{i.Year}",
                    RoomCode = i.Room?.RoomCode,
                    Status = i.Status,
                    Amount = i.TotalAmount
                })
                .ToList();

            var summary = new DashboardSummaryResponse
            {
                MonthlyRevenue = monthlyRevenue,
                RevenueGrowthPercent = revenueGrowth,
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                OccupancyRate = totalRooms > 0 ? Math.Round((decimal)occupiedRooms / totalRooms * 100, 1) : 0,
                PendingInvoices = invoices.Count(i => i.Status is "Unpaid" or "PartiallyPaid" or "Overdue" || (i.RemainingAmount ?? 0) > 0),
                RevenueLastSixMonths = lastSixMonths,
                RecentActivities = recentInvoices
            };

            return ApiResponse<DashboardSummaryResponse>.Ok("Lấy tổng quan dashboard thành công.", summary);
        }

        private static string FormatTimeLabel(DateTime? value)
        {
            if (!value.HasValue)
                return string.Empty;

            var local = value.Value.ToLocalTime();
            var today = DateTime.Now.Date;
            if (local.Date == today)
                return local.ToString("HH:mm");
            if (local.Date == today.AddDays(-1))
                return "Hôm qua";
            return local.ToString("dd/MM/yyyy");
        }
    }
}
