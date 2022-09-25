using UnityEngine;

public class QuitApplication : MonoBehaviour {

	public void Quit() {
        QuitApp();
    }

    public static void QuitApp() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}