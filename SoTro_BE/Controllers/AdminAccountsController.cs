using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoTro_BE.Data;
using SoTro_BE.DTOs.Auth;
using SoTro_BE.Models;

namespace SoTro_BE.Controllers
{
    [ApiController]
    [Route("api/admin/accounts")]
    [Authorize]
    public class AdminAccountsController : ControllerBase
    {
        private readonly SoTroDbContext _context;

        public AdminAccountsController(SoTroDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            var users = await _context.Users
                .AsNoTracking()
                .Include(user => user.Role)
                .Include(user => user.Landlord)
                    .ThenInclude(landlord => landlord!.LandlordSubscriptions)
                        .ThenInclude(subscription => subscription.SubscriptionPlan)
                .OrderByDescending(user => user.CreatedAt)
                .ToListAsync();

            var accounts = users.Select(user =>
            {
                var activeSubscription = user.Landlord?.LandlordSubscriptions
                    .Where(subscription => string.Equals(subscription.Status, "Active", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(subscription => subscription.StartDate)
                    .FirstOrDefault();

                return new AdminAccountResponse
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    RoleId = user.RoleId,
                    RoleName = user.Role?.RoleName,
                    Status = string.IsNullOrWhiteSpace(user.Status) ? "Active" : user.Status,
                    CreatedAt = user.CreatedAt,
                    LandlordId = user.Landlord?.LandlordId,
                    CurrentPlanId = activeSubscription?.PlanId,
                    CurrentPlanName = activeSubscription?.SubscriptionPlan?.PlanName ?? "Basic",
                    SubscriptionEndDate = activeSubscription?.EndDate
                };
            }).ToList();

            return Ok(ApiResponse<List<AdminAccountResponse>>.Ok("Lay danh sach account thanh cong.", accounts));
        }

        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans()
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            await EnsureDefaultPlansAsync();

            var plans = await _context.SubscriptionPlans
                .AsNoTracking()
                .Where(plan => plan.IsActive != false)
                .Select(plan => new AdminSubscriptionPlanResponse
                {
                    PlanId = plan.PlanId,
                    PlanName = plan.PlanName,
                    Price = plan.Price,
                    DurationDays = plan.DurationDays,
                    MaxBuildings = plan.MaxBuildings,
                    MaxRooms = plan.MaxRooms
                })
                .ToListAsync();

            plans.Sort((left, right) =>
            {
                var rankCompare = GetPlanSortRank(left.PlanName).CompareTo(GetPlanSortRank(right.PlanName));
                if (rankCompare != 0)
                {
                    return rankCompare;
                }

                return (left.Price ?? 0).CompareTo(right.Price ?? 0);
            });

            return Ok(ApiResponse<List<AdminSubscriptionPlanResponse>>.Ok("Lay danh sach goi dich vu thanh cong.", plans));
        }

        [HttpPut("{userId:int}/status")]
        public async Task<IActionResult> UpdateAccountStatus(int userId, [FromBody] UpdateAccountStatusRequest request)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            if (userId == GetCurrentUserId())
            {
                return BadRequest(ApiResponse<bool>.Fail("Admin khong the tu khoa tai khoan dang dang nhap."));
            }

            var normalizedStatus = NormalizeStatus(request.Status);
            if (normalizedStatus == null)
            {
                return BadRequest(ApiResponse<bool>.Fail("Trang thai account khong hop le."));
            }

            var user = await _context.Users.FirstOrDefaultAsync(item => item.UserId == userId);
            if (user == null)
            {
                return NotFound(ApiResponse<bool>.Fail("Khong tim thay account."));
            }

            user.Status = normalizedStatus;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok("Cap nhat trang thai account thanh cong.", true));
        }

        [HttpPut("{userId:int}/subscription")]
        public async Task<IActionResult> UpdateAccountSubscription(int userId, [FromBody] UpdateAccountSubscriptionRequest request)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            var user = await _context.Users
                .Include(item => item.Landlord)
                    .ThenInclude(landlord => landlord!.LandlordSubscriptions)
                .FirstOrDefaultAsync(item => item.UserId == userId);

            if (user == null)
            {
                return NotFound(ApiResponse<bool>.Fail("Khong tim thay account."));
            }

            if (user.RoleId == 1)
            {
                return BadRequest(ApiResponse<bool>.Fail("Khong the doi goi dich vu cho tai khoan admin."));
            }

            if (user.Landlord == null)
            {
                user.Landlord = new Landlord
                {
                    UserId = user.UserId,
                    DisplayName = user.FullName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Landlords.Add(user.Landlord);
                await _context.SaveChangesAsync();
            }

            var now = DateTime.UtcNow;
            await EnsureDefaultPlansAsync();

            foreach (var subscription in user.Landlord.LandlordSubscriptions.Where(item =>
                string.Equals(item.Status, "Active", StringComparison.OrdinalIgnoreCase)))
            {
                subscription.Status = "Expired";
                subscription.EndDate = subscription.EndDate.HasValue && subscription.EndDate.Value < now ? subscription.EndDate : now;
                subscription.UpdatedAt = now;
            }

            if (request.PlanId.HasValue && request.PlanId.Value > 0)
            {
                var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(item => item.PlanId == request.PlanId.Value);
                if (plan == null)
                {
                    return BadRequest(ApiResponse<bool>.Fail("Goi dich vu khong ton tai."));
                }

                _context.LandlordSubscriptions.Add(new LandlordSubscription
                {
                    LandlordId = user.Landlord.LandlordId,
                    PlanId = plan.PlanId,
                    StartDate = now,
                    EndDate = now.AddDays(plan.DurationDays.GetValueOrDefault(30)),
                    Status = "Active",
                    AutoRenew = false,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            user.UpdatedAt = now;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok("Cap nhat goi dich vu thanh cong.", true));
        }

        private bool IsAdmin()
        {
            return string.Equals(User.FindFirstValue("RoleId"), "1", StringComparison.Ordinal);
        }

        private static int GetPlanSortRank(string? planName)
        {
            var normalized = (planName ?? string.Empty).Trim().ToLowerInvariant();
            return normalized switch
            {
                "basic" => 0,
                "premium" => 1,
                _ => 2
            };
        }

        private int? GetCurrentUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
        }

        private static string? NormalizeStatus(string? status)
        {
            return status?.Trim().ToLowerInvariant() switch
            {
                "active" => "Active",
                "inactive" => "Inactive",
                "locked" => "Locked",
                _ => null
            };
        }

        private async Task EnsureDefaultPlansAsync()
        {
            var now = DateTime.UtcNow;
            var plans = await _context.SubscriptionPlans.ToListAsync();

            if (!plans.Any(plan => string.Equals(plan.PlanName, "Basic", StringComparison.OrdinalIgnoreCase)))
            {
                _context.SubscriptionPlans.Add(new SubscriptionPlan
                {
                    PlanName = "Basic",
                    Price = 0,
                    DurationDays = 3650,
                    MaxBuildings = 1,
                    MaxRooms = 20,
                    CanUseZalo = false,
                    CanUseFacebookPosting = false,
                    CanUseOCR = false,
                    CanExportExcel = true,
                    Description = "Goi co ban, khong bao gom tinh nang tu dong hoa VIP.",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            if (!plans.Any(plan => string.Equals(plan.PlanName, "Premium", StringComparison.OrdinalIgnoreCase)))
            {
                _context.SubscriptionPlans.Add(new SubscriptionPlan
                {
                    PlanName = "Premium",
                    Price = 99000,
                    DurationDays = 30,
                    MaxBuildings = 10,
                    MaxRooms = 500,
                    CanUseZalo = true,
                    CanUseFacebookPosting = true,
                    CanUseOCR = true,
                    CanExportExcel = true,
                    Description = "Mo khoa ngay xuat/gui hoa don va tu dong hoa VIP.",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            await _context.SaveChangesAsync();
        }
    }

    public class AdminAccountResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? LandlordId { get; set; }
        public int? CurrentPlanId { get; set; }
        public string? CurrentPlanName { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
    }

    public class AdminSubscriptionPlanResponse
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int? DurationDays { get; set; }
        public int? MaxBuildings { get; set; }
        public int? MaxRooms { get; set; }
    }

    public class UpdateAccountStatusRequest
    {
        public string? Status { get; set; }
    }

    public class UpdateAccountSubscriptionRequest
    {
        public int? PlanId { get; set; }
    }
}
