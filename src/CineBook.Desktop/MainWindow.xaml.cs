using System.Windows;
using CineBook.Desktop.ViewModels;

namespace CineBook.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // The window's DataContext is the "shell" view model that controls navigation.
        DataContext = new MainViewModel();
    }
}
