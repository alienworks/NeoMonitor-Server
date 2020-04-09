using System.Threading.Tasks;
using NeoMonitor.Basics.Models;

namespace NeoMonitor.App.Abstractions.Services.Data
{
    public interface IRawMemPoolDataLoader
    {
        Task<RawMemPoolSizeModel[]> LoadAsync();
    }
}