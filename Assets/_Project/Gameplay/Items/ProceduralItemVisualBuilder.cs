using System;
using HumanGlassWatcher.Core.Items;
using UnityEngine;

namespace HumanGlassWatcher.Gameplay.Items
{
    public static class ProceduralItemVisualBuilder
    {
        public static ProceduralItemVisual Build(GameObject itemRoot, ItemDefinition definition)
        {
            if (itemRoot == null)
            {
                throw new ArgumentNullException(nameof(itemRoot));
            }

            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var visualRoot = new GameObject($"Visual_{definition.CanonicalId}");
            visualRoot.transform.SetParent(itemRoot.transform, false);
            var context = new BuildContext(visualRoot.transform, definition.Scale);

            switch (definition.CanonicalId)
            {
                case "apple":
                    BuildApple(context, definition.Color);
                    break;
                case "chocolate_cake":
                    BuildCake(context);
                    break;
                case "water_bottle":
                    BuildWaterBottle(context);
                    break;
                case "dog_feces":
                    BuildDogFeces(context);
                    break;
                case "rubber_ball":
                    BuildRubberBall(context, definition.Color);
                    break;
                case "baseball_bat":
                    BuildBaseballBat(context);
                    break;
                case "hockey_stick":
                    BuildHockeyStick(context);
                    break;
                case "blanket":
                    BuildBlanket(context, definition.Color);
                    break;
                case "rope":
                    BuildRope(context);
                    break;
                case "scissors":
                    BuildScissors(context);
                    break;
                case "sponge":
                    BuildSponge(context);
                    break;
                case "flashlight":
                    BuildFlashlight(context);
                    break;
                default:
                    BuildIdeaObject(context, definition.Color);
                    break;
            }

            var identity = itemRoot.AddComponent<ProceduralItemVisual>();
            identity.Initialize(
                IsAuthoredCatalogItem(definition.CanonicalId) ? definition.CanonicalId : "idea_object",
                context.PartCount);
            return identity;
        }

        public static bool IsAuthoredCatalogItem(string canonicalId)
        {
            switch (canonicalId)
            {
                case "apple":
                case "chocolate_cake":
                case "water_bottle":
                case "dog_feces":
                case "rubber_ball":
                case "baseball_bat":
                case "hockey_stick":
                case "blanket":
                case "rope":
                case "scissors":
                case "sponge":
                case "flashlight":
                    return true;
                default:
                    return false;
            }
        }

        private static void BuildApple(BuildContext context, Color red)
        {
            context.Part("Apple Body", PrimitiveType.Sphere, Vector3.zero, new Vector3(0.94f, 0.82f, 0.94f), red);
            context.Part("Apple Highlight", PrimitiveType.Sphere, new Vector3(-0.20f, 0.09f, -0.38f),
                new Vector3(0.24f, 0.35f, 0.16f), new Color(1f, 0.32f, 0.25f));
            context.Part("Apple Stem", PrimitiveType.Cylinder, new Vector3(0f, 0.52f, 0f),
                new Vector3(0.12f, 0.20f, 0.12f), new Color(0.30f, 0.13f, 0.04f));
            context.Part("Apple Leaf", PrimitiveType.Sphere, new Vector3(0.20f, 0.58f, 0f),
                new Vector3(0.34f, 0.08f, 0.20f), new Color(0.18f, 0.56f, 0.18f),
                new Vector3(0f, 0f, -24f));
        }

        private static void BuildCake(BuildContext context)
        {
            context.Part("Cake Plate", PrimitiveType.Cylinder, new Vector3(0f, -0.43f, 0f),
                new Vector3(1.08f, 0.07f, 1.08f), new Color(0.80f, 0.86f, 0.92f));
            context.Part("Chocolate Cake", PrimitiveType.Cylinder, new Vector3(0f, -0.04f, 0f),
                new Vector3(0.92f, 0.42f, 0.92f), new Color(0.25f, 0.08f, 0.03f));
            context.Part("Frosting", PrimitiveType.Cylinder, new Vector3(0f, 0.39f, 0f),
                new Vector3(0.96f, 0.09f, 0.96f), new Color(0.49f, 0.18f, 0.08f));
            context.Part("Cake Cherry", PrimitiveType.Sphere, new Vector3(0f, 0.58f, 0f),
                new Vector3(0.24f, 0.24f, 0.24f), new Color(0.82f, 0.04f, 0.08f));
        }

        private static void BuildWaterBottle(BuildContext context)
        {
            context.Part("Water Bottle Body", PrimitiveType.Cylinder, new Vector3(0f, -0.08f, 0f),
                new Vector3(0.82f, 0.68f, 0.82f), new Color(0.18f, 0.72f, 1f, 0.48f),
                Vector3.zero, true);
            context.Part("Water Fill", PrimitiveType.Cylinder, new Vector3(0f, -0.25f, 0f),
                new Vector3(0.67f, 0.45f, 0.67f), new Color(0.08f, 0.46f, 0.92f, 0.72f),
                Vector3.zero, true);
            context.Part("Bottle Neck", PrimitiveType.Cylinder, new Vector3(0f, 0.64f, 0f),
                new Vector3(0.42f, 0.22f, 0.42f), new Color(0.62f, 0.90f, 1f, 0.58f),
                Vector3.zero, true);
            context.Part("Blue Bottle Cap", PrimitiveType.Cylinder, new Vector3(0f, 0.88f, 0f),
                new Vector3(0.48f, 0.10f, 0.48f), new Color(0.05f, 0.26f, 0.76f));
            context.Part("Bottle Label", PrimitiveType.Cube, new Vector3(0f, 0f, -0.43f),
                new Vector3(0.72f, 0.25f, 0.04f), new Color(0.91f, 0.97f, 1f));
        }

        private static void BuildDogFeces(BuildContext context)
        {
            var darkBrown = new Color(0.19f, 0.065f, 0.02f);
            var brown = new Color(0.34f, 0.12f, 0.025f);
            context.Part("Waste Coil Base", PrimitiveType.Sphere, new Vector3(0f, -0.30f, 0f),
                new Vector3(1.08f, 0.44f, 0.94f), darkBrown);
            context.Part("Waste Coil Middle", PrimitiveType.Sphere, new Vector3(0.10f, 0.02f, 0f),
                new Vector3(0.78f, 0.46f, 0.70f), brown);
            context.Part("Waste Coil Top", PrimitiveType.Sphere, new Vector3(-0.08f, 0.32f, 0f),
                new Vector3(0.48f, 0.44f, 0.46f), brown);
            context.Part("Waste Tip", PrimitiveType.Capsule, new Vector3(0.02f, 0.56f, 0f),
                new Vector3(0.18f, 0.28f, 0.18f), brown, new Vector3(0f, 0f, -22f));
            context.Part("Stink Wisp Left", PrimitiveType.Cylinder, new Vector3(-0.42f, 0.55f, 0f),
                new Vector3(0.05f, 0.36f, 0.05f), new Color(0.58f, 0.78f, 0.24f, 0.55f),
                new Vector3(0f, 0f, -16f), true);
            context.Part("Stink Wisp Right", PrimitiveType.Cylinder, new Vector3(0.42f, 0.62f, 0f),
                new Vector3(0.05f, 0.30f, 0.05f), new Color(0.58f, 0.78f, 0.24f, 0.55f),
                new Vector3(0f, 0f, 18f), true);
        }

        private static void BuildRubberBall(BuildContext context, Color orange)
        {
            context.Part("Rubber Ball Body", PrimitiveType.Sphere, Vector3.zero,
                new Vector3(0.96f, 0.96f, 0.96f), orange);
            var ink = new Color(0.08f, 0.10f, 0.14f);
            context.Part("Ball Spot Front", PrimitiveType.Sphere, new Vector3(0f, 0f, -0.47f),
                new Vector3(0.22f, 0.22f, 0.10f), ink);
            context.Part("Ball Spot Left", PrimitiveType.Sphere, new Vector3(-0.43f, 0.10f, -0.08f),
                new Vector3(0.15f, 0.20f, 0.18f), ink);
            context.Part("Ball Spot Top", PrimitiveType.Sphere, new Vector3(0.10f, 0.44f, 0f),
                new Vector3(0.18f, 0.12f, 0.18f), ink);
        }

        private static void BuildBaseballBat(BuildContext context)
        {
            var wood = new Color(0.67f, 0.36f, 0.12f);
            context.Part("Bat Handle", PrimitiveType.Cylinder, new Vector3(0f, -0.46f, 0f),
                new Vector3(0.38f, 0.55f, 0.38f), new Color(0.24f, 0.10f, 0.03f));
            context.Part("Bat Barrel", PrimitiveType.Cylinder, new Vector3(0f, 0.24f, 0f),
                new Vector3(0.72f, 0.78f, 0.72f), wood);
            context.Part("Bat End Cap", PrimitiveType.Sphere, new Vector3(0f, 0.98f, 0f),
                new Vector3(0.72f, 0.16f, 0.72f), wood);
            context.Part("Bat Knob", PrimitiveType.Cylinder, new Vector3(0f, -0.95f, 0f),
                new Vector3(0.56f, 0.12f, 0.56f), new Color(0.24f, 0.10f, 0.03f));
        }

        private static void BuildHockeyStick(BuildContext context)
        {
            context.Part("Hockey Shaft", PrimitiveType.Cube, new Vector3(0f, 0.10f, 0f),
                new Vector3(0.62f, 1.72f, 0.58f), new Color(0.82f, 0.86f, 0.92f));
            context.Part("Hockey Grip", PrimitiveType.Cube, new Vector3(0f, 0.88f, 0f),
                new Vector3(0.72f, 0.28f, 0.68f), new Color(0.08f, 0.12f, 0.20f));
            context.Part("Hockey Blade", PrimitiveType.Cube, new Vector3(1.04f, -0.79f, 0f),
                new Vector3(2.55f, 0.30f, 0.88f), new Color(0.08f, 0.12f, 0.20f),
                new Vector3(0f, 0f, -9f));
            context.Part("Blade Tape", PrimitiveType.Cube, new Vector3(1.65f, -0.86f, -0.01f),
                new Vector3(0.52f, 0.34f, 0.92f), new Color(0.90f, 0.94f, 0.98f),
                new Vector3(0f, 0f, -9f));
        }

        private static void BuildBlanket(BuildContext context, Color blue)
        {
            context.Part("Folded Blanket", PrimitiveType.Cube, Vector3.zero,
                new Vector3(0.94f, 0.66f, 0.90f), blue);
            context.Part("Blanket Fold", PrimitiveType.Cube, new Vector3(0.06f, 0.34f, 0f),
                new Vector3(0.80f, 0.12f, 0.82f), new Color(0.24f, 0.68f, 0.88f));
            context.Part("Blanket Stripe Left", PrimitiveType.Cube, new Vector3(-0.28f, -0.01f, -0.46f),
                new Vector3(0.10f, 0.68f, 0.04f), new Color(0.90f, 0.95f, 0.98f));
            context.Part("Blanket Stripe Right", PrimitiveType.Cube, new Vector3(0.28f, -0.01f, -0.46f),
                new Vector3(0.10f, 0.68f, 0.04f), new Color(0.90f, 0.95f, 0.98f));
            context.Part("Blanket Tassel Left", PrimitiveType.Capsule, new Vector3(-0.38f, -0.39f, 0f),
                new Vector3(0.08f, 0.25f, 0.08f), new Color(0.90f, 0.95f, 0.98f));
            context.Part("Blanket Tassel Right", PrimitiveType.Capsule, new Vector3(0.38f, -0.39f, 0f),
                new Vector3(0.08f, 0.25f, 0.08f), new Color(0.90f, 0.95f, 0.98f));
        }

        private static void BuildRope(BuildContext context)
        {
            var ropeColor = new Color(0.70f, 0.50f, 0.24f);
            for (var index = 0; index < 7; index++)
            {
                var normalized = index / 6f;
                var x = Mathf.Lerp(-1.35f, 1.35f, normalized);
                var y = Mathf.Sin(normalized * Mathf.PI * 2f) * 0.12f;
                context.Part($"Rope Segment {index + 1}", PrimitiveType.Capsule, new Vector3(x, y, 0f),
                    new Vector3(0.34f, 0.42f, 0.34f), ropeColor,
                    new Vector3(0f, 0f, 74f + Mathf.Cos(normalized * Mathf.PI * 2f) * 14f));
            }

            context.Part("Rope Knot Left", PrimitiveType.Sphere, new Vector3(-1.48f, -0.05f, 0f),
                new Vector3(0.52f, 0.52f, 0.52f), new Color(0.55f, 0.36f, 0.15f));
            context.Part("Rope Knot Right", PrimitiveType.Sphere, new Vector3(1.48f, 0.05f, 0f),
                new Vector3(0.52f, 0.52f, 0.52f), new Color(0.55f, 0.36f, 0.15f));
        }

        private static void BuildScissors(BuildContext context)
        {
            var steel = new Color(0.72f, 0.78f, 0.84f);
            var handle = new Color(0.82f, 0.08f, 0.12f);
            var frontFacingScale = new Vector3(context.Scale.x, context.Scale.z, context.Scale.y);
            context.PartUsingScale("Scissor Blade Left", PrimitiveType.Cube, frontFacingScale,
                new Vector3(-0.18f, 0.32f, 0f),
                new Vector3(0.18f, 1.06f, 0.22f), steel, new Vector3(0f, 0f, -15f));
            context.PartUsingScale("Scissor Blade Right", PrimitiveType.Cube, frontFacingScale,
                new Vector3(0.18f, 0.32f, 0f),
                new Vector3(0.18f, 1.06f, 0.22f), steel, new Vector3(0f, 0f, 15f));
            context.PartUsingScale("Scissor Pivot", PrimitiveType.Cylinder, frontFacingScale,
                new Vector3(0f, -0.08f, -0.02f),
                new Vector3(0.28f, 0.10f, 0.28f), new Color(0.18f, 0.21f, 0.25f),
                new Vector3(90f, 0f, 0f));
            context.PartUsingScale("Scissor Handle Left", PrimitiveType.Sphere, frontFacingScale,
                new Vector3(-0.30f, -0.55f, 0f),
                new Vector3(0.56f, 0.52f, 0.25f), handle);
            context.PartUsingScale("Scissor Handle Right", PrimitiveType.Sphere, frontFacingScale,
                new Vector3(0.30f, -0.55f, 0f),
                new Vector3(0.56f, 0.52f, 0.25f), handle);
            context.PartUsingScale("Handle Hole Left", PrimitiveType.Sphere, frontFacingScale,
                new Vector3(-0.30f, -0.55f, -0.14f),
                new Vector3(0.28f, 0.26f, 0.08f), new Color(0.05f, 0.06f, 0.08f));
            context.PartUsingScale("Handle Hole Right", PrimitiveType.Sphere, frontFacingScale,
                new Vector3(0.30f, -0.55f, -0.14f),
                new Vector3(0.28f, 0.26f, 0.08f), new Color(0.05f, 0.06f, 0.08f));
        }

        private static void BuildSponge(BuildContext context)
        {
            context.Part("Sponge Body", PrimitiveType.Cube, new Vector3(0f, -0.08f, 0f),
                new Vector3(0.94f, 0.70f, 0.92f), new Color(1f, 0.82f, 0.05f));
            context.Part("Green Scrub Layer", PrimitiveType.Cube, new Vector3(0f, 0.36f, 0f),
                new Vector3(0.98f, 0.18f, 0.96f), new Color(0.08f, 0.46f, 0.22f));
            var poreColor = new Color(0.76f, 0.53f, 0.02f);
            context.Part("Sponge Pore 1", PrimitiveType.Sphere, new Vector3(-0.28f, -0.07f, -0.48f),
                new Vector3(0.15f, 0.15f, 0.06f), poreColor);
            context.Part("Sponge Pore 2", PrimitiveType.Sphere, new Vector3(0.05f, 0.08f, -0.48f),
                new Vector3(0.11f, 0.11f, 0.06f), poreColor);
            context.Part("Sponge Pore 3", PrimitiveType.Sphere, new Vector3(0.30f, -0.16f, -0.48f),
                new Vector3(0.14f, 0.14f, 0.06f), poreColor);
        }

        private static void BuildFlashlight(BuildContext context)
        {
            var dark = new Color(0.08f, 0.11f, 0.17f);
            context.Part("Flashlight Grip", PrimitiveType.Cylinder, new Vector3(0f, -0.22f, 0f),
                new Vector3(0.72f, 0.68f, 0.72f), dark);
            context.Part("Flashlight Head", PrimitiveType.Cylinder, new Vector3(0f, 0.48f, 0f),
                new Vector3(1.04f, 0.28f, 1.04f), new Color(0.16f, 0.20f, 0.29f));
            context.Part("Flashlight Lens", PrimitiveType.Cylinder, new Vector3(0f, 0.73f, 0f),
                new Vector3(0.86f, 0.08f, 0.86f), new Color(1f, 0.90f, 0.46f, 0.72f),
                Vector3.zero, true);
            context.Part("Flashlight Button", PrimitiveType.Cube, new Vector3(0f, 0.05f, -0.39f),
                new Vector3(0.26f, 0.18f, 0.12f), new Color(0.92f, 0.18f, 0.10f));

            var beamObject = new GameObject("Flashlight Beam");
            beamObject.transform.SetParent(context.Root, false);
            beamObject.transform.localPosition = Vector3.Scale(new Vector3(0f, 1.02f, 0f), context.Scale);
            beamObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var light = beamObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 7f;
            light.spotAngle = 42f;
            light.intensity = 5f;
            light.color = new Color(1f, 0.92f, 0.68f);
            context.CountExtraPart();
        }

        private static void BuildIdeaObject(BuildContext context, Color color)
        {
            context.Part("Idea Parcel", PrimitiveType.Cube, Vector3.zero,
                new Vector3(0.90f, 0.82f, 0.90f), color);
            context.Part("Idea Ribbon Vertical", PrimitiveType.Cube, new Vector3(0f, 0f, -0.47f),
                new Vector3(0.18f, 0.84f, 0.05f), new Color(1f, 0.78f, 0.15f));
            context.Part("Idea Ribbon Horizontal", PrimitiveType.Cube, new Vector3(0f, 0f, -0.48f),
                new Vector3(0.92f, 0.16f, 0.05f), new Color(1f, 0.78f, 0.15f));
            context.Part("Idea Bubble", PrimitiveType.Sphere, new Vector3(0f, 0.72f, 0f),
                new Vector3(0.58f, 0.58f, 0.58f), new Color(0.96f, 0.97f, 1f, 0.88f),
                Vector3.zero, true);
            context.Part("Question Dot", PrimitiveType.Sphere, new Vector3(0f, 0.64f, -0.31f),
                new Vector3(0.13f, 0.13f, 0.08f), new Color(0.30f, 0.14f, 0.54f));
            context.Part("Question Stem", PrimitiveType.Capsule, new Vector3(0f, 0.84f, -0.31f),
                new Vector3(0.12f, 0.24f, 0.08f), new Color(0.30f, 0.14f, 0.54f),
                new Vector3(0f, 0f, -24f));
        }

        private sealed class BuildContext
        {
            public BuildContext(Transform root, Vector3 scale)
            {
                Root = root;
                Scale = scale;
            }

            public Transform Root { get; }
            public Vector3 Scale { get; }
            public int PartCount { get; private set; }

            public void Part(
                string partName,
                PrimitiveType primitive,
                Vector3 normalizedPosition,
                Vector3 normalizedScale,
                Color color,
                Vector3 normalizedEuler = default,
                bool transparent = false)
            {
                PartUsingScale(
                    partName,
                    primitive,
                    Scale,
                    normalizedPosition,
                    normalizedScale,
                    color,
                    normalizedEuler,
                    transparent);
            }

            public void PartUsingScale(
                string partName,
                PrimitiveType primitive,
                Vector3 layoutScale,
                Vector3 normalizedPosition,
                Vector3 normalizedScale,
                Color color,
                Vector3 normalizedEuler = default,
                bool transparent = false)
            {
                var part = GameObject.CreatePrimitive(primitive);
                part.name = partName;
                part.transform.SetParent(Root, false);
                part.transform.localPosition = Vector3.Scale(normalizedPosition, layoutScale);
                part.transform.localScale = Vector3.Scale(normalizedScale, layoutScale);
                part.transform.localRotation = Quaternion.Euler(normalizedEuler);
                part.GetComponent<Renderer>().sharedMaterial = transparent
                    ? PlaceholderMaterials.CreateTransparent(color)
                    : PlaceholderMaterials.CreateOpaque(color);

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

                PartCount++;
            }

            public void CountExtraPart()
            {
                PartCount++;
            }
        }
    }
}
