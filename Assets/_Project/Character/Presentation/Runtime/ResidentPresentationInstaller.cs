using System;
using HumanGlassWatcher.Character.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HumanGlassWatcher.Character.Presentation
{
    public static class ResidentPresentationInstaller
    {
        public const string GameplayTargetName = "Resident Target - Juniper";
        public const float StandingLocalYOffset = 1.4f;
        private const int DefaultResidentSeed = 1729;

        public static event Action<ResidentPresentationController> Installed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SubscribeToScenes()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallAfterInitialScene()
        {
            TryInstallInScene(SceneManager.GetActiveScene());
        }

        public static ResidentPresentationController Install(
            GameObject gameplayTarget,
            ResidentState state = null,
            ResidentAppearance appearance = null)
        {
            if (gameplayTarget == null)
            {
                throw new ArgumentNullException(nameof(gameplayTarget));
            }

            var existing = gameplayTarget.GetComponentInChildren<ResidentPresentationController>(true);
            if (existing != null)
            {
                if (state != null)
                {
                    existing.Bind(state);
                }

                return existing;
            }

            HideGrayboxVisual(gameplayTarget);
            var rig = ResidentPresentationFactory.Build(gameplayTarget.transform, appearance);
            rig.transform.localPosition = new Vector3(0f, StandingLocalYOffset, 0f);
            var controller = rig.gameObject.AddComponent<ResidentPresentationController>();
            controller.Initialize(
                rig,
                state ?? ResidentState.Create(DefaultResidentSeed, "juniper"));
            Installed?.Invoke(controller);
            return controller;
        }

        public static ResidentPresentationController TryInstallInScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return null;
            }

            var roots = scene.GetRootGameObjects();
            for (var index = 0; index < roots.Length; index++)
            {
                var target = FindNamed(roots[index].transform, GameplayTargetName);
                if (target != null)
                {
                    return Install(target.gameObject);
                }
            }

            return null;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryInstallInScene(scene);
        }

        private static Transform FindNamed(Transform root, string name)
        {
            if (string.Equals(root.name, name, StringComparison.Ordinal))
            {
                return root;
            }

            for (var index = 0; index < root.childCount; index++)
            {
                var match = FindNamed(root.GetChild(index), name);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static void HideGrayboxVisual(GameObject target)
        {
            var placeholderRenderer = target.GetComponent<Renderer>();
            if (placeholderRenderer != null)
            {
                placeholderRenderer.enabled = false;
            }

            var facingMarker = target.transform.Find("Facing Marker");
            if (facingMarker == null)
            {
                return;
            }

            var markerRenderers = facingMarker.GetComponentsInChildren<Renderer>(true);
            for (var index = 0; index < markerRenderers.Length; index++)
            {
                markerRenderers[index].enabled = false;
            }
        }
    }
}
