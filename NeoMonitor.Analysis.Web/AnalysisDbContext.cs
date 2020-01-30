using Microsoft.EntityFrameworkCore;
using NeoMonitor.Analysis.Web.Models;

namespace NeoMonitor.Analysis.Web
{
    public sealed class AnalysisDbContext : DbContext
    {
        public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options) : base(options)
        {
        }

        public DbSet<IpVisitAnaData> IpVisitors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var t = modelBuilder.Entity<IpVisitAnaData>();
            t.Property(p => p.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();
            t.Property(p => p.Ip)
                .HasMaxLength(50)
                .IsRequired();
            t.HasKey(p => p.Id);
            t.HasIndex(p => p.Ip);
            t.HasIndex(p => new { p.Year, p.Month, p.Day })
                .HasName("Index_Date");
        }
    }
}