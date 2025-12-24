using NAudio.Wave;
using System.IO;
using System.Threading;

namespace voicio.Models
{
    public class InteractiveAudioRecorder
    {
        private readonly WaveInEvent Microphone;
        private readonly WaveFileWriter CustomWaveProvider;
        private readonly MemoryStream CustomStream;
        private void DataAvailableEvent(object? sender, WaveInEventArgs e)
        {
            CustomWaveProvider.Write(e.Buffer, 0, e.BytesRecorded);
        }
        public void StartRecord()
        {
            Microphone.StartRecording();
        }
        public void StopRecord()
        {
            Microphone.StopRecording();
        }
        public byte[] GetByteArray()
        {
            return CustomStream.ToArray();
        }
        public float GetRecorderSampleRate()
        {
            return (float)Microphone.WaveFormat.SampleRate;
        }
        public InteractiveAudioRecorder()
        {
            Microphone = new WaveInEvent()
            {
                WaveFormat = new WaveFormat(rate: 48000, bits: 16, channels: 1),
                DeviceNumber = 0,
                BufferMilliseconds = 10000,
            };
            Microphone.DataAvailable += DataAvailableEvent;
            CustomStream = new MemoryStream();
            CustomWaveProvider = new WaveFileWriter(CustomStream, Microphone.WaveFormat) { };
        }
    }
}
