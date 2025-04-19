# 🎙️ MP4 Transcriber

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![FFmpeg](https://img.shields.io/badge/FFmpeg-Automatic-green)](https://ffmpeg.org/)
[![Azure Speech](https://img.shields.io/badge/Azure%20Cognitive%20Services-Speech-blue)](https://learn.microsoft.com/azure/cognitive-services/speech-service/)
![dotnet](https://github.com/aherrick/AzureAISearchVectorSemantic/actions/workflows/dotnet.yml/badge.svg)

This console app takes an `.mp4` video file, extracts the audio using **FFmpeg**, and transcribes the speech to text using **Azure Cognitive Services Speech API**.

---

## 🔧 Requirements

- An Azure Speech resource (with API key and region)
- An `.mp4` file (e.g., `test.mp4`) in the same directory
- No FFmpeg installation needed — the app downloads it automatically

---

## 🗝️ User Secrets Configuration

Before running the app, set up user secrets with the following structure:

```json
{
  "Azure": {
    "Speech": {
      "ApiKey": "your-api-key-here",
      "Region": "your-region-here"
    }
  }
}
```

---

## 📁 Output

- `test.wav` – the extracted audio file
- `test.txt` – the transcribed speech text
