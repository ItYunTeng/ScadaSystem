using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Scada.AlarmModule;

public class AlarmModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
    }
    
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 注册视图用于导航
        // Console.WriteLine(typeof(Views.AlarmView));
        containerRegistry.RegisterForNavigation<Views.AlarmView>();
    }
}