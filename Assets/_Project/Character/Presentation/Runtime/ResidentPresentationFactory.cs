using System;
using System.Collections.Generic;
using UnityEngine;

namespace HumanGlassWatcher.Character.Presentation
{
    public static class ResidentPresentationFactory
    {
        public const string PresentationRootName = "Stylized Adult Presentation";

        public static ResidentVisualRig Build(
            Transform gameplayAnchor,
            ResidentAppearance appearance = null)
        {
            if (gameplayAnchor == null)
            {
                throw new ArgumentNullException(nameof(gameplayAnchor));
            }

            var existing = gameplayAnchor.Find(PresentationRootName);
            if (existing != null)
            {
                return existing.GetComponent<ResidentVisualRig>();
            }

            appearance = appearance ?? ResidentAppearance.Juniper();
            var materials = new MaterialPalette(appearance);
            var root = new GameObject(PresentationRootName);
            root.transform.SetParent(gameplayAnchor, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = InverseScale(gameplayAnchor.localScale);

            var rig = root.AddComponent<ResidentVisualRig>();
            rig.OwnMaterials(materials.All);

            rig.BodyRoot = Pivot("Body", root.transform, Vector3.zero);
            BuildTorso(rig, materials);
            BuildHead(rig, materials);
            BuildArm(rig, materials, true);
            BuildArm(rig, materials, false);
            BuildLeg(rig, materials, true);
            BuildLeg(rig, materials, false);

            return rig;
        }

        private static void BuildTorso(ResidentVisualRig rig, MaterialPalette materials)
        {
            Part(
                "Hips",
                PrimitiveType.Cube,
                rig.BodyRoot,
                new Vector3(0f, -0.22f, 0f),
                new Vector3(0.54f, 0.24f, 0.30f),
                materials.Trousers);
            rig.TorsoVisual = Part(
                "Torso",
                PrimitiveType.Cube,
                rig.BodyRoot,
                new Vector3(0f, 0.18f, 0f),
                new Vector3(0.62f, 0.68f, 0.31f),
                materials.Shirt).transform;
            Part(
                "Shirt Front",
                PrimitiveType.Cube,
                rig.TorsoVisual,
                new Vector3(0f, 0.03f, -0.51f),
                new Vector3(0.50f, 0.52f, 0.035f),
                materials.ShirtAccent);
            Part(
                "Belt",
                PrimitiveType.Cube,
                rig.BodyRoot,
                new Vector3(0f, -0.13f, -0.005f),
                new Vector3(0.565f, 0.075f, 0.325f),
                materials.Ink);
            Part(
                "Neck",
                PrimitiveType.Cylinder,
                rig.BodyRoot,
                new Vector3(0f, 0.575f, 0f),
                new Vector3(0.13f, 0.10f, 0.13f),
                materials.Skin);
        }

        private static void BuildHead(ResidentVisualRig rig, MaterialPalette materials)
        {
            rig.HeadPivot = Pivot("Head Pivot", rig.BodyRoot, new Vector3(0f, 0.79f, 0f));
            var head = Part(
                "Head",
                PrimitiveType.Sphere,
                rig.HeadPivot,
                Vector3.zero,
                new Vector3(0.42f, 0.50f, 0.39f),
                materials.Skin).transform;

            Part(
                "Hair",
                PrimitiveType.Sphere,
                head,
                new Vector3(0f, 0.30f, 0.05f),
                new Vector3(1.055f, 0.47f, 1.06f),
                materials.Hair);
            Part(
                "Left Ear",
                PrimitiveType.Sphere,
                head,
                new Vector3(-0.51f, 0f, 0f),
                new Vector3(0.18f, 0.25f, 0.18f),
                materials.Skin);
            Part(
                "Right Ear",
                PrimitiveType.Sphere,
                head,
                new Vector3(0.51f, 0f, 0f),
                new Vector3(0.18f, 0.25f, 0.18f),
                materials.Skin);

            rig.LeftEye = BuildEye("Left", head, -0.25f, materials).transform;
            rig.RightEye = BuildEye("Right", head, 0.25f, materials).transform;
            Part(
                "Nose",
                PrimitiveType.Sphere,
                head,
                new Vector3(0f, -0.035f, -0.535f),
                new Vector3(0.16f, 0.22f, 0.15f),
                materials.Skin);

            rig.LeftBrow = Part(
                "Left Brow",
                PrimitiveType.Cube,
                head,
                new Vector3(-0.25f, 0.245f, -0.51f),
                new Vector3(0.28f, 0.055f, 0.055f),
                materials.Ink).transform;
            rig.RightBrow = Part(
                "Right Brow",
                PrimitiveType.Cube,
                head,
                new Vector3(0.25f, 0.245f, -0.51f),
                new Vector3(0.28f, 0.055f, 0.055f),
                materials.Ink).transform;

            var mouthPivot = Pivot("Mouth", head, new Vector3(0f, -0.28f, -0.525f));
            rig.MouthCenter = Part(
                "Mouth Center",
                PrimitiveType.Cube,
                mouthPivot,
                Vector3.zero,
                new Vector3(0.25f, 0.055f, 0.045f),
                materials.Mouth).transform;
            rig.MouthLeft = Part(
                "Mouth Left",
                PrimitiveType.Sphere,
                mouthPivot,
                new Vector3(-0.13f, 0f, 0f),
                new Vector3(0.07f, 0.07f, 0.055f),
                materials.Mouth).transform;
            rig.MouthRight = Part(
                "Mouth Right",
                PrimitiveType.Sphere,
                mouthPivot,
                new Vector3(0.13f, 0f, 0f),
                new Vector3(0.07f, 0.07f, 0.055f),
                materials.Mouth).transform;
        }

        private static GameObject BuildEye(
            string side,
            Transform head,
            float x,
            MaterialPalette materials)
        {
            var eye = Part(
                side + " Eye",
                PrimitiveType.Sphere,
                head,
                new Vector3(x, 0.09f, -0.49f),
                new Vector3(0.25f, 0.21f, 0.13f),
                materials.EyeWhite);
            Part(
                side + " Pupil",
                PrimitiveType.Sphere,
                eye.transform,
                new Vector3(0f, 0f, -0.54f),
                new Vector3(0.36f, 0.47f, 0.29f),
                materials.Ink);
            return eye;
        }

        private static void BuildArm(
            ResidentVisualRig rig,
            MaterialPalette materials,
            bool left)
        {
            var side = left ? "Left" : "Right";
            var direction = left ? -1f : 1f;
            var shoulder = Pivot(
                side + " Shoulder",
                rig.BodyRoot,
                new Vector3(direction * 0.39f, 0.43f, 0f));
            Part(
                side + " Upper Arm",
                PrimitiveType.Capsule,
                shoulder,
                new Vector3(0f, -0.22f, 0f),
                new Vector3(0.15f, 0.22f, 0.15f),
                materials.Shirt);

            var elbow = Pivot(side + " Elbow", shoulder, new Vector3(0f, -0.44f, 0f));
            Part(
                side + " Forearm",
                PrimitiveType.Capsule,
                elbow,
                new Vector3(0f, -0.20f, 0f),
                new Vector3(0.125f, 0.20f, 0.125f),
                materials.Skin);
            var hand = Part(
                side + " Hand",
                PrimitiveType.Sphere,
                elbow,
                new Vector3(0f, -0.43f, -0.015f),
                new Vector3(0.18f, 0.20f, 0.15f),
                materials.Skin).transform;

            if (left)
            {
                rig.LeftShoulder = shoulder;
                rig.LeftElbow = elbow;
                rig.LeftHand = hand;
            }
            else
            {
                rig.RightShoulder = shoulder;
                rig.RightElbow = elbow;
                rig.RightHand = hand;
            }
        }

        private static void BuildLeg(
            ResidentVisualRig rig,
            MaterialPalette materials,
            bool left)
        {
            var side = left ? "Left" : "Right";
            var direction = left ? -1f : 1f;
            var hip = Pivot(
                side + " Hip",
                rig.BodyRoot,
                new Vector3(direction * 0.17f, -0.33f, 0f));
            Part(
                side + " Thigh",
                PrimitiveType.Capsule,
                hip,
                new Vector3(0f, -0.27f, 0f),
                new Vector3(0.19f, 0.27f, 0.19f),
                materials.Trousers);

            var knee = Pivot(side + " Knee", hip, new Vector3(0f, -0.53f, 0f));
            Part(
                side + " Shin",
                PrimitiveType.Capsule,
                knee,
                new Vector3(0f, -0.23f, 0f),
                new Vector3(0.16f, 0.23f, 0.16f),
                materials.Trousers);
            Part(
                side + " Foot",
                PrimitiveType.Cube,
                knee,
                new Vector3(0f, -0.49f, -0.09f),
                new Vector3(0.22f, 0.14f, 0.38f),
                materials.Shoes);

            if (left)
            {
                rig.LeftHip = hip;
                rig.LeftKnee = knee;
            }
            else
            {
                rig.RightHip = hip;
                rig.RightKnee = knee;
            }
        }

        private static Transform Pivot(string name, Transform parent, Vector3 localPosition)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = localPosition;
            return pivot;
        }

        private static GameObject Part(
            string name,
            PrimitiveType primitiveType,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            var part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.identity;
            part.transform.localScale = localScale;
            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(collider);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }
            }

            var renderer = part.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return part;
        }

        private static Vector3 InverseScale(Vector3 scale)
        {
            return new Vector3(SafeInverse(scale.x), SafeInverse(scale.y), SafeInverse(scale.z));
        }

        private static float SafeInverse(float value)
        {
            return Mathf.Abs(value) < 0.0001f ? 1f : 1f / value;
        }

        private sealed class MaterialPalette
        {
            public MaterialPalette(ResidentAppearance appearance)
            {
                Skin = Create("Resident Skin", appearance.Skin, 0.42f);
                Hair = Create("Resident Hair", appearance.Hair, 0.2f);
                Shirt = Create("Resident Shirt", appearance.Shirt, 0.26f);
                ShirtAccent = Create("Resident Shirt Accent", appearance.ShirtAccent, 0.24f);
                Trousers = Create("Resident Trousers", appearance.Trousers, 0.18f);
                Shoes = Create("Resident Shoes", appearance.Shoes, 0.38f);
                EyeWhite = Create("Resident Eye White", appearance.EyeWhite, 0.55f);
                Ink = Create("Resident Ink", appearance.Ink, 0.15f);
                Mouth = Create("Resident Mouth", appearance.Mouth, 0.28f);
                All = new[]
                {
                    Skin, Hair, Shirt, ShirtAccent, Trousers, Shoes, EyeWhite, Ink, Mouth
                };
            }

            public Material Skin { get; }
            public Material Hair { get; }
            public Material Shirt { get; }
            public Material ShirtAccent { get; }
            public Material Trousers { get; }
            public Material Shoes { get; }
            public Material EyeWhite { get; }
            public Material Ink { get; }
            public Material Mouth { get; }
            public IReadOnlyList<Material> All { get; }

            private static Material Create(string name, Color color, float smoothness)
            {
                var shader =
                    Shader.Find("Universal Render Pipeline/Lit") ??
                    Shader.Find("Standard") ??
                    Shader.Find("Sprites/Default");
                var material = new Material(shader)
                {
                    name = name,
                    color = color,
                    enableInstancing = true
                };
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", color);
                }

                if (material.HasProperty("_Smoothness"))
                {
                    material.SetFloat("_Smoothness", smoothness);
                }

                if (material.HasProperty("_Metallic"))
                {
                    material.SetFloat("_Metallic", 0f);
                }

                return material;
            }
        }
    }
}
