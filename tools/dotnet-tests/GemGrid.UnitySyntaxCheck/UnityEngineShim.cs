// Hand-written shim of the small slice of the UnityEngine API surface that
// M1's Unity-adapter files (GameManagerBehaviour, GridController,
// BlockDragController, HapticHookListener, GameplayAnimationHooks,
// AudioHookListener, BlockShapeSet, GameplayConfig) reference.
//
// PURPOSE: catch typos, wrong member names/signatures, and structural C# errors
// in those files by actually compiling them, since the real Unity Editor/UnityEngine
// assemblies are not available in this sandbox. This is NOT the real UnityEngine —
// it does not reproduce real engine behavior (rendering, physics, input, object
// lifecycle) and cannot prove the code runs correctly in an actual Unity project.
// See README_M1.md for exactly what this does and does not verify.
using System;

namespace UnityEngine
{
    public class Object
    {
        public string name;
        public static void Destroy(Object obj) { }
        public static T FindObjectOfType<T>() where T : class => null;
    }

    public class Component : Object
    {
        public Transform transform { get; internal set; }
        public GameObject gameObject { get; internal set; }
        public string tag { get; set; }
        public T GetComponent<T>() where T : class => default;
    }

    public class Behaviour : Component
    {
        public bool enabled { get; set; }
    }

    public class MonoBehaviour : Behaviour
    {
        public static void DontDestroyOnLoad(Object obj) { }
        public object StartCoroutine(System.Collections.IEnumerator routine) => null;
    }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject, new() => new T();
    }

    public sealed class GameObject : Object
    {
        public Transform transform { get; }
        public string tag { get; set; }
        public bool activeSelf { get; private set; } = true;
        public GameObject() { }
        public GameObject(string name) { this.name = name; }
        public T AddComponent<T>() where T : Component, new() => new T();
        public T GetComponent<T>() where T : class => default;
        public void SetActive(bool active) => activeSelf = active;
    }

    // Real UnityEngine.Transform is itself a Component (and RectTransform extends
    // Transform) — mirrored here so RectTransform can be added via AddComponent<T>().
    public class Transform : Component
    {
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }
        public Vector3 localScale { get; set; }
        public void SetParent(Transform parent, bool worldPositionStays) { }
    }

    public sealed class RectTransform : Transform
    {
        public Vector2 anchorMin { get; set; }
        public Vector2 anchorMax { get; set; }
        public Vector2 pivot { get; set; }
        public Vector2 sizeDelta { get; set; }
        public Vector2 anchoredPosition { get; set; }
        public Vector2 offsetMin { get; set; }
        public Vector2 offsetMax { get; set; }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 one => new Vector3(1f, 1f, 1f);
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) =>
            new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
    }

    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => new Vector2(0f, 0f);
        public static Vector2 one => new Vector2(1f, 1f);

        // Real UnityEngine.Vector2 defines this implicit conversion (dropping/adding a
        // zero z component); several call sites in the codebase rely on it.
        public static implicit operator Vector3(Vector2 v) => new Vector3(v.x, v.y, 0f);
    }

    public class Collider2D : Component
    {
    }

    public sealed class BoxCollider2D : Collider2D
    {
        public Vector2 size { get; set; }
    }

    public struct Color
    {
        public Color(float r, float g, float b, float a = 1f) { }
        public static Color gray => default;
        public static Color cyan => default;
        public static Color white => default;
        public static Color Lerp(Color a, Color b, float t) => default;
    }

    public sealed class SpriteRenderer : Component
    {
        public Sprite sprite { get; set; }
        public Color color { get; set; }
    }

    public sealed class Sprite
    {
        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit) => new Sprite();
    }

    public struct Rect
    {
        public Rect(float x, float y, float width, float height) { }
    }

    public enum TextureFormat
    {
        RGBA32
    }

    public sealed class Texture2D
    {
        public Texture2D(int width, int height) { }
        public Texture2D(int width, int height, TextureFormat format, bool mipChain) { }
        public void SetPixel(int x, int y, Color color) { }
        public void Apply() { }
    }

    public enum CameraClearFlags
    {
        Skybox, SolidColor, Depth, Nothing
    }

    public sealed class Camera : Component
    {
        public static Camera main => default;
        public bool orthographic { get; set; }
        public float orthographicSize { get; set; }
        public CameraClearFlags clearFlags { get; set; }
        public Color backgroundColor { get; set; }
        public Vector3 ScreenToWorldPoint(Vector3 position) => position;
    }

    public sealed class Font : Object
    {
    }

    public static class Resources
    {
        public static T GetBuiltinResource<T>(string path) where T : Object => null;
    }

    public static class GUI
    {
        public static void Label(Rect position, string text) { }
        public static bool Button(Rect position, string text) => false;
    }

    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
        public static void LogError(object message) { }
    }

    public enum TouchPhase
    {
        Began, Moved, Stationary, Ended, Canceled
    }

    public struct Touch
    {
        public Vector2 position;
        public TouchPhase phase;
    }

    public static class Input
    {
        public static Vector3 mousePosition => default;
        public static int touchCount => 0;
        public static Touch GetTouch(int index) => default;
        public static bool GetMouseButtonUp(int button) => false;
    }

    public static class Mathf
    {
        public static int RoundToInt(float f) => (int)Math.Round(f, MidpointRounding.AwayFromZero);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Math.Clamp(t, 0f, 1f);
    }

    public static class Time
    {
        public static float deltaTime => 0f;
    }

    public static class PlayerPrefs
    {
        public static int GetInt(string key, int defaultValue = 0) => defaultValue;
        public static void SetInt(string key, int value) { }
        public static void Save() { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializeField : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CreateAssetMenuAttribute : Attribute
    {
        public string menuName { get; set; }
        public string fileName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequireComponent : Attribute
    {
        public RequireComponent(Type type) { }
    }
}

namespace UnityEngine.Events
{
    public class UnityEvent
    {
        public void Invoke() { }
        public void AddListener(Action call) { }
    }
}
