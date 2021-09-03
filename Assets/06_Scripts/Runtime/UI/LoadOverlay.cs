using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RFB.Utilities;

namespace RFB.Portfolio
{
    public class LoadOverlay : MonoBehaviour
    {
        // Hide
        public float hideTime = 0.5f;
        public TweenEase hideEase = TweenEase.easeOutQuad;

        // Fader
        public CanvasGroup fader { get; private set; }

        // Awake
        private void Awake()
        {
            fader = gameObject.GetComponent<CanvasGroup>();
            if (fader == null)
            {
                fader = gameObject.AddComponent<CanvasGroup>();
                fader.alpha = 1f;
                fader.blocksRaycasts = true;
            }
            SetLoaded(false);
            AppManager.onLoadComplete += LoadComplete;
        }
        // Loaded
        private void LoadComplete()
        {
            SetLoaded(true);
        }
        // Destroy
        private void OnDestroy()
        {
            AppManager.onLoadComplete -= LoadComplete;
        }

        // Adjust based on loaded
        private void SetLoaded(bool toLoaded)
        {
            // Move to front
            if (!toLoaded)
            {
                gameObject.SetActive(true);
            }
            // Move to back
            else
            {
                Invoke("Hide", 0.5f);
            }
        }

        // Hide
        public void Hide()
        {
            TweenUtility.StartTween(gameObject, "LOAD_HIDE", 1f, 0f, hideTime, hideEase, delegate (GameObject go, string id, float a)
            {
                fader.alpha = a;
            }, delegate (GameObject go, string id, bool cancelled)
            {
                gameObject.SetActive(false);
            });
        }
    }
}
