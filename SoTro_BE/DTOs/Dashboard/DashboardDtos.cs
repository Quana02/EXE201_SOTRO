namespace SoTro_BE.DTOs.Dashboard
{
    public class DashboardSummaryResponse
    {
        public decimal MonthlyRevenue { get; set; }
        public decimal RevenueGrowthPercent { get; set; }
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public decimal OccupancyRate { get; set; }
        public int PendingInvoices { get; set; }
        public List<DashboardRevenuePoint> RevenueLastSixMonths { get; set; } = new();
        public List<DashboardActivityItem> RecentActivities { get; set; } = new();
    }

    public class DashboardRevenuePoint
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DashboardActivityItem
    {
        public string TimeLabel { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? RoomCode { get; set; }
        public string? Status { get; set; }
        public decimal? Amount { get; set; }
    }
}
