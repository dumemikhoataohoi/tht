// Extends UnityEngineShim.cs to cover the slice of UnityEditor / scene-management /
// Unity Test Framework API used by Assets/_Project/Editor/*.cs and
// Assets/Tests/PlayMode/*.cs. Same purpose and same limitations as
// UnityEngineShim.cs — see the comment at the top of that file. This is NOT the real
// Unity Editor and cannot verify actual asset/scene creation or PlayMode execution.
using System;
using System.Collections;

namespace UnityEngine.SceneManagement
{
    public enum LoadSceneMode
    {
        Single, Additive
    }

    public struct Scene
    {
        public string name;
    }

    public static class SceneManager
    {
        public static void LoadScene(string sceneName) { }
        public static void LoadScene(string sceneName, LoadSceneMode mode) { }
        public static Scene GetActiveScene() => default;
    }
}

namespace UnityEditor
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MenuItem : Attribute
    {
        public MenuItem(string itemName) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InitializeOnLoadAttribute : Attribute
    {
    }

    public static class AssetDatabase
    {
        public static T LoadAssetAtPath<T>(string path) where T : class => null;
        public static void CreateAsset(UnityEngine.Object asset, string path) { }
        public static void SaveAssets() { }
        public static void Refresh() { }
        public static bool IsValidFolder(string path) => true;
        public static string CreateFolder(string parentFolder, string newFolderName) => parentFolder + "/" + newFolderName;
    }

    public sealed class EditorBuildSettingsScene
    {
        public string path { get; }
        public bool enabled { get; }
        public EditorBuildSettingsScene(string path, bool enabled) { this.path = path; this.enabled = enabled; }
    }

    public static class EditorBuildSettings
    {
        public static EditorBuildSettingsScene[] scenes { get; set; } = new EditorBuildSettingsScene[0];
    }

    public static class EditorApplication
    {
        public static Action delayCall;
    }

    public static class Selection
    {
        public static UnityEngine.Object activeObject { get; set; }
    }

    public sealed class SerializedObject
    {
        public SerializedObject(UnityEngine.Object obj) { }
        public SerializedProperty FindProperty(string propertyPath) => new SerializedProperty();
        public void ApplyModifiedPropertiesWithoutUndo() { }
    }

    public sealed class SerializedProperty
    {
        public UnityEngine.Object objectReferenceValue { get; set; }
    }

    public sealed class SceneAsset : UnityEngine.Object
    {
    }
}

namespace UnityEditor.SceneManagement
{
    public enum NewSceneSetup
    {
        EmptyScene, DefaultGameObjects
    }

    public enum NewSceneMode
    {
        Single, Additive
    }

    public static class EditorSceneManager
    {
        public static UnityEditor.SceneAsset playModeStartScene { get; set; }
        public static UnityEngine.SceneManagement.Scene NewScene(NewSceneSetup setup, NewSceneMode mode) => default;
        public static bool SaveScene(UnityEngine.SceneManagement.Scene scene, string path) => true;
    }
}

namespace UnityEngine.TestTools
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class UnityTestAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class UnityTearDownAttribute : Attribute
    {
    }
}
