using AutoMapper;


namespace NeoMonitor.Infrastructure.Mapping
{
    public class AutoMapperConfig
    {
        public static void Init()
        {
            Mapper.Initialize(cfg => {
                NodeConfig.InitMap(cfg);
            });
        }
    }
}
