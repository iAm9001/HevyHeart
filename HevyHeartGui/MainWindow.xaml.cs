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
        
        // Set initial password if available
        if (!string.IsNullOrEmpty(_viewModel.HevyPassword))
        {
            HevyPasswordBox.Password = _viewModel.HevyPassword;
        }
    }

    private void HevyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.HevyPassword = passwordBox.Password;
        }
    }
}
