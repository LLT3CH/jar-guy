using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HumanGlassWatcher.Voice.Editor
{
    public static class VoiceConversationDemoBuilder
    {
        public const string ScenePath = "Assets/_Project/Voice/Scenes/VoiceConversationDemo.unity";

        [MenuItem("Human Glass Watcher/Voice/Create Voice Conversation Demo")]
        public static void CreateFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            BuildAndSave();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        public static void BuildForAutomation()
        {
            BuildAndSave();
        }

        private static void BuildAndSave()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            var inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();

            var canvasObject = new GameObject(
                "Voice Conversation Canvas",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = CreateRect("Panel", canvasObject.transform);
            panel.anchorMin = new Vector2(0.08f, 0.1f);
            panel.anchorMax = new Vector2(0.92f, 0.9f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = new Color(0.08f, 0.1f, 0.14f, 0.96f);

            var title = CreateText(
                "Title",
                panel,
                "Human Glass Watcher — Voice Conversation",
                44,
                TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, 0.05f, 0.88f, 0.95f, 0.98f);
            title.color = new Color(0.9f, 0.94f, 1f);

            var disclosure = CreateText(
                "Disclosure",
                panel,
                "Real-provider mode uses an AI-generated resident voice. Mock mode uses an audio cue.",
                23,
                TextAnchor.MiddleCenter);
            SetRect(disclosure.rectTransform, 0.08f, 0.81f, 0.92f, 0.88f);
            disclosure.color = new Color(0.95f, 0.78f, 0.35f);

            var status = CreateText(
                "Status",
                panel,
                "Ready.",
                27,
                TextAnchor.MiddleCenter);
            SetRect(status.rectTransform, 0.08f, 0.71f, 0.92f, 0.8f);

            var transcript = CreateText(
                "Transcript",
                panel,
                "You: —",
                30,
                TextAnchor.UpperLeft);
            SetRect(transcript.rectTransform, 0.08f, 0.57f, 0.92f, 0.7f);

            var response = CreateText(
                "Resident Response",
                panel,
                "Resident: —",
                32,
                TextAnchor.UpperLeft);
            SetRect(response.rectTransform, 0.08f, 0.4f, 0.92f, 0.56f);
            response.color = new Color(0.58f, 0.86f, 1f);

            var microphoneButton = CreateButton(
                "Push To Talk",
                panel,
                "HOLD TO TALK",
                new Color(0.2f, 0.55f, 0.78f));
            SetRect(microphoneButton.GetComponent<RectTransform>(), 0.16f, 0.27f, 0.84f, 0.38f);

            var typedInput = CreateInputField(panel);
            SetRect(typedInput.GetComponent<RectTransform>(), 0.08f, 0.14f, 0.73f, 0.24f);

            var sendButton = CreateButton(
                "Send Typed",
                panel,
                "SEND",
                new Color(0.3f, 0.65f, 0.36f));
            SetRect(sendButton.GetComponent<RectTransform>(), 0.75f, 0.14f, 0.92f, 0.24f);

            var hint = CreateText(
                "Hint",
                panel,
                "Start the local game-brain first. On Android, use the computer's LAN URL.",
                22,
                TextAnchor.MiddleCenter);
            SetRect(hint.rectTransform, 0.08f, 0.03f, 0.92f, 0.12f);
            hint.color = new Color(0.7f, 0.74f, 0.8f);

            var controllerObject = new GameObject(
                "Voice Conversation Controller",
                typeof(AudioSource),
                typeof(VoiceConversationController));
            var audioSource = controllerObject.GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            var controller = controllerObject.GetComponent<VoiceConversationController>();
            controller.ConfigureService("http://127.0.0.1:8787", "resident_1");
            controller.ConfigureDemoUi(status, transcript, response, typedInput, audioSource);

            var pushToTalk = microphoneButton.gameObject.AddComponent<VoicePushToTalkButton>();
            pushToTalk.Configure(controller);
            UnityEventTools.AddPersistentListener(sendButton.onClick, controller.SubmitTypedFromInput);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? string.Empty);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created voice conversation demo scene at {ScenePath}");
        }

        private static InputField CreateInputField(Transform parent)
        {
            var root = CreateRect("Typed Message", parent);
            var image = root.gameObject.AddComponent<Image>();
            image.color = new Color(0.95f, 0.97f, 1f);
            var input = root.gameObject.AddComponent<InputField>();

            var text = CreateText("Text", root, string.Empty, 26, TextAnchor.MiddleLeft);
            text.color = new Color(0.08f, 0.1f, 0.14f);
            text.supportRichText = false;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(18f, 6f);
            text.rectTransform.offsetMax = new Vector2(-18f, -6f);

            var placeholder = CreateText(
                "Placeholder",
                root,
                "Type a message…",
                26,
                TextAnchor.MiddleLeft);
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.color = new Color(0.25f, 0.28f, 0.34f, 0.6f);
            placeholder.rectTransform.anchorMin = Vector2.zero;
            placeholder.rectTransform.anchorMax = Vector2.one;
            placeholder.rectTransform.offsetMin = new Vector2(18f, 6f);
            placeholder.rectTransform.offsetMax = new Vector2(-18f, -6f);

            input.textComponent = text;
            input.placeholder = placeholder;
            input.lineType = InputField.LineType.SingleLine;
            input.characterLimit = 1000;
            return input;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Color color)
        {
            var root = CreateRect(name, parent);
            var image = root.gameObject.AddComponent<Image>();
            image.color = color;
            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var text = CreateText("Label", root, label, 29, TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            int size,
            TextAnchor alignment)
        {
            var rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<RectTransform>();
        }

        private static void SetRect(
            RectTransform rect,
            float minimumX,
            float minimumY,
            float maximumX,
            float maximumY)
        {
            rect.anchorMin = new Vector2(minimumX, minimumY);
            rect.anchorMax = new Vector2(maximumX, maximumY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
