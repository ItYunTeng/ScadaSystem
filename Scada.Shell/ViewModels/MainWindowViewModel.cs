using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Scada.Shell.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly IRegionManager _regionManager;
    private string _statusMessage = "系统运行正常";
    private DateTime _currentTime;

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public DateTime CurrentTime
    {
        get => _currentTime;
        set => SetProperty(ref _currentTime, value);
    }

    public DelegateCommand<string> NavigateCommand { get; private set; }

    public MainWindowViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
        NavigateCommand = new DelegateCommand<string>(Navigate);
        // 时钟更新
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        timer.Tick += (_, _) => CurrentTime = DateTime.Now;
        timer.Start();
    }

    private void Navigate(string viewName)
    {
        _regionManager.RequestNavigate("MainRegion", viewName);
        StatusMessage = $"当前模块: {viewName}";
    }
}