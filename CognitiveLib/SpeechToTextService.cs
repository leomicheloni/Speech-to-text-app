namespace CognitiveLib
{
    using Microsoft.CognitiveServices.SpeechRecognition;
    using System;
    using System.IO;
    using System.Text;

    public class Transcription
    {
        public Guid ID { get; set; }
        public string Text { get; set; }

        private Transcription()
        {

        }
        public static Transcription New(string message)
        {
            return new Transcription { ID = Guid.NewGuid(), Text = message };
        }
    }

    public class SpeechToTextService
    {
        DataRecognitionClient dataClient;
        private string path = "";

        public SpeechRecognitionMode Mode { get; private set; } = SpeechRecognitionMode.LongDictation;
        public string DefaultLocale { get; private set; } = "es-ES";
        public string SubscriptionKey { get; private set; }
        public string AuthenticationUri { get; private set; } = "https://api.cognitive.microsoft.com/sts/v1.0";
        public Transcription LastMessage { get; set; }
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
            }
        }

        public string FileName
        {
            get
            {
                return System.IO.Path.Combine(this.path, "out.txt");
            }
           
        }

        public event Action<Transcription> NewMesage;


        public SpeechToTextService(string suscriptionKey)
        {
            this.SubscriptionKey = suscriptionKey;
        }

        private void CreateDataRecoClient()
        {
            this.dataClient = SpeechRecognitionServiceFactory.CreateDataClient(
                this.Mode,
                this.DefaultLocale,
                this.SubscriptionKey);
            this.dataClient.AuthenticationUri = "";

            this.dataClient.OnResponseReceived += this.OnDataDictationResponseReceivedHandler;

            this.dataClient.OnPartialResponseReceived += this.OnPartialResponseReceivedHandler;
            this.dataClient.OnConversationError += this.OnConversationErrorHandler;
        }

        private void OnDataDictationResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            if (e.PhraseResponse.RecognitionStatus == RecognitionStatus.EndOfDictation ||
                e.PhraseResponse.RecognitionStatus == RecognitionStatus.DictationEndSilenceTimeout)
            {
                // The end
                this.WriteLine("End...");
            }

            this.WriteResponseResult(e);
        }

        private void WriteResponseResult(SpeechResponseEventArgs e)
        {
            var fullMessage = new StringBuilder();

            if (e.PhraseResponse.Results.Length == 0)
            {
                this.WriteLine($"No phrase response is available. {e.PhraseResponse.RecognitionStatus}");
            }
            else
            {

                foreach (var result in e.PhraseResponse.Results)
                {
                    fullMessage.AppendLine(result.DisplayText.Replace("?", string.Empty).Replace("¿", string.Empty));
                    //this.WriteLine(result.DisplayText.Replace("?", string.Empty));
                    //this.WriteLine($"Confidence {result.Confidence}");
                    this.WriteLine(fullMessage.ToString());
                }
            }
        }

        private void WriteLine(string displayText)
        {
            Console.WriteLine(displayText);
            try
            {
                File.AppendAllText(this.FileName, displayText);
                this.LastMessage = Transcription.New(File.ReadAllText(this.FileName));
            }
            catch (Exception)
            {


            }

            this.NewMesage?.Invoke(this.LastMessage);
        }

        private void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
        }

        private void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            WriteLine($"ERROR: {e.SpeechErrorText}");
        }

        private void SendAudioHelper(string wavFileName)
        {
            using (var fileStream = new FileStream(wavFileName, FileMode.Open, FileAccess.Read))
            {
                // Note for wave files, we can just send data from the file right to the server.
                // In the case you are not an audio file in wave format, and instead you have just
                // raw data (for example audio coming over bluetooth), then before sending up any 
                // audio data, you must first send up an SpeechAudioFormat descriptor to describe 
                // the layout and format of your raw audio data via DataRecognitionClient's sendAudioFormat() method.
                int bytesRead = 0;
                byte[] buffer = new byte[1024];

                try
                {
                    do
                    {
                        // Get more Audio data to send into byte buffer.
                        bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                        // Send of audio data to service. 
                        this.dataClient.SendAudio(buffer, bytesRead);
                    }
                    while (bytesRead > 0);
                }
                finally
                {
                    // We are done sending audio.  Final recognition results will arrive in OnResponseReceived event call.
                    this.dataClient.EndAudio();
                }
            }
        }

        public void Start(string audiofileName, string outputPath)
        {
            this.Path = outputPath;
            File.Delete(this.FileName);
            this.CreateDataRecoClient();
            this.SendAudioHelper(audiofileName);
        }
    }
}
