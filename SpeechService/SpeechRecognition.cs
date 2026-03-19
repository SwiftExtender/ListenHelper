using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using Vosk;

namespace voicio.SpeechService
{
    public class SpeechRecognition
    {
        private readonly VoskRecognizer rec;
        public string Recognize(byte[] buffer)
        {
            using (MemoryStream source = new MemoryStream(buffer))
            {
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    rec.AcceptWaveform(buffer, bytesRead);
                }
            }
            return rec.FinalResult();
        }
        public string GetRecognizeTextResult(byte[] audioData)
        {
            JObject rss = JObject.Parse(Recognize(audioData));
            return rss.Properties().Last().Value.ToString().ToLower();
        }
        public SpeechRecognition(string modelpath, float samplerate, bool wordsFlag, int maxAlternatives)
        {
            Model model = new Model(modelpath);
            rec = new VoskRecognizer(model, samplerate);
            rec.SetWords(wordsFlag); //for keyword better false
            rec.SetMaxAlternatives(maxAlternatives);
        }
    }
}
