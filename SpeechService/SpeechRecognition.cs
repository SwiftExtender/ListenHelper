using System.IO;
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
        public SpeechRecognition(string modelpath, float samplerate, bool wordsFlag, int maxAlternatives) {
            Model model = new Model(modelpath);
            rec = new VoskRecognizer(model, samplerate);
            rec.SetWords(wordsFlag); //for keyword better false
            rec.SetMaxAlternatives(maxAlternatives);
        }
    }
}
