using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GameCoroutine : NetworkBehaviour {

    static GameCoroutine Current { get; set; }

    public static Coroutine BeginCoroutine(IEnumerator routine) {
        CreateIfNull();

        return Current.StartCoroutine(routine);
    }

    public static void EndCoroutine(IEnumerator routine) {
        if(Current == null) return;

        Debug.Log("EndCoroutine(" + routine.ToString() + ")");
        Current.StopCoroutine(routine);
    }
    public static void EndCoroutine(Coroutine routine) {
        if(Current == null) return;

        Current.StopCoroutine(routine);
    }

    static void CreateIfNull() {
        if(Current == null) {
            var gameObject = new GameObject("Game Coroutine", typeof(GameCoroutine));

            Current = gameObject.GetComponent<GameCoroutine>();
        }
    }
}