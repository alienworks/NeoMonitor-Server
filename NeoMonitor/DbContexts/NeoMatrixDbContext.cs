using Microsoft.EntityFrameworkCore;
using NeoMonitor.Abstractions.Models;

namespace NeoMonitor.DbContexts
{
    public sealed class NeoMatrixDbContext : DbContext
    {
        public NeoMatrixDbContext(DbContextOptions<NeoMatrixDbContext> options) : base(options)
        {
        }

        public DbSet<NeoMatrixItemEntity> MatrixItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<NeoMatrixItemEntity>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Id).ValueGeneratedOnAdd();
                e.HasIndex(p => p.GroupId);
                e.Property(p => p.GroupId).IsRequired();
                e.HasIndex(p => p.Url);
                e.Property(p => p.Url).HasColumnType("VARCHAR(50)").IsRequired();
                e.HasIndex(p => p.Method);
                e.Property(p => p.Method).HasColumnType("VARCHAR(30)").IsRequired();
                e.Property(p => p.Net).HasColumnType("VARCHAR(20)");
                e.Property(p => p.Error).HasColumnType("VARCHAR(500)");
                e.Property(p => p.CreateTime)
                .HasColumnType("DATETIME")
                .HasDefaultValueSql("now()")
                .IsRequired();
            });
        }
    }
}