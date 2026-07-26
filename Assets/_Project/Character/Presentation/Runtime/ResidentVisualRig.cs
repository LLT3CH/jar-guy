using System.Collections.Generic;
using UnityEngine;

namespace HumanGlassWatcher.Character.Presentation
{
    public sealed class ResidentVisualRig : MonoBehaviour
    {
        private readonly List<Material> ownedMaterials = new List<Material>();

        public Transform BodyRoot { get; internal set; }
        public Transform TorsoVisual { get; internal set; }
        public Transform HeadPivot { get; internal set; }
        public Transform LeftShoulder { get; internal set; }
        public Transform RightShoulder { get; internal set; }
        public Transform LeftElbow { get; internal set; }
        public Transform RightElbow { get; internal set; }
        public Transform LeftHip { get; internal set; }
        public Transform RightHip { get; internal set; }
        public Transform LeftKnee { get; internal set; }
        public Transform RightKnee { get; internal set; }
        public Transform LeftHand { get; internal set; }
        public Transform RightHand { get; internal set; }
        public Transform LeftEye { get; internal set; }
        public Transform RightEye { get; internal set; }
        public Transform LeftBrow { get; internal set; }
        public Transform RightBrow { get; internal set; }
        public Transform MouthCenter { get; internal set; }
        public Transform MouthLeft { get; internal set; }
        public Transform MouthRight { get; internal set; }

        public int RendererCount => GetComponentsInChildren<Renderer>(true).Length;
        public int MaterialCount => ownedMaterials.Count;

        internal void OwnMaterials(IEnumerable<Material> materials)
        {
            ownedMaterials.AddRange(materials);
        }

        private void OnDestroy()
        {
            for (var index = 0; index < ownedMaterials.Count; index++)
            {
                var material = ownedMaterials[index];
                if (material == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(material);
                }
                else
                {
                    DestroyImmediate(material);
                }
            }

            ownedMaterials.Clear();
        }
    }
}
