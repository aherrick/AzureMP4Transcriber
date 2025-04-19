using System.Text;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

// Setup FFmpeg
var ffmpegDirectory = Path.Combine(
    Environment.GetEnvironmentVariable("HOME") ?? Directory.GetCurrentDirectory(),
    "ffmpeg",
    "bin"
);
Directory.CreateDirectory(ffmpegDirectory); // Ensure the directory exists

// Log the established directory
Console.WriteLine($"FFmpeg directory established at: {ffmpegDirectory}");

// Check if binaries exist in D:\home
string ffmpegPath = Path.Combine(ffmpegDirectory, "ffmpeg.exe");
string ffprobePath = Path.Combine(ffmpegDirectory, "ffprobe.exe");

if (!File.Exists(ffmpegPath) || !File.Exists(ffprobePath))
{
    Console.WriteLine("FFmpeg binaries not found... Downloading...");

    try
    {
        // Download the latest FFmpeg binaries to D:\home\ffmpeg\bin
        await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegDirectory);
        Console.WriteLine("FFmpeg binaries downloaded successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to download FFmpeg binaries: {ex.Message}");
        throw; // Handle or rethrow as needed
    }
}
else
{
    Console.WriteLine("FFmpeg binaries already exist");
}

// Set FFmpeg path
FFmpeg.SetExecutablesPath(ffmpegDirectory);

// convert mp4 to wav

var mp4File = new FileInfo("test.mp4");

var mp4Path = mp4File.FullName;
var mp4Bytes = await File.ReadAllBytesAsync(mp4Path);

// Use same directory and base name for .wav file
var mp4Wav = Path.Combine(
    mp4File.DirectoryName,
    Path.GetFileNameWithoutExtension(mp4File.Name) + ".wav"
);

await FFmpeg.Conversions.New().AddParameter($"-i \"{mp4Path}\"").SetOutput(mp4Wav).Start();

var transcription = new StringBuilder();

var speechConfig = SpeechConfig.FromSubscription(
    config["Azure:Speech:ApiKey"],
    config["Azure:Speech:Region"]
);

speechConfig.SpeechRecognitionLanguage = "en-US";

var stopRecognition = new TaskCompletionSource<int>();

using var audioInput = AudioConfig.FromWavFileInput(mp4Wav);
using var recognizer = new SpeechRecognizer(speechConfig, audioInput);
recognizer.Recognized += (s, e) =>
{
    transcription.AppendLine(e.Result.Text);
    Console.WriteLine(e.Result.Text); // Print recognized text as it comes in
};

recognizer.SessionStopped += (s, e) =>
{
    stopRecognition.TrySetResult(0);
};

// Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

Task.WaitAny([stopRecognition.Task]);

// Stops recognition.
await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

// === Write to .txt ===
var transcriptFile = Path.Combine(
    mp4File.DirectoryName,
    Path.GetFileNameWithoutExtension(mp4File.Name) + ".txt"
);
await File.WriteAllTextAsync(transcriptFile, transcription.ToString());

Console.WriteLine($"Transcription saved to: {transcriptFile}");