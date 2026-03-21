using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using voicio.Services;
using voicio.Views;

namespace voicio.ViewModels
{
    public class MainGlobalViewModel : ViewModelBase
    {
        public SearchService SearchService { get; set; }

        public string[] garbageWords = ["the"];
        //init word
        public const string VoiceSearchWord = "find";
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
            AudioRecorderService recorder = new(0) { };
            AudioPlayerService player = new();
            string model_path = AppContext.BaseDirectory + "voice_model";
            signalFolderPath = Path.Join(AppContext.BaseDirectory, "assets", "signals");
            string firstWordErrorSignal = Path.Join(signalFolderPath, "initworderror.wav");
            string secondWordErrorSignal = Path.Join(signalFolderPath, "initworderror.wav");
            string fatalErrorSignal = Path.Join(signalFolderPath, "warning.wav");
            SpeechRecognitionService recognition = new SpeechRecognitionService(model_path, 16000, false, 0);
            try
            {
                while (!token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    //keyword processing
                    byte[] audio = recorder.StartRecord(3);//, "temp" + DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + ".wav"
                    string word = recognition.GetRecognizeTextResult(audio);
                    switch (word)
                    {
                        case VoiceSearchWord:
                            
                            var redirectSearchWindow = new VoiceDialogWindow() { DataContext = new VoiceDialogWindowViewModel(false, recorder, recognition, SearchService) };
                            redirectSearchWindow.Show();
                            break;
                        case VoiceExecuteWord:
                            var redirectExecuteWindow = new VoiceDialogWindow() { DataContext = new VoiceDialogWindowViewModel(false, recorder, recognition, SearchService) };
                            redirectExecuteWindow.Show();
                            break;
                        case SetSearchTypeWord:
                            var settingsWindow = new SetSearchTypeWindow() { DataContext = new SetSearchTypeWindowViewModel(recorder, recognition) };
                            settingsWindow.Show(); 
                            //audio = recorder.StartRecord(2);
                            //string secondWordForType = recognition.GetRecognizeTextResult(audio);
                            //if (secondWordForType == FuzzySearchWord || secondWordForType == StrictSearchWord)
                            //{
                            //    _searchType = secondWordForType;
                            //    var redirectSetSearchTypeWindow = new SetSearchTypeWindow() { DataContext = new SetSearchTypeWindowViewModel(secondWordForType) };
                            //    redirectSetSearchTypeWindow.Show();
                            //}
                            //else
                            //{
                                player.Play(secondWordErrorSignal);
                            //}
                            break;
                        default:
                            if (!garbageWords.Contains(word)) {
                                player.Play(firstWordErrorSignal);
                            }
                            break;
                    }
                }
            }
            finally
            {
                player.Play(fatalErrorSignal);
            }

        }
        public MainGlobalViewModel()
        {
            SearchService = new();
            OpenMainWindow = ReactiveCommand.Create(() =>
            {
                var w1 = new MainWindow() { DataContext = new MainWindowViewModel(SearchService) };
                w1.Show();
            });
            ShowVoiceSettingsCommand = ReactiveCommand.Create(() =>
            {
                var w2 = new ScriptWindow() { DataContext = new ScriptWindowViewModel() };
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
