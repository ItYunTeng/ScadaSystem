using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Scada.ReportModule;

public class ReportModule:IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion("MainRegion", typeof(Views.ReportView));
    }
    
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 注册视图用于导航
        containerRegistry.RegisterForNavigation<Views.ReportView>();
    }
}