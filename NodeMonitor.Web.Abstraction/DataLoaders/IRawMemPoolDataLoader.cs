using System.Threading.Tasks;
using NodeMonitor.Web.Abstraction.Models;

namespace NodeMonitor.Web.Abstraction.DataLoaders
{
    public interface IRawMemPoolDataLoader
    {
        Task<RawMemPoolData[]> LoadAsync();
    }
}