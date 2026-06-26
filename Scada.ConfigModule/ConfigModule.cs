using Prism.Ioc;
using Prism.Modularity;

namespace Scada.ConfigModule;

public class ConfigModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        // 将视图注入到主区域
        // var regionManager = containerProvider.Resolve<IRegionManager>();
        // regionManager.RegisterViewWithRegion("MainRegion", typeof(Views.ConfigView));
    }
    
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 注册视图用于导航
        containerRegistry.RegisterForNavigation<Views.ConfigView>();
    }
}