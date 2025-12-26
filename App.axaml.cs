using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using voicio.Models;
using voicio.ViewModels;
using voicio.Views;

namespace voicio
{
    public partial class App : Application
    {
        public App()
        {
            MainGlobalView AppModelView = new MainGlobalView();
            DataContext = AppModelView;
        }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

        }
        public override void OnFrameworkInitializationCompleted()
        {
            var tempdb = new HelpContext();
            tempdb.Database.EnsureCreated(); //create DB if no DB is found
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            } 
            base.OnFrameworkInitializationCompleted();
        }
    }
}