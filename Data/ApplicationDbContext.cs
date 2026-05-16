using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GMLSSystem.Models;

namespace GMLSSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add your business DbSets
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Client configuration
            builder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.ClientId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Region).IsRequired().HasMaxLength(100);

                entity.HasMany(e => e.Contracts)
                    .WithOne(c => c.Client)
                    .HasForeignKey(c => c.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Contract configuration
            builder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.ContractId);
                entity.HasIndex(e => e.ContractNumber).IsUnique();
                entity.Property(e => e.ContractNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ClientName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.ServiceLevel).HasConversion<string>();

                entity.HasOne(e => e.Client)
                    .WithMany(c => c.Contracts)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ServiceRequest configuration
            builder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(e => e.ServiceRequestId);
                entity.HasIndex(e => e.RequestNumber).IsUnique();
                entity.Property(e => e.RequestNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.CostUSD).HasPrecision(18, 2);
                entity.Property(e => e.CostZAR).HasPrecision(18, 2);

                entity.HasOne(e => e.Contract)
                    .WithMany(c => c.ServiceRequests)
                    .HasForeignKey(e => e.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ApplicationUser configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(e => e.Client)
                    .WithMany()
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}