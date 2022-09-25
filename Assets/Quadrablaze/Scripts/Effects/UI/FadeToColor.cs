using UnityEngine;
using UnityEngine.UI;

public class FadeToColor : MonoBehaviour {

    [SerializeField]
    Image _image;

    [SerializeField]
    float _fadeTime = 1;

    public void FadeAlpha(float alpha) {
        _image.CrossFadeAlpha(alpha, _fadeTime, true);
    }

    public void FadeColor(Color color) {
        _image.CrossFadeColor(color, _fadeTime, true, false);
    }
}