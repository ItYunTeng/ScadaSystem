using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using Scada.Application.Services;
using Scada.Core.Models;
using Prism.Mvvm;
using Prism.Regions;

namespace Scada.MonitorModule.ViewModels;

public class MonitorViewModel : BindableBase, INavigationAware
{
    private readonly IDataAcquisitionService _acquisitionService;
    private float _tankLevel;
    private float _reactorTemp;
    private float _reactorPressure;
    private float _flowRate;
    private bool _pumpStatus;

    // 模拟数据相关
    private readonly DispatcherTimer _simTimer;
    private double _simPhase;
    private int _pumpToggleCounter;
    private readonly Random _random = new();

    public float TankLevel
    {
        get => _tankLevel;
        set => SetProperty(ref _tankLevel, value);
    }

    public float ReactorTemp
    {
        get => _reactorTemp;
        set => SetProperty(ref _reactorTemp, value);
    }

    public float ReactorPressure
    {
        get => _reactorPressure;
        set => SetProperty(ref _reactorPressure, value);
    }

    public float FlowRate
    {
        get => _flowRate;
        set => SetProperty(ref _flowRate, value);
    }

    public Brush PumpStatusColor => _pumpStatus ? Brushes.Green : Brushes.Gray;
    public ObservableCollection<TagPoint> TagList { get; } = new();
    

    public MonitorViewModel(IDataAcquisitionService acquisitionService)
    {
        _acquisitionService = acquisitionService;
        _acquisitionService.TagValueUpdated += OnTagValueUpdated;
        // 初始化列表
        foreach (var tag in _acquisitionService.GetAllTags())
        {
            TagList.Add(tag);
        }

        Console.WriteLine(TagList.Count);

        // 初始化模拟数据定时器（300ms 刷新一次）
        _simTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _simTimer.Tick += SimTimerOnTick;
        _simTimer.Start();
    }

    /// <summary>
    /// 模拟数据：各仪表用不同频率/幅度的正弦波 + 随机噪声
    /// </summary>
    private void SimTimerOnTick(object? sender, EventArgs e)
    {
        // 液位：20~80，慢速波动
        TankLevel = (float)(50 + 28 * Math.Sin(_simPhase * 0.7) + (_random.NextDouble() - 0.5) * 4);

        // 反应温度：150~300，中速波动
        ReactorTemp = (float)(225 + 70 * Math.Sin(_simPhase * 1.1) + (_random.NextDouble() - 0.5) * 10);

        // 反应压力：1.0~5.0，快速波动
        ReactorPressure = (float)(3.0 + 1.8 * Math.Sin(_simPhase * 1.5) + (_random.NextDouble() - 0.5) * 0.3);

        // 流量：10~50
        FlowRate = (float)(30 + 18 * Math.Sin(_simPhase * 0.9) + (_random.NextDouble() - 0.5) * 5);

        // 泵状态：每约5秒切换一次
        _pumpToggleCounter++;
        if (_pumpToggleCounter >= 16) // ~4.8s at 300ms interval
        {
            _pumpStatus = !_pumpStatus;
            _pumpToggleCounter = 0;
            RaisePropertyChanged(nameof(PumpStatusColor));
        }

        // 同步更新 TagList 中对应项的 Value（让列表也跟着动）
        foreach (var tag in TagList)
        {
            switch (tag.Id)
            {
                case "TAG001": tag.Value = TankLevel; break;
                case "TAG002": tag.Value = ReactorPressure; break;
                case "TAG003": tag.Value = ReactorTemp; break;
                case "TAG004": tag.Value = FlowRate; break;
                case "TAG005": tag.Value = _pumpStatus; break;
            }
        }

        _simPhase += 0.15;
        if (_simPhase > 2 * Math.PI) _simPhase -= 2 * Math.PI;
    }

    private void OnTagValueUpdated(object? sender, TagPoint tag)
    {
        // 更新绑定属性（注意UI 线程调度）
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            switch (tag.Id)
            {
                case "TAG001":
                    TankLevel = Convert.ToSingle(tag.Value);
                    break;
                case "TAG002":
                    ReactorPressure = Convert.ToSingle(tag.Value);
                    break;
                case "TAG003":
                    ReactorTemp = Convert.ToSingle(tag.Value);
                    break;
                case "TAG004":
                    FlowRate = Convert.ToSingle(tag.Value);
                    break;
                case "TAG005":
                    _pumpStatus = Convert.ToBoolean(tag.Value);
                    RaisePropertyChanged(nameof(PumpStatusColor));
                    break;
            }
        });
    }
    
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        Console.WriteLine("MonitorView");
        _simTimer.Start(); // 导航回来时重新启动定时器
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true; // 或者根据参数决定是否接受导航
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        // 离开页面时停止定时器，避免后台空转
        _simTimer.Stop();
    }
}