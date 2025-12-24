using NAudio.Wave;
using System;
using System.Threading;
using System.Threading.Tasks;
using voicio.Models;

namespace voicio.Services
{
    public class VoiceService : IDisposable
    {
        //public VoiceService()
        //{

        //}
        //public async void StartService()
        //{

        //}
        //public async void StopService()
        //{

        //}
        private WasapiLoopbackCapture _capture;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly int _sampleRate = 16000;
        private readonly int _chunkDurationMs = 1000;

        public event EventHandler WakeWordDetected;

        public void Start()
        {
            _capture = new WasapiLoopbackCapture();
            _capture.WaveFormat = new WaveFormat(_sampleRate, 16, 1);

            _capture.DataAvailable += OnAudioDataAvailable;
            _cancellationTokenSource = new CancellationTokenSource();

            _capture.StartRecording();
        }

        private async void OnAudioDataAvailable(object sender, WaveInEventArgs e)
        {
            // Process audio in chunks
            var audioData = new float[e.BytesRecorded / 2];

            // Convert byte[] to float[]
            for (int i = 0; i < audioData.Length; i++)
            {
                audioData[i] = BitConverter.ToInt16(e.Buffer, i * 2) / 32768f;
            }

            // Run wake word detection (using ONNX model or custom algorithm)
            if (await DetectWakeWord(audioData))
            {
                WakeWordDetected?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task<bool> DetectWakeWord(float[] audioData)
        {
            // Implement your wake word detection logic
            // Could use:
            // 1. Pre-trained ONNX model (like Porcupine, Snowboy alternatives)
            // 2. Custom MFCC + ML approach
            // 3. Simple audio pattern matching

            return false;
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _capture?.StopRecording();
            _capture?.Dispose();
        }
    }
}

    


