using System.Collections.Generic;
using GemGrid.Configuration;
using GemGrid.Core;
using UnityEditor;
using UnityEngine;

namespace GemGrid.EditorTools
{
    /// <summary>
    /// Creates the ScriptableObject data assets GemGrid needs to run, so opening the
    /// project in Unity never requires hand-editing .asset YAML (per GAME_DESIGN.md /
    /// ECONOMY_DESIGN.md: shapes and score/combo tuning are data-driven, not
    /// hard-coded). Starter shapes mirror the table already published in README_M1.md.
    ///
    /// Exposes idempotent <c>Ensure...</c> methods (safe to call every time scripts
    /// compile — used by <see cref="GemGridAutoSetup"/>) alongside menu items for
    /// manual/explicit use.
    ///
    /// NOT run in this environment (no Unity Editor available) — see UNITY_SETUP.md.
    /// </summary>
    public static class GemGridAssetSetup
    {
        internal const string AssetFolder = "Assets/_Project/ScriptableObjects";
        internal const string BlockShapeSetPath = AssetFolder + "/BlockShapeSet.asset";
        internal const string GameplayConfigPath = AssetFolder + "/GameplayConfig.asset";

        /// <summary>Returns the existing asset, or creates it with starter shapes if missing. Never overwrites.</summary>
        internal static BlockShapeSet EnsureBlockShapeSet()
        {
            var existing = AssetDatabase.LoadAssetAtPath<BlockShapeSet>(BlockShapeSetPath);
            if (existing != null) return existing;

            EnsureScriptableObjectFolder();

            var set = ScriptableObject.CreateInstance<BlockShapeSet>();
            set.EditorSetShapes(BuildStarterShapes());

            AssetDatabase.CreateAsset(set, BlockShapeSetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[GemGrid] Created {BlockShapeSetPath} with {set.Shapes.Count} starter shapes. " +
                       "Edit/add shapes in the Inspector as needed — this is a starting point, not final balance.");
            return set;
        }

        /// <summary>Returns the existing asset, or creates it with default rules if missing. Never overwrites.</summary>
        internal static GameplayConfig EnsureGameplayConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameplayConfig>(GameplayConfigPath);
            if (existing != null) return existing;

            EnsureScriptableObjectFolder();

            var config = ScriptableObject.CreateInstance<GameplayConfig>();
            AssetDatabase.CreateAsset(config, GameplayConfigPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[GemGrid] Created {GameplayConfigPath} with default ScoreRules/ComboRules " +
                       "(see ECONOMY_DESIGN.md for tuning guidance).");
            return config;
        }

        [MenuItem("GemGrid/Setup/1. Create Starter Block Shape Set")]
        public static void CreateBlockShapeSet()
        {
            bool alreadyExisted = AssetDatabase.LoadAssetAtPath<BlockShapeSet>(BlockShapeSetPath) != null;
            var set = EnsureBlockShapeSet();
            if (alreadyExisted)
                Debug.LogWarning($"{BlockShapeSetPath} already exists — not overwriting. Delete it first if you want to regenerate.");
            Selection.activeObject = set;
        }

        [MenuItem("GemGrid/Setup/2. Create Default Gameplay Config")]
        public static void CreateGameplayConfig()
        {
            bool alreadyExisted = AssetDatabase.LoadAssetAtPath<GameplayConfig>(GameplayConfigPath) != null;
            var config = EnsureGameplayConfig();
            if (alreadyExisted)
                Debug.LogWarning($"{GameplayConfigPath} already exists — not overwriting. Delete it first if you want to regenerate.");
            Selection.activeObject = config;
        }

        [MenuItem("GemGrid/Setup/0. Create All Required Data Assets")]
        public static void CreateAllDataAssets()
        {
            CreateBlockShapeSet();
            CreateGameplayConfig();
        }

        private static void EnsureScriptableObjectFolder()
        {
            if (AssetDatabase.IsValidFolder(AssetFolder)) return;

            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
            {
                Debug.LogError("Expected folder 'Assets/_Project' to already exist in the project.");
                return;
            }

            AssetDatabase.CreateFolder("Assets/_Project", "ScriptableObjects");
        }

        private static List<BlockShapeDefinition> BuildStarterShapes()
        {
            return new List<BlockShapeDefinition>
            {
                new BlockShapeDefinition("dot", new[] { new Int2(0, 0) }, spawnWeight: 2),
                new BlockShapeDefinition("domino_h", new[] { new Int2(0, 0), new Int2(1, 0) }, spawnWeight: 4),
                new BlockShapeDefinition("domino_v", new[] { new Int2(0, 0), new Int2(0, 1) }, spawnWeight: 4),
                new BlockShapeDefinition("tromino_l", new[] { new Int2(0, 0), new Int2(1, 0), new Int2(0, 1) }, spawnWeight: 3),
                new BlockShapeDefinition("square", new[] { new Int2(0, 0), new Int2(1, 0), new Int2(0, 1), new Int2(1, 1) }, spawnWeight: 2),
                new BlockShapeDefinition("line3_h", new[] { new Int2(0, 0), new Int2(1, 0), new Int2(2, 0) }, spawnWeight: 3),
                new BlockShapeDefinition("line3_v", new[] { new Int2(0, 0), new Int2(0, 1), new Int2(0, 2) }, spawnWeight: 3),
            };
        }
    }
}
