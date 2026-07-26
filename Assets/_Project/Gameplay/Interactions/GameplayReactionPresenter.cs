using System.Collections.Generic;
using System.Linq;
using HumanGlassWatcher.Character.Presentation;
using HumanGlassWatcher.Core.Interactions;
using HumanGlassWatcher.Core.Items;
using HumanGlassWatcher.Gameplay.Items;
using UnityEngine;
using UnityEngine.UI;

namespace HumanGlassWatcher.Gameplay.Interactions
{
    public enum GameplayReactionKind
    {
        Neutral,
        Curious,
        Delighted,
        Comforted,
        Cautious,
        Disgusted,
        Illuminated
    }

    public sealed class GameplayReactionPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject resident;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Text reactionBadge;
        [SerializeField] private Text combinationBanner;

        private Renderer residentRenderer;
        private Material residentMaterial;
        private ResidentPresentationController presentationController;
        private Vector3 residentBasePosition;
        private Vector3 residentBaseScale;
        private Color residentBaseColor;
        private Color reactionColor;
        private Vector3 reactionOffset;
        private float reactionStartedAt = float.NegativeInfinity;
        private int observedAffordanceCount;

        public GameplayReactionKind LastReaction { get; private set; }
        public Affordance? LastCombination { get; private set; }
        public Text ReactionBadge => reactionBadge;
        public Text CombinationBanner => combinationBanner;
        public ResidentPresentationController PresentationController => presentationController;

        public void Configure(
            GameObject residentTarget,
            Text feedback,
            Text residentReactionBadge,
            Text comboBanner)
        {
            resident = residentTarget;
            feedbackText = feedback;
            reactionBadge = residentReactionBadge;
            combinationBanner = comboBanner;

            presentationController =
                resident != null
                    ? resident.GetComponentInChildren<ResidentPresentationController>(true)
                    : null;
            ResidentPresentationInstaller.Installed -= OnPresentationInstalled;
            ResidentPresentationInstaller.Installed += OnPresentationInstalled;
            residentRenderer = resident != null ? resident.GetComponentInChildren<Renderer>() : null;
            if (residentRenderer != null)
            {
                residentMaterial = residentRenderer.material;
                residentBaseColor = residentMaterial.color;
            }

            if (resident != null)
            {
                residentBasePosition = resident.transform.localPosition;
                residentBaseScale = resident.transform.localScale;
            }

            LastReaction = GameplayReactionKind.Neutral;
            SetBadge("JUNIPER IS WATCHING", new Color(0.32f, 0.92f, 0.72f));
            if (combinationBanner != null)
            {
                combinationBanner.gameObject.SetActive(false);
            }
        }

        public void ObserveItem(SpawnedItem item)
        {
            if (item == null || item.Definition == null)
            {
                return;
            }

            var definition = item.Definition;
            if (definition.Has(ItemCapability.Dirty) || definition.Has(ItemCapability.Toxic))
            {
                React(
                    GameplayReactionKind.Disgusted,
                    "JUNIPER RECOILS • HAZARD",
                    $"{definition.DisplayName} makes Juniper recoil. Hygiene and comfort are now at risk.",
                    new Color(1f, 0.28f, 0.16f),
                    new Vector3(0.38f, 0.08f, 0f));
            }
            else if (definition.Has(ItemCapability.LightSource))
            {
                React(
                    GameplayReactionKind.Illuminated,
                    "JUNIPER SHIELDS THEIR EYES • LIGHT",
                    $"Juniper notices the beam from {definition.DisplayName}. Signaling is possible.",
                    new Color(1f, 0.86f, 0.38f),
                    new Vector3(0.14f, 0.03f, 0f));
            }
            else if (definition.Has(ItemCapability.Entertainment) || definition.Has(ItemCapability.Bouncy))
            {
                React(
                    GameplayReactionKind.Delighted,
                    "JUNIPER PERKS UP • PLAY",
                    $"{definition.DisplayName} catches Juniper's attention as something playable.",
                    new Color(0.26f, 0.92f, 0.72f),
                    new Vector3(-0.10f, 0.10f, 0f));
            }
            else if (definition.Has(ItemCapability.Edible) || definition.Has(ItemCapability.Drinkable))
            {
                React(
                    GameplayReactionKind.Curious,
                    "JUNIPER LEANS CLOSER • NEED",
                    $"Juniper appraises {definition.DisplayName} as food or drink.",
                    new Color(0.44f, 0.88f, 0.42f),
                    new Vector3(-0.12f, 0.06f, 0f));
            }
            else if (definition.Has(ItemCapability.Comfort))
            {
                React(
                    GameplayReactionKind.Comforted,
                    "JUNIPER RELAXES • COMFORT",
                    $"{definition.DisplayName} looks useful for rest and comfort.",
                    new Color(0.38f, 0.68f, 1f),
                    new Vector3(0f, -0.06f, 0f));
            }
            else if (definition.Has(ItemCapability.SharpEdge) ||
                     definition.Has(ItemCapability.SwingTool) ||
                     definition.Has(ItemCapability.Lever))
            {
                React(
                    GameplayReactionKind.Cautious,
                    "JUNIPER STEPS BACK • TOOL",
                    $"Juniper treats {definition.DisplayName} as useful, but potentially dangerous.",
                    new Color(1f, 0.65f, 0.18f),
                    new Vector3(0.24f, 0.04f, 0f));
            }
            else
            {
                React(
                    GameplayReactionKind.Curious,
                    "JUNIPER INSPECTS THE DROP",
                    $"Juniper studies {definition.DisplayName} before deciding what to do.",
                    new Color(0.56f, 0.78f, 1f),
                    new Vector3(-0.08f, 0.04f, 0f));
            }
        }

        public void PresentAffordances(IReadOnlyList<Affordance> affordances)
        {
            if (affordances == null)
            {
                return;
            }

            var newCombination = affordances
                .Skip(Mathf.Clamp(observedAffordanceCount, 0, affordances.Count))
                .LastOrDefault(affordance =>
                    !string.IsNullOrEmpty(affordance.SecondaryId) &&
                    affordance.SecondaryId != "jar_boundary");
            observedAffordanceCount = affordances.Count;

            if (string.IsNullOrEmpty(newCombination.PrimaryId))
            {
                return;
            }

            LastCombination = newCombination;
            var readableKind = SplitPascalCase(newCombination.Kind.ToString()).ToUpperInvariant();
            if (combinationBanner != null)
            {
                combinationBanner.gameObject.SetActive(true);
                combinationBanner.text = $"COMBINATION DISCOVERED  •  {readableKind}\n{newCombination.Description}";
                combinationBanner.color = new Color(1f, 0.78f, 0.22f);
            }

            if (feedbackText != null)
            {
                feedbackText.text = $"New item-to-item action: {newCombination.Description}";
            }

            presentationController?.SetReaction(ResidentReaction.Celebrate, 0.82f, 1.5f);
        }

        private void Update()
        {
            if (resident == null ||
                presentationController != null ||
                float.IsNegativeInfinity(reactionStartedAt))
            {
                return;
            }

            var elapsed = Time.unscaledTime - reactionStartedAt;
            var envelope = Mathf.Clamp01(1f - elapsed / 2.4f);
            var pulse = 1f + Mathf.Sin(elapsed * 12f) * 0.05f * envelope;
            resident.transform.localScale = residentBaseScale * pulse;
            resident.transform.localPosition = Vector3.Lerp(
                residentBasePosition,
                residentBasePosition + reactionOffset,
                envelope);

            if (residentMaterial != null)
            {
                var color = Color.Lerp(residentBaseColor, reactionColor, envelope * 0.72f);
                residentMaterial.color = color;
                residentMaterial.SetColor("_BaseColor", color);
            }

            if (elapsed < 2.4f)
            {
                return;
            }

            resident.transform.localScale = residentBaseScale;
            resident.transform.localPosition = residentBasePosition;
            reactionStartedAt = float.NegativeInfinity;
        }

        private void React(
            GameplayReactionKind kind,
            string badge,
            string feedback,
            Color color,
            Vector3 offset)
        {
            LastReaction = kind;
            reactionColor = color;
            reactionOffset = offset;
            presentationController ??=
                resident != null
                    ? resident.GetComponentInChildren<ResidentPresentationController>(true)
                    : null;
            if (presentationController != null)
            {
                presentationController.SetReaction(ToResidentReaction(kind), 1f, 2.2f);
                reactionStartedAt = float.NegativeInfinity;
            }
            else
            {
                reactionStartedAt = Time.unscaledTime;
            }

            SetBadge(badge, color);
            if (feedbackText != null)
            {
                feedbackText.text = feedback;
            }
        }

        private void SetBadge(string content, Color color)
        {
            if (reactionBadge == null)
            {
                return;
            }

            reactionBadge.text = content;
            reactionBadge.color = color;
        }

        private void OnDestroy()
        {
            ResidentPresentationInstaller.Installed -= OnPresentationInstalled;
        }

        private void OnPresentationInstalled(ResidentPresentationController controller)
        {
            if (controller != null &&
                resident != null &&
                controller.transform.IsChildOf(resident.transform))
            {
                presentationController = controller;
                resident.transform.localPosition = residentBasePosition;
                resident.transform.localScale = residentBaseScale;
                reactionStartedAt = float.NegativeInfinity;
                if (residentMaterial != null)
                {
                    residentMaterial.color = residentBaseColor;
                    residentMaterial.SetColor("_BaseColor", residentBaseColor);
                }
            }
        }

        private static ResidentReaction ToResidentReaction(GameplayReactionKind kind)
        {
            switch (kind)
            {
                case GameplayReactionKind.Disgusted:
                    return ResidentReaction.Disgust;
                case GameplayReactionKind.Delighted:
                    return ResidentReaction.Celebrate;
                case GameplayReactionKind.Comforted:
                    return ResidentReaction.Comfort;
                case GameplayReactionKind.Cautious:
                    return ResidentReaction.Recoil;
                case GameplayReactionKind.Curious:
                case GameplayReactionKind.Illuminated:
                    return ResidentReaction.Inspect;
                default:
                    return ResidentReaction.None;
            }
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var characters = new List<char>(value.Length + 4) { value[0] };
            for (var index = 1; index < value.Length; index++)
            {
                if (char.IsUpper(value[index]) && !char.IsUpper(value[index - 1]))
                {
                    characters.Add(' ');
                }

                characters.Add(value[index]);
            }

            return new string(characters.ToArray());
        }
    }
}
