using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Gouter.Extensions;
using Gouter.Services;
using Gouter.ViewModels;
using Gouter.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Gouter;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; }

    private static App? _instance;
    public static App Instance => (_instance ??= Current as App)
        ?? throw new InvalidOperationException();

    /// <summary>アセンブリ情報</summary>
    private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    public App() : base()
    {
        this.ServiceProvider = new ServiceCollection()
            // 共通処理の登録
            .RegisterSharedComponents()
            // add Services
            // add ViewModels
            .AddTransient<MainWindowViewModel>()
            .AddTransient<PreferenceWindowViewModel>()
           // To ServiceProvider
           .BuildServiceProvider();
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var serviceProvider = this.ServiceProvider;
        serviceProvider.GetRequiredService<SettingsService>().Load().AsTask().Wait();
    }

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        var serviceProvider = this.ServiceProvider;

        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
