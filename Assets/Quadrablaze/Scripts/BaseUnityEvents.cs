using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class BooleanEvent : UnityEvent<bool> { }

[Serializable]
public class IntEvent : UnityEvent<int> { }

[Serializable]
public class UIntEvent : UnityEvent<uint> { }

[Serializable]
public class FloatEvent : UnityEvent<float> { }

[Serializable]
public class StringEvent : UnityEvent<string> { }

[Serializable]
public class Vector2Event : UnityEvent<Vector2> { }

[Serializable]
public class Vector3Event : UnityEvent<Vector3> { }

[Serializable]
public class Vector4Event : UnityEvent<Vector4> { }

[Serializable]
public class ColliderEvent : UnityEvent<Collider> { }

[Serializable]
public class TransformEvent : UnityEvent<Transform> { }

[Serializable]
public class GameObjectEvent : UnityEvent<GameObject> { }

[Serializable]
public class MonoBehaviourEvent : UnityEvent<MonoBehaviour> { }

public static class UnityEventHelper {
    public static void InvokeEvent(this UnityEvent action) {
        if(action != null) action.Invoke();
    }
    public static void InvokeEvent<T>(this UnityEvent<T> action, T parameter) {
        if(action != null) action.Invoke(parameter);
    }
}