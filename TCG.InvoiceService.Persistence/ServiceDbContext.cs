using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TCG.InvoiceService.Domain;

namespace TCG.InvoiceService.Persistence;

public class ServiceDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public ServiceDbContext()
    {
        
    }
    public ServiceDbContext(DbContextOptions<ServiceDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
        Database.Migrate();
    }

    public DbSet<Dispute> Disputes { get; set; }
    public DbSet<DisputeState> DisputeStates { get; set; }
    public DbSet<InvoiceBody> InvoiceBodies { get; set; }
    public DbSet<InvoiceHead> InvoiceHeads { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderState> OrderStates { get; set; }

    public Task<int> SaveChangesAsync()
    {
        return base.SaveChangesAsync();
    }

    public Task Migrate()
    {
        return base.Database.MigrateAsync();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Order>()
            .HasOne(p => p.OrderState)
            .WithMany(s => s.Orders)
            .HasForeignKey(p => p.OrderStateId)
            .IsRequired();
        
        modelBuilder.Entity<Order>()
            .HasOne(p => p.InvoiceHead)
            .WithOne(s => s.Order)
            .HasForeignKey<InvoiceHead>(p => p.OrderId)
            .IsRequired();
        
        modelBuilder.Entity<InvoiceBody>()
            .HasOne(p => p.InvoiceHead)
            .WithMany(s => s.InvoiceBodies)
            .HasForeignKey(p => p.InvoiceHeadId)
            .IsRequired();
        
        modelBuilder.Entity<Order>()
            .HasOne(p => p.Dispute)
            .WithOne(s => s.Order)
            .HasForeignKey<Dispute>(p => p.OrderId)
            .IsRequired();
        
        modelBuilder.Entity<Dispute>()
            .HasOne(p => p.DisputeState)
            .WithMany(s => s.Disputes)
            .HasForeignKey(p => p.DisputeStateId)
            .IsRequired();
    }
}