using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using voicio.SpeechService;
using voicio.Views;

namespace voicio.ViewModels
{
    public class MainGlobalView: ViewModelBase
    {
        //private ImportWindow importWindow = null;
        //private TagWindow tagWindow = null;
        private CancellationTokenSource? _cts;
        public Task? ListenTask;
        public ReactiveCommand<Unit, Unit> ShowVoiceSettingsCommand { get; }
        public ReactiveCommand<Unit,Unit> OpenMainWindow { get; }
        public ReactiveCommand<Unit, Unit> QuitAppCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowTagsWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowImportWindowCommand { get; }
        public void StartListenService(CancellationToken token)
        {
            try
            {
                BackgroundAudioRecorder rec = new BackgroundAudioRecorder();
                while (!token.IsCancellationRequested)
                {
                    rec.RecordLoopForAssistantCall(token);
                    //Dispatcher.UIThread.Post(() => { /* update property */ });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //return false;
            }

        }
        public MainGlobalView() {
            OpenMainWindow = ReactiveCommand.Create(() => {
                var w1 = new MainWindow() { DataContext = new MainWindowViewModel() };
                w1.Show();
            });
            ShowVoiceSettingsCommand = ReactiveCommand.Create(() => {
                var w2 = new VoiceSettingWindow() { DataContext = new VoiceSettingWindowViewModel(_cts) };
                w2.Show();
            });
            ShowImportWindowCommand = ReactiveCommand.Create(() => {
                var w3 = new ImportWindow() { DataContext = new ImportWindowViewModel() };
                w3.Show();
            });
            ShowTagsWindowCommand = ReactiveCommand.Create(() => {
                var w4 = new TagWindow() { DataContext = new TagWindowViewModel() };
                w4.Show();
            });
            QuitAppCommand = ReactiveCommand.Create(() => {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Shutdown();
                }
            });
            _cts = new CancellationTokenSource();
            ListenTask = Task.Run(() => StartListenService(_cts.Token), _cts.Token);
            
        }
    }
}
