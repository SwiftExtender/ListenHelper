using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using voicio.ViewModels;
using voicio.Views;
using Timer = System.Timers.Timer;

namespace voicio.SpeechService
{
    public class BackgroundAudioRecorder
    {
        private WasapiLoopbackCapture _capture;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly WaveInEvent Microphone;
        //private readonly int _sampleRate = 16000;
        //private readonly int _chunkDurationMs = 1000;
        private Timer recordingTimer;
        private WaveFileWriter CustomWaveProvider;
        private MemoryStream CustomStream;

        public event EventHandler WakeWordDetected;

        public BackgroundAudioRecorder() {

            Microphone = new WaveInEvent()
            {
                WaveFormat = new WaveFormat(rate: 48000, bits: 16, channels: 1),
                DeviceNumber = 0,
                BufferMilliseconds = 3500,
            };
            Microphone.DataAvailable += DataAvailableEvent;
            
        }
        public void RecordLoopForAssistantCall(CancellationToken token)
        {
            try
            {
                CustomStream = new MemoryStream();
                CustomWaveProvider = new WaveFileWriter(CustomStream, Microphone.WaveFormat) { };
                Microphone.StartRecording();
                recordingTimer = new Timer(3000);
                recordingTimer.Elapsed += (s, e) => Microphone.StopRecording();
                while (!token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    var audioData = GetByteArray();
                    string model_path = AppContext.BaseDirectory + "voice_model";
                    var recognition = new SpeechRecognition(model_path, GetRecorderSampleRate(), false, 0);
                    JObject rss = JObject.Parse(recognition.Recognize(audioData));
                    if (rss.Properties().Last().Value.ToString().ToLower() == "search") {
                        var redirectWindow = new VoiceActionWindow() { DataContext = new VoiceActionViewModel() };
                        redirectWindow.Show();
                    }
                    Console.WriteLine(rss.ToString());
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
            return Microphone.WaveFormat.SampleRate;
        }
        private async void DataAvailableEvent(object sender, WaveInEventArgs e)
        {
            CustomWaveProvider.Write(e.Buffer, 0, e.BytesRecorded);
        }
        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _capture?.StopRecording();
            _capture?.Dispose();
        }
    }
}
