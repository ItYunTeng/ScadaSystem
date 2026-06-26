using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Scada.TrendModule;

public class TrendModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion("MainRegion", typeof(Views.TrendView));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<Views.TrendView>();
        
        // 【重点】如果有 ViewModel，一定要注册！
        // 即使你没有显式地在 XAML 里写 DataContext，Prism 的自动绑定机制也需要它在容器里
        containerRegistry.RegisterForNavigation<ViewModels.TrendViewModel>(); 
    }
}