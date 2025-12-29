using Newtonsoft.Json.Linq;
using OpenTK.Audio.OpenAL;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using voicio.ViewModels;
using voicio.Views;
using Timer = System.Timers.Timer;

namespace voicio.SpeechService
{
    public class BackgroundAudioRecorder
    {
        //init word
        private const string VoiceSearchWord = "search";
        private const string VoiceExecuteWord = "execute";
        private const string SetSearchTypeWord = "type";
        //search types
        private const string FuzzySearchWord = "fuzzy";
        private const string StrictSearchWord = "strict";

        private CancellationTokenSource _cancellationTokenSource;
        private readonly ALCaptureDevice Microphone;
        private readonly int _chunkDurationMs = 1000;
        private Timer recordingTimer;
        private MemoryStream CustomStream;

        private string _searchType = "strict";
        private string signalFolderPath = AppContext.BaseDirectory + "Assets" + Path.DirectorySeparatorChar + "Signals" + Path.DirectorySeparatorChar;
        public BackgroundAudioRecorder(int sampleRate) {
            Microphone = ALC.CaptureOpenDevice(null, sampleRate, ALFormat.Mono16, bufferSize / (channels * bits / 8));
            //Microphone.DataAvailable += DataAvailableEvent;

        }
        public void StartRecord(int ms) {
            //CustomStream = new MemoryStream();
            //CustomWaveProvider = new WaveFileWriter(CustomStream, Microphone.WaveFormat) { };
            //Microphone.StartRecording();
            //recordingTimer = new Timer(ms);
            //recordingTimer.Elapsed += (s, e) => Microphone.StopRecording();
        }
        public string Recognizing(bool wordsFlag, int maxAlternatives)
        {
            var audioData = GetByteArray();
            string model_path = AppContext.BaseDirectory + "voice_model";
            var recognition = new SpeechRecognition(model_path, GetRecorderSampleRate(), wordsFlag, maxAlternatives);
            JObject rss = JObject.Parse(recognition.Recognize(audioData));
            return rss.Properties().Last().Value.ToString().ToLower();
        }

        public void InitWordError()
        {
            
            //var audioFile = new AudioFileReader(signalFolderPath + "initworderror.mp3") { ReadTimeout = 5 };
              
            
        }
        public void SecondWordError()
        {
            
        }
        public void RecordLoopForAssistantCall(CancellationToken token)
        {
            try
            {
                StartRecord(3000);
                while (!token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    //keyword processing
                    switch (Recognizing(false, 0))
                    {
                        case VoiceSearchWord:
                            StartRecord(2000);
                            string secondWordForSearch = Recognizing(false, 0);
                            var redirectSearchWindow = new VoiceActionWindow() { DataContext = new VoiceActionWindowViewModel(false, secondWordForSearch) };
                            redirectSearchWindow.Show();
                            break;
                        case VoiceExecuteWord:
                            StartRecord(2000);
                            string secondWordForExecute = Recognizing(false, 0);
                            var redirectExecuteWindow = new VoiceActionWindow() { DataContext = new VoiceActionWindowViewModel(true, secondWordForExecute) };
                            redirectExecuteWindow.Show();
                            break;
                        case SetSearchTypeWord:
                            StartRecord(2000);
                            string secondWordForType = Recognizing(false, 0);
                            if (secondWordForType == FuzzySearchWord || secondWordForType == StrictSearchWord)
                            {
                                _searchType = secondWordForType;
                                var redirectSetSearchTypeWindow = new SetSearchTypeWindow() { DataContext = new SetSearchTypeWindowViewModel(secondWordForType) };
                                redirectSetSearchTypeWindow.Show();
                            }
                            else {
                                SecondWordError();
                            }
                            break;
                        default:
                            InitWordError();
                            break;
                    }     
                }
            }
            finally
            {
                Dispose();
            }
        }
        public byte[] GetByteArray()
        {
            return CustomStream.ToArray();
        }
        public float GetRecorderSampleRate()
        {
            return 0.1f;
            //return Microphone.WaveFormat.SampleRate;
        }
        //private async void DataAvailableEvent(object sender, WaveInEventArgs e)
        //{
        //    //CustomWaveProvider.Write(e.Buffer, 0, e.BytesRecorded);
        //}
        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            //Microphone?.StopRecording();
            //Microphone?.Dispose();
        }
    }
}
