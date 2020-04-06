using Microsoft.EntityFrameworkCore;
using NeoMonitor.Basics.Models;

namespace NeoMonitor.DbContexts
{
    public class NeoMonitorContext : DbContext
    {
        public NeoMonitorContext(DbContextOptions<NeoMonitorContext> options) : base(options)
        {
        }

        public DbSet<NodeException> NodeExceptionList { get; set; }
        public DbSet<Node> Nodes { get; set; }
    }
}