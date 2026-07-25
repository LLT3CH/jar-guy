using System;
using System.Threading;
using System.Threading.Tasks;
using HumanGlassWatcher.Core.Services;
using HumanGlassWatcher.Gameplay.Input;
using HumanGlassWatcher.Gameplay.Items;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HumanGlassWatcher.Gameplay.UI
{
    public sealed class LidSearchController : MonoBehaviour
    {
        public const float RequiredDragFraction = 0.2f;

        [SerializeField] private Transform lid;
        [SerializeField] private RectTransform gestureZone;
        [SerializeField] private GameObject searchPanel;
        [SerializeField] private InputField itemInput;
        [SerializeField] private Text feedbackText;
        [SerializeField] private RuntimeItemFactory itemFactory;
        [SerializeField] private Vector3 dropPosition = new Vector3(0f, 7.8f, 0f);
        [SerializeField] private float openLidOffset = 4.75f;

        private IPointerInputSource pointerInput;
        private IItemResolver itemResolver;
        private Vector2 dragStart;
        private Vector3 closedLidPosition;
        private bool isDragging;
        private bool isSubmitting;

        public bool IsSearchOpen { get; private set; }
        public float CurrentDragFraction { get; private set; }
        public InputField ItemInput => itemInput;

        public void Configure(
            Transform lidTransform,
            RectTransform lidGestureZone,
            GameObject panel,
            InputField input,
            Text feedback,
            RuntimeItemFactory factory,
            Vector3 spawnPosition,
            IItemResolver resolver = null,
            IPointerInputSource inputSource = null)
        {
            lid = lidTransform;
            gestureZone = lidGestureZone;
            searchPanel = panel;
            itemInput = input;
            feedbackText = feedback;
            itemFactory = factory;
            dropPosition = spawnPosition;
            itemResolver = resolver ?? new LocalItemCatalog();
            pointerInput = inputSource ?? new UnifiedPointerInputSource();
            closedLidPosition = lid.localPosition;
            CloseVisuals();
        }

        public void SetPointerInputSource(IPointerInputSource inputSource)
        {
            pointerInput = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
        }

        public void SetItemResolver(IItemResolver resolver)
        {
            itemResolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        private void Update()
        {
            if (pointerInput != null && pointerInput.TryRead(out var pointer))
            {
                ProcessPointer(pointer, Screen.width);
            }

            var keyboard = Keyboard.current;
            if (!IsSearchOpen || keyboard == null)
            {
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Cancel();
            }
            else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                SubmitCurrentPrompt();
            }
        }

        public void ProcessPointer(PointerSample pointer, float screenWidth)
        {
            if (IsSearchOpen)
            {
                return;
            }

            if (pointer.PressedThisFrame &&
                (gestureZone == null ||
                 RectTransformUtility.RectangleContainsScreenPoint(gestureZone, pointer.ScreenPosition)))
            {
                isDragging = true;
                dragStart = pointer.ScreenPosition;
                CurrentDragFraction = 0f;
            }

            if (!isDragging)
            {
                return;
            }

            var safeScreenWidth = Mathf.Max(screenWidth, 1f);
            var delta = pointer.ScreenPosition.x - dragStart.x;
            CurrentDragFraction = Mathf.Clamp01(Mathf.Abs(delta) / safeScreenWidth);
            var direction = Mathf.Approximately(delta, 0f) ? 1f : Mathf.Sign(delta);
            lid.localPosition = closedLidPosition +
                                Vector3.right * direction * openLidOffset *
                                Mathf.Clamp01(CurrentDragFraction / RequiredDragFraction);

            if (!pointer.ReleasedThisFrame)
            {
                return;
            }

            isDragging = false;
            if (CurrentDragFraction >= RequiredDragFraction)
            {
                OpenSearch(direction);
            }
            else
            {
                lid.localPosition = closedLidPosition;
                SetFeedback("Slide the lid at least 20% of the screen width.");
            }
        }

        public void OpenSearch(float direction = 1f)
        {
            IsSearchOpen = true;
            isDragging = false;
            CurrentDragFraction = 1f;
            lid.localPosition = closedLidPosition + Vector3.right * Mathf.Sign(direction) * openLidOffset;
            searchPanel.SetActive(true);
            itemInput.text = string.Empty;
            itemInput.interactable = true;
            itemInput.Select();
            itemInput.ActivateInputField();
            SetFeedback("What should fall into the jar?");
        }

        public void Cancel()
        {
            if (!IsSearchOpen)
            {
                return;
            }

            CloseVisuals();
            SetFeedback("Canceled. Nothing was spawned.");
        }

        public async void SubmitCurrentPrompt()
        {
            await SubmitPromptAsync(itemInput != null ? itemInput.text : string.Empty);
        }

        public async Task<ItemResolution> SubmitPromptAsync(string prompt)
        {
            if (isSubmitting)
            {
                return new ItemResolution(ItemResolutionStatus.Unsupported, null, "An item is already resolving.");
            }

            if (itemResolver == null || itemFactory == null)
            {
                throw new InvalidOperationException("Lid search controller is not configured.");
            }

            isSubmitting = true;
            if (itemInput != null)
            {
                itemInput.interactable = false;
            }

            ItemResolution resolution;
            try
            {
                resolution = await itemResolver.ResolveAsync(prompt, CancellationToken.None);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Item resolver failed; the local loop remains active. {exception.Message}");
                resolution = new ItemResolution(
                    ItemResolutionStatus.OfflineFallback,
                    null,
                    "The item resolver is unavailable. Try an authored item.");
            }
            finally
            {
                isSubmitting = false;
                if (itemInput != null)
                {
                    itemInput.interactable = true;
                }
            }

            SetFeedback(resolution.Feedback);
            if (!resolution.CanSpawn)
            {
                if (itemInput != null)
                {
                    itemInput.Select();
                    itemInput.ActivateInputField();
                }

                return resolution;
            }

            CloseVisuals();
            itemFactory.Spawn(resolution.Definition, dropPosition);
            return resolution;
        }

        private void CloseVisuals()
        {
            IsSearchOpen = false;
            isDragging = false;
            CurrentDragFraction = 0f;
            if (lid != null)
            {
                lid.localPosition = closedLidPosition;
            }

            if (searchPanel != null)
            {
                searchPanel.SetActive(false);
            }
        }

        private void SetFeedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
            }
        }
    }
}
