using GemGrid.Audio;
using GemGrid.Bootstrap;
using GemGrid.Configuration;
using GemGrid.Gameplay;
using GemGrid.Journey;
using GemGrid.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GemGrid.EditorTools
{
    /// <summary>
    /// Builds the Boot, Main Menu, and Gameplay scenes entirely through Unity's own
    /// scene/GameObject/component APIs — deliberately not hand-authored .unity YAML,
    /// since that could not be verified without a real Unity Editor (see README_M1.md /
    /// UNITY_SETUP.md for why). Requires the data assets from
    /// <see cref="GemGridAssetSetup"/> to already exist.
    ///
    /// Exposes idempotent <c>Ensure...</c> methods (skip + return early if the scene
    /// file already exists — never silently overwrites manual edits) alongside menu
    /// items for manual/explicit use. Used by <see cref="GemGridAutoSetup"/>.
    ///
    /// UI here is placeholder-quality (flat colors, legacy uGUI Text) per M2 scope —
    /// functional and responsive, not final art.
    ///
    /// NOT run in this environment (no Unity Editor available) — see UNITY_SETUP.md.
    /// </summary>
    public static class GemGridSceneSetup
    {
        internal const string ScenesFolder = "Assets/_Project/Scenes";
        internal const string BootScenePath = ScenesFolder + "/Boot.unity";
        internal const string MainMenuScenePath = ScenesFolder + "/MainMenu.unity";
        internal const string GameplayScenePath = ScenesFolder + "/Gameplay.unity";
        internal const string JourneyMapScenePath = ScenesFolder + "/JourneyMap.unity";

        private static readonly Vector2 ReferenceResolution = new Vector2(1080, 1920);
        private static readonly Color BackgroundColor = GemPalette.Background;
        private static readonly Color AccentColor = GemPalette.Accent;
        private static readonly Color TextColor = GemPalette.TextPrimary;

        /// <summary>Creates Boot.unity if it doesn't already exist. Returns true if it was (or already is) present.</summary>
        internal static bool EnsureBootScene()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(BootScenePath) != null)
                return true;

            var blockShapeSet = GemGridAssetSetup.EnsureBlockShapeSet();
            var gameplayConfig = GemGridAssetSetup.EnsureGameplayConfig();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<AudioListener>();

            var bootstrapGo = new GameObject("Bootstrap");
            var gameManagerBehaviour = bootstrapGo.AddComponent<GameManagerBehaviour>();

            var serializedGameManager = new SerializedObject(gameManagerBehaviour);
            serializedGameManager.FindProperty("blockShapeSet").objectReferenceValue = blockShapeSet;
            serializedGameManager.FindProperty("gameplayConfig").objectReferenceValue = gameplayConfig;
            serializedGameManager.ApplyModifiedPropertiesWithoutUndo();

            bootstrapGo.AddComponent<BootLoader>();
            bootstrapGo.AddComponent<MetaProgressionService>();

            EnsureScenesFolder();
            EditorSceneManager.SaveScene(scene, BootScenePath);
            Debug.Log($"[GemGrid] Created {BootScenePath}.");
            return true;
        }

        /// <summary>Creates MainMenu.unity if it doesn't already exist. Returns true if it was (or already is) present.</summary>
        internal static bool EnsureMainMenuScene()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(MainMenuScenePath) != null)
                return true;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<AudioListener>();

            EnsureEventSystem();
            var canvas = CreateCanvas("MainMenuCanvas");

            CreateFullScreenBackground(canvas.transform, BackgroundColor);
            var safeArea = CreateSafeAreaContainer(canvas.transform);

            var title = CreateText(safeArea, "TitleText", "GemGrid", 96, TextColor);
            AnchorTopCenter(title.rectTransform, new Vector2(0f, -260f), new Vector2(800f, 140f));

            var subtitle = CreateText(safeArea, "SubtitleText", "Line up. Clear. Chain combos.", 36, GemPalette.TextSecondary);
            AnchorTopCenter(subtitle.rectTransform, new Vector2(0f, -360f), new Vector2(760f, 70f));

            var bestScoreText = CreateText(safeArea, "BestScoreText", "Best 0", 48, TextColor);
            AnchorCenter(bestScoreText.rectTransform, new Vector2(0f, 60f), new Vector2(600f, 80f));

            var startButton = CreateButton(safeArea, "StartGameButton", "START GAME", AccentColor);
            AnchorCenter(startButton.GetComponent<RectTransform>(), new Vector2(0f, -200f), new Vector2(560f, 130f));

            var journeyButton = CreateButton(safeArea, "JourneyButton", "JOURNEY", GemPalette.AccentWarm);
            AnchorCenter(journeyButton.GetComponent<RectTransform>(), new Vector2(0f, -350f), new Vector2(560f, 120f));

            var dailyButton = CreateButton(safeArea, "DailyChallengeButton", "DAILY CHALLENGE", GemPalette.Neutral);
            AnchorCenter(dailyButton.GetComponent<RectTransform>(), new Vector2(0f, -490f), new Vector2(560f, 110f));

            var controllerGo = new GameObject("MainMenuController");
            var controller = controllerGo.AddComponent<GemGrid.UI.MainMenuController>();
            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("bestScoreText").objectReferenceValue = bestScoreText;
            serializedController.FindProperty("startGameButton").objectReferenceValue = startButton;
            serializedController.FindProperty("journeyButton").objectReferenceValue = journeyButton;
            serializedController.FindProperty("dailyChallengeButton").objectReferenceValue = dailyButton;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EnsureScenesFolder();
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            Debug.Log($"[GemGrid] Created {MainMenuScenePath}.");
            return true;
        }

        /// <summary>
        /// Creates JourneyMap.unity if it doesn't already exist: a 3x5 grid of the 15
        /// Chapter 1 level tiles (see <see cref="GemGrid.UI.JourneyLevelButtonView"/>) plus
        /// a Back button. Only Chapter 1 is exposed in this milestone — later chapters are
        /// already fully generatable via ChapterBuilder, just not yet wired into this
        /// scene's UI (a follow-up, not a design limitation).
        /// </summary>
        internal static bool EnsureJourneyMapScene()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(JourneyMapScenePath) != null)
                return true;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<AudioListener>();

            EnsureEventSystem();
            var canvas = CreateCanvas("JourneyMapCanvas");
            CreateFullScreenBackground(canvas.transform, BackgroundColor);
            var safeArea = CreateSafeAreaContainer(canvas.transform);

            var title = CreateText(safeArea, "JourneyTitle", "JOURNEY — CHAPTER 1", 56, TextColor);
            AnchorTopCenter(title.rectTransform, new Vector2(0f, -110f), new Vector2(940f, 90f));

            const int columns = 3;
            float[] columnX = { -340f, 0f, 340f };
            float[] rowY = { 600f, 400f, 200f, 0f, -200f };

            for (int i = 0; i < LevelDefinition.LevelsPerChapter; i++)
            {
                int row = i / columns;
                int col = i % columns;

                var button = CreateButton(safeArea, $"Level{i + 1}Button", string.Empty, AccentColor);
                AnchorCenter(button.GetComponent<RectTransform>(), new Vector2(columnX[col], rowY[row]), new Vector2(280f, 160f));

                var numberText = CreateText(button.transform, "LevelNumber", (i + 1).ToString(), 44, TextColor);
                AnchorCenter(numberText.rectTransform, new Vector2(0f, 25f), new Vector2(240f, 60f));

                var statusText = CreateText(button.transform, "LevelStatus", "PLAY", 24, GemPalette.TextSecondary);
                AnchorCenter(statusText.rectTransform, new Vector2(0f, -35f), new Vector2(240f, 40f));

                var buttonView = button.gameObject.AddComponent<GemGrid.UI.JourneyLevelButtonView>();
                buttonView.Initialize(i, statusText);
            }

            var backButton = CreateButton(safeArea, "JourneyBackButton", "BACK", GemPalette.Neutral);
            AnchorBottomCenter(backButton.GetComponent<RectTransform>(), new Vector2(0f, 50f), new Vector2(400f, 110f));
            backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

            EnsureScenesFolder();
            EditorSceneManager.SaveScene(scene, JourneyMapScenePath);
            Debug.Log($"[GemGrid] Created {JourneyMapScenePath}.");
            return true;
        }

        /// <summary>Creates Gameplay.unity if it doesn't already exist. Returns true if it was (or already is) present.</summary>
        internal static bool EnsureGameplayScene()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GameplayScenePath) != null)
                return true;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.transform.position = new Vector3(3.5f, 2.5f, -10f);
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<AudioListener>();

            var rootGo = new GameObject("GameplayRoot");
            rootGo.AddComponent<GameplaySessionStarter>();
            var gridController = rootGo.AddComponent<GridController>();
            var dragController = rootGo.AddComponent<BlockDragController>();
            var placementPreview = rootGo.AddComponent<PlacementPreviewController>();
            rootGo.AddComponent<HapticHookListener>();
            rootGo.AddComponent<GameplayAnimationHooks>();
            rootGo.AddComponent<AudioHookListener>();

            var serializedPlacementPreview = new SerializedObject(placementPreview);
            serializedPlacementPreview.FindProperty("gridController").objectReferenceValue = gridController;
            serializedPlacementPreview.ApplyModifiedPropertiesWithoutUndo();

            var serializedDragController = new SerializedObject(dragController);
            serializedDragController.FindProperty("gridController").objectReferenceValue = gridController;
            serializedDragController.FindProperty("placementPreview").objectReferenceValue = placementPreview;
            serializedDragController.ApplyModifiedPropertiesWithoutUndo();

            var trayGo = new GameObject("BlockTray");
            trayGo.transform.SetParent(rootGo.transform, false);
            var trayView = trayGo.AddComponent<BlockTrayView>();
            var serializedTrayView = new SerializedObject(trayView);
            serializedTrayView.FindProperty("dragController").objectReferenceValue = dragController;
            serializedTrayView.ApplyModifiedPropertiesWithoutUndo();

            EnsureEventSystem();
            var canvas = CreateCanvas("GameplayCanvas");
            var safeArea = CreateSafeAreaContainer(canvas.transform);
            BuildGameplayHud(safeArea);
            BuildJourneyHud(safeArea);
            BuildPowerUpBar(safeArea, placementPreview);
            BuildGameOverPanel(canvas.transform);
            BuildTutorialOverlay(canvas.transform);

            EnsureScenesFolder();
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
            Debug.Log($"[GemGrid] Created {GameplayScenePath}.");
            return true;
        }

        private static void BuildGameplayHud(Transform canvasTransform)
        {
            // Backdrop bar behind the score/best/combo text so it stays legible over any
            // board/tray color underneath — created first so it renders behind everything
            // else added to this same parent.
            var backdropGo = new GameObject("HudBackdrop");
            backdropGo.transform.SetParent(canvasTransform, false);
            var backdropRect = backdropGo.AddComponent<RectTransform>();
            backdropRect.anchorMin = new Vector2(0f, 1f);
            backdropRect.anchorMax = new Vector2(1f, 1f);
            backdropRect.pivot = new Vector2(0.5f, 1f);
            backdropRect.sizeDelta = new Vector2(0f, 180f);
            backdropRect.anchoredPosition = Vector2.zero;
            var backdropImage = backdropGo.AddComponent<Image>();
            backdropImage.color = new Color(GemPalette.Surface.r, GemPalette.Surface.g, GemPalette.Surface.b, 0.85f);

            var scoreText = CreateText(canvasTransform, "ScoreText", "Score 0", 56, TextColor);
            AnchorTopLeft(scoreText.rectTransform, new Vector2(40f, -50f), new Vector2(400f, 70f));

            var bestScoreText = CreateText(canvasTransform, "BestScoreText", "Best 0", 36, TextColor);
            AnchorTopLeft(bestScoreText.rectTransform, new Vector2(40f, -110f), new Vector2(400f, 50f));

            var comboBadgeGo = new GameObject("ComboBadge");
            comboBadgeGo.transform.SetParent(canvasTransform, false);
            var comboBadgeRect = comboBadgeGo.AddComponent<RectTransform>();
            AnchorTopCenter(comboBadgeRect, new Vector2(0f, -60f), new Vector2(320f, 70f));

            var comboText = CreateText(comboBadgeGo.transform, "ComboText", string.Empty, 44, new Color(1f, 0.85f, 0.3f));
            comboText.alignment = TextAnchor.MiddleCenter;
            var comboTextRect = comboText.rectTransform;
            comboTextRect.anchorMin = Vector2.zero;
            comboTextRect.anchorMax = Vector2.one;
            comboTextRect.offsetMin = Vector2.zero;
            comboTextRect.offsetMax = Vector2.zero;

            var hudGo = new GameObject("GameplayHud");
            var hud = hudGo.AddComponent<GameplayHud>();
            var serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("scoreText").objectReferenceValue = scoreText;
            serializedHud.FindProperty("bestScoreText").objectReferenceValue = bestScoreText;
            serializedHud.FindProperty("comboText").objectReferenceValue = comboText;
            serializedHud.FindProperty("comboBadge").objectReferenceValue = comboBadgeRect;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Journey-only strip (objective + progress, Gem Energy) — see
        /// <see cref="GemGrid.UI.JourneyHud"/>. Hides itself in Classic Mode. Placed just
        /// below the score/best/combo HUD bar so it never overlaps the board/tray below.
        /// </summary>
        private static void BuildJourneyHud(Transform canvasTransform)
        {
            var panelGo = new GameObject("JourneyHudPanel");
            panelGo.transform.SetParent(canvasTransform, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            AnchorTopCenter(panelRect, new Vector2(0f, -190f), new Vector2(1000f, 60f));
            var panelImage = panelGo.AddComponent<Image>();
            panelImage.color = new Color(GemPalette.Surface.r, GemPalette.Surface.g, GemPalette.Surface.b, 0.75f);

            var objectiveText = CreateText(panelGo.transform, "ObjectiveText", string.Empty, 30, TextColor);
            AnchorTopLeft(objectiveText.rectTransform, new Vector2(20f, -5f), new Vector2(700f, 50f));
            objectiveText.alignment = TextAnchor.MiddleLeft;

            var energyText = CreateText(panelGo.transform, "EnergyText", string.Empty, 30, GemPalette.Accent);
            AnchorTopLeft(energyText.rectTransform, new Vector2(740f, -5f), new Vector2(240f, 50f));
            energyText.alignment = TextAnchor.MiddleRight;

            var hudGo = new GameObject("JourneyHud");
            var hud = hudGo.AddComponent<GemGrid.UI.JourneyHud>();
            var serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("panelRoot").objectReferenceValue = panelGo;
            serializedHud.FindProperty("objectiveText").objectReferenceValue = objectiveText;
            serializedHud.FindProperty("energyText").objectReferenceValue = energyText;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Hint/Hammer/Shuffle/Undo buttons — see <see cref="GemGrid.UI.PowerUpBar"/>. Row anchored to the bottom of the screen, below the board/tray.</summary>
        private static void BuildPowerUpBar(Transform canvasTransform, PlacementPreviewController placementPreview)
        {
            var barGo = new GameObject("PowerUpBar");
            barGo.transform.SetParent(canvasTransform, false);
            var barRect = barGo.AddComponent<RectTransform>();
            AnchorBottomCenter(barRect, new Vector2(0f, 30f), new Vector2(1000f, 130f));

            var hintButton = CreatePowerUpButton(barRect, "HintButton", "HINT", new Vector2(-375f, 0f));
            var hammerButton = CreatePowerUpButton(barRect, "HammerButton", "HAMMER", new Vector2(-125f, 0f));
            var shuffleButton = CreatePowerUpButton(barRect, "ShuffleButton", "SHUFFLE", new Vector2(125f, 0f));
            var undoButton = CreatePowerUpButton(barRect, "UndoButton", "UNDO", new Vector2(375f, 0f));

            var controllerGo = new GameObject("PowerUpBarController");
            var controller = controllerGo.AddComponent<GemGrid.UI.PowerUpBar>();
            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("placementPreview").objectReferenceValue = placementPreview;
            serializedController.FindProperty("hintButton").objectReferenceValue = hintButton;
            serializedController.FindProperty("hammerButton").objectReferenceValue = hammerButton;
            serializedController.FindProperty("shuffleButton").objectReferenceValue = shuffleButton;
            serializedController.FindProperty("undoButton").objectReferenceValue = undoButton;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button CreatePowerUpButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var button = CreateButton(parent, name, label, GemPalette.SurfaceAlt);
            AnchorCenter(button.GetComponent<RectTransform>(), anchoredPosition, new Vector2(220f, 120f));
            return button;
        }

        private static void BuildGameOverPanel(Transform canvasTransform)
        {
            var panelGo = new GameObject("GameOverPanel");
            panelGo.transform.SetParent(canvasTransform, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var panelImage = panelGo.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.75f);
            var panelCanvasGroup = panelGo.AddComponent<CanvasGroup>();
            panelGo.SetActive(false);

            var safeArea = CreateSafeAreaContainer(panelGo.transform);

            // Card backdrop behind the title/scores/buttons, created first so it renders
            // behind them — gives Game Over a distinct "panel" presentation instead of
            // text floating directly over the dim overlay.
            var cardGo = new GameObject("GameOverCard");
            cardGo.transform.SetParent(safeArea, false);
            var cardRect = cardGo.AddComponent<RectTransform>();
            AnchorCenter(cardRect, new Vector2(0f, -10f), new Vector2(860f, 840f));
            var cardImage = cardGo.AddComponent<Image>();
            cardImage.color = GemPalette.Surface;

            var title = CreateText(safeArea, "GameOverTitle", "GAME OVER", 72, TextColor);
            AnchorCenter(title.rectTransform, new Vector2(0f, 300f), new Vector2(760f, 90f));

            // Explains WHY the game ended (Classic) or shows the Journey win/lose result
            // + stars (Journey/Daily) — set dynamically by GameOverScreen.cs.
            var reasonText = CreateText(safeArea, "GameOverReasonText", "No more valid moves.", 34, GemPalette.TextSecondary);
            AnchorCenter(reasonText.rectTransform, new Vector2(0f, 215f), new Vector2(700f, 50f));

            var finalScoreText = CreateText(safeArea, "FinalScoreText", "Score 0", 48, TextColor);
            AnchorCenter(finalScoreText.rectTransform, new Vector2(0f, 145f), new Vector2(600f, 55f));

            var bestScoreText = CreateText(safeArea, "BestScoreText", "Best 0", 36, TextColor);
            AnchorCenter(bestScoreText.rectTransform, new Vector2(0f, 85f), new Vector2(600f, 45f));

            // Journey-only reward summary ("+40 Coins  +10 XP") — hidden in Classic Mode.
            var rewardText = CreateText(safeArea, "JourneyRewardText", string.Empty, 30, GemPalette.TextCombo);
            AnchorCenter(rewardText.rectTransform, new Vector2(0f, 30f), new Vector2(600f, 40f));

            // Journey-only: Continue (shown on a loss, while still available) and Next
            // Level (shown on a win) share the same slot — never both at once.
            var continueButton = CreateButton(safeArea, "ContinueButton", "CONTINUE", GemPalette.AccentWarm);
            AnchorCenter(continueButton.GetComponent<RectTransform>(), new Vector2(0f, -55f), new Vector2(500f, 110f));
            continueButton.gameObject.SetActive(false);

            var nextLevelButton = CreateButton(safeArea, "NextLevelButton", "NEXT LEVEL", GemPalette.AccentWarm);
            AnchorCenter(nextLevelButton.GetComponent<RectTransform>(), new Vector2(0f, -55f), new Vector2(500f, 110f));
            nextLevelButton.gameObject.SetActive(false);

            var restartButton = CreateButton(safeArea, "RestartButton", "RESTART", AccentColor);
            AnchorCenter(restartButton.GetComponent<RectTransform>(), new Vector2(0f, -190f), new Vector2(500f, 100f));

            var mainMenuButton = CreateButton(safeArea, "MainMenuButton", "MAIN MENU", GemPalette.Neutral);
            AnchorCenter(mainMenuButton.GetComponent<RectTransform>(), new Vector2(0f, -330f), new Vector2(500f, 90f));

            var screenGo = new GameObject("GameOverScreen");
            var screen = screenGo.AddComponent<GameOverScreen>();
            var serializedScreen = new SerializedObject(screen);
            serializedScreen.FindProperty("panelRoot").objectReferenceValue = panelGo;
            serializedScreen.FindProperty("panelCanvasGroup").objectReferenceValue = panelCanvasGroup;
            serializedScreen.FindProperty("titleText").objectReferenceValue = title;
            serializedScreen.FindProperty("reasonOrResultText").objectReferenceValue = reasonText;
            serializedScreen.FindProperty("finalScoreText").objectReferenceValue = finalScoreText;
            serializedScreen.FindProperty("bestScoreText").objectReferenceValue = bestScoreText;
            serializedScreen.FindProperty("rewardText").objectReferenceValue = rewardText;
            serializedScreen.FindProperty("restartButton").objectReferenceValue = restartButton;
            serializedScreen.FindProperty("mainMenuButton").objectReferenceValue = mainMenuButton;
            serializedScreen.FindProperty("continueButton").objectReferenceValue = continueButton;
            serializedScreen.FindProperty("nextLevelButton").objectReferenceValue = nextLevelButton;
            serializedScreen.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// First-time-only "how to play" overlay (see <see cref="TutorialOverlay"/>) — a
        /// dim full-screen panel with a short rules card and a single dismiss button.
        /// Created last so it renders on top of the HUD/Game Over panel siblings.
        /// </summary>
        private static void BuildTutorialOverlay(Transform canvasTransform)
        {
            var panelGo = new GameObject("TutorialPanel");
            panelGo.transform.SetParent(canvasTransform, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var panelImage = panelGo.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.82f);
            panelGo.SetActive(false); // TutorialOverlay.Start() reactivates it iff not yet seen.

            var safeArea = CreateSafeAreaContainer(panelGo.transform);

            var cardGo = new GameObject("TutorialCard");
            cardGo.transform.SetParent(safeArea, false);
            var cardRect = cardGo.AddComponent<RectTransform>();
            AnchorCenter(cardRect, Vector2.zero, new Vector2(880f, 980f));
            var cardImage = cardGo.AddComponent<Image>();
            cardImage.color = GemPalette.Surface;

            var title = CreateText(safeArea, "TutorialTitle", "HOW TO PLAY", 60, TextColor);
            AnchorCenter(title.rectTransform, new Vector2(0f, 380f), new Vector2(760f, 100f));

            const string body =
                "Drag a block onto the board.\n\n" +
                "Fill a row or column to clear it.\n\n" +
                "Clear multiple lines for combo.\n\n" +
                "No valid move = Game Over.";
            var bodyText = CreateText(safeArea, "TutorialBodyText", body, 38, TextColor);
            AnchorCenter(bodyText.rectTransform, new Vector2(0f, 40f), new Vector2(760f, 560f));

            var dismissButton = CreateButton(safeArea, "TutorialDismissButton", "GOT IT", AccentColor);
            AnchorCenter(dismissButton.GetComponent<RectTransform>(), new Vector2(0f, -380f), new Vector2(420f, 130f));

            var overlayGo = new GameObject("TutorialOverlay");
            var overlay = overlayGo.AddComponent<TutorialOverlay>();
            var serializedOverlay = new SerializedObject(overlay);
            serializedOverlay.FindProperty("panelRoot").objectReferenceValue = panelGo;
            serializedOverlay.FindProperty("dismissButton").objectReferenceValue = dismissButton;
            serializedOverlay.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---- Reusable UI construction helpers (placeholder-quality, no external assets) ----

        private static Canvas CreateCanvas(string name)
        {
            var canvasGo = new GameObject(name);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        /// <summary>
        /// Full-stretch child of the Canvas fitted to <see cref="UnityEngine.Screen.safeArea"/>
        /// at runtime (see <see cref="GemGrid.UI.SafeAreaFitter"/>) — real screen content
        /// (text, buttons) is parented under this instead of directly under the Canvas so
        /// it never sits under a device notch/cutout/rounded corner. Full-screen
        /// backgrounds/dim overlays stay direct Canvas children since those should cover
        /// the whole screen regardless of the safe area.
        /// </summary>
        private static RectTransform CreateSafeAreaContainer(Transform parent)
        {
            var go = new GameObject("SafeArea");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.AddComponent<GemGrid.UI.SafeAreaFitter>();
            return rect;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();
        }

        private static void CreateFullScreenBackground(Transform parent, Color color)
        {
            var go = new GameObject("Background");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = go.AddComponent<Image>();
            image.color = color;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = DefaultFont.Get();
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            var image = go.AddComponent<Image>();
            image.color = color;

            var button = go.AddComponent<Button>();
            go.AddComponent<ButtonPunchFeedback>();
            go.AddComponent<GemGrid.Audio.ButtonClickSfx>();

            var label_ = CreateText(go.transform, "Label", label, 40, TextColor);
            var labelRect = label_.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return button;
        }

        private static void AnchorTopLeft(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
        }

        private static void AnchorTopCenter(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
        }

        private static void AnchorCenter(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
        }

        private static void AnchorBottomCenter(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
        }

        // ---- Menu items (manual/explicit re-runs; auto-setup covers the normal path) ----

        [MenuItem("GemGrid/Setup/3. Create Boot Scene")]
        public static void CreateBootScene()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(BootScenePath) != null)
            {
                Debug.LogWarning($"{BootScenePath} already exists — not overwriting. Delete it first if you want to regenerate.");
                return;
            }
            EnsureBootScene();
            Debug.Log("Add it to File ▸ Build Settings ▸ Scenes In Build (index 0) so it loads first — see UNITY_SETUP.md.");
        }

        [MenuItem("GemGrid/Setup/4. Create Main Menu Scene")]
        public static void CreateMainMenuScene()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(MainMenuScenePath) != null)
            {
                Debug.LogWarning($"{MainMenuScenePath} already exists — not overwriting. Delete it first if you want to regenerate.");
                return;
            }
            EnsureMainMenuScene();
        }

        [MenuItem("GemGrid/Setup/5. Create Gameplay Scene")]
        public static void CreateGameplayScene()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GameplayScenePath) != null)
            {
                Debug.LogWarning($"{GameplayScenePath} already exists — not overwriting. Delete it first if you want to regenerate.");
                return;
            }
            EnsureGameplayScene();
            Debug.Log("Add it to File ▸ Build Settings ▸ Scenes In Build (after Boot + MainMenu) — see UNITY_SETUP.md.");
        }

        [MenuItem("GemGrid/Setup/6. Create Journey Map Scene")]
        public static void CreateJourneyMapScene()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(JourneyMapScenePath) != null)
            {
                Debug.LogWarning($"{JourneyMapScenePath} already exists — not overwriting. Delete it first if you want to regenerate.");
                return;
            }
            EnsureJourneyMapScene();
            Debug.Log("Add it to File ▸ Build Settings ▸ Scenes In Build — see UNITY_SETUP.md.");
        }

        [MenuItem("GemGrid/Setup/7. Create Boot, Main Menu, Gameplay And Journey Map Scenes")]
        public static void CreateAllScenes()
        {
            CreateBootScene();
            CreateMainMenuScene();
            CreateGameplayScene();
            CreateJourneyMapScene();
        }

        private static void EnsureScenesFolder()
        {
            if (AssetDatabase.IsValidFolder(ScenesFolder)) return;

            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
            {
                Debug.LogError("Expected folder 'Assets/_Project' to already exist in the project.");
                return;
            }

            AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
        }
    }
}
