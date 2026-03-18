using Pv;
using System;
using System.IO;
using Timer = System.Timers.Timer;

namespace voicio.SpeechService
{
    public class BackgroundAudioRecorder
    {
        //private CancellationTokenSource _cancellationTokenSource;
        private PvRecorder _microphone;
        private MemoryStream _customStream = new MemoryStream();
        //private int _bufferSize;
        //private int _sampleRate = 16000;
        //private int _channels = 1;
        //private int _bits = 16;

        public BackgroundAudioRecorder()
        {
            _microphone = PvRecorder.Create(512);
        }
        public byte[] StartRecord(int recordMS)
        {
            Timer recordingTimer = new Timer(recordMS);
            recordingTimer.Elapsed += (s, e) => _microphone.Dispose();
            _microphone.Start();
            while (_microphone.IsRecording)
            {
                short[] frame = _microphone.Read();
                foreach (var f in frame)
                {
                    _customStream.Write(BitConverter.GetBytes(f));
                }
            }
            return _customStream.ToArray();
        }
        public void StopRecord()
        {
            _microphone.Dispose();
        }

        public byte[] GetByteArray()
        {
            return _customStream.ToArray();
        }
    }
}
