using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenController : MonoBehaviour {

    [SerializeField]
    string _nextSceneName = "";

    [SerializeField]
    string _skipToScene = "";

    [SerializeField]
    string _playerPrefsKey = "SkipSplashScreen";

    [SerializeField]
    bool _canSkip = true;

    [SerializeField]
    bool _useSkipToScene;

    [SerializeField]
    bool _canRestart = false;

    void Awake() {
        bool skipPreference = _canSkip && (PlayerPrefs.GetInt(_playerPrefsKey, 0) != 0);

        if(skipPreference) {
            if(_useSkipToScene)
                LoadSkipToScene();
            else
                Skip();
        }
        else if(_useSkipToScene)
            LoadScene();
    }

    void Update() {
        if(_canRestart && Input.GetKeyDown(KeyCode.R)) {
            Animator animator = GetComponentInChildren<Animator>();
            animator.Play("Icon Fade", -1, 0);
            animator.speed = 1;
        }

        if(_canSkip && Input.anyKeyDown) {
            Skip();

            if(!string.IsNullOrEmpty(_playerPrefsKey)) {
                PlayerPrefs.SetInt(_playerPrefsKey, 1);
                PlayerPrefs.Save();
            }
        }
    }

    public void LoadScene() {
        SceneManager.LoadScene(_nextSceneName);
    }

    public void LoadSkipToScene() {
        SceneManager.LoadScene(_skipToScene);
    }

    [ContextMenu("Delete Skip Preference")]
    public void DeletePref() {
        PlayerPrefs.DeleteKey(_playerPrefsKey);
        PlayerPrefs.Save();
    }

    void Skip() {
        var animator = GetComponentInChildren<Animator>();

        if(animator) animator.speed = 20;
    }
}