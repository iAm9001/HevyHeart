using System.Windows;
using System.Windows.Controls;
using HevyHeartGui.ViewModels;

namespace HevyHeartGui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        // No password boxes to pre-fill; token fields bind directly via XAML
    }

    // HevyPasswordBox_PasswordChanged removed — tokens now use plain TextBox bindings
}
