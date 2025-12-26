using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace voicio.Models
{
    public class BackgroundAudioRecorder
    {
        private WasapiLoopbackCapture _capture;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly int _sampleRate = 16000;
        private readonly int _chunkDurationMs = 1000;

        public event EventHandler WakeWordDetected;

        public void RecordLoop(CancellationToken token)
        {
            try
            {
                _capture = new WasapiLoopbackCapture();
                _capture.DataAvailable += OnAudioDataAvailable;
                //_capture.RecordingStopped += (s, a) => { /* Handle stop */ };
                _capture.StartRecording();

                // Poll token periodically (NAudio doesn't auto-check it)
                while (!token.IsCancellationRequested && _capture?.CaptureState == NAudio.CoreAudioApi.CaptureState.Capturing)
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(50); // Low CPU poll
                }
            }
            finally
            {
                Dispose();
            }
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
