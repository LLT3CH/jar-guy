# Voice conversation slice

This package adds a service-backed push-to-talk and typed conversation loop without changing Gameplay or Character-owned source.

## Test in JarLoop

1. Start the local service:

   ```powershell
   Set-Location services/game-brain
   npm.cmd start
   ```

2. Open or run the existing `JarLoop` scene.
3. The runtime installer finds the scene and creates a compact lower-left `TALK TO JUNIPER` panel once.
4. Hold `HOLD TO TALK`, speak, and release; or type a message and select `SEND`.

The default service URL is `http://127.0.0.1:8787`. With the default mock provider, no key or network is needed. The microphone audio is still captured and uploaded, but mock transcription is deterministic and the reply sound is an audio cue, not synthesized speech. The status line labels that behavior explicitly.

The overlay is restricted to the `JarLoop` scene, uses a marker for idempotent installation, and sits below the lid gesture area. It submits one legal client action (`speak_reply`) and rejects any service result that does not exactly match the offered action, intent, and resident target. It publishes a validated event but does not execute Gameplay actions.

## Standalone demo

Use `Human Glass Watcher > Voice > Create Voice Conversation Demo` to create or refresh:

`Assets/_Project/Voice/Scenes/VoiceConversationDemo.unity`

The scene exercises the same controller without requiring JarLoop.

## Real provider mode

Unity configuration does not change. Set `GAME_BRAIN_PROVIDER=openai` and `OPENAI_API_KEY` in the Node service environment as documented in `services/game-brain/README.md`, then restart that service. Never place provider keys in a Unity scene, scriptable object, player setting, APK, desktop build, or client request.

Real mode sends bounded WAV capture to server-side transcription, creates a schema-constrained conversation reply with session memory and personality context, and requests server-configured synthesized speech. The UI discloses that real-mode voice is AI-generated.

## Platform notes

- Windows/editor: allow microphone access when Unity asks.
- Android: Unity requests microphone permission at the moment push-to-talk begins. Permission denial leaves typed conversation available.
- An APK cannot reach a desktop service through its own `127.0.0.1`. For device testing, configure the controller to use an authenticated, reachable development endpoint (for example a trusted HTTPS tunnel or LAN reverse proxy). Android cleartext-network policy may reject an unencrypted LAN URL.
- A stopped or unreachable service produces a clear local fallback. Jar physics and typed gameplay continue, but there is no local speech recognizer or synthesizer.
- Capture is PCM WAV, 16 kHz, mono, and bounded to 15 seconds by the controller (30 seconds is the hard recorder maximum).

## Automated verification

The Voice edit-mode assembly covers WAV round-trip, contract validation, bounded memory/personality context, malformed speech, and idempotent JarLoop installation. Run it from a Unity 6000.5.5f1 batch process:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.5.5f1\Editor\Unity.exe' `
  -batchmode -nographics `
  -projectPath (Get-Location) `
  -runTests -testPlatform EditMode `
  -testFilter HumanGlassWatcher.Voice.Tests `
  -testResults Logs/voice-tests.xml `
  -logFile Logs/voice-tests.log `
  -quit
```
