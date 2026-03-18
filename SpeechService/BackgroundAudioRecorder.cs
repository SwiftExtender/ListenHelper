using Pv;
using System;
using System.IO;
using System.Threading;
//using Timer = System.Timers.Timer;
using Timer = System.Threading.Timer;

namespace voicio.SpeechService
{
    public class BackgroundAudioRecorder
    {
        //private CancellationTokenSource _cancellationTokenSource;
        private PvRecorder _microphone;
        private MemoryStream _customStream;
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
            _customStream = new MemoryStream();
            //{
                //Timer recordingTimer = new Timer(recordMS);
                
                _microphone.Start();
                Timer recordingTimer = new Timer(_ => StopRecord(), null, recordMS * 1000, Timeout.Infinite);
                while (_microphone.IsRecording)
                {
                    short[] frame = _microphone.Read();
                    foreach (var f in frame)
                    {
                        _customStream.Write(BitConverter.GetBytes(f));
                    }
                }
                byte[] resultArray = _customStream.ToArray();
                _customStream.Dispose();
                return resultArray;
            //}
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
