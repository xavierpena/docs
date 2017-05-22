# SpeechSynthesizer in c#

Ass seen in ["How a blind developer uses Visual Studio 2017 to code and debug"](https://www.reddit.com/r/programming/comments/6bc1s7/how_a_blind_developer_uses_visual_studio_2017_to/).


## Code example

	using System.Linq;
	using System.Speech.Synthesis;

	namespace SpeechTest
	{
		class Program
		{
			static void Main(string[] args)
			{
				var speechSynthesizer = new SpeechSynthesizer();

				var voices = speechSynthesizer.GetInstalledVoices();
				var selectedVoice = voices
					.Where(x => x.VoiceInfo.Culture.Name.Contains("en"))
					.Single();
				speechSynthesizer.SelectVoice(selectedVoice.VoiceInfo.Name);

				speechSynthesizer.Speak("Hello world");
			}
		}
	}


## Problems

I haven't been able to find an equivalent to make it work in dotnet core.

Available voices have to be installed in the Windows OS. Installing new ones seem to be hard ([link](http://www.zero2000.com/free-text-to-speech-natural-voices.html)).
