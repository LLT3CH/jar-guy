using System;
using UnityEngine;

namespace HumanGlassWatcher.Character.Presentation
{
    [Serializable]
    public sealed class ResidentAppearance
    {
        public Color Skin = new Color(0.74f, 0.47f, 0.32f, 1f);
        public Color Hair = new Color(0.13f, 0.055f, 0.035f, 1f);
        public Color Shirt = new Color(0.12f, 0.68f, 0.62f, 1f);
        public Color ShirtAccent = new Color(0.95f, 0.67f, 0.19f, 1f);
        public Color Trousers = new Color(0.08f, 0.13f, 0.24f, 1f);
        public Color Shoes = new Color(0.055f, 0.065f, 0.09f, 1f);
        public Color EyeWhite = new Color(0.98f, 0.97f, 0.91f, 1f);
        public Color Ink = new Color(0.025f, 0.032f, 0.045f, 1f);
        public Color Mouth = new Color(0.42f, 0.075f, 0.08f, 1f);

        public static ResidentAppearance Juniper()
        {
            return new ResidentAppearance();
        }
    }
}
