using Ownaudio.Core;
using OwnaudioNET;
using OwnaudioNET.Mixing;
using OwnaudioNET.Sources;
using System.Linq;

namespace voicio.SpeechService
{
    public class SignalAudioPlayer
    {
        public AudioMixer _mixer;
        public AudioConfig _config;
        public SignalAudioPlayer()
        {
            AudioConfig _config = new AudioConfig()
            {
                SampleRate = 48000,
                Channels = 2,
                BufferSize = 512,
                HostType = EngineHostType.None
            };

            // Initialize via OwnaudioNet (uses AudioEngineFactory internally)
            // This will try NativeAudioEngine first, then fallback to platform-specific engines
            OwnaudioNet.Initialize(_config);
            var outputDevices = OwnaudioNet.Engine?.UnderlyingEngine.GetOutputDevices();
            if (outputDevices != null && outputDevices.Count > 0)
            {
                // Find the current device (either by OutputDeviceId or the default device)
                AudioDeviceInfo? currentDevice = null;

                if (!string.IsNullOrEmpty(_config.OutputDeviceId))
                {
                    currentDevice = outputDevices.FirstOrDefault(d => d.DeviceId == _config.OutputDeviceId);
                }
                else
                {
                    currentDevice = outputDevices.FirstOrDefault(d => d.IsDefault);
                }
            }
            OwnaudioNet.Start();
            var Engine = OwnaudioNet.Engine!.UnderlyingEngine;

            _mixer = new AudioMixer(Engine, bufferSizeInFrames: 512);
        }
        public void Play() {
            FileSource f = new FileSource("D:\\auxil\\TaskListener\\TaskListener\\Assets\\Signals\\initworderror.mp3");
            _mixer.AddSource(f);
            f.Play();
        }

    }
}
