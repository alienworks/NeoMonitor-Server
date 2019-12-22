using Microsoft.EntityFrameworkCore;
using NeoMonitor.Data.Models;

namespace NeoMonitor.Data
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