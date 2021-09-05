using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RFB.Utilities
{
    public class RFBProgressBar : MonoBehaviour
    {
        // Min load speed
        public float minLoadTime = 0.5f;

        // Progress slider
        public Slider progressSlider;
        // Progress image
        public Image progressImage;

        // Value
        public float value { get; private set; }
        // Desired value
        private float _desValue = 0f;

        // Disable interaction
        protected virtual void Awake()
        {
            if (progressSlider != null)
            {
                progressSlider.interactable = false;
            }
            SetProgress(0f);
        }

        // Update
        protected virtual void Update()
        {
            // New progress
            float newProgress = value;
            // Progress
            newProgress += Time.deltaTime / minLoadTime;
            // Dont progress too quick
            newProgress = Mathf.Min(newProgress, _desValue);

            // Apply
            if (value != newProgress)
            {
                SetActualProgress(newProgress);
            }
        }

        // Set progress
        public void SetProgress(float newProgress)
        {
            // Apply
            _desValue = Mathf.Clamp01(newProgress);

            // Auto set if 0
            if (_desValue == 0f)
            {
                SetActualProgress(_desValue);
            }
        }
        // Set actual progress
        protected void SetActualProgress(float newProgress)
        {
            // Apply value
            value = newProgress;

            // Set slider
            if (progressSlider != null)
            {
                progressSlider.value = value;
            }

            // Set image
            if (progressImage != null)
            {
                progressImage.fillAmount = value;
            }
        }
    }
}
