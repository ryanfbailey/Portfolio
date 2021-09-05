using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RFB.Utilities;

namespace RFB.Portfolio
{
    public class LoadOverlay : MonoBehaviour
    {
        // Min time
        public float minLoadTime = 2f;

        // Hide
        public float hideTime = 0.5f;
        public TweenEase hideEase = TweenEase.easeOutQuad;

        // Progress bar
        public Slider loadProgrBar;
        // Progress
        public float loadProgress { get; private set; }
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
            loadProgress = 0.0001f;
            loadProgrBar.value = loadProgress;
            SetLoaded(false);
            AppManager.onLoadComplete += LoadComplete;
        }
        // Apply progress
        private void Update()
        {
            // Dont load
            if (isLoaded)
            {
                return;
            }

            // New progress
            float newProgress = loadProgress;
            // Progress
            newProgress += Time.deltaTime / minLoadTime;
            // Dont progress too quick
            newProgress = Mathf.Min(newProgress, AppManager.instance.loadProgress);
            
            // Set progress
            SetLoadProgress(newProgress);
        }
        // Destroy
        private void OnDestroy()
        {
            AppManager.onLoadComplete -= LoadComplete;
        }

        // Set load progress
        private void SetLoadProgress(float newProgress)
        {
            // Skip
            if (loadProgress == newProgress)
            {
                return;
            }

            // Set progress
            loadProgress = Mathf.Clamp01(newProgress);
            loadProgrBar.value = loadProgress;

            // Apply
            if (loadProgress >= 1f)
            {
                SetLoaded(true);
            }
        }
        // Loaded
        private void LoadComplete()
        {
            //SetLoaded(true);
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
