using Pv;
using System;
using System.IO;
using System.Text;
using System.Threading;
//using Timer = System.Timers.Timer;
using Timer = System.Threading.Timer;

namespace voicio.Services
{
    public class AudioRecorderService
    {
        //private CancellationTokenSource _cancellationTokenSource;
        private PvRecorder _microphone;
        private MemoryStream _customStream;
        private BinaryWriter _outputFileWriter;
        //private int _bufferSize;
        //private int _sampleRate = 16000;
        //private int _channels = 1;
        //private int _bits = 16;

        public AudioRecorderService(int deviceIndex = -1)
        {
            _microphone = PvRecorder.Create(512, deviceIndex);
        }
        private static void WriteWavHeader(BinaryWriter writer, ushort channelCount, ushort bitDepth, int sampleRate, int totalSampleCount)
        {
            if (writer == null)
                return;

            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write((bitDepth / 8 * totalSampleCount) + 36);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((ushort)1);
            writer.Write(channelCount);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channelCount * bitDepth / 8);
            writer.Write((ushort)(channelCount * bitDepth / 8));
            writer.Write(bitDepth);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(bitDepth / 8 * totalSampleCount);
        }

        public byte[] StartRecord(int recordMS, string? outputWavPath = null)
        {
            _customStream = new MemoryStream();
            int totalSamplesWritten = 0;
            if (outputWavPath != null)
            {
                _outputFileWriter = new BinaryWriter(new FileStream(outputWavPath, FileMode.OpenOrCreate, FileAccess.Write));
                WriteWavHeader(_outputFileWriter, 1, 16, _microphone.SampleRate, totalSamplesWritten);
            }

            _microphone.Start();
            Timer recordingTimer = new Timer(_ => StopRecord(), null, recordMS * 1000, Timeout.Infinite);
            while (_microphone.IsRecording)
            {
                short[] frame = _microphone.Read();
                if (_outputFileWriter != null)
                {
                    foreach (var f in frame)
                    {
                        _customStream.Write(BitConverter.GetBytes(f));
                        _outputFileWriter.Write(BitConverter.GetBytes(f));
                    }
                }
                else {
                    foreach (var f in frame)
                    {
                        _customStream.Write(BitConverter.GetBytes(f));
                    }
                }
                    
                totalSamplesWritten += frame.Length;
            }
            byte[] resultArray = _customStream.ToArray();
            _customStream.Dispose();
            if (_outputFileWriter != null)
            {
                WriteWavHeader(_outputFileWriter, 1, 16, _microphone.SampleRate, totalSamplesWritten);
                _outputFileWriter.Flush();
                _outputFileWriter.Dispose();
            }
            
            return resultArray;
        }
        public void StopRecord()
        {
            _microphone.Stop();
            //_microphone.Dispose();
        }

        public byte[] GetByteArray()
        {
            return _customStream.ToArray();
        }
    }
}
