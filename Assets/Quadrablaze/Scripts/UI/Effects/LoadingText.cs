using System.Collections;
using TMPro;
using UnityEngine;

namespace Quadrablaze {
    public class LoadingText : MonoBehaviour {

        public TMP_Text text;

        IEnumerator animateRoutine;

        IEnumerator Animate() {
            int increment = 0;
            int textLength = text.text.Length - 3;

            text.CrossFadeAlpha(0, 0, true);
            yield return new WaitForEndOfFrame();
            text.CrossFadeAlpha(1, 2, true);
            yield return new WaitForSecondsRealtime(1);

            while(true) {
                text.maxVisibleCharacters = textLength + increment;

                yield return new WaitForSecondsRealtime(.5f);

                increment++;
                increment %= 4;
            }
        }

        IEnumerator DeactivateRoutine(float delay) {
            yield return new WaitForSeconds(delay);
            gameObject.SetActive(false);
        }

        public void StartAnimation() {
            if(animateRoutine != null) StopCoroutine(animateRoutine);

            gameObject.SetActive(true);
            animateRoutine = Animate();

            StartCoroutine(animateRoutine);
        }

        public void StopAnimation(bool destroy = true) {
            if(animateRoutine != null) StopCoroutine(animateRoutine);

            text.CrossFadeAlpha(0, .5f, true);

            if(gameObject.activeSelf)
                StartCoroutine(DeactivateRoutine(.5f));
            //gameObject.SetActive(false);

            //if(destroy)
            //    Destroy(gameObject, .6f);
        }
    }
}