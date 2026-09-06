using System;
using System.Collections.Generic;
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
                GemGridSceneSetup.EnsureMainMenuScene();
                GemGridSceneSetup.EnsureGameplayScene();
                EnsureBuildSettingsScenes();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GemGrid] Automatic setup failed — you can still run it manually via " +
                                $"GemGrid ▸ Setup ▸ 0/6, or fix the error below and it will retry on next compile.\n{ex}");
            }
        }

        private static void EnsureBuildSettingsScenes()
        {
            string[] canonicalOrder =
            {
                GemGridSceneSetup.BootScenePath,
                GemGridSceneSetup.MainMenuScenePath,
                GemGridSceneSetup.GameplayScenePath,
            };

            var current = EditorBuildSettings.scenes;
            bool alreadyCorrect = current.Length >= canonicalOrder.Length
                                  && canonicalOrder.SequenceEqual(current.Take(canonicalOrder.Length).Select(s => s.path));
            if (alreadyCorrect) return;

            // Keep any other scenes the project already had, appended after the three
            // canonical ones, so this never silently discards unrelated Build Settings entries.
            var extras = current.Where(s => !canonicalOrder.Contains(s.path)).ToList();

            var newScenes = new List<EditorBuildSettingsScene>();
            foreach (var path in canonicalOrder)
                newScenes.Add(new EditorBuildSettingsScene(path, true));
            newScenes.AddRange(extras);

            EditorBuildSettings.scenes = newScenes.ToArray();
            Debug.Log("[GemGrid] Set Build Settings scene order: Boot, MainMenu, Gameplay.");
        }
    }
}
