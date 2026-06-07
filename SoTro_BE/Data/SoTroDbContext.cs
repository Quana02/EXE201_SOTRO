using Microsoft.EntityFrameworkCore;
using SoTro_BE.Models;

namespace SoTro_BE.Data
{
    public class SoTroDbContext : DbContext
    {
        public SoTroDbContext(DbContextOptions<SoTroDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<OtpVerification> OtpVerifications { get; set; } = null!;
        public DbSet<PendingRegistration> PendingRegistrations { get; set; } = null!;
        public DbSet<Landlord> Landlords { get; set; } = null!;
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
        public DbSet<LandlordSubscription> LandlordSubscriptions { get; set; } = null!;
        public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; } = null!;
        public DbSet<Building> Buildings { get; set; } = null!;
        public DbSet<RoomType> RoomTypes { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<RoomImage> RoomImages { get; set; } = null!;
        public DbSet<RoomStatusHistory> RoomStatusHistories { get; set; } = null!;
        public DbSet<RoomAsset> RoomAssets { get; set; } = null!;
        public DbSet<Tenant> Tenants { get; set; } = null!;
        public DbSet<TenantDocument> TenantDocuments { get; set; } = null!;
        public DbSet<RentalRecord> RentalRecords { get; set; } = null!;
        public DbSet<RentalAttachment> RentalAttachments { get; set; } = null!;
        public DbSet<RoomOccupant> RoomOccupants { get; set; } = null!;
        public DbSet<TenantMember> TenantMembers { get; set; } = null!;
        public DbSet<DepositTransaction> DepositTransactions { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<RoomService> RoomServices { get; set; } = null!;
        public DbSet<ServicePriceHistory> ServicePriceHistories { get; set; } = null!;
        public DbSet<BillingSchedule> BillingSchedules { get; set; } = null!;
        public DbSet<MeterReading> MeterReadings { get; set; } = null!;
        public DbSet<AdditionalCharge> AdditionalCharges { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<InvoiceItem> InvoiceItems { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<BankAccount> BankAccounts { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
        public DbSet<IntegrationSetting> IntegrationSettings { get; set; } = null!;
        public DbSet<ZaloMessage> ZaloMessages { get; set; } = null!;
        public DbSet<ReminderLog> ReminderLogs { get; set; } = null!;
        public DbSet<FacebookPostLog> FacebookPostLogs { get; set; } = null!;
        public DbSet<MaintenanceReport> MaintenanceReports { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Roles
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(e => e.RoleName).IsUnique();
            });

            // Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.GoogleId);
                
                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OTP Verifications
            modelBuilder.Entity<OtpVerification>(entity =>
            {
                entity.HasIndex(e => new { e.Email, e.Purpose, e.IsUsed });
                entity.HasIndex(e => e.ResetPasswordToken);
            });

            // Pending Registrations
            modelBuilder.Entity<PendingRegistration>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Landlords
            modelBuilder.Entity<Landlord>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Landlord)
                    .HasForeignKey<Landlord>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // LandlordSubscriptions
            modelBuilder.Entity<LandlordSubscription>(entity =>
            {
                entity.HasOne(d => d.Landlord)
                    .WithMany(p => p.LandlordSubscriptions)
                    .HasForeignKey(d => d.LandlordId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.SubscriptionPlan)
                    .WithMany(p => p.LandlordSubscriptions)
                    .HasForeignKey(d => d.PlanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SubscriptionPayments
            modelBuilder.Entity<SubscriptionPayment>(entity =>
            {
                entity.HasOne(d => d.LandlordSubscription)
                    .WithMany(p => p.SubscriptionPayments)
                    .HasForeignKey(d => d.SubscriptionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Buildings
            modelBuilder.Entity<Building>(entity =>
            {
                entity.HasOne(d => d.Landlord)
                    .WithMany(p => p.Buildings)
                    .HasForeignKey(d => d.LandlordId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RoomTypes
            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.HasOne(d => d.Landlord)
                    .WithMany(p => p.RoomTypes)
                    .HasForeignKey(d => d.LandlordId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Rooms
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasIndex(e => new { e.BuildingId, e.RoomCode }).IsUnique();

                entity.HasOne(d => d.Building)
                    .WithMany(p => p.Rooms)
                    .HasForeignKey(d => d.BuildingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.RoomType)
                    .WithMany(p => p.Rooms)
                    .HasForeignKey(d => d.RoomTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RoomImages
            modelBuilder.Entity<RoomImage>(entity =>
            {
                entity.HasOne(d => d.Room)
                    .WithMany(p => p.RoomImages)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RoomStatusHistories
            modelBuilder.Entity<RoomStatusHistory>(entity =>
            {
                entity.HasOne(d => d.Room)
                    .WithMany(p => p.RoomStatusHistories)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Changer)
                    .WithMany(p => p.RoomStatusHistories)
                    .HasForeignKey(d => d.ChangedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RoomAssets
            modelBuilder.Entity<RoomAsset>(entity =>
            {
                entity.HasOne(d => d.Room)
                    .WithMany(p => p.RoomAssets)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Tenants
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasOne(d => d.Landlord)
                    .WithMany(p => p.Tenants)
                    .HasForeignKey(d => d.LandlordId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TenantDocuments
            modelBuilder.Entity<TenantDocument>(entity =>
            {
                entity.HasOne(d => d.Tenant)
                    .WithMany(p => p.TenantDocuments)
                    .HasForeignKey(d => d.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RentalRecords
            modelBuilder.Entity<RentalRecord>(entity =>
            {
                entity.HasOne(d => d.Room)
                    .WithMany(p => p.RentalRecords)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Tenant)
                    .WithMany(p => p.RentalRecords)
                    .HasForeignKey(d => d.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RentalAttachments
            modelBuilder.Entity<RentalAttachment>(entity =>
            {
                entity.HasOne(d => d.RentalRecord)
                    .WithMany(p => p.RentalAttachments)
                    .HasForeignKey(d => d.RentalId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RoomOccupants
            modelBuilder.Entity<RoomOccupant>(entity =>
            {
                entity.HasOne(d => d.Room)
                    .WithMany(p => p.RoomOccupants)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.RentalRecord)
                    .WithMany(p => p.RoomOccupants)
                    .HasForeignKey(d => d.RentalId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Tenant)
                    .WithMany(p => p.RoomOccupants)
                    .HasForeignKey(d => d.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TenantMembers
            modelBuilder.Entity<TenantMember>(entity =>
            {
                entity.HasOne(d => d.RentalRecord)
                    .WithMany(p => p.TenantMembers)
                    .HasForeignKey(d => d.RentalId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DepositTransactions
            modelBuilder.Entity<DepositTransaction>(entity =>
            {
                entity.HasOne(d => d.RentalRecord)
                    .WithMany(p => p.DepositTransactions)
                    .HasForeignKey(d => d.RentalId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Services
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasOne(d => d.Landlord)
                    .WithMany(p => p.Services)
                    .HasForeignKey(d => d.LandlordId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RoomServices
            modelBuilder.Entity<RoomService>(entity =>
            {
                entity.HasOne(d => d.Room)
                    .WithMany(p => p.RoomServices)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.RoomServices)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ServicePriceHistories
            modelBuilder.Entity<ServicePriceHistory>(entity =>
            {
                entity.HasOne(d => d.RoomService)
                    .WithMany(p => p.ServicePriceHistories)
                    .HasForeignKey(d => d.RoomServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BillingSchedules
            modelBuilder.Entity<BillingSchedule>(entity =>
            {
                entity.HasIndex(e => new { e.BuildingId, e.Month, e.Year }).IsUnique();

                entity.HasOne(d => d.Building)
                    .WithMany(p => p.BillingSchedules)
                    .HasForeignKey(d => d.BuildingId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // MeterReadings
            modelBuilder.Entity<MeterReading>(entity =>
            {
                entity.HasIndex(e => new { e.RoomId, e.Month, e.Year }).IsUnique();

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.MeterReadings)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Recorder)
                    .WithMany(p => p.MeterReadings)
                    .HasForeignKey(d => d.RecordedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AdditionalCharges
            modelBuilder.Entity<AdditionalCharge>(entity =>
            {
                entity.HasOne(d => d.Room)
                    .WithMany(p => p.AdditionalCharges)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.RentalRecord)
                    .WithMany(p => p.AdditionalCharges)
                    .HasForeignKey(d => d.RentalId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Invoices
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasIndex(e => e.InvoiceCode).IsUnique();
                entity.HasIndex(e => new { e.RoomId, e.Month, e.Year }).IsUnique();

                entity.HasOne(d => d.Landlord)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.LandlordId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.RentalRecord)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.RentalId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // InvoiceItems
            modelBuilder.Entity<InvoiceItem>(entity =>
            {
                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.InvoiceItems)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Payments
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Receiver)
                    .WithMany(p => p.ReceivedPayments)
                    .HasForeignKey(d => d.ReceivedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BankAccounts
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.HasOne(d => d.Landlord)
                    .WithMany(p => p.BankAccounts)
                    .HasForeignKey(d => d.LandlordId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SystemSettings
            modelBuilder.Entity<SystemSetting>(entity =>
            {
                entity.HasIndex(e => e.LandlordId).IsUnique();

                entity.HasOne(d => d.Landlord)
                    .WithOne(p => p.SystemSetting)
                    .HasForeignKey<SystemSetting>(d => d.LandlordId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // IntegrationSettings
            modelBuilder.Entity<IntegrationSetting>(entity =>
            {
                entity.HasOne(d => d.Landlord)
                    .WithMany(p => p.IntegrationSettings)
                    .HasForeignKey(d => d.LandlordId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ZaloMessages
            modelBuilder.Entity<ZaloMessage>(entity =>
            {
                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.ZaloMessages)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ReminderLogs
            modelBuilder.Entity<ReminderLog>(entity =>
            {
                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.ReminderLogs)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // FacebookPostLogs
            modelBuilder.Entity<FacebookPostLog>(entity =>
            {
                entity.HasOne(d => d.Room)
                    .WithMany(p => p.FacebookPostLogs)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // MaintenanceReports
            modelBuilder.Entity<MaintenanceReport>(entity =>
            {
                entity.HasOne(d => d.Room)
                    .WithMany(p => p.MaintenanceReports)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Reporter)
                    .WithMany(p => p.MaintenanceReports)
                    .HasForeignKey(d => d.ReportedByTenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AuditLogs
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.AuditLogs)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
