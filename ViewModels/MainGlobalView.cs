using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.IO;
using System.Linq;
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

        //init word
        public const string VoiceSearchWord = "search";
        public const string VoiceExecuteWord = "execute";
        public const string SetSearchTypeWord = "type";
        //search types
        public const string FuzzySearchWord = "fuzzy";
        public const string StrictSearchWord = "strict";

        private string _searchType = "strict";
        //sound signal folder
        private string signalFolderPath = AppContext.BaseDirectory + "Assets" + Path.DirectorySeparatorChar + "Signals" + Path.DirectorySeparatorChar;
        private CancellationTokenSource? _cts;
        public Task? ListenTask;
        public ReactiveCommand<Unit, Unit> ShowVoiceSettingsCommand { get; }
        public ReactiveCommand<Unit,Unit> OpenMainWindow { get; }
        public ReactiveCommand<Unit, Unit> QuitAppCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowTagsWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowImportWindowCommand { get; }
        public string GetRecognizeTextResult(byte[] audioData, bool wordsFlag, int maxAlternatives)
        {
            string model_path = AppContext.BaseDirectory + "voice_model";
            var recognition = new SpeechRecognition(model_path, 16000, wordsFlag, maxAlternatives);
            JObject rss = JObject.Parse(recognition.Recognize(audioData));
            return rss.Properties().Last().Value.ToString().ToLower();
        }
        public void StartListenService(CancellationToken token)
        {
            BackgroundAudioRecorder rec = new() {};
            SignalAudioPlayer player = new();
            
            //SpeechRecognition recognition = new SpeechRecognition(model_path, GetRecorderSampleRate(), wordsFlag, maxAlternatives);
            try
            {
                byte[] audio = rec.StartRecord(3);
                while (!token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    //keyword processing
                    switch (GetRecognizeTextResult(audio, false, 0))
                    {
                        case VoiceSearchWord:
                            rec.StartRecord(2);
                            string secondWordForSearch = GetRecognizeTextResult(audio, false, 0);
                            var redirectSearchWindow = new VoiceActionWindow() { DataContext = new VoiceActionWindowViewModel(false, secondWordForSearch) };
                            redirectSearchWindow.Show();
                            break;
                        case VoiceExecuteWord:
                            rec.StartRecord(2);
                            string secondWordForExecute = GetRecognizeTextResult(audio, false, 0);
                            var redirectExecuteWindow = new VoiceActionWindow() { DataContext = new VoiceActionWindowViewModel(true, secondWordForExecute) };
                            redirectExecuteWindow.Show();
                            break;
                        case SetSearchTypeWord:
                            rec.StartRecord(2);
                            string secondWordForType = GetRecognizeTextResult(audio, false, 0);
                            if (secondWordForType == FuzzySearchWord || secondWordForType == StrictSearchWord)
                            {
                                _searchType = secondWordForType;
                                var redirectSetSearchTypeWindow = new SetSearchTypeWindow() { DataContext = new SetSearchTypeWindowViewModel(secondWordForType) };
                                redirectSetSearchTypeWindow.Show();
                            }
                            else
                            {
                                player.Play();
                            }
                            break;
                        default:
                            player.Play();
                            break;
                    }
                }
            }
            finally
            {
                player.Play();
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
