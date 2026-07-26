using UnityEngine;

namespace HumanGlassWatcher.Gameplay.Items
{
    public sealed class ProceduralItemVisual : MonoBehaviour
    {
        [SerializeField] private string styleId;
        [SerializeField] private int partCount;

        public string StyleId => styleId;
        public int PartCount => partCount;

        public void Initialize(string visualStyleId, int visualPartCount)
        {
            styleId = visualStyleId;
            partCount = visualPartCount;
        }
    }
}
