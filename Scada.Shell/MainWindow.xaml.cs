using System.Windows;
using Scada.Shell.ViewModels;

namespace Scada.Shell;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel; // 【关键】把 ViewModel 赋值给界面
    }
}