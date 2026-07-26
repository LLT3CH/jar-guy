using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace HumanGlassWatcher.Voice
{
    public readonly struct RecordedAudio
    {
        public RecordedAudio(byte[] wavBytes, float durationSeconds)
        {
            WavBytes = wavBytes ?? Array.Empty<byte>();
            DurationSeconds = Mathf.Max(0f, durationSeconds);
        }

        public byte[] WavBytes { get; }
        public float DurationSeconds { get; }
    }

    public static class WavCodec
    {
        public static byte[] EncodePcm16(float[] samples, int channels, int sampleRate)
        {
            if (samples == null || samples.Length == 0)
            {
                throw new ArgumentException("Audio samples are required.", nameof(samples));
            }

            if (channels < 1 || channels > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(channels));
            }

            if (sampleRate < 8000 || sampleRate > 192000)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate));
            }

            using (var stream = new MemoryStream(44 + (samples.Length * 2)))
            using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + (samples.Length * 2));
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(samples.Length * 2);

                for (var index = 0; index < samples.Length; index++)
                {
                    var clamped = Mathf.Clamp(samples[index], -1f, 1f);
                    writer.Write((short)Mathf.RoundToInt(clamped * short.MaxValue));
                }

                writer.Flush();
                return stream.ToArray();
            }
        }

        public static bool TryReadPcm(
            byte[] bytes,
            out float[] samples,
            out int channels,
            out int sampleRate,
            out string error)
        {
            samples = Array.Empty<float>();
            channels = 0;
            sampleRate = 0;
            error = string.Empty;
            if (bytes == null || bytes.Length < 44 ||
                ReadText(bytes, 0, 4) != "RIFF" ||
                ReadText(bytes, 8, 4) != "WAVE")
            {
                error = "invalid_wav_header";
                return false;
            }

            var audioFormat = 0;
            var bitsPerSample = 0;
            var dataOffset = -1;
            var dataLength = 0;
            var offset = 12;
            while (offset + 8 <= bytes.Length)
            {
                var chunkId = ReadText(bytes, offset, 4);
                var chunkLength = ReadInt32(bytes, offset + 4);
                if (chunkLength < 0 || offset + 8 + chunkLength > bytes.Length)
                {
                    error = "invalid_wav_chunk";
                    return false;
                }

                if (chunkId == "fmt " && chunkLength >= 16)
                {
                    audioFormat = ReadUInt16(bytes, offset + 8);
                    channels = ReadUInt16(bytes, offset + 10);
                    sampleRate = ReadInt32(bytes, offset + 12);
                    bitsPerSample = ReadUInt16(bytes, offset + 22);
                }
                else if (chunkId == "data")
                {
                    dataOffset = offset + 8;
                    dataLength = chunkLength;
                    break;
                }

                offset += 8 + chunkLength + (chunkLength & 1);
            }

            if (audioFormat != 1 ||
                channels < 1 ||
                sampleRate < 8000 ||
                dataOffset < 0 ||
                (bitsPerSample != 8 && bitsPerSample != 16))
            {
                error = "unsupported_wav_format";
                return false;
            }

            var bytesPerSample = bitsPerSample / 8;
            var sampleCount = dataLength / bytesPerSample;
            if (sampleCount == 0 || sampleCount % channels != 0)
            {
                error = "invalid_wav_data";
                return false;
            }

            samples = new float[sampleCount];
            if (bitsPerSample == 16)
            {
                for (var index = 0; index < sampleCount; index++)
                {
                    var byteOffset = dataOffset + (index * 2);
                    var value = (short)(bytes[byteOffset] | (bytes[byteOffset + 1] << 8));
                    samples[index] = value / 32768f;
                }
            }
            else
            {
                for (var index = 0; index < sampleCount; index++)
                {
                    samples[index] = (bytes[dataOffset + index] - 128) / 128f;
                }
            }

            return true;
        }

        public static AudioClip CreateAudioClip(byte[] bytes, string clipName)
        {
            if (!TryReadPcm(bytes, out var samples, out var channels, out var sampleRate, out var error))
            {
                throw new FormatException($"Unable to decode WAV audio: {error}");
            }

            var frameCount = samples.Length / channels;
            var clip = AudioClip.Create(
                string.IsNullOrEmpty(clipName) ? "VoiceReply" : clipName,
                frameCount,
                channels,
                sampleRate,
                false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static string ReadText(byte[] bytes, int offset, int count)
        {
            return Encoding.ASCII.GetString(bytes, offset, count);
        }

        private static int ReadInt32(byte[] bytes, int offset)
        {
            return bytes[offset] |
                   (bytes[offset + 1] << 8) |
                   (bytes[offset + 2] << 16) |
                   (bytes[offset + 3] << 24);
        }

        private static ushort ReadUInt16(byte[] bytes, int offset)
        {
            return (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
        }
    }
}
