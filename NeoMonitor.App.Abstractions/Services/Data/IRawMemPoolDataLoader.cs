using System.Threading.Tasks;
using NeoMonitor.App.Abstractions.ViewModels;

namespace NeoMonitor.App.Abstractions.Services.Data
{
    public interface IRawMemPoolDataLoader
    {
        Task<RawMemPoolSizeModel[]> LoadAsync();
    }
}