using System.Threading.Tasks;
using NeoMonitor.App.Abstractions.Models;

namespace NeoMonitor.App.Abstractions.Services.Data
{
    public interface IRawMemPoolDataLoader
    {
        Task<RawMemPoolSizeModel[]> LoadAsync();
    }
}