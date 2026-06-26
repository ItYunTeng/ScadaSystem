using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Scada.Application.Services;
using Scada.Core.Models;
using SkiaSharp;
using System.Collections.ObjectModel;
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
        AvailableTags = new
            ObservableCollection<TagPoint>(_acquisitionService.GetAllTags());
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
    }

    private void StartRecord()
    {
        if (_selectedTag == null || _isRecording) return;
        _isRecording = true;
        _values.Clear();
        _times.Clear();
        _acquisitionService.TagValueUpdated += OnTagValueUpdated;
    }

    private void StopRecord()
    {
        _isRecording = false;
        _acquisitionService.TagValueUpdated -= OnTagValueUpdated;
    }

    private void OnTagValueUpdated(object? sender, TagPoint tag)
    {
        if (tag.Id != _selectedTag?.Id) return;
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _values.Add(Convert.ToDouble(tag.Value));
            _times.Add(DateTime.Now);
            // 最多保留100 个点
            if (_values.Count > 100)
            {
                _values.RemoveAt(0);
                _times.RemoveAt(0);
            }

            if (Series.FirstOrDefault() is LineSeries<double> series)
            {
                series.Values = _values.ToArray();
            }
        });
    }
    
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        Console.WriteLine("TrendView");
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