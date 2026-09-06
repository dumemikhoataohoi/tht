using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GemGrid.EditorTools
{
    /// <summary>
    /// Runs automatically once Unity finishes compiling scripts, so the project is
    /// guaranteed to have working data assets/scenes even if the "GemGrid" top-level
    /// menu is somehow not visible/discoverable in a given Editor session — the
    /// gameplay-critical setup no longer depends on anyone finding or clicking a menu
    /// item. Idempotent: every step checks for existing assets/scenes/Build Settings
    /// entries first and never overwrites — safe to run on every domain reload.
    ///
    /// The <c>GemGrid ▸ Setup ▸ ...</c> menu items (see <see cref="GemGridAssetSetup"/>,
    /// <see cref="GemGridSceneSetup"/>) still exist for manual/explicit re-runs.
    ///
    /// NOT run in this environment (no Unity Editor available) — see UNITY_SETUP.md.
    /// </summary>
    [InitializeOnLoad]
    public static class GemGridAutoSetup
    {
        static GemGridAutoSetup()
        {
            // Defer past the current domain-reload callback — creating assets/scenes
            // directly inside a static constructor is unsafe timing in Unity.
            EditorApplication.delayCall += RunOnce;
        }

        private static void RunOnce()
        {
            EditorApplication.delayCall -= RunOnce;

            try
            {
                GemGridAssetSetup.EnsureBlockShapeSet();
                GemGridAssetSetup.EnsureGameplayConfig();
                GemGridSceneSetup.EnsureBootScene();
                GemGridSceneSetup.EnsureGameplayScene();
                EnsureBuildSettingsScenes();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GemGrid] Automatic setup failed — you can still run it manually via " +
                                $"GemGrid ▸ Setup ▸ 0/5, or fix the error below and it will retry on next compile.\n{ex}");
            }
        }

        private static void EnsureBuildSettingsScenes()
        {
            bool hasBoot = EditorBuildSettings.scenes.Any(s => s.path == GemGridSceneSetup.BootScenePath);
            bool hasGameplay = EditorBuildSettings.scenes.Any(s => s.path == GemGridSceneSetup.GameplayScenePath);
            if (hasBoot && hasGameplay) return;

            var scenes = EditorBuildSettings.scenes.ToList();
            if (!hasBoot)
                scenes.Insert(0, new EditorBuildSettingsScene(GemGridSceneSetup.BootScenePath, true));
            if (!hasGameplay)
                scenes.Add(new EditorBuildSettingsScene(GemGridSceneSetup.GameplayScenePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[GemGrid] Added Boot/Gameplay scenes to Build Settings (Scenes In Build).");
        }
    }
}
