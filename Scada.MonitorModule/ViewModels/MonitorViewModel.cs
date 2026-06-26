using System.Collections.ObjectModel;
using System.Windows.Media;
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
        // 处理导航到达事件
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true; // 或者根据参数决定是否接受导航
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        // 处理导航离开事件
    }
}