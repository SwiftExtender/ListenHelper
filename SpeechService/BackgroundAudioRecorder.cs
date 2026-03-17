using OpenTK.Audio.OpenAL;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace voicio.SpeechService
{
    public class BackgroundAudioRecorder
    {
        private CancellationTokenSource _cancellationTokenSource;
        private ALCaptureDevice _microphone;
        private MemoryStream _customStream;
        private int _bufferSize;
        private int _sampleRate = 16000;
        private int _channels = 1;
        private int _bits = 16;

        public BackgroundAudioRecorder() {
            _microphone = ALC.CaptureOpenDevice(null, _sampleRate, ALFormat.Mono16, _bufferSize);
        }
        public byte[] StartRecord(int recordSeconds) {
            _bufferSize = _sampleRate * _channels * _bits / 8 * recordSeconds;
            ALC.CaptureStart(_microphone);
            Stopwatch stopwatch = Stopwatch.StartNew();
            byte[] buffer = new byte[_bufferSize];
            int totalSamples = 0;
            while (stopwatch.Elapsed.Seconds < recordSeconds)
            {

                //int samples = ALC.GetInteger(_microphone, AlcGetInteger.CaptureSamples);

            }

            ALC.CaptureStop(_microphone);
            ALC.CaptureCloseDevice(_microphone);
            return buffer;
        }
        public bool StopRecord()
        {
            ALC.CaptureStop(_microphone);
            return ALC.CaptureCloseDevice(_microphone);
        }
        public void InitWordError()
        {
            
            //var audioFile = new AudioFileReader(signalFolderPath + "initworderror.mp3") { ReadTimeout = 5 };
              
            
        }
        public void SecondWordError()
        {
            
        }
        public byte[] GetByteArray()
        {
            return _customStream.ToArray();
        }
        public float GetRecorderSampleRate()
        {
            //return _microphone.
            return 0.1f;
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
