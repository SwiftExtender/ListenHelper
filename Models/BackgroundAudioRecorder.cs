using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace voicio.Models
{
    public class BackgroundAudioRecorder
    {
        private WasapiLoopbackCapture _capture;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly WaveInEvent Microphone;
        //private readonly int _sampleRate = 16000;
        //private readonly int _chunkDurationMs = 1000;
        private WaveFileWriter CustomWaveProvider;
        private MemoryStream CustomStream;

        public event EventHandler WakeWordDetected;

        public BackgroundAudioRecorder() {

            Microphone = new WaveInEvent()
            {
                WaveFormat = new WaveFormat(rate: 48000, bits: 16, channels: 1),
                DeviceNumber = 0,
                BufferMilliseconds = 5000,
            };
            Microphone.DataAvailable += DataAvailableEvent;
            
        }
        public void RecordLoop(CancellationToken token)
        {
            try
            {
                CustomStream = new MemoryStream();
                CustomWaveProvider = new WaveFileWriter(CustomStream, Microphone.WaveFormat) { };
                Microphone.StartRecording();
                Thread.Sleep(7 * 1000);
                Microphone.StopRecording();
                // Poll token periodically (NAudio doesn't auto-check it)
                while (!token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(50);
                    var audioData = GetByteArray();
                    string model_path = AppContext.BaseDirectory + "voice_model";
                    var recognition = new SpeechRecognition(model_path, GetRecorderSampleRate());
                    JObject rss = JObject.Parse(recognition.Recognize(audioData));
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
            return (float)Microphone.WaveFormat.SampleRate;
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
