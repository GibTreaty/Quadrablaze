using UnityEngine;

public abstract class NetworkedEffectBehaviour : MonoBehaviour {
    public abstract void OnPlayEffect(string[] parameters);
}