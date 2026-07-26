using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HumanGlassWatcher.Voice.Tests
{
    public sealed class VoiceContractAndAudioTests
    {
        [Test]
        public void WavCodec_RoundTripsPcm16Audio()
        {
            var input = new float[320];
            for (var index = 0; index < input.Length; index++)
            {
                input[index] = (float)Math.Sin(index * 0.1) * 0.25f;
            }

            var bytes = WavCodec.EncodePcm16(input, 1, 16000);

            Assert.That(WavCodec.TryReadPcm(
                bytes,
                out var output,
                out var channels,
                out var sampleRate,
                out var error), Is.True, error);
            Assert.That(channels, Is.EqualTo(1));
            Assert.That(sampleRate, Is.EqualTo(16000));
            Assert.That(output.Length, Is.EqualTo(input.Length));
            Assert.That(output[100], Is.EqualTo(input[100]).Within(0.001f));
        }

        [Test]
        public void DialogueValidator_AcceptsOnlyExactOfferedIntentAndTargets()
        {
            var request = Request();
            var valid = new DialogueTurnDto
            {
                contractVersion = 1,
                turnId = request.turnId,
                spokenLine = "I heard you.",
                emotion = "curiosity",
                intensity = 0.5f,
                selectedActionId = "speak_reply",
                selectedIntent = "speak",
                targetEntityIds = new[] { "resident_1" },
                memoryNote = "The player said hello."
            };

            Assert.That(VoiceContractValidator.IsValidDialogue(valid, request, out var error), Is.True, error);

            valid.targetEntityIds = new[] { "missing_entity" };
            Assert.That(VoiceContractValidator.IsValidDialogue(valid, request, out error), Is.False);
            Assert.That(error, Is.EqualTo("dialogue_offer_mismatch"));
        }

        [Test]
        public void ConversationMemory_BoundsTurnsAndCarriesPersonalityAndMemory()
        {
            var memory = new VoiceConversationMemory("The player brought water.");
            for (var index = 0; index < 10; index++)
            {
                memory.RecordTurn(
                    $"Player line {index}",
                    $"Resident line {index}",
                    $"Memory {index}.");
            }

            var snapshot = memory.Snapshot("resident_1", "Wry and cautious.");
            Assert.That(snapshot.recentTurns.Length, Is.EqualTo(12));
            Assert.That(snapshot.personality, Is.EqualTo("Wry and cautious."));
            Assert.That(snapshot.memorySummary, Does.Contain("Memory 9."));
            Assert.That(memory.BuildStateDigest(snapshot.personality).Length, Is.LessThanOrEqualTo(2000));
        }

        [Test]
        public void SpeechValidator_RejectsNonWavOrMalformedAudio()
        {
            var validBytes = WavCodec.EncodePcm16(new float[160], 1, 16000);
            var result = new VoiceSynthesisResultDto
            {
                contractVersion = 1,
                audioBase64 = Convert.ToBase64String(validBytes),
                mimeType = "audio/wav",
                provider = "mock"
            };

            Assert.That(VoiceContractValidator.IsValidSpeech(result, out var error), Is.True, error);

            result.audioBase64 = "bm90IGEgd2F2";
            Assert.That(VoiceContractValidator.IsValidSpeech(result, out error), Is.False);
            Assert.That(error, Is.EqualTo("invalid_speech_audio"));
        }

        [Test]
        public void RuntimeInstaller_InstallsIntoJarLoopExactlyOnce()
        {
            var scene = EditorSceneManager.OpenScene(
                "Assets/_Project/Gameplay/Scenes/JarLoop.unity",
                OpenSceneMode.Additive);
            var resident = new GameObject("Resident Target - Juniper");
            SceneManager.MoveGameObjectToScene(resident, scene);
            try
            {
                Assert.That(VoiceRuntimeInstaller.TryInstall(scene), Is.True);
                Assert.That(VoiceRuntimeInstaller.TryInstall(scene), Is.False);
                Assert.That(
                    scene.GetRootGameObjects().Count(
                        value => value.name == VoiceRuntimeInstaller.OverlayName),
                    Is.EqualTo(1));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static DialogueRequestDto Request()
        {
            return new DialogueRequestDto
            {
                turnId = "voice_turn_1",
                playerMessage = "Hello.",
                residentState = "Listening.",
                knownEntityIds = new[] { "resident_1" },
                legalActions = new[]
                {
                    new ActionOfferDto
                    {
                        actionId = "speak_reply",
                        verb = "speak",
                        targetEntityIds = new[] { "resident_1" },
                        utilityHint = 50f,
                        reasonCode = "conversation_reply"
                    }
                }
            };
        }
    }
}
