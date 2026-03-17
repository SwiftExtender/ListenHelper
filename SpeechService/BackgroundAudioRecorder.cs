using Newtonsoft.Json.Linq;
using OpenTK.Audio.OpenAL;
using System;
using System.Diagnostics;
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
        private readonly ALCaptureDevice _microphone;
        private readonly int _chunkDurationMs = 1000;
        private Timer _recordingTimer;
        private MemoryStream _customStream;
        private int _bufferSize;
        private int _sampleRate = 16000;
        private int _channels = 1;
        private int _bits = 16;

        private string _searchType = "strict";
        private string signalFolderPath = AppContext.BaseDirectory + "Assets" + Path.DirectorySeparatorChar + "Signals" + Path.DirectorySeparatorChar;
        public BackgroundAudioRecorder(int sampleRate) {
            _sampleRate = sampleRate;
            _microphone = ALC.CaptureOpenDevice(null, sampleRate, ALFormat.Mono16, _bufferSize);
        }
        public void StartRecord(int recordMS) {
            _bufferSize = _sampleRate * _channels * _bits / 8 * 3;
            ALC.CaptureStart(_microphone);
        }
        public void RecordToWav()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            byte [] buffer = new byte[_bufferSize];
            int totalSamples = 0;
            while (stopwatch.Elapsed.Seconds < 3) {
                //ALDevice device = (ALDevice)_microphone;
                //int samples = ALC.GetInteger(_microphone, AlcGetInteger.CaptureSamples);

            }
            ALC.CaptureStop(_microphone);
            ALC.CaptureCloseDevice(_microphone);
        }
        public void StopRecord()
        {
            ALC.CaptureStop(_microphone);
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
            return _customStream.ToArray();
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
