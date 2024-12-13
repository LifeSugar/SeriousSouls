using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class ScreenFadeController : MonoBehaviour
    {
        public bool startFade = false;
        public CanvasGroup canvasGroup;
        public float fadeDuration = 1f;

        private void Awake()
        {
            if (!startFade)
            {
                if (canvasGroup == null)
                    canvasGroup = GetComponent<CanvasGroup>();

                // 初始化为全透明
                canvasGroup.alpha = 0f;
            }
            else
            {
                StartCoroutine(FadeOut());
            }
        }

        public IEnumerator FadeIn()
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        public IEnumerator FadeOut()
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
    }

}