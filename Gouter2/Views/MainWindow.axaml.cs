using Avalonia.Controls;
using Avalonia.Interactivity;
using Gouter.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Gouter.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var window = new PreferenceWindow()
        {
            DataContext = ((App)App.Current!).ServiceProvider.GetService<PreferenceWindowViewModel>(),
        };
        window.ShowDialog(this);
    }
}
