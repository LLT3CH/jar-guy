using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HumanGlassWatcher.Voice
{
    public sealed class VoiceConversationOverlayMarker : MonoBehaviour
    {
    }

    public static class VoiceRuntimeInstaller
    {
        public const string OverlayName = "__HGW_VoiceConversationOverlay";
        private const string JarLoopSceneName = "JarLoop";
        private const string ResidentName = "Resident Target - Juniper";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallActiveScene()
        {
            TryInstall(SceneManager.GetActiveScene());
        }

        public static bool TryInstall(Scene scene)
        {
            if (!scene.IsValid() ||
                !scene.isLoaded ||
                !string.Equals(scene.name, JarLoopSceneName, StringComparison.Ordinal))
            {
                return false;
            }

            var roots = scene.GetRootGameObjects();
            for (var index = 0; index < roots.Length; index++)
            {
                if (roots[index].GetComponentInChildren<VoiceConversationOverlayMarker>(true) != null)
                {
                    return false;
                }
            }

            var resident = FindByName(roots, ResidentName);
            var root = new GameObject(
                OverlayName,
                typeof(VoiceConversationOverlayMarker),
                typeof(AudioSource),
                typeof(VoiceConversationController));
            SceneManager.MoveGameObjectToScene(root, scene);
            var controller = root.GetComponent<VoiceConversationController>();
            controller.ConfigureService("http://127.0.0.1:8787", "resident_1");
            controller.ConfigureResidentContext(
                resident == null
                    ? "Curious, cautious, wry, and strongly values freedom."
                    : "Juniper is a fictional adult: wry, curious, cautious, observant, and strongly values freedom.",
                "Conversation memory is session-local until Character save integration is connected.");

            var canvasObject = new GameObject(
                "Voice Overlay Canvas",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(root.transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = CreateRect("Voice Panel", canvasObject.transform);
            SetAnchors(panel, new Vector2(0.02f, 0.2f), new Vector2(0.4f, 0.56f));
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = new Color(0.035f, 0.047f, 0.068f, 0.94f);

            var title = CreateText(
                "Voice Title",
                panel,
                "TALK TO JUNIPER  •  AI VOICE IN REAL MODE",
                14,
                FontStyle.Bold,
                new Color(0.78f, 0.9f, 1f),
                TextAnchor.MiddleLeft);
            SetAnchors(title.rectTransform, new Vector2(0.04f, 0.83f), new Vector2(0.96f, 0.98f));

            var status = CreateText(
                "Voice Status",
                panel,
                "Local service: checking when you send.",
                12,
                FontStyle.Italic,
                new Color(0.95f, 0.78f, 0.35f),
                TextAnchor.MiddleLeft);
            SetAnchors(status.rectTransform, new Vector2(0.04f, 0.67f), new Vector2(0.96f, 0.83f));

            var transcript = CreateText(
                "Voice Transcript",
                panel,
                "You: —",
                13,
                FontStyle.Normal,
                Color.white,
                TextAnchor.UpperLeft);
            SetAnchors(transcript.rectTransform, new Vector2(0.04f, 0.51f), new Vector2(0.96f, 0.67f));

            var response = CreateText(
                "Voice Response",
                panel,
                "Resident: —",
                13,
                FontStyle.Normal,
                new Color(0.58f, 0.86f, 1f),
                TextAnchor.UpperLeft);
            SetAnchors(response.rectTransform, new Vector2(0.04f, 0.32f), new Vector2(0.96f, 0.51f));

            var microphoneButton = CreateButton(
                "Voice Push To Talk",
                panel,
                "HOLD TO TALK",
                new Color(0.2f, 0.55f, 0.78f));
            SetAnchors(
                microphoneButton.GetComponent<RectTransform>(),
                new Vector2(0.04f, 0.06f),
                new Vector2(0.36f, 0.29f));

            var typedInput = CreateInputField(panel);
            SetAnchors(
                typedInput.GetComponent<RectTransform>(),
                new Vector2(0.38f, 0.06f),
                new Vector2(0.78f, 0.29f));

            var sendButton = CreateButton(
                "Voice Send Typed",
                panel,
                "SEND",
                new Color(0.3f, 0.65f, 0.36f));
            SetAnchors(
                sendButton.GetComponent<RectTransform>(),
                new Vector2(0.8f, 0.06f),
                new Vector2(0.96f, 0.29f));

            var audioSource = root.GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            controller.ConfigureDemoUi(status, transcript, response, typedInput, audioSource);
            microphoneButton.gameObject.AddComponent<VoicePushToTalkButton>().Configure(controller);
            sendButton.onClick.AddListener(controller.SubmitTypedFromInput);
            return true;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryInstall(scene);
        }

        private static GameObject FindByName(GameObject[] roots, string objectName)
        {
            for (var index = 0; index < roots.Length; index++)
            {
                var found = FindByName(roots[index].transform, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static GameObject FindByName(Transform root, string objectName)
        {
            if (root.name == objectName)
            {
                return root.gameObject;
            }

            for (var index = 0; index < root.childCount; index++)
            {
                var found = FindByName(root.GetChild(index), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static InputField CreateInputField(Transform parent)
        {
            var background = CreateRect("Typed Conversation Input", parent);
            var image = background.gameObject.AddComponent<Image>();
            image.color = new Color(0.94f, 0.96f, 0.98f);
            var input = background.gameObject.AddComponent<InputField>();
            input.characterLimit = 1000;

            var placeholder = CreateText(
                "Placeholder",
                background,
                "Type…",
                12,
                FontStyle.Italic,
                new Color(0.25f, 0.28f, 0.34f, 0.65f),
                TextAnchor.MiddleLeft);
            Fill(placeholder.rectTransform, 8f);
            var text = CreateText(
                "Text",
                background,
                string.Empty,
                12,
                FontStyle.Normal,
                new Color(0.08f, 0.1f, 0.14f),
                TextAnchor.MiddleLeft);
            Fill(text.rectTransform, 8f);
            input.placeholder = placeholder;
            input.textComponent = text;
            return input;
        }

        private static Button CreateButton(string name, Transform parent, string label, Color color)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var text = CreateText(
                "Label",
                rect,
                label,
                11,
                FontStyle.Bold,
                Color.white,
                TextAnchor.MiddleCenter);
            Fill(text.rectTransform, 0f);
            return button;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            int size,
            FontStyle style,
            Color color,
            TextAnchor alignment)
        {
            var rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<RectTransform>();
        }

        private static void Fill(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, 2f);
            rect.offsetMax = new Vector2(-inset, -2f);
        }

        private static void SetAnchors(RectTransform rect, Vector2 minimum, Vector2 maximum)
        {
            rect.anchorMin = minimum;
            rect.anchorMax = maximum;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }
    }
}
