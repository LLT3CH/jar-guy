using UnityEngine;
using UnityEngine.EventSystems;

namespace HumanGlassWatcher.Voice
{
    public sealed class VoicePushToTalkButton :
        MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerExitHandler
    {
        [SerializeField] private VoiceConversationController controller;
        private bool pressed;

        public void Configure(VoiceConversationController value)
        {
            controller = value;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (controller == null || pressed)
            {
                return;
            }

            pressed = true;
            controller.BeginPushToTalk();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Release();
        }

        private void OnDisable()
        {
            Release();
        }

        private void Release()
        {
            if (!pressed)
            {
                return;
            }

            pressed = false;
            if (controller != null)
            {
                controller.EndPushToTalk();
            }
        }
    }
}
