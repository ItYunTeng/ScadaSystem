using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Scada.Application.Services;
using Scada.Core.Models;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Scada.TrendModule.ViewModels;

public class TrendViewModel : BindableBase, INavigationAware
{
    private readonly IDataAcquisitionService _acquisitionService;
    private bool _isRecording;
    private TagPoint? _selectedTag;
    private readonly List<double> _values = new();
    private readonly List<DateTime> _times = new();
    
    // 模拟数据相关
    private readonly DispatcherTimer _simTimer;
    private double _simPhase;           // 正弦波相位
    private readonly Random _random = new();
    
    public ObservableCollection<TagPoint> AvailableTags { get; }
    public ObservableCollection<ISeries> Series { get; } = new();

    public TagPoint? SelectedTag
    {
        get => _selectedTag;
        set => SetProperty(ref _selectedTag, value);
    }

    public DelegateCommand StartRecordCommand { get; }
    public DelegateCommand StopRecordCommand { get; }

    public TrendViewModel(IDataAcquisitionService acquisitionService)
    {
        _acquisitionService = acquisitionService;
        AvailableTags = new ObservableCollection<TagPoint>(_acquisitionService.GetAllTags());
        StartRecordCommand = new DelegateCommand(StartRecord);
        StopRecordCommand = new DelegateCommand(StopRecord);
        
        // 初始化空曲线
        Series.Add(new LineSeries<double>
        {
            Name = "实时值",
            Fill = null,
            Stroke = new SolidColorPaint(SKColors.CornflowerBlue, 2),
            GeometrySize = 0
        });
        
        // 初始化模拟数据定时器（200ms 刷新一次）
        _simTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _simTimer.Tick += SimTimerOnTick;
    }

    private void StartRecord()
    {
        if (_isRecording) return;
        _isRecording = true;
        _values.Clear();
        _times.Clear();
        _simPhase = 0;
        
        // 优先订阅真实数据
        _acquisitionService.TagValueUpdated += OnTagValueUpdated;
        // 同时启动模拟数据（无真实数据时也能看到曲线滚动）
        _simTimer.Start();
    }

    private void StopRecord()
    {
        _isRecording = false;
        _simTimer.Stop();
        _acquisitionService.TagValueUpdated -= OnTagValueUpdated;
    }

    /// <summary>
    /// 模拟数据：正弦波 + 随机噪声，循环滚动
    /// </summary>
    private void SimTimerOnTick(object? sender, EventArgs e)
    {
        // 生成模拟值：基础正弦波(0~80) + 小幅随机噪声
        double baseValue = 50 + 30 * Math.Sin(_simPhase);
        double noise = (_random.NextDouble() - 0.5) * 6;
        double simulatedValue = baseValue + noise;
        
        PushValue(simulatedValue);
        
        // 相位递增，形成连续波形
        _simPhase += 0.15;
        if (_simPhase > 2 * Math.PI) _simPhase -= 2 * Math.PI;
    }

    private void OnTagValueUpdated(object? sender, TagPoint tag)
    {
        if (tag.Id != _selectedTag?.Id) return;
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            PushValue(Convert.ToDouble(tag.Value));
        });
    }

    /// <summary>
    /// 向曲线追加一个数据点，保留最近 100 个点形成滚动窗口
    /// </summary>
    private void PushValue(double value)
    {
        _values.Add(value);
        _times.Add(DateTime.Now);
        
        if (_values.Count > 100)
        {
            _values.RemoveAt(0);
            _times.RemoveAt(0);
        }

        if (Series.FirstOrDefault() is LineSeries<double> series)
        {
            series.Values = _values.ToArray();
        }
    }
    
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        Console.WriteLine("TrendView");
        // 导航回来时自动启动模拟数据
        if (!_isRecording) StartRecord();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        // 离开页面时停止，避免后台空转
        if (_isRecording) StopRecord();
    }
}