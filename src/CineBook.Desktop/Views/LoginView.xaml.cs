using System.Windows;
using System.Windows.Controls;
using CineBook.Desktop.ViewModels;

namespace CineBook.Desktop.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    // PasswordBox.Password can't be bound in XAML, so we hand it to the view model here.
    // This is the one small piece of code-behind in the project, and it's purely about the UI.
    private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
        {
            viewModel.Password = PasswordInput.Password;
        }
    }
}
