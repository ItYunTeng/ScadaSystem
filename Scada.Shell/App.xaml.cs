using System.Windows;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Scada.Application.Services;
using Scada.Communication.Drivers;
using Scada.Core.Models;
using Scada.Shell.ViewModels;

namespace Scada.Shell;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.Register<MainWindowViewModel>();
        // 注册单例服务
        containerRegistry.RegisterSingleton<IDataAcquisitionService, DataAcquisitionService>();
        containerRegistry.RegisterSingleton<IAlarmService, AlarmService>();
        // 注册通信驱动工厂
        containerRegistry.RegisterInstance<Func<string, int, byte, ModbusTcpDriver>>((ip, port, slaveId) => new ModbusTcpDriver(ip, port, slaveId));
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        base.ConfigureModuleCatalog(moduleCatalog);
        moduleCatalog.AddModule<MonitorModule.MonitorModule>();
        moduleCatalog.AddModule<AlarmModule.AlarmModule>();
        moduleCatalog.AddModule<TrendModule.TrendModule>();
        moduleCatalog.AddModule<ReportModule.ReportModule>();
        moduleCatalog.AddModule<ConfigModule.ConfigModule>();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        // 初始化模拟数据
        InitializeDemoData();
        // 启动数据采集
        var acquisitionService = Container.Resolve<IDataAcquisitionService>();
        var cancellationTokenSource = new CancellationTokenSource();
        _ = Task.Run(() => acquisitionService.StartAcquisitionAsync(cancellationTokenSource.Token));
    }

    private void InitializeDemoData()
    {
        var acquisitionService = Container.Resolve<IDataAcquisitionService>();
        // 注册模拟驱动
        var driver = new ModbusTcpDriver("127.0.0.1", 502, 1);
        if (acquisitionService is DataAcquisitionService service)
        {
            service.AddDriver("PLC01", driver);
        }

        // 添加模拟点位
        var tags = new List<TagPoint>
        {
            new()
            {
                Id = "TAG001", Name = "储罐液位", Address = "HR:100",Value = 2.34,
                DataType = TagDataType.Float, Unit = "m"
            },
            new()
            {
                Id = "TAG002", Name = "管道压力", Address = "HR:102",Value = 1.24,
                DataType = TagDataType.Float, Unit = "MPa"
            },
            new()
            {
                Id = "TAG003", Name = "温度传感器", Address = "HR:104",Value = 0.23,
                DataType = TagDataType.Float, Unit = "℃"
            },
            new()
            {
                Id = "TAG004", Name = "流量累计", Address = "HR:106",Value = 33.23,
                DataType = TagDataType.Float, Unit = "m?/h"
            },
            new()
            {
                Id = "TAG005", Name = "泵运行状态", Address = "CO:0",Value = 3.45,
                DataType = TagDataType.Bool
            }
        };
        foreach (var tag in tags)
        {
            acquisitionService.AddTag(tag, "PLC01");
        }
    }
}