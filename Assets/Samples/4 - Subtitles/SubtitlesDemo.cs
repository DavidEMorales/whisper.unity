using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;

namespace Whisper.Samples
{
    /// <summary>
    /// Shows transcription in a subtitles style (synced with audio).
    /// For each word show confidence level coded by color.
    /// </summary>
    public class SubtitlesDemo : MonoBehaviour
    {
        public static int MIN_CENSORED_WORD_LENGTH = 6;
        public static SubtitlesDemo Instance;

        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public AudioClip noiseAudio;

        public Text outputText;
        
        public List<string> BannedWords;
        int offsetSample = 0;

        AudioChunk recordedAudio;
        string previousSpeech;

        private void Awake()
        {
            Instance = this;
            BannedWords = new List<string>();

            // we need to force this settings for whisper
            whisper.enableTokens = true;
            whisper.tokensTimestamps = true;
            whisper.useVad = false;
            
            microphoneRecord.OnRecordStop += OnRecordStop;
        }
        
        public void OnButtonPressed()
        {
            if (!microphoneRecord.IsRecording)
            {
                previousSpeech = null;
                microphoneRecord.StartRecord();
            }
            else
            {
                microphoneRecord.StopRecord();
            }
        }

        private void OnRecordStop(AudioChunk newRecordedAudio)
        {
            recordedAudio = newRecordedAudio;
        }

        public async Task ProcessAudio()
        {
            for (int sampleIndex = 0; sampleIndex < recordedAudio.Data.Length; sampleIndex++)
            {
                if (recordedAudio.Data[sampleIndex] > 0.35f)
                {
                    offsetSample = sampleIndex;
                    sampleIndex = recordedAudio.Data.Length;
                }
            }

            var newRecordedAudio = new AudioChunk()
            {
                Data = new float[recordedAudio.Data.Length - offsetSample],
                Frequency = recordedAudio.Frequency,
                Channels = recordedAudio.Channels,
                Length = recordedAudio.Length - (offsetSample / recordedAudio.Frequency),
                IsVoiceDetected = recordedAudio.IsVoiceDetected
            };


            for (int sampleIndex = 0; sampleIndex < newRecordedAudio.Data.Length; sampleIndex++)
            {
                newRecordedAudio.Data[sampleIndex] = recordedAudio.Data[sampleIndex + offsetSample];
            }
            
            recordedAudio = newRecordedAudio;

            AudioClip voiceClip = AudioClip.Create(
                "Speech",
                recordedAudio.Data.Length,
                recordedAudio.Channels,
                recordedAudio.Frequency,
                false);
            voiceClip.SetData(recordedAudio.Data, 0);

            var res = await whisper.GetTextAsync(voiceClip);

            var noiseSamples = new float[noiseAudio.samples];
            noiseAudio.GetData(noiseSamples, 0);

            foreach (var segment in res.Segments)
            {
                foreach (var token in segment.Tokens)
                {
                    var fullWord = new string(token.Text.Trim().ToLower().Where(c => !char.IsPunctuation(c)).ToArray());
                    var endsInS = fullWord.Length > 3 && fullWord.Substring(fullWord.Length - 1) == "s";

                    UnityEngine.Debug.Log(fullWord);
                    if (BannedWords.Contains(fullWord) || (endsInS && BannedWords.Contains(fullWord.Substring(0, fullWord.Length - 1))))
                    {
                        UnityEngine.Debug.Log("You said " + fullWord + " at " + token.Timestamp.Start.TotalSeconds + " until " + token.Timestamp.End.TotalSeconds);

                        var startingSample = (int)(token.Timestamp.Start.TotalSeconds * recordedAudio.Frequency * recordedAudio.Channels);
                        var endingSample = (int)(token.Timestamp.End.TotalSeconds * recordedAudio.Frequency * recordedAudio.Channels);
                        
                        for (int sampleIndex = startingSample; sampleIndex < endingSample; sampleIndex++)
                        {
                            recordedAudio.Data[sampleIndex] = noiseSamples[sampleIndex + offsetSample];
                        }
                    }
                }

                for (int tokenIndex = 0; tokenIndex < segment.Tokens.Length - 1; tokenIndex++)
                {
                    var text = segment.Tokens[tokenIndex].Text + segment.Tokens[tokenIndex + 1].Text;
                    var fullWord = new string(text.Trim().ToLower().Where(c => !char.IsPunctuation(c)).ToArray());
                    var endsInS = fullWord.Length > 3 && fullWord.Substring(fullWord.Length - 1) == "s";

                    UnityEngine.Debug.Log(fullWord);
                    if (BannedWords.Contains(fullWord) || (endsInS && BannedWords.Contains(fullWord.Substring(0, fullWord.Length - 1))))
                    {
                        var start = segment.Tokens[tokenIndex].Timestamp.Start;
                        var end = segment.Tokens[tokenIndex + 1].Timestamp.End;

                        UnityEngine.Debug.Log("You said " + fullWord + " at " + start.TotalSeconds + " until " + end.TotalSeconds);

                        var startingSample = (int)(start.TotalSeconds * recordedAudio.Frequency * recordedAudio.Channels);
                        var endingSample = (int)(end.TotalSeconds * recordedAudio.Frequency * recordedAudio.Channels);

                        for (int sampleIndex = startingSample; sampleIndex < endingSample; sampleIndex++)
                        {
                            recordedAudio.Data[sampleIndex + offsetSample] = noiseSamples[sampleIndex + offsetSample];
                        }
                    }
                }
            }

            // start playing sound
            var go = new GameObject("Audio Echo");
            var source = go.AddComponent<AudioSource>();
            voiceClip.SetData(recordedAudio.Data, 0);
            source.clip = voiceClip;
            source.Play();
            outputText = GameObject.Find("Output Text").GetComponent<Text>();
            outputText.text = string.Empty;

            // and show subtitles at the same time
            while (source.isPlaying)
            {
                var subs = GetSubtitles(res, source.time);
                /*while (subs.Length > 40)
                {
                    subs = subs.Substring(40).Substring(subs.IndexOf(' ') + 1);
                }*/
                outputText.text = subs;
                await Task.Yield();

                // check that audio source still here and wasn't destroyed
                if (!source)
                    return;
            }

            previousSpeech = outputText.text;
            Destroy(go);
        }

        // TODO: this isn't optimized and for demo use only
        private string GetSubtitles(WhisperResult res, float timeSec)
        {
            var sb = new StringBuilder();
            var time = TimeSpan.FromSeconds(timeSec);
            foreach (var seg in res.Segments)
            {
                // check if we already passed whole segment
                if (time >= seg.End)
                {
                    sb.Append(SegmentToText(seg));
                    continue;
                }

                foreach (var token in seg.Tokens)
                {
                    if (time > token.Timestamp.Start)
                    {
                        var text = token.Text;
                        sb.Append(text);
                    }
                }
            }

            return sb.ToString();
        }

        private static string SegmentToText(WhisperSegment segment)
        {
            var sb = new StringBuilder();
            foreach (var token in segment.Tokens)
            {
                var tokenText = token.Text;
                sb.Append(tokenText);
            }

            return sb.ToString();
        }

        public int GetPreviousTotalWordCount()
        {
            var previousString = new string(previousSpeech.Trim().ToLower().Where(c => !char.IsPunctuation(c)).ToArray());
            var previousWords = previousString.Split(" ");
            return previousWords.Count();
        }

        public int GetPreviousCensoredWordCount()
        {
            var previousString = new string(previousSpeech.Trim().ToLower().Where(c => !char.IsPunctuation(c)).ToArray());
            var previousWords = previousString.Split(" ");
            return previousWords.Where(previousWord => BannedWords.Contains(previousWord)).Count();
        }

        public List<string> BanSomeWords(int newBannedWordsCount)
        {
            var previousStringChars = previousSpeech.Trim().ToLower().ToCharArray();
            var newString = "";
            var adding = true;
            for (int i = 0; i < previousStringChars.Length; i++)
            {
                if (previousStringChars[i] == '[')
                {
                    adding = false;
                }
                else if (i > 1 && previousStringChars[i - 1] == ']')
                {
                    adding = true;
                }

                if (adding)
                {
                    newString += previousStringChars[i];
                }
            }

            var previousString = new string(newString.Where(c => !char.IsPunctuation(c)).ToArray());
            var previousWords = previousString.Split(" ");

            var previousWordsByCountDict = new Dictionary<string, int>();

            foreach (var word in previousWords)
            {
                if (previousWordsByCountDict.ContainsKey(word))
                {
                    previousWordsByCountDict[word] = previousWordsByCountDict[word] + 1;
                }
                else
                {
                    previousWordsByCountDict[word] = 1;
                }
            }

            var previousWordsOrderedByCount = previousWordsByCountDict.Keys.OrderByDescending(word => previousWordsByCountDict[word]).Where(word => word.Length >= MIN_CENSORED_WORD_LENGTH).ToList();

            var newBannedWords = new List<string>();
            while (previousWordsOrderedByCount.Count > 0 && newBannedWords.Count < newBannedWordsCount)
            {
                if (!BannedWords.Contains(previousWordsOrderedByCount[0]))
                {
                    BannedWords.Add(previousWordsOrderedByCount[0]);
                    newBannedWords.Add(previousWordsOrderedByCount[0]);
                }

                previousWordsOrderedByCount.RemoveAt(0);
            }

            return newBannedWords;
        }
    }
}