using UnityEngine;
using UnityEngine.InputSystem;

namespace HumanGlassWatcher.Gameplay.Input
{
    public readonly struct PointerSample
    {
        public PointerSample(
            Vector2 screenPosition,
            bool pressedThisFrame,
            bool isPressed,
            bool releasedThisFrame)
        {
            ScreenPosition = screenPosition;
            PressedThisFrame = pressedThisFrame;
            IsPressed = isPressed;
            ReleasedThisFrame = releasedThisFrame;
        }

        public Vector2 ScreenPosition { get; }
        public bool PressedThisFrame { get; }
        public bool IsPressed { get; }
        public bool ReleasedThisFrame { get; }
    }

    public interface IPointerInputSource
    {
        bool TryRead(out PointerSample sample);
    }

    public sealed class UnifiedPointerInputSource : IPointerInputSource
    {
        public bool TryRead(out PointerSample sample)
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                var touch = touchscreen.primaryTouch;
                var pressed = touch.press.isPressed;
                var pressedThisFrame = touch.press.wasPressedThisFrame;
                var releasedThisFrame = touch.press.wasReleasedThisFrame;
                if (pressed || pressedThisFrame || releasedThisFrame)
                {
                    sample = new PointerSample(
                        touch.position.ReadValue(),
                        pressedThisFrame,
                        pressed,
                        releasedThisFrame);
                    return true;
                }
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                var leftButton = mouse.leftButton;
                sample = new PointerSample(
                    mouse.position.ReadValue(),
                    leftButton.wasPressedThisFrame,
                    leftButton.isPressed,
                    leftButton.wasReleasedThisFrame);
                return true;
            }

            sample = default;
            return false;
        }
    }
}
