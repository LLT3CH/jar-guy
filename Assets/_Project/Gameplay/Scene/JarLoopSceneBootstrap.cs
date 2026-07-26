using System.Linq;
using HumanGlassWatcher.Core.Interactions;
using HumanGlassWatcher.Gameplay.Interactions;
using HumanGlassWatcher.Gameplay.Items;
using HumanGlassWatcher.Gameplay.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace HumanGlassWatcher.Gameplay.Scene
{
    public sealed class JarLoopSceneBootstrap : MonoBehaviour
    {
        private const float JarRadius = 3f;
        private const float JarHeight = 6f;
        private static readonly Color Ink = new Color(0.08f, 0.10f, 0.14f, 1f);
        private static readonly Color Paper = new Color(0.92f, 0.95f, 0.98f, 1f);
        private static readonly Color Accent = new Color(0.96f, 0.52f, 0.12f, 1f);

        public bool IsReady { get; private set; }
        public RuntimeItemFactory ItemFactory { get; private set; }
        public RuntimeAffordanceTracker AffordanceTracker { get; private set; }
        public GameplayReactionPresenter ReactionPresenter { get; private set; }
        public LidSearchController LidController { get; private set; }
        public GameObject ResidentTarget { get; private set; }

        private void Awake()
        {
            Build();
        }

        public void Build()
        {
            if (IsReady)
            {
                return;
            }

            var runtimeRoot = new GameObject("JarLoop_Runtime");
            runtimeRoot.transform.SetParent(transform, false);

            CreateCamera(runtimeRoot.transform);
            CreateLighting(runtimeRoot.transform);
            var lid = CreateJar(runtimeRoot.transform);
            ResidentTarget = CreateResident(runtimeRoot.transform);

            var systems = new GameObject("Systems");
            systems.transform.SetParent(runtimeRoot.transform, false);
            var itemRoot = new GameObject("SpawnedItems").transform;
            itemRoot.SetParent(runtimeRoot.transform, false);

            ItemFactory = systems.AddComponent<RuntimeItemFactory>();
            ItemFactory.Configure(itemRoot);
            AffordanceTracker = systems.AddComponent<RuntimeAffordanceTracker>();
            ItemFactory.ItemSpawned += AffordanceTracker.Observe;

            var ui = CreateInterface(runtimeRoot.transform);
            LidController = systems.AddComponent<LidSearchController>();
            LidController.Configure(
                lid,
                ui.GestureZone,
                ui.SearchPanel,
                ui.Input,
                ui.Feedback,
                ItemFactory,
                new Vector3(0f, 8.1f, 0f));

            ui.Submit.onClick.AddListener(LidController.SubmitCurrentPrompt);
            ui.Cancel.onClick.AddListener(LidController.Cancel);
            ReactionPresenter = systems.AddComponent<GameplayReactionPresenter>();
            ReactionPresenter.Configure(ResidentTarget, ui.Feedback, ui.ReactionBadge, ui.CombinationBanner);

            ItemFactory.ItemSpawned -= AffordanceTracker.Observe;
            ItemFactory.ItemSpawned += ReactionPresenter.ObserveItem;
            ItemFactory.ItemSpawned += AffordanceTracker.Observe;
            AffordanceTracker.Changed += affordances =>
            {
                ui.Affordances.text = FormatAffordances(affordances);
                ReactionPresenter.PresentAffordances(affordances);
            };

            IsReady = true;
        }

        private static void CreateCamera(Transform parent)
        {
            if (Camera.main != null)
            {
                return;
            }

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.position = new Vector3(0f, 3.3f, -15.5f);
            cameraObject.transform.rotation = Quaternion.Euler(3f, 0f, 0f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 43f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.075f, 0.11f);
        }

        private static void CreateLighting(Transform parent)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.32f, 0.39f, 0.52f);
            RenderSettings.ambientEquatorColor = new Color(0.18f, 0.22f, 0.30f);
            RenderSettings.ambientGroundColor = new Color(0.07f, 0.08f, 0.11f);

            var keyObject = new GameObject("Key Light");
            keyObject.transform.SetParent(parent, false);
            keyObject.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            var key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1f, 0.9f, 0.77f);
            key.intensity = 1.4f;
            key.shadows = LightShadows.Soft;

            var fillObject = new GameObject("Jar Fill Light");
            fillObject.transform.SetParent(parent, false);
            fillObject.transform.position = new Vector3(-4f, 4f, -3f);
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.range = 16f;
            fill.intensity = 5f;
            fill.color = new Color(0.34f, 0.66f, 1f);
        }

        private static Transform CreateJar(Transform parent)
        {
            var jar = new GameObject("Jar");
            jar.transform.SetParent(parent, false);

            var baseVisual = CreatePrimitive(
                "Jar Base",
                PrimitiveType.Cylinder,
                jar.transform,
                new Vector3(0f, 0.15f, 0f),
                new Vector3(3.25f, 0.15f, 3.25f),
                new Color(0.10f, 0.14f, 0.19f));
            Object.Destroy(baseVisual.GetComponent<Collider>());

            var pedestal = CreatePrimitive(
                "Jar Pedestal",
                PrimitiveType.Cylinder,
                jar.transform,
                new Vector3(0f, -0.10f, 0f),
                new Vector3(3.55f, 0.10f, 3.55f),
                new Color(0.035f, 0.045f, 0.065f));
            Object.Destroy(pedestal.GetComponent<Collider>());

            var floor = new GameObject("Interior Floor Collision");
            floor.transform.SetParent(jar.transform, false);
            floor.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            var floorCollider = floor.AddComponent<BoxCollider>();
            floorCollider.size = new Vector3(5.35f, 0.35f, 5.35f);

            var glass = CreatePrimitive(
                "Transparent Jar",
                PrimitiveType.Cylinder,
                jar.transform,
                new Vector3(0f, JarHeight * 0.5f, 0f),
                new Vector3(JarRadius, JarHeight * 0.5f, JarRadius),
                Color.white);
            Object.Destroy(glass.GetComponent<Collider>());
            glass.GetComponent<Renderer>().sharedMaterial =
                PlaceholderMaterials.CreateTransparent(new Color(0.66f, 0.88f, 1f, 0.14f));

            var glassHighlight = CreatePrimitive(
                "Glass Highlight",
                PrimitiveType.Cube,
                jar.transform,
                new Vector3(-1.82f, JarHeight * 0.53f, -2.20f),
                new Vector3(0.20f, JarHeight * 0.72f, 0.05f),
                new Color(0.80f, 0.96f, 1f, 0.20f));
            Object.Destroy(glassHighlight.GetComponent<Collider>());
            glassHighlight.GetComponent<Renderer>().sharedMaterial =
                PlaceholderMaterials.CreateTransparent(new Color(0.80f, 0.96f, 1f, 0.20f));

            var collisionInterior = new GameObject("Collision Interior");
            collisionInterior.transform.SetParent(jar.transform, false);
            const int segmentCount = 16;
            for (var index = 0; index < segmentCount; index++)
            {
                var angle = index * 360f / segmentCount;
                var radians = angle * Mathf.Deg2Rad;
                var segment = new GameObject($"Wall Collision {index:00}");
                segment.transform.SetParent(collisionInterior.transform, false);
                segment.transform.localPosition = new Vector3(
                    Mathf.Sin(radians) * 2.78f,
                    JarHeight * 0.5f,
                    Mathf.Cos(radians) * 2.78f);
                segment.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
                var collider = segment.AddComponent<BoxCollider>();
                collider.size = new Vector3(1.12f, JarHeight, 0.22f);
            }

            var rim = CreatePrimitive(
                "Jar Rim",
                PrimitiveType.Cylinder,
                jar.transform,
                new Vector3(0f, JarHeight, 0f),
                new Vector3(3.12f, 0.10f, 3.12f),
                new Color(0.58f, 0.78f, 0.9f, 0.36f));
            Object.Destroy(rim.GetComponent<Collider>());
            rim.GetComponent<Renderer>().sharedMaterial =
                PlaceholderMaterials.CreateTransparent(new Color(0.60f, 0.82f, 0.96f, 0.34f));

            var lid = CreatePrimitive(
                "Sliding Lid",
                PrimitiveType.Cylinder,
                jar.transform,
                new Vector3(0f, JarHeight + 0.25f, 0f),
                new Vector3(3.3f, 0.18f, 3.3f),
                new Color(0.96f, 0.54f, 0.13f));
            Object.Destroy(lid.GetComponent<Collider>());

            var lidBand = CreatePrimitive(
                "Lid Grip Band",
                PrimitiveType.Cylinder,
                lid.transform,
                new Vector3(0f, -0.62f, 0f),
                new Vector3(1.02f, 0.58f, 1.02f),
                new Color(0.19f, 0.22f, 0.28f));
            Object.Destroy(lidBand.GetComponent<Collider>());

            return lid.transform;
        }

        private static GameObject CreateResident(Transform parent)
        {
            var resident = new GameObject("Resident Target - Juniper");
            resident.transform.SetParent(parent, false);
            resident.transform.localPosition = new Vector3(1.15f, 0.24f, 0.35f);
            var residentCollider = resident.AddComponent<CapsuleCollider>();
            residentCollider.center = new Vector3(0f, 0.96f, 0f);
            residentCollider.radius = 0.44f;
            residentCollider.height = 1.92f;

            var placeholderVisual = new GameObject("Facing Marker");
            placeholderVisual.transform.SetParent(resident.transform, false);

            var body = CreatePrimitive(
                "Juniper Body",
                PrimitiveType.Capsule,
                placeholderVisual.transform,
                new Vector3(0f, 0.78f, 0f),
                new Vector3(0.48f, 0.65f, 0.42f),
                new Color(0.22f, 0.76f, 0.56f));
            Object.Destroy(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().sharedMaterial =
                PlaceholderMaterials.CreateOpaque(new Color(0.22f, 0.76f, 0.56f));

            var head = CreatePrimitive(
                "Juniper Head",
                PrimitiveType.Sphere,
                placeholderVisual.transform,
                new Vector3(0f, 1.78f, -0.03f),
                new Vector3(0.43f, 0.47f, 0.42f),
                new Color(0.76f, 0.55f, 0.40f));
            Object.Destroy(head.GetComponent<Collider>());

            CreateResidentLimb("Left Arm", placeholderVisual.transform, new Vector3(-0.48f, 0.84f, 0f), 8f);
            CreateResidentLimb("Right Arm", placeholderVisual.transform, new Vector3(0.48f, 0.84f, 0f), -8f);
            CreateResidentLimb("Left Leg", placeholderVisual.transform, new Vector3(-0.21f, 0.12f, 0f), 0f);
            CreateResidentLimb("Right Leg", placeholderVisual.transform, new Vector3(0.21f, 0.12f, 0f), 0f);

            var leftEye = CreatePrimitive(
                "Left Eye",
                PrimitiveType.Sphere,
                placeholderVisual.transform,
                new Vector3(-0.14f, 1.82f, -0.39f),
                new Vector3(0.075f, 0.095f, 0.055f),
                Ink);
            Object.Destroy(leftEye.GetComponent<Collider>());
            var rightEye = CreatePrimitive(
                "Right Eye",
                PrimitiveType.Sphere,
                placeholderVisual.transform,
                new Vector3(0.14f, 1.82f, -0.39f),
                new Vector3(0.075f, 0.095f, 0.055f),
                Ink);
            Object.Destroy(rightEye.GetComponent<Collider>());

            return resident;
        }

        private static void CreateResidentLimb(
            string limbName,
            Transform parent,
            Vector3 localPosition,
            float zRotation)
        {
            var limb = CreatePrimitive(
                limbName,
                PrimitiveType.Capsule,
                parent,
                localPosition,
                new Vector3(0.15f, limbName.Contains("Leg") ? 0.42f : 0.48f, 0.15f),
                new Color(0.18f, 0.54f, 0.43f));
            limb.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
            Object.Destroy(limb.GetComponent<Collider>());
        }

        private static InterfaceReferences CreateInterface(Transform parent)
        {
            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.transform.SetParent(parent, false);
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<InputSystemUIInputModule>();
            }

            var canvasObject = new GameObject("Jar Loop UI");
            canvasObject.transform.SetParent(parent, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var gestureZone = CreateRect("Lid Gesture Zone", canvasObject.transform);
            SetAnchors(gestureZone, new Vector2(0f, 0.63f), Vector2.one, Vector2.zero, Vector2.zero);

            var title = CreateText(
                "Title",
                canvasObject.transform,
                "HUMAN GLASS WATCHER  •  PLAYABLE JAR LOOP",
                22,
                FontStyle.Bold,
                Paper,
                TextAnchor.UpperLeft);
            SetAnchors(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(28f, -58f), new Vector2(-28f, -18f));

            var instruction = CreateText(
                "Gesture Instruction",
                canvasObject.transform,
                "DRAG THE ORANGE LID HORIZONTALLY ≥ 20%  •  MOUSE OR TOUCH",
                18,
                FontStyle.Bold,
                Accent,
                TextAnchor.UpperCenter);
            SetAnchors(instruction.rectTransform, new Vector2(0f, 0.72f), new Vector2(1f, 0.72f),
                new Vector2(20f, -24f), new Vector2(-20f, 24f));

            var combinationBanner = CreateText(
                "Combination Banner",
                canvasObject.transform,
                string.Empty,
                18,
                FontStyle.Bold,
                new Color(1f, 0.76f, 0.26f),
                TextAnchor.MiddleCenter);
            SetAnchors(combinationBanner.rectTransform, new Vector2(0.18f, 0.57f), new Vector2(0.82f, 0.66f),
                Vector2.zero, Vector2.zero);
            combinationBanner.gameObject.SetActive(false);

            var searchPanel = CreateImage(
                "Search Panel",
                canvasObject.transform,
                new Color(0.045f, 0.06f, 0.085f, 0.96f));
            SetAnchors(searchPanel.rectTransform, new Vector2(0.16f, 0.69f), new Vector2(0.84f, 0.91f),
                Vector2.zero, Vector2.zero);

            var prompt = CreateText(
                "Prompt",
                searchPanel.transform,
                "DROP AN ITEM INTO THE JAR",
                17,
                FontStyle.Bold,
                Paper,
                TextAnchor.MiddleLeft);
            SetAnchors(prompt.rectTransform, new Vector2(0.04f, 0.68f), new Vector2(0.96f, 0.94f),
                Vector2.zero, Vector2.zero);

            var input = CreateInputField(searchPanel.transform);
            SetAnchors(input.GetComponent<RectTransform>(), new Vector2(0.04f, 0.25f), new Vector2(0.70f, 0.66f),
                Vector2.zero, Vector2.zero);

            var submit = CreateButton("Submit", searchPanel.transform, "DROP", Accent);
            SetAnchors(submit.GetComponent<RectTransform>(), new Vector2(0.72f, 0.25f), new Vector2(0.84f, 0.66f),
                Vector2.zero, Vector2.zero);
            var cancel = CreateButton("Cancel", searchPanel.transform, "CANCEL", new Color(0.30f, 0.34f, 0.42f));
            SetAnchors(cancel.GetComponent<RectTransform>(), new Vector2(0.85f, 0.25f), new Vector2(0.96f, 0.66f),
                Vector2.zero, Vector2.zero);

            var feedbackPanel = CreateImage(
                "Feedback Panel",
                canvasObject.transform,
                new Color(0.035f, 0.047f, 0.068f, 0.84f));
            SetAnchors(feedbackPanel.rectTransform, new Vector2(0.02f, 0.035f), new Vector2(0.98f, 0.19f),
                Vector2.zero, Vector2.zero);
            var feedback = CreateText(
                "Feedback",
                feedbackPanel.transform,
                "The resident is watching. Slide the lid to name an item.",
                18,
                FontStyle.Normal,
                Paper,
                TextAnchor.MiddleCenter);
            SetAnchors(feedback.rectTransform, new Vector2(0.02f, 0.48f), new Vector2(0.98f, 0.95f),
                Vector2.zero, Vector2.zero);
            var affordances = CreateText(
                "Available Affordances",
                feedbackPanel.transform,
                "Available actions will appear here after items land.",
                14,
                FontStyle.Italic,
                new Color(0.62f, 0.78f, 0.94f),
                TextAnchor.MiddleCenter);
            SetAnchors(affordances.rectTransform, new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.48f),
                Vector2.zero, Vector2.zero);

            var residentCaption = CreateText(
                "Resident Caption",
                canvasObject.transform,
                "JUNIPER  •  FICTIONAL ADULT  •  RESPONSIVE TARGET",
                13,
                FontStyle.Bold,
                new Color(0.30f, 0.92f, 0.70f),
                TextAnchor.MiddleRight);
            SetAnchors(residentCaption.rectTransform, new Vector2(0.45f, 0.20f), new Vector2(0.97f, 0.25f),
                Vector2.zero, Vector2.zero);

            var reactionBadge = CreateText(
                "Resident Reaction",
                canvasObject.transform,
                "WATCHING",
                15,
                FontStyle.Bold,
                new Color(0.74f, 0.92f, 0.86f),
                TextAnchor.MiddleRight);
            SetAnchors(reactionBadge.rectTransform, new Vector2(0.47f, 0.25f), new Vector2(0.97f, 0.30f),
                Vector2.zero, Vector2.zero);

            searchPanel.gameObject.SetActive(false);
            return new InterfaceReferences
            {
                GestureZone = gestureZone,
                SearchPanel = searchPanel.gameObject,
                Input = input,
                Submit = submit,
                Cancel = cancel,
                Feedback = feedback,
                Affordances = affordances,
                ReactionBadge = reactionBadge,
                CombinationBanner = combinationBanner
            };
        }

        private static string FormatAffordances(System.Collections.Generic.IReadOnlyList<Affordance> affordances)
        {
            if (affordances == null || affordances.Count == 0)
            {
                return "No capability combination is available yet.";
            }

            var latest = affordances.Skip(Mathf.Max(0, affordances.Count - 3))
                .Select(affordance => $"• {affordance.Description}");
            return string.Join("   ", latest);
        }

        private static GameObject CreatePrimitive(
            string objectName,
            PrimitiveType primitive,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color)
        {
            var gameObject = GameObject.CreatePrimitive(primitive);
            gameObject.name = objectName;
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localScale = localScale;
            gameObject.GetComponent<Renderer>().sharedMaterial = PlaceholderMaterials.CreateOpaque(color);
            return gameObject;
        }

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            var gameObject = new GameObject(objectName, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<RectTransform>();
        }

        private static Image CreateImage(string objectName, Transform parent, Color color)
        {
            var rect = CreateRect(objectName, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            string content,
            int fontSize,
            FontStyle style,
            Color color,
            TextAnchor alignment)
        {
            var rect = CreateRect(objectName, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static InputField CreateInputField(Transform parent)
        {
            var background = CreateImage("Item Input", parent, new Color(0.94f, 0.96f, 0.98f));
            var input = background.gameObject.AddComponent<InputField>();
            input.lineType = InputField.LineType.SingleLine;
            input.characterLimit = 160;

            var placeholder = CreateText(
                "Placeholder",
                background.transform,
                "e.g. rubber ball",
                18,
                FontStyle.Italic,
                new Color(0.34f, 0.38f, 0.46f, 0.75f),
                TextAnchor.MiddleLeft);
            SetAnchors(placeholder.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(14f, 2f), new Vector2(-14f, -2f));

            var text = CreateText(
                "Text",
                background.transform,
                string.Empty,
                19,
                FontStyle.Normal,
                Ink,
                TextAnchor.MiddleLeft);
            SetAnchors(text.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(14f, 2f), new Vector2(-14f, -2f));

            input.placeholder = placeholder;
            input.textComponent = text;
            return input;
        }

        private static Button CreateButton(string objectName, Transform parent, string label, Color color)
        {
            var image = CreateImage(objectName, parent, color);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var buttonText = CreateText(
                "Label",
                image.transform,
                label,
                14,
                FontStyle.Bold,
                Color.white,
                TextAnchor.MiddleCenter);
            SetAnchors(buttonText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static void SetAnchors(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
        }

        private sealed class InterfaceReferences
        {
            public RectTransform GestureZone;
            public GameObject SearchPanel;
            public InputField Input;
            public Button Submit;
            public Button Cancel;
            public Text Feedback;
            public Text Affordances;
            public Text ReactionBadge;
            public Text CombinationBanner;
        }
    }
}
