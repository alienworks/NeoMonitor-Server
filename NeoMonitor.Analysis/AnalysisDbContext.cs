using System;
using Microsoft.EntityFrameworkCore;
using NeoMonitor.Analysis.Models;

namespace NeoMonitor.Analysis
{
    public sealed class AnalysisDbContext : DbContext
    {
        public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options) : base(options)
        {
        }

        public DbSet<IpVisitData> IpVisitors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var t = modelBuilder
                .Entity<IpVisitData>()
                .ToTable(nameof(IpVisitors) + DateTime.UtcNow.ToString("_yyyyMM"));
            t.Property(p => p.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();
            t.Property(p => p.Ip)
                .HasMaxLength(50)
                .IsRequired();
            t.HasKey(p => p.Id);
            t.HasIndex(p => p.Ip);
            t.HasIndex(p => p.Day);
        }
    }
}