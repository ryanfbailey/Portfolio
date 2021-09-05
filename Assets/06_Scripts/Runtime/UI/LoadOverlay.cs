using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RFB.Utilities;

namespace RFB.Portfolio
{
    public class LoadOverlay : MonoBehaviour
    {
        // Hide
        public float hideTime = 0.5f;
        public TweenEase hideEase = TweenEase.easeOutQuad;

        // Progress bar
        public RFBProgressBar loadBar;
        // Load
        public bool isLoaded { get; private set; }

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
            loadBar.SetProgress(0f);
            SetLoaded(false);
            AppManager.onLoadProgressChange += SetLoadProgress;
        }
        // Destroy
        private void OnDestroy()
        {
            AppManager.onLoadProgressChange -= SetLoadProgress;
        }

        // Set load progress
        private void SetLoadProgress(float newProgress)
        {
            loadBar.SetProgress(newProgress);
        }
        // Set loaded
        private void Update()
        {
            if (!isLoaded && loadBar.value >= 1f)
            {
                SetLoaded(true);
            }
        }
        // Adjust based on loaded
        private void SetLoaded(bool toLoaded)
        {
            // Apply
            isLoaded = toLoaded;

            // Move to front
            if (!isLoaded)
            {
                gameObject.SetActive(true);
            }
            // Move to back
            else
            {
                Hide();
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
