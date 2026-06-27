using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Scada.MonitorModule;

public class MonitorModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        Console.WriteLine("-------");
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 注册视图和ViewModel用于Prism导航
        containerRegistry.RegisterForNavigation<Views.MonitorView, ViewModels.MonitorViewModel>();
    }
}