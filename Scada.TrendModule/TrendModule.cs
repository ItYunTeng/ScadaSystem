using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Scada.TrendModule;

public class TrendModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<Views.TrendView, ViewModels.TrendViewModel>();
    }
}