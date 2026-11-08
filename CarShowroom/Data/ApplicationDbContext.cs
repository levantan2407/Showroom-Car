using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CarShowroom.Models;

namespace CarShowroom.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<CarBrand> CarBrands { get; set; }
        public DbSet<CarCategory> CarCategories { get; set; }
        public DbSet<CarModel> CarModels { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<VehicleOwnership> VehicleOwnerships { get; set; }
        public DbSet<Warranty> Warranties { get; set; }
        public DbSet<WarrantyRequest> WarrantyRequests { get; set; }
        public DbSet<MaintenanceAppointment> MaintenanceAppointments { get; set; }
        public DbSet<MaintenanceHistory> MaintenanceHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ========== Customer ==========
            builder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasOne(e => e.User)
                      .WithOne()
                      .HasForeignKey<Customer>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== CarBrand ==========
            builder.Entity<CarBrand>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // ========== CarCategory ==========
            builder.Entity<CarCategory>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // ========== CarModel ==========
            builder.Entity<CarModel>(entity =>
            {
                entity.HasOne(e => e.Brand)
                      .WithMany(b => b.CarModels)
                      .HasForeignKey(e => e.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                      .WithMany(c => c.CarModels)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========== Car ==========
            builder.Entity<Car>(entity =>
            {
                entity.HasIndex(e => e.CarCode).IsUnique();
                entity.HasIndex(e => e.VIN).IsUnique().HasFilter("[VIN] IS NOT NULL");
                entity.HasIndex(e => e.BrandId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Year);

                entity.Property(e => e.Price).HasColumnType("decimal(18,0)");

                entity.HasOne(e => e.Brand)
                      .WithMany(b => b.Cars)
                      .HasForeignKey(e => e.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Cars)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CarModel)
                      .WithMany(m => m.Cars)
                      .HasForeignKey(e => e.ModelId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========== Order ==========
            builder.Entity<Order>(entity =>
            {
                entity.HasIndex(e => e.OrderCode).IsUnique();
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.CarId);
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Car)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(e => e.CarId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========== VehicleOwnership ==========
            builder.Entity<VehicleOwnership>(entity =>
            {
                entity.HasIndex(e => e.CarId).IsUnique();
                entity.HasIndex(e => e.OrderId).IsUnique();
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.LicensePlate).HasFilter("[LicensePlate] IS NOT NULL");

                entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,0)");

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.VehicleOwnerships)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Car)
                      .WithMany(c => c.VehicleOwnerships)
                      .HasForeignKey(e => e.CarId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Order)
                      .WithOne(o => o.VehicleOwnership)
                      .HasForeignKey<VehicleOwnership>(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========== Warranty ==========
            builder.Entity<Warranty>(entity =>
            {
                entity.HasIndex(e => e.WarrantyCode).IsUnique();
                entity.HasIndex(e => e.VehicleOwnershipId).IsUnique();
                entity.HasIndex(e => e.CarId);
                entity.HasIndex(e => e.CustomerId);

                entity.HasOne(e => e.VehicleOwnership)
                      .WithOne(v => v.Warranty)
                      .HasForeignKey<Warranty>(e => e.VehicleOwnershipId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.Warranties)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Car)
                      .WithMany(c => c.Warranties)
                      .HasForeignKey(e => e.CarId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========== WarrantyRequest ==========
            builder.Entity<WarrantyRequest>(entity =>
            {
                entity.HasIndex(e => e.RequestCode).IsUnique();

                entity.HasOne(e => e.Warranty)
                      .WithMany(w => w.WarrantyRequests)
                      .HasForeignKey(e => e.WarrantyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.WarrantyRequests)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Car)
                      .WithMany(c => c.WarrantyRequests)
                      .HasForeignKey(e => e.CarId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========== MaintenanceAppointment ==========
            builder.Entity<MaintenanceAppointment>(entity =>
            {
                entity.HasIndex(e => e.AppointmentCode).IsUnique();

                entity.Property(e => e.EstimatedCost).HasColumnType("decimal(18,0)");
                entity.Property(e => e.ActualCost).HasColumnType("decimal(18,0)");

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.MaintenanceAppointments)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Car)
                      .WithMany(c => c.MaintenanceAppointments)
                      .HasForeignKey(e => e.CarId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========== MaintenanceHistory ==========
            builder.Entity<MaintenanceHistory>(entity =>
            {
                entity.HasIndex(e => e.AppointmentId)
                      .IsUnique()
                      .HasFilter("[AppointmentId] IS NOT NULL");

                entity.Property(e => e.Cost).HasColumnType("decimal(18,0)");

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.MaintenanceHistories)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Car)
                      .WithMany(c => c.MaintenanceHistories)
                      .HasForeignKey(e => e.CarId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Appointment)
                      .WithOne(a => a.MaintenanceHistory)
                      .HasForeignKey<MaintenanceHistory>(e => e.AppointmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

