using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using voicio.SpeechService;
using voicio.Views;

namespace voicio.ViewModels
{

    public class MainGlobalView : ViewModelBase
    {
        //private ImportWindow importWindow = null;
        //private TagWindow tagWindow = null;

        //init word
        public const string VoiceSearchWord = "search";
        public const string VoiceExecuteWord = "execute";
        public const string SetSearchTypeWord = "type";
        //search types
        public const string FuzzySearchWord = "fuzzy";
        public const string StrictSearchWord = "strict";

        private string _searchType = "strict";
        //sound signal folder
        private string signalFolderPath;// = AppContext.BaseDirectory + "Assets" + Path.DirectorySeparatorChar + "Signals" + Path.DirectorySeparatorChar;
        private CancellationTokenSource? _cts;
        public Task? ListenTask;
        public ReactiveCommand<Unit, Unit> ShowVoiceSettingsCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenMainWindow { get; }
        public ReactiveCommand<Unit, Unit> QuitAppCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowTagsWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowImportWindowCommand { get; }
        public void StartListenService(CancellationToken token, int deviceIndex = -1)
        {
            BackgroundAudioRecorder recorder = new() { };
            SignalAudioPlayer player = new();
            string model_path = AppContext.BaseDirectory + "voice_model";
            signalFolderPath = Path.Join(AppContext.BaseDirectory, "assets", "signals");
            string firstWordErrorSignal = Path.Join(signalFolderPath, "initworderror.wav");
            string secondWordErrorSignal = Path.Join(signalFolderPath, "initworderror.wav");
            string fatalErrorSignal = Path.Join(signalFolderPath, "warning.wav");
            SpeechRecognition recognition = new SpeechRecognition(model_path, 16000, false, 0);
            //SpeechRecognition recognition = new SpeechRecognition(model_path, GetRecorderSampleRate(), wordsFlag, maxAlternatives);
            try
            {
                while (!token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    //keyword processing
                    byte[] audio = recorder.StartRecord(3, "temp" + DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + ".wav");
                    string word = recognition.GetRecognizeTextResult(audio);
                    switch (word)
                    {
                        case VoiceSearchWord:
                            audio = recorder.StartRecord(2);
                            string secondWordForSearch = recognition.GetRecognizeTextResult(audio);
                            var redirectSearchWindow = new VoiceActionWindow() { DataContext = new VoiceActionWindowViewModel(false, secondWordForSearch) };
                            redirectSearchWindow.Show();
                            break;
                        case VoiceExecuteWord:
                            audio = recorder.StartRecord(2);
                            string secondWordForExecute = recognition.GetRecognizeTextResult(audio);
                            var redirectExecuteWindow = new VoiceActionWindow() { DataContext = new VoiceActionWindowViewModel(true, secondWordForExecute) };
                            redirectExecuteWindow.Show();
                            break;
                        case SetSearchTypeWord:
                            audio = recorder.StartRecord(2);
                            string secondWordForType = recognition.GetRecognizeTextResult(audio);
                            if (secondWordForType == FuzzySearchWord || secondWordForType == StrictSearchWord)
                            {
                                _searchType = secondWordForType;
                                var redirectSetSearchTypeWindow = new SetSearchTypeWindow() { DataContext = new SetSearchTypeWindowViewModel(secondWordForType) };
                                redirectSetSearchTypeWindow.Show();
                            }
                            else
                            {
                                player.Play(secondWordErrorSignal);
                            }
                            break;
                        default:
                            player.Play(firstWordErrorSignal);
                            break;
                    }
                }
            }
            finally
            {
                player.Play(fatalErrorSignal);
            }

        }
        public MainGlobalView()
        {
            OpenMainWindow = ReactiveCommand.Create(() =>
            {
                var w1 = new MainWindow() { DataContext = new MainWindowViewModel() };
                w1.Show();
            });
            ShowVoiceSettingsCommand = ReactiveCommand.Create(() =>
            {
                var w2 = new VoiceSettingWindow() { DataContext = new VoiceSettingWindowViewModel(_cts) };
                w2.Show();
            });
            ShowImportWindowCommand = ReactiveCommand.Create(() =>
            {
                var w3 = new ImportWindow() { DataContext = new ImportWindowViewModel() };
                w3.Show();
            });
            ShowTagsWindowCommand = ReactiveCommand.Create(() =>
            {
                var w4 = new TagWindow() { DataContext = new TagWindowViewModel() };
                w4.Show();
            });
            QuitAppCommand = ReactiveCommand.Create(() =>
            {
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
