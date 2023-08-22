using Azure.AI.Language.QuestionAnswering;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace QnAapp
{
    class Program
    {
        private static SpeechConfig speechConfig;
        static async Task Main(string[] args)
        {
            // Configuring Language Service credentials
            Uri endpoint = new Uri("https://langservicewesteuqnaapp.cognitiveservices.azure.com/");
            Azure.AzureKeyCredential credential = new Azure.AzureKeyCredential("73997db905ec4e1a8f7b8ee43d03f574");
            string projectName = "QnA-CustomerService";
            string deploymentName = "production";
            QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

            try 
            {
                // Config Cognitive Service credentials
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string cogSvcKey = configuration["CognitiveServiceKey"];
                string cogSvcRegion = configuration["CognitiveServiceRegion"];

                // Configure speech service
                speechConfig = SpeechConfig.FromSubscription(cogSvcKey, cogSvcRegion);

                // Configure voice
                speechConfig.SpeechSynthesisVoiceName = "en-US-AriaNeural";
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            System.Console.WriteLine("Welcome to Customer Support. All of our operatives are currently busy.");
            System.Console.WriteLine("There are 3245 callers ahead of you. Would you like to speak to our support bot instead?");
            System.Console.WriteLine("yes/no");
            string choice = Console.ReadLine();
            if (choice == "yes") 
            {
                System.Console.WriteLine("Please wait...");
            }
            else 
            {
                System.Console.WriteLine("You don't have a choice :)");
            }

            string keepGoing = "";
            
            do
            {
                string question = await SpeechToText();
                Azure.Response<AnswersResult> response = client.GetAnswers(question, project);

                foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
                {
                    System.Console.WriteLine(answer.Answer);
                    await TextToSpeech(answer.Answer);
                }

                System.Console.WriteLine("Press 1 to Continue");
                System.Console.WriteLine("Press 2 to Exit");
                keepGoing = Console.ReadLine();
            }
            while(keepGoing != "2");

            await TextToSpeech("Goodbye");
            System.Console.WriteLine("Goodbye!");
        }
        static async Task<string> SpeechToText()
        {
            string command = "";
            
            // Configure speech recognition
            using AudioConfig audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
            Console.WriteLine("Speak clearly into the mic...");

            // Process speech input
            SpeechRecognitionResult speech = await speechRecognizer.RecognizeOnceAsync();
            if (speech.Reason == ResultReason.RecognizedSpeech)
            {
                command = speech.Text;
                Console.WriteLine(command);
            }
            else
            {
                Console.WriteLine(speech.Reason);
                if (speech.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(speech);
                    Console.WriteLine(cancellation.Reason);
                    Console.WriteLine(cancellation.ErrorDetails);
                }
            }

            // Return the command
            return command;
        }
        static async Task TextToSpeech(string answer)
        {
            string response = answer;
                        
            // Configure speech synthesis
            speechConfig.SpeechSynthesisVoiceName = "en-IE-EmilyNeural"; //en-GB-LibbyNeural
            using SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig);

            // Synthesize spoken output
            SpeechSynthesisResult speak = await speechSynthesizer.SpeakTextAsync(response);
            if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
            {
                Console.WriteLine(speak.Reason);
            }
        }
    }
}
