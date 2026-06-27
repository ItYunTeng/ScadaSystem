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
        containerRegistry.RegisterForNavigation<Views.TrendView, ViewModels.TrendViewModel>();
    }
}