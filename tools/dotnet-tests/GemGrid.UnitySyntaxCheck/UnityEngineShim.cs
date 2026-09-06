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
    }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject, new() => new T();
    }

    public sealed class GameObject : Object
    {
        public Transform transform { get; }
        public string tag { get; set; }
        public GameObject() { }
        public GameObject(string name) { this.name = name; }
        public T AddComponent<T>() where T : Component, new() => new T();
        public T GetComponent<T>() where T : class => default;
    }

    public sealed class Transform
    {
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }
        public void SetParent(Transform parent, bool worldPositionStays) { }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public struct Vector2
    {
        public float x, y;

        // Real UnityEngine.Vector2 defines this implicit conversion (dropping/adding a
        // zero z component); several call sites in the codebase rely on it.
        public static implicit operator Vector3(Vector2 v) => new Vector3(v.x, v.y, 0f);
    }

    public struct Color
    {
        public static Color gray => default;
        public static Color cyan => default;
    }

    public sealed class SpriteRenderer : Component
    {
        public Sprite sprite { get; set; }
        public Color color { get; set; }
    }

    public sealed class Sprite
    {
    }

    public sealed class Camera : Component
    {
        public static Camera main => default;
        public bool orthographic { get; set; }
        public float orthographicSize { get; set; }
        public Vector3 ScreenToWorldPoint(Vector3 position) => position;
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
